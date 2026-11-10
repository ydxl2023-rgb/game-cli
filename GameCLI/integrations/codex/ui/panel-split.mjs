export function setupWorkspaceSplit(){
 const editor=document.querySelector('.editor'),layers=document.querySelector('.layers'),palette=document.querySelector('.palette');
 const key='gamecli.studio.panelWidths.v1';let widths={left:280,right:180};
 try{const saved=JSON.parse(localStorage.getItem(key));if(saved&&['left','right'].every(k=>Number.isFinite(saved[k])))widths=saved;}catch{}
 const dividers={};
 function apply(){
  const total=editor.clientWidth,limit=Math.max(0,total-180-12);
  let left=Math.max(130,Math.min(600,widths.left)),right=Math.max(100,Math.min(480,widths.right));
  if(left+right>limit){const ratio=limit/(left+right);left*=ratio;right*=ratio;}
  editor.style.setProperty('--left-width',`${left}px`);editor.style.setProperty('--right-width',`${right}px`);
  for(const [side,value]of Object.entries({left,right}))dividers[side]?.setAttribute('aria-valuenow',String(Math.round(value)));
 }
 function persist(){try{localStorage.setItem(key,JSON.stringify(widths));}catch{}}
 for(const side of ['left','right']){
  const divider=document.createElement('div');dividers[side]=divider;divider.className='workspace-splitter';divider.id=`${side}-splitter`;divider.tabIndex=0;
  divider.setAttribute('role','separator');divider.setAttribute('aria-orientation','vertical');divider.setAttribute('aria-label',side==='left'?'调整节点面板宽度':'调整控件面板宽度');divider.title='左右拖动调整宽度；双击恢复；方向键微调';
  if(side==='left')layers.after(divider);else palette.before(divider);
  let drag=null;
  divider.onpointerdown=e=>{if(e.button!==0)return;e.preventDefault();drag={x:e.clientX,width:(side==='left'?layers:palette).getBoundingClientRect().width};divider.setPointerCapture(e.pointerId);document.body.classList.add('resizing-workspace');};
  divider.onpointermove=e=>{if(!drag)return;widths[side]=Math.max(side==='left'?130:100,Math.min(editor.clientWidth-300,drag.width+(e.clientX-drag.x)*(side==='left'?1:-1)));apply();};
  const finish=()=>{if(!drag)return;drag=null;document.body.classList.remove('resizing-workspace');persist();};
  for(const event of ['pointerup','pointercancel','lostpointercapture'])divider.addEventListener(event,finish);
  window.addEventListener('blur',finish);
  divider.ondblclick=()=>{widths[side]=side==='left'?280:180;apply();persist();};
  divider.onkeydown=e=>{if(!['ArrowLeft','ArrowRight'].includes(e.key))return;e.preventDefault();e.stopPropagation();widths[side]=Math.max(side==='left'?130:100,widths[side]+(e.key==='ArrowRight'?1:-1)*(side==='left'?1:-1)*(e.shiftKey?40:10));apply();persist();};
 }
 new ResizeObserver(apply).observe(editor);apply();
}

export function setupPanelSplit(){
 const layers=document.querySelector('.layers'),tree=document.getElementById('tree'),inspector=document.getElementById('inspector');
 const divider=document.createElement('div');divider.id='panel-splitter';divider.tabIndex=0;divider.setAttribute('role','separator');divider.setAttribute('aria-label','调整节点列表与属性区高度');divider.setAttribute('aria-orientation','horizontal');divider.setAttribute('aria-controls','tree inspector');divider.title='上下拖动调整高度；双击恢复默认；方向键微调';divider.innerHTML='<span></span>';inspector.before(divider);
 const key='gamecli.studio.inspectorRatio.v1';let ratio=.6,dragging=null;
 try{const saved=Number(localStorage.getItem(key));if(saved>0&&saved<1)ratio=saved;}catch{}
 function available(){const r=layers.getBoundingClientRect(),style=getComputedStyle(layers);return Math.max(0,r.bottom-parseFloat(style.paddingBottom)-tree.getBoundingClientRect().top-divider.getBoundingClientRect().height);}
 function apply(){
  const total=available();if(!total)return;
  const min=Math.min(100,total*.25),max=total-Math.min(70,total*.25),height=Math.max(min,Math.min(max,total*ratio));
  inspector.style.setProperty('--inspector-height',`${height}px`);
  divider.setAttribute('aria-valuemin',String(Math.round(min/total*100)));divider.setAttribute('aria-valuemax',String(Math.round(max/total*100)));divider.setAttribute('aria-valuenow',String(Math.round(height/total*100)));divider.setAttribute('aria-valuetext',`节点属性占 ${Math.round(height/total*100)}%`);
 }
 function persist(){try{localStorage.setItem(key,String(ratio));}catch{}}
 function finish(){if(!dragging)return;dragging=null;document.body.classList.remove('resizing-panels');persist();}
 divider.addEventListener('pointerdown',e=>{if(e.button!==0)return;e.preventDefault();inspector.open=true;apply();dragging={id:e.pointerId,startY:e.clientY,startHeight:parseFloat(inspector.style.getPropertyValue('--inspector-height'))};divider.setPointerCapture(e.pointerId);divider.focus();document.body.classList.add('resizing-panels');});
 divider.addEventListener('pointermove',e=>{if(!dragging||dragging.id!==e.pointerId)return;const total=available();if(!total)return;ratio=Math.max(Math.min(100,total*.25)/total,Math.min(1-Math.min(70,total*.25)/total,(dragging.startHeight+dragging.startY-e.clientY)/total));apply();});
 for(const event of ['pointerup','pointercancel','lostpointercapture'])divider.addEventListener(event,finish);
 window.addEventListener('blur',finish);
 divider.addEventListener('dblclick',()=>{ratio=.6;inspector.open=true;apply();persist();});
 divider.addEventListener('keydown',e=>{if(!['ArrowUp','ArrowDown','Home','End'].includes(e.key))return;e.preventDefault();e.stopPropagation();inspector.open=true;ratio=e.key==='Home'?.15:e.key==='End'?.9:Math.max(.1,Math.min(.9,ratio+(e.key==='ArrowUp'?1:-1)*(e.shiftKey?.1:.02)));apply();persist();});
 inspector.addEventListener('toggle',apply);new ResizeObserver(apply).observe(layers);apply();
}
