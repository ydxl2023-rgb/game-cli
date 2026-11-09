import test from 'node:test';import assert from 'node:assert/strict';
import {groupLoadingFooter} from '../shared/loading-hierarchy.mjs';
import {resolveLayout} from '../shared/rect-transform.mjs';
test('Loading footer reparent preserves geometry and is idempotent',()=>{
 const d={width:1920,height:1080,nodes:[{id:'root',name:'Loading',parent:null,x:0,y:0,width:1920,height:1080},{id:'bg',name:'RateBg',parent:'root',x:-260,y:984,width:2440,height:100},...['TipText','Ball','Haikyu','Rate'].map((name,i)=>({id:name,name,parent:'root',x:28+i*100,y:1002,width:64,height:64,localScale:{x:.8,y:.8}})),{id:'arrow',name:'Arrow',parent:'bg',x:964,y:-22,width:512,height:152}]};
 resolveLayout(d);const old=structuredClone(d);assert.equal(groupLoadingFooter(d),4);
 for(const n of d.nodes.filter(n=>['TipText','Ball','Haikyu','Rate'].includes(n.name))){const previous=old.nodes.find(v=>v.id===n.id);assert.equal(n.parent,'bg');assert.equal(n.x-260,previous.x);assert.equal(n.y+984,previous.y);assert.equal(n.width,previous.width);assert.deepEqual(n.localScale,previous.localScale);}
 assert.equal(groupLoadingFooter(d),0);assert.equal(d.nodes.find(n=>n.id==='arrow').parent,'bg');
});
