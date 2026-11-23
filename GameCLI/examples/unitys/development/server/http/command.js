
const commander = require('commander')
const path = require('path')
const log = require('log4js')
const core = require('server/core')
const os = require('server/os')
const resmng = require('server/resmng')

// command start
/*
* use example
* command start --env=<environment>
*/
commander.command("start")
    .description("Start server application")
    .option('-e, --env <environment>', 'environment', 'dev')
    .option('-d, --daemon', 'enable daemon start')
    .action((opts) => {
        opts.daemon = !!opts.daemon

        // start server
        start(opts)
    })

// command stop
/*
* use example
* command start --env=<environment>
*/
commander.command("stop")
    .description('stop the application')
    .option('-e, --env <environment>', 'environment')
    .action((opts) => {
        // stop server
        stop(opts)
    })

// etc dir
const etcdir = path.join(__dirname, '/server/etc')
const routes = {
    async ReqHost(master, session, query){
        const server = master.alloc()
        if (server){
            log.getLogger("master").info("\u001b[0mdevice \u001b[31m%s \u001b[0malloc to server id \u001b[32m%s \u001b[0maddr \u001b[33m%s \u001b[0mclient version \u001b[35m%s", 
                query.username, server.id, server.bind, query.version)

            const versionPatch = resmng.queryPatch(query.version)
            return [resmng.SUCCEED, {
                id:server.id, host:server.bind, patch:versionPatch
            }]
        }

        return [resmng.FAILURE]
    }
}

// start all server
/*
* @opts 
*/
function start(opts)
{
    const etcenv = path.join(etcdir, opts.env)
    if (opts.daemon)
    {

    }
    else
    {
        const config = require(etcenv)
        if (config){
            config.path = etcenv
            
            const log4 = os.loadConfigFile(path.join(etcdir, config.log4), {
                serverId:config.port
            })
            if (log4){
                log.configure(log4)
            }

            const server = core.create(config, routes)
            if (server){
                server.startup()
                server.run()
            }
        }
    }
}

// stop all server
/*
* @opts 
*/
function stop(opts)
{

}

commander.parse(process.argv)
