export function setupTemplateDock(onRefresh){
 const area=document.querySelector('.canvas-area'),library=document.getElementById('template-library');
 const divider=document.createElement('div');divider.id='template-splitter';divider.tabIndex=0;divider.setAttribute('role','separator');divider.setAttribute('aria-label','调整模板窗口高度');divider.setAttribute('aria-orientation','horizontal');divider.title='上下拖动调整模板窗口高度；双击恢复默认';divider.innerHTML='<span></span>';
 const dock=document.createElement('section');dock.id='template-dock';dock.setAttribute('aria-label','模板窗口');
 const tabs=document.createElement('div');tabs.className='studio-tabs';tabs.setAttribute('role','tablist');tabs.setAttribute('aria-label','模板分类');
 let category='prefab',height=220;
 try{category=localStorage.getItem('gamecli.studio.templateTab')==='system'?'system':'prefab';height=Number(localStorage.getItem('gamecli.studio.templateHeight'))||220;}catch{}
 function select(value){category=value;for(const b of tabs.children){b.setAttribute('aria-selected',b.dataset.category===category);b.tabIndex=b.dataset.category===category?0:-1;}for(const panel of library.querySelectorAll('[data-library-category]'))panel.hidden=panel.dataset.libraryCategory!==category;try{localStorage.setItem('gamecli.studio.templateTab',category);}catch{}}
 for(const [id,name]of [['prefab','预制控件'],['system','系统模板']]){
  const button=document.createElement('button');button.type='button';button.id='library-tab-'+id;button.dataset.category=id;button.setAttribute('role','tab');button.setAttribute('aria-controls','library-panel-'+id);button.textContent=name;button.onclick=()=>select(id);
  button.onkeydown=e=>{if(!['ArrowLeft','ArrowRight'].includes(e.key))return;e.preventDefault();select(category==='prefab'?'system':'prefab');document.getElementById('library-tab-'+category).focus();};tabs.append(button);
 }
 const toolbar=document.createElement('div');toolbar.className='template-toolbar';
 const refresh=document.createElement('button');refresh.id='refresh-templates';refresh.type='button';refresh.title='刷新模板库';refresh.setAttribute('aria-label','刷新模板库');
 refresh.innerHTML='<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 7v5h-5M4 17v-5h5"/><path d="M6.1 6.1A8 8 0 0 1 20 12M4 12a8 8 0 0 0 13.9 5.9"/></svg>';
 refresh.onclick=onRefresh;toolbar.append(tabs,refresh);dock.append(toolbar,library);area.append(divider,dock);
 const note=document.querySelector('.palette .palette-note');if(note)note.textContent='拖入画布添加 UGUI 控件。单击可居中添加，Delete 删除，方向键移动。';
 function apply(){const max=Math.max(80,area.clientHeight-210);const value=Math.max(80,Math.min(max,height));dock.style.height=value+'px';divider.setAttribute('aria-valuenow',String(Math.round(value)));}
 function persist(){try{localStorage.setItem('gamecli.studio.templateHeight',String(height));}catch{}}
 let drag=null;
 divider.onpointerdown=e=>{if(e.button!==0)return;e.preventDefault();drag={y:e.clientY,height:dock.getBoundingClientRect().height};divider.setPointerCapture(e.pointerId);document.body.classList.add('resizing-panels');};
 divider.onpointermove=e=>{if(!drag)return;height=Math.max(80,Math.min(Math.max(80,area.clientHeight-210),drag.height+drag.y-e.clientY));apply();};
 const finish=()=>{if(!drag)return;drag=null;document.body.classList.remove('resizing-panels');persist();};
 for(const event of ['pointerup','pointercancel','lostpointercapture'])divider.addEventListener(event,finish);
 window.addEventListener('blur',finish);
 divider.ondblclick=()=>{height=220;apply();persist();};
 divider.onkeydown=e=>{if(!['ArrowUp','ArrowDown'].includes(e.key))return;e.preventDefault();e.stopPropagation();height=Math.max(80,Math.min(Math.max(80,area.clientHeight-210),height+(e.key==='ArrowUp'?1:-1)*(e.shiftKey?40:10)));apply();persist();};
 new ResizeObserver(apply).observe(area);apply();select(category);
 return ()=>select(category);
}
