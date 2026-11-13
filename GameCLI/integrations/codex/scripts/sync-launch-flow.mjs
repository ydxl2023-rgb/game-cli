import assert from 'node:assert/strict';
import {mkdir,writeFile} from 'node:fs/promises';
import {fileURLToPath} from 'node:url';
import {createWorkspaces} from '../server/workspaces.mjs';
import {connectUi,startupPortId,relations} from '../shared/relations.mjs';

// Publish the reusable event capability; keep the destination in GameCLIApp only.
const apply=process.argv.includes('--apply');
assert(process.argv.slice(2).every(a=>a==='--apply'),'Only --apply is supported.');
const w=createWorkspaces(),projectId='p-2d314388-18c4-42f5-80eb-f6729dd23f0e';
try{
 const published=await w.templateEditor.published('SvStart'),snapshot=await w.snapshot(projectId);
 assert.equal(snapshot.project.name,'GameCLIApp');
 const design=structuredClone(published.design),root=design.nodes.find(n=>n.parent===null);
 const launch=snapshot.design.nodes.find(n=>n.parent===null&&n.templateSourceId==='SvStart');
 const login=snapshot.design.nodes.find(n=>n.parent===null&&n.templateSourceId==='SvLogin');
 assert(launch&&login,'GameCLIApp requires Launch and Login.');
 assert(!root.startupComplete?.target,'Standard template must not contain a product destination.');
 assert(!launch.startupComplete?.target||launch.startupComplete.target===login.id,'Preserve a different configured destination for review.');
 const summary={apply,project:'GameCLIApp',baseVersion:published.workspace.baseVersion,source:launch.id,target:login.id,event:'startupComplete'};
 if(!apply){console.log(JSON.stringify(summary));}
 else{
  const backup=new URL(`../.data/template-history/launch-flow-${Date.now()}/`,import.meta.url);
  await mkdir(backup,{recursive:true});
  await writeFile(new URL('before-template.json',backup),JSON.stringify(published));
  await writeFile(new URL('before-project.json',backup),JSON.stringify(snapshot));
  let version=published.workspace.baseVersion;
  if(root.startupComplete===undefined){
   const {id}=await w.templateEditor.open({templateId:'SvStart'}),draft=await w.templateEditor.read(id);
   assert.deepEqual(draft.design,published.design,'Do not replace an edited template draft.');
   root.startupComplete={type:'none'};
   const saved=await w.templateEditor.save(id,{draftId:id,revision:draft.revision,design});
   const candidate=await w.templateEditor.submit(id,{draftId:id,revision:saved.revision});
   version=(await w.publish({candidateId:candidate.id,confirm:true})).version;
  }
  launch.startupComplete??={type:'none'};
  connectUi(snapshot.design,startupPortId(launch.id),login.id,'show');
  launch.templateSourceVersion=version;
  const saved=await w.save(projectId,{projectId,revision:snapshot.revision,projectVersion:snapshot.project.version,design:snapshot.design});
  const actual=await w.snapshot(projectId),standard=await w.templateEditor.published('SvStart');
  assert.deepEqual(actual.design,snapshot.design);
  assert.deepEqual(standard.design.nodes.find(n=>n.parent===null).startupComplete,{type:'none'});
  const report={...summary,version,projectRevision:saved.revision,relations:relations(actual.design),backup:fileURLToPath(backup),verified:true};
  await writeFile(new URL('report.json',backup),JSON.stringify(report,null,2));console.log(JSON.stringify(report,null,2));
 }
}finally{await w.close();}
