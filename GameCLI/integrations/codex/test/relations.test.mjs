import test from 'node:test';
import assert from 'node:assert/strict';
import {connectUi,relations,thumbnailVisibility} from '../shared/relations.mjs';
import {validateDesign} from '../shared/design.mjs';
import {createPreviewState,runPreviewAction} from '../shared/preview.mjs';
function fixture(){return {schemaVersion:1,name:'ProjectUi',width:1080,height:1920,nodes:[
 {id:'Home',parent:null,type:'Container',relationPosition:{x:32,y:40}},
 {id:'Open',parent:'Home',type:'Button'},
 {id:'Inventory',parent:null,type:'Container',previewHidden:true}
].map(n=>({...n,name:n.id,x:0,y:0,width:128,height:128,color:'#ffffff',text:'',fontSize:20}))};}

test('thumbnail restores a hidden root but preserves inactive internal subtrees',()=>{
 const d={nodes:[{id:'Loading',parent:null,active:false},{id:'Animator',parent:'Loading'},{id:'Image',parent:'Animator'},{id:'Dialog',parent:'Animator',active:false},{id:'Text',parent:'Dialog'}]};
 const before=JSON.stringify(d),visibility=thumbnailVisibility(d,'Loading');
 for(const id of ['Loading','Animator','Image'])assert.equal(visibility.get(id),true);
 for(const id of ['Dialog','Text'])assert.equal(visibility.get(id),false);
 assert.equal(JSON.stringify(d),before);
});
test('connecting UI writes the preview action and survives saving',()=>{
 const d=fixture();connectUi(d,'Open','Inventory');
 const saved=validateDesign(JSON.parse(JSON.stringify(d)));
 assert.deepEqual(relations(saved),[{sourceId:'Open',sourceRoot:'Home',targetId:'Inventory',targetRoot:'Inventory',type:'navigate'}]);
 const state=runPreviewAction(saved,createPreviewState(saved),saved.nodes.find(n=>n.id==='Open').action);
 assert.ok(state.hidden.includes('Home'));assert.ok(!state.hidden.includes('Inventory'));
 assert.deepEqual(saved.nodes[0].relationPosition,{x:32,y:40});
 delete saved.nodes[1].action;assert.equal(relations(saved).length,0);
});
test('reconnecting replaces the source action and validates targets and positions',()=>{
 const d=fixture();connectUi(d,'Open','Inventory','show');connectUi(d,'Open','Home','hide');assert.equal(relations(d).length,0);assert.equal(d.nodes[1].action.type,'hide');
 assert.throws(()=>connectUi(d,'Open','Missing'));assert.throws(()=>connectUi(d,'Home','Open'));
 d.nodes[0].relationPosition.x=Infinity;assert.throws(()=>validateDesign(d),/卡片/);
});

test('different buttons retain independent actions across save, rename and removal',()=>{
 const d=fixture();d.nodes.push({...d.nodes[1],id:'Account',name:'Account'});
 connectUi(d,'Open','Inventory','show');connectUi(d,'Account','Inventory','navigate');
 const saved=validateDesign(JSON.parse(JSON.stringify(d)));
 assert.equal(relations(saved).length,2);
 saved.nodes.find(n=>n.id==='Open').name='公告入口';
 assert.equal(relations(saved).find(e=>e.sourceId==='Open').type,'show');
 delete saved.nodes.find(n=>n.id==='Account').action;
 assert.deepEqual(relations(saved).map(e=>e.sourceId),['Open']);
});

test('system graph keeps opening while closing and local interactions still preview',()=>{
 const d=fixture();
 d.nodes.push(...[
  {id:'Close',parent:'Inventory',type:'Button',action:{type:'hide',target:'Inventory'}},
  {id:'Expand',parent:'Inventory',type:'Button',action:{type:'toggle',target:'Content'}},
  {id:'Check',parent:'Inventory',type:'Button',action:{type:'toggle',target:'Selected'}},
  {id:'Content',parent:'Inventory',type:'Container'},
  {id:'Selected',parent:'Inventory',type:'Image'}
 ].map(n=>({...d.nodes[1],...n,name:n.id})));
 connectUi(d,'Open','Inventory','show');
 const saved=validateDesign(JSON.parse(JSON.stringify(d))),before=JSON.stringify(saved);
 assert.deepEqual(relations(saved).map(e=>[e.sourceId,e.targetId,e.type]),[
  ['Open','Inventory','show']
 ]);
 assert.equal(JSON.stringify(saved),before);
 let state=createPreviewState(saved);
 for(const id of ['Expand','Check'])state=runPreviewAction(saved,state,saved.nodes.find(n=>n.id===id).action);
 assert.ok(state.hidden.includes('Content'));
 assert.ok(state.hidden.includes('Selected'));
 state=runPreviewAction(saved,state,saved.nodes.find(n=>n.id==='Open').action);
 assert.ok(!state.hidden.includes('Inventory'));
 state=runPreviewAction(saved,state,saved.nodes.find(n=>n.id==='Close').action);
 assert.ok(state.hidden.includes('Inventory'));
});
