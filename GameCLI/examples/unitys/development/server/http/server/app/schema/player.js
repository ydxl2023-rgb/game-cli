const mongoose = require('mongoose')

module.exports = new mongoose.Schema({
    _id : Number,
    // player id
    pid : {type:Number, default:0},
    // player device
    device : {type:String, default:''},
    loginTime: {type:String, default:''},
    // last login time
    loginAt : {type:Number, default:0},
    // active time
    activeAt : {type:Number, default:0},
    // memory
    memoryTotal : {type:Number, default:0},
    //graphics devices
    graphicsDevice : {type:String, default:''},
    // graphics memory size
    graphicsMemorySize : {type:Number, default:0},
    // ip address
    ip : {type:String, default:''},
})
