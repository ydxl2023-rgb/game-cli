const resmng = require('server/resmng')
const playermgr = require('server/playermgr')
const word = require('server/word')
const util = require('util')
const log = require('log4js').getLogger('cmd')
const config = require('../../resmng/resmng')

async function cmdResetPlayer(pid){
    const player = await playermgr.get(pid)
    await player.reset()
    
    playermgr.destory(pid)
    return [resmng.SUCCEED]
}

async function cmdReloadConfig(pid) {
    await config.reload()

    const player = await playermgr.get(pid)  
    player.clear()

    return [resmng.SUCCEED]
}

module.exports = {
    async run(pid, str)
    {
        const player = await playermgr.get(pid)
        if (!player){
            return [resmng.FAILURE]
        }

        const cmd = util.format(`(async () => {%s})()`, str)
        return await eval(cmd)
    },
}