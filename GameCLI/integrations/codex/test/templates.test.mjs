import test from 'node:test';
import assert from 'node:assert/strict';
import {builtInTemplates,instantiateTemplate,validateTemplate} from '../shared/templates.mjs';
import {validateDesign,resizeNode} from '../shared/design.mjs';
import {createPreviewState,defaultAction,runPreviewAction} from '../shared/preview.mjs';

const empty=()=>({schemaVersion:1,name:'CanvasDesign',width:1080,height:1920,nodes:[]});
test('landscape system switches canvas resolution without stretching source layout or existing nodes',()=>{
  const t=builtInTemplates().find(t=>t.category==='system');
  t.payload.design.width=1920;t.payload.design.height=1080;
  const d=empty(), original={id:'Existing',parent:null,type:'Container',x:10,y:20,width:100,height:200,color:'#000000',text:'',fontSize:32};d.nodes.push(original);
  const sourceTitle=t.payload.design.nodes.find(n=>n.id.endsWith('Title'));
  const id=instantiateTemplate(d,t,null,{x:0,y:0});
  assert.equal(d.width,1920);assert.equal(d.height,1080);
  const title=d.nodes.find(n=>n.id===id+'Title');assert.equal(title.x,sourceTitle.x);assert.equal(title.width,sourceTitle.width);
  assert.deepEqual(d.nodes.find(n=>n.id==='Existing'),validateDesign({...empty(),nodes:[original]}).nodes[0]);
});
test('stored templates instantiate their payload and remap nested references independently',()=>{
  const t=builtInTemplates().find(t=>t.id==='InventorySystem');
  t.payload.design.nodes.find(n=>n.id.endsWith('Title')).text='来自数据库的标题';
  const d=empty(), first=instantiateTemplate(d,t,null,{x:0,y:0}), count=d.nodes.length;
  const second=instantiateTemplate(d,t,null,{x:0,y:0});
  assert.notEqual(first,second);assert.equal(new Set(d.nodes.map(n=>n.id)).size,d.nodes.length);
  assert.equal(d.nodes.find(n=>n.id===first+'Title').text,'来自数据库的标题');
  const button=d.nodes.find(n=>n.id===first+'Use'), dialog=d.nodes.find(n=>n.id===button.action.target);
  assert.equal(dialog.parent,d.nodes.find(n=>n.parent===first&&n.animationHost).id);
  const secondAction=d.nodes.find(n=>n.id===second+'Use').action;assert.notEqual(secondAction.target,dialog.id);
  let state=createPreviewState(d);state=runPreviewAction(d,state,button.action);assert.ok(!state.hidden.includes(dialog.id));
  assert.deepEqual(defaultAction(d,d.nodes.find(n=>n.id===dialog.id+'Confirm')),{type:'hide',target:dialog.id});
  const scroll=d.nodes.find(n=>n.parent===d.nodes.find(v=>v.parent===first&&v.animationHost).id&&n.template==='ScrollPanel');resizeNode(d,scroll,320,600);
  assert.equal(d.nodes.filter(n=>n.parent===scroll.id+'Content').length,12);
  assert.doesNotThrow(()=>validateDesign(d));assert.equal(d.nodes.length,count*2);
});
test('stored prefabs retain 128px size, place in selected container and reject broken packages atomically',()=>{
  const library=builtInTemplates(),d=empty();
  const system=instantiateTemplate(d,library.find(t=>t.category==='system'),null,{x:0,y:0});
  const id=instantiateTemplate(d,library.find(t=>t.id==='ItemSlot'),system,{x:200,y:200});
  const item=d.nodes.find(n=>n.id===id);assert.equal(item.parent,system);assert.equal(item.width,128);assert.equal(item.x,136);
  const broken=structuredClone(library[0]);broken.payload.root='missing';const before=JSON.stringify(d);
  assert.throws(()=>instantiateTemplate(d,broken,null,{x:0,y:0}));assert.equal(JSON.stringify(d),before);
  const external=structuredClone(library[0]);external.payload.design.nodes[0].action={type:'show',target:'external'};
  assert.throws(()=>validateTemplate(external.payload));
});
