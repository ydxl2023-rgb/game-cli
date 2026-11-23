import test from 'node:test';
import assert from 'node:assert/strict';
import {checkLayout,validateSuite} from '../server/scenarios.mjs';
import {integrationGate,runtimeChecks} from '../shared/integration-gate.mjs';

const step = {id:'cards',op:'layout',path:'List',count:4,columns:2,separate:true,fullyVisible:true,uniformScale:true};
const suite = {schema:'gamecli-scenarios/1',templateId:'test',steps:[step]};
function grid() {
  return {items:[0,1,2,3].map(i => {
    const bounds={x:(i%2)*104,y:Math.floor(i/2)*104,width:100,height:100};
    return {instanceId:i,bounds,visible:{...bounds},scale:{x:1,y:1}};
  })};
}

test('accept a two-row grid and match price bars to their own cards',()=>{
  const cards=grid(), prices=grid();
  for(const item of prices.items) item.bounds.height=15;
  assert.deepEqual(checkLayout(step,cards),[]);
  assert.deepEqual(checkLayout({...step,inside:'cards'},prices,cards),[]);
  prices.items[0].instanceId=1;
  assert.ok(checkLayout({...step,inside:'cards'},prices,cards).some(s=>s.includes('Outside')));
});

test('flag a covered row, partial clipping, incorrect wrapping and distorted scale',()=>{
  const data=grid();
  data.items[0].bounds.height+=16;
  data.items[1].scale.y=1.22;
  const result=checkLayout(step,data);
  for(const reason of ['Overlapping','Clipped','Nonuniform','row lengths']) assert.ok(result.some(s=>s.includes(reason)),reason);
});

test('validate scenario identity, selectors, operations and earlier containment targets',()=>{
  validateSuite(suite,'test');
  assert.throws(()=>validateSuite(suite,'other'));
  for(const change of [{count:0},{fullyVisible:'true'},{inside:'missing'},{op:'unknown'},{id:12},{overlap:false}])
    assert.throws(()=>validateSuite({...suite,steps:[{...step,...change}]},'test'));
  assert.throws(()=>validateSuite({...suite,steps:[step,step]},'test'));
});

test('automated scenarios supplement independent review and cannot override a failure',()=>{
  const report={fingerprint:'f',items:[{status:'present'}],references:[],placeholders:[]};
  const review={fingerprint:'f',reviewer:'QA',checks:runtimeChecks.map(name=>({name,status:'passed',evidence:'qa'}))};
  assert.equal(integrationGate({...report,scenarios:{status:'passed'}},null).passed,false);
  for(const [status,stage] of [['failed','repair'],['not-verified','awaiting-acceptance'],['passed','complete']])
    assert.equal(integrationGate({...report,scenarios:{status}},review).stage,stage);
});
