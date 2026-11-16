// Publish the regenerated Home source/material package through a guarded candidate.
import assert from 'node:assert/strict';
import {readFile} from 'node:fs/promises';
import {createWorkspaces} from '../server/workspaces.mjs';
import {remapAnimations} from '../shared/animations.mjs';
const w=createWorkspaces(),projectId='p-2d314388-18c4-42f5-80eb-f6729dd23f0e';
try{
  const scripts=JSON.parse(await readFile('.data/imports/sv-home/scripts.json','utf8'));
  const published=await w.templateEditor.published('SvHome');
  const opened=await w.templateEditor.open({templateId:'SvHome'});
  const draft=await w.templateEditor.read(opened.id);
  assert.deepEqual(draft.design,published.design,'Preserve existing draft edits');
  if(process.argv.includes('--animation'))published.design.animations=JSON.parse(await readFile('config/home-entrance.json','utf8'));
  published.design.nodes.find(n=>n.parent===null).scripts=scripts;
  const saved=await w.templateEditor.save(opened.id,{draftId:opened.id,revision:draft.revision,design:published.design});
  const candidate=await w.templateEditor.submit(opened.id,{draftId:opened.id,revision:saved.revision});
  const result=await w.publish({candidateId:candidate.id,confirm:true});
  const project=await w.snapshot(projectId);
  const root=project.design.nodes.find(n=>n.parent===null&&n.templateSourceId==='SvHome');
  assert(root,'Home instance missing');root.scripts=scripts;root.templateSourceVersion=result.version;
  if(process.argv.includes('--animation')){
    const sourceRoot=published.design.nodes.find(n=>n.parent===null);
    assert(root.id.endsWith(sourceRoot.id),'Unexpected Home instance mapping');
    const prefix=root.id.slice(0,-sourceRoot.id.length);
    const map=new Map(published.design.nodes.map(n=>[n.id,prefix+n.id]));
    for(const animation of published.design.animations)for(const curve of animation.clip.curves)assert(project.design.nodes.some(n=>n.id===map.get(curve.targetNodeId)),'Instance animation target missing');
    const hosts=new Set(published.design.animations.map(a=>map.get(a.hostNodeId)));
    project.design.animations=[...(project.design.animations??[]).filter(a=>!hosts.has(a.hostNodeId)),...remapAnimations(published.design.animations,map,prefix)];
  }
  const updated=await w.save(projectId,{projectId,revision:project.revision,projectVersion:project.project.version,design:project.design});
  console.log(JSON.stringify({template:'SvHome',version:result.version,projectRevision:updated.revision,candidate:candidate.id}));
}finally{await w.close();}
