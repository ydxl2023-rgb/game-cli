// Unity coordinates: bottom-left origin, positive Y up. Editor bounds: top-left, Y down.
export function normalizeRect(r) {
  const out = {};
  for (const key of ['anchorMin','anchorMax','pivot','anchoredPosition','sizeDelta']) {
    if (!r?.[key] || !['x','y'].every(a => Number.isFinite(r[key][a]) && Math.abs(r[key][a]) <= 32768)) throw new Error(`RectTransform ${key} 无效。`);
    out[key] = {x:r[key].x,y:r[key].y};
  }
  return out;
}
export function offsets(r) {
  const offsetMin={}, offsetMax={};
  for (const a of ['x','y']) {
    offsetMin[a]=r.anchoredPosition[a]-r.sizeDelta[a]*r.pivot[a];
    offsetMax[a]=r.anchoredPosition[a]+r.sizeDelta[a]*(1-r.pivot[a]);
  }
  return {offsetMin,offsetMax};
}
export function setOffset(r, key, axis, value) {
  const o=offsets(r); o[key][axis]=value;
  r.sizeDelta[axis]=o.offsetMax[axis]-o.offsetMin[axis];
  r.anchoredPosition[axis]=o.offsetMin[axis]+r.sizeDelta[axis]*r.pivot[axis];
}
export function bounds(r, parent) {
  const o=offsets(r);
  const x=parent.width*r.anchorMin.x+o.offsetMin.x;
  const bottom=parent.height*r.anchorMin.y+o.offsetMin.y;
  const width=parent.width*(r.anchorMax.x-r.anchorMin.x)+r.sizeDelta.x;
  const height=parent.height*(r.anchorMax.y-r.anchorMin.y)+r.sizeDelta.y;
  return {x,y:parent.height-bottom-height,width,height};
}
export function captureBounds(design,n) {
  const p=design.nodes.find(v=>v.id===n.parent)??design;
  const fresh=!n.rectTransform;
  const r=n.rectTransform??={anchorMin:{x:0,y:1},anchorMax:{x:0,y:1},pivot:{x:0,y:1},anchoredPosition:{x:0,y:0},sizeDelta:{x:0,y:0}};
  if(fresh && p!==design && (p.template==='ItemSlot' || (p.type==='Button' && n.type==='Text') || (p.template==='InventorySystem' && n.role==='background'))) {
    r.anchorMin={x:n.x/p.width,y:1-(n.y+n.height)/p.height};
    r.anchorMax={x:(n.x+n.width)/p.width,y:1-n.y/p.height};
  }
  r.sizeDelta={x:n.width-p.width*(r.anchorMax.x-r.anchorMin.x),y:n.height-p.height*(r.anchorMax.y-r.anchorMin.y)};
  r.anchoredPosition={x:n.x-p.width*r.anchorMin.x+r.sizeDelta.x*r.pivot.x,y:p.height-n.y-n.height-p.height*r.anchorMin.y+r.sizeDelta.y*r.pivot.y};
}
export function resolveLayout(design) {
  const done=new Set(), visiting=new Set(), map=new Map(design.nodes.map(n=>[n.id,n]));
  function resolve(n) {
    if(done.has(n.id))return;
    if(visiting.has(n.id))throw new Error('RectTransform 层级循环。');
    visiting.add(n.id);
    const p=n.parent===null?design:map.get(n.parent);
    if(!p)throw new Error('RectTransform 父节点不存在。');
    if(p!==design)resolve(p);
    if(!n.rectTransform)captureBounds(design,n);
    n.rectTransform=normalizeRect(n.rectTransform);
    const b=bounds(n.rectTransform,p);
    if(!Object.values(b).every(Number.isFinite)||b.width<1||b.height<1||Object.values(b).some(v=>Math.abs(v)>8192))throw new Error('RectTransform 计算尺寸须为 1–8192，坐标须在 ±8192 内。');
    Object.assign(n,b);visiting.delete(n.id);done.add(n.id);
  }
  design.nodes.forEach(resolve);return design;
}
// Legacy tools explicitly changing bounds must update the corresponding Unity parameters.
export function captureChanges(design,before) {
  const old=new Map(before.nodes.map(n=>[n.id,n]));
  for(const n of design.nodes) {
    const previous=old.get(n.id);
    if(previous && ['x','y','width','height'].some(k=>n[k]!==previous[k]) && JSON.stringify(n.rectTransform)===JSON.stringify(previous.rectTransform))captureBounds(design,n);
  }
}
