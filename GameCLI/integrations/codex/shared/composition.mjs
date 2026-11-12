import {validateDesign} from './design.mjs';
import {instantiateTemplate} from './templates.mjs';
import {connectUi,relations} from './relations.mjs';
export function checkComposition(design){
 const d=validateDesign(design),systems=d.nodes.filter(n=>n.parent===null);
 return {systems:systems.map(n=>({id:n.id,name:n.name,kind:n.systemKind??'ui',templateId:n.templateSourceId,version:n.templateSourceVersion})),relations:relations(d),warnings:systems.filter(n=>!n.templateSourceId).map(n=>n.name+' 未关联标准模板'),valid:systems.length>0};
}
export function compose(source,operations,templates){
 const design=structuredClone(source),added={};if(!Array.isArray(operations)||operations.length>100)throw Error('每次最多 100 个编排操作。');
 const resolve=id=>added[id]??id;
 for(const op of operations){
  if(op.type==='add'){
   const t=templates.find(t=>t.id===op.templateId&&t.version===op.version);if(!t)throw Error('模板或指定版本不存在。');
   if(!op.key||added[op.key])throw Error('添加系统必须提供唯一 key。');
   const id=instantiateTemplate(design,t,null,{x:0,y:0},{allowCode:true});added[op.key]=id;design.nodes.find(n=>n.id===id).relationPosition=op.position??{x:32,y:40};
  }else if(op.type==='connect'){
   if(!['show','navigate','toggle'].includes(op.action))throw Error('关系区仅支持打开、跳转、切换。');
   connectUi(design,resolve(op.sourceId),resolve(op.targetId),op.action);
  }else if(op.type==='remove'){
   const id=resolve(op.id);if(!design.nodes.some(n=>n.id===id&&n.parent===null))throw Error('请选择系统根节点。');
   const ids=new Set([id]);let size;do{size=ids.size;for(const n of design.nodes)if(ids.has(n.parent))ids.add(n.id);}while(size!==ids.size);
   design.nodes=design.nodes.filter(n=>!ids.has(n.id));for(const n of design.nodes)if(ids.has(n.action?.target))delete n.action;
   if(design.animations)design.animations=design.animations.filter(a=>!ids.has(a.hostNodeId)).map(a=>({...a,clip:{...a.clip,curves:a.clip.curves.filter(c=>!ids.has(c.targetNodeId))}})).filter(a=>a.clip.curves.length);
  }else if(op.type==='arrange'){design.nodes.filter(n=>n.parent===null).forEach((n,i)=>n.relationPosition={x:32+i%3*310,y:40+Math.floor(i/3)*285});}
  else throw Error('未知编排操作。');
 }
 return {design:validateDesign(design),added};
}
