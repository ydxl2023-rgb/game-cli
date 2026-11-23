const XPlayer = require('server/player')
const time = require('./time')
const TTLCache = require('@isaacs/ttlcache')
const log = require('log4js').getLogger('playermgr')

async function onTTLDispose(player, pid, reason)
{
    log.info(`player ${pid} dispose reason: ${reason}`)
    if (player && typeof player.update === 'function') {
        try {
            await player.update()
        } catch(e) {
            log.error(`player ${pid} auto-save on dispose failed:`, e)
        }
    }
}

//https://github.com/isaacs/ttlcache
const players = new TTLCache({ ttl: 1000 * 60 * 15 , dispose: onTTLDispose, updateAgeOnGet: true })

// create player
function create(classType, pid){
    const player = new classType(pid)
    players.set(pid, player)

    return player
}

// get player if not exist, create it
async function get(pid)
{
    let player = query(pid)
    if (!player){
        player = create(XPlayer, pid)
        await player.loadPlayerData()
    }

    return player
}

// query player
function query(pid){
    return players.get(pid)
}

// destory player
function destory(pid)
{
    players.delete(pid)
}

module.exports = {
    create : create,
    get : get,
    destory : destory
}
