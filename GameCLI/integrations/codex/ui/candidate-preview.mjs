import {validateDesign,layerStyle,nodeName} from '../shared/design.mjs';
import {createPreviewState,runPreviewAction,hierarchyVisibility,defaultAction} from '../shared/preview.mjs';
import {playAnimations} from './animation-player.mjs';

// A detached, read-only runtime: never writes the candidate or its project.
export function mountCandidatePreview(host,payload){
 const design=validateDesign(payload.design),root=design.nodes.find(n=>n.id===payload.root);
 let state;const elements=new Map();
 const toolbar=document.createElement('div');toolbar.className='preview-toolbar';
 const label=document.createElement('span');label.textContent=`${design.width} × ${design.height} · 可点击预演`;
 const reset=document.createElement('button');reset.textContent='重置预演';
 const back=document.createElement('button');back.textContent='返回';
 const full=document.createElement('button');full.textContent='全屏预演';
 const shell=document.createElement('div');shell.className='candidate-player';
 const viewport=document.createElement('div');viewport.className='candidate-viewport';
 const wrap=document.createElement('div');wrap.className='candidate-wrap';
 const canvas=document.createElement('div');canvas.className='candidate-canvas';canvas.style.width=design.width+'px';canvas.style.height=design.height+'px';
 const status=document.createElement('p');status.className='preview-status';status.setAttribute('role','status');
 toolbar.append(label,back,reset,full);wrap.append(canvas);viewport.append(wrap);shell.append(toolbar,viewport,status);host.append(shell);
 function draw(parent,target){for(const n of design.nodes.filter(n=>n.parent===parent)){
  const e=document.createElement(n.type==='Button'?'button':'div');e.className='candidate-node';e.dataset.node=n.id;e.dataset.type=n.type;e.setAttribute('aria-label',nodeName(n));
  Object.assign(e.style,{left:n.x+'px',top:n.y+'px',width:n.width+'px',height:n.height+'px',backgroundColor:n.color,color:n.type==='Text'?n.color:'#101923',fontSize:n.fontSize+'px'});
  e.style.cssText+=layerStyle(n);e.textContent=n.text;
  const interactive=!!defaultAction(design,n)||n.type==='Button'||n.template==='ItemSlot';
  e.style.pointerEvents=interactive||n.template==='ScrollPanel'?'auto':'none';
  if(interactive){e.dataset.interactive='true';e.style.cursor='pointer';if(n.type!=='Button'){e.tabIndex=0;e.setAttribute('role','button');}}
  if(n.type==='Button')e.disabled=n.interactable===false;
  if(n.template==='ConfirmDialog')e.style.zIndex='100';
  elements.set(n.id,e);target.append(e);draw(n.id,e);
 }}draw(null,canvas);
 function apply(){const visibility=hierarchyVisibility(design,state.hidden,true);for(const [id,e]of elements)e.hidden=!visibility.get(id);back.disabled=!state.history.length;}
 function restart(){state=createPreviewState(design);if(root)state.hidden=state.hidden.filter(id=>id!==root.id);apply();status.textContent='播放候选中的动画及已配置动作；不执行原始 C# 或服务器逻辑。';}
 canvas.addEventListener('click',event=>{const e=event.target.closest('[data-interactive]');if(!e)return;const n=design.nodes.find(n=>n.id===e.dataset.node);let owner=n;while(owner){if(owner.type==='Button'&&owner.interactable===false)return;owner=design.nodes.find(p=>p.id===owner.parent);}const action=defaultAction(design,n);if(!action||action.type==='none'){status.textContent=nodeName(n)+'：尚未配置预演动作。';return;}state=runPreviewAction(design,state,action);apply();status.textContent=nodeName(n)+'：'+({show:'显示',hide:'关闭',toggle:'切换',navigate:'跳转',back:'返回'})[action.type];});
 canvas.addEventListener('keydown',event=>{if(event.target.matches('[data-interactive]:not(button)')&&['Enter',' '].includes(event.key)){event.preventDefault();event.target.click();}});
 back.onclick=()=>{state=runPreviewAction(design,state,{type:'back'});apply();};reset.onclick=restart;
 const expand=()=>{const expanded=shell.classList.toggle('expanded');full.textContent=expanded?'退出全屏':'全屏预演';document.body.style.overflow=expanded?'hidden':'';};
 full.onclick=expand;
 const escape=event=>{if(event.key==='Escape'&&shell.classList.contains('expanded'))expand();};document.addEventListener('keydown',escape);
 const fit=()=>{const scale=Math.min((viewport.clientWidth-24)/design.width,(viewport.clientHeight-24)/design.height);if(scale<=0)return;wrap.style.width=design.width*scale+'px';wrap.style.height=design.height*scale+'px';canvas.style.transform=`scale(${scale})`;};
 const observer=new ResizeObserver(fit);observer.observe(viewport);restart();fit();
 let stop=()=>{};const visible=new IntersectionObserver(entries=>{stop();stop=entries[0].isIntersecting?playAnimations(design,canvas):()=>{};});visible.observe(canvas);
 return ()=>{observer.disconnect();visible.disconnect();stop();document.removeEventListener('keydown',escape);if(shell.classList.contains('expanded')){shell.classList.remove('expanded');document.body.style.overflow='';}};
}
