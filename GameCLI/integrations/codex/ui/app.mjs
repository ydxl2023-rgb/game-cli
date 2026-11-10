import {createRelations} from './relations.mjs';
import {createCardScripts} from './card-scripts.mjs';
import {createCodePage} from './code-preview.mjs';
import {setupTemplateDock} from './template-dock.mjs';
import {upgradeTemplate} from '../shared/template-upgrade.mjs';
import {playAnimations} from './animation-player.mjs';
let stopAnimations=()=>{},centerMode='edit';
import {setupPanelSplit,setupWorkspaceSplit} from './panel-split.mjs';
import {groupedTemplates} from '../shared/template-groups.mjs';
import {localDelta} from '../shared/inspector.mjs';
import {createInspector} from './inspector.mjs';
import {resolveLayout, captureBounds, captureChanges, offsets, setOffset} from '../shared/rect-transform.mjs';
import { Bridge } from './bridge.mjs';
import { createPreviewState, defaultAction, runPreviewAction, hierarchyVisibility } from '../shared/preview.mjs';
import {instantiateTemplate} from '../shared/templates.mjs';
import { starter, validateDesign, phoneDesign, pointInDesign, appendControl, textTarget, resizeNode, resizedRect, layerStyle, nodeName } from '../shared/design.mjs';
const $ = id => document.getElementById(id);
const bridge = new Bridge();
let design = starter(), selected = 'EquipButton', revision = 0, dirty = false, scale = 1, preview = false, ready = false;
const history = [];
const collapsedNodes = new Set();
const library=new Map();
let librarySource=null,projectVersion=null;
let projectRole='viewer';
const draftKey=()=>`gamecli.project.${bridge.projectId}.draft`;
function persistDraft(){if(!bridge.projectId)return;try{sessionStorage.setItem(draftKey(),JSON.stringify({design,projectVersion}));}catch{notice('浏览器草稿空间不足，请及时保存工程。',true);}}
async function loadTemplates() {
  const area=$('template-library');
  const refresh=$('refresh-templates');refresh.disabled=true;refresh.setAttribute('aria-busy','true');
  try {
    const result=await bridge.call('templates');librarySource=result.source;
    const expanded=new Map([...area.querySelectorAll('[data-template-group]')].map(e=>[e.dataset.templateGroup,e.open]));
    area.replaceChildren();library.clear();
    for(const category of result.categories) {
      const panel=document.createElement('div');panel.dataset.libraryCategory=category.id;panel.id='library-panel-'+category.id;panel.setAttribute('role','tabpanel');panel.setAttribute('aria-labelledby','library-tab-'+category.id);area.append(panel);
      const heading=document.createElement('div');heading.className='section-title prefab-title';heading.textContent=category.name;
      const source=document.createElement('span');source.textContent=result.source==='postgresql'?'数据库':'内置';heading.append(source);panel.append(heading);
      for(const group of groupedTemplates(category.id,result.groups??[],result.templates)){
      const folder=document.createElement('details');folder.className='template-group';folder.dataset.templateGroup=category.id+'/'+group.id;folder.open=expanded.get(folder.dataset.templateGroup)??true;
      const summary=document.createElement('summary');const title=document.createElement('span');title.textContent=group.name;const count=document.createElement('span');count.className='group-count';count.textContent=group.templates.length;summary.append(title,count);folder.append(summary);panel.append(folder);
      for(const template of group.templates) {
        library.set(template.id,template);
        const button=document.createElement('button');button.className='control';button.style.width='100%';button.style.marginBottom='9px';button.dataset.control=template.id;button.setAttribute('aria-label','拖入'+template.name);button.disabled=!controlAllowed(template.id);
        const icon=document.createElement('span');icon.className='control-icon';icon.textContent=category.id==='system'?'▦':'▣';
        const label=document.createElement('span');label.textContent=template.name;const detail=document.createElement('small');detail.textContent=template.description;label.append(detail);button.append(icon,label);folder.append(button);bindControl(button);
      }
    }
    }
    refreshTemplateDock();
    cardScripts.render();
  } catch(e) {
    area.replaceChildren();const message=document.createElement('p');message.className='palette-note';message.textContent=e.message;area.append(message);
    const retry=document.createElement('button');retry.textContent='重新加载模板';retry.onclick=loadTemplates;area.append(retry);
  } finally {refresh.disabled=false;refresh.removeAttribute('aria-busy');}
}
let previewState = null;
function applyPreviewState() {
  const visibility = hierarchyVisibility(design, preview ? previewState.hidden : [], preview);
  for (const el of $('canvas').querySelectorAll('[data-node]')) el.hidden = !visibility.get(el.dataset.node);
  $('preview-back').hidden = $('preview-reset').hidden = !preview;
  $('preview-back').disabled = !previewState?.history.length;
}
function performAction(action) {
  previewState = runPreviewAction(design,previewState,action);
  applyPreviewState();
}
const names = { Container: '容器', Text: '文本', Button: '按钮', Image: '图片', Progress: '进度条', ItemSlot: '道具框', ScrollPanel: '道具滚动面板', ConfirmDialog: '确认对话框', InventorySystem: '背包系统' };
const roles = {background:'背景',icon:'图标',mask:'蒙版',count:'数量',frame:'外边框',content:'滚动内容'};
const icons = { Container: '▣', Text: 'T', Button: '▭', Image: '▧', Progress: '▰' };
const selectionBox = document.createElement('div'); selectionBox.id = 'selection-box'; selectionBox.hidden = true; $('canvas-wrap').append(selectionBox);
const handles = {nw:'左上', n:'上', ne:'右上', e:'右', se:'右下', s:'下', sw:'左下', w:'左'};
for (const [direction, label] of Object.entries(handles)) { const h = document.createElement('span'); h.className = `resize-handle ${direction}`; h.dataset.resize = direction; h.title = `${label}缩放`; h.setAttribute('aria-label', `${label}缩放`); selectionBox.append(h); }
function updateSelection() {
  const n = node(); selectionBox.hidden = !n || preview || n.active===false;
  if (!n || preview || n.active===false) return;
  const el=[...$('canvas').querySelectorAll('[data-node]')].find(e=>e.dataset.node===n.id);
  if(!el||!el.getClientRects().length){selectionBox.hidden=true;return;}
  const box=el.getBoundingClientRect(),wrap=$('canvas-wrap').getBoundingClientRect();
  Object.assign(selectionBox.style,{left:`${box.left-wrap.left}px`,top:`${box.top-wrap.top}px`,width:`${box.width}px`,height:`${box.height}px`});
  let current=n,unsupportedResize=false;while(current){unsupportedResize ||= !!current.rotation || (current.localScale?.x??1)<=0 || (current.localScale?.y??1)<=0;current=design.nodes.find(p=>p.id===current.parent);}
  for(const h of selectionBox.children)h.hidden=unsupportedResize;
}
function syncNode(n) {
  const el = [...$('canvas').querySelectorAll('[data-node]')].find(el => el.dataset.node === n.id);
  if (!el) return;
  Object.assign(el.style, {left:`${n.x}px`,top:`${n.y}px`,width:`${n.width}px`,height:`${n.height}px`,fontSize:`${n.fontSize}px`});
  if (n.type === 'Text') el.textContent = n.text;
}
function notice(text, error = false) { $('notice').textContent = text; $('notice').classList.toggle('error', error); }
function mark() { dirty = true; $('dirty').hidden = false; $('undo').disabled = !history.length; persistDraft(); }
function checkpoint() { history.push(JSON.stringify(design)); if (history.length > 60) history.shift(); }
function edit(fn) {
  if(ready&&projectRole==='viewer'){notice('当前项目为只读权限。',true);return;}
  const before = JSON.stringify(design);
  try { fn(); captureChanges(design,JSON.parse(before)); design = validateDesign(design); if(JSON.stringify(design)===before){render();return;} history.push(before); if (history.length > 60) history.shift(); mark(); render(); }
  catch (e) { design = JSON.parse(before); notice(e.message, true); render(); }
}
function node() { return design.nodes.find(n => n.id === selected); }
let manualScale = null;
function fit() {
  const box = $('viewport').getBoundingClientRect();
  scale = (!preview && manualScale !== null) ? manualScale : Math.max(.01, Math.min((box.width - 40) / design.width, (box.height - 40) / design.height, 1));
  $('canvas').style.transform = `scale(${scale})`;
  $('canvas-wrap').style.width = `${design.width * scale}px`; $('canvas-wrap').style.height = `${design.height * scale}px`;
  $('zoom').textContent = `${Math.round(scale * 100)}%`;
  updateSelection();
}
function select(id, preserveTree = false) { selected = id; $('current-ui').textContent='当前节点：'+(node()?.name??'Canvas'); $('inspector').open = true; render(preserveTree); }
function render(preserveTree = false) {
  stopAnimations();
  resolveLayout(design);
  renderRectInspector();
  $('design-title').textContent = 'Canvas';
  $('root-node').classList.toggle('selected', !node());
  $('resolution').textContent = `${design.width} × ${design.height}`;
  $('design-name').value = design.name;
  const preset = `${design.width}x${design.height}`;
  $('preset').value = ['1920x1080','1080x1920','1080x2400','1170x2532'].includes(preset) ? preset : 'custom';
  $('node-count').textContent = design.nodes.length + 1;
  if (!preserveTree) $('tree').replaceChildren();
  else for (const row of $('tree').children) {
    row.classList.toggle('selected', row.title === selected);
    row.setAttribute('aria-selected', row.title === selected);
  }
  $('canvas').replaceChildren();
  $('canvas').style.width = `${design.width}px`; $('canvas').style.height = `${design.height}px`;
  $('canvas').classList.toggle('preview', preview);
  const draw = (parent, target, depth, parentVisible=true, treeVisible=true) => {
    for (const n of design.nodes.filter(n => n.parent === parent)) {
      const visible=parentVisible && n.active!==false;
      const hasChildren=design.nodes.some(child=>child.parent===n.id);
      const collapsed=collapsedNodes.has(n.id);
      const layer=document.createElement('div');layer.className='tree-node'+(n.id===selected?' selected':'')+(!visible?' inactive':'');
      layer.hidden=!treeVisible;
      if(hasChildren)layer.setAttribute('aria-expanded',!collapsed);
      layer.style.paddingLeft=`${6+depth*10}px`;layer.setAttribute('role','treeitem');layer.setAttribute('aria-selected',n.id===selected);layer.setAttribute('aria-level',depth+1);layer.setAttribute('aria-label',icons[n.type]+' '+nodeName(n));layer.title=n.id;
      const toggle=document.createElement('input');toggle.type='checkbox';toggle.className='node-visibility';toggle.checked=visible;toggle.disabled=!parentVisible;toggle.dataset.visibility=n.id;
      toggle.setAttribute('aria-label','显示 '+nodeName(n));toggle.title=parentVisible?'控制节点及其子节点显示':'父节点已隐藏；重新显示父节点后恢复本节点原有状态';
      toggle.onclick=e=>e.stopPropagation();toggle.ondblclick=e=>e.stopPropagation();
      toggle.onchange=()=>{const checked=toggle.checked,scroll=$('tree').scrollTop;edit(()=>{n.active=checked;});$('tree').scrollTop=scroll;[...$('tree').querySelectorAll('[data-visibility]')].find(e=>e.dataset.visibility===n.id)?.focus({preventScroll:true});};
      const choose=document.createElement('button');choose.type='button';choose.className='node-select';choose.setAttribute('aria-label','选择 '+nodeName(n));
      const icon=document.createElement('span');icon.className='node-icon';icon.textContent=hasChildren?(collapsed?'▸':'▾'):icons[n.type];const label=document.createElement('span');label.className='node-label';label.textContent=nodeName(n);choose.append(icon,label);
      choose.title=hasChildren?'双击折叠 / 展开子节点':'选择节点';
      choose.onclick=()=>select(n.id,true);
      choose.ondblclick=()=>{
        if(!hasChildren)return;
        const scroll=$('tree').scrollTop;
        if(collapsedNodes.has(n.id))collapsedNodes.delete(n.id);else collapsedNodes.add(n.id);
        render();$('tree').scrollTop=scroll;
      };
      layer.append(toggle,choose);if(!preserveTree)$('tree').append(layer);
      const el = document.createElement(n.type === 'Button' ? 'button' : 'div'); el.className = 'ui-node' + (!preview && n.id === selected ? ' selected' : ''); el.dataset.node = n.id; el.dataset.name = nodeName(n); el.dataset.type = n.type;
      Object.assign(el.style, { left: `${n.x}px`, top: `${n.y}px`, width: `${n.width}px`, height: `${n.height}px`, backgroundColor: n.color, color: n.type === 'Text' ? n.color : '#101923', fontSize: `${n.fontSize}px` });
      if (design.nodes.find(p => p.id === n.parent)?.type === 'Button') Object.assign(el.style, { display: 'flex', alignItems: 'center', justifyContent: 'center', textAlign: 'center' });
      el.style.cssText += layerStyle(n);
      if(n.type==='Button')el.disabled=preview&&n.interactable===false;
      if (n.template === 'ScrollPanel') el.addEventListener('scroll', updateSelection);
      if (['frame','mask'].includes(n.role) && selected !== n.id) el.style.pointerEvents = 'none';
      if (preview) {
        const interactive = !!defaultAction(design,n) || n.type === 'Button' || n.template === 'ItemSlot';
        // Empty hierarchy containers must not intercept sibling buttons behind them.
        el.style.pointerEvents = interactive || n.template === 'ScrollPanel' ? 'auto' : 'none';
        if (interactive) { el.style.cursor='pointer'; el.dataset.interactive='true'; if(n.type!=='Button') { el.tabIndex=0; el.setAttribute('role','button'); } }
        if (n.template === 'ConfirmDialog') el.style.zIndex='100';
      }
      el.textContent = n.text; el.setAttribute('aria-label', nodeName(n)); target.append(el); draw(n.id, el, depth + 1, visible, treeVisible&&!collapsed);
    }
  };
  draw(null, $('canvas'), 0);
  $('preview-progress').hidden=!preview;
  $('preview-progress').replaceChildren();
  for(const progress of design.nodes.filter(n=>n.type==='Progress')) {
    updateProgress(progress.id,progress.progress??0);
    if(preview) {
      const label=document.createElement('label');label.textContent='模拟加载进度';label.style.margin='0';
      const slider=document.createElement('input');slider.type='range';slider.min='0';slider.max='100';slider.value=String(Math.round((progress.progress??0)*100));slider.setAttribute('aria-label','模拟加载进度');
      slider.oninput=()=>updateProgress(progress.id,Number(slider.value)/100);label.append(slider);$('preview-progress').append(label);
    }
  }
  const n = node(); $('properties').hidden = !n; $('empty-selection').hidden = !!n; $('selected-type').textContent = n ? names[n.template] ?? names[n.type] : '—';
  if (n) for (const key of ['id','x','y','width','height','color']) $('field-' + key).value = key === 'id' ? nodeName(n) : n[key];
  const text = textTarget(design, n);
  for (const key of ['text','fontSize']) { $('field-' + key).parentElement.hidden = !text; $('field-' + key).value = text?.[key] ?? ''; }
  $('field-text').placeholder = n?.type === 'Button' ? '按钮文字' : '输入文本，支持换行';
  $('field-opacity').value = n?.opacity ?? 1;
  $('progress-property').hidden=n?.type!=='Progress';$('field-progress').value=Math.round((n?.progress??0)*100);
  $('field-borderWidth').parentElement.hidden = n?.role !== 'frame'; $('field-borderWidth').value = n?.borderWidth ?? 6;
  const action = n && defaultAction(design,n);
  $('field-action').value = n?.action?.type ?? 'auto';
  $('field-action').options[0].textContent = action?.type==='hide' && !n?.action ? '默认：关闭所属对话框' : '默认点击反馈';
  $('action-target-label').hidden = !['show','hide','toggle','navigate'].includes(n?.action?.type);
  $('field-action-target').replaceChildren(...design.nodes.map(target=>new Option(nodeName(target)+' · '+target.id,target.id)));
  $('field-action-target').value = action?.target ?? n?.id ?? '';
  $('field-preview-hidden').checked = n?.previewHidden ?? n?.template === 'ConfirmDialog';
  applyPreviewState();
  $('undo').disabled = !history.length; fit();
  if(preview)stopAnimations=playAnimations(design,$('canvas'));
  relationView.render();
}
const renderRectInspector=createInspector({node,design:()=>design,edit});
function updateProgress(id,value) {
  const el=[...$('canvas').querySelectorAll('[data-node]')].find(e=>e.dataset.node===id);
  if(!el)return;
  el.setAttribute('role','progressbar');el.setAttribute('aria-valuenow',Math.round(value*100));el.setAttribute('aria-valuemin','0');el.setAttribute('aria-valuemax','100');
  for(const child of el.children) {
    if(child.dataset.name==='Fill')child.style.width=`${value*100}%`;
    if(child.dataset.name==='Percentage')child.textContent=`Loading... ${Math.round(value*100)}%`;
  }
}
$('field-progress').onchange=e=>{const value=Number(e.target.value)/100;edit(()=>{node().progress=value;});};
$('properties').onsubmit = e => e.preventDefault();
$('field-action').onchange = e => { const type=e.target.value; edit(()=>{ if(type==='auto') delete node().action; else node().action=['none','back'].includes(type)?{type}:{type,target:$('field-action-target').value || node().id}; }); };
$('field-action-target').onchange = e => { const target=e.target.value; edit(()=>{ if(node().action) node().action.target=target; }); };
$('field-preview-hidden').onchange = e => { const hidden=e.target.checked; edit(()=>{node().previewHidden=hidden;}); };
for (const key of ['opacity','borderWidth']) $('field-'+key).addEventListener('change', e => { if (node()) edit(()=>{node()[key]=Number(e.target.value);}); });
for (const key of ['id','color']) $('field-' + key).onchange = e => {
  if (!node()) return;
  const value = ['x','y','width','height','fontSize'].includes(key) ? Number(e.target.value) : e.target.value;
  const oldId = selected;
  edit(() => { const n = node();
    if (key === 'width' || key === 'height') resizeNode(design, n, key === 'width' ? value : n.width, key === 'height' ? value : n.height);
    else if (key === 'fontSize') { const t = textTarget(design, n); if (t) t.fontSize = value; }
    else if (key === 'id') n.name = value;
    else n[key] = value;

  });

};
for (const key of ['x','y','width','height','fontSize']) {
  const field = $('field-' + key); let recorded = false;
  field.addEventListener('focus', () => { recorded = false; });
  field.addEventListener('input', () => {
    if(projectRole==='viewer')return;
    const n = node(), value = Number(field.value), min = ['x','y'].includes(key) ? -8192 : key === 'fontSize' ? 8 : 1, max = key === 'fontSize' ? 128 : 8192;
    if (!n || field.value === '' || !Number.isFinite(value) || value < min || value > max) return;
    if (!recorded) { checkpoint(); recorded = true; }
    if (key === 'x' || key === 'y') { n[key] = value; captureBounds(design,n); syncNode(n); }
    else if (key === 'fontSize') { const t = textTarget(design, n); if (t) { t.fontSize = value; syncNode(t); } }
    else { resizeNode(design, n, key === 'width' ? value : n.width, key === 'height' ? value : n.height); syncNode(n); for (const child of design.nodes) syncNode(child); }
    mark(); renderRectInspector(); updateSelection();
  });
  field.addEventListener('blur', () => { const n = node(); if (n) field.value = key === 'fontSize' ? textTarget(design,n)?.fontSize ?? '' : n[key]; });
}
let textBefore = null;
$('field-text').addEventListener('focus', () => { textBefore = JSON.stringify(design); });
$('field-text').addEventListener('input', e => { if(projectRole==='viewer')return; const t = textTarget(design, node()); if (!t) return; textBefore ??= JSON.stringify(design); t.text = e.target.value; syncNode(t); mark(); });
$('field-text').addEventListener('blur', () => { if (textBefore && textBefore !== JSON.stringify(design)) { history.push(textBefore); if (history.length > 60) history.shift(); $('undo').disabled = false; } textBefore = null; });
$('design-name').onchange = e => edit(() => { design.name = e.target.value; });
$('root-node').onclick = () => select(null);
$('preset').onchange = e => { const value = e.target.value; edit(() => { [design.width, design.height] = value.split('x').map(Number); }); notice('已切换手机分辨率，控件按 RectTransform 锚点重新布局。'); };
$('rotate').onclick = () => { edit(() => { [design.width, design.height] = [design.height, design.width]; }); notice('已切换横竖屏，控件按 RectTransform 锚点重新布局。'); };
function insert(type, parent, point) {
  if (!ready || preview) return;
  edit(() => { selected = library.has(type) ? instantiateTemplate(design,library.get(type),parent,point) : appendControl(design, type, parent, point); });
  notice(`已添加 ${selected} · 拖动调整位置，双击编辑属性。`);
}
let paletteDrag;
let suppressClick = false;
function controlAllowed(type){return ready&&projectRole!=='viewer'&&!preview&&(centerMode==='edit'||(centerMode==='relations'&&library.get(type)?.category==='system'));}
function clearDrop() { $('drag-ghost').hidden = true; $('canvas-wrap').classList.remove('drop-target'); $('relations-view').classList.remove('drop-target'); }
function dropControl(type, x, y) {
  if(!controlAllowed(type))return;
  if(centerMode==='relations'){
    const position=relationView.dropPosition(x,y);
    if(!position){notice('已取消添加：请将系统模板放入关系区。');return;}
    edit(()=>{selected=instantiateTemplate(design,library.get(type),null,{x:0,y:0});design.nodes.find(n=>n.id===selected).relationPosition=position;});
    relationView.selectCard(selected);notice('已创建系统的项目副本，可连接其他 UI；点击保存写入当前项目。');return;
  }
  if (!insideCanvas(x, y)) { notice('已取消添加：请将控件放入手机画布。'); return; }
  const hit = document.elementsFromPoint(x, y).find(el => el.dataset?.type === 'Container' && $('canvas').contains(el));
  const parent = hit?.dataset.node ?? null;
  const target = parent ? design.nodes.find(n => n.id === parent) : design;
  insert(type, parent, pointInDesign(x, y, (hit ?? $('canvas')).getBoundingClientRect(), target.width, target.height));
}
function insideCanvas(x, y) { const r = $('canvas').getBoundingClientRect(); return x >= r.left && x <= r.right && y >= r.top && y <= r.bottom; }
function bindControl(control) {
  control.draggable = false;
  control.addEventListener('dragstart', e => e.preventDefault());
  control.addEventListener('pointerdown', e => {
    if (e.button !== 0 || !controlAllowed(control.dataset.control)) return;
    suppressClick = false;
    paletteDrag = { type: control.dataset.control, id: e.pointerId, x: e.clientX, y: e.clientY, moved: false };
    e.preventDefault();
  });
  control.addEventListener('click', () => { if (suppressClick) { suppressClick = false; return; } addCentered(control.dataset.control); });
}
for (const control of document.querySelectorAll('[data-control]')) bindControl(control);
window.addEventListener('pointermove', e => {
    if (!paletteDrag) return;
    if (Math.hypot(e.clientX - paletteDrag.x, e.clientY - paletteDrag.y) > 4) paletteDrag.moved = true;
    if (!paletteDrag.moved) return;
    $('drag-ghost').hidden = false; $('drag-ghost').textContent = `＋ ${library.get(paletteDrag.type)?.name ?? names[paletteDrag.type]}`;
    Object.assign($('drag-ghost').style, { left: `${e.clientX + 12}px`, top: `${e.clientY + 12}px` });
    $('canvas-wrap').classList.toggle('drop-target', centerMode==='edit'&&insideCanvas(e.clientX, e.clientY));
    $('relations-view').classList.toggle('drop-target',centerMode==='relations'&&!!relationView.dropPosition(e.clientX,e.clientY));
});
function finishPaletteDrag(e) {
    if (!paletteDrag) return;
    const state = paletteDrag; paletteDrag = null; clearDrop();
    if (state.moved) {
      suppressClick = true; dropControl(state.type, e.clientX, e.clientY);
    }
}
window.addEventListener('pointerup', finishPaletteDrag);
window.addEventListener('mouseup', finishPaletteDrag);
window.addEventListener('pointercancel', () => { paletteDrag = null; clearDrop(); });
window.addEventListener('blur', () => { paletteDrag = null; clearDrop(); });
function addCentered(type) {
  if(!controlAllowed(type))return;
  if(centerMode==='relations'){const r=$('relations-view').getBoundingClientRect();dropControl(type,(r.left+r.right)/2,(r.top+r.bottom)/2);return;}
  let target = node();
  while (target && target.type !== 'Container') target = design.nodes.find(n => n.id === target.parent);
  insert(type, target?.id ?? null, { x: (target?.width ?? design.width) / 2, y: (target?.height ?? design.height) / 2 });
}
function deleteSelected() {
  if (!node()) return;
  const remove = new Set([selected]); let count;
  do { count = remove.size; for (const n of design.nodes) if (remove.has(n.parent)) remove.add(n.id); } while (count !== remove.size);
  edit(() => { design.nodes = design.nodes.filter(n => !remove.has(n.id)); if(design.animations)design.animations=design.animations.filter(a=>!remove.has(a.hostNodeId)).map(a=>({...a,clip:{...a.clip,curves:a.clip.curves.filter(c=>!remove.has(c.targetNodeId))}})).filter(a=>a.clip.curves.length); for(const n of design.nodes) if(remove.has(n.action?.target)) delete n.action; selected = design.nodes[0]?.id; });
}
$('delete').onclick = deleteSelected;
window.addEventListener('keydown', e => {
  const target = e.target;
  if (target instanceof Element && (target.closest('input,textarea,select,[contenteditable]:not([contenteditable="false"]),[role="textbox"],[role="separator"]') || target.isContentEditable)) return;
  if (!ready || preview || centerMode!=='edit' || !node() || e.isComposing || e.ctrlKey || e.metaKey || e.altKey || drag || resize || paletteDrag) return;
  if (e.key === 'Delete') {
    e.preventDefault();
    if (!e.repeat) deleteSelected();
    return;
  }
  const directions = { ArrowLeft: [-1,0], ArrowRight: [1,0], ArrowUp: [0,-1], ArrowDown: [0,1] };
  const delta = directions[e.key]; if (!delta) return;
  e.preventDefault();
  const step = e.shiftKey ? 10 : 1;
  edit(() => { const n = node(); n.x = Math.max(-8192, Math.min(8192, n.x + delta[0] * step)); n.y = Math.max(-8192, Math.min(8192, n.y + delta[1] * step)); });
});
$('undo').onclick = () => { if (history.length) { design = JSON.parse(history.pop()); mark(); render(); } };
$('fit').onclick = () => {manualScale=null;fit();};
$('viewport').addEventListener('wheel', e => {
  if(preview||centerMode!=='edit')return;
  e.preventDefault();
  const viewport=$('viewport'),before=$('canvas-wrap').getBoundingClientRect();
  const x=(e.clientX-before.left)/scale,y=(e.clientY-before.top)/scale;
  const delta=e.deltaY*(e.deltaMode===1?16:e.deltaMode===2?viewport.clientHeight:1);
  manualScale=Math.max(.05,Math.min(4,scale*Math.exp(-Math.max(-400,Math.min(400,delta))*.002)));
  fit();
  const after=$('canvas-wrap').getBoundingClientRect();
  viewport.scrollLeft+=after.left+x*scale-e.clientX;viewport.scrollTop+=after.top+y*scale-e.clientY;
  updateSelection();
},{passive:false});
$('viewport').addEventListener('scroll',updateSelection);
function setPreview(value) {
  if(value)setCenterMode('edit');
  preview = value;
  previewState = value ? createPreviewState(design) : null;
  document.body.classList.toggle('rehearsing', preview);
  $('preview').setAttribute('aria-pressed', preview);
  $('preview').textContent = preview ? '退出预演' : '预演';
  $('import').disabled = preview;
  for (const c of document.querySelectorAll('[data-control]')) c.disabled = !controlAllowed(c.dataset.control);
  render();
  notice(preview ? '预演模式 · 点击执行已配置的界面动作；可返回或重置。Esc 退出，预演不会修改布局。' : '编辑模式 · 选择控件，在“点击动作”中配置显示、关闭或跳转目标。');
}
$('preview').onclick = () => setPreview(!preview);
$('preview-back').onclick = () => performAction({type:'back'});
$('preview-reset').onclick = () => { previewState=createPreviewState(design); render(); notice('预演已重置。'); };
window.addEventListener('keydown', e => { if (preview && e.key === 'Escape') { e.preventDefault(); setPreview(false); } });
let drag, resize;
selectionBox.addEventListener('pointerdown', e => {
  const handle = e.target.dataset.resize;
  if (!handle || !node() || preview || projectRole==='viewer' || e.button !== 0) return;
  e.preventDefault(); e.stopPropagation();
  resize = {handle, startX:e.clientX, startY:e.clientY, rect:{...node()}, before:JSON.stringify(design), moved:false};
  $('inspector').open = true;
});
window.addEventListener('pointermove', e => {
  if (!resize) return;
  const delta=localDelta(design,node().id,(e.clientX-resize.startX)/scale,(e.clientY-resize.startY)/scale);if(!delta)return;
  const rect = resizedRect(resize.rect, resize.handle, delta.x, delta.y);
  const n = node(); resizeNode(design, n, rect.width, rect.height); n.x = rect.x; n.y = rect.y; captureBounds(design,n); resolveLayout(design);
  resize.moved = JSON.stringify(design) !== resize.before;
  syncNode(n); for (const child of design.nodes) syncNode(child);
  for (const key of ['x','y','width','height']) $('field-' + key).value = key === 'id' ? nodeName(n) : n[key];
  renderRectInspector(); updateSelection();
});
function finishResize() { if (resize?.moved) { history.push(resize.before); if (history.length > 60) history.shift(); mark(); } resize = null; }
window.addEventListener('pointerup', finishResize); window.addEventListener('mouseup', finishResize); window.addEventListener('pointercancel', finishResize); window.addEventListener('blur', finishResize);
$('canvas').addEventListener('pointerdown', e => {
  let target = e.target.closest('[data-node]'); if (!target || preview || e.button !== 0) return;
  if (target.parentElement?.dataset.type === 'Button' && selected !== target.dataset.node) target = target.parentElement;
  const hitNode = design.nodes.find(n=>n.id === target.dataset.node), parentNode = design.nodes.find(n=>n.id === hitNode?.parent);
  if (parentNode?.template === 'ItemSlot' && selected !== hitNode.id) target = target.parentElement;
  selected = target.dataset.node; const n = node();
  if(projectRole==='viewer'){select(selected);return;}
  $('inspector').open = true;
  drag = { id: selected, pointer: e.pointerId, startX: e.clientX, startY: e.clientY, x: n.x, y: n.y, before: JSON.stringify(design), moved: false };
  $('canvas').setPointerCapture(e.pointerId); e.preventDefault(); render();
});
window.addEventListener('pointermove', e => {
  if (!drag || drag.pointer !== e.pointerId) return;
  const delta=localDelta(design,node().parent,(e.clientX-drag.startX)/scale,(e.clientY-drag.startY)/scale);if(!delta)return;
  const dx=delta.x,dy=delta.y;
  if (!drag.moved && Math.abs(dx) + Math.abs(dy) < 2) return;
  drag.moved = true;
  const n = node(); n.x = Math.max(-8192, Math.min(8192, Math.round(drag.x + dx))); n.y = Math.max(-8192, Math.min(8192, Math.round(drag.y + dy)));
  captureBounds(design,n);
  const el = [...$('canvas').querySelectorAll('[data-node]')].find(el => el.dataset.node === selected); el.style.left = `${n.x}px`; el.style.top = `${n.y}px`; $('field-x').value = n.x; $('field-y').value = n.y;
  renderRectInspector(); updateSelection();
});
function finishDrag() { if (drag?.moved) { history.push(drag.before); if (history.length > 60) history.shift(); mark(); } drag = null; }
window.addEventListener('pointerup', finishDrag); window.addEventListener('mouseup', finishDrag); window.addEventListener('pointercancel', finishDrag);
$('canvas').addEventListener('dblclick', () => { if (!preview) { $('inspector').open = true; if (textTarget(design,node())) { $('field-text').focus(); $('field-text').select(); } } });
$('canvas').addEventListener('click', e => {
  if (!preview) return;
  const el=e.target.closest('[data-interactive]'); if(!el) return;
  const n=design.nodes.find(n=>n.id===el.dataset.node), action=defaultAction(design,n);
  let owner=n;while(owner){if(owner.type==='Button'&&owner.interactable===false)return;owner=design.nodes.find(p=>p.id===owner.parent);}
  if(action) { performAction(action); notice(`${n.id}：${({show:'显示',hide:'关闭',toggle:'切换显示',navigate:'跳转',back:'返回',none:'无动作'})[action.type]}${action.target ? ' → '+action.target : ''}`); }
  else { el.animate([{filter:'brightness(1.5)'},{filter:'brightness(1)'}],{duration:220}); notice(`${n.id} 已点击。可在编辑模式的“点击动作”中指定目标界面。`); }
});
$('canvas').addEventListener('keydown', e => { const el=e.target.closest('[data-interactive]'); if(preview && el && el.tagName!=='BUTTON' && ['Enter',' '].includes(e.key)) {e.preventDefault();el.click();} });
async function busy(id, fn) { $(id).disabled = true; try { await fn(); } catch (e) { notice(e.message, true); } finally { $(id).disabled = !ready||(['save','sync'].includes(id)&&projectRole==='viewer'); } }
$('save').onclick = () => busy('save', async () => {
  const sent=JSON.stringify(design),snapshot=await bridge.call('save',{design,revision,projectVersion});revision=snapshot.revision;
  const result=snapshot.projectSave;
  if(result?.ok){projectVersion=result.version;if(JSON.stringify(design)===sent){dirty=false;$('dirty').hidden=true;sessionStorage.removeItem(draftKey());}}
  notice(result?.ok?`工程 ${result.name} 已保存到数据库 · v${result.version} · 基础模板未修改${dirty?' · 仍有未保存修改':''}`:`本机草稿 v${revision} 已保存 · 工程数据库保存失败：${result?.error??'服务未支持工程保存'}`,!result?.ok);
});
$('sync').onclick = () => busy('sync', async () => {
  if(librarySource!=='postgresql')throw new Error('请先连接数据库模板库。');
  let root=node();while(root?.parent)root=design.nodes.find(n=>n.id===root.parent);
  if(!root){const roots=design.nodes.filter(n=>n.parent===null);if(roots.length!==1)throw new Error('请先选择要同步的系统节点。');root=roots[0];}
  const template=root.templateSourceId?library.get(root.templateSourceId):[...library.values()].find(t=>t.payload.root===root.id);
  if(!template)throw new Error('此系统未关联基础模板，不能同步；请使用保存将它存入工程。');
  if(!window.confirm(`将当前项目的「${root.name}」提交为「${template.name}」候选？此操作不会发布或覆盖标准模板。`))return;
  const result=await bridge.call('candidate',{design,revision,projectVersion,rootId:root.id,templateId:template.id,version:template.version});
  notice('已提交候选 '+result.id+' · 标准模板未修改，请在模板管理入口审核。');
});
function download(name, text, type) { const url = URL.createObjectURL(new Blob([text], { type })); const a = document.createElement('a'); a.href = url; a.download = name; a.click(); setTimeout(() => URL.revokeObjectURL(url), 2000); }
$('export').onclick = () => busy('export', async () => {
  const result = await bridge.call('export', { design });
  if (!bridge.embedded) { download(`${result.design.name}.html`, result.html, 'text/html'); download(`${result.design.name}.studio.json`, JSON.stringify(result.design, null, 2), 'application/json'); }
  notice(`${result.notice} 文件目录：${result.directory}`);
});
$('import').onclick = () => $('import-file').click();
$('import-file').onchange = async e => {
  if(projectRole==='viewer')return;
  const file = e.target.files[0]; if (!file) return;
  try { if (file.size > 1_000_000) throw new Error('草稿文件不能超过 1 MB。'); const next = validateDesign(phoneDesign(validateDesign(JSON.parse(await file.text())))); checkpoint(); design = next; selected = design.nodes[0]?.id; mark(); render(); notice('已导入草稿，点击保存写入当前项目；可撤销返回之前的设计。'); } catch (err) { notice(err.message, true); }
  e.target.value = '';
};
bridge.onContext = () => { $('expand').hidden = !bridge.modes.includes('fullscreen'); };
$('expand').onclick = async () => { try { await bridge.expand(); } catch (e) { notice(e.message, true); } };
function setCenterMode(mode){
  if(preview&&mode!=='edit')setPreview(false);
  centerMode=mode;const graph=mode==='relations';
  $('viewport').hidden=mode!=='edit';$('relations-view').hidden=!graph;$('relations-tools').hidden=!graph;$('relations-info').hidden=!graph;$('code-view').hidden=mode!=='code';
  for(const id of ['edit','relations','code'])$('tab-'+id).setAttribute('aria-selected',mode===id);
  $('preset').hidden=$('rotate').hidden=$('fit').hidden=mode!=='edit';
  for(const c of document.querySelectorAll('[data-control]'))c.disabled=!controlAllowed(c.dataset.control);
  cardScripts.setVisible(mode!=='edit');
  document.querySelector('.canvas-footer').hidden=mode==='code';
  $('template-dock').hidden=$('template-splitter').hidden=mode==='code';
  if(graph)relationView.render();else if(mode==='edit')fit();
}
const codePage=createCodePage(()=>setCenterMode('code'));
const cardScripts=createCardScripts({getDesign:()=>design,onOpen:(file,name)=>codePage.open(file,name),getTemplate:root=>({name:'项目副本'+(root.templateSourceVersion?' · 来源 v'+root.templateSourceVersion:''),scripts:root.scripts??null})});
const relationView=createRelations({getDesign:()=>design,edit,notice,onDelete:id=>{if(projectRole==='viewer'){notice('当前项目为只读权限。',true);return;}selected=id;deleteSelected();cardScripts.select(null);$('current-ui').textContent='当前节点：'+(node()?.name??'Canvas');notice('已删除 UI 及其子节点和相关连线，可撤销；保存后写入当前项目。');},onSelect:id=>{selected=id;$('current-ui').textContent='当前节点：'+(node()?.name??'Canvas');cardScripts.select(id);},onEdit:id=>{setCenterMode('edit');select(id);},onPreview:id=>{setPreview(true);previewState.hidden=[...new Set([...previewState.hidden.filter(n=>n!==id),...design.nodes.filter(n=>n.parent===null&&n.id!==id).map(n=>n.id)])];applyPreviewState();}});
$('tab-edit').onclick=()=>setCenterMode('edit');$('tab-relations').onclick=()=>setCenterMode('relations');
const projectBar=document.createElement('div');projectBar.className='project-banner';
const workspaceMode=document.createElement('strong');workspaceMode.textContent='项目工作区';
const currentUi=document.createElement('span');currentUi.id='current-ui';currentUi.textContent='当前节点：Canvas';
const switchProject=document.createElement('button');switchProject.textContent='切换项目';switchProject.onclick=async()=>{
 if(dirty){const choice=prompt('当前项目有未保存修改。输入“保存”后切换，或输入“放弃”；取消则留在当前项目。');if(choice==='保存'){await $('save').onclick();if(dirty)return;}else if(choice==='放弃'){dirty=false;sessionStorage.removeItem(draftKey());}else return;}
 if(bridge.embedded){bridge.projectId='';location.reload();}else location.href='/studio';
};
const historyButton=document.createElement('button');historyButton.textContent='项目历史';historyButton.onclick=async()=>{try{const result=await bridge.call('revisions');const dialog=document.createElement('dialog');dialog.className='sync-confirm';const title=document.createElement('h2');title.textContent='恢复项目历史（生成新版本）';const select=document.createElement('select');for(const r of result.revisions)select.add(new Option(`v${r.version} · ${new Date(r.created_at).toLocaleString()}`,String(r.version)));const cancel=document.createElement('button');cancel.textContent='取消';cancel.onclick=()=>dialog.close();const restore=document.createElement('button');restore.textContent='确认恢复';restore.disabled=projectRole==='viewer';restore.onclick=async()=>{if(!confirm('恢复将替换当前项目布局和代码，未保存的修改会被放弃。继续？'))return;restore.disabled=true;try{const value=await bridge.call('restore',{version:Number(select.value),revision,projectVersion});design=value.design;revision=value.revision;projectVersion=value.projectSave.version;dirty=false;history.length=0;sessionStorage.removeItem(draftKey());$('dirty').hidden=true;selected=null;render();notice('已恢复项目历史为新版本 v'+projectVersion);dialog.close();}catch(e){notice(e.message,true);restore.disabled=false;}};dialog.append(title,select,cancel,restore);dialog.onclose=()=>dialog.remove();dialog.onkeydown=e=>e.stopPropagation();document.body.append(dialog);dialog.showModal();}catch(e){notice(e.message,true);}};
const upgradeButton=document.createElement('button');upgradeButton.textContent='升级模板副本';upgradeButton.onclick=async()=>{try{
 let root=node();while(root?.parent)root=design.nodes.find(n=>n.id===root.parent);if(!root)throw Error('请选择要升级的系统节点。');
 const template=library.get(root.templateSourceId);if(!template)throw Error('未关联标准模板。');if(template.version===root.templateSourceVersion)throw Error('当前副本来源已是最新版本；项目修改不会自动覆盖。');
 const result=upgradeTemplate(design,root.id,template),dialog=document.createElement('dialog');dialog.className='sync-confirm upgrade-confirm';const title=document.createElement('h2');title.textContent=`升级 ${root.name}：v${root.templateSourceVersion??'?'} → v${template.version}`;const note=document.createElement('p');note.textContent='将替换当前项目中的此系统布局和源码（包括本地调整），标准库和其他项目不变。确认后仍需保存项目，可撤销。';const diff=document.createElement('pre');diff.textContent=JSON.stringify({before:design,after:result.design},null,2);const cancel=document.createElement('button');cancel.textContent='取消';cancel.onclick=()=>dialog.close();const apply=document.createElement('button');apply.textContent='确认替换项目副本';apply.onclick=()=>{edit(()=>{design=result.design;selected=result.rootId;});dialog.close();};dialog.append(title,note,diff,cancel,apply);dialog.onclose=()=>dialog.remove();dialog.onkeydown=e=>e.stopPropagation();document.body.append(dialog);dialog.showModal();
 }catch(e){notice(e.message,true);}};
const templateManagement=document.createElement('button');templateManagement.id='template-management';templateManagement.textContent='标准模板管理';templateManagement.title='在新页面审核候选、发布模板和查看版本历史';
templateManagement.onclick=()=>window.open(bridge.embedded?'http://127.0.0.1:18091/studio/templates':'/studio/templates','_blank','noopener,noreferrer');
projectBar.append(workspaceMode,currentUi,switchProject,historyButton,upgradeButton,templateManagement);document.querySelector('.topbar').after(projectBar);
$('sync').textContent='提交候选';$('sync').title='仅提交模板候选，不发布标准模板';
setupPanelSplit();
setupWorkspaceSplit();
const refreshTemplateDock=setupTemplateDock(loadTemplates);
new ResizeObserver(fit).observe($('viewport'));
window.addEventListener('beforeunload', e => { if (dirty) { e.preventDefault(); e.returnValue = ''; } });
render();
async function start() {
  try { const snapshot = await bridge.init(); const original = validateDesign(snapshot.design); design = validateDesign(phoneDesign(original)); revision = snapshot.revision;projectVersion=snapshot.project.version;projectRole=snapshot.project.role;$('project-label').textContent=`项目：${snapshot.project.name} [${snapshot.project.id}]`;$('project-label').title=`项目 ID：${snapshot.project.id} · 权限：${projectRole}`; ready = true; $('save').disabled = $('sync').disabled = upgradeButton.disabled = projectRole==='viewer';$('export').disabled=false; $('connection').textContent = bridge.embedded ? 'MCP 已连接' : '本机';
   let cached;try{cached=JSON.parse(sessionStorage.getItem(draftKey()));}catch{}
   if(cached){if(cached.projectVersion===projectVersion&&confirm('发现本标签页此项目的未保存草稿，是否恢复？')){design=validateDesign(cached.design);dirty=true;$('dirty').hidden=false;}else if(cached.projectVersion!==projectVersion){sessionStorage.setItem(draftKey()+'.conflict.'+cached.projectVersion,JSON.stringify(cached));const recovery=document.createElement('button');recovery.textContent='下载冲突草稿 v'+cached.projectVersion;recovery.onclick=()=>download('conflict-v'+cached.projectVersion+'.studio.json',JSON.stringify(cached.design,null,2),'application/json');projectBar.append(recovery);notice('数据库项目已有更新。旧草稿已单独保留，可下载核对，未覆盖项目。',true);}else sessionStorage.removeItem(draftKey());}
   if (JSON.stringify(original) !== JSON.stringify(design)) { history.push(JSON.stringify(original)); mark(); } render(); if(!cached||cached.projectVersion===projectVersion)notice(`正在编辑项目 ${snapshot.project.name} · v${projectVersion} · 保存仅影响此项目。`);
  }
  catch (e) { $('connection').textContent = '连接失败'; notice(e.message, true); }
}
void start().then(loadTemplates);
