import {moveGroup,resizeGroupFrame,fitGroupCards} from '../shared/relation-groups.mjs';
import {selectionRect,intersectsSelection} from '../shared/marquee.mjs';

export function createRelationGroups({getDesign,edit,notice,cards,positions,point,drawWires,render,canSelect=()=>true}){
  const selected=new Set();let drag=null,resize=null,selectedGroup=null;
  const viewport=document.getElementById('relations-view'),plane=document.getElementById('relations-plane');
  let marquee=null;
  function el(tag,cls,text){const n=document.createElement(tag);n.className=cls;if(text)n.textContent=text;return n;}
  function button(text,run){const b=el('button','',text);b.type='button';b.onclick=e=>{e.stopPropagation();run();};return b;}
  function selectGroup(id){selectedGroup=id;for(const box of cards.querySelectorAll('.relation-group'))box.classList.toggle('selected-group',box.dataset.group===id);}
  viewport.addEventListener('pointerdown',e=>{if(e.button===0&&!e.target.closest('.relation-group'))selectGroup(null);},true);
  function configure(group){
    const d=getDesign(),dialog=el('dialog','relation-picker'),name=el('input','');
    dialog.setAttribute('aria-label',group?'编辑分组':'创建分组');name.setAttribute('aria-label','分组名称');name.maxLength=60;name.value=group?.name??'新分组';
    const members=new Set(group?.members??selected),list=el('div','relation-picker-list'),error=el('p','error');error.setAttribute('role','status');
    for(const root of d.nodes.filter(n=>n.parent===null)){
      const owner=d.relationGroups?.find(g=>g.id!==group?.id&&g.members.includes(root.id));
      const label=el('label','relation-group-choice'),check=el('input','');check.type='checkbox';check.checked=members.has(root.id)&&!owner;check.disabled=!!owner;
      if(owner)members.delete(root.id);
      check.onchange=()=>check.checked?members.add(root.id):members.delete(root.id);
      label.append(check,document.createTextNode(root.name+(owner?' · 已属于 '+owner.name:'')));list.append(label);
    }
    const save=button('确定',()=>{
      if(!name.value.trim()||!members.size){error.textContent='请输入分组名称，并至少选择一个系统。';return;}
      const saved={id:group?.id??'group-'+crypto.randomUUID(),name:name.value.trim(),members:[...members],...(group?.frame?{frame:{...group.frame}}:{})};
      if(edit(()=>{const design=getDesign();design.relationGroups=[...(design.relationGroups??[]).filter(g=>g.id!==saved.id),saved];})){selected.clear();dialog.close();render();notice('分组已更新，保存项目后生效。');}
    });
    dialog.append(el('h3','',group?'编辑分组':'创建分组'),name,list,error,save,button('取消',()=>dialog.close()));
    dialog.onclose=()=>dialog.remove();document.body.append(dialog);dialog.showModal();name.focus();
  }
  const add=button('＋ 分组',()=>configure());add.setAttribute('aria-label','创建关系分组');add.title='勾选卡片后创建分组，也可在弹窗中选择系统';document.getElementById('relations-tools').append(add);
  function showSelection(){
    for(const card of cards.children){if(!card.dataset.root)continue;const checked=selected.has(card.dataset.root);card.classList.toggle('group-selected',checked);const input=card.querySelector('.relation-group-select');if(input)input.checked=checked;}
    add.textContent=selected.size?'＋ 分组（'+selected.size+'）':'＋ 分组';
  }
  viewport.addEventListener('pointerdown',e=>{
    if(e.button!==0||drag||resize||!canSelect()||e.target.closest('.relation-card,.relation-group-header,button,input,.relation-wire-hit'))return;
    const bounds=viewport.getBoundingClientRect();
    if(e.clientX>=bounds.left+viewport.clientWidth||e.clientY>=bounds.top+viewport.clientHeight)return;
    e.preventDefault();const box=el('div','relation-marquee');box.setAttribute('aria-hidden','true');plane.append(box);
    marquee={id:e.pointerId,start:point(e),client:{x:e.clientX,y:e.clientY},base:new Set(e.shiftKey?selected:[]),previous:new Set(selected),box,moved:false};
    viewport.setPointerCapture(e.pointerId);
  });
  viewport.addEventListener('pointermove',e=>{
    if(!marquee||e.pointerId!==marquee.id)return;
    if(Math.hypot(e.clientX-marquee.client.x,e.clientY-marquee.client.y)<4&&!marquee.moved)return;
    marquee.moved=true;const rect=selectionRect(marquee.start,point(e));
    Object.assign(marquee.box.style,{left:rect.x+'px',top:rect.y+'px',width:rect.width+'px',height:rect.height+'px'});
    selected.clear();for(const id of marquee.base)selected.add(id);
    const grouped=new Set((getDesign().relationGroups??[]).flatMap(g=>g.members));
    for(const card of cards.children){const id=card.dataset.root;if(!id||card.hidden||grouped.has(id))continue;const p=positions.get(id);if(intersectsSelection(rect,{...p,width:card.offsetWidth*Number(card.dataset.scale??1),height:card.offsetHeight*Number(card.dataset.scale??1)}))selected.add(id);}
    showSelection();
  });
  function finishSelection(cancelled=false){
    if(!marquee)return;const value=marquee;marquee=null;value.box.remove();
    if(cancelled){selected.clear();for(const id of value.previous)selected.add(id);}else if(!value.moved){selected.clear();for(const id of value.base)selected.add(id);}
    if(viewport.hasPointerCapture(value.id))viewport.releasePointerCapture(value.id);showSelection();
    if(!cancelled&&value.moved)notice(selected.size?'已选中 '+selected.size+' 张卡片，点击“＋ 分组”创建分组。':'未框选到可分组卡片；已有分组的成员请通过组的“编辑”调整。');
  }
  window.addEventListener('pointerup',e=>{if(marquee&&e.pointerId===marquee.id)finishSelection();});
  viewport.addEventListener('lostpointercapture',()=>finishSelection(true));
  window.addEventListener('pointercancel',()=>finishSelection(true));
  window.addEventListener('blur',()=>finishSelection(true));
  window.addEventListener('keydown',e=>{if(e.key==='Escape'&&marquee){e.preventDefault();finishSelection(true);}});
  function checkbox(root){
    const input=el('input','relation-group-select');input.type='checkbox';input.checked=selected.has(root.id);input.setAttribute('aria-label','分组选择 '+root.name);
    input.onclick=e=>e.stopPropagation();input.onpointerdown=e=>e.stopPropagation();input.onchange=()=>{input.checked?selected.add(root.id):selected.delete(root.id);showSelection();};return input;
  }
  function bounds(group){
    const members=group.members.map(id=>{const p=positions.get(id),card=[...cards.children].find(c=>c.dataset.root===id);return {...p,width:232*Number(card?.dataset.scale??1),height:Number(card?.dataset.height??236)};});
    const x=Math.max(0,Math.min(...members.map(p=>p.x))-16),y=Math.max(0,Math.min(...members.map(p=>p.y))-52);
    return {x,y,width:Math.max(280,Math.max(...members.map(p=>p.x+p.width+16))-x),height:Math.max(120,Math.max(...members.map(p=>p.y+p.height+16))-y)};
  }
  function updateBounds(overrides=new Map()){
    for(const group of getDesign().relationGroups??[]){const box=[...cards.children].find(c=>c.dataset.group===group.id);if(!box)continue;const b=overrides.get(group.id)??group.frame??bounds(group);box.style.left=b.x+'px';box.style.top=b.y+'px';box.style.width=b.width+'px';box.style.height=b.height+'px';}
  }
  function draw(){
    const d=getDesign(),roots=new Map(d.nodes.map(n=>[n.id,n]));
    for(const id of selected)if(!roots.has(id))selected.delete(id);
    for(const card of cards.children){if(!card.dataset.root)continue;card.dataset.height=card.offsetHeight*Number(card.dataset.scale??1);card.hidden=false;}
    for(const group of d.relationGroups??[]){
      const box=el('section','relation-group');box.dataset.group=group.id;box.classList.toggle('selected-group',selectedGroup===group.id);box.setAttribute('aria-label','系统分组 '+group.name);
      const header=el('div','relation-group-header');header.append(el('strong','',group.name),el('small','',group.members.length+' 个系统'));
      header.append(button('编辑',()=>configure(group)),button('适应内容',()=>edit(()=>{delete getDesign().relationGroups.find(g=>g.id===group.id).frame;})),button('解散',()=>edit(()=>{getDesign().relationGroups=getDesign().relationGroups.filter(g=>g.id!==group.id);})));box.append(header);
      header.onpointerdown=e=>{if(e.button!==0||e.target.closest('button'))return;e.preventDefault();e.stopPropagation();selectGroup(group.id);drag={id:group.id,start:point(e),original:new Map([...positions].map(([id,p])=>[id,{...p}])),dx:0,dy:0};header.setPointerCapture(e.pointerId);};
      for(const [handle,label]of Object.entries({n:'上',s:'下',e:'右',w:'左',nw:'左上',ne:'右上',sw:'左下',se:'右下'})){
        const grip=el('button','relation-group-resize '+handle);grip.type='button';grip.setAttribute('aria-label','调整 '+group.name+' '+label+'边框');grip.title='拖动调整分组边框大小';
        grip.onpointerdown=e=>{if(e.button!==0||drag||!canSelect())return;e.preventDefault();e.stopPropagation();if(selectedGroup!==group.id){selectGroup(group.id);return;}resize={members:group.members.map(id=>{const c=[...cards.children].find(c=>c.dataset.root===id);return {id,...positions.get(id),width:c.offsetWidth,height:c.offsetHeight,scale:Number(c.dataset.scale??1)};}),id:group.id,pointer:e.pointerId,start:point(e),original:{...(group.frame??bounds(group))},handle};grip.setPointerCapture(e.pointerId);};
        grip.onclick=e=>e.stopPropagation();box.append(grip);
      }
      cards.prepend(box);
    }
    updateBounds();showSelection();
  }
  window.addEventListener('pointermove',e=>{
    if(resize){if(e.pointerId!==resize.pointer)return;const p=point(e);resize.frame=resizeGroupFrame(resize.original,resize.handle,p.x-resize.start.x,p.y-resize.start.y);resize.layout=fitGroupCards(resize.frame,resize.members);for(const item of resize.layout){const card=[...cards.children].find(c=>c.dataset.root===item.id);positions.set(item.id,{x:item.x,y:item.y});card.style.left=item.x+'px';card.style.top=item.y+'px';card.style.transform=`scale(${item.scale})`;}updateBounds(new Map([[resize.id,resize.frame]]));drawWires();return;}
    if(!drag)return;
    const q=point(e),d=structuredClone(getDesign());moveGroup(d,drag.id,drag.original,q.x-drag.start.x,q.y-drag.start.y);
    for(const id of d.relationGroups.find(g=>g.id===drag.id).members){const p=d.nodes.find(n=>n.id===id).relationPosition;positions.set(id,p);const card=[...cards.children].find(c=>c.dataset.root===id);card.style.left=p.x+'px';card.style.top=p.y+'px';}
    drag.dx=q.x-drag.start.x;drag.dy=q.y-drag.start.y;updateBounds(new Map(d.relationGroups.filter(g=>g.frame).map(g=>[g.id,g.frame])));drawWires();
  });
  window.addEventListener('pointerup',e=>{if(e.button!==0)return;if(resize&&e.pointerId===resize.pointer){const value=resize;resize=null;if(value.frame)edit(()=>{getDesign().relationGroups.find(g=>g.id===value.id).frame=value.frame;for(const item of value.layout){const node=getDesign().nodes.find(n=>n.id===item.id);node.relationPosition={x:item.x,y:item.y};node.relationScale=item.scale;}});return;}if(!drag)return;const value=drag;drag=null;if(value.dx||value.dy)edit(()=>moveGroup(getDesign(),value.id,value.original,value.dx,value.dy));});
  function cancel(){if(drag||resize){drag=null;resize=null;render();}}
  window.addEventListener('keydown',e=>{if(e.key==='Escape'&&(drag||resize)){e.preventDefault();cancel();}});
  window.addEventListener('pointercancel',cancel);window.addEventListener('blur',cancel);
  return {checkbox,draw,updateBounds,get selecting(){return !!marquee||!!resize||!!drag;}};
}
