import test from 'node:test';
import assert from 'node:assert/strict';
import {validateTemplate,instantiateTemplate} from '../shared/templates.mjs';

const payload={kind:'code',root:null,design:{schemaVersion:1,name:'MasterSystem',width:1920,height:1080,nodes:[]}};
test('code-only systems retain an empty design and never instantiate UI',()=>{
 const result=validateTemplate(payload,'system');
 assert.equal(result.kind,'code');assert.equal(result.root,null);assert.deepEqual(result.design.nodes,[]);
 const design=structuredClone(payload.design),before=structuredClone(design);
 assert.throws(()=>instantiateTemplate(design,{category:'system',payload:result},null,{x:0,y:0}),/纯代码/);
 assert.deepEqual(design,before);
});
test('code-only payloads reject prefab categories and hidden UI content',()=>{
 assert.throws(()=>validateTemplate(payload,'prefab'));
 assert.throws(()=>validateTemplate({...payload,root:'Fake'},'system'));
 assert.throws(()=>validateTemplate({...payload,design:{...payload.design,nodes:[{id:'Fake'}]}},'system'));
 assert.throws(()=>validateTemplate({...payload,design:{...payload.design,animations:[{}]}},'system'));
});
