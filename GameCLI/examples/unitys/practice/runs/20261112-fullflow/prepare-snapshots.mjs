import {readFile,writeFile} from 'node:fs/promises';
import {fileURLToPath} from 'node:url';
import {join} from 'node:path';
import {nodeName} from '../../../../../integrations/codex/shared/design.mjs';
const root=fileURLToPath(new URL('./inputs/',import.meta.url));
for(const name of ['Launch','Login','Notice','Loading']){
 const directory=join(root,name),d=JSON.parse(await readFile(join(directory,'source-design.json'),'utf8'));
 const manifest=JSON.parse(await readFile(join(directory,'asset-manifest.json'),'utf8'));
 const visualTextures=[];
 for(const a of manifest.assets){const b=await readFile(join(directory,a.file));const users=d.nodes.filter(n=>n.image===a.id);visualTextures.push({path:a.path,deliverySha256:a.id,deliveryWidth:b.readUInt32BE(16),deliveryHeight:b.readUInt32BE(20),settings:{spriteMode:1,spritePixelsToUnits:100,spriteBorder:users.find(n=>n.imageBorder)?.imageBorder??{x:0,y:0,z:0,w:0},wrapMode:users.some(n=>n.uvRect)?0:1}});}
 const nodes=d.nodes.map(n=>{
  const components=[],graphic=n.type==='Text'?'UnityEngine.UI.Text':n.uvRect?'UnityEngine.UI.RawImage':n.image||n.type!=='Container'||n.surface!=='transparent'?'UnityEngine.UI.Image':null;
  const hex=n.color.replace('#','');const rgba=[0,2,4].map(i=>parseInt(hex.slice(i,i+2),16)/255);rgba.push((hex.length===8?parseInt(hex.slice(6,8),16)/255:1)*(n.opacity??1));
  if(n.type==='Button'&&!n.image&&n.surface==='transparent')rgba[3]=0;
  if(graphic){
   components.push({id:'renderer',type:'UnityEngine.CanvasRenderer',properties:{}});
   const p={'m_Color.r':rgba[0],'m_Color.g':rgba[1],'m_Color.b':rgba[2],'m_Color.a':rgba[3],m_Enabled:true,m_RaycastTarget:n.type==='Button'};
   if(n.type==='Text')Object.assign(p,{m_Text:n.text??'','m_FontData.m_FontSize':Math.round(n.fontSize??20),'m_FontData.m_Alignment':({top:0,start:0,center:1,bottom:2,end:2}[n.verticalAlign??'center']*3)+({left:0,center:1,right:2}[n.textAlign??'left'])});
   if(n.image)p[n.uvRect?'m_Texture':'m_Sprite']={assetPath:manifest.assets.find(a=>a.id===n.image).path};
   if(n.imageBorder&&!n.uvRect)p.m_Type=1;
   if(n.uvRect)Object.assign(p,{'m_UVRect.x':n.uvRect.x,'m_UVRect.y':n.uvRect.y,'m_UVRect.width':n.uvRect.width,'m_UVRect.height':n.uvRect.height});
   components.push({id:'graphic',type:graphic,properties:p});
  }
  if(n.type==='Button')components.push({id:'button',type:'UnityEngine.UI.Button',properties:{m_Enabled:true,m_Interactable:n.interactable!==false,m_TargetGraphic:{nodeId:n.id,componentType:graphic}}});
  const angle=(n.rotation??0)*Math.PI/360;
  return {id:n.id,name:nodeName(n),parentId:n.parent,siblingIndex:d.nodes.filter(x=>x.parent===n.parent).indexOf(n),kind:'Node',active:n.active!==false,
   rect:Object.fromEntries(Object.entries(n.rectTransform).map(([k,v])=>[k,[v.x,v.y]])),localPosition:[0,0,0],localScale:[n.localScale?.x??1,n.localScale?.y??1,1],localRotation:[0,0,Math.sin(angle),Math.cos(angle)],components};
 });
 await writeFile(join(directory,name+'.snapshot.json'),JSON.stringify({schemaVersion:'ui-template/1',source:{template:name,note:'Derived from frozen database layout, not a Unity acceptance baseline.'},visualTextures,resources:{},prefab:{name,rootNodeId:d.nodes.find(n=>n.parent===null).id,nodes}},null,2));
 console.log(name+': snapshot prepared; specialized controls, script references and animations require module developer completion.');
}
