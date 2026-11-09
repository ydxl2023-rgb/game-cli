import {resolveLayout} from './rect-transform.mjs';
export function ensureAnimationHost(design,rootId){
 resolveLayout(design);
 const root=design.nodes.find(n=>n.id===rootId);if(!root)throw new Error('界面根节点不存在。');
 let host=design.nodes.find(n=>n.parent===rootId&&n.name==='Animator');
 if(host){if(host.type!=='Container'||host.x!==0||host.y!==0||host.width!==root.width||host.height!==root.height||(host.rotation??0)!==0||(host.localScale?.x??1)!==1||(host.localScale?.y??1)!==1)throw new Error('已有 Animator 不是全屏单位变换容器，请先检查。');}
 else{
  let id=rootId.slice(0,48)+'Animator',i=1;while(design.nodes.some(n=>n.id===id)||id===design.name)id=rootId.slice(0,48)+'Animator'+i++;
  host={id,name:'Animator',parent:rootId,type:'Container',x:0,y:0,width:root.width,height:root.height,color:'#ffffff',text:'',fontSize:20,surface:'transparent',localScale:{x:1,y:1},rectTransform:{anchorMin:{x:0,y:0},anchorMax:{x:1,y:1},pivot:{x:.5,y:.5},anchoredPosition:{x:0,y:0},sizeDelta:{x:0,y:0}}};
  design.nodes.splice(design.nodes.indexOf(root)+1,0,host);
 }
 host.animationHost=true;
 for(const n of design.nodes)if(n.parent===rootId&&n.id!==host.id)n.parent=host.id;
 resolveLayout(design);return host.id;
}
