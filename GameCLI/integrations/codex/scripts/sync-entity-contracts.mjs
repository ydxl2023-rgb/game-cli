// Update only reviewed entity-contract files, preserving template art and product navigation.
import assert from 'node:assert/strict';
import {readFile,writeFile,mkdir} from 'node:fs/promises';
import {join} from 'node:path';
import {isDeepStrictEqual} from 'node:util';
import {createWorkspaces} from '../server/workspaces.mjs';
import {validateScripts} from '../server/template-scripts.mjs';

assert(process.argv.slice(2).every(arg=>arg==='--apply'));
const apply=process.argv.includes('--apply');
const projectId='p-2d314388-18c4-42f5-80eb-f6729dd23f0e';
const plans={
 SvHome:['Home/HomeEntity.cs','Home/HomeVisibility.cs'],
 SvShop:['Shop/ShopEntity.cs','Shop/ShopSystem.cs'],
 SvLogin:['Login/LoginEntity.cs','Login/LoginSystem.cs'],
 MasterSystem:['Master/MasterEntity.cs'],
 RedDotSystem:['RedDot/RedDotEntity.cs','RedDot/RedDotSystem.cs','RedDot/RedDotScope.cs']
};
const removed=new Set(['Assets/Scripts/Systems/RedDot/RedDotConditions.cs','Assets/Scripts/Systems/RedDot/RedDotConditions.cs.meta']);
const beforeText='SignalCount reads RedDotEntity.signals.';
const afterText='RedDotEntity.UpdateSignal validates inputs; Refresh evaluates conditions and parent aggregation; GetCount exposes calculated state.';
const w=createWorkspaces();
try{
 const binding=await w.unityBindings.requireBinding(projectId);
 const identity=JSON.parse(await readFile(join(binding.path,'ProjectSettings/GameCLIProject.json'),'utf8'));
 assert.equal(identity.projectId,projectId);
 assert.equal(identity.unityProjectId,binding.unityProjectId);
 await mkdir('.data/entity-contracts',{recursive:true});
 for(const [templateId,paths] of Object.entries(plans)){
  const published=await w.templateEditor.published(templateId);
  const design=structuredClone(published.design),root=design.nodes.find(node=>node.parent===null);
  const replacements=new Map();
  for(const path of paths){const key='Assets/Scripts/Systems/'+path;replacements.set(key,await readFile(join(binding.path,key),'utf8'));}
  for(const path of replacements.keys())assert(root.scripts.files.some(file=>file.path===path),`Missing source entry: ${path}`);
  const files=root.scripts.files.filter(file=>!removed.has(file.path)).map(file=>({
   path:file.path,content:replacements.get(file.path)??file.content.replaceAll(beforeText,afterText)
  }));
  root.scripts=validateScripts({schemaVersion:1,files});
  if(root.configurations)root.configurations=JSON.parse(JSON.stringify(root.configurations).replaceAll(beforeText,afterText));
  const project=await w.snapshot(projectId);
  const copies=project.design.nodes.filter(node=>node.parent===null&&node.templateSourceId===templateId);
  const baseline=published.design.nodes.find(node=>node.parent===null);
  for(const copy of copies){
   assert.equal(copy.templateSourceVersion,published.revision,`${templateId}: project version conflict`);
   assert.deepEqual(copy.scripts,baseline.scripts,`${templateId}: preserve product source edits`);
  }
  const changed=!isDeepStrictEqual(design,published.design);
  await writeFile(`.data/entity-contracts/${templateId}-scripts.json`,JSON.stringify(root.scripts,null,2));
  if(!apply||!changed){console.log(JSON.stringify({templateId,changed,dryRun:!apply,version:published.revision}));continue;}
  await writeFile(`.data/entity-contracts/${templateId}-before.json`,JSON.stringify({published,project}));
  const opened=await w.templateEditor.open({templateId}),draft=await w.templateEditor.read(opened.id);
  assert.deepEqual(draft.design,published.design,`${templateId}: preserve edited draft`);
  const saved=await w.templateEditor.save(opened.id,{draftId:opened.id,revision:draft.revision,design});
  const candidate=await w.templateEditor.submit(opened.id,{draftId:opened.id,revision:saved.revision});
  const result=await w.publish({candidateId:candidate.id,confirm:true});
  assert.equal(result.version,published.revision+1);
  for(const copy of copies){copy.scripts=root.scripts;copy.templateSourceVersion=result.version;}
  if(copies.length)await w.save(projectId,{projectId,revision:project.revision,projectVersion:project.project.version,design:project.design});
  const actual=await w.templateEditor.published(templateId);
  assert.deepEqual(actual.design.nodes.find(node=>node.parent===null).scripts,root.scripts);
  const current=await w.snapshot(projectId);
  for(const copy of copies){const verified=current.design.nodes.find(node=>node.id===copy.id);assert.deepEqual(verified.scripts,root.scripts);assert.equal(verified.templateSourceVersion,result.version);}
  console.log(JSON.stringify({templateId,version:result.version,projectCopies:copies.length,projectRevision:current.revision}));
 }
}finally{await w.close();}
