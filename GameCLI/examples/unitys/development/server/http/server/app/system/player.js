const db = require('server/db')
const os = require('server/os')
const time = require('server/time')
const resmng = require('server/resmng')
const word = require('server/word')

class XPlayer {
    // player id
    pid = 0
    // player name
    device = ''
    // login time
    loginTime = ''
    // last login time
    loginAt = 0
    // active time
    activeAt = 0
    // memory
    memoryTotal = 0
    // graphics devices
    graphicsDevice = ''
    //  graphics memory size
    graphicsMemorySize = 0
    // ip address
    ip = ''
    // player create time
    tmcreate = 0

    /// create player
    constructor(pid)
    {
        this.pid = pid
    }

    /// get player data schema
    getSchema() 
    {
        return {
            pid: this.pid, 
            device: this.device,
            loginTime : this.loginTime,
            loginAt: this.loginAt, 
            activeAt: this.activeAt, 
            memoryTotal : this.memoryTotal,
            graphicsDevice : this.graphicsDevice,
            graphicsMemorySize : this.graphicsMemorySize,
            ip : this.ip,
            tmcreate : this.tmcreate,
        }
    }

    /// get sync data
    getSyncData() 
    {
        return {
            pid : this.pid, loginAt:this.loginAt, activeAt:time.now(), tmcreate:this.tmcreate,
        }
    }

    /// load player data from db
    async loadPlayerData() {
       const data = await db.findById(word.player, this.pid)
       if (!data) {
           return this.savePlayerData()
       }
       else 
       {
            os.merge(this, data)
       }
       
       return [resmng.SUCCEED, data]
    }

    /// update client ip
    async refresh(clientIp, info)
    {
        this.ip = clientIp || ''
        this.graphicsDevice = info.graphicsDevice || ''
        this.graphicsMemorySize = info.graphicsMemorySize || 0
        this.memoryTotal = info.memoryTotal || 0
        this.loginAt = time.now() 
        this.loginTime = time.toString()
        this.device = info.device || ''
        
        return await this.update()
    }

    /// save player data to db
    async savePlayerData()
    {
        return await this.update()
    }

    /// save player data
    async save(name, archive)
    {
        if (!name)
            return [resmng.FAILURE]

        await db.updateById(name, this.pid, {
                name:name, archive:archive
            })

        return [resmng.SUCCEED, {
            pid:this.pid, name:name
        }]
    }

    /// read player data
    async read(name)
    {
        const data = await db.findById(name, this.pid)
        if (!data) {
            return [resmng.FAILURE]
        }

        return [resmng.SUCCEED, {
            pid:this.pid, name:name, archive:data.archive, 
        }]
    }

    // save player data
    async update(){
        const data = this.getSchema()
        await db.updateById(word.player,
            this.pid,  data)
        
        return [resmng.SUCCEED, data]
    }

    // get player id
    getPID(){
        return this.pid
    }
}

module.exports = XPlayer
