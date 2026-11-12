import test from 'node:test';
import assert from 'node:assert/strict';
import {compose,checkComposition} from '../shared/composition.mjs';
import {builtInTemplates} from '../shared/templates.mjs';
test('agent composition batches preserve input and reject unsupported close graph edges',()=>{
 const source={schemaVersion:1,name:'Project',width:1920,height:1080,nodes:[]},t=builtInTemplates().find(t=>t.category==='system');
 const result=compose(source,[{type:'add',key:'bag',templateId:t.id,version:t.version},{type:'arrange'}],[t]);assert.equal(source.nodes.length,0);assert.equal(checkComposition(result.design).systems.length,1);
 assert.throws(()=>compose(result.design,[{type:'connect',sourceId:result.added.bag,targetId:result.added.bag,action:'hide'}],[]),/仅支持/);
 assert.equal(compose(result.design,[{type:'remove',id:result.added.bag}],[]).design.nodes.length,0);
});
