import {resolveLayout,captureBounds} from './rect-transform.mjs';
// Explicit Studio hierarchy override requested by the user. Source Prefab is unchanged.
export function groupLoadingFooter(design) {
 resolveLayout(design);
 let changed=0;
 for(const bg of design.nodes.filter(n=>n.name==='RateBg')) {
  const root=design.nodes.find(n=>n.id===bg.parent);
  if(root?.name!=='Loading' && !(root?.animationHost && design.nodes.find(n=>n.id===root.parent)?.name==='Loading'))continue;
  if((bg.localScale?.x??1)!==1 || (bg.localScale?.y??1)!==1)throw new Error('RateBg 分组要求父节点缩放为 1。');
  for(const n of design.nodes.filter(n=>n.parent===root.id && ['TipText','Ball','Haikyu','HAIKYU','Rate'].includes(n.name))) {
   n.parent=bg.id;n.x-=bg.x;n.y-=bg.y;captureBounds(design,n);changed++;
  }
 }
 resolveLayout(design);return changed;
}
