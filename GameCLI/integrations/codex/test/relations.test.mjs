import test from 'node:test';
import assert from 'node:assert/strict';
import {connectUi,relations} from '../shared/relations.mjs';
import {validateDesign} from '../shared/design.mjs';
import {createPreviewState,runPreviewAction} from '../shared/preview.mjs';
function fixture(){return {schemaVersion:1,name:'ProjectUi',width:1080,height:1920,nodes:[
 {id:'Home',parent:null,type:'Container',relationPosition:{x:32,y:40}},
 {id:'Open',parent:'Home',type:'Button'},
 {id:'Inventory',parent:null,type:'Container',previewHidden:true}
].map(n=>({...n,name:n.id,x:0,y:0,width:128,height:128,color:'#ffffff',text:'',fontSize:20}))};}
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
 const d=fixture();connectUi(d,'Open','Inventory','show');connectUi(d,'Open','Home','hide');assert.equal(relations(d).length,1);assert.equal(d.nodes[1].action.type,'hide');
 assert.throws(()=>connectUi(d,'Open','Missing'));assert.throws(()=>connectUi(d,'Home','Open'));
 d.nodes[0].relationPosition.x=Infinity;assert.throws(()=>validateDesign(d),/卡片/);
});
