import assert from 'node:assert/strict';
import {readFile,writeFile} from 'node:fs/promises';
import {join} from 'node:path';
import {createHash} from 'node:crypto';
import {openDatabase} from '../server/database.mjs';
import {createWorkspaces} from '../server/workspaces.mjs';
const db=await openDatabase(),w=createWorkspaces(),hash=b=>createHash('sha256').update(b).digest('hex');
try{
 const projectId='p-2d314388-18c4-42f5-80eb-f6729dd23f0e';
 const {path:project}=await w.unityBindings.requireBinding(projectId),results=[];
 for(const id of ['RewardEntry','SvMail']){
  const row=(await db.pool.query('SELECT category_id,version,scripts FROM gamecli_ui.templates WHERE id=$1',[id])).rows[0];let code=0,materials=0;
  for(const file of row.scripts.files){
   if(file.path.startsWith('Assets/')){assert.equal(hash(await readFile(join(project,file.path))),file.sha256,file.path);code++;}
   if(!file.path.endsWith('/materials.json'))continue;
   for(const material of JSON.parse(file.content).files){
    if(!material.path.startsWith('Assets/'))continue;
    const local=await readFile(join(project,material.path));assert.equal(hash(local),material.sha256,material.path);
    if(material.assetId){const a=(await db.pool.query('SELECT body FROM gamecli_ui.assets WHERE id=$1',[material.assetId])).rows[0];assert(a);assert.equal(hash(a.body),hash(local));}
    else assert.equal(material.content,local.toString('utf8'));
    materials++;
   }
  }
  results.push({id,category:row.category_id,version:row.version,code,materials});
 }
 const snapshot=await w.snapshot(projectId),instance=snapshot.design.nodes.find(n=>n.parent===null&&n.templateSourceId==='SvMail');
 assert.equal(instance.templateSourceVersion,results.find(r=>r.id==='SvMail').version);
 assert.deepEqual(instance.scripts,(await db.pool.query("SELECT scripts FROM gamecli_ui.templates WHERE id='SvMail'")).rows[0].scripts);
 const report={verified:true,templates:results,projectRevision:snapshot.revision};
 await writeFile('reports/reward-entry/readback.json',JSON.stringify(report,null,2));console.log(JSON.stringify(report));
}finally{await db.close();await w.close();}
