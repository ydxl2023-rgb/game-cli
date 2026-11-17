// Preserve all product navigation while replacing only an unedited reference layout.
import assert from 'node:assert/strict';
import {readFile,writeFile} from 'node:fs/promises';
import {createWorkspaces} from '../server/workspaces.mjs';
import {instantiateTemplate} from '../shared/templates.mjs';
import {validateDesign} from '../shared/design.mjs';
import {remapAnimations} from '../shared/animations.mjs';
const folder='.data/imports/sv-home/fidelity-candidate';
const w=createWorkspaces(),projectId='p-2d314388-18c4-42f5-80eb-f6729dd23f0e';
try{
 const payload=JSON.parse(await readFile(folder+'/publication.json','utf8'));
 const scripts=JSON.parse(await readFile(folder+'/scripts.json','utf8'));
 const published=await w.templateEditor.published('SvHome');
 const project=await w.snapshot(projectId),root=project.design.nodes.find(n=>n.parent===null&&n.templateSourceId==='SvHome');
 assert(root&&root.templateSourceVersion===published.revision,'Project Home is not at the reviewed baseline');
 const sourceRoot=payload.design.nodes.find(n=>n.parent===null),baselineRoot=published.design.nodes.find(n=>n.parent===null);
 assert(root.id.endsWith(sourceRoot.id));
 const prefix=root.id.slice(0,-sourceRoot.id.length),ids=new Set([root.id]);
 let count;do{count=ids.size;for(const n of project.design.nodes)if(ids.has(n.parent))ids.add(n.id);}while(count!==ids.size);
 const expected={schemaVersion:1,name:project.design.name,width:project.design.width,height:project.design.height,nodes:[]};
 instantiateTemplate(expected,{id:'SvHome',category:'system',version:published.revision,payload:{root:baselineRoot.id,design:published.design,configurations:baselineRoot.configurations},scripts:baselineRoot.scripts},null,{x:960,y:540});
 const normalize=nodes=>nodes.map(n=>{const value=structuredClone(n);delete value.action;if(value.id===root.id){delete value.relationPosition;delete value.relationScale;delete value.active;}return value;}).sort((a,b)=>a.id.localeCompare(b.id));
 assert.deepEqual(normalize(project.design.nodes.filter(n=>ids.has(n.id))),normalize(expected.nodes),'Home has layout/source edits; preserve them instead of replacing');
 const sortAnimations=value=>structuredClone(value??[]).sort((a,b)=>a.id.localeCompare(b.id));
 assert.deepEqual(sortAnimations(project.design.animations?.filter(a=>ids.has(a.hostNodeId))),sortAnimations(expected.animations),'Project Home animations were edited');
 const actions=new Map(project.design.nodes.filter(n=>ids.has(n.id)&&n.action).map(n=>[n.id,n.action]));
 sourceRoot.templateSourceId='SvHome';sourceRoot.scripts=scripts;sourceRoot.configurations=payload.configurations;
 const map=new Map(payload.design.nodes.map(n=>[n.id,prefix+n.id]));
 const replacement=structuredClone(payload.design.nodes).map(n=>{n.id=map.get(n.id);n.parent=n.parent===null?null:map.get(n.parent);if(n.action?.target)n.action.target=map.get(n.action.target);if(actions.has(n.id))n.action=actions.get(n.id);return n;});
 const nextRoot=replacement.find(n=>n.parent===null);
 nextRoot.templateSourceVersion=published.revision+1;nextRoot.active=root.active;nextRoot.relationPosition=root.relationPosition;nextRoot.relationScale=root.relationScale;
 const next=validateDesign({...project.design,nodes:[...project.design.nodes.filter(n=>!ids.has(n.id)),...replacement],animations:[...(project.design.animations??[]).filter(a=>!ids.has(a.hostNodeId)),...remapAnimations(payload.design.animations,map,prefix)]});
 await writeFile(folder+'/project-before.json',JSON.stringify(project));
 if(process.argv.includes('--apply')){
  const opened=await w.templateEditor.open({templateId:'SvHome'}),draft=await w.templateEditor.read(opened.id);
  assert.deepEqual(draft.design,published.design,'Preserve edited template draft');
  const saved=await w.templateEditor.save(opened.id,{draftId:opened.id,revision:draft.revision,design:payload.design});
  const candidate=await w.templateEditor.submit(opened.id,{draftId:opened.id,revision:saved.revision});
  const result=await w.publish({candidateId:candidate.id,confirm:true});
  assert.equal(result.version,nextRoot.templateSourceVersion);
  await w.save(projectId,{projectId,revision:project.revision,projectVersion:project.project.version,design:next});
  const actual=await w.snapshot(projectId);
  assert.equal(actual.design.nodes.find(n=>n.id===root.id).templateSourceVersion,result.version);
  for(const [id,action]of actions)assert.deepEqual(actual.design.nodes.find(n=>n.id===id)?.action,action,'Navigation changed');
  console.log(JSON.stringify({version:result.version,projectRevision:actual.revision,candidate:candidate.id}));
 }else console.log(JSON.stringify({dryRun:true,nextVersion:published.revision+1,nodes:next.nodes.length,navigationPreserved:actions.size}));
}finally{await w.close();}
