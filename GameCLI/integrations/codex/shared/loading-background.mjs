import {normalizeRect,resolveLayout} from './rect-transform.mjs';
// Runtime LoadingViewLogic assigns this RawImage's texture and activates it.
export function restoreLoadingBackground(design,sourceNode,image) {
 let added=0;
 for(const root of design.nodes.filter(n=>n.name==='Loading'&&n.parent===null)){
  const content=design.nodes.find(n=>n.parent===root.id&&n.animationHost)??root;
  const wrapper=design.nodes.find(n=>n.parent===content.id&&n.name==='Root');
  const bg=design.nodes.find(n=>n.parent===wrapper?.id&&n.name==='Bg');
  if(!bg)continue;
  if(design.nodes.some(n=>n.parent===bg.id&&n.name==='LoadingBg'))continue;
  let id=root.id+'LoadingBg',i=1;while(design.nodes.some(n=>n.id===id))id=root.id+'LoadingBg'+i++;
  design.nodes.push({id,name:'LoadingBg',parent:bg.id,type:'Image',x:0,y:0,width:2440,height:1080,fontSize:20,color:'#ffffff',text:'',active:true,opacity:1,image,imageFit:'100% 100%',uvRect:{x:0,y:0,width:1,height:1},rectTransform:normalizeRect(sourceNode.rectTransform),localScale:{x:1,y:1}});added++;
 }
 resolveLayout(design);return added;
}
