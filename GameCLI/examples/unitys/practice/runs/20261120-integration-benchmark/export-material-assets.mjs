import {openDatabase} from '../../../../../integrations/codex/server/database.mjs';
import {readdir,readFile,writeFile,mkdir} from 'node:fs/promises';
import {fileURLToPath} from 'node:url';
import path from 'node:path';
import {createHash} from 'node:crypto';
const out=path.dirname(fileURLToPath(import.meta.url)), assets=new Map();
async function scan(dir){for(const f of await readdir(dir,{withFileTypes:true})){const p=path.join(dir,f.name);if(f.isDirectory())await scan(p);else if(f.name==='materials.json'){const j=JSON.parse(await readFile(p,'utf8'));for(const a of j.files??[])if(a.assetId)assets.set(a.assetId,{sha256:a.sha256,path:a.path});}}}
await scan(path.join(out,'sources'));await scan(path.join(out,'standard-sources'));
await mkdir(path.join(out,'assets'),{recursive:true});
const db=await openDatabase();const results=[];
try{for(const [id,meta] of assets){const a=(await db.pool.query('SELECT mime,body FROM gamecli_ui.assets WHERE id=$1',[id])).rows[0];if(!a)throw Error('Missing '+id);const sha=createHash('sha256').update(a.body).digest('hex');if(sha!==id||meta.sha256&&sha!==meta.sha256)throw Error('Hash mismatch '+id);const extension=path.extname(meta.path)||'.bin';await writeFile(path.join(out,'assets',id+extension),a.body);results.push({id,sha256:sha,extension,mime:a.mime});}await writeFile(path.join(out,'material-assets.json'),JSON.stringify(results,null,2));console.log(JSON.stringify({assets:results.length}));}finally{await db.close();}
