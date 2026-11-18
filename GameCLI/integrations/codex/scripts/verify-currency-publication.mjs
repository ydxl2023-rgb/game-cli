import assert from 'node:assert/strict';
import {readFile,writeFile} from 'node:fs/promises';
import {createHash} from 'node:crypto';
import {openDatabase} from '../server/database.mjs';
const db=await openDatabase(),hash=b=>createHash('sha256').update(b).digest('hex');
try{
 const result=[];
 for(const id of ['CurrencyBar','SvHome']){
  const row=(await db.pool.query('SELECT version,scripts,payload FROM gamecli_ui.templates WHERE id=$1',[id])).rows[0];
  let checked=0;
  for(const f of row.scripts.files){
   if(f.path.startsWith('Assets/')){const bytes=await readFile('../../examples/unitys/development/client/'+f.path);assert.equal(hash(bytes),f.sha256,f.path);checked++;}
   if(f.path.endsWith('/materials.json'))for(const m of JSON.parse(f.content).files){
    const bytes=await readFile('../../examples/unitys/development/client/'+m.path);assert.equal(hash(bytes),m.sha256,m.path);
    if(m.assetId){const a=(await db.pool.query('SELECT body FROM gamecli_ui.assets WHERE id=$1',[m.assetId])).rows[0];assert(a,m.path);assert.equal(hash(a.body),m.sha256);}
    else assert.equal(hash(Buffer.from(m.content)),m.sha256);
    checked++;
   }
  }
  result.push({id,version:row.version,checked});
 }
 await writeFile('.data/currency-template/database-readback.json',JSON.stringify(result,null,2));console.log(JSON.stringify(result));
}finally{await db.close();}
