// Extract the approved SV RechargeMain source; never write Unity assets directly.
import assert from 'node:assert/strict';
import {readFile,writeFile,mkdir} from 'node:fs/promises';
import {join} from 'node:path';
import {createHash} from 'node:crypto';
import {execFileSync} from 'node:child_process';
import {normalizeRect,resolveLayout} from '../shared/rect-transform.mjs';
import {validateTemplate} from '../shared/templates.mjs';
import {readChineseLanguage} from '../server/sv-language.mjs';

const project=process.env.SV_PROJECT??'G:/Works/SV/Development/Project';
const folder='.data/imports/sv-settings';
const language=await readChineseLanguage(join(project,'Assets/data/@Cfg/language_cfg.bytes'));
const hash=bytes=>createHash('sha256').update(bytes).digest('hex');
const pages={main:'Settings'};
const assets=new Map(),contracts=[];
for(const [key,name] of Object.entries(pages)){
 const bytes=await readFile(`${folder}/source/${key}/unity-prefab.json`),source=JSON.parse(bytes);
 const nodes=[],mapping=[],notes=[],sourceNames=new Map();
 for(const n of source.nodes)sourceNames.set(n.path,n.parent?sourceNames.get(n.parent)+'/'+n.name:'');
 // Authoring libraries and inactive particle branches remain in the source archive.
 const excluded=source.nodes.filter(n=>['Design','TxKefu','RiceRollSet','PowerSet','WeekendSet','HQMatchSet','GridPushMessage'].includes(n.name)||n.path==='0/3/0/2'||n.components.some(c=>/ParticleSystem|MeshRenderer/.test(c.type))).map(n=>n.path);
 const included=source.nodes.filter(n=>n.rectTransform&&!excluded.some(p=>n.path===p||n.path.startsWith(p+'/')));
 const id=path=>name+path.replaceAll('/','_');
 for(const n of included){
  const components=n.components.filter(c=>c.json).map(c=>({...c,data:Object.values(JSON.parse(c.json))[0]}));
  const graphic=components.find(c=>c.data?.m_Color&&('m_FontData'in c.data||'m_Sprite'in c.data||'m_Texture'in c.data));
  const g=graphic?.data,button=components.find(c=>c.type.startsWith('UnityEngine.UI.Button,'));
  const match=g?.m_Text?.match(/^\[LID:(\d+)\]$/);
  if(match)assert(language.has(Number(match[1])),'Missing language '+match[1]);
  const text=match?language.get(Number(match[1])):g?.m_Text??'';
  const node={id:id(n.path),name:n.path==='0'?name:n.name.replace(/[^a-zA-Z0-9]+(.)?/g,(_,c)=>c?.toUpperCase()??'')||'Node',parent:n.parent?id(n.parent):null,type:g?.m_FontData?'Text':button?'Button':graphic?'Image':'Container',x:0,y:0,width:1,height:1,color:g?'#'+['r','g','b'].map(k=>Math.round(g.m_Color[k]*255).toString(16).padStart(2,'0')).join(''):'#ffffff',text,fontSize:g?.m_FontData?.m_FontSize??20,active:n.activeSelf,rectTransform:normalizeRect(n.rectTransform),localScale:{x:n.rectTransform.localScale.x,y:n.rectTransform.localScale.y},rotation:n.rectTransform.localEulerAngles.z};
  if(!graphic)node.surface='transparent';
  if(button){node.interactable=true;node.action={type:'none'};}
  if(g){node.opacity=g.m_Color.a;if(g.m_FontData){node.textAlign=['left','center','right'][g.m_FontData.m_Alignment%3];node.verticalAlign=['start','center','end'][Math.floor(g.m_FontData.m_Alignment/3)];}}
  const ref=graphic?.references.find(r=>['m_Sprite','m_Texture'].includes(r.property)&&r.path);
  if(ref&&ref.path!=='Resources/unity_builtin_extra'){
   assert(/\.(png|jpg|jpeg)$/i.test(ref.path),'Unsupported graphic '+ref.path);
   const original=await readFile(join(project,ref.path));
   const imageBytes=ref.path.endsWith('.png')?original:execFileSync('python',['-c','import sys,io;from PIL import Image;Image.open(io.BytesIO(sys.stdin.buffer.read())).save(sys.stdout.buffer,format="PNG")'],{input:original,maxBuffer:32*1024*1024});
   const assetId=hash(imageBytes);
   assets.set(assetId,{body:imageBytes,path:ref.path});node.image=assetId;node.imageFit=g.m_PreserveAspect?'contain':'100% 100%';
   if(g.m_Type===1){const meta=await readFile(join(project,ref.path+'.meta'),'utf8');const border=meta.match(/spriteBorder: \{x: ([\d.]+), y: ([\d.]+), z: ([\d.]+), w: ([\d.]+)\}/);assert(border);node.imageBorder=Object.fromEntries(['x','y','z','w'].map((k,i)=>[k,Number(border[i+1])]));node.imageType='sliced';}
   if(g.m_UVRect)node.uvRect=g.m_UVRect;
  }
  if(components.some(c=>c.type.startsWith('UnityEngine.UI.Mask,')&&c.data.m_ShowMaskGraphic===false)||g?.m_Enabled===false){node.surface='transparent';delete node.image;delete node.imageBorder;delete node.imageType;node.opacity=1;}
  if(n.path==='0'){node.localScale={x:1,y:1};node.rectTransform={anchorMin:{x:0,y:0},anchorMax:{x:1,y:1},pivot:{x:.5,y:.5},anchoredPosition:{x:0,y:0},sizeDelta:{x:0,y:0}};}
  // A Graphic's alpha affects its own pixels, never its children.
  if(included.some(child=>child.parent===n.path)&&node.opacity<1&&!node.image){const alpha=node.opacity;node.opacity=1;node.surface='transparent';nodes.push(node);if(alpha>0)nodes.push({id:node.id+'Surface',name:'Surface',parent:node.id,type:'Image',x:0,y:0,width:1,height:1,color:node.color,text:'',fontSize:20,opacity:alpha,rectTransform:{anchorMin:{x:0,y:0},anchorMax:{x:1,y:1},pivot:{x:.5,y:.5},anchoredPosition:{x:0,y:0},sizeDelta:{x:0,y:0}}});}else nodes.push(node);
  mapping.push({id:node.id,path:n.path,sourceName:n.name,sourceHierarchy:sourceNames.get(n.path),type:node.type,text,active:n.activeSelf});
 }
 const design=resolveLayout({schemaVersion:1,name,width:1920,height:1080,nodes});
 for(const n of design.nodes){n.width=Math.max(1,n.width);n.height=Math.max(1,n.height);}
 const payload=validateTemplate({root:id('0'),design},'system');
 for(const n of payload.design.nodes){n.width=Math.max(1,n.width);n.height=Math.max(1,n.height);}
 const contract={key,name,sourcePath:source.sourcePath,sourceGuid:source.sourceGuid,exportSha256:hash(bytes),sourceNodes:source.nodes.length,nodes:payload.design.nodes.length,mapping,excluded,notes,animationDependencies:source.dependencies.filter(d=>/\.(anim|controller)$/.test(d.path))};
 await mkdir(`${folder}/${key}`,{recursive:true});
 await writeFile(`${folder}/${key}/template.json`,JSON.stringify(payload,null,2));
 await writeFile(`${folder}/${key}/contract.json`,JSON.stringify(contract,null,2));
 contracts.push(contract);
}
await mkdir(`${folder}/assets`,{recursive:true});
for(const [id,asset]of assets)await writeFile(`${folder}/assets/${id}.png`,asset.body);
await writeFile(`${folder}/assets/asset-names.json`,JSON.stringify(Object.fromEntries([...assets].map(([id,a])=>[id,a.path])),null,2));
await writeFile(`${folder}/source-inventory.json`,JSON.stringify({status:'extracted-not-published',scope:'SV settings with local preferences and explicit product integration events',pages:contracts.map(({mapping,...c})=>c),images:assets.size},null,2));
console.log(JSON.stringify({pages:contracts.map(c=>({name:c.name,nodes:c.nodes})),images:assets.size}));

