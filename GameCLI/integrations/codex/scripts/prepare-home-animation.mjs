// Read original Unity curves; all asset writes are performed by UnityCLI.
import assert from 'node:assert/strict';
import {readFile,writeFile} from 'node:fs/promises';
import {createWorkspaces} from '../server/workspaces.mjs';
import {validateDesign} from '../shared/design.mjs';
const folder='.data/imports/sv-home';
const source=JSON.parse(await readFile(folder+'/unity/unity-prefab.json','utf8'));
const text=await readFile('G:/Works/SV/Development/Project/Assets/data/UI/Home/@HomeView/Effect/Home/RootView_Anim_01.anim','utf8');
const w=createWorkspaces();
try{
 const published=await w.templateEditor.published('SvHome'),design=published.design;
 assert(!design.animations?.length,'Existing Home animation must be reviewed');
 const host=design.nodes.find(n=>n.animationHost),byPath=new Map();
 const names=new Map();for(const n of source.nodes)names.set(n.path,n.parent?names.get(n.parent)+'/'+n.name:n.name);
 for(const n of source.nodes)if(n.path==='0/6'||n.path.startsWith('0/6/'))byPath.set(names.get(n.path).replace('Home/RootView/',''),design.nodes.find(d=>d.id==='Home'+n.path.replaceAll('/','_')));
 const stop=Number(text.match(/m_StopTime: ([\d.]+)/)[1]),curves=[];
 const section=text.split('  m_FloatCurves:')[1].split('  m_PPtrCurves:')[0];
 for(const block of section.split(/\n  - curve:/).slice(1)){
  const prop=block.match(/\n    attribute: (.+)/)?.[1].trim(),path=block.match(/\n    path: (.*)/)?.[1].trim(),node=byPath.get(path);
  if(!node||(!/^m_AnchoredPosition\.[xy]$/.test(prop)&&prop!=='m_Alpha'))continue;
  const keys=[...block.matchAll(/time: ([^\r\n]+)\r?\n\s+value: ([^\r\n]+)\r?\n\s+inSlope: ([^\r\n]+)\r?\n\s+outSlope: ([^\r\n]+)/g)].map(m=>({time:Number(m[1]),value:Number(m[2]),inTangent:Number(m[3]),outTangent:Number(m[4]),inWeight:1/3,outWeight:1/3,weightedMode:0}));
  if(!keys.some(k=>k.value!==keys.at(-1).value))continue;
  assert(keys.every(k=>Object.values(k).every(Number.isFinite)));
  if(prop.startsWith('m_AnchoredPosition.')){const delta=node.rectTransform.anchoredPosition[prop.at(-1)]-keys.at(-1).value;for(const k of keys)k.value+=delta;}
  if(keys[0].time>0){keys[0].inTangent=0;keys.unshift({...keys[0],time:0,inTangent:0,outTangent:0});}
  if(keys.at(-1).time<stop)keys.push({...keys.at(-1),time:stop,inTangent:0,outTangent:0});
  curves.push({targetNodeId:node.id,component:prop==='m_Alpha'?'UnityEngine.CanvasGroup':'UnityEngine.RectTransform',property:prop,keys,preWrapMode:8,postWrapMode:8});
 }
 assert(curves.length>0);
 design.animations=[{id:'HomeEntrance',hostNodeId:host.id,source:{path:'SV/Home/RootView_Anim_01.anim',adaptation:'Retained basic-lobby position and CanvasGroup entrance curves; decorative shader and 3D loops excluded.'},state:{name:'Open',default:true,speed:1,cycleOffset:0},clip:{name:'HomeOpen',startTime:0,stopTime:stop,frameRate:30,loopTime:false,curves}}];
 validateDesign(design);
 const namesById=new Map(design.nodes.map(n=>[n.id,n]));
 function path(id){const n=namesById.get(id);return n.parent===host.id?n.name:path(n.parent)+'/'+n.name;}
 const clip={schemaVersion:'animation-clip/1',frameRate:30,loop:false,curves:curves.map(c=>({path:path(c.targetNodeId),component:c.component,property:c.property,preWrapMode:8,postWrapMode:8,keys:c.keys.map(k=>({...k,inSlope:k.inTangent,outSlope:k.outTangent,tangentMode:0}))}))};
 await writeFile(folder+'/home-open.clip.json',JSON.stringify(clip,null,2));
 await writeFile(folder+'/animation-design.json',JSON.stringify(design,null,2));
 console.log(JSON.stringify({curves:curves.length,targets:curves.map(c=>[path(c.targetNodeId),c.property]),duration:stop}));
}finally{await w.close();}
