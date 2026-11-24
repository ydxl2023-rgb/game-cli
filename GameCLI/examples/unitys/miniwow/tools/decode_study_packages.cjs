const fs=require('fs'),p=require('path'),z=require('zlib'),c=require('crypto');
const root=p.resolve(__dirname,'../research/code-study/packages');
const records=[];
for(const f of fs.readdirSync(root+'/members',{recursive:true}).filter(x=>x.endsWith('.br'))){
 const b=fs.readFileSync(p.join(root,'members',f)),d=z.brotliDecompressSync(b);
 const out=p.join(root,'decoded',f.replace(/\.br$/,''));
 fs.mkdirSync(p.dirname(out),{recursive:true});
 if(fs.existsSync(out)&&!fs.readFileSync(out).equals(d))throw Error('Refusing overwrite');
 fs.writeFileSync(out,d);
 records.push({source:f,path:p.relative(root,out),bytes:d.length,sha256:c.createHash('sha256').update(d).digest('hex'),wasm_valid:out.endsWith('.wasm')?WebAssembly.validate(d):null});
}
fs.writeFileSync(root+'/decoded-manifest.json',JSON.stringify(records,null,2));
console.log(JSON.stringify(records));
