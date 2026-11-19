// Adapt default source Animator clips to the standard's reviewed hierarchy.
import assert from 'node:assert/strict';
import {readFile,writeFile} from 'node:fs/promises';
import {validateDesign} from '../shared/design.mjs';
const project=process.env.SV_PROJECT??'G:/Works/SV/Development/Project';
const folder='.data/imports/sv-settings';
const inventory=[];
for(const key of ['main']){
 const payload=JSON.parse(await readFile(`${folder}/${key}/template.json`,'utf8'));
 assert(!payload.design.animations?.length,'Re-extract before regenerating animations');
 const source=JSON.parse(await readFile(`${folder}/source/${key}/unity-prefab.json`,'utf8'));
 const contract=JSON.parse(await readFile(`${folder}/${key}/contract.json`,'utf8'));
 const map=new Map(contract.mapping.map(n=>[n.path,payload.design.nodes.find(d=>d.id===n.id)]));
 const sourceMap=new Map(source.nodes.map(n=>[n.path,n])),names=new Map();
 for(const n of source.nodes)names.set(n.path,n.parent?names.get(n.parent)+'/'+n.name:n.name);
 const groups=new Map(),notes=[];
 for(const origin of source.nodes){
  if(!map.has(origin.path))continue;
  let visible=true,parent=origin;
  while(parent){visible&&=parent.activeSelf;parent=sourceMap.get(parent.parent);}
  if(!visible)continue;
  const animator=origin.components.find(c=>c.type.startsWith('UnityEngine.Animator,'));
  const controller=animator?.references.find(r=>r.property==='m_Controller');
  if(!controller?.path)continue;
  const controllerText=await readFile(`${project}/${controller.path}`,'utf8');
  const state=controllerText.match(/m_DefaultState: \{fileID: (-?\d+)\}/)?.[1];
  const stateText=controllerText.split(`--- !u!1102 &${state}`)[1]?.split('--- !u!')[0];
  const motion=stateText?.match(/m_Motion: \{fileID: [^,]+, guid: ([a-f0-9]+)/)?.[1];
  const dependency=source.dependencies.find(d=>d.guid===motion);
  if(!dependency){notes.push({path:origin.path,controller:controller.path,reason:'No default clip resolved'});continue;}
  const text=await readFile(`${project}/${dependency.path}`,'utf8');
  const stop=Number(text.match(/m_StopTime: ([\d.]+)/)?.[1]);
  const loop=text.match(/m_LoopTime: (\d)/)?.[1]==='1';
  let host=map.get(origin.path);
  if(origin.path==='0')host=payload.design.nodes.find(n=>n.parent===null);
  else while(host&&host.type!=='Container')host=payload.design.nodes.find(n=>n.id===host.parent);
  assert(host?.type==='Container','Animator needs an unambiguous container host');
  host.animationHost=true;
  const curves=[];
  const floatSection=text.split('  m_FloatCurves:')[1]?.split('  m_PPtrCurves:')[0]??'';
  function target(path){return map.get(source.nodes.find(n=>names.get(n.path)===names.get(origin.path)+(path?'/'+path:''))?.path);}
  function add(node,property,component,keys){
   if(!node||!keys.length||!keys.some(k=>k.value!==keys.at(-1).value))return;
   assert(keys.every(k=>Object.values(k).every(Number.isFinite)),'Unsupported weighted or stepped curve');
   if(property.startsWith('m_AnchoredPosition.')){const delta=node.rectTransform.anchoredPosition[property.at(-1)]-keys.at(-1).value;for(const k of keys)k.value+=delta;}
   if(keys[0].time>0){keys[0].inTangent=0;keys.unshift({...keys[0],time:0,inTangent:0,outTangent:0});}
   if(keys.at(-1).time<stop)keys.push({...keys.at(-1),time:stop,inTangent:0,outTangent:0});
   curves.push({targetNodeId:node.id,property,component,keys,preWrapMode:8,postWrapMode:8});
  }
  for(const block of floatSection.split(/\n  - curve:/).slice(1)){
   let property=block.match(/\n    attribute: ([^\r\n]+)/)?.[1].trim();
   const path=block.match(/\n    path: ([^\r\n]*)/)?.[1].trim()??'',node=target(path);
   if(!node)continue;
   if(property==='m_Interactable'){notes.push({path:origin.path,property,reason:'Restore the original 0.5 second interaction gate in UISettings.'});continue;}
   let component=property==='m_Alpha'?'UnityEngine.CanvasGroup':/^m_AnchoredPosition\.[xy]$/.test(property)?'UnityEngine.RectTransform':/^m_LocalScale\.[xyz]$/.test(property)?'UnityEngine.Transform':null;
   if(property==='m_Color.a'&&!payload.design.nodes.some(n=>n.parent===node.id)){
    property='m_Alpha';component='UnityEngine.CanvasGroup';node.opacity=1;
   }
   if(!component){notes.push({path:origin.path,property,target:path,reason:'Unsupported source binding; review before publication'});continue;}
   const keys=[...block.matchAll(/time: ([^\r\n]+)\r?\n\s+value: ([^\r\n]+)\r?\n\s+inSlope: ([^\r\n]+)\r?\n\s+outSlope: ([^\r\n]+)/g)].map(m=>({time:Number(m[1]),value:Number(m[2]),inTangent:Number(m[3]),outTangent:Number(m[4]),inWeight:1/3,outWeight:1/3,weightedMode:0}));
   add(node,property,component,keys);
  }
  const scaleSection=text.split('  m_ScaleCurves:')[1]?.split('  m_FloatCurves:')[0]??'';
  for(const block of scaleSection.split(/\n  - curve:/).slice(1)){
   const node=target(block.match(/\n    path: ([^\r\n]*)/)?.[1].trim()??'');
   const vector=value=>Object.fromEntries([...value.matchAll(/([xyz]): ([^,}]+)/g)].map(m=>[m[1],Number(m[2])]));
   const values=[...block.matchAll(/time: ([^\r\n]+)\r?\n\s+value: (\{[^}]+\})\r?\n\s+inSlope: (\{[^}]+\})\r?\n\s+outSlope: (\{[^}]+\})/g)];
   for(const axis of ['x','y','z'])add(node,'m_LocalScale.'+axis,'UnityEngine.Transform',values.map(m=>({time:Number(m[1]),value:vector(m[2])[axis],inTangent:vector(m[3])[axis],outTangent:vector(m[4])[axis],inWeight:1/3,outWeight:1/3,weightedMode:0})));
  }
  if(curves.length){
   const group=groups.get(host.id)??{host,stop:0,loop,curves:[],sources:[]};
   assert(group.loop===loop,'Do not merge looping and entrance clips');
   group.stop=Math.max(group.stop,stop);group.curves.push(...curves);group.sources.push(dependency.path);groups.set(host.id,group);
  }
 }
 payload.design.animations=[...groups.values()].map((group,i)=>{
  for(const curve of group.curves)if(curve.keys.at(-1).time<group.stop)curve.keys.push({...curve.keys.at(-1),time:group.stop,inTangent:0,outTangent:0});
  return {id:`${pagesName(key)}Entrance${i}`,hostNodeId:group.host.id,source:{paths:group.sources},state:{name:'Open',default:true,speed:1,cycleOffset:0},clip:{name:`${pagesName(key)}Open${i}`,startTime:0,stopTime:group.stop,frameRate:30,loopTime:group.loop,curves:group.curves}};
 });
 validateDesign(payload.design);
 for(const n of payload.design.nodes){n.width=Math.max(1,n.width);n.height=Math.max(1,n.height);}
 await writeFile(`${folder}/${key}/template.json`,JSON.stringify(payload,null,2));
 inventory.push({page:key,animations:payload.design.animations.map(a=>({id:a.id,curves:a.clip.curves.length,source:a.source})),notes});
}
function pagesName(key){return {main:'Settings'}[key];}
await writeFile(`${folder}/animation-inventory.json`,JSON.stringify(inventory,null,2));
console.log(JSON.stringify(inventory));

