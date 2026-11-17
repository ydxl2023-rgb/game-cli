import {readFile,writeFile} from 'node:fs/promises';
import {validateDesign} from '../shared/design.mjs';
const folder='.data/imports/sv-home/fidelity-candidate';
const payload=JSON.parse(await readFile(folder+'/template.json','utf8'));
const contract=JSON.parse(await readFile(folder+'/presentation.json','utf8'));
const design=payload.design;
const key=(time,value,input=0,output=input)=>({time,value,inTangent:input,outTangent:output,inWeight:1/3,outWeight:1/3,weightedMode:0});
const curve=(targetNodeId,component,property,keys)=>({targetNodeId,component,property,keys,preWrapMode:8,postWrapMode:8});
const entrance=design.animations.find(a=>a.id==='HomeEntrance');
entrance.clip.curves=entrance.clip.curves.filter(c=>c.targetNodeId!=='Home0_6_2');
const left=design.nodes.find(n=>n.id==='Home0_6_2'),shift=left.rectTransform.anchoredPosition.x-960;
entrance.clip.curves.push(curve(left.id,'UnityEngine.RectTransform','m_AnchoredPosition.x',[key(0,772+shift),key(.1,772+shift,0,989.685),key(.46666667,960+shift),key(1.2666667,960+shift)]),
  curve(left.id,'UnityEngine.CanvasGroup','m_Alpha',[key(0,0),key(.1,0),key(.46666667,1),key(1.2666667,1)]));
design.animations=[entrance];
for(const state of contract.states.filter(s=>/NewTag/.test(s.name))){
  const node=design.nodes.find(n=>n.id===state.node);node.animationHost=true;
  const values=[[0,1],[.26666668,1.3],[.53333336,1],[.8,1.2],[1.0666667,1],[1.3666667,1.1],[1.6,1],[3,1]];
  design.animations.push({id:node.id+'Loop',hostNodeId:node.id,state:{name:'Loop',default:true,speed:1,cycleOffset:0},
    clip:{name:'HomeNewTag',startTime:0,stopTime:3,frameRate:30,loopTime:true,curves:['x','y','z'].map(axis=>curve(node.id,'UnityEngine.Transform','m_LocalScale.'+axis,values.map(v=>key(...v))))}});
}
const drop=design.nodes.find(n=>n.id==='Home0_6_5_0_4');drop.animationHost=true;
design.animations.push({id:'HomeDropLoop',hostNodeId:drop.id,state:{name:'Loop',default:true,speed:1,cycleOffset:0},clip:{name:'HomeDrop',startTime:0,stopTime:3.9333334,frameRate:30,loopTime:true,curves:[curve(drop.id,'UnityEngine.RectTransform','m_AnchoredPosition.y',[
  key(0,10.6),key(.6666667,16.6),key(1.3333334,10.6,-9.283),key(2.0333333,7.267,0,3.658),key(2.6666667,10.6),key(3.9333334,10.6)])]}});
payload.design=validateDesign(design);
const paths=new Map();for(const n of design.nodes)paths.set(n.id,n.parent?paths.get(n.parent)+'/'+n.name:n.name);
const host=paths.get(entrance.hostNodeId);
await writeFile(folder+'/home-open.clip.json',JSON.stringify({schemaVersion:'animation-clip/1',frameRate:30,loop:false,curves:entrance.clip.curves.map(c=>({path:paths.get(c.targetNodeId).slice(host.length+1),component:c.component,property:c.property,preWrapMode:8,postWrapMode:8,keys:c.keys.map(k=>({...k,inSlope:k.inTangent,outSlope:k.outTangent,tangentMode:0}))}))},null,2));
await writeFile(folder+'/template.json',JSON.stringify(payload,null,2));
console.log(JSON.stringify({animations:design.animations.length,entranceCurves:entrance.clip.curves.length}));
