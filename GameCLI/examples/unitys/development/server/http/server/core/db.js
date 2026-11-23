const mongoose = require('mongoose')
const defaultValue = require('mongoose-get-default-value')
const defLeanValue = require('mongoose-lean-defaults')
const path = require('path')
const util = require('util')
const fs = require('fs')
const word = require('server/word')
const time = require('server/time')
const strict = require('assert/strict')
const log = require('log4js').getLogger('db')

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

function uuid()
{
    var d = Date.now();
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
        var r = (d + Math.random() * 16) % 16 | 0
        d = Math.floor(d / 16)

        return (c === 'x' ? r : (r & 0x3) | 0x8).toString(16)
    })
}

const outputd = false
const lockuid = uuid()
const modules = {
}

const lstatus = new mongoose.Schema({
    _id: String,
}, {strict: false})

const gstatus = new mongoose.Schema({
    _id: String,
}, {strict: false})

const glocked = new mongoose.Schema({
    _id: String,
    acquirer: String,
    acquiredAt: Date,
    updatedAt: {type: Date, expires:3, default:Date.now}
})

const account = new mongoose.Schema({
    _id : String,
    acc : String,
    pid : Number,
    email: String,
    phone: String,
    identifier: String,
    status: Number,
    tmcreate : Number,
})

const glorder = new mongoose.Schema({
    _id : String,
    id: String,
    pid: Number,
    propId: Number,
    createAt: Number,
    payAt: Number,
    verifyAt: Number,
    shipAt: Number,
    state: Number,
    orderId: String,
    payload: String,
    channel: String
})

const archive = new mongoose.Schema({
    _id : Number,
    name : String,
    archive : {type:String, default:''},
}, {strict:false})

const schemas = {
    lstatus : {
        name : 'lstatus', scheam : lstatus
    },

    gstatus : {
        name : 'gstatus', scheam : gstatus
    },

    glocked : {
        name : 'glocked', scheam : glocked
    },

    account : {
        name : 'account', scheam : account
    },

    glorder : {
        name : 'glorder', scheam : glorder
    }
}

let conn = null

// connect to db
function connect(url, dir)
{
    mongoose.plugin(defaultValue)
    mongoose.plugin(defLeanValue)

    conn = mongoose.createConnection(url, {
        useNewUrlParser :true, useUnifiedTopology : true, useCreateIndex : true, useFindAndModify : false
    })

    conn.on('connected', () => {
        log.info(`MongoDB connected: ${url}`)
    })

    conn.on('disconnected', () => {
        log.error(`MongoDB disconnected! url: ${url}`)
    })

    conn.on('error', (err) => {
        log.error(`MongoDB connection error: ${url}`, err)
    })

    conn.then(()=>{
        for (var name in schemas){
            const s = schemas[name]
            if (s){
                modules[s.name] = conn.model(s.name, s.scheam, s.name)
            }
        }
    
        if (dir){
            scan(dir, (filepath)=>{
                const name = path.basename(filepath, '.js')
                
                const file = require(filepath)
                if (file){
                    modules[name] = conn.model(name, file, name)
                }
            })
        }

        log.info('MongoDB models registered:', Object.keys(modules))
    })
    
    return true
}

// get model by name if not exist, create it
function get(name)
{
    let model = modules[name]
    if (!model) {
        modules[name] = conn.model(name, archive, name)
    }

    return modules[name]
}

// create new model instance
function create(name, data, defaults)
{
    const model = get(name)
    const newMd = new model(data, {defaults: defaults})

    return newMd.toJSON()
}

// sleep for ms
function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

// find model instance by id
async function findById(name, id, projection){
    const model = get(name)
    if (outputd){
        log.debug("findById", name, id, projection)
    }
    
    return model.findById(id, 
        projection && projection.join(' ')).lean()
}

// find model instances by ids
async function findByIds(name, ids, projection){
    const model = get(name)
    if (outputd){
        log.debug("findByIds", name, ids, projection)
    }
    
    return model.find({_id:{$in:ids}}, 
        projection && projection.join(' ')).lean()
}

// find model instance by filter
async function findOne(name, filter, projection)
{
    const model = get(name)
    if (outputd){
        log.debug("findOne", name, filter, projection)
    }
    return model.findOne(filter, 
        projection && projection.join(' ')).lean()
}

// find model instances by filter
async function find(name, filter, projection)
{
    const model = get(name)
    if (outputd){
        log.debug("find", name, filter, projection)
    }

    return model.find(filter, 
        projection && projection.join(' ') ).lean()
}

// update model instance by id
async function updateById(name, id, data)
{
    const model = get(name)
    if (outputd){
        log.debug("updateById", name, id, data)
    }

    return model.updateOne({_id:id}, 
        data, {upsert:true})
}

// delete model instance by id
async function deleteById(name, id)
{
    const model = get(name)
    if (outputd){
        log.debug("deleteById", name, id)
    }

    return model.findByIdAndDelete(id).lean()
}

// insert model instance
async function insertOne(name, data)
{
    const model = get(name)
    if (outputd){
        log.debug("insertOne", name, data)
    }

    return model.create(data)
}

// delete model instances by filter
async function deleteMany(name, filter)
{
    const model = get(name)
    if (outputd){
        log.debug("deleteMany", name, filter)
    }

    return model.deleteMany(filter)
}

// insert model instances
async function insertMany(name, data, opts)
{
    const model = get(name)
    if (outputd){
        log.debug("insertMany", name, data, opts)
    }

    return model.insertMany(data, opts)
}

// lookup model instances by id
async function lookup(name, id, pipeline, projection)
{
    const head = []

    head.push({$match:{_id:id}})
    if (projection) {
        head.push({$project:projection})
    }
    const aggregate = head.concat(pipeline.map(x => {return {$lookup:x}}))

    const model = get(name)
    const result = await model.aggregate(aggregate)

    const data = result[0] || {}
    const res = {}
    for (let v of pipeline) {
        res[v.from] = data[v.as] || []
        delete data[v.as]
    }

    res.primary = data
    
    return res
}

// load model instance fields by id
async function loadFields(name, loaded, id, ids)
{
    if (!ids) {
        const datas = await findById(name, id)
        if (!datas) {
            return {}
        }
        delete datas._id
        delete datas.__v
        return datas
    }

    const fields = []
    for (const _id of ids) {
        if (!loaded[_id]) {
            fields.push(_id)
        }
    }

    if (fields.length == 0) {
        return {}
    }

    return await findById(name, id, fields) || {}
}

// load model instance fields array by id
async function lookupArray(name, id, field, conds)
{
    const finConds = {}
    for (const exp in conds) {
        const [key, val] = conds[exp]
        const finKey = util.format('$$%s.%s', field, key)
        finConds[exp] = [finKey, val]
    }

    const pipe = [
        {$match: {_id:id}},
        {$project: {
            [field]: {
                $filter: {
                    input: `$${field}`,
                    as: field,
                    cond: finConds,
                }
            },
        }}
    ]

    const result = await get(name).aggregate(pipe)
    return result[0][field]
}

// acquire lock by id
async function acquire(id) {
    let ret = true

    const dt = new Date()
    await insertOne(schemas.glocked.name, {
        _id: id,
        acquirer: lockuid,
        acquiredAt: dt,
        updatedAt: dt,
    }).catch((e) => {ret = false})

    return ret
}

// try to acquire lock by id
async function trylock(name, retryInterval = 500) {
    while (true) {
        if (await acquire(name)) {
            break
        } else {
            await sleep(retryInterval)
        }
    }
}

// unlock lock by id
async function unlock(name){
    await deleteMany(schemas.glocked.name, {
        _id : name, acquirer : lockuid
    })
}

// renew lock by id
async function renew(name) {
    await dao.updateOne(schemas.glocked.name, { _id: name, acquirer: lockuid },
        {
            updatedAt: new Date()
        }
    )
}

// lock model instance by id
async function lock(name, fn)
{
    await trylock(name)

    try{
        const result = await fn()
        await unlock(name)

        return result
    }catch(exception){
        await unlock(name)
        throw exception
    }
}

/// generate unique id by name
class idGen {
    static BaseGUID = 1000000
}
async function guid(name)
{
    const info = await get(schemas.lstatus.name).findByIdAndUpdate({_id:word.guid}, 
        {$inc: {[name + '.sn'] : 1}}, {upsert: false, new: true}).lean()
    if (info) {
        const t = info[name]
        if (t.at && t.sn < idGen.BaseGUID) {
            return t.at * idGen.BaseGUID + t.sn
        } else {
            const t = {
                _id: name, 
                at: await section(name),
                sn: 1
            }

            await updateById(schemas.lstatus.name, 
                word.guid, 
                {[name]:t})

            return t.at * idGen.BaseGUID + t.sn
        }
    } else {
        return await lock(word.guid, async() => {
            const t = {
                _id: name, 
                at: await section(name),
                sn: 1
            }
            
            await updateById(schemas.lstatus.name, 
                word.guid, 
                {[name]:t}, {upsert:true})
            
            return t.at * idGen.BaseGUID + t.sn
        })
    }
}

// generate unique section id by name
async function section(name)
{
    const info = await get(schemas.gstatus.name).findByIdAndUpdate({_id:word.guid}, 
        {$inc : {[name + '.sn'] : 1}}, {upsert:true, new: true}).lean()

    return info[name].sn
}

/// register new account by username
async function register(username, email, phone, identifier)
{
    const pid = await guid(word.pid)
    if (pid <= 0){
        throw new Error(pid.toString())
    }

    const account = await create(schemas.account.name, {
        _id : username, acc : username, pid:pid, status:0, email: email || '', phone: phone || '', identifier: identifier || username, tmcreate : time.now()
    })

    await updateById(schemas.account.name, 
        account._id, account)

    return account
}

/// query account by username
async function query(username)
{
    return await findById(schemas.account.name, username)
}

/// unregister account by username
async function unregister(username)
{
    return await deleteById(schemas.account.name, username)
}

/// connect to database
module.exports = {
    connect : connect,
    create : create,
    findById : findById,
    findByIds : findByIds,
    findOne : findOne,
    find : find,
    updateById : updateById,
    deleteById : deleteById,
    insertOne : insertOne,
    deleteMany : deleteMany,
    insertMany : insertMany,
    lookup : lookup,
    loadFields : loadFields,
    lookupArray : lookupArray,
    lock : lock,
    register : register,
    query : query,
    unregister : unregister,
    guid : guid,
}