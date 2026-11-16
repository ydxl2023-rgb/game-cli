import assert from 'node:assert/strict';
import {mkdir,writeFile,readFile} from 'node:fs/promises';
import {resolve,join} from 'node:path';
import {createWorkspaces} from '../server/workspaces.mjs';
import {selectPrefab} from '../shared/prefab-verification.mjs';
import {inspectRenderOrder,assertRenderOrder} from '../shared/render-order.mjs';

const projectId='p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484';
const directory=resolve(process.argv[2]??'.data/notice-layer-repair');
const apply=process.argv.includes('--apply');
const workspaces=createWorkspaces();
function repair(design){
 const root=design.nodes.find(n=>n.parent===null&&n.templateSourceId==='SvNotice');assert(root,'Missing Notice root');
 const host=design.nodes.find(n=>n.parent===root.id&&n.name==='Animator');assert(host);
 const bg=design.nodes.find(n=>n.parent===host.id&&n.name==='Bg');
 const panel=design.nodes.find(n=>n.parent===host.id&&n.name==='Panel');assert(bg&&panel);
 bg.role='background';bg.mustRenderBehind=['Panel'];
 const before=inspectRenderOrder(selectPrefab(design,root.id));
 design.nodes.splice(design.nodes.indexOf(bg),1);design.nodes.splice(design.nodes.indexOf(panel),0,bg);
 assertRenderOrder(selectPrefab(design,root.id));return {rootId:root.id,backdropId:bg.id,before};
}
try{
 await mkdir(directory,{recursive:true});
 const binding=await workspaces.unityBindings.requireBinding(projectId);
 const identity=JSON.parse(await readFile(join(binding.path,'ProjectSettings/GameCLIProject.json'),'utf8'));
 assert.equal(identity.projectId,projectId);assert.equal(identity.unityProjectId,binding.unityProjectId);
 const snapshot=await workspaces.snapshot(projectId),design=structuredClone(snapshot.design);
 const {rootId,backdropId,before}=repair(design);
 if(!apply){
  await writeFile(join(directory,'project-before.json'),JSON.stringify(snapshot,null,2),{flag:'wx'});
  await writeFile(join(directory,'desired.json'),JSON.stringify({root:rootId,design:selectPrefab(design,rootId)},null,2),{flag:'wx'});
  await writeFile(join(directory,'bad.json'),JSON.stringify({root:rootId,design:selectPrefab({...snapshot.design,nodes:snapshot.design.nodes.map(n=>n.id===backdropId?{...n,role:'backdrop',mustRenderBehind:['Panel']}:n)},rootId)},null,2),{flag:'wx'});
  console.log(JSON.stringify({projectVersion:snapshot.revision,before,directory}));
 }else{
  const frozen=JSON.parse(await readFile(join(directory,'project-before.json'),'utf8'));
  assert.equal(snapshot.revision,frozen.revision,'Project changed; prepare again before applying');
  const saved=await workspaces.save(projectId,{projectId,projectVersion:snapshot.revision,revision:snapshot.revision,design});
  const {id}=await workspaces.templateEditor.open({templateId:'SvNotice'});
  const draft=await workspaces.templateEditor.read(id);
  await writeFile(join(directory,'draft-before.json'),JSON.stringify(draft,null,2),{flag:'wx'});
  repair(draft.design);
  const ds=await workspaces.templateEditor.save(id,{draftId:id,revision:draft.revision,design:draft.design});
  const candidate=await workspaces.templateEditor.submit(id,{draftId:id,revision:ds.revision});
  const result={projectVersion:saved.revision,draftId:id,candidateId:candidate.id,published:false};
  await writeFile(join(directory,'database-result.json'),JSON.stringify(result,null,2));console.log(JSON.stringify(result));
 }
}finally{await workspaces.close();}
