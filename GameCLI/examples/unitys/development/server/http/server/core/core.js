
const express = require('express')
const session = require('express-session')
const https = require('https')
const bodyParser = require('body-parser')
const path = require('path')
const fs = require('fs')
const childProcess = require('child_process')
const mongoStore = require('connect-mongo')
const resmng = require('server/resmng')
const db = require('server/db')
const playermgr = require('server/playermgr')
const time = require('server/time')
const log = require('log4js').getLogger('core')

const LogFlag = {
    Recv : "recv", Send : 'send', Push : 'push'
}

class XServer {
    constructor(config, routes)
    {
        this.config = config
        this.routes = routes
        this.express = express()
    }

    getConfig()
    {
        return this.config
    }

    startup()
    {
        const exp = this.express
        exp.use(bodyParser.json())
        exp.use(bodyParser.urlencoded({extended: false}))

        const config = this.config
        if (config.session){
            try{
                db.connect(config.mongoUrl, config.schema)
            } catch(exception){
                log.error(exception)
            }
            
            const mongoStoreSchema = mongoStore.create({ 
                mongoUrl: config.mongoUrl, ttl: config.session.TTL, touchAfter: config.session.touchAfter
            })

            exp.use(session({
                secure: config.session.secure,
                secret: config.session.secret,

                // Force the session to be saved even if it hasn't changed
                resave: false,
                // Cookies are forcibly set on each request, which resets the cookie expiration time
                rolling: true,

                // mongo store
                store: mongoStoreSchema,

                cookie: {
                    secure: config.session.secure, 
                    maxAge: config.session.maxAge
                },
                
                // Force an uninitialized session to be stored
                saveUninitialized: true,
            }))
        }

        const options = {
            key : fs.readFileSync(config.cert.key), cert : fs.readFileSync(config.cert.crt)
        }
        this.app = https.createServer(options, exp)
    }

    run()
    {
        time.update(Date.now())
        time.setAccurateInterval(timer => {
            const msec = timer.expect
            time.update(msec)
        }, 200)

        const exp = this.express
        exp.post('/:name', 
            (req, res)=> this.handle(req, res))

        const config = this.config
        this.app.listen(config.port, ()=>{
            log.trace("listen", config.port)
        })
    }

    rsp(res, code, data, msg)
    {
        res.send(JSON.stringify({code:code, data:data, msg:msg}))
    }

    getClientIp(req)
    {
        let clientIp = req.socket.remoteAddress

        const forwardedFor = req.headers['x-forwarded-for']
        if (forwardedFor) {
            clientIp = forwardedFor.split(',')[0].trim()
        }

        if (clientIp === '::1' || clientIp === '::ffff:127.0.0.1') {
            clientIp = '127.0.0.1';
        }

        return clientIp
    }

    async dispatch(session, name, query)
    {
        const pid = session.pid || 0
        log.trace(pid, LogFlag.Recv, name, query)
      
        const asyncfn = this.routes[name]
        if (asyncfn){
            if (pid != 0){                
                const[code, data] = await db.lock(pid.toString(), async ()=>{
                    return await asyncfn(this, pid, query)
                })

                log.trace(pid, LogFlag.Send, name, code, data || {})
                return [code, data]
            } 
            else 
            {
                if (query.username && query.version){
                    return await asyncfn(this, session, query)
                } else {
                    return [resmng.OFFLINE]
                }
            } 
        } else {
            log.error(pid, name, query)
        }

        return [resmng.FAILURE]
    }

    async handle(req, res)
    {
        const name = req.params.name
        const body = req.body
        
        try{
            const profilerStart = new Date()
            const beginTime = profilerStart.getTime()

            let ip = this.getClientIp(req)
            if (ip) {
                req.session.clientIp = ip
            }

            const [code, data, messages] = await this.dispatch(req.session, name, body)
            if (code != resmng.SUCCEED){
                log.warn(name, code, data)
            }

            const profilerEnd = new Date()

            const pid = req.session.pid || 0
            console.info(pid, ip, name, profilerEnd.toString(), profilerEnd.getTime() - beginTime, req.session.lastModified || 0)

            const json = JSON.stringify({code:code, data:data, msgs:messages})
            console.info(pid, ip, name, json)

            res.send(json)
        }catch(exception){
            res.send(JSON.stringify({
                code:resmng.EXCEPTION, data:exception
            }))

            log.error(exception)
        }

        return 0
    }
}

class XMaster extends XServer
{
    constructor(config, routes) {
        super(config, routes)

        this.works = []
        this.index = 0
    }

    close(sprocess, autoRestart){
        for(var i=0; i<this.works.length; i++){
            if (this.works[i].sprocess.pid == sprocess.pid){
                const server = {
                    id : this.works[i].id, bind : this.works[i].bind
                }
                this.works.splice(i, 1)

                // try restart the server
                if (autoRestart){
                    try {
                        this.launch(server)
                    } 
                    catch (error) 
                    {
                        log.error(error)                    
                    }
                }
      
                break
            }
        }
    }

    launch(server) {
        const cfg = this.getConfig()
        if (cfg) {
            const cp = childProcess.spawn(process.execPath,[
                path.join(__dirname, cfg.main), cfg.path, server.id, server.bind
            ])
            this.works.push({
                id : server.id, bind : server.bind, sprocess : cp
            })

            cp.on('exit', (code, signal)=>{
                log.error("server exit code [%s] signal %s id %s bind %s", 
                    code, signal, server.id, server.bind)

                this.close(cp, cfg.restart)
            })

            cp.stdout.on('data', (data)=>{ process.stdout.write(data) })
            cp.stderr.on('data', (data)=>{ process.stderr.write(data) })
        
            cp.on('error', (code)=>{
                log.error(server.id, server.bind, code)
            })
        }
    }

    alloc() {
        const count = this.works.length
        if (count != 0){
            return this.works[this.index ++ % count]
        }
    }

    run() {
        const cfg = this.getConfig()
        if (cfg) {
            for (const server of cfg.list){
                this.launch(server)
            }

            const time = cfg.dump || 0
            if (time > 0){
                this.dump(time)
            }
        }

        super.run()   
    }

    dump(time){
        setInterval(()=>{
            const servers = []
            for (var work of this.works){
                servers.push({
                    id:work.id, addr:work.bind, pid: work.sprocess.pid
                })
            }

            console.table(servers)
        }, time)
    }
}

module.exports = {
    launch : function(config, routes){
        return new XServer(config, routes)
    },

    create : function(config, routes){
        return new XMaster(config, routes)
    },
}