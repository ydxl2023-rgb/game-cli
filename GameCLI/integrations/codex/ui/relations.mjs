import {createRelationGroups} from './relation-groups.mjs';
import {relations,systemRoot,connectUi,thumbnailVisibility,relationSource,disconnectUi,startupPortId} from '../shared/relations.mjs';
const labels={navigate:'跳转',show:'打开',toggle:'切换'};
export function createRelations({getDesign,getProjectId=()=>null,edit,notice,onEdit,onPreview,onSelect=()=>{},onDelete=()=>{}}){
  const viewport=document.getElementById('relations-view'),plane=document.getElementById('relations-plane');
  const svg=document.getElementById('relations-wires'),cards=document.getElementById('relations-cards');
  const positions=new Map(),draftPorts=new Set();let pending=null,drag=null,selectedEdge=null,selectedCard=null;
  let zoom=1,pan=null,panX=0,panY=0,currentProject=null;
  function saveView(){
    if(!currentProject||currentProject!==getProjectId()||viewport.hidden)return;
    // Store the effective screen offset so restoration is independent of scroll bounds.
    try{localStorage.setItem('gamecli.relations.view.'+currentProject,JSON.stringify({zoom,x:panX-viewport.scrollLeft,y:panY-viewport.scrollTop}));}catch{}
  }
  function restoreView(){
    const id=getProjectId();if(id===currentProject)return;
    currentProject=id;zoom=1;panX=panY=0;pan=null;
    try{const v=JSON.parse(localStorage.getItem('gamecli.relations.view.'+id));
      if(v&&Number.isFinite(v.zoom)&&Number.isFinite(v.x)&&Number.isFinite(v.y)){zoom=Math.max(.15,Math.min(3,v.zoom));panX=v.x;panY=v.y;}
    }catch{}
    viewport.scrollLeft=viewport.scrollTop=0;applyZoom();
  }
  function applyPan(){plane.style.transform=`translate(${panX/zoom}px, ${panY/zoom}px)`;viewport.style.backgroundPosition=`${panX-viewport.scrollLeft}px ${panY-viewport.scrollTop}px`;}
  const zoomButton=document.createElement('button');zoomButton.id='relations-zoom';zoomButton.type='button';zoomButton.title='关系图缩放 · 点击恢复 100%';zoomButton.setAttribute('aria-label','重置关系图缩放');document.getElementById('relations-tools').append(zoomButton);
  function applyZoom(){plane.style.zoom=String(zoom);viewport.style.backgroundSize=`${20*zoom}px ${20*zoom}px`;zoomButton.textContent=Math.round(zoom*100)+'%';applyPan();}
  zoomButton.onclick=()=>{zoom=1;panX=panY=0;viewport.scrollLeft=0;viewport.scrollTop=0;applyZoom();saveView();};
  viewport.addEventListener('wheel',e=>{
    e.preventDefault();if(drag||pan||groups.selecting)return;
    const p=point(e),delta=e.deltaY*(e.deltaMode===1?16:e.deltaMode===2?viewport.clientHeight:1);
    zoom=Math.max(.25,Math.min(3,zoom*Math.exp(-Math.max(-400,Math.min(400,delta))*.002)));
    applyZoom();const r=plane.getBoundingClientRect();
    viewport.scrollLeft+=r.left+p.x*zoom-e.clientX;viewport.scrollTop+=r.top+p.y*zoom-e.clientY;
    if(pending)drawWires(point(e));saveView();
  },{passive:false});
  applyZoom();
  viewport.addEventListener('scroll',()=>{applyPan();saveView();});
  viewport.addEventListener('pointerdown',e=>{
    if(e.button!==1||drag||groups.selecting)return;
    e.preventDefault();e.stopPropagation();
    pan={id:e.pointerId,x:e.clientX,y:e.clientY,startX:panX,startY:panY};
    viewport.setPointerCapture(e.pointerId);viewport.classList.add('panning');
  },true);
  function finishPan(){if(!pan)return;saveView();const id=pan.id;pan=null;viewport.classList.remove('panning');if(viewport.hasPointerCapture(id))viewport.releasePointerCapture(id);}
  viewport.addEventListener('auxclick',e=>{if(e.button===1)e.preventDefault();});
  viewport.addEventListener('lostpointercapture',finishPan);
  window.addEventListener('blur',finishPan);
  document.getElementById('relations-tools').querySelector('span:nth-child(2)').textContent='左键框选 · Shift 追加 · 中键平移 · 滚轮缩放';
  function element(tag,cls,text){const el=document.createElement(tag);el.className=cls;if(text)el.textContent=text;return el;}
  function svgEl(tag,attrs){const el=document.createElementNS('http://www.w3.org/2000/svg',tag);for(const [k,v]of Object.entries(attrs))el.setAttribute(k,String(v));return el;}
  function point(event){const r=plane.getBoundingClientRect();return {x:(event.clientX-r.left)/zoom,y:(event.clientY-r.top)/zoom};}
  function port(id,side,sourceId,type){
    const button=[...cards.querySelectorAll('.relation-port')].find(b=>!b.closest('[hidden]')&&(side==='out'?b.dataset.source===sourceId:b.dataset.destination===id));
    if(button){const r=button.getBoundingClientRect(),base=plane.getBoundingClientRect();return {x:(r.left+r.width/2-base.left)/zoom,y:(r.top+r.height/2-base.top)/zoom};}
    const p=positions.get(id);return {x:p.x+(side==='out'?232:0),y:p.y+194};
  }
  function path(a,b){const bend=Math.max(60,Math.abs(b.x-a.x)/2);return `M ${a.x} ${a.y} C ${a.x+bend} ${a.y}, ${b.x-bend} ${b.y}, ${b.x} ${b.y}`;}
  function drawWires(pointer){
    svg.replaceChildren();const defs=svgEl('defs',{}),marker=svgEl('marker',{id:'relation-arrow',viewBox:'0 0 10 10',refX:9,refY:5,markerWidth:6,markerHeight:6,orient:'auto'});marker.append(svgEl('path',{d:'M 0 0 L 10 5 L 0 10 z',fill:'#c4ef91'}));defs.append(marker);svg.append(defs);
    const design=getDesign(),edges=relations(design);
    for(const edge of edges){
      if(!positions.has(edge.sourceRoot)||!positions.has(edge.targetRoot))continue;
      const a=port(edge.sourceRoot,'out',edge.sourceId),b=port(edge.targetRoot,'in',null,edge.type);
      const d=edge.sourceRoot===edge.targetRoot?`M ${a.x} ${a.y} C ${a.x+85} ${a.y-300}, ${b.x-85} ${b.y-300}, ${b.x} ${b.y}`:path(a,b);
      const line=svgEl('path',{d,class:'relation-wire'+(selectedEdge===edge.sourceId?' selected':''),'marker-end':'url(#relation-arrow)'});
      const hit=svgEl('path',{d,class:'relation-wire-hit',tabindex:0,role:'button','aria-label':'选择连线 '+(relationSource(design,edge.sourceId)?.name??edge.sourceId)});
      const choose=()=>{selectedCard=null;for(const item of cards.children)item.classList.remove('selected-card');selectedEdge=edge.sourceId;showInfo();drawWires();};hit.onclick=choose;hit.onkeydown=e=>{if(e.key==='Enter'||e.key===' '){e.preventDefault();choose();}};
      svg.append(line,hit);
      const label=svgEl('text',{x:(a.x+b.x)/2,y:(a.y+b.y)/2-10,class:'relation-edge-label'});label.textContent=(relationSource(design,edge.sourceId)?.name??edge.sourceId)+' · '+(edge.trigger==='startupComplete'?'激活':labels[edge.type]);svg.append(label);
    }
    if(pending&&positions.has(pending.root))svg.append(svgEl('path',{d:path(port(pending.root,'out',pending.source),pointer??port(pending.root,'out',pending.source)),class:'relation-wire pending'}));
    document.getElementById('relations-count').textContent=`${positions.size} 个系统 · ${design.relationGroups?.length??0} 组 · ${edges.length} 条关系`;
  }
  function showInfo(){
    if(!relations(getDesign()).some(edge=>edge.sourceId===selectedEdge))selectedEdge=null;
  }
  function deleteSelectedEdge(){
    const id=selectedEdge;if(!id)return;
    if(edit(()=>{disconnectUi(getDesign(),id);})){draftPorts.delete(id);selectedEdge=null;drawWires();}
  }
  function complete(rootId,type='show'){
    if(!pending)return;const {source}=pending;pending=null;
    const old=relationSource(getDesign(),source)?.action;
    if(old?.target&&(old.target!==rootId||old.type!==type)&&!window.confirm('这个触发端口已有动作，是否替换原连线？')){drawWires();return;}
    if(!edit(()=>connectUi(getDesign(),source,rootId,type)))return;
    draftPorts.delete(source);selectedEdge=source;showInfo();drawWires();notice(relationSource(getDesign(),source)?.trigger==='startupComplete'?'已配置启动完成后激活目标系统。保存后写入当前项目。':'已配置独立按钮动作：'+labels[type]+'。保存后写入当前项目。');
  }
  function triggerName(design,node,root){
    if(node.trigger==='startupComplete')return '启动完成';
    if(node.id===root.id)return root.systemKind==='code'?'系统触发（仅记录）':'点击界面';
    const queue=[node.id];
    for(let i=0;i<queue.length;i++)for(const child of design.nodes.filter(n=>n.parent===queue[i])){
      if(child.type==='Text'&&child.text?.trim()&&child.text.trim().length<=20)return child.text.trim();
      queue.push(child.id);
    }
    return node.name;
  }
  function chooseTrigger(root,candidates){
    const dialog=element('dialog','relation-picker');dialog.setAttribute('aria-label',root.name+' 添加交互');
    const heading=element('h3','',root.name+' · 添加交互');
    const search=element('input','');search.type='search';search.placeholder='搜索按钮名称或控件 ID';search.setAttribute('aria-label','搜索交互');
    const list=element('div','relation-picker-list');
    const update=()=>{
      list.replaceChildren();const query=search.value.trim().toLowerCase();
      for(const node of candidates){
        const name=triggerName(getDesign(),node,root);
        if(![name,node.name,node.id].some(v=>v.toLowerCase().includes(query)))continue;
        const choice=element('button','relation-picker-choice');choice.append(element('strong','',name),element('small','',node.name+' · '+node.id));
        const connected=!!node.action?.target;choice.disabled=connected||draftPorts.has(node.id);
        if(choice.disabled)choice.append(element('small','',connected?'已连接':'已添加'));
        choice.onclick=()=>{draftPorts.add(node.id);dialog.close();render();notice('已添加“'+name+'”端口，请从右侧圆点连接目标。');};list.append(choice);
      }
      if(!list.children.length)list.append(element('p','','没有匹配的交互'));
    };
    search.oninput=update;const cancel=element('button','','取消');cancel.onclick=()=>dialog.close();
    dialog.append(heading,search,list,cancel);dialog.onclose=()=>dialog.remove();document.body.append(dialog);update();dialog.showModal();search.focus();
  }
  function render(){
    if(viewport.hidden)return;
    restoreView();
    pending=null;drag=null;positions.clear();cards.replaceChildren();const design=getDesign(),roots=design.nodes.filter(n=>n.parent===null);
    const connectedSources=new Set(relations(design).map(edge=>edge.sourceId));
    if(!roots.some(n=>n.id===selectedCard))selectedCard=null;
    for(const [index,root]of roots.entries()){
      const p=root.relationPosition??{x:32+(index%2)*310,y:40+Math.floor(index/2)*330};positions.set(root.id,{...p});
      const card=element('article','relation-card');card.dataset.root=root.id;card.dataset.scale=root.relationScale??1;card.style.transform=`scale(${root.relationScale??1})`;card.style.transformOrigin='top left';card.style.left=p.x+'px';card.style.top=p.y+'px';card.setAttribute('aria-label','UI 卡片 '+root.name);
      card.tabIndex=0;card.classList.toggle('selected-card',selectedCard===root.id);
      const selectCard=()=>{selectedEdge=null;selectedCard=root.id;for(const item of cards.children)item.classList.toggle('selected-card',item.dataset.root===root.id);onSelect(root.id);showInfo();drawWires();};
      card.onclick=selectCard;
      card.onkeydown=e=>{if(e.target===card&&['Enter',' '].includes(e.key)){e.preventDefault();selectCard();}};
      const header=element('div','relation-card-header');header.title='拖动调整卡片位置';header.append(groups.checkbox(root),element('strong','',root.name));
      const editButton=element('button','',root.systemKind==='code'?'代码':'编辑');editButton.onclick=()=>root.systemKind==='code'?selectCard():onEdit(root.id);header.append(editButton);if(root.systemKind!=='code'){const play=element('button','','预演');play.onclick=()=>onPreview(root.id);header.append(play);}
      header.onpointerdown=e=>{if(e.button!==0||e.target.closest('button,input'))return;e.preventDefault();const q=point(e);drag={id:root.id,start:q,original:{...p},card};header.setPointerCapture(e.pointerId);};card.append(header);
      const thumb=element('div','relation-thumbnail');thumb.inert=true;thumb.setAttribute('aria-hidden','true');
      const stage=element('div','relation-mini-stage');const factor=Math.min(216/design.width,128/design.height);stage.style.width=design.width+'px';stage.style.height=design.height+'px';stage.style.transform=`scale(${factor})`;stage.style.left=(216-design.width*factor)/2+'px';stage.style.top=(128-design.height*factor)/2+'px';
      const original=[...document.querySelectorAll('#canvas [data-node]')].find(e=>e.dataset.node===root.id);
      if(original){const clone=original.cloneNode(true),visibility=thumbnailVisibility(design,root.id);for(const el of [clone,...clone.querySelectorAll('*')]){if(el.dataset.node)el.hidden=!visibility.get(el.dataset.node);el.removeAttribute('id');el.classList.remove('selected');el.removeAttribute('data-node');el.removeAttribute('tabindex');}stage.append(clone);}
      if(root.systemKind==='code'){thumb.append(element('strong','','代码系统'),element('p','',`${root.scripts?.files?.length??0} 个源码文件`),element('small','','选择卡片查看源码 · 连线仅记录关系'));thumb.style.cssText+='display:flex;flex-direction:column;align-items:center;justify-content:center;';}else thumb.append(stage);card.append(thumb);
      const incoming=element('button','relation-port relation-in relation-card-input');incoming.dataset.destination=root.id;
      incoming.title='连接到 '+root.name+'（打开 / 激活）';incoming.setAttribute('aria-label','连接到 '+root.name);
      incoming.onclick=e=>{e.stopPropagation();complete(root.id);};card.append(incoming);
      const outputs=element('div','relation-outputs');
      const candidates=design.nodes.filter(n=>systemRoot(design,n.id)?.id===root.id&&(n.type==='Button'||n.action||n.id===root.id)&&(!n.action?.target||connectedSources.has(n.id)));
      if(root.startupComplete!==undefined)candidates.unshift(relationSource(design,startupPortId(root.id)));
      for(const n of candidates.filter(n=>n.trigger==='startupComplete'||connectedSources.has(n.id)||draftPorts.has(n.id))){
        const row=element('div','relation-port-row');row.dataset.trigger=n.id;
        const name=triggerName(design,n,root);
        const label=element('button','relation-trigger-label',name);label.title=n.trigger==='startupComplete'?'启动流程完成时触发；选择卡片或进度达到100%不会触发':name+' · '+n.id+' · 点击定位控件';
        label.onclick=e=>{e.stopPropagation();if(n.trigger==='startupComplete'){selectCard();notice('由启动流程完成事件触发，激活连线目标系统。');}else onEdit(n.id);};row.append(label);
        const badge=element('small','relation-port-status',n.action?.target?(n.trigger==='startupComplete'?'激活':labels[n.action.type]??'已配置'):'未连接');row.append(badge);
        const outgoing=element('button','relation-port relation-out');outgoing.dataset.source=n.id;
        const eventLabel=n.trigger==='startupComplete'?'完成事件':'Click';
        outgoing.title=name+' · '+eventLabel;outgoing.setAttribute('aria-label','从 '+root.name+' / '+name+' · '+eventLabel+' 连出');
        const begin=()=>{pending={root:root.id,source:n.id};selectedEdge=null;showInfo();notice('从“'+name+'”连接到目标卡片的输入圆点，打开 / 激活目标系统。Esc 取消。');drawWires();};
        outgoing.onpointerdown=e=>{if(e.button===0){e.preventDefault();e.stopPropagation();begin();}};
        outgoing.onclick=e=>{e.stopPropagation();if(!pending)begin();};row.append(outgoing);outputs.append(row);
      }
      card.append(outputs);
      if(root.startupComplete!==undefined)outputs.append(element('small','relation-port-status','启动流程完成后触发'));
      const add=element('button','relation-add','＋ 添加交互');add.setAttribute('aria-label',root.name+' 添加交互');
      add.onclick=e=>{e.stopPropagation();chooseTrigger(root,candidates);};card.append(add);cards.append(card);
    }
    groups.draw();
    plane.style.width=Math.max(640,...[...cards.children].map(c=>parseFloat(c.style.left)+c.offsetWidth*Number(c.dataset.scale??1)+45))+'px';plane.style.height=Math.max(420,...[...cards.children].map(c=>parseFloat(c.style.top)+c.offsetHeight*Number(c.dataset.scale??1)+45))+'px';
    document.getElementById('relations-empty').hidden=roots.length>0;showInfo();drawWires();
  }
  viewport.addEventListener('pointermove',e=>{
    if(pan){if(e.pointerId!==pan.id)return;e.preventDefault();panX=pan.startX+e.clientX-pan.x;panY=pan.startY+e.clientY-pan.y;applyPan();if(pending)drawWires(point(e));return;}
    if(drag){const q=point(e),p={x:Math.max(24,Math.min(6000,drag.original.x+q.x-drag.start.x)),y:Math.max(24,Math.min(6000,drag.original.y+q.y-drag.start.y))};positions.set(drag.id,p);drag.card.style.left=p.x+'px';drag.card.style.top=p.y+'px';groups.updateBounds();drawWires();}
    else if(pending)drawWires(point(e));
  });
  window.addEventListener('pointerup',e=>{
    if(pan&&e.pointerId===pan.id){finishPan();return;}
    if(e.button!==0)return;
    if(drag){const {id,original}=drag,p=positions.get(id);drag=null;selectedCard=id;onSelect(id);if(p.x!==original.x||p.y!==original.y)edit(()=>{getDesign().nodes.find(n=>n.id===id).relationPosition={x:Math.round(p.x),y:Math.round(p.y)};});}
    else if(pending){const destination=document.elementFromPoint(e.clientX,e.clientY)?.closest('[data-destination]');if(destination)complete(destination.dataset.destination,destination.dataset.operation);}
  });
  window.addEventListener('pointercancel',()=>{finishPan();drag=null;pending=null;if(!viewport.hidden)render();});
  window.addEventListener('keydown',e=>{
    if(viewport.hidden||e.isComposing||e.ctrlKey||e.metaKey||e.altKey||e.target.closest('input,select,textarea,[contenteditable]:not([contenteditable="false"]),[role="textbox"],dialog'))return;
    if(e.key==='Escape'){pending=null;drawWires();}
    if(e.key!=='Delete'||e.repeat||drag||pan)return;
    if(selectedEdge){e.preventDefault();deleteSelectedEdge();}
    else if(selectedCard){e.preventDefault();pending=null;onDelete(selectedCard);}
  });
  const groups=createRelationGroups({getDesign,edit,notice,cards,positions,point,drawWires,render,canSelect:()=>!pending&&!drag&&!pan});
  const fitButton=element('button','','适应');fitButton.setAttribute('aria-label','适应全部关系卡片');
  fitButton.onclick=()=>{
    const visible=[...cards.children].filter(c=>!c.hidden);if(!visible.length)return;
    const left=Math.min(...visible.map(c=>parseFloat(c.style.left))),top=Math.min(...visible.map(c=>parseFloat(c.style.top)));
    const right=Math.max(...visible.map(c=>parseFloat(c.style.left)+c.offsetWidth*Number(c.dataset.scale??1))),bottom=Math.max(...visible.map(c=>parseFloat(c.style.top)+c.offsetHeight*Number(c.dataset.scale??1)));
    zoom=Math.max(.15,Math.min(1,(viewport.clientWidth-40)/(right-left),(viewport.clientHeight-40)/(bottom-top)));panX=20-left*zoom;panY=20-top*zoom;viewport.scrollLeft=viewport.scrollTop=0;applyZoom();drawWires();saveView();
  };document.getElementById('relations-tools').append(fitButton);
  function dropPosition(x,y){
    const r=viewport.getBoundingClientRect();
    if(viewport.hidden||x<r.left||x>r.right||y<r.top||y>r.bottom)return null;
    const p=point({clientX:x,clientY:y});
    return {x:Math.max(24,Math.min(6000,Math.round(p.x-116))),y:Math.max(24,Math.min(6000,Math.round(p.y-100)))};
  }
  function selectCard(id){selectedEdge=null;selectedCard=id;onSelect(id);render();}
  return {render,dropPosition,selectCard};
}
