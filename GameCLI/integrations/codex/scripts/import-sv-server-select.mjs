// Read-only SV export to a local server selection template.
import {readFile,writeFile,mkdir} from 'node:fs/promises';
import {join} from 'node:path';
import {createHash} from 'node:crypto';
import {normalizeRect,resolveLayout} from '../shared/rect-transform.mjs';
import {readChineseLanguage} from '../server/sv-language.mjs';
const project='G:/Works/SV/Development/Project',folder='.data/imports/sv-server-select';
const source=JSON.parse(await readFile(join(folder,'unity/unity-prefab.json'),'utf8'));
if(source.sourcePath!=='Assets/data/UI/NewLogin/@NewLoginView/Prefab/ServerSelectView.prefab')throw new Error('Wrong source prefab');
const resolved=JSON.parse(await readFile(join(folder,'unity/unity-prefab.json'),'utf8'));
const languagePath=join(project,'Assets/data/@Cfg/language_cfg.bytes');
const language=await readChineseLanguage(languagePath),texts=[];
for(const n of source.nodes)for(const component of n.components){
 const value=Object.values(JSON.parse(component.json))[0]?.m_Text,match=value?.match(/^\[LID:(\d+)\]$/);
 if(match){if(!language.has(Number(match[1])))throw Error('Missing Chinese language ID: '+match[1]);texts.push({path:n.path,text:language.get(Number(match[1]))});}
}
const locale={locale:'zh-CN',source:languagePath,texts};
await writeFile(join(folder,'locale.json'),JSON.stringify(locale,null,2));
if(resolved.nodes[0].rectTransform.sizeDelta.x!==1920)throw Error('Unity layout export not evaluated');
const production=resolved.nodes,assets=new Map(),nodes=[];
const names={ServerSelectView:'ServerSelect',CloseBtn:'Close'};
const id=path=>'ServerSelect'+path.replaceAll('/','_');
const notes=[];
for(const n of production){
 const components=n.components.map(c=>({...c,data:Object.values(JSON.parse(c.json))[0]}));
 const graphic=components.find(c=>c.data?.m_Color&&('m_FontData'in c.data||'m_Sprite'in c.data||'m_Texture'in c.data));
 const button=components.find(c=>c.type.startsWith('UnityEngine.UI.Button,'));
 const g=graphic?.data,r=normalizeRect(n.rectTransform);
 const name=names[n.name.trim()]??n.name.replace(/[^a-zA-Z0-9]+(.)?/g,(_,c)=>c?c.toUpperCase():'').replace(/^./,c=>c.toUpperCase());
 const node={id:id(n.path),name,parent:n.path==='0'?null:id(n.parent),type:g?.m_FontData?'Text':button?'Button':'Container',x:0,y:0,width:1,height:1,color:g?'#'+['r','g','b'].map(k=>Math.round(g.m_Color[k]*255).toString(16).padStart(2,'0')).join(''):'#ffffff',text:(locale.texts.find(t=>t.path===n.path)?.text??g?.m_Text??'').replace(/<[^>]*>/g,''),fontSize:g?.m_FontData?.m_FontSize??20,active:n.activeSelf,rectTransform:r,localScale:{x:n.rectTransform.localScale.x,y:n.rectTransform.localScale.y},rotation:n.rectTransform.localEulerAngles.z};
 if(graphic&&!g.m_FontData&&!button&&!production.some(v=>v.parent===n.path))node.type='Image';
 if(!graphic)node.surface='transparent';

 if(button)node.interactable=button.data.m_Interactable;
 if(g){node.opacity=g.m_Color.a;if(g.m_FontData){node.textAlign=['left','center','right'][g.m_FontData.m_Alignment%3];node.verticalAlign=['start','center','end'][Math.floor(g.m_FontData.m_Alignment/3)];}}
 const ref=graphic?.references.find(r=>['m_Sprite','m_Texture'].includes(r.property)&&r.path);
 if(ref&&ref.path!=='Resources/unity_builtin_extra'){
  if(!ref.path.endsWith('.png'))throw new Error('Unsupported image: '+ref.path);
  const body=await readFile(join(project,ref.path)),hash=createHash('sha256').update(body).digest('hex');
  assets.set(hash,{body,path:ref.path});node.image=hash;node.imageFit=g.m_PreserveAspect?'contain':'100% 100%';
  if(g.m_Type===1){const meta=await readFile(join(project,ref.path+'.meta'),'utf8');const border=meta.match(/spriteBorder: \{x: ([\d.]+), y: ([\d.]+), z: ([\d.]+), w: ([\d.]+)\}/);if(!border)throw new Error('Missing sprite border: '+ref.path);node.imageBorder=Object.fromEntries(['x','y','z','w'].map((k,i)=>[k,Number(border[i+1])]));node.imageType='sliced';}
  if(g.m_UVRect)node.uvRect=Object.fromEntries(['x','y','width','height'].map(k=>[k,g.m_UVRect[k]]));
 }
 if(n.path==='0'){node.localScale={x:1,y:1};node.rectTransform={anchorMin:{x:0,y:0},anchorMax:{x:1,y:1},pivot:{x:.5,y:.5},anchoredPosition:{x:0,y:0},sizeDelta:{x:0,y:0}};}
 if(ref?.path==='Resources/unity_builtin_extra')notes.push({node:id(n.path),reason:'Unity built-in sprite retained in native evidence; Studio uses the original graphic tint.',fileId:ref.fileId});
 if(graphic&&g.m_Enabled===false){node.surface='transparent';delete node.image;delete node.imageBorder;delete node.imageType;if(node.type==='Text')node.text='';notes.push({node:node.id,reason:'Disabled source graphic omitted visually; original component retained in native evidence.'});}
 if(graphic&&g.m_Enabled===false)node.opacity=1;
 const mask=components.find(c=>c.type.startsWith('UnityEngine.UI.Mask,'));
 if(mask?.data.m_ShowMaskGraphic===false){node.surface='transparent';delete node.image;delete node.imageBorder;delete node.imageType;notes.push({node:node.id,reason:'Mask graphic is hidden in the source; descendants remain visible.'});}
 // UGUI Graphic alpha never fades descendants. Solid parent graphics become explicit background layers.
 const descendants=production.some(child=>child.parent===n.path);
 if(descendants&&node.opacity<1&&!node.image){
  const alpha=node.opacity;node.opacity=1;node.surface='transparent';nodes.push(node);
  if(alpha>0&&g.m_Enabled!==false)nodes.push({id:node.id+'Surface',name:'Background',parent:node.id,type:'Image',x:0,y:0,width:1,height:1,color:node.color,text:'',fontSize:20,opacity:alpha,rectTransform:{anchorMin:{x:0,y:0},anchorMax:{x:1,y:1},pivot:{x:.5,y:.5},anchoredPosition:{x:0,y:0},sizeDelta:{x:0,y:0}}});
 }else nodes.push(node);
}

// Preserve the SV visual hierarchy; runtime lists populate the two original cells.
const payload={root:id('0'),design:resolveLayout({schemaVersion:1,name:'ServerSelect',width:1920,height:1080,nodes}),source:{path:source.sourcePath,guid:source.sourceGuid}};
for(const node of payload.design.nodes){node.width=Math.max(1,node.width);node.height=Math.max(1,node.height);}
await mkdir(join(folder,'assets'),{recursive:true});
const assetNames={};
for(const [hash,a]of assets){await writeFile(join(folder,'assets',hash+'.png'),a.body);assetNames[hash]=a.path;}
await writeFile(join(folder,'assets','asset-names.json'),JSON.stringify(assetNames,null,2));
await writeFile(join(folder,'template.json'),JSON.stringify(payload,null,2));
console.log(JSON.stringify({nodes:nodes.length,images:assets.size}));
