import {hierarchyVisibility} from './preview.mjs';
export function thumbnailVisibility(design,rootId){
  return hierarchyVisibility({...design,nodes:design.nodes.map(n=>n.id===rootId?{...n,active:true}:n)});
}

export function systemRoot(design,id){
  const map=new Map(design.nodes.map(n=>[n.id,n]));let n=map.get(id),count=0;
  while(n?.parent&&count++<design.nodes.length)n=map.get(n.parent);
  return n??null;
}
export function relations(design){
  // Only actions targeting an entire system belong in the relationship graph.
  // Closing and local interactions remain available in the preview without graph edges.
  const roots=new Set(design.nodes.filter(n=>n.parent===null).map(n=>n.id));
  return design.nodes.filter(n=>roots.has(n.action?.target)&&['navigate','show','toggle'].includes(n.action.type)).map(n=>({sourceId:n.id,sourceRoot:systemRoot(design,n.id)?.id,targetId:n.action.target,targetRoot:n.action.target,type:n.action.type}));
}
export function connectUi(design,sourceId,targetId,type='navigate'){
  const source=design.nodes.find(n=>n.id===sourceId),target=design.nodes.find(n=>n.id===targetId);
  if(!source||!target||target.parent!==null||!['navigate','show','hide','toggle'].includes(type))throw new Error('连接的节点或动作无效。');
  source.action={type,target:target.id};
}
