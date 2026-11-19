import test from 'node:test';
import assert from 'node:assert/strict';
import {validateAnimations,sampleCurve,animationTime} from '../shared/animations.mjs';
import {validateDesign} from '../shared/design.mjs';
import {instantiateTemplate} from '../shared/templates.mjs';
import {playAnimations} from '../ui/animation-player.mjs';
const key=(time,value)=>({time,value,inTangent:-180,outTangent:-180,inWeight:1/3,outWeight:1/3,weightedMode:0});
const nodes=[{id:'Root',name:'Loading',parent:null},{id:'Host',name:'Animator',parent:'Root',animationHost:true},{id:'Ball',name:'Ball',parent:'Host'}].map(n=>({...n,type:'Container',x:0,y:0,width:128,height:128,color:'#ffffff',text:'',fontSize:20}));
const animation={id:'Spin',hostNodeId:'Host',clip:{startTime:0,stopTime:2,frameRate:30,loopTime:true,curves:[{targetNodeId:'Ball',component:'UnityEngine.Transform',property:'localEulerAnglesRaw.z',keys:[key(0,0),key(2,-360)]}]},state:{default:true,speed:1,cycleOffset:0}};

test('a multi-system project supports 65 animation hosts while retaining a bounded limit',()=>{
 const allNodes=[],allAnimations=[];
 for(let i=0;i<257;i++){
  allNodes.push(...nodes.map(n=>({...n,id:n.id+i,parent:n.parent===null?null:n.parent+i})));
  const a=structuredClone(animation);a.id+=i;a.hostNodeId+=i;a.clip.curves[0].targetNodeId+=i;allAnimations.push(a);
 }
 assert.equal(validateAnimations(allAnimations.slice(0,65),allNodes).length,65);
 assert.equal(validateAnimations(allAnimations.slice(0,256),allNodes).length,256);
 assert.throws(()=>validateAnimations(allAnimations,allNodes));
});

test('Home entrance translates from anchored positions and restores opacity',t=>{
 const targetNodes=structuredClone(nodes);targetNodes[2].rectTransform={anchoredPosition:{x:20,y:30}};
 const a=structuredClone(animation);a.clip.loopTime=false;
 a.clip.curves=[['UnityEngine.RectTransform','m_AnchoredPosition.y',130,30],['UnityEngine.CanvasGroup','m_Alpha',0,1]].map(([component,property,from,to])=>({targetNodeId:'Ball',component,property,keys:[key(0,from),key(2,to)].map(k=>({...k,inTangent:0,outTangent:0}))}));
 validateAnimations([a],targetNodes);
 const el={dataset:{node:'Ball'},style:{transform:'original',backgroundPosition:'',opacity:'0.8'}};let frame;
 const previous={requestAnimationFrame:globalThis.requestAnimationFrame,cancelAnimationFrame:globalThis.cancelAnimationFrame};t.after(()=>Object.assign(globalThis,previous));
 globalThis.requestAnimationFrame=f=>{frame=f;return 1;};globalThis.cancelAnimationFrame=()=>{};
 const stop=playAnimations({nodes:targetNodes,animations:[a]},{querySelectorAll:()=>[el]});
 assert.match(el.style.transform,/translate\(0px,-100px\)/);assert.equal(el.style.opacity,'0');
 frame(performance.now()+3000);assert.equal(el.style.opacity,'1');assert.doesNotMatch(el.style.transform,/translate/);
 stop();assert.equal(el.style.transform,'original');assert.equal(el.style.opacity,'0.8');
});
test('original rotation speed and loop boundary',()=>{for(const t of [0,.5,1,1.5,2])assert.ok(Math.abs(sampleCurve(animation.clip.curves[0].keys,t)+180*t)<1e-8);assert.equal(animationTime(animation,2.5),.5);});
test('scale and rotation compose during playback and restore on stop',t=>{
 const a=structuredClone(animation);a.clip.curves.push(...['x','y','z'].map(axis=>({targetNodeId:'Ball',component:'UnityEngine.Transform',property:'m_LocalScale.'+axis,keys:[key(0,.95),key(2,.95)].map(k=>({...k,inTangent:0,outTangent:0}))})));
 validateAnimations([a],nodes);
 const element={dataset:{node:'Ball'},style:{transform:'original',backgroundPosition:'0px 0px'}};let callback;
 const previous={requestAnimationFrame:globalThis.requestAnimationFrame,cancelAnimationFrame:globalThis.cancelAnimationFrame};t.after(()=>Object.assign(globalThis,previous));globalThis.requestAnimationFrame=fn=>{callback=fn;return 1;};
 globalThis.cancelAnimationFrame=()=>{};
 const stop=playAnimations({nodes,animations:[a]},{querySelectorAll:()=>[element]});
 assert.match(element.style.transform,/scale\(0.95,0.95\)/);callback(performance.now()+500);
 const values=element.style.transform.match(/rotate\(([\d.]+)deg\) scale\(([\d.]+),([\d.]+)\)/);assert.ok(values);assert.ok(Number(values[1])>80);assert.ok(Math.abs(Number(values[2])-.95)<1e-10);assert.ok(Math.abs(Number(values[3])-.95)<1e-10);stop();assert.equal(element.style.transform,'original');
});
test('reject missing targets, unsupported weights and nonzero 3D rotation',()=>{for(const mutate of [a=>a.clip.curves[0].targetNodeId='Missing',a=>a.clip.curves[0].keys[0].weightedMode=1,a=>a.clip.curves[0].property='localEulerAnglesRaw.x']){const a=structuredClone(animation);mutate(a);assert.throws(()=>validateAnimations([a],nodes));}});
test('save roundtrip and two instances retain distinct animation bindings',()=>{
 const source={schemaVersion:1,name:'Loading',width:1920,height:1080,nodes,animations:[animation]};
 const roundtrip=validateDesign(JSON.parse(JSON.stringify(validateDesign(source))));assert.deepEqual(roundtrip.animations,[animation]);
 const design={schemaVersion:1,name:'Stage',width:1920,height:1080,nodes:[]};
 const template={category:'prefab',payload:{root:'Root',design:source}};
 instantiateTemplate(design,template,null,{x:0,y:0});instantiateTemplate(design,template,null,{x:0,y:0});
 assert.equal(design.animations.length,2);assert.notEqual(design.animations[0].clip.curves[0].targetNodeId,design.animations[1].clip.curves[0].targetNodeId);validateDesign(design);
});
