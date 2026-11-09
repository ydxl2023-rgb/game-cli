import {rectFields,setRectField,anchorPreset} from '../shared/inspector.mjs';
import {captureBounds} from '../shared/rect-transform.mjs';
export function createInspector({node,design,edit}) {
 const $=id=>document.getElementById(id), form=$('properties');
 function component(title){const d=document.createElement('details');d.className='unity-component';d.open=true;const h=document.createElement('summary');h.textContent=title;d.append(h);form.append(d);return d;}
 function field(parent,label,id,{type='number',disabled=false,options}={},change){
  const l=document.createElement('label');l.className='unity-field';const span=document.createElement('span');span.textContent=label;
  const input=document.createElement(options?'select':'input');input.id=id;
  if(options)for(const [value,text]of options)input.add(new Option(text,value));else{input.type=type;if(type==='number')input.step='any';}
  input.disabled=disabled;l.append(span,input);parent.append(l);
  if(change)input.onchange=()=>{if(!node()||input.value==='')return;const value=type==='checkbox'?input.checked:type==='number'&&!options?Number(input.value):input.value;edit(()=>change(node(),value));};
  if(change && type==='number' && !options)input.oninput=input.onchange;
  return input;
 }
 const oldBounds=$('field-x').parentElement.parentElement;oldBounds.hidden=true;
 const header=document.createElement('div');header.className='unity-object';form.prepend(header);header.append($('field-id').parentElement);
 field(header,'Active','unity-active',{type:'checkbox'},(n,v)=>n.active=v);
 const rect=component('Rect Transform');rect.id='unity-rect';
 const preset=document.createElement('details');preset.className='anchor-presets';preset.innerHTML='<summary>▣ Anchor Presets</summary><div class="preset-grid"></div><small>Shift：同时设置 Pivot · Alt：同时设置 Position</small>';rect.append(preset);
 for(let y=0;y<4;y++)for(let x=0;x<4;x++){
  const b=document.createElement('button');b.type='button';b.dataset.preset=`${x},${y}`;b.title=b.ariaLabel=`Anchor ${['Left','Center','Right','Stretch'][x]} ${['Top','Middle','Bottom','Stretch'][y]}`;
  b.innerHTML=`<i style="left:${x===3?12:12+x*32}%;top:${y===3?12:12+y*32}%;width:${x===3?76:12}%;height:${y===3?76:12}%"></i>`;
  b.onclick=e=>edit(()=>anchorPreset(design(),node(),x,y,{shift:e.shiftKey,alt:e.altKey}));preset.querySelector('.preset-grid').append(b);
 }
 const box=document.createElement('div');box.className='rect-position';rect.append(box);
 for(const key of ['firstX','firstY','secondX','secondY'])field(box,key,'unity-'+key,{},(n,v)=>setRectField(n.rectTransform,key,v));
 field(box,'Pos Z','unity-pos-z',{disabled:true}).value=0;
 const anchors=document.createElement('details');anchors.open=true;anchors.innerHTML='<summary>Anchors</summary>';rect.append(anchors);
 for(const [key,label]of [['anchorMin','Min'],['anchorMax','Max'],['pivot','Pivot']]){
  const row=document.createElement('div');row.className='unity-vector';const title=document.createElement('span');title.textContent=label;row.append(title);(key==='pivot'?rect:anchors).append(row);
  for(const a of ['x','y'])field(row,a.toUpperCase(),`unity-${key}-${a}`,{},(n,v)=>{n.rectTransform[key][a]=v;captureBounds(design(),n);});
 }
 for(const key of ['Rotation','Scale']){
  const row=document.createElement('div');row.className='unity-vector three';row.append(Object.assign(document.createElement('span'),{textContent:key}));rect.append(row);
  for(const a of ['x','y','z'])field(row,a.toUpperCase(),`unity-${key}-${a}`,{disabled:key==='Rotation'?a!=='z':a==='z'},(n,v)=>{if(key==='Rotation')n.rotation=v;else{n.localScale??={x:1,y:1};n.localScale[a]=v;}});
 }
 const note=document.createElement('small');note.textContent='当前为 2D Canvas：Pos Z、Rotation X/Y、Scale Z 暂不支持。';rect.append(note);
 const graphic=component('Image');graphic.id='unity-graphic';
 for(const id of ['field-color','field-opacity','field-borderWidth','field-text','field-fontSize'])graphic.append($(id).parentElement);
 field(graphic,'Source Image','unity-source',{type:'text',disabled:true});
 field(graphic,'Image Type','unity-image-type',{options:[['simple','Simple'],['sliced','Sliced']]},(n,v)=>{if(v==='sliced'&&!n.imageBorder)throw new Error('此资源尚无 Sprite Border 数据，不能使用 Sliced。');n.imageType=v;});
 field(graphic,'Preserve Aspect','unity-aspect',{type:'checkbox'},(n,v)=>n.imageFit=v?'contain':'100% 100%');
 const uv=document.createElement('div');uv.className='field-grid';graphic.append(uv);
 for(const a of ['x','y','width','height'])field(uv,'UV '+a,'unity-uv-'+a,{},(n,v)=>{n.uvRect??={x:0,y:0,width:1,height:1};n.uvRect[a]=v;});
 field(graphic,'Alignment','unity-align',{options:[['left','Left'],['center','Center'],['right','Right']]},(n,v)=>n.textAlign=v);
 field(graphic,'Vertical','unity-vertical',{options:[['start','Top'],['center','Middle'],['end','Bottom']]},(n,v)=>n.verticalAlign=v);
 const material=field(graphic,'Material','unity-material',{type:'text',disabled:true});material.value='Default UI Material';
 const hint=document.createElement('small');hint.textContent='资源引用只读；自定义 Shader、Unity 字体及 Best Fit 尚未模拟。';graphic.append(hint);
 const button=component('Button');
 field(button,'Interactable','unity-interactable',{type:'checkbox'},(n,v)=>n.interactable=v);
 field(button,'Transition','unity-transition',{type:'text',disabled:true}).value='None';
 const bn=document.createElement('small');bn.textContent='Color Tint、Sprite Swap、Animation 和 Navigation 尚未接入。';button.append(bn);
 const extra=component('Studio · 交互与预演');
 for(const id of ['progress-property','field-action','field-action-target','field-preview-hidden'])extra.append(id==='progress-property'?$(id):$(id).parentElement);
 form.append($('delete'));form.insertBefore(rect,graphic);
 // Existing legacy fields are kept for keyboard/drag synchronization, outside the Inspector presentation.
 for(const id of ['field-id','field-color','field-opacity','field-text','field-fontSize']){
  const l=$(id).parentElement;if(l.firstChild?.nodeType===3)l.firstChild.textContent=({'field-id':'Name','field-color':'Color','field-opacity':'Alpha','field-text':'Text','field-fontSize':'Font Size'})[id];
 }
 const set=(id,v)=>{const e=$(id);if(e.type==='checkbox')e.checked=v;else e.value=v;};
 return function render(){const n=node();if(!n?.rectTransform)return;const r=n.rectTransform;
  set('unity-active',n.active!==false);button.hidden=n.type!=='Button';set('unity-interactable',n.interactable!==false);
  for(const [key,f]of Object.entries(rectFields(r))){const e=$('unity-'+key);e.parentElement.querySelector('span').textContent=f.label;set(e.id,Number(f.value.toFixed(6)));}
  for(const key of ['anchorMin','anchorMax','pivot'])for(const a of ['x','y'])set(`unity-${key}-${a}`,Number(r[key][a].toFixed(6)));
  for(const a of ['x','y','z']){set(`unity-Rotation-${a}`,a==='z'?n.rotation??0:0);set(`unity-Scale-${a}`,n.localScale?.[a]??1);}
  for(const b of preset.querySelectorAll('button')){const [x,y]=b.dataset.preset.split(',').map(Number);const match=r.anchorMin.x===(x===3?0:x/2)&&r.anchorMax.x===(x===3?1:x/2)&&r.anchorMin.y===(y===3?0:1-y/2)&&r.anchorMax.y===(y===3?1:1-y/2);b.classList.toggle('chosen',match);b.setAttribute('aria-pressed',match);}
  const isText=n.type==='Text',isRaw=!!n.uvRect,hasGraphic=isText||n.type==='Image'||n.type==='Button'||!!n.image||n.surface!=='transparent';
  graphic.hidden=!hasGraphic;graphic.querySelector('summary').textContent=isText?'Text':isRaw?'Raw Image':'Image';
  set('unity-source',n.image??'None');$('unity-source').title=n.image??'';
  $('unity-source').parentElement.querySelector('span').textContent=isRaw?'Texture':'Source Image';
  for(const id of ['unity-source','unity-material'])$(id).parentElement.hidden=isText&&id==='unity-source';
  for(const id of ['unity-image-type','unity-aspect'])$(id).parentElement.hidden=isText||isRaw||!n.image;
  set('unity-image-type',n.imageType??(n.imageBorder?'sliced':'simple'));set('unity-aspect',n.imageFit==='contain');uv.hidden=!isRaw;
  for(const a of ['x','y','width','height'])set('unity-uv-'+a,n.uvRect?.[a]??(a==='x'||a==='y'?0:1));
  for(const id of ['unity-align','unity-vertical'])$(id).parentElement.hidden=!isText;
  set('unity-align',n.textAlign??'left');set('unity-vertical',n.verticalAlign??'start');
 };
}
