import {offsets,setOffset,captureBounds} from './rect-transform.mjs';
export function rectFields(r) {
 const o=offsets(r),sx=r.anchorMin.x!==r.anchorMax.x,sy=r.anchorMin.y!==r.anchorMax.y;
 return {firstX:{label:sx?'Left':'Pos X',value:sx?o.offsetMin.x:r.anchoredPosition.x},firstY:{label:sy?'Top':'Pos Y',value:sy?-o.offsetMax.y:r.anchoredPosition.y},secondX:{label:sx?'Right':'Width',value:sx?-o.offsetMax.x:r.sizeDelta.x},secondY:{label:sy?'Bottom':'Height',value:sy?o.offsetMin.y:r.sizeDelta.y}};
}
export function setRectField(r,key,value){
 const sx=r.anchorMin.x!==r.anchorMax.x,sy=r.anchorMin.y!==r.anchorMax.y;
 if(key==='firstX'){if(sx)setOffset(r,'offsetMin','x',value);else r.anchoredPosition.x=value;}
 if(key==='firstY'){if(sy)setOffset(r,'offsetMax','y',-value);else r.anchoredPosition.y=value;}
 if(key==='secondX'){if(sx)setOffset(r,'offsetMax','x',-value);else r.sizeDelta.x=value;}
 if(key==='secondY'){if(sy)setOffset(r,'offsetMin','y',value);else r.sizeDelta.y=value;}
}
export function anchorPreset(design,n,x,y,{shift=false,alt=false}={}){
 const r=n.rectTransform;
 r.anchorMin={x:x===3?0:x/2,y:y===3?0:1-y/2};r.anchorMax={x:x===3?1:x/2,y:y===3?1:1-y/2};
 if(shift)r.pivot={x:x===3?.5:x/2,y:y===3?.5:1-y/2};
 captureBounds(design,n);
 if(alt){for(const [a,stretch]of [['x',x===3],['y',y===3]]){r.anchoredPosition[a]=0;if(stretch)r.sizeDelta[a]=0;}}
}
// Convert a canvas-space displacement into an ancestor's local axes (2D R*S).
export function localDelta(design,parentId,dx,dy){
 const chain=[];let n=design.nodes.find(n=>n.id===parentId);
 while(n){chain.unshift(n);n=design.nodes.find(v=>v.id===n.parent);}
 for(const n of chain){const a=(n.rotation??0)*Math.PI/180,c=Math.cos(a),s=Math.sin(a),sx=n.localScale?.x??1,sy=n.localScale?.y??1;if(Math.abs(sx)<1e-8||Math.abs(sy)<1e-8)return null;[dx,dy]=[(c*dx-s*dy)/sx,(s*dx+c*dy)/sy];}
 return {x:dx,y:dy};
}
