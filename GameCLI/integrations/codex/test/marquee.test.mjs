import test from 'node:test';
import assert from 'node:assert/strict';
import {selectionRect,intersectsSelection} from '../shared/marquee.mjs';
test('marquee works in either drag direction and includes partially intersecting cards',()=>{
 const box=selectionRect({x:300,y:400},{x:100,y:80});
 assert.deepEqual(box,selectionRect({x:100,y:80},{x:300,y:400}));
 assert.equal(intersectsSelection(box,{x:120,y:100,width:100,height:100}),true);
 assert.equal(intersectsSelection(box,{x:290,y:390,width:100,height:100}),true);
 assert.equal(intersectsSelection(box,{x:301,y:100,width:100,height:100}),false);
 assert.equal(intersectsSelection(box,{x:300,y:100,width:100,height:100}),false);
 assert.equal(intersectsSelection(selectionRect({x:100,y:100},{x:100,y:100}),{x:90,y:90,width:100,height:100}),false);
});
