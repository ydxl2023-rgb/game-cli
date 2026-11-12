import assert from 'node:assert/strict';
import {createWorkspaces,workspaceActor} from '../server/workspaces.mjs';
import {openDatabase} from '../server/database.mjs';
import {createTemplateEditor} from '../server/template-editor.mjs';
import {validateDesign} from '../shared/design.mjs';
import {sampleCurve,validateAnimations} from '../shared/animations.mjs';
import {playAnimations} from '../ui/animation-player.mjs';

const apply=process.argv.includes('--apply');
const workspaces=createWorkspaces();
const database=await openDatabase();
const actor=workspaceActor();
const templateEditor=createTemplateEditor({connection:async()=>database,actor,admin:async()=>{
 const result=await database.pool.query('SELECT actor FROM gamecli_ui.template_admins WHERE actor=$1',[actor]);
 assert(result.rowCount,'Template administrator required');
}});
try {
 const published=await templateEditor.published('SvNotice');
 const design=structuredClone(published.design);
 const root=design.nodes.find(n=>n.parent===null);
 const host=design.nodes.find(n=>n.parent===root.id&&n.name==='Animator');
 assert(host?.animationHost,'Notice animation host is missing');
 assert(!design.animations?.length,'Review existing animation before replacing it');
 const standard=(await workspaces.animations()).animations.find(a=>a.id==='dialog.open.scale'&&a.version===1);
 assert(standard,'Published standard animation v1 is missing');
 const panel={...structuredClone(host),id:root.id+'OpenPanel',name:'Panel',parent:host.id};
 delete panel.animationHost;
 const members=['WhiteBg','TopBg','ScrollView','WeekNomoreShow'];
 for(const name of members){
  const node=design.nodes.find(n=>n.parent===host.id&&n.name===name);
  assert(node,`Missing ${name}`);node.parent=panel.id;
 }
 design.nodes.splice(design.nodes.indexOf(host)+1,0,panel);
 const targets=[panel,...design.nodes.filter(n=>n.parent===host.id&&n.name==='Empty')];
 const curves=targets.flatMap(n=>standard.definition.clip.curves.map(c=>({
  targetNodeId:n.id,component:c.component,property:c.property,
  keys:c.keys.map(k=>({time:k.time,value:k.value,inTangent:k.inSlope,outTangent:k.outSlope,inWeight:k.inWeight,outWeight:k.outWeight,weightedMode:k.weightedMode})),
  preWrapMode:c.preWrapMode,postWrapMode:c.postWrapMode
 })));
 design.animations=[{id:root.id+'StandardOpen',hostNodeId:host.id,
  source:{standardAnimationId:standard.id,version:standard.version},
  state:{name:'Open',default:true,speed:1,cycleOffset:0},
  clip:{name:'NoticeOpen',startTime:0,stopTime:0.22,frameRate:60,loopTime:false,curves}}];
 validateDesign(design);validateAnimations(design.animations,design.nodes);
 assert.equal(sampleCurve(curves[0].keys,0),0.94);
 assert.equal(sampleCurve(curves[0].keys,0.22),1);
 // Exercise the actual preview player with deterministic frames.
 const elements=design.nodes.map(n=>({dataset:{node:n.id},style:{transform:'',backgroundPosition:''}}));
 let frame;globalThis.requestAnimationFrame=callback=>{frame=callback;return 1;};globalThis.cancelAnimationFrame=()=>{};
 const before=performance.now();const stop=playAnimations(design,{querySelectorAll:()=>elements});
 const element=elements.find(e=>e.dataset.node===panel.id);
 assert.match(element.style.transform,/scale\(0.94,0.94\)/);
 const bg=elements.find(e=>e.dataset.node===design.nodes.find(n=>n.name==='Bg').id);
 assert.equal(bg.style.transform,'');frame(before+1000);assert.match(element.style.transform,/scale\(1,1\)/);stop();
 let result={dryRun:!apply,template:'SvNotice',baseVersion:published.workspace.baseVersion,previewPlayerVerified:true};
 if(apply){
  const {id}=await templateEditor.open({templateId:'SvNotice'});
  const draft=await templateEditor.read(id);
  assert.deepEqual(draft.design,published.design,'Existing draft differs; preserve it and review before proceeding');
  const saved=await templateEditor.save(id,{draftId:id,revision:draft.revision,design});
  const candidate=await templateEditor.submit(id,{draftId:id,revision:saved.revision});
  result={...result,...await workspaces.publish({candidateId:candidate.id,confirm:true})};
  const readback=await templateEditor.published('SvNotice');
  assert.deepEqual(readback.design.animations,design.animations);
  assert.deepEqual(readback.design.nodes.find(n=>n.parent===null).scripts,root.scripts);
  result.readbackVerified=true;
 }
 console.log(JSON.stringify(result));
} finally {await workspaces.close();await database.close();}
