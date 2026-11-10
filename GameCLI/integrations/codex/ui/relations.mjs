import {relations,systemRoot,connectUi,thumbnailVisibility} from '../shared/relations.mjs';
const labels={navigate:'跳转',show:'打开',hide:'关闭',toggle:'切换'};
export function createRelations({getDesign,edit,notice,onEdit,onPreview,onSelect=()=>{},onDelete=()=>{}}){
  const viewport=document.getElementById('relations-view'),plane=document.getElementById('relations-plane');
  const svg=document.getElementById('relations-wires'),cards=document.getElementById('relations-cards'),info=document.getElementById('relations-info');
  const selections=new Map(),positions=new Map();let pending=null,drag=null,selectedEdge=null,selectedCard=null;
  let zoom=1,pan=null,panX=0,panY=0;
  function applyPan(){plane.style.transform=`translate(${panX/zoom}px, ${panY/zoom}px)`;viewport.style.backgroundPosition=`${panX-viewport.scrollLeft}px ${panY-viewport.scrollTop}px`;}
  const zoomButton=document.createElement('button');zoomButton.id='relations-zoom';zoomButton.type='button';zoomButton.title='关系图缩放 · 点击恢复 100%';zoomButton.setAttribute('aria-label','重置关系图缩放');document.getElementById('relations-tools').append(zoomButton);
  function applyZoom(){plane.style.zoom=String(zoom);viewport.style.backgroundSize=`${20*zoom}px ${20*zoom}px`;zoomButton.textContent=Math.round(zoom*100)+'%';applyPan();}
  zoomButton.onclick=()=>{zoom=1;panX=panY=0;viewport.scrollLeft=0;viewport.scrollTop=0;applyZoom();};
  viewport.addEventListener('wheel',e=>{
    e.preventDefault();if(drag||pan)return;
    const p=point(e),delta=e.deltaY*(e.deltaMode===1?16:e.deltaMode===2?viewport.clientHeight:1);
    zoom=Math.max(.25,Math.min(3,zoom*Math.exp(-Math.max(-400,Math.min(400,delta))*.002)));
    applyZoom();const r=plane.getBoundingClientRect();
    viewport.scrollLeft+=r.left+p.x*zoom-e.clientX;viewport.scrollTop+=r.top+p.y*zoom-e.clientY;
    if(pending)drawWires(point(e));
  },{passive:false});
  applyZoom();
  viewport.addEventListener('scroll',applyPan);
  viewport.addEventListener('pointerdown',e=>{
    if(e.button!==1||drag)return;
    e.preventDefault();e.stopPropagation();
    pan={id:e.pointerId,x:e.clientX,y:e.clientY,startX:panX,startY:panY};
    viewport.setPointerCapture(e.pointerId);viewport.classList.add('panning');
  },true);
  function finishPan(){if(!pan)return;const id=pan.id;pan=null;viewport.classList.remove('panning');if(viewport.hasPointerCapture(id))viewport.releasePointerCapture(id);}
  viewport.addEventListener('auxclick',e=>{if(e.button===1)e.preventDefault();});
  viewport.addEventListener('lostpointercapture',finishPan);
  window.addEventListener('blur',finishPan);
  document.getElementById('relations-tools').querySelector('span:nth-child(2)').textContent='滚轮缩放 · 按住中键平移 · 拖动卡片 / 圆点连线';
  function element(tag,cls,text){const el=document.createElement(tag);el.className=cls;if(text)el.textContent=text;return el;}
  function svgEl(tag,attrs){const el=document.createElementNS('http://www.w3.org/2000/svg',tag);for(const [k,v]of Object.entries(attrs))el.setAttribute(k,String(v));return el;}
  function point(event){const r=plane.getBoundingClientRect();return {x:(event.clientX-r.left)/zoom,y:(event.clientY-r.top)/zoom};}
  function port(id,side){const p=positions.get(id);return {x:p.x+(side==='out'?232:0),y:p.y+194};}
  function path(a,b){const bend=Math.max(60,Math.abs(b.x-a.x)/2);return `M ${a.x} ${a.y} C ${a.x+bend} ${a.y}, ${b.x-bend} ${b.y}, ${b.x} ${b.y}`;}
  function drawWires(pointer){
    svg.replaceChildren();const defs=svgEl('defs',{}),marker=svgEl('marker',{id:'relation-arrow',viewBox:'0 0 10 10',refX:9,refY:5,markerWidth:6,markerHeight:6,orient:'auto'});marker.append(svgEl('path',{d:'M 0 0 L 10 5 L 0 10 z',fill:'#c4ef91'}));defs.append(marker);svg.append(defs);
    const design=getDesign(),edges=relations(design);
    for(const edge of edges){
      if(!positions.has(edge.sourceRoot)||!positions.has(edge.targetRoot))continue;
      const a=port(edge.sourceRoot,'out'),b=port(edge.targetRoot,'in');
      const d=edge.sourceRoot===edge.targetRoot?`M ${a.x} ${a.y} C ${a.x+85} ${a.y-300}, ${b.x-85} ${b.y-300}, ${b.x} ${b.y}`:path(a,b);
      const line=svgEl('path',{d,class:'relation-wire'+(selectedEdge===edge.sourceId?' selected':''),'marker-end':'url(#relation-arrow)'});
      const hit=svgEl('path',{d,class:'relation-wire-hit',tabindex:0,role:'button','aria-label':'选择连线 '+(design.nodes.find(n=>n.id===edge.sourceId)?.name??edge.sourceId)});
      const choose=()=>{selectedCard=null;for(const item of cards.children)item.classList.remove('selected-card');selectedEdge=edge.sourceId;showInfo();drawWires();};hit.onclick=choose;hit.onkeydown=e=>{if(e.key==='Enter'||e.key===' '){e.preventDefault();choose();}};
      svg.append(line,hit);
      const label=svgEl('text',{x:(a.x+b.x)/2,y:(a.y+b.y)/2-10,class:'relation-edge-label'});label.textContent=labels[edge.type];svg.append(label);
    }
    if(pending&&positions.has(pending.root))svg.append(svgEl('path',{d:path(port(pending.root,'out'),pointer??port(pending.root,'out')),class:'relation-wire pending'}));
    document.getElementById('relations-count').textContent=`${positions.size} 个 UI · ${edges.length} 条关系`;
  }
  function showInfo(){
    info.replaceChildren();const design=getDesign(),source=design.nodes.find(n=>n.id===selectedEdge);
    if(!source?.action?.target){selectedEdge=null;info.textContent='选择触发节点，从右侧圆点拖到目标卡片左侧圆点。也可依次点击两个圆点。';return;}
    const target=design.nodes.find(n=>n.id===source.action.target);
    info.append(element('span','',`${source.name} · 点击 → ${labels[source.action.type]} ${target?.name??''}`));
    const remove=element('button','danger','删除连线');remove.onclick=()=>{edit(()=>{delete getDesign().nodes.find(n=>n.id===selectedEdge).action;});selectedEdge=null;showInfo();};info.append(remove);
  }
  function complete(rootId){
    if(!pending)return;const {source,type}=pending;pending=null;
    edit(()=>connectUi(getDesign(),source,rootId,type));selectedEdge=source;showInfo();drawWires();notice('已配置点击'+labels[type]+'。进入预演可验证；保存会写入工程。');
  }
  function render(){
    if(viewport.hidden)return;
    pending=null;drag=null;positions.clear();cards.replaceChildren();const design=getDesign(),roots=design.nodes.filter(n=>n.parent===null);
    if(!roots.some(n=>n.id===selectedCard))selectedCard=null;
    for(const [index,root]of roots.entries()){
      const p=root.relationPosition??{x:32+(index%2)*310,y:40+Math.floor(index/2)*285};positions.set(root.id,{...p});
      const card=element('article','relation-card');card.dataset.root=root.id;card.style.left=p.x+'px';card.style.top=p.y+'px';card.setAttribute('aria-label','UI 卡片 '+root.name);
      card.tabIndex=0;card.classList.toggle('selected-card',selectedCard===root.id);
      const selectCard=()=>{selectedEdge=null;selectedCard=root.id;for(const item of cards.children)item.classList.toggle('selected-card',item.dataset.root===root.id);onSelect(root.id);showInfo();drawWires();};
      card.onclick=selectCard;
      card.onkeydown=e=>{if(e.target===card&&['Enter',' '].includes(e.key)){e.preventDefault();selectCard();}};
      const header=element('div','relation-card-header');header.title='拖动调整卡片位置';header.append(element('strong','',root.name));
      const editButton=element('button','','编辑');editButton.onclick=()=>onEdit(root.id);const play=element('button','','预演');play.onclick=()=>onPreview(root.id);header.append(editButton,play);
      header.onpointerdown=e=>{if(e.button!==0||e.target.closest('button'))return;e.preventDefault();const q=point(e);drag={id:root.id,start:q,original:{...p},card};header.setPointerCapture(e.pointerId);};card.append(header);
      const thumb=element('div','relation-thumbnail');thumb.inert=true;thumb.setAttribute('aria-hidden','true');
      const stage=element('div','relation-mini-stage');const factor=Math.min(216/design.width,128/design.height);stage.style.width=design.width+'px';stage.style.height=design.height+'px';stage.style.transform=`scale(${factor})`;stage.style.left=(216-design.width*factor)/2+'px';stage.style.top=(128-design.height*factor)/2+'px';
      const original=[...document.querySelectorAll('#canvas [data-node]')].find(e=>e.dataset.node===root.id);
      if(original){const clone=original.cloneNode(true),visibility=thumbnailVisibility(design,root.id);for(const el of [clone,...clone.querySelectorAll('*')]){if(el.dataset.node)el.hidden=!visibility.get(el.dataset.node);el.removeAttribute('id');el.classList.remove('selected');el.removeAttribute('data-node');el.removeAttribute('tabindex');}stage.append(clone);}
      thumb.append(stage);card.append(thumb);
      const row=element('div','relation-controls');const source=document.createElement('select');source.setAttribute('aria-label',root.name+' 触发节点');
      const candidates=design.nodes.filter(n=>systemRoot(design,n.id)?.id===root.id&&(n.id===root.id||n.type==='Button'||n.action));
      for(const n of candidates){const option=document.createElement('option');option.value=n.id;option.textContent=n.id===root.id?'点击界面':n.name;source.append(option);}
      source.value=candidates.some(n=>n.id===selections.get(root.id)?.source)?selections.get(root.id).source:(candidates.find(n=>n.type==='Button')?.id??root.id);
      const action=document.createElement('select');action.setAttribute('aria-label',root.name+' 连线动作');for(const [value,label]of Object.entries(labels)){const option=document.createElement('option');option.value=value;option.textContent=label;action.append(option);}action.value=selections.get(root.id)?.type??'navigate';
      const remember=()=>{selections.set(root.id,{source:source.value,type:action.value});pending=null;drawWires();};source.onchange=action.onchange=remember;row.append(source,action);card.append(row);
      const incoming=element('button','relation-port relation-in');incoming.dataset.destination=root.id;incoming.title='连接到 '+root.name;incoming.setAttribute('aria-label','连接到 '+root.name);incoming.onclick=()=>complete(root.id);
      const outgoing=element('button','relation-port relation-out');outgoing.title='从 '+root.name+' 连出';outgoing.setAttribute('aria-label','从 '+root.name+' 连出');
      const begin=()=>{pending={root:root.id,source:source.value,type:action.value};selectedEdge=null;showInfo();notice('选择目标卡片左侧圆点，或按 Esc 取消。');drawWires();};outgoing.onpointerdown=e=>{if(e.button===0){e.preventDefault();begin();}};outgoing.onclick=()=>{if(!pending)begin();};
      card.append(incoming,outgoing);cards.append(card);
    }
    plane.style.width=Math.max(640,...[...positions.values()].map(p=>p.x+310))+'px';plane.style.height=Math.max(420,...[...positions.values()].map(p=>p.y+290))+'px';
    document.getElementById('relations-empty').hidden=roots.length>0;showInfo();drawWires();
  }
  viewport.addEventListener('pointermove',e=>{
    if(pan){if(e.pointerId!==pan.id)return;e.preventDefault();panX=pan.startX+e.clientX-pan.x;panY=pan.startY+e.clientY-pan.y;applyPan();if(pending)drawWires(point(e));return;}
    if(drag){const q=point(e),p={x:Math.max(24,Math.min(6000,drag.original.x+q.x-drag.start.x)),y:Math.max(24,Math.min(6000,drag.original.y+q.y-drag.start.y))};positions.set(drag.id,p);drag.card.style.left=p.x+'px';drag.card.style.top=p.y+'px';drawWires();}
    else if(pending)drawWires(point(e));
  });
  window.addEventListener('pointerup',e=>{
    if(pan&&e.pointerId===pan.id){finishPan();return;}
    if(e.button!==0)return;
    if(drag){const {id,original}=drag,p=positions.get(id);drag=null;selectedCard=id;onSelect(id);if(p.x!==original.x||p.y!==original.y)edit(()=>{getDesign().nodes.find(n=>n.id===id).relationPosition={x:Math.round(p.x),y:Math.round(p.y)};});}
    else if(pending){const destination=document.elementFromPoint(e.clientX,e.clientY)?.closest('[data-destination]');if(destination)complete(destination.dataset.destination);}
  });
  window.addEventListener('pointercancel',()=>{finishPan();drag=null;pending=null;if(!viewport.hidden)render();});
  window.addEventListener('keydown',e=>{
    if(viewport.hidden||e.isComposing||e.ctrlKey||e.metaKey||e.altKey||e.target.closest('input,select,textarea,[contenteditable]:not([contenteditable="false"]),[role="textbox"],dialog'))return;
    if(e.key==='Escape'){pending=null;drawWires();}
    if(e.key!=='Delete'||e.repeat||drag||pan)return;
    if(selectedEdge){e.preventDefault();info.querySelector('button')?.click();}
    else if(selectedCard){e.preventDefault();pending=null;onDelete(selectedCard);}
  });
  function dropPosition(x,y){
    const r=viewport.getBoundingClientRect();
    if(viewport.hidden||x<r.left||x>r.right||y<r.top||y>r.bottom)return null;
    const p=point({clientX:x,clientY:y});
    return {x:Math.max(24,Math.min(6000,Math.round(p.x-116))),y:Math.max(24,Math.min(6000,Math.round(p.y-100)))};
  }
  function selectCard(id){selectedEdge=null;selectedCard=id;onSelect(id);render();}
  return {render,dropPosition,selectCard};
}
