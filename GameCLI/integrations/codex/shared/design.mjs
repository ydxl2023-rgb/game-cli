import {validateAnimations} from './animations.mjs';
import {resolveLayout, captureBounds} from './rect-transform.mjs';
export const types = ['Container', 'Image', 'Text', 'Button', 'Progress'];
export function nodeName(n) {
  if (n.name) return n.name[0].toUpperCase()+n.name.slice(1);
  if (n.role) return n.role[0].toUpperCase()+n.role.slice(1);
  if (n.template) return ({InventorySystem:'Inventory',ConfirmDialog:'Dialog',ScrollPanel:'Scroll',ItemSlot:'Item'})[n.template];
  const suffix=n.id.match(/(Background|Subtitle|Title|Description|Details|Message|Cancel|Confirm|Close|Use|Label|Panel|Content|Mask)$/)?.[1];
  const value=suffix ?? n.id;
  return value[0].toUpperCase()+value.slice(1);
}
export function textTarget(design, node) {
  if (node?.type === 'Text') return node;
  if (node?.type === 'Button') return design.nodes.find(n => n.parent === node.id && n.type === 'Text') ?? null;
  return null;
}
export function resizeNode(design, node, width, height) {
  if (![width, height].every(v => Number.isFinite(v) && v >= 1 && v <= 8192)) throw new Error('宽高须在 1–8192 之间。');
  if (node.rectTransform) {
    node.width=width;node.height=height;captureBounds(design,node);resolveLayout(design);
    if(node.template==='ScrollPanel') { layoutScrollPanel(design,node); for(const child of design.nodes.filter(n=>n.parent===node.id || design.nodes.find(p=>p.id===n.parent)?.parent===node.id))captureBounds(design,child);resolveLayout(design); }
    return;
  }
  if (node.type === 'Button') {
    for (const child of design.nodes.filter(n => n.parent === node.id && n.type === 'Text')) {
      if (child.x === 0 && child.y === 0 && child.width === node.width && child.height === node.height) { child.width = width; child.height = height; }
    }
  }
  if (node.template === 'ItemSlot') {
    for (const child of design.nodes.filter(n => n.parent === node.id)) {
      child.x *= width / node.width; child.width = Math.max(1, child.width * width / node.width);
      child.y *= height / node.height; child.height = Math.max(1, child.height * height / node.height);
    }
  }
  node.width = width; node.height = height;
  if (node.template === 'InventorySystem') for (const child of design.nodes.filter(n=>n.parent===node.id && n.role==='background')) Object.assign(child,{x:0,y:0,width,height});
  if (node.template === 'ScrollPanel') layoutScrollPanel(design, node);
  if (node.template === 'ConfirmDialog') layoutConfirmDialog(design, node);
}
export function layoutScrollPanel(design, panel) {
  const content = design.nodes.find(n => n.parent === panel.id && n.role === 'content');
  if (!content) return;
  const slots = design.nodes.filter(n => n.parent === content.id && n.template === 'ItemSlot');
  const width = Math.max(1, panel.width - 16), padding = Math.min(16, Math.max(0, (width - 128) / 2));
  const columns = Math.max(1, Math.min(slots.length || 1, Math.floor((width - padding * 2 + 16) / 144)));
  const rows = Math.ceil(slots.length / columns);
  const gapX = 16;
  const gapY = 16;
  Object.assign(content, {x:0,y:0,width,height:Math.max(panel.height,32 + rows * 128 + Math.max(0,rows - 1) * gapY)});
  slots.forEach((slot,index) => {
    resizeNode(design,slot,128,128);
    slot.x = padding + (index % columns) * (128 + gapX);
    slot.y = 16 + Math.floor(index / columns) * (128 + gapY);
  });
}
export function resizedRect(rect, handle, dx, dy) {
  let {x, y, width, height} = rect;
  if (handle.includes('e')) width = Math.max(1, Math.min(8192, Math.round(rect.width + dx)));
  if (handle.includes('s')) height = Math.max(1, Math.min(8192, Math.round(rect.height + dy)));
  if (handle.includes('w')) { width = Math.max(1, Math.min(8192, Math.round(rect.width - dx))); x = rect.x + rect.width - width; }
  if (handle.includes('n')) { height = Math.max(1, Math.min(8192, Math.round(rect.height - dy))); y = rect.y + rect.height - height; }
  return { x: Math.max(-8192, Math.min(8192, x)), y: Math.max(-8192, Math.min(8192, y)), width, height };
}
function legacyStarter() {
  return { schemaVersion: 1, name: 'UIInventory', width: 960, height: 640, nodes: [
    { id: 'InventoryPanel', parent: null, type: 'Container', x: 70, y: 50, width: 820, height: 540, color: '#243044', text: '', fontSize: 20 },
    { id: 'Title', parent: 'InventoryPanel', type: 'Text', x: 32, y: 26, width: 480, height: 44, color: '#eef2f8', text: '旅人的背包', fontSize: 30 },
    { id: 'Subtitle', parent: 'InventoryPanel', type: 'Text', x: 32, y: 78, width: 480, height: 30, color: '#9caec4', text: '选择物品，开始下一段旅程', fontSize: 16 },
    ...Array.from({ length: 8 }, (_, i) => ({ id: `Item${String(i + 1).padStart(2, '0')}`, parent: 'InventoryPanel', type: 'Button', x: 32 + (i % 4) * 128, y: 146 + Math.floor(i / 4) * 124, width: 108, height: 104, color: '#36475f', text: `物品 ${i + 1}`, fontSize: 16 })),
    { id: 'DetailPanel', parent: 'InventoryPanel', type: 'Container', x: 568, y: 146, width: 220, height: 228, color: '#1b2637', text: '', fontSize: 20 },
    { id: 'DetailText', parent: 'DetailPanel', type: 'Text', x: 20, y: 28, width: 180, height: 100, color: '#afbdd0', text: '物品详情\n在这里展示名称与描述', fontSize: 17 },
    { id: 'EquipButton', parent: 'InventoryPanel', type: 'Button', x: 568, y: 432, width: 220, height: 60, color: '#b8e578', text: '装备物品', fontSize: 20 },
    { id: 'Capacity', parent: 'InventoryPanel', type: 'Text', x: 32, y: 442, width: 280, height: 36, color: '#9caec4', text: '背包容量  8 / 32', fontSize: 17 }
  ] };
}
export function materializeLabels(input) {
  const d = structuredClone(input);
  const ids = new Set([d.name, ...d.nodes.map(n => n.id)]);
  for (const n of [...d.nodes]) {
    if (n.type !== 'Button' || !n.text) continue;
    let id = `${n.id.slice(0, 50)}Label`, i = 1;
    while (ids.has(id)) id = `${n.id.slice(0, 50)}Label${i++}`;
    ids.add(id);
    d.nodes.push({ id, parent: n.id, type: 'Text', x: 0, y: 0, width: n.width, height: n.height, color: '#101923', text: n.text, fontSize: n.fontSize });
    n.text = '';
  }
  return d;
}
export function starter() {
  const d = legacyStarter(); d.width = 1080; d.height = 1920;
  for (const n of d.nodes) {
    if (n.id === 'InventoryPanel') Object.assign(n, { x: 48, y: 120, width: 984, height: 1680 });
    else if (n.id === 'Title') Object.assign(n, { x: 48, y: 50, width: 880, height: 80, fontSize: 58 });
    else if (n.id === 'Subtitle') Object.assign(n, { x: 48, y: 150, width: 880, height: 60, fontSize: 32 });
    else if (/^Item\d+$/.test(n.id)) { const i = Number(n.id.slice(4)) - 1; Object.assign(n, { x: 48 + (i % 4) * 224, y: 290 + Math.floor(i / 4) * 224, width: 208, height: 208, fontSize: 32 }); }
    else if (n.id === 'DetailPanel') Object.assign(n, { x: 48, y: 830, width: 880, height: 430 });
    else if (n.id === 'DetailText') Object.assign(n, { x: 40, y: 50, width: 800, height: 300, fontSize: 36 });
    else if (n.id === 'EquipButton') Object.assign(n, { x: 48, y: 1450, width: 880, height: 130, fontSize: 42 });
    else if (n.id === 'Capacity') Object.assign(n, { x: 48, y: 1330, width: 880, height: 70, fontSize: 34 });
  }
  return materializeLabels(d);
}
export function phoneDesign(input) {
  if (JSON.stringify(input) === JSON.stringify(legacyStarter())) return starter();
  const d = materializeLabels(input);
  for (const root of d.nodes.filter(n=>n.template==='InventorySystem')) {
    if (d.nodes.some(n=>n.parent===root.id && n.role==='background')) continue;
    const base=root.id.slice(0,48)+'Background'; let id=base, i=1;
    while(d.nodes.some(n=>n.id===id) || d.name===id) id=base+i++;
    d.nodes.splice(d.nodes.indexOf(root)+1,0,{id,parent:root.id,type:'Image',role:'background',x:0,y:0,width:root.width,height:root.height,color:root.color,text:'',fontSize:32});
  }
  if (d.width === 960 && d.height === 640) {
    const ratio = 1080 / d.width, offset = (1920 - d.height * ratio) / 2;
    for (const n of d.nodes) {
      for (const key of ['x','y','width','height']) n[key] = Math.round(n[key] * ratio);
      n.fontSize = Math.min(128, Math.round(n.fontSize * ratio));
      if (n.parent === null) n.y += offset;
    }
    d.width = 1080; d.height = 1920;
  }
  return d;
}
export function pointInDesign(clientX, clientY, bounds, width, height) {
  return { x: (clientX - bounds.left) * width / bounds.width, y: (clientY - bounds.top) * height / bounds.height };
}
export function appendControl(d, type, parent, point) {
  if (type === 'InventorySystem') return appendInventorySystem(d);
  if (type === 'ConfirmDialog') return appendConfirmDialog(d);
  if (type === 'ScrollPanel') return appendScrollPanel(d, parent, point);
  if (type === 'ItemSlot') return appendItemSlot(d, parent, point);
  if (!types.includes(type)) throw new Error('不支持的控件。');
  const container = d.nodes.find(n => n.id === parent);
  if (parent !== null && container?.type !== 'Container') throw new Error('目标必须为容器。');
  let i = 1; while (d.nodes.some(n => n.id === `${type}${i}`) || d.name === `${type}${i}`) i++;
  const width = Math.min(type === 'Container' ? 600 : 360, container?.width ?? d.width);
  const height = Math.min(type === 'Container' ? 400 : 100, container?.height ?? d.height);
  const maxWidth = container?.width ?? d.width, maxHeight = container?.height ?? d.height;
  const n = { id: `${type}${i}`, type, parent, x: Math.round(Math.max(0, Math.min(maxWidth - width, point.x - width / 2))), y: Math.round(Math.max(0, Math.min(maxHeight - height, point.y - height / 2))), width, height, color: type === 'Text' ? '#eef2f8' : type === 'Button' ? '#b8e578' : '#36475f', text: type === 'Text' ? '文本' : type === 'Button' ? '按钮' : '', fontSize: 36 };
  d.nodes.push(n);
  d.nodes = materializeLabels(d).nodes;
  return n.id;
}
export function appendItemSlot(d, parent, point) {
  const container = d.nodes.find(n => n.id === parent);
  if (parent !== null && container?.type !== 'Container') throw new Error('目标必须为容器。');
  const suffixes = ['', 'Background', 'Icon', 'Mask', 'Count', 'Frame'];
  const ids = new Set([d.name, ...d.nodes.map(n => n.id)]);
  let i = 1; while (suffixes.some(s => ids.has(`ItemSlot${i}${s}`))) i++;
  const id = `ItemSlot${i}`, size = 128;
  const root = { id, parent, type:'Container', template:'ItemSlot', x:Math.round(Math.max(0,Math.min((container?.width ?? d.width)-size, point.x-size/2))), y:Math.round(Math.max(0,Math.min((container?.height ?? d.height)-size, point.y-size/2))), width:size, height:size, color:'#192332', text:'', fontSize:32 };
  const layer = (suffix, role, type, x, y, w, h, color, extra={}) => ({id:id+suffix,parent:id,type,role,x:x*size,y:y*size,width:w*size,height:h*size,color,text:'',fontSize:32,...extra});
  d.nodes.push(root,
    layer('Background','background','Image',0,0,1,1,'#26354b'),
    layer('Icon','icon','Image',0,0,1,1,'#8297b3'),
    layer('Mask','mask','Image',0,0,1,1,'#000000',{opacity:.25}),
    layer('Count','count','Text',.1,.73,.8,.22,'#ffffff',{text:'99',textAlign:'right'}),
    layer('Frame','frame','Image',0,0,1,1,'#c4ef91',{borderWidth:6})
  );
  return id;
}
export function appendScrollPanel(d, parent, point) {
  const container = d.nodes.find(n => n.id === parent);
  if (parent !== null && container?.type !== 'Container') throw new Error('目标必须为容器。');
  if (d.nodes.length + 74 > 300) throw new Error('添加滚动面板需要 74 个节点，超出 300 个节点上限。');
  const ids = new Set([d.name, ...d.nodes.map(n => n.id)]);
  let i = 1; while (ids.has(`ScrollPanel${i}`) || ids.has(`ScrollPanel${i}Content`)) i++;
  const id = `ScrollPanel${i}`, width = 464, height = 320;
  const base = {type:'Container',color:'#243044',text:'',fontSize:32};
  d.nodes.push({...base,id,parent,template:'ScrollPanel',x:Math.round(Math.max(0,Math.min((container?.width ?? d.width)-width,point.x-width/2))),y:Math.round(Math.max(0,Math.min((container?.height ?? d.height)-height,point.y-height/2))),width,height});
  d.nodes.push({...base,id:id+'Content',parent:id,role:'content',x:0,y:0,width:448,height:592});
  for (let index=0; index<12; index++) appendItemSlot(d,id+'Content',{x:80+(index%3)*144,y:80+Math.floor(index/3)*144});
  return id;
}
export function layoutConfirmDialog(d, root) {
  const get = suffix => d.nodes.find(n=>n.id===root.id+suffix);
  const panel=get('Panel'), mask=get('Mask');
  if (mask) Object.assign(mask,{x:0,y:0,width:root.width,height:root.height});
  if (!panel) return;
  const w=Math.min(820,root.width*.86), h=Math.min(480,root.height*.8);
  Object.assign(panel,{x:(root.width-w)/2,y:(root.height-h)/2,width:w,height:h});
  const set=(suffix,x,y,width,height)=>{const n=get(suffix); if(n)Object.assign(n,{x:x*w,y:y*h,width:Math.max(1,width*w),height:Math.max(1,height*h)});};
  set('Title',.07,.09,.86,.15); set('Message',.07,.3,.86,.32);
  set('Cancel',.07,.72,.4,.2); set('Confirm',.53,.72,.4,.2);
  for (const suffix of ['Cancel','Confirm']) { const button=get(suffix),label=get(suffix+'Label'); if(button&&label)Object.assign(label,{x:0,y:0,width:button.width,height:button.height}); }
}
export function appendConfirmDialog(d) {
  if (d.nodes.length+9>300) throw new Error('添加确认对话框超出 300 个节点上限。');
  const suffixes=['','Mask','Panel','Title','Message','Cancel','CancelLabel','Confirm','ConfirmLabel'];
  const ids=new Set([d.name,...d.nodes.map(n=>n.id)]);
  let i=1; while(suffixes.some(s=>ids.has(`ConfirmDialog${i}${s}`))) i++;
  const id=`ConfirmDialog${i}`;
  const add=(suffix,parent,type,color,text='',extra={})=>d.nodes.push({id:id+suffix,parent,type,x:0,y:0,width:128,height:128,color,text,fontSize:32,...extra});
  add('',null,'Container','#243044','',{template:'ConfirmDialog',width:d.width,height:d.height});
  add('Mask',id,'Image','#000000','',{opacity:.6});
  add('Panel',id,'Container','#243044');
  add('Title',id+'Panel','Text','#eef2f8','确认操作',{fontSize:44,textAlign:'center'});
  add('Message',id+'Panel','Text','#b6c2d2','是否确认执行此操作？',{textAlign:'center'});
  add('Cancel',id+'Panel','Button','#46566c');
  add('CancelLabel',id+'Cancel','Text','#eef2f8','取消');
  add('Confirm',id+'Panel','Button','#b8e578');
  add('ConfirmLabel',id+'Confirm','Text','#172211','确认');
  layoutConfirmDialog(d,d.nodes.find(n=>n.id===id));
  return id;
}
export function appendInventorySystem(design) {
  const d=structuredClone(design), ids=new Set([d.name,...d.nodes.map(n=>n.id)]);
  const suffixes=['','Background','Title','Subtitle','Details','Description','Use','UseLabel','Close','CloseLabel'];
  let i=1; while(suffixes.some(s=>ids.has(`InventorySystem${i}${s}`))) i++;
  const id=`InventorySystem${i}`, w=d.width, h=d.height;
  const add=(suffix,parent,type,x,y,width,height,color,text='',extra={})=>d.nodes.push({id:id+suffix,parent,type,x,y,width,height,color,text,fontSize:32,...extra});
  add('',null,'Container',0,0,w,h,'#192332','',{template:'InventorySystem'});
  add('Background',id,'Image',0,0,w,h,'#192332','',{role:'background'});
  add('Title',id,'Text',w*.06,h*.045,w*.7,h*.05,'#eef2f8','背包',{fontSize:48});
  add('Subtitle',id,'Text',w*.06,h*.105,w*.88,h*.035,'#afbdd0','选择道具，查看详情并使用');
  const listId=appendScrollPanel(d,id,{x:w/2,y:h*.38}), list=d.nodes.find(n=>n.id===listId);
  Object.assign(list,{x:w*.06,y:h*.16}); resizeNode(d,list,w*.88,h*.44);
  add('Details',id,'Container',w*.06,h*.64,w*.88,h*.16,'#243044');
  add('Description',id+'Details','Text',w*.025,h*.015,w*.8,h*.13,'#dce5f0','道具详情\n点击道具框可显示此区域。');
  add('Use',id,'Button',w*.06,h*.85,w*.88,h*.065,'#b8e578');
  add('UseLabel',id+'Use','Text',0,0,w*.88,h*.065,'#172211','使用道具');
  add('Close',id,'Button',w*.82,h*.045,w*.12,h*.05,'#46566c','',{action:{type:'hide',target:id}});
  add('CloseLabel',id+'Close','Text',0,0,w*.12,h*.05,'#eef2f8','关闭');
  const dialogId=appendConfirmDialog(d), dialog=d.nodes.find(n=>n.id===dialogId);
  dialog.parent=id;
  d.nodes.find(n=>n.id===dialogId+'Title').text='使用道具';
  d.nodes.find(n=>n.id===dialogId+'Message').text='确定使用当前道具吗？';
  d.nodes.find(n=>n.id===id+'Use').action={type:'show',target:dialogId};
  for(const slot of d.nodes.filter(n=>n.parent===listId+'Content' && n.template==='ItemSlot')) slot.action={type:'show',target:id+'Details'};
  validateDesign(d);
  design.nodes=d.nodes;
  return id;
}
export function layerStyle(n) {
  return graphicStyle(n)+baseLayerStyle(n)+(n.type==='Text' && n.verticalAlign ? `display:flex;flex-direction:column;justify-content:${n.verticalAlign};`:'');
}
function graphicStyle(n) {
  const r=n.rectTransform,p=r?.pivot??{x:0,y:1},scale=n.localScale??{x:1,y:1};
  return `transform-origin:${p.x*100}% ${(1-p.y)*100}%;transform:rotate(${-(n.rotation??0)}deg) scale(${scale.x},${scale.y});opacity:${n.opacity??1};`;
}
function baseLayerStyle(n) {
  if(n.image && n.imageBorder && n.imageType!=='simple') { const b=n.imageBorder;return `background:transparent;border-style:solid;border-color:transparent;border-width:0;border-image:url(/api/studio/assets/${n.image}) ${b.w} ${b.z} ${b.y} ${b.x} fill / ${b.w}px ${b.z}px ${b.y}px ${b.x}px stretch;`; }
  if(n.image && n.uvRect) {const u=n.uvRect;return `background:transparent url(/api/studio/assets/${n.image}) repeat;background-size:${100/u.width}% ${100/u.height}%;background-position:${-u.x*n.width/u.width}px ${(u.y+u.height-1)*n.height/u.height}px;`;}
  if(n.image) return `${n.imageTone==='light'?'filter:brightness(0) invert(1);':''}background:transparent url('/api/studio/assets/${n.image}') center / ${n.imageFit??'contain'} no-repeat;`;
  if(n.surface==='transparent') return 'background:transparent;';
  if (n.template === 'ConfirmDialog') return 'background:transparent;';
  if (n.template === 'ScrollPanel') return 'overflow-x:hidden;overflow-y:auto;overscroll-behavior:contain;';
  return `${n.role === 'frame' ? `background:transparent;border:${n.borderWidth ?? 6}px solid ${n.color};` : ''}${n.opacity !== undefined ? `opacity:${n.opacity};` : ''}${n.textAlign ? `text-align:${n.textAlign};` : ''}`;
}
export function validateDesign(value) {
  for(const n of value?.nodes??[])if(n.templateSourceId!==undefined&&(typeof n.templateSourceId!=="string"||!n.templateSourceId||n.templateSourceId.length>128))throw new Error("模板来源标识无效。");
  if (!value || value.schemaVersion !== 1 || !/^[A-Za-z][A-Za-z0-9_]{0,63}$/.test(value.name)) throw new Error('设计名称须为以字母开头的英文标识，最多 64 字符。');
  for (const key of ['width', 'height']) if (!Number.isInteger(value[key]) || value[key] < 240 || value[key] > 4096) throw new Error('设计分辨率须为 240–4096 的整数。');
  if (!Array.isArray(value.nodes) || value.nodes.length > 300) throw new Error('最多支持 300 个节点。');
  const ids = new Set();
  for (const n of value.nodes) {
    if (!n || n.id === 'Canvas' || !/^[A-Za-z][A-Za-z0-9_]{0,63}$/.test(n.id) || n.id === value.name || ids.has(n.id)) throw new Error('节点 ID 必须合法、唯一且不能与设计同名。');
    ids.add(n.id);
    if(n.systemKind!==undefined&&(n.systemKind!=='code'||n.type!=='Container'||n.parent!==null||value.nodes.some(child=>child.parent===n.id)))throw new Error('代码系统必须为独立根节点，不能包含 UI 子节点。');
    if(n.templateSourceVersion!==undefined&&(!Number.isInteger(n.templateSourceVersion)||n.templateSourceVersion<1))throw new Error('模板来源版本无效。');
    if(n.scripts!==undefined&&n.scripts!==null){if(n.scripts.schemaVersion!==1||!Array.isArray(n.scripts.files)||n.scripts.files.length>128||n.scripts.files.some(f=>typeof f.path!=='string'||typeof f.content!=='string')||JSON.stringify(n.scripts).length>5*1024*1024)throw new Error('项目源码副本格式无效。');}
    for(const [key,axes] of [['localScale',['x','y']],['imageBorder',['x','y','z','w']],['uvRect',['x','y','width','height']]]) if(n[key]!==undefined && (!n[key] || !axes.every(a=>Number.isFinite(n[key][a])&&Math.abs(n[key][a])<=8192)))throw new Error('图片布局参数无效。');
    if(n.imageBorder && Object.values(n.imageBorder).some(v=>v<0))throw new Error('九宫格边界不能为负。');
    if(n.uvRect && (n.uvRect.width<=0||n.uvRect.height<=0))throw new Error('UV 尺寸必须为正。');
    if(n.relationPosition!==undefined&&(n.parent!==null||!['x','y'].every(k=>Number.isFinite(n.relationPosition?.[k])&&n.relationPosition[k]>=0&&n.relationPosition[k]<=6000)))throw new Error('关系卡片坐标无效。');
    if(n.rotation!==undefined && (!Number.isFinite(n.rotation)||Math.abs(n.rotation)>36000))throw new Error('Rotation Z 无效。');
    if(n.animationHost!==undefined && (n.animationHost!==true||n.type!=='Container'))throw new Error('动画容器标记无效。');
    if(n.interactable!==undefined && typeof n.interactable!=='boolean')throw new Error('Interactable 状态无效。');
    if(n.active!==undefined && typeof n.active!=='boolean')throw new Error('Active 状态无效。');
    if(n.imageType!==undefined && !['simple','sliced'].includes(n.imageType))throw new Error('Image Type 无效。');
    if(n.verticalAlign!==undefined && !['start','center','end'].includes(n.verticalAlign))throw new Error('垂直对齐无效。');
    if(n.image!==undefined && !/^[a-f0-9]{64}$/.test(n.image))throw new Error('图片资源 ID 无效。');
    if(n.imageFit!==undefined && !['cover','contain','100% 100%'].includes(n.imageFit))throw new Error('图片填充方式无效。');
    if(n.progress!==undefined && (!Number.isFinite(n.progress)||n.progress<0||n.progress>1))throw new Error('进度须为 0–1。');
    if (n.name !== undefined && (typeof n.name !== 'string' || !/^[a-zA-Z][a-zA-Z0-9]{0,63}$/.test(n.name))) throw new Error('节点名称须使用大驼峰英文命名，例如 Background 或 ItemList。');
    if (!types.includes(n.type)) throw new Error('存在不支持的控件类型。');
    if (n.parent !== null && typeof n.parent !== 'string') throw new Error('父节点无效。');
    for (const key of ['x','y','width','height','fontSize']) if (!Number.isFinite(n[key]) || Math.abs(n[key]) > 8192) throw new Error('节点尺寸或坐标无效。');
    if (n.width < 0 || n.height < 0 || n.fontSize < 8 || n.fontSize > 128) throw new Error('尺寸不能为负，字号须为 8–128。');
    if (!/^#[0-9a-f]{6}$/i.test(n.color) || typeof n.text !== 'string' || n.text.length > 2000) throw new Error('颜色或文本无效。');
    if (n.template !== undefined && (!['ItemSlot','ScrollPanel','ConfirmDialog','InventorySystem'].includes(n.template) || n.type !== 'Container')) throw new Error('预制控件类型无效。');
    if (n.role !== undefined && !['background','icon','mask','count','frame','content'].includes(n.role)) throw new Error('图层语义无效。');
    if (n.opacity !== undefined && (!Number.isFinite(n.opacity) || n.opacity < 0 || n.opacity > 1)) throw new Error('不透明度须为 0–1。');
    if (n.borderWidth !== undefined && (!Number.isFinite(n.borderWidth) || n.borderWidth < 0 || n.borderWidth > 64)) throw new Error('边框宽度须为 0–64。');
    if (n.previewHidden !== undefined && typeof n.previewHidden !== 'boolean') throw new Error('预演初始状态无效。');
    if (n.action !== undefined && (!n.action || !['none','show','hide','toggle','navigate','back'].includes(n.action.type) || (!['none','back'].includes(n.action.type) && typeof n.action.target !== 'string'))) throw new Error('点击动作无效。');
    if (n.textAlign !== undefined && !['left','center','right'].includes(n.textAlign)) throw new Error('文本对齐无效。');
  }
  const map = new Map(value.nodes.map(n => [n.id, n]));
  for (const n of value.nodes) {
    if (n.action && !['none','back'].includes(n.action.type) && !map.has(n.action.target)) throw new Error('点击动作的目标节点不存在。');
    const seen = new Set([n.id]);
    let parent = n.parent;
    while (parent !== null) {
      if (!map.has(parent) || seen.has(parent)) throw new Error('层级存在循环或缺失父节点。');
      // All UGUI Graphics own RectTransforms and can contain child nodes (including Text).
      seen.add(parent); parent = map.get(parent).parent;
    }
  }
  return resolveLayout({ schemaVersion: 1, rootId: 'Canvas', name: value.name, width: value.width, height: value.height, ...(value.animations!==undefined?{animations:validateAnimations(value.animations,value.nodes)}:{}), nodes: value.nodes.map(n => ({...Object.fromEntries(['systemKind','id','parent','type','x','y','width','height','color','text','fontSize','template','role','opacity','borderWidth','textAlign','action','previewHidden','surface','image','imageFit','imageTone','progress','rectTransform','localScale','imageBorder','uvRect','verticalAlign','rotation','active','imageType','interactable','animationHost','templateSourceId','templateSourceVersion','scripts','relationPosition'].filter(k=>n[k] !== undefined).map(k => [k,n[k]])),name:nodeName(n)})) });
}
export function escapeHtml(s) { return String(s).replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c])); }
export function designHtml(input) {
  const d = validateDesign(input);
  const render = parent => d.nodes.filter(n => n.parent === parent && n.systemKind!=='code').map(n => {
    const owner=d.nodes.find(p=>p.id===n.parent);
    if(owner?.type==='Progress') {
      if(nodeName(n)==='Fill')n={...n,width:owner.width*(owner.progress??0)};
      if(nodeName(n)==='Percentage')n={...n,text:`Loading... ${Math.round((owner.progress??0)*100)}%`};
    }
    const isLabel = d.nodes.find(p => p.id === n.parent)?.type === 'Button';
    const style = `position:absolute;left:${n.x}px;top:${n.y}px;width:${n.width}px;height:${n.height}px;font-size:${n.fontSize}px;${n.type === 'Text' ? `color:${n.color};` : `background-color:${n.color};color:#101923;`}white-space:pre-wrap;${isLabel ? 'display:flex;align-items:center;justify-content:center;text-align:center;' : ''}`;
    const tag = n.type === 'Button' ? 'button' : 'div';
    return `<${tag} data-ui-id="${n.id}"${n.active===false?' hidden':''}${n.type==='Button'&&n.interactable===false?' disabled':''} data-ui-name="${nodeName(n)}" data-ui-type="${n.type}"${n.animationHost?' data-ui-animation-host="true"':''}${n.role ? ` data-studio-role="${n.role}"` : ''}${n.template ? ` data-studio-template="${n.template}"` : ''} style="${style}${layerStyle(n)}">${escapeHtml(n.text)}${render(n.id)}</${tag}>`;
  }).join('\n');
  return `<!doctype html><html lang="zh-CN"><head><meta charset="utf-8"><meta name="fui-design-resolution" content="${d.width}x${d.height}"><title>${d.name}</title><style>*{box-sizing:border-box}html,body{margin:0;width:${d.width}px;height:${d.height}px;overflow:hidden;font-family:Arial,"Microsoft YaHei",sans-serif}button{border:0;font:inherit;cursor:pointer}body{background:#121b2a}</style></head><body><div data-ui-id="Canvas" data-ui-type="Container" data-design-width="${d.width}" data-design-height="${d.height}" style="position:relative;width:${d.width}px;height:${d.height}px">${render(null)}</div></body></html>`;
}
