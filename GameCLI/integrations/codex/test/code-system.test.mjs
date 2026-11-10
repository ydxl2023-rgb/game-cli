import test from 'node:test';
import assert from 'node:assert/strict';
import {instantiateTemplate} from '../shared/templates.mjs';
import {validateDesign,designHtml} from '../shared/design.mjs';
import {templateFromDraft} from '../server/template-publish.mjs';
import {upgradeTemplate} from '../shared/template-upgrade.mjs';
import {hierarchyVisibility,createPreviewState,runPreviewAction} from '../shared/preview.mjs';
const template={id:'MasterSystem',name:'MasterSystem',category:'system',version:1,payload:{kind:'code',root:null,design:{schemaVersion:1,name:'MasterSource',width:1920,height:1080,nodes:[]}},scripts:{schemaVersion:1,files:[{path:'Master.cs',content:'class Master {}'}]}};
const stage=()=>({schemaVersion:1,name:'Project',width:1080,height:1920,nodes:[]});
test('code systems instantiate independent relation cards and survive project serialization',()=>{
 const design=stage();assert.throws(()=>instantiateTemplate(design,template,null,{x:0,y:0}),/关系区/);
 const id=instantiateTemplate(design,template,null,{x:0,y:0},{allowCode:true});const second=instantiateTemplate(design,template,null,{x:0,y:0},{allowCode:true});assert.notEqual(id,second);
 design.nodes[0].relationPosition={x:120,y:220};design.nodes[0].scripts.files[0].content='changed';
 const saved=validateDesign(JSON.parse(JSON.stringify(design)));assert.equal(saved.nodes[0].systemKind,'code');assert.equal(saved.width,1080);assert.equal(saved.nodes[1].scripts.files[0].content,'class Master {}');assert.equal(template.scripts.files[0].content,'class Master {}');
 assert.equal(hierarchyVisibility(saved,[],true).get(id),false);assert.ok(!designHtml(saved).includes('data-ui-name="MasterSystem"'));
 const state=createPreviewState(saved);assert.equal(runPreviewAction(saved,state,{type:'navigate',target:id}),state);
 const payload=templateFromDraft({design:saved},{rootId:id},{...template,category_id:'system'});assert.equal(payload.kind,'code');assert.equal(payload.design.nodes.length,0);
 const result=upgradeTemplate(saved,id,{...template,version:2});assert.equal(result.design.nodes.find(n=>n.id===result.rootId).templateSourceVersion,2);
});
