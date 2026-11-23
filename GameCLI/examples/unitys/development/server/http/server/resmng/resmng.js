
const fs = require('fs')
const vm = require('vm')
const path = require('path')
const child_process = require('child_process')
const log = require('log4js').getLogger('resmng')

const resmng = {
    package : {
        seeall : undefined,
    },
    setmetatable(t, mt)
    {
        Object.setPrototypeOf(t, mt.__index)
        return t
    },
    module() {},
    svnnum() {},
    
    getConstValue(propId) {
        return this.propConst[propId].Value
    },

    queryPatch(version){
        const versionInfo = []
        const versionDump = []
        for(var idx in this.VersionPatch){
            const patch = this.VersionPatch[idx]
            if (patch.version <= version){
                versionDump.push({version:patch.version, length:patch.text.length})
                versionInfo.push({
                    version:patch.version, text:patch.text, length:patch.text.length
                })
            }
        }
        
        if (versionDump.length > 0){
            console.table(versionDump)
        }
            
        return versionInfo
    },

    SUCCEED : 0,
    FAILURE : 1,
    OFFLINE : 2,
    EXCEPTION : 3,
}

resmng.resmng = resmng
vm.createContext(resmng)

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

function read(filepath)
{
    const str = fs.readFileSync(filepath, 'utf-8')
    if (str.length > 0){
        console.info("ReadFileSync:", filepath)
    }

    const script = new vm.Script(str, {filename: filepath})
    script.runInContext(resmng)
}

function getDirectories (srcpath) {
    return fs.readdirSync(srcpath)
      .filter(file => fs.lstatSync(path.join(srcpath, file)).isDirectory())
}

// Patch storage directory
const dirnamepatch = 'patch'

// Read all the patches from the client and sort them by version number
function readPatch(){
    const patch = []
    scan(path.join(__dirname, dirnamepatch), (filepath)=>{
        const filename = path.basename(filepath, '.lua')
        if(filename){
            const text = fs.readFileSync(filepath, 'utf-8')
            if (text.length > 0){
                patch.push({
                    version:filename, text:text
                })
            }
        }
    })

    patch.sort((a,b)=>{
        return a.version < b.version
    })

    resmng.VersionPatch = patch
}

function load(){
    const filters = [dirnamepatch]

    // all server configuration files
    const dirs = getDirectories(__dirname)
    for (var dir of dirs){
        //the server needs to filter the patch directory when loading the configuration file
        var need = true
        for (var i in filters){
            if (filters[i] == dir){
                need = false
                break
            }
        }
        
        if (need){
            scan(path.join(__dirname, dir), (filepath)=>{
                read(filepath)
            })
        }
    }

    precompile()

    return resmng
}

async function reload(){
    const cmdStr = 'svn up ' + __dirname
    log.warn(cmdStr)

    await new Promise((resolve, reject) => {
        child_process.exec(cmdStr, (err, stdout, stderr) => {
            log.warn('exec', stdout)
            resolve()
        })
    })
}

const compiler = {

}

function precompile()
{
    for (const k in compiler) {
        compiler[k]()
    }

    readPatch()
}

module.exports = {
    load : load, reload : reload
}