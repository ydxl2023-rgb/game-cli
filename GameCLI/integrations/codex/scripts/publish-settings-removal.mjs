// Publish the requested removals and patch only their corresponding project nodes.
import assert from 'node:assert/strict';
import {readFile,writeFile} from 'node:fs/promises';
import {createWorkspaces} from '../server/workspaces.mjs';
import {openDatabase} from '../server/database.mjs';
const baseVersion=Number(process.argv[2]||1),version=baseVersion+1;
assert(Number.isInteger(baseVersion)&&baseVersion>0);
const w=createWorkspaces(),db=await openDatabase();
const read=async path=>JSON.parse(await readFile(path,'utf8'));
try{
 const folder='.data/imports/sv-settings';
 const payload=await read(folder+'/publication.json'),scripts=await read(folder+'/scripts.json');
 const published=await w.templateEditor.published('SvSettings');assert.equal(published.revision,baseVersion);
 const root=payload.design.nodes.find(n=>n.parent===null);root.templateSourceId='SvSettings';root.scripts=scripts;root.configurations=payload.configurations;
 const opened=await w.templateEditor.open({templateId:'SvSettings'}),draft=await w.templateEditor.read(opened.id);
 assert.deepEqual(draft.design,published.design,'Preserve unrelated draft edits');
 const saved=await w.templateEditor.save(opened.id,{draftId:opened.id,revision:draft.revision,design:payload.design});
 const candidate=await w.templateEditor.submit(opened.id,{draftId:opened.id,revision:saved.revision});
 const result=await w.publish({candidateId:candidate.id,confirm:true});
 const row=(await db.pool.query("SELECT version,payload,scripts FROM gamecli_ui.templates WHERE id='SvSettings'")).rows[0];
 assert.equal(row.version,result.version);assert.deepEqual(row.scripts,scripts);
 const projectId='p-2d314388-18c4-42f5-80eb-f6729dd23f0e';
 const snapshot=await w.snapshot(projectId);
 await writeFile(folder+'/project-before-v'+version+'.json',JSON.stringify(snapshot));
 const reference=snapshot.design.nodes.find(n=>n.parent===null&&n.templateSourceId==='SvSettings');assert(reference);assert.equal(reference.templateSourceVersion,baseVersion);
 const prefix=reference.id.slice(0,-payload.root.length);
 const removal=await read('reports/settings-template/removal-v'+version+'.json');
 const ids=new Set(removal.removedNodeIds.map(id=>prefix+id));
 assert.equal(snapshot.design.nodes.filter(n=>ids.has(n.id)).length,removal.removedNodeIds.length,'Reference hierarchy changed; review before removal');
 snapshot.design.nodes=snapshot.design.nodes.filter(n=>!ids.has(n.id));
 // Compact the remaining notification row only where the reference layout was untouched.
 for(const id of ['Settings0_5_0_0_0_0_4','Settings0_5_0_0_0_0_4_4','Settings0_5_0_0_0_0_4_5']){
  const current=snapshot.design.nodes.find(n=>n.id===prefix+id),before=published.design.nodes.find(n=>n.id===id),after=payload.design.nodes.find(n=>n.id===id);
  if(current&&before&&after)for(const key of ['x','y','height','rectTransform']){
   if(JSON.stringify(current[key])===JSON.stringify(before[key]))current[key]=structuredClone(after[key]);
  }
 }
 reference.scripts=row.scripts;reference.configurations=row.payload.configurations;reference.templateSourceVersion=row.version;
 const updated=await w.save(projectId,{projectId,revision:snapshot.revision,projectVersion:snapshot.project.version,design:snapshot.design});
 const actual=await w.snapshot(projectId);assert(!actual.design.nodes.some(n=>ids.has(n.id)));
 assert.deepEqual(actual.design.nodes.find(n=>n.id===reference.id).scripts,scripts);
 const report={template:'SvSettings',version:row.version,projectRevision:updated.revision,removedNodes:ids.size,root:reference.id};
 await writeFile('reports/settings-template/v'+version+'-publication.json',JSON.stringify(report,null,2));console.log(JSON.stringify(report));
}finally{await w.close();await db.close();}
