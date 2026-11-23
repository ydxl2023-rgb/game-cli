const word = require('server/word')
const time = require('server/time')
const uuid = require('uuid')

const XOrderState = {
    Unpay:0,
    Payed:1,
    Shiped:2,
    VerifyFailed:3
}

class XOrder{
    schema = {}
    
    constructor(data){
        if (data){
            os.merge(this.schema, data)
        }
    }

    getSchema(){
        return this.schema
    }

    async save(){
        const schema = this.getSchema()
        await db.updateById(word.glorder, 
            schema.id, schema)

        return true
    }
}

/*
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
*/
async function create(pid, propId, payload, channel){
    const id = uuid.v1()
    const order = new XOrder({
        id:id, pid:pid, propId:propId, channel:channel, payload:payload, createAt:time.now(), state:XOrderState.Unpay
    })

    await order.save()
    return order
}

/// get order by id
async function getOrder(id){
    const data = await db.findById(word.glorder, id)
    return new XOrder(data)
}

/// pay order
async function pay(player, orderId){
    const order = await getOrder(orderId)
    if (!order){
        return false
    }
    
    // check order state if unpay
    if (order.state != XOrderState.Unpay){
        return false
    }
    
    order.state = XOrderState.Payed
    order.payAt = time.now()

    return await order.save()
}

module.exports = {
    create : create,
    getOrder : getOrder,
    pay : pay
}