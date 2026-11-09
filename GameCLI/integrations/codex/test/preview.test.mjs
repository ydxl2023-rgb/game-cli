import test from 'node:test';
import assert from 'node:assert/strict';
import {starter,appendControl,validateDesign} from '../shared/design.mjs';
import {createPreviewState,defaultAction,runPreviewAction} from '../shared/preview.mjs';

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
