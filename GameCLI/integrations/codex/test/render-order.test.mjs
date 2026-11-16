import test from 'node:test';
import assert from 'node:assert/strict';
import {inspectRenderOrder,assertRenderOrder} from '../shared/render-order.mjs';
import {createWorkspaces} from '../server/workspaces.mjs';
import {builtInTemplates,instantiateTemplate} from '../shared/templates.mjs';
import {validateDesign} from '../shared/design.mjs';
const bg={id:'bg',name:'Bg',parent:'host',role:'backdrop',mustRenderBehind:['Panel'],color:'#000000',opacity:180/255};
const panel={id:'panel',name:'Panel',parent:'host'};
test('Notice regression: backdrop after Panel fails even when baseline matches Unity',()=>{
 const r=inspectRenderOrder({nodes:[panel,bg]});assert.equal(r.status,'failed');assert.equal(r.errors[0].code,'backdrop-over-content');assert.throws(()=>assertRenderOrder({nodes:[panel,bg]}));
});
test('backdrop behind panel passes without lowering opacity',()=>assert.equal(inspectRenderOrder({nodes:[bg,panel]}).passed,true));
test('legacy background role supports explicit contracts without guessing ordinary background intent',()=>{
 assert.equal(inspectRenderOrder({nodes:[{...bg,role:'background'},panel]}).passed,true);
 assert.equal(inspectRenderOrder({nodes:[panel,{...bg,role:'background'}]}).status,'failed');
 const {mustRenderBehind,...ordinary}=bg;assert.equal(inspectRenderOrder({nodes:[panel,{...ordinary,role:'background'}]}).passed,true);
});
test('missing, duplicate, self and non-sibling targets fail closed',()=>{
 for(const nodes of [[bg],[bg,panel,{...panel,id:'duplicate'}],[{...bg,mustRenderBehind:['Bg']}],[bg,{...panel,parent:'other'}]])assert.equal(inspectRenderOrder({nodes}).passed,false);
});
test('malformed or empty contract fails',()=>{
 for(const mustRenderBehind of [null,[],42,['Panel','Panel'],[{}]])assert.equal(inspectRenderOrder({nodes:[{...bg,mustRenderBehind},panel]}).passed,false);
});
test('legacy ambiguous dark layer is warning rather than blocking guessed intent',()=>{
 const {role,mustRenderBehind,...legacy}=bg;const r=inspectRenderOrder({nodes:[panel,legacy]});assert.equal(r.errors.length,0);assert.equal(r.warnings[0].code,'unannotated-backdrop');
});
test('contract survives instance ID remapping',()=>assert.equal(inspectRenderOrder({nodes:[{...bg,id:'T1bg',parent:'T1host'},{...panel,id:'T1panel',parent:'T1host'}]}).passed,true));
test('saving and instantiating preserve contracts',()=>{
 const template=builtInTemplates()[0],nodes=template.payload.design.nodes;
 const siblings=nodes.filter(n=>n.parent===nodes[0].id);
 siblings[0].role='backdrop';siblings[0].mustRenderBehind=[siblings[1].name];
 template.payload.design=validateDesign(template.payload.design);
 const design={schemaVersion:1,name:'Test',width:1080,height:1920,nodes:[]};
 instantiateTemplate(design,template,null,{x:0,y:0});
 assert.deepEqual(design.nodes.find(n=>n.role==='backdrop').mustRenderBehind,[siblings[1].name]);
 assert.equal(inspectRenderOrder(design).passed,true);
});
test('publishing rejects wrong backdrop order before any database write',async()=>{
 const template=builtInTemplates()[0],nodes=template.payload.design.nodes;
 const siblings=nodes.filter(n=>n.parent===nodes[0].id);
 siblings[1].role='backdrop';siblings[1].mustRenderBehind=[siblings[0].name];
 const queries=[];
 const client={release(){},async query(sql){queries.push(sql);
  if(sql.includes('template_admins'))return {rows:[{actor:'local'}]};
  if(sql.includes('template_candidates WHERE'))return {rows:[{status:'pending',template_id:template.id,base_version:1,payload:template.payload,scripts:null}]};
  if(sql.includes('templates WHERE'))return {rows:[{id:template.id,version:1,category_id:template.category}]};
  return {rows:[]};
 }};
 const w=createWorkspaces({connect:async()=>({pool:{connect:async()=>client},close:async()=>{}})});
 try{await assert.rejects(w.publish({candidateId:'test',confirm:true}),/backdrop-over-content/);assert.equal(queries.at(-1),'ROLLBACK');assert.equal(queries.some(q=>q.startsWith('UPDATE')),false);}finally{await w.close();}
});
