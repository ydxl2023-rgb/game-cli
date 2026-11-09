import test from 'node:test';import assert from 'node:assert/strict';
import {restoreLoadingBackground} from '../shared/loading-background.mjs';
import {resolveLayout} from '../shared/rect-transform.mjs';
test('Loading background restores runtime image without overwriting edited nodes',()=>{
 const d=resolveLayout({width:1920,height:1080,nodes:[{id:'a',name:'Loading',parent:null,x:0,y:0,width:1920,height:1080},{id:'b',name:'Root',parent:'a',x:0,y:0,width:1920,height:1080},{id:'c',name:'Bg',parent:'b',x:0,y:0,width:1920,height:1080},{id:'d',name:'RateBg',parent:'a',x:0,y:984,width:1920,height:100,text:'User edit'}]});
 const before=structuredClone(d.nodes),source={rectTransform:{anchorMin:{x:.5,y:.5},anchorMax:{x:.5,y:.5},pivot:{x:.5,y:.5},anchoredPosition:{x:0,y:0},sizeDelta:{x:2440,y:1080}}};
 assert.equal(restoreLoadingBackground(d,source,'a'.repeat(64)),1);assert.deepEqual(d.nodes.slice(0,4),before);const n=d.nodes.at(-1);assert.equal(n.parent,'c');assert.equal(n.x,-260);assert.equal(n.active,true);assert.equal(n.image,'a'.repeat(64));assert.equal(restoreLoadingBackground(d,source,'b'.repeat(64)),0);assert.equal(n.image,'a'.repeat(64));
});
