import test from 'node:test';
import assert from 'node:assert/strict';
import {validateDesign} from '../shared/design.mjs';
import {relations} from '../shared/relations.mjs';
import {moveGroup,resizeGroupFrame,fitGroupCards} from '../shared/relation-groups.mjs';
import {compose} from '../shared/composition.mjs';
import {templateFromDraft} from '../server/template-publish.mjs';
function fixture(){return {schemaVersion:1,name:'Demo',width:1080,height:1920,nodes:[
  {id:'Launch',parent:null,startupComplete:{type:'show',target:'Login'}},
  {id:'Login',parent:null,relationPosition:{x:100,y:100}},
  {id:'Announcement',parent:'Login',action:{type:'show',target:'Notice'}},
  {id:'Notice',parent:null,relationPosition:{x:400,y:100}}
].map(n=>({type:'Container',name:n.id,x:0,y:0,width:100,height:100,text:'',color:'#ffffff',fontSize:20,...n})),relationGroups:[{id:'group-login',name:'登录流程',members:['Login','Notice'],collapsed:true}]};}
test('legacy collapsed groups become frames without changing endpoints, actions or preview hierarchy',()=>{
  const source=fixture(),d=validateDesign(JSON.parse(JSON.stringify(source)));
  assert.deepEqual(d.relationGroups,[{id:'group-login',name:'登录流程',members:['Login','Notice']}]);
  assert.equal(relations(d).length,2);
  assert.deepEqual(relations(d).map(e=>e.targetId).sort(),['Login','Notice']);
  d.relationGroups=[];assert.equal(relations(d).length,2);
  assert.equal(d.nodes.find(n=>n.id==='Notice').parent,null);
});
test('groups reject nested, duplicate, missing and non-root members',()=>{
  for(const members of [['Login','Login'],['Missing'],['Announcement'],['group-login'],[]]){const d=fixture();d.relationGroups[0].members=members;assert.throws(()=>validateDesign(d));}
  const d=fixture();d.relationGroups.push({...d.relationGroups[0],id:'group-other'});assert.throws(()=>validateDesign(d));
});
test('deleting systems prunes group membership and removes empty groups',()=>{
  let d=compose(fixture(),[{type:'remove',id:'Notice'}],[]).design;
  assert.deepEqual(d.relationGroups[0].members,['Login']);
  d=compose(d,[{type:'remove',id:'Login'}],[]).design;assert.deepEqual(d.relationGroups,[]);
});
test('moving a group preserves member offsets and clamps the group as a unit',()=>{
  const d=fixture(),positions=new Map(d.nodes.filter(n=>n.relationPosition).map(n=>[n.id,n.relationPosition]));
  moveGroup(d,'group-login',positions,-200,75);
  assert.deepEqual(d.nodes.find(n=>n.id==='Login').relationPosition,{x:0,y:175});
  assert.deepEqual(d.nodes.find(n=>n.id==='Notice').relationPosition,{x:300,y:175});
  assert.equal(d.nodes.find(n=>n.id==='Announcement').action.target,'Notice');
});
test('publishing one template never copies project group membership',()=>{
  const d=fixture();delete d.nodes.find(n=>n.id==='Announcement').action;
  const template=templateFromDraft({design:d},{rootId:'Login'},{id:'SvLogin',category_id:'system',payload:{root:'Login',design:d}});
  assert.equal(template.design.relationGroups,undefined);
});
test('manual frame sizes survive saving and moving without resizing members',()=>{
  const d=fixture(),before=structuredClone(d.nodes);
  d.relationGroups[0].frame=resizeGroupFrame({x:50,y:40,width:600,height:400},'se',100,60);
  const saved=validateDesign(d);
  assert.deepEqual(saved.relationGroups[0].frame,{x:50,y:40,width:700,height:460});
  assert.deepEqual(d.nodes,before);
  moveGroup(saved,'group-login',new Map([['Login',{x:100,y:100}],['Notice',{x:400,y:100}]]),-100,20);
  assert.deepEqual(saved.relationGroups[0].frame,{x:0,y:60,width:700,height:460});
  assert.deepEqual(saved.nodes.find(n=>n.id==='Login').relationPosition,{x:50,y:120});
  d.relationGroups[0].frame.width=-1;assert.throws(()=>validateDesign(d),/边框/);
});
test('resizing north-west preserves the opposite corner and enforces minimum dimensions',()=>{
  const frame={x:50,y:40,width:600,height:400};
  assert.deepEqual(resizeGroupFrame(frame,'nw',100,50),{x:150,y:90,width:500,height:350});
  assert.deepEqual(resizeGroupFrame(frame,'nw',900,900),{x:370,y:320,width:280,height:120});
  assert.deepEqual(resizeGroupFrame(frame,'nw',-900,-900),{x:0,y:0,width:650,height:440});
});
test('shrinking a frame fits every card proportionally inside its content area',()=>{
  const cards=[{id:'Login',x:100,y:100,width:232,height:264,scale:1},{id:'Notice',x:600,y:120,width:232,height:228,scale:1}];
  const frame={x:80,y:40,width:350,height:220},layout=fitGroupCards(frame,cards);
  assert.equal(layout[0].scale,layout[1].scale);assert.ok(layout[0].scale<1);
  for(const item of layout){const source=cards.find(c=>c.id===item.id);assert.ok(item.x>=frame.x+16);assert.ok(item.y>=frame.y+52);assert.ok(item.x+source.width*item.scale<=frame.x+frame.width-16+1e-8);assert.ok(item.y+source.height*item.scale<=frame.y+frame.height-16+1e-8);}
  const d=fixture();for(const item of layout){const node=d.nodes.find(n=>n.id===item.id);node.relationPosition={x:item.x,y:item.y};node.relationScale=item.scale;}
  assert.equal(validateDesign(d).nodes.find(n=>n.id==='Login').relationScale,layout[0].scale);
  assert.deepEqual(fitGroupCards({x:0,y:0,width:1000,height:600},cards),cards.map(({id,x,y,scale})=>({id,x,y,scale})));
});
