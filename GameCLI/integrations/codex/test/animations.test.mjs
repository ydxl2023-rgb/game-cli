import test from 'node:test';
import assert from 'node:assert/strict';
import {validateAnimations,sampleCurve,animationTime} from '../shared/animations.mjs';
import {validateDesign} from '../shared/design.mjs';
import {instantiateTemplate} from '../shared/templates.mjs';
const key=(time,value)=>({time,value,inTangent:-180,outTangent:-180,inWeight:1/3,outWeight:1/3,weightedMode:0});
const nodes=[{id:'Root',name:'Loading',parent:null},{id:'Host',name:'Animator',parent:'Root',animationHost:true},{id:'Ball',name:'Ball',parent:'Host'}].map(n=>({...n,type:'Container',x:0,y:0,width:128,height:128,color:'#ffffff',text:'',fontSize:20}));
const animation={id:'Spin',hostNodeId:'Host',clip:{startTime:0,stopTime:2,frameRate:30,loopTime:true,curves:[{targetNodeId:'Ball',component:'UnityEngine.Transform',property:'localEulerAnglesRaw.z',keys:[key(0,0),key(2,-360)]}]},state:{default:true,speed:1,cycleOffset:0}};
test('original rotation speed and loop boundary',()=>{for(const t of [0,.5,1,1.5,2])assert.ok(Math.abs(sampleCurve(animation.clip.curves[0].keys,t)+180*t)<1e-8);assert.equal(animationTime(animation,2.5),.5);});
test('reject missing targets, unsupported weights and nonzero 3D rotation',()=>{for(const mutate of [a=>a.clip.curves[0].targetNodeId='Missing',a=>a.clip.curves[0].keys[0].weightedMode=1,a=>a.clip.curves[0].property='localEulerAnglesRaw.x']){const a=structuredClone(animation);mutate(a);assert.throws(()=>validateAnimations([a],nodes));}});
test('save roundtrip and two instances retain distinct animation bindings',()=>{
 const source={schemaVersion:1,name:'Loading',width:1920,height:1080,nodes,animations:[animation]};
 const roundtrip=validateDesign(JSON.parse(JSON.stringify(validateDesign(source))));assert.deepEqual(roundtrip.animations,[animation]);
 const design={schemaVersion:1,name:'Stage',width:1920,height:1080,nodes:[]};
 const template={category:'prefab',payload:{root:'Root',design:source}};
 instantiateTemplate(design,template,null,{x:0,y:0});instantiateTemplate(design,template,null,{x:0,y:0});
 assert.equal(design.animations.length,2);assert.notEqual(design.animations[0].clip.curves[0].targetNodeId,design.animations[1].clip.curves[0].targetNodeId);validateDesign(design);
});
