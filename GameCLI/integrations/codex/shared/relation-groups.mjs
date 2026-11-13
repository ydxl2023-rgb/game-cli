// Groups organize the project graph only; runtime ownership and actions stay on systems.
export function validateGroups(groups,nodes){
  if(!Array.isArray(groups)||groups.length>200)throw Error('关系分组列表无效。');
  const roots=new Set(nodes.filter(n=>n.parent===null).map(n=>n.id)),ids=new Set(),members=new Set();
  return groups.map(g=>{
    if(!g||typeof g.id!=='string'||!/^group-[\w-]+$/.test(g.id)||ids.has(g.id))throw Error('关系分组 ID 无效或重复。');
    if(typeof g.name!=='string'||!g.name.trim()||g.name.length>60)throw Error('分组名称须为 1–60 个字符。');
    if(!Array.isArray(g.members)||!g.members.length)throw Error('分组必须包含系统，且只能使用单层结构。');
    ids.add(g.id);
    for(const id of g.members){if(!roots.has(id)||members.has(id))throw Error('分组只能包含系统根节点，每个系统只能属于一个组。');members.add(id);}
    if(g.frame!==undefined&&(!g.frame||!['x','y','width','height'].every(k=>Number.isFinite(g.frame[k]))||g.frame.x<0||g.frame.y<0||g.frame.x>6000||g.frame.y>6000||g.frame.width<280||g.frame.height<120||g.frame.width>8192||g.frame.height>8192))throw Error('分组边框尺寸无效。');
    return {id:g.id,name:g.name.trim(),members:[...g.members],...(g.frame?{frame:{x:g.frame.x,y:g.frame.y,width:g.frame.width,height:g.frame.height}}:{})};
  });
}
export function pruneGroups(design){
  if(!design.relationGroups)return;
  const roots=new Set(design.nodes.filter(n=>n.parent===null).map(n=>n.id));
  design.relationGroups=design.relationGroups.map(g=>({...g,members:g.members.filter(id=>roots.has(id))})).filter(g=>g.members.length);
}
export function moveGroup(design,id,positions,dx,dy){
  const group=design.relationGroups?.find(g=>g.id===id);
  if(!group)throw Error('分组不存在。');
  const entries=group.members.map(id=>[id,positions.get(id)]);
  const points=entries.map(([,p])=>p);if(group.frame)points.push(group.frame);
  dx=Math.max(-Math.min(...points.map(p=>p.x)),Math.min(dx,6000-Math.max(...points.map(p=>p.x))));
  dy=Math.max(-Math.min(...points.map(p=>p.y)),Math.min(dy,6000-Math.max(...points.map(p=>p.y))));
  for(const [id,p]of entries)design.nodes.find(n=>n.id===id).relationPosition={x:Math.round(p.x+dx),y:Math.round(p.y+dy)};
  if(group.frame)group.frame={...group.frame,x:Math.round(group.frame.x+dx),y:Math.round(group.frame.y+dy)};
}
export function resizeGroupFrame(frame,handle,dx,dy){
  let {x,y,width,height}=frame;
  if(handle.includes('e'))width=Math.max(280,Math.min(8192,width+dx));
  if(handle.includes('s'))height=Math.max(120,Math.min(8192,height+dy));
  if(handle.includes('w')){const right=x+width;x=Math.max(0,Math.min(6000,right-280,Math.max(right-8192,x+dx)));width=right-x;}
  if(handle.includes('n')){const bottom=y+height;y=Math.max(0,Math.min(6000,bottom-120,Math.max(bottom-8192,y+dy)));height=bottom-y;}
  return {x:Math.round(x),y:Math.round(y),width:Math.round(width),height:Math.round(height)};
}
// Fit graph cards only; prefab coordinates and UI dimensions are never changed.
export function fitGroupCards(frame,cards){
  const left=Math.min(...cards.map(c=>c.x)),top=Math.min(...cards.map(c=>c.y));
  const width=Math.max(...cards.map(c=>c.x+c.width*c.scale))-left;
  const height=Math.max(...cards.map(c=>c.y+c.height*c.scale))-top;
  const factor=Math.min(1,(frame.width-32)/width,(frame.height-68)/height);
  const x=Math.max(frame.x+16,Math.min(left,frame.x+frame.width-16-width*factor));
  const y=Math.max(frame.y+52,Math.min(top,frame.y+frame.height-16-height*factor));
  return cards.map(c=>({id:c.id,x:x+(c.x-left)*factor,y:y+(c.y-top)*factor,scale:c.scale*factor}));
}
