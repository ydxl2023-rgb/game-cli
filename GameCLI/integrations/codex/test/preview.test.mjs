import test from 'node:test';
import assert from 'node:assert/strict';
import {starter,appendControl,validateDesign} from '../shared/design.mjs';
import {createPreviewState,defaultAction,runPreviewAction,hierarchyVisibility} from '../shared/preview.mjs';

test('parent visibility cascades through descendants without overwriting their own active state',()=>{
  const d={nodes:[{id:'Text',parent:'Panel'},{id:'Panel',parent:'Loading'},{id:'Loading',parent:null,active:false},{id:'Hidden',parent:'Panel',active:false},{id:'Other',parent:null}]};
  let visibility=hierarchyVisibility(d);
  for(const id of ['Loading','Panel','Text','Hidden']) assert.equal(visibility.get(id),false);
  assert.equal(visibility.get('Other'),true);
  d.nodes.find(n=>n.id==='Loading').active=true;
  visibility=hierarchyVisibility(d);
  for(const id of ['Loading','Panel','Text']) assert.equal(visibility.get(id),true);
  assert.equal(visibility.get('Hidden'),false);
  assert.equal(hierarchyVisibility(d,['Loading']).get('Text'),false);
  assert.equal(d.nodes.find(n=>n.id==='Text').active,undefined);
});

test('preview activates authored inactive dialogs without changing source flags',()=>{
 const d={nodes:[{id:'Root',parent:null},{id:'Dialog',parent:'Root',active:false},{id:'Label',parent:'Dialog'},{id:'Variant',parent:'Dialog',active:false}]};
 const before=JSON.stringify(d);let state=createPreviewState(d);
 assert.equal(hierarchyVisibility(d,state.hidden,true).get('Label'),false);
 state=runPreviewAction(d,state,{type:'show',target:'Dialog'});
 assert.equal(hierarchyVisibility(d,state.hidden,true).get('Label'),true);
 assert.equal(hierarchyVisibility(d,state.hidden,true).get('Variant'),false);
 state=runPreviewAction(d,state,{type:'back'});
 assert.equal(hierarchyVisibility(d,state.hidden,true).get('Label'),false);
 assert.equal(JSON.stringify(d),before);
});
test('preview opens and dismisses dialogs and returns without changing design',()=>{
  const d=starter(), id=appendControl(d,'ConfirmDialog',null,{x:0,y:0});
  const before=JSON.stringify(d);
  let state=createPreviewState(d);
  assert.ok(state.hidden.includes(id));
  state=runPreviewAction(d,state,{type:'show',target:id});
  assert.ok(!state.hidden.includes(id));
  for(const suffix of ['Cancel','Confirm','Mask']) assert.deepEqual(defaultAction(d,d.nodes.find(n=>n.id===id+suffix)),{type:'hide',target:id});
  state=runPreviewAction(d,state,defaultAction(d,d.nodes.find(n=>n.id===id+'Cancel')));
  assert.ok(state.hidden.includes(id));
  state=runPreviewAction(d,state,{type:'back'});
  assert.ok(!state.hidden.includes(id));
  assert.equal(JSON.stringify(d),before);
});
test('navigation hides sibling screens, supports return, and retains validated actions',()=>{
  const d=starter(), id=appendControl(d,'Container',null,{x:0,y:0});
  const button=d.nodes.find(n=>n.id==='EquipButton');
  button.action={type:'navigate',target:id};
  d.nodes.find(n=>n.id===id).previewHidden=true;
  const clean=validateDesign(d);
  assert.deepEqual(clean.nodes.find(n=>n.id===button.id).action,button.action);
  let state=createPreviewState(clean);
  state=runPreviewAction(clean,state,button.action);
  assert.ok(state.hidden.includes('InventoryPanel')); assert.ok(!state.hidden.includes(id));
  state=runPreviewAction(clean,state,{type:'back'});
  assert.ok(!state.hidden.includes('InventoryPanel')); assert.ok(state.hidden.includes(id));
  button.action.target='missing'; assert.throws(()=>validateDesign(d),/目标节点不存在/);
  button.action={type:'script'}; assert.throws(()=>validateDesign(d),/点击动作无效/);
});
