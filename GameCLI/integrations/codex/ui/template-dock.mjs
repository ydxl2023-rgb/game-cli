export function setupTemplateDock(onRefresh,designPanel,templateSelector=false){
 const area=document.querySelector('.canvas-area'),library=document.getElementById('template-library');
 const divider=document.createElement('div');divider.id='template-splitter';divider.tabIndex=0;divider.setAttribute('role','separator');divider.setAttribute('aria-label','调整模板窗口高度');divider.setAttribute('aria-orientation','horizontal');divider.title='上下拖动调整模板窗口高度；双击恢复默认';divider.innerHTML='<span></span>';
 const dock=document.createElement('section');dock.id='template-dock';dock.setAttribute('aria-label','模板窗口');
 const tabs=document.createElement('div');tabs.className='studio-tabs';tabs.setAttribute('role','tablist');tabs.setAttribute('aria-label','模板分类');
 const choices=designPanel?[['prefab','预制控件'],['system','系统模板'],['design','系统设计']]:templateSelector?[['prefab','预制控件'],['system','系统模板']]:[['prefab','预制控件']];
 const tabStorageKey=designPanel?'gamecli.studio.templateTab':'gamecli.studio.templateEditorTab';
 const search=document.createElement('input');search.type='search';search.id='template-search';search.placeholder='搜索模板…';search.setAttribute('aria-label','搜索预制模板');search.title='按模板名称或描述搜索当前分类';search.autocomplete='off';
 const folderStates=new WeakMap();
 function filterTemplates(){
  const query=search.value.trim().toLocaleLowerCase();
  for(const panel of library.querySelectorAll('[data-library-category]')){
   let matches=0;
   for(const folder of panel.querySelectorAll('[data-template-group]')){
    let count=0;
    for(const item of folder.querySelectorAll('.control')){const visible=!query||item.textContent.toLocaleLowerCase().includes(query);item.hidden=!visible;if(visible)count++;}
    folder.hidden=count===0;matches+=count;
    if(query){if(!folderStates.has(folder))folderStates.set(folder,folder.open);if(count)folder.open=true;}
    else if(folderStates.has(folder)){folder.open=folderStates.get(folder);folderStates.delete(folder);}
    const badge=folder.querySelector('.group-count');if(badge)badge.textContent=String(count);
   }
   let empty=panel.querySelector('.template-search-empty');
   if(!empty){empty=document.createElement('p');empty.className='template-search-empty';empty.setAttribute('role','status');panel.append(empty);}
   empty.hidden=!query||matches>0;empty.textContent='没有匹配的模板，请更换关键词。';
  }
 }
 search.oninput=filterTemplates;search.onsearch=filterTemplates;
 search.onkeydown=e=>{e.stopPropagation();if(e.key==='Escape'){e.preventDefault();search.value='';filterTemplates();}};
 const zoomControls=document.createElement('label');zoomControls.className='template-zoom';zoomControls.hidden=true;
 const zoomLabel=document.createElement('span');zoomLabel.textContent='缩放';
 const zoom=document.createElement('input');zoom.type='range';zoom.id='template-zoom';zoom.min='0';zoom.max='100';zoom.step='1';zoom.setAttribute('aria-label','系统模板缩放');zoom.title='拖到 0 显示目录列表，向右放大卡片';
 const zoomValue=document.createElement('output');zoomValue.htmlFor=zoom.id;zoomControls.append(zoomLabel,zoom,zoomValue);
 const zoomKeys={prefab:'gamecli.studio.prefabTemplateZoom',system:'gamecli.studio.systemTemplateZoom'},zoomSizes={prefab:35,system:35};
 for(const kind of Object.keys(zoomKeys))try{const saved=localStorage.getItem(zoomKeys[kind]);if(saved!==null&&Number.isFinite(Number(saved)))zoomSizes[kind]=Math.max(0,Math.min(100,Number(saved)));}catch{}
 function applyZoom(){const zoomSize=zoomSizes[category]??35;zoom.value=String(zoomSize);zoom.setAttribute('aria-label',category==='prefab'?'预制控件缩放':'系统模板缩放');library.classList.toggle('template-list-view',zoomSize===0);library.style.setProperty('--template-card-width',(140+zoomSize*1.8)+'px');library.style.setProperty('--template-icon-size',(24+zoomSize*.48)+'px');zoomValue.textContent=zoomSize===0?'目录':zoomSize+'%';zoom.setAttribute('aria-valuetext',zoomSize===0?'目录列表':zoomSize+'% 卡片大小');}
 zoom.oninput=()=>{if(!zoomKeys[category])return;zoomSizes[category]=Number(zoom.value);applyZoom();try{localStorage.setItem(zoomKeys[category],String(zoomSizes[category]));}catch{}};
 zoom.onkeydown=e=>e.stopPropagation();
 let category='prefab',height=220;
 try{category=localStorage.getItem(tabStorageKey)||'prefab';if(!['prefab','system','design'].includes(category))category='prefab';height=Number(localStorage.getItem('gamecli.studio.templateHeight'))||220;}catch{}
 function select(value){if(!choices.some(([id])=>id===value))value='prefab';if(value==='design'&&category!=='design'&&height<300){height=300;apply();persist();}category=value;search.hidden=category==='design';filterTemplates();zoomControls.hidden=category==='design';applyZoom();for(const b of tabs.children){b.setAttribute('aria-selected',b.dataset.category===category);b.tabIndex=b.dataset.category===category?0:-1;}library.hidden=category==='design';if(designPanel)designPanel.hidden=category!=='design';for(const panel of library.querySelectorAll('[data-library-category]'))panel.hidden=panel.dataset.libraryCategory!==category;try{localStorage.setItem(tabStorageKey,category);}catch{}}
 for(const [id,name]of choices){
  const button=document.createElement('button');button.type='button';button.id='library-tab-'+id;button.dataset.category=id;button.setAttribute('role','tab');button.setAttribute('aria-controls','library-panel-'+id);button.textContent=name;button.onclick=()=>select(id);
  button.onkeydown=e=>{if(!['ArrowLeft','ArrowRight'].includes(e.key))return;e.preventDefault();const ids=choices.map(([id])=>id);select(ids[(ids.indexOf(category)+(e.key==='ArrowRight'?1:ids.length-1))%ids.length]);document.getElementById('library-tab-'+category).focus();};tabs.append(button);
 }
 const toolbar=document.createElement('div');toolbar.className='template-toolbar';
 const refresh=document.createElement('button');refresh.id='refresh-templates';refresh.type='button';refresh.title='刷新模板库';refresh.setAttribute('aria-label','刷新模板库');
 refresh.innerHTML='<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 7v5h-5M4 17v-5h5"/><path d="M6.1 6.1A8 8 0 0 1 20 12M4 12a8 8 0 0 0 13.9 5.9"/></svg>';
 refresh.onclick=onRefresh;toolbar.append(tabs,zoomControls,search);const actions=document.createElement('div');actions.className='editor-preview-actions';actions.append(document.getElementById('preview-back'),document.getElementById('preview'),document.getElementById('preview-pause'),document.getElementById('preview-stop'),refresh);document.querySelector('.canvas-toolbar').append(actions);dock.append(toolbar,library);if(designPanel)dock.append(designPanel);area.append(divider,dock);
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
