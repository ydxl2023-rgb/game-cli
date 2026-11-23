const path = require('path')

module.exports = {
    // master server port
    port : 41000,
    // https key and cert
    cert : {
        key : path.join(__dirname, './cert/cert.key'),
        crt : path.join(__dirname, './cert/cert.crt'),
    },

    // server list
    list : [
        {id: 41001, bind: "https://110.40.178.238:41001"},
        {id: 41002, bind: "https://110.40.178.238:41002"},
    ],

    // server main
    main : '../app/index.js',
    log4 : 'log4.js',
    route : 'route',
    schema : 'schema',

    // dump server info, Interval time
    dump : 60000,
    // if true then open auto restart server
    restart : true, 

    session : {
        secure : true,
        TTL : 15 * 60,
        touchAfter : 60,
        maxAge : 15 * 60 * 1000,
        resave : false,
        secret : 'oathx.ltd',
    },

    mongoUrl:'mongodb://127.0.0.1:27017/dayf',
}
