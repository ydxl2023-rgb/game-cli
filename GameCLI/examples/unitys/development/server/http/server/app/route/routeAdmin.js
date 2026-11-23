const resmng = require('server/resmng')
const admin = require('../admin')
const log = require('log4js').getLogger('cmd')

module.exports = {
    async ExecCommand(server, pid, query){
        log.warn(pid, query)

        const result = await admin.run(pid, query.cmd)
        return [resmng.SUCCEED, result]
    }
}