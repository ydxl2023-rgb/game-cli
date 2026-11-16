import test from 'node:test';import assert from 'node:assert/strict';import {integrationGate,runtimeChecks} from '../shared/integration-gate.mjs';
test('completion requires current evidence for every runtime check',()=>{
 const r={fingerprint:'current',items:[{path:'Assets/A',status:'present'}],references:[],placeholders:[]};
 assert.equal(integrationGate(r).stage,'awaiting-acceptance');
 const a={fingerprint:'current',reviewer:'reviewer',checks:runtimeChecks.map(name=>({name,status:'passed',evidence:'Unity CLI evidence/report.json'}))};
 assert.equal(integrationGate(r,a).stage,'complete');
 assert.equal(integrationGate(r,{...a,fingerprint:'old'}).stage,'awaiting-acceptance');
 assert.equal(integrationGate({...r,placeholders:[{}]},a).stage,'repair');
 assert.equal(integrationGate({...r,renderOrder:[{errors:[{code:'backdrop-over-content'}]}]},a).stage,'repair','runtime claims cannot waive a failed structural contract');
 assert.equal(integrationGate({...r,items:[{path:'Assets/A',status:'adapted-review-required'}]},a).stage,'repair');
 assert.equal(integrationGate(r,{...a,checks:[]}).passed,false);
});
