import assert from 'node:assert/strict';
import {readFile,writeFile} from 'node:fs/promises';
import {createWorkspaces} from '../server/workspaces.mjs';
import {openDatabase} from '../server/database.mjs';
import {validateScripts} from '../server/template-scripts.mjs';
const w=createWorkspaces(),db=await openDatabase(),projectId='p-2d314388-18c4-42f5-80eb-f6729dd23f0e';
try{
 const {path:project}=await w.unityBindings.requireBinding(projectId);
 const results=[],before={},scriptsById={};
 for(const[id,version]of [['RewardEntry',2],['SvMail',6]]){
  const row=(await db.pool.query('SELECT * FROM gamecli_ui.templates WHERE id=$1',[id])).rows[0];assert.equal(row.version,version);before[id]=row;
  const payload=structuredClone(row.payload),files=row.scripts.files.map(f=>({path:f.path,content:f.content}));
  if(id==='RewardEntry'){
   const file=files.find(f=>f.path.endsWith('/UIRewardEntry.cs'));file.content=await readFile(project+'/'+file.path,'utf8');
   payload.configurations.integration.push('Unref preserves the item data: temporarily clear Item to release the child subscription, unsubscribe the parent, then restore the same Item. Recycled cells may bind the same reward model again.');
   files.find(f=>f.path.endsWith('/README.md')).content=payload.configurations.integration.join('\n\n');
  }else{
   payload.configurations.dependencies=payload.configurations.dependencies.map(d=>d.templateId==='RewardEntry'?{...d,version:3}:d);
   payload.configurations.integration=payload.configurations.integration.map(s=>s.replaceAll('RewardEntry v2','RewardEntry v3'));
   const materials=files.find(f=>f.path.endsWith('/materials.json')),m=JSON.parse(materials.content);m.dependencies=[{templateId:'RewardEntry',version:3}];materials.content=JSON.stringify(m);
   const readme=files.find(f=>f.path.endsWith('/README.md'));readme.content=readme.content.replaceAll('RewardEntry v2','RewardEntry v3');
  }
  const scripts=validateScripts({schemaVersion:1,files});scriptsById[id]=scripts;
  const published=await w.templateEditor.published(id);assert.equal(published.revision,version);
  const root=payload.design.nodes.find(n=>n.parent===null);root.templateSourceId=id;root.scripts=scripts;root.configurations=payload.configurations;
  const opened=await w.templateEditor.open({templateId:id}),draft=await w.templateEditor.read(opened.id);assert.deepEqual(draft.design,published.design);
  const saved=await w.templateEditor.save(opened.id,{draftId:opened.id,revision:draft.revision,design:payload.design});
  const candidate=await w.templateEditor.submit(opened.id,{draftId:opened.id,revision:saved.revision});const result=await w.publish({candidateId:candidate.id,confirm:true});
  const actual=(await db.pool.query('SELECT scripts FROM gamecli_ui.templates WHERE id=$1',[id])).rows[0];assert.deepEqual(actual.scripts,scripts);
  results.push({id,version:result.version,configurations:payload.configurations});
 }
 const snapshot=await w.snapshot(projectId);await writeFile('.data/item-slot/project-before-lifecycle.json',JSON.stringify(snapshot));
 for(const result of results)for(const root of snapshot.design.nodes.filter(n=>n.parent===null&&n.templateSourceId===result.id)){
  assert.deepEqual(root.scripts,before[result.id].scripts);root.scripts=scriptsById[result.id];root.configurations=result.configurations;root.templateSourceVersion=result.version;
 }
 const saved=await w.save(projectId,{projectId,revision:snapshot.revision,projectVersion:snapshot.project.version,design:snapshot.design});
 const report={verified:true,templates:[{id:'ItemSlot',version:2},...results.map(({id,version})=>({id,version}))],projectRevision:saved.revision};
 await writeFile('reports/item-slot/publication-initial.json',await readFile('reports/item-slot/publication.json'));
 await writeFile('reports/item-slot/publication.json',JSON.stringify(report,null,2));console.log(JSON.stringify(report));
}finally{await w.close();await db.close();}
