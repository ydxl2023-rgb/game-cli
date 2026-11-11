import test from 'node:test';
import assert from 'node:assert/strict';
import {readFile} from 'node:fs/promises';
import {validateStandardAnimation,animationHash,bindStandardAnimation} from '../server/standard-animations.mjs';
const sample=JSON.parse(await readFile(new URL('../config/animations/dialog.open.scale.v1.json',import.meta.url),'utf8'));

test('portable standard binds multiple window nodes without changing keyframes',()=>{
 const before=structuredClone(sample);
 const bound=bindStandardAnimation(sample,{dialogContent:['Visual/Panel','EmptyLabel']});
 assert.equal(bound.curves.length,6);
 assert.deepEqual(bound.curves[0].keys,sample.clip.curves[0].keys);
 assert.equal(bound.curves[3].path,'EmptyLabel');
 assert.deepEqual(sample,before);
 assert.ok(sample.clip.curves.every(c=>!('path' in c)));
 assert.equal(animationHash(sample),animationHash(Object.fromEntries(Object.entries(sample).reverse())));
});
test('reject incomplete, duplicate, invalid and concrete standard bindings',()=>{
 assert.throws(()=>bindStandardAnimation(sample,{}));
 assert.throws(()=>bindStandardAnimation(sample,{dialogContent:['../Panel']}));
 assert.throws(()=>bindStandardAnimation(sample,{dialogContent:['Panel','Panel']}));
 const bad=structuredClone(sample);bad.clip.curves[0].path='Notice/Panel';
 assert.throws(()=>validateStandardAnimation(bad));
});
test('reject inconsistent duration, endpoints and missing scale axes',()=>{
 const bad=structuredClone(sample);bad.spec.duration=0.5;
 assert.throws(()=>validateStandardAnimation(bad));
 const endpoint=structuredClone(sample);endpoint.spec.scaleFrom=0.8;
 assert.throws(()=>validateStandardAnimation(endpoint));
 const missing=structuredClone(sample);missing.clip.curves.pop();
 assert.throws(()=>validateStandardAnimation(missing));
});
