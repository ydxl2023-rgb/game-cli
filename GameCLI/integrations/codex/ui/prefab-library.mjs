export function setupPrefabLibrary(onRefresh,refreshPrototype){
 const panel=document.createElement('section');panel.id='prefab-resource-panel';panel.setAttribute('role','tabpanel');
 const tools=document.createElement('div');tools.className='prefab-library-tools';
 const library=document.createElement('div');library.id='prefab-library';
 const search=document.createElement('input');search.type='search';search.placeholder='搜索预制控件…';search.setAttribute('aria-label','搜索预制控件');
 const refresh=refreshPrototype.cloneNode(true);refresh.id='refresh-prefab-library';refresh.className='template-library-refresh';refresh.title='刷新预制控件和系统模板';refresh.setAttribute('aria-label',refresh.title);refresh.onclick=onRefresh;
 const label=document.createElement('label');label.className='template-zoom';label.append('缩放');
 const zoom=document.createElement('input');zoom.type='range';zoom.min='0';zoom.max='100';zoom.step='1';zoom.setAttribute('aria-label','预制控件缩放');
 let size=35;try{const value=localStorage.getItem('gamecli.studio.prefabTemplateZoom');if(value!==null&&Number.isFinite(Number(value)))size=Math.max(0,Math.min(100,Number(value)));}catch{}
 zoom.value=String(size);const output=document.createElement('output');label.append(zoom,output);tools.append(label,search,refresh);panel.append(tools,library);document.querySelector('.palette').append(panel);
 const expanded=new WeakMap();
 function update(){
  const query=search.value.trim().toLocaleLowerCase();let matches=0;
  for(const folder of library.querySelectorAll('[data-template-group]')){
   let count=0;for(const item of folder.querySelectorAll('.control')){item.hidden=!!query&&!item.textContent.toLocaleLowerCase().includes(query);if(!item.hidden)count++;}
   folder.hidden=count===0;matches+=count;
   if(query){if(!expanded.has(folder))expanded.set(folder,folder.open);if(count)folder.open=true;}
   else if(expanded.has(folder)){folder.open=expanded.get(folder);expanded.delete(folder);}
   const badge=folder.querySelector('.group-count');if(badge)badge.textContent=String(count);
  }
  let empty=library.querySelector('.template-search-empty');if(!empty){empty=document.createElement('p');empty.className='template-search-empty';empty.setAttribute('role','status');library.append(empty);}empty.hidden=!query||matches>0;empty.textContent='没有匹配的预制控件。';
  library.classList.toggle('template-list-view',size===0);library.style.setProperty('--template-card-width',(140+size*1.8)+'px');library.style.setProperty('--template-icon-size',(24+size*.48)+'px');output.textContent=size===0?'目录':size+'%';
 }
 search.oninput=update;search.onsearch=update;search.onkeydown=e=>{e.stopPropagation();if(e.key==='Escape'){e.preventDefault();search.value='';update();}};
 zoom.oninput=()=>{size=Number(zoom.value);try{localStorage.setItem('gamecli.studio.prefabTemplateZoom',String(size));}catch{}update();};zoom.onkeydown=e=>e.stopPropagation();
 update();return update;
}
