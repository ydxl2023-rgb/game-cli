import assert from 'node:assert/strict';
import {readFile,writeFile} from 'node:fs/promises';
import {openDatabase} from '../server/database.mjs';
import {createWorkspaces} from '../server/workspaces.mjs';
const db=await openDatabase(),w=createWorkspaces();
try{
 const publication=JSON.parse(await readFile('reports/micros-directory/publication.json','utf8'));
 const migration=JSON.parse(await readFile('reports/micros-directory/migration.json','utf8'));
 const stale=s=>Object.keys(migration.directories).some(p=>s.includes(p));
 const rows={},residuals=[];
 for(const item of publication.templates){
  const actual=(await db.pool.query('SELECT * FROM gamecli_ui.templates WHERE id=$1',[item.id])).rows[0];rows[item.id]=actual;
  const candidate=JSON.parse(await readFile('.data/micros-directory/candidates/'+item.id+'.json','utf8'));
  assert.equal(actual.version,item.version);assert.deepEqual(actual.scripts,candidate.scripts);assert.deepEqual(actual.payload.configurations,candidate.payload.configurations);
  for(const file of actual.scripts.files){
   if(/(?:migration|resource-moves)\.json$/.test(file.path))continue;
   if(stale(file.path)||stale(file.content))residuals.push({id:item.id,path:file.path});
  }
  const payload=structuredClone(actual.payload);
  delete payload.scripts;for(const n of payload.design?.nodes??[])delete n.scripts;
  // Historical imported source descriptions are not restored native assets.
  if(stale(JSON.stringify(payload)))residuals.push({id:item.id,path:'payload outside scripts'});
 }
 const snapshot=await w.snapshot('p-2d314388-18c4-42f5-80eb-f6729dd23f0e');let instances=0;
 for(const node of snapshot.design.nodes.filter(n=>n.parent===null&&rows[n.templateSourceId])){
  const row=rows[node.templateSourceId];assert.equal(node.templateSourceVersion,row.version);assert.deepEqual(node.scripts,row.scripts);assert.deepEqual(node.configurations,row.payload.configurations);instances++;
 }
 const evidence={verified:residuals.length===0,templates:publication.templates.length,instances,projectRevision:snapshot.revision,residuals};
 await writeFile('reports/micros-directory/readback.json',JSON.stringify(evidence,null,2));console.log(JSON.stringify(evidence));
}finally{await w.close();await db.close();}
