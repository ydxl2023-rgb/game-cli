import {instantiateTemplate} from './templates.mjs';
export function upgradeTemplate(design,rootId,template){
 const next=structuredClone(design),old=next.nodes.find(n=>n.id===rootId);
 if(!old||old.parent!==null||old.templateSourceId!==template.id)throw new Error('请选择关联此模板的系统根节点。');
 const ids=new Set([rootId]);let count;do{count=ids.size;for(const n of next.nodes)if(ids.has(n.parent))ids.add(n.id);}while(count!==ids.size);
 for(const n of next.nodes)if(!ids.has(n.id)&&ids.has(n.action?.target)&&n.action.target!==rootId)throw new Error('其他界面引用了此系统内部节点，请先处理该连线再升级。');
 next.nodes=next.nodes.filter(n=>!ids.has(n.id));next.animations=(next.animations??[]).filter(a=>!ids.has(a.hostNodeId));
 const id=instantiateTemplate(next,template,null,{x:0,y:0},{allowCode:true}),root=next.nodes.find(n=>n.id===id);
 root.name=old.name;root.active=old.active;if(old.relationPosition)root.relationPosition=old.relationPosition;
 if(old.action&&(!ids.has(old.action.target)||old.action.target===rootId))root.action={...old.action,...(old.action.target===rootId?{target:id}:{})};
 for(const n of next.nodes)if(!ids.has(n.id)&&n.action?.target===rootId)n.action.target=id;
 return {design:next,rootId:id};
}
