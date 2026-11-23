const resmng = require('server/resmng')
const db = require('server/db')
const word = require('server/word')
const order = require('server/order')
const time = require('server/time')
const playermgr = require('server/playermgr')
const XPlayer = require('server/player')
const log = require('log4js').getLogger('player')

module.exports = {
    // client req login to sterver
    async ReqLogin(server, session, query)
    {
        const username = query.username
        if (!username){
            return [resmng.FAILURE]
        }

        let account = await db.query(username)
        if (!account){
            account = await db.register(username)
        }
        
        if (account){
            const pid = account.pid
            if (pid != 0) {
                session.pid = pid
            }
            
            return await db.lock(pid.toString(), async ()=>{
                let player = await playermgr.get(pid)
                
                // update player info
                if (session.clientIp) {
                    await player.refresh(session.clientIp, query)
                }
                
                // push player info to client
                player.tmcreate = account.tmcreate || 0
                return [resmng.SUCCEED, player.getSyncData()]
            })
        }

        return [resmng.FAILURE]
    },

    /// req save player data
    async ReqSave(server, pid, query){
        const player = await playermgr.get(pid)
        if (!player)
            return [resmng.FAILURE]

        player.activeAt = time.now()
        await player.update()
        
        return await player.save(query.name, query.archive)
    },

    /// req load player data
    async ReqLoad(server, pid, query){
        const player = await playermgr.get(pid)
        if (!player){
            return [resmng.FAILURE]
        }

        player.activeAt = time.now()
        await player.update()
 
        return await player.read(query.name)
    },

    // client load player all measured data
    async ReqLoadPlayer(server, pid, query){
        const player = await playermgr.get(pid)
        if (!player)
            return [resmng.FAILURE]

        player.activeAt = time.now()
        await player.update()

        // push player info to client
        const data = player.getSyncData()
        return [resmng.SUCCEED, data]
    },

    // req save player data
    async ReqSavePlayer(server, pid, query)
    {
        const player = await playermgr.get(pid)
        if (!player)
            return [resmng.FAILURE]
        
        player.activeAt = time.now()
        return await player.update()
    },

    // req curernt server time
    async ReqServerTime(server, pid, query)
    {
        const player = await playermgr.get(pid)
        if (!player){
            return [resmng.FAILURE]
        }

        player.activeAt = time.now()
        await player.update()

        return [resmng.SUCCEED, {time:time.now()}]
    },

    /// req create order
    async ReqCreateOrder(server, pid, query)
    {
        const player = await playermgr.get(pid)
        if (!player){
            return [resmng.FAILURE]
        }

        return [resmng.SUCCEED, {order:order.create(player)}]
    },

    /// req pay order
    async ReqPayOrder(server, pid, query)
    {
        const player = await playermgr.get(pid)
        if (!player){
            return [resmng.FAILURE]
        }

        return [resmng.SUCCEED, {order:order.pay(player, query.orderId)}]
    },
}
