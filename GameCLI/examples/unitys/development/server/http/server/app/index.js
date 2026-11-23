const path = require('path')
const fs = require('fs')
const log4js = require('log4js')
const core = require('server/core')
const os = require('server/os')

/*
* parse process param
*/
const [_, file, environment, id, bind] = process.argv

const env = require(environment)
if (env){
    const conf = os.loadConfigFile(path.join(path.dirname(environment), env.log4), {
        serverId: id
    })
    if (conf){
        log4js.configure(conf)
    }    
}

function scan(dir, fn) {
    if (!fs.existsSync(dir)) {
        return false
    }

    const dirents = fs.readdirSync(dir, {withFileTypes: true})
    for (const dirent of dirents) {
        if (!dirent.isDirectory()) {
            const subdir = path.resolve(dir, dirent.name)
            fn(subdir)
        }
    }

    return true
}

function load(directory){
    const route = {}
    scan(directory, (filepath) => {
        const script = require(filepath)
        for(const key in script){
            const fun = script[key]
            if (typeof fun == 'function'){
                route[key] = fun
            }
        }
    })

    console.table(route)
    return route
}

const routes = load(path.join(__dirname, env.route))
if (routes){
    const server = core.launch({
        port: id, bind: bind, cert: env.cert, session: env.session, file: file, mongoUrl: env.mongoUrl, schema:path.join(__dirname, env.schema)
    }, routes)
    
    server.startup()
    server.run() 
}

