import {hierarchyVisibility} from './preview.mjs';
export function thumbnailVisibility(design,rootId){
  return hierarchyVisibility({...design,nodes:design.nodes.map(n=>n.id===rootId?{...n,active:true}:n)});
}

export function systemRoot(design,id){
  const map=new Map(design.nodes.map(n=>[n.id,n]));let n=map.get(id),count=0;
  while(n?.parent&&count++<design.nodes.length)n=map.get(n.parent);
  return n??null;
}
export const startupPortId=rootId=>rootId+'::startupComplete';
export function relationSource(design,id){
  const node=design.nodes.find(n=>n.id===id);
  if(node)return node;
  const root=design.nodes.find(n=>n.parent===null&&n.startupComplete!==undefined&&startupPortId(n.id)===id);
  return root?{id,name:'启动完成',parent:root.id,trigger:'startupComplete',action:root.startupComplete}:null;
}
export function disconnectUi(design,sourceId){
  const source=relationSource(design,sourceId);
  if(source?.trigger==='startupComplete')design.nodes.find(n=>n.id===source.parent).startupComplete={type:'none'};
  else if(source)delete source.action;
}
export function relations(design){
  // Only actions targeting an entire system belong in the relationship graph.
  // Closing and local interactions remain available in the preview without graph edges.
  const roots=new Set(design.nodes.filter(n=>n.parent===null).map(n=>n.id));
  const clicks=design.nodes.filter(n=>roots.has(n.action?.target)&&['navigate','show','toggle'].includes(n.action.type)).map(n=>({sourceId:n.id,sourceRoot:systemRoot(design,n.id)?.id,targetId:n.action.target,targetRoot:n.action.target,type:n.action.type}));
  const events=design.nodes.filter(n=>n.parent===null&&roots.has(n.startupComplete?.target)).map(n=>({sourceId:startupPortId(n.id),sourceRoot:n.id,targetId:n.startupComplete.target,targetRoot:n.startupComplete.target,type:'show',trigger:'startupComplete'}));
  return [...clicks,...events];
}
export function connectUi(design,sourceId,targetId,type='navigate'){
  const source=relationSource(design,sourceId),target=design.nodes.find(n=>n.id===targetId);
  if(!source||!target||target.parent!==null||!['navigate','show','hide','toggle'].includes(type))throw new Error('连接的节点或动作无效。');
  if(source.trigger==='startupComplete'){
    if(type!=='show'||source.parent===target.id)throw new Error('启动完成只能激活其他系统。');
    design.nodes.find(n=>n.id===source.parent).startupComplete={type:'show',target:target.id};return;
  }
  source.action={type,target:target.id};
}
