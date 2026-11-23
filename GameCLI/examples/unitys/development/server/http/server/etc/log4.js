
module.exports = {
    appenders : {
        console : {
            type : "console",
            layout : {
                type : "pattern",
                pattern: "%["+"%.1p %d{yyyy-MM-dd hh:mm:ss} ${serverId}: %c, %m"+"%]"
            },
        },
        seperateFile: {
            type: 'file',
            filename: './log/${serverId}/local.log',
            layout: {
                type: 'pattern',
                pattern: "%.1p %d{yyyy-MM-dd hh:mm:ss} ${serverId}: %c, %m",
            },
            encoding: 'utf8',
            pattern: "yyyy-MM-dd",
            backups: 14,
            maxLogSize: 1024 * 1024 * 1024,
            alwaysIncludePattern: false,
        },
        dateFile: {
            type: 'dateFile',
            filename: './log/local.log',
            layout: {
                type: 'pattern',
                pattern: "%.1p %d{yyyy-MM-dd hh:mm:ss} ${serverId}: %c, %m",
            },
            encoding: 'utf8',
            pattern: "yyyy-MM-dd",
            daysToKeep: 14,
            alwaysIncludePattern: true,
        },
        errorFile: {
            type: 'dateFile',
            filename: './log/error.log',
            layout: {
                type: 'pattern',
                pattern: "%.1p %d{yyyy-MM-dd hh:mm:ss} ${serverId}: %c, %m",
            },
            encoding: 'utf8',
            pattern: "yyyy-MM",
            daysToKeep: 3,
            alwaysIncludePattern: true,
        },
        logFilter: {
            type: 'logLevelFilter',
            level: 'all',
            appender: 'dateFile',
        },
        errorFilter: {
            type: 'logLevelFilter',
            level: 'error',
            appender: 'errorFile',
        }
    },
    categories: {
        default: {
            appenders: ['console', 'logFilter', 'errorFilter'],
            level: 'all',
        },
    },
}
