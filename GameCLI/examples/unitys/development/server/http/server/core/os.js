const fs = require('fs')
const vm = require('vm')
const uuid = require('uuid')

var __make_base_cid__ = 100000
module.exports = {
    uid(){
        return uuid.v1()
    },

    cid(){
        return __make_base_cid__ ++
    },
    
    merge(a, b)
    {
        for (const k in b) {
            const v = b[k]
            if (v == null || (typeof(v) == 'number' && isNaN(v))) {
                
            } else {
                a[k] = b[k]
            }
        }
        
        return a
    },

    size(m){
        var cnt = 0
        for (const k in m){
            cnt ++
        }

        return cnt
    },

    find(t, ...args)
    {
        let node = t
        for (const v of args) {
            if (node[v] == undefined) {
                return undefined
            } else {
                node = node[v]
            }
        }
        return node
    },

    formatWithProperties(fmt, args)
    {
        let res = fmt
        for (const matched of fmt.match(/\${[\S_.]+}/g) || []) {
            const ks = matched.substring(2, matched.length - 1).split('.')
            const val = this.find(args, ...ks)
            res = res.replace(matched, val)
        }
        return res
    },

    loadConfigFile(filepath, opts)
    {
        const str = fs.readFileSync(filepath, 'utf8')
        const replaced = this.formatWithProperties(str, opts)
        const script = new vm.Script(replaced, {filename: filepath})
        const context = vm.createContext({
            module : {
                exports : {}
            }
        })
        script.runInContext(context)
        return context.module.exports
    },

    random(min, max)
    {
        min = Math.floor(min)
        max = Math.ceil(max)
        return Math.floor(Math.random() * (max - min + 1) + min)
    },

    round(number, samples)
    {
        let ext = []
        let sum = 0
        for(var idx in samples){
            const [id, weight] = samples[idx]
            sum += weight
        }

        for (var i=0; i<number; i++){
            let n = random(0, sum)
            for (var idx in samples)
            {
                const [id, weight] = samples[idx]
                if (n <= weight)
                {
                    ext.push(id)
                    break
                }
    
                n = n - weight
            }
        }

        return ext
    }
}