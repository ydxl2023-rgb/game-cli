import {remapAnimations} from './animations.mjs';
import {initialTemplateGroups} from './template-groups.mjs';
import {ensureAnimationHost} from './animation-host.mjs';
import {captureBounds} from './rect-transform.mjs';
import {appendControl,validateDesign,resizeNode,layoutScrollPanel} from './design.mjs';

export const categories=[{id:'prefab',name:'预制控件',sort:10},{id:'system',name:'系统模板',sort:20}];
export function builtInTemplates() {
  return [
    ['ItemSlot','prefab','道具框','128 × 128 · 5 层'],
    ['ScrollPanel','prefab','道具滚动面板','连续网格 · 道具框'],
    ['ConfirmDialog','prefab','确认对话框','遮罩 · 标题正文 · 双按钮'],
    ['InventorySystem','system','背包系统','完整界面 · 道具列表 · 确认弹窗'],
  ].map(([id,category,name,description],index)=>{
    const design={schemaVersion:1,name:'TemplateSource',width:1080,height:1920,nodes:[]};
    const root=appendControl(design,id,null,{x:540,y:960});
    if(category==='system')ensureAnimationHost(design,root);
    return {id,category,group:initialTemplateGroups[id]??'general',name,description,sort:index*10,version:1,payload:{root,design:validateDesign(design)}};
  });
}
export function validateTemplate(payload, category) {
  const design=validateDesign(payload?.design), root=design.nodes.find(n=>n.id===payload.root);
  if (!root || root.parent!==null || design.nodes.filter(n=>n.parent===null).length!==1) throw new Error('模板必须有且仅有一个根节点。');
  if(category==='system')ensureAnimationHost(design,root.id);
  return {root:root.id,design:validateDesign(design)};
}
export function instantiateTemplate(design, template, parent, point) {
  const source=validateTemplate(template.payload,template.category), nodes=structuredClone(source.design.nodes);
  if(design.nodes.length+nodes.length>300) throw new Error('添加模板后超出 300 个节点上限。');
  const root=nodes.find(n=>n.id===source.root);
  const canvasRoot=template.category==='system' || root.template==='ConfirmDialog';
  const container=design.nodes.find(n=>n.id===parent);
  if(!canvasRoot && parent!==null && container?.type!=='Container') throw new Error('目标必须为容器。');
  const ids=new Set([design.name,...design.nodes.map(n=>n.id)]);
  let i=1;
  while(nodes.some(n=>ids.has(`T${i}${n.id}`))) i++;
  const remap=new Map(nodes.map(n=>[n.id,`T${i}${n.id}`]));
  for(const n of nodes) {
    n.id=remap.get(n.id); n.parent=n.parent===null ? (canvasRoot?null:parent) : remap.get(n.parent);
    if(n.action?.target) n.action.target=remap.get(n.action.target);
  }
  const dimensions=template.category==='system' ? {width:source.design.width,height:source.design.height} : {width:design.width,height:design.height};
  const result={...design,...dimensions,nodes:[...design.nodes,...nodes]};
  if(source.design.animations?.length)result.animations=[...(design.animations??[]),...remapAnimations(source.design.animations,remap,`T${i}`)];
  if(canvasRoot) {
    const sx=result.width/source.design.width, sy=result.height/source.design.height;
    for(const n of nodes) {n.x*=sx;n.y*=sy;n.width=Math.max(1,n.width*sx);n.height=Math.max(1,n.height*sy);}
    root.x=0;root.y=0;resizeNode(result,root,result.width,result.height);
    for(const n of nodes) if(n.template==='ScrollPanel') layoutScrollPanel(result,n);
  } else {
    root.x=Math.round(Math.max(0,Math.min((container?.width??design.width)-root.width,point.x-root.width/2)));
    root.y=Math.round(Math.max(0,Math.min((container?.height??design.height)-root.height,point.y-root.height/2)));
  }
  for(const n of nodes)captureBounds(result,n);
  const validated=validateDesign(result);design.nodes=validated.nodes;
  if(validated.animations)design.animations=validated.animations;
  design.width=result.width;design.height=result.height;
  return root.id;
}
