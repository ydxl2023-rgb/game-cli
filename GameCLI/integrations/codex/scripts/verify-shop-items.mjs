import assert from 'node:assert/strict';
import {readFile,writeFile} from 'node:fs/promises';
import {createHash} from 'node:crypto';
import {openDatabase} from '../server/database.mjs';
import {createWorkspaces} from '../server/workspaces.mjs';
const db=await openDatabase(),w=createWorkspaces(),hash=b=>createHash('sha256').update(b).digest('hex');
try{
 const {path:project}=await w.unityBindings.requireBinding('p-2d314388-18c4-42f5-80eb-f6729dd23f0e');
 const candidate=JSON.parse(await readFile('.data/shop-item-slot/candidate.json','utf8'));
 const slot=(await db.pool.query("SELECT scripts FROM gamecli_ui.templates WHERE id='ItemSlot'")).rows[0];
 const owned=new Set();let code=0,materials=0;
 for(const scripts of [candidate.scripts,slot.scripts])for(const f of scripts.files){
  if(f.path.startsWith('Assets/')){owned.add(f.path);if(f.path.includes('/Shop/')){assert.equal(hash(await readFile(project+'/'+f.path)),f.sha256,f.path);code++;}}
  if(f.path.endsWith('/materials.json'))for(const m of JSON.parse(f.content).files){
   if(!m.path.startsWith('Assets/'))continue;owned.add(m.path);
   const body=await readFile(project+'/'+m.path);assert.equal(hash(body),m.sha256,m.path);
   if(m.assetId){const a=(await db.pool.query('SELECT body FROM gamecli_ui.assets WHERE id=$1',[m.assetId])).rows[0];assert(a,m.path);assert.equal(hash(a.body),m.sha256);}
   materials++;
  }
 }
 const native=JSON.parse(await readFile('reports/shop-item-slot/native.json','utf8'));assert(native.ok);
 const cards=JSON.parse(native.data.returnSummary);
 const missing=[...new Set(cards.flatMap(c=>c.dependencies).filter(p=>p.startsWith('Assets/')&&!owned.has(p)))];assert.deepEqual(missing,[],'Unpackaged card dependencies');
 const evidence={verified:true,code,materials,cards:cards.map(c=>c.card),missingDependencies:missing};
 if(process.argv.includes('--published')){
  const current=(await db.pool.query("SELECT version,scripts,payload FROM gamecli_ui.templates WHERE id='SvShop'")).rows[0];assert.equal(current.version,5);assert.deepEqual(current.scripts,candidate.scripts);assert.deepEqual(current.payload.configurations,candidate.payload.configurations);
  const snapshot=await w.snapshot('p-2d314388-18c4-42f5-80eb-f6729dd23f0e');const instance=snapshot.design.nodes.find(n=>n.parent===null&&n.templateSourceId==='SvShop');assert(instance);assert.equal(instance.templateSourceVersion,5);assert.deepEqual(instance.scripts,current.scripts);assert.deepEqual(instance.configurations,current.payload.configurations);
  Object.assign(evidence,{templateVersion:5,projectRevision:snapshot.revision});
 }
 await writeFile('reports/shop-item-slot/readback.json',JSON.stringify(evidence,null,2));console.log(JSON.stringify(evidence));
}finally{await w.close();await db.close();}
