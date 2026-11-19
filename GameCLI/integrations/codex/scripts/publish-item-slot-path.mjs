import assert from 'node:assert/strict';
import {readFile,writeFile,mkdir} from 'node:fs/promises';
import {createWorkspaces} from '../server/workspaces.mjs';
import {openDatabase} from '../server/database.mjs';
import {validateScripts} from '../server/template-scripts.mjs';
const w=createWorkspaces(),db=await openDatabase(),projectId='p-2d314388-18c4-42f5-80eb-f6729dd23f0e';
const note='Icon is a string asset path resolved by existing SyncSprite/UILoader. IconVisible expresses host visibility intent, not loading completion. Mail does not load Sprite or maintain image callbacks. Shared loader cancellation and out-of-order completion need separate validation.';
const update=s=>s.replaceAll('ItemSlot v2','ItemSlot v3').replaceAll('RewardEntry v3','RewardEntry v4').replaceAll('Mail owns claim rules and asynchronous icon requests.','Mail owns claim rules; SyncSprite/UILoader owns icon loading.').replaceAll('Host must guard Item against stale callbacks. ','').replaceAll('Icon accepts either a valid string path or a loaded Sprite through existing SyncSprite. Never assign null to clear it; explicitly assign IconVisible=false while loading. Host guards asynchronous requests.',note);
try {
 await mkdir('.data/item-slot-path/final',{recursive:true});
 const {path:project}=await w.unityBindings.requireBinding(projectId),results=[],before={},after={};
 for(const [id,version] of [['ItemSlot',2],['RewardEntry',3],['SvMail',7]]){
  const row=(await db.pool.query('SELECT * FROM gamecli_ui.templates WHERE id=$1',[id])).rows[0];assert.equal(row.version,version);before[id]=row;
  await writeFile(`.data/item-slot-path/${id}-before.json`,JSON.stringify(row));
  const payload=structuredClone(row.payload),files=row.scripts.files.map(f=>({path:f.path,content:f.content}));
  for(const f of files){
   if(f.path.endsWith('/VMItemSlot.cs')||f.path.endsWith('/MailSystem.cs')) f.content=await readFile(project+'/'+f.path,'utf8');
   if(f.path.endsWith('/README.md')) f.content=update(f.content)+'\n\n'+note;
   if(f.path.endsWith('/materials.json')){
    const m=JSON.parse(f.content);
    if(m.dependencies) m.dependencies=m.dependencies.map(d=>d.templateId==='ItemSlot'?{...d,version:3}:d.templateId==='RewardEntry'?{...d,version:4}:d);
    f.content=JSON.stringify(m);
   }
  }
  payload.configurations.integration=payload.configurations.integration.map(update);
  if(!payload.configurations.integration.includes(note))payload.configurations.integration.push(note);
  payload.configurations.dependencies=payload.configurations.dependencies.map(d=>d.templateId==='ItemSlot'?{...d,version:3}:d.templateId==='RewardEntry'?{...d,version:4}:d);
  const scripts=validateScripts({schemaVersion:1,files});after[id]={scripts,configurations:payload.configurations};
  const published=await w.templateEditor.published(id);assert.equal(published.revision,version);
  const root=payload.design.nodes.find(n=>n.parent===null);root.templateSourceId=id;root.scripts=scripts;root.configurations=payload.configurations;
  const opened=await w.templateEditor.open({templateId:id}),draft=await w.templateEditor.read(opened.id);assert.deepEqual(draft.design,published.design);
  const saved=await w.templateEditor.save(opened.id,{draftId:opened.id,revision:draft.revision,design:payload.design});
  const candidate=await w.templateEditor.submit(opened.id,{draftId:opened.id,revision:saved.revision});const result=await w.publish({candidateId:candidate.id,confirm:true});
  const actual=(await db.pool.query('SELECT * FROM gamecli_ui.templates WHERE id=$1',[id])).rows[0];assert.deepEqual(actual.scripts,scripts);
  await writeFile(`.data/item-slot-path/final/${id}.json`,JSON.stringify(actual));results.push({id,version:result.version});
 }
 const snapshot=await w.snapshot(projectId);await writeFile('.data/item-slot-path/project-before.json',JSON.stringify(snapshot));
 for(const result of results)for(const root of snapshot.design.nodes.filter(n=>n.parent===null&&n.templateSourceId===result.id)){
  assert.deepEqual(root.scripts,before[result.id].scripts);Object.assign(root,after[result.id]);root.templateSourceVersion=result.version;
 }
 const saved=await w.save(projectId,{projectId,revision:snapshot.revision,projectVersion:snapshot.project.version,design:snapshot.design});
 const report={verified:true,templates:results,projectRevision:saved.revision};await writeFile('reports/item-slot-path/publication.json',JSON.stringify(report,null,2));console.log(JSON.stringify(report));
}finally{await w.close();await db.close();}
