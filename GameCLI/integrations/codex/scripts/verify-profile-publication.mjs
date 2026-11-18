import assert from 'node:assert/strict';
import {readFile,writeFile} from 'node:fs/promises';
import {createHash} from 'node:crypto';
import {openDatabase} from '../server/database.mjs';
const db=await openDatabase(),hash=b=>createHash('sha256').update(b).digest('hex'),cached=new Map(),result=[];
try{
 for(const id of ['PlayerProfile','ProfileShortcuts']){
  const row=(await db.pool.query('SELECT version,scripts FROM gamecli_ui.templates WHERE id=$1',[id])).rows[0];assert(row,id);let checked=0;
  for(const f of row.scripts.files){
   if(f.path.startsWith('Assets/')){assert.equal(hash(await readFile('../../examples/unitys/development/client/'+f.path)),f.sha256,f.path);checked++;}
   if(f.path.endsWith('/materials.json'))for(const m of JSON.parse(f.content).files){
    if(!cached.has(m.path))cached.set(m.path,hash(await readFile('../../examples/unitys/development/client/'+m.path)));
    assert.equal(cached.get(m.path),m.sha256,m.path);
    if(m.assetId){if(!cached.has(m.assetId)){const a=(await db.pool.query('SELECT body FROM gamecli_ui.assets WHERE id=$1',[m.assetId])).rows[0];assert(a);cached.set(m.assetId,hash(a.body));}assert.equal(cached.get(m.assetId),m.sha256);}
    else assert.equal(hash(Buffer.from(m.content)),m.sha256);
    checked++;
   }
  }
  result.push({id,version:row.version,checked});
 }
 await writeFile('.data/profile/database-readback.json',JSON.stringify(result,null,2));console.log(JSON.stringify(result));
}finally{await db.close();}
