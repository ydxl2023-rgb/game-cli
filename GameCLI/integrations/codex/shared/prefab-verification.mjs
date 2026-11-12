import {nodeName} from './design.mjs';
import {resolveLayout} from './rect-transform.mjs';
import {verifyWidgetBindings} from './widget-binding-verification.mjs';

export const verificationPolicy='studio-prefab/2';
export const numericTolerance=0.0001;

// IDs identify nodes within each document; comparison uses hierarchy and sibling order.
export function selectPrefab(design,id){
 const root=design.nodes.find(n=>n.id===id&&n.parent===null);
 if(!root)throw Error('预制 ID 必须是当前项目中的根节点 ID。');
 if(root.systemKind==='code')throw Error('纯代码系统没有可验收的预制体。');
 const ids=new Set([id]);let count;
 do{count=ids.size;for(const n of design.nodes)if(ids.has(n.parent))ids.add(n.id);}while(count!==ids.size);
 const selected=structuredClone({...design,nodes:design.nodes.filter(n=>ids.has(n.id)),animations:(design.animations??[]).filter(a=>ids.has(a.hostNodeId))});
 resolveLayout(selected);return selected;
}

export function comparePrefab(design,document,diagnostics=[]){
 if(document.schemaVersion!=='ui-template/1'||!Array.isArray(document.prefab?.nodes))throw Error('不支持的 Unity 导出格式。');
 const differences=[],unverified=[];
 const bindings=verifyWidgetBindings(document);
 differences.push(...bindings.differences.map(item=>({...item,property:'widgetBinding',source:'widget-bindings'})));
 unverified.push(...bindings.unverified.map(item=>({...item,source:'widget-bindings'})));
 const diff=(node,property,expected,actual)=>differences.push({node,property,expected,actual:actual??null});
 function equal(node,property,expected,actual){
  if(typeof expected==='number'){
   if(!Number.isFinite(actual)||Math.abs(expected-actual)>numericTolerance)diff(node,property,expected,actual);
  }else if(Array.isArray(expected)){
   if(!Array.isArray(actual)||expected.length!==actual.length)diff(node,property,expected,actual);
   else expected.forEach((v,i)=>equal(node,`${property}[${i}]`,v,actual[i]));
  }else if(expected!==actual)diff(node,property,expected,actual);
 }
 function paths(nodes,parentKey,idKey,name){
  const map=new Map(),byId=new Map(nodes.map(n=>[n[idKey],n]));
  if(byId.size!==nodes.length)throw Error('导出包含重复节点 ID。');
  function walk(n,seen=new Set()){
   if(seen.has(n[idKey]))throw Error('节点层级循环。');
   seen.add(n[idKey]);const parent=n[parentKey]==null?null:byId.get(n[parentKey]);
   if(n[parentKey]!=null&&!parent)throw Error('节点父级缺失。');
   const siblings=nodes.filter(x=>x[parentKey]===n[parentKey]);
   const index=parentKey==='parentId'?n.siblingIndex:siblings.indexOf(n);
   const path=(parent?walk(parent,seen)+'/':'')+index+':'+name(n);
   return path;
  }
  for(const n of nodes){const path=walk(n);if(map.has(path))throw Error('节点路径重复。');map.set(path,n);}return map;
 }
 const expected=paths(design.nodes,'parent','id',nodeName),actual=paths(document.prefab.nodes,'parentId','id',n=>n.name);
 for(const d of diagnostics)unverified.push({code:'unity-export-diagnostic',detail:d});
 if(design.animations?.length)unverified.push({code:'animation-evidence-unavailable',message:'当前 Unity 导出器不提供完整 Animator 曲线证据，禁止判定动画一致。'});
 for(const [path,n] of expected){
  const a=actual.get(path);if(!a){diff(path,'node','present','missing');continue;}
  equal(path,'active',n.active!==false,a.active);
  if(n.scripts?.files?.length)unverified.push({node:path,code:'script-binding-contract-unavailable',message:'已检查可读取的 VM/Metadata 静态一致性；源码包不包含产品预期目标映射，不能证明绑定到了设计指定的节点。'});
  equal(path,'kind','Node',a.kind);
  for(const key of ['anchorMin','anchorMax','pivot','anchoredPosition','sizeDelta'])equal(path,'rect.'+key,[n.rectTransform[key].x,n.rectTransform[key].y],a.rect?.[key]);
  equal(path,'localScale',[n.localScale?.x??1,n.localScale?.y??1,1],a.localScale);
  const angle=(n.rotation??0)*Math.PI/360;
  let rotation=a.localRotation;if(rotation?.[3]<0)rotation=rotation.map(v=>-v);
  equal(path,'localRotation',[0,0,Math.sin(angle),Math.cos(angle)],rotation);
  const components=a.components??[];
  const hasImage=!!n.image||n.type!=='Container'||n.surface!=='transparent';
  const graphic=n.type==='Text'?'UnityEngine.UI.Text':n.uvRect?'UnityEngine.UI.RawImage':hasImage?'UnityEngine.UI.Image':null;
  const allowed=new Set(['UnityEngine.CanvasRenderer',...(graphic?[graphic]:[]),...(n.type==='Button'?['UnityEngine.UI.Button']:[])]);
  for(const c of components)if(!allowed.has(c.type))unverified.push({node:path,code:'undeclared-component',component:c.type});
  const find=type=>components.filter(c=>c.type===type);
  for(const type of [...allowed].filter(t=>t!=='UnityEngine.CanvasRenderer'))if(find(type).length!==1)diff(path,'component.'+type,1,find(type).length);
  const props=find(graphic)[0]?.properties;
  if(props){
   const hex=n.color.replace('#','');const rgba=[0,2,4].map(i=>parseInt(hex.slice(i,i+2),16)/255);
   rgba.push((hex.length===8?parseInt(hex.slice(6,8),16)/255:1)*(n.opacity??1));
   if(n.type==='Button'&&!n.image&&n.surface==='transparent')rgba[3]=0;
   equal(path,'color',rgba,props.m_Color);
   equal(path,'graphic.enabled',true,props.m_Enabled);
   equal(path,'raycastTarget',n.type==='Button',props.m_RaycastTarget);
   if(n.type==='Text'){
    equal(path,'text',n.text??'',props.m_Text);equal(path,'fontSize',Math.round(n.fontSize??20),props['m_FontData.m_FontSize']);
    const vertical={top:0,start:0,center:1,bottom:2,end:2}[n.verticalAlign??'center'];
    const horizontal={left:0,center:1,right:2}[n.textAlign??'left'];
    equal(path,'alignment',vertical*3+horizontal,props['m_FontData.m_Alignment']);
    unverified.push({node:path,code:'font-contract-unavailable',message:'项目设计尚无可比较的字体资源身份。'});
   }
   if(n.image)unverified.push({node:path,code:'resource-hash-unavailable',expected:n.image});
  }
  if(n.type==='Button'){
   const p=find('UnityEngine.UI.Button')[0]?.properties;
   equal(path,'interactable',n.interactable!==false,p?.m_Interactable);
   equal(path,'button.enabled',true,p?.m_Enabled);
   equal(path,'targetGraphic.node',a.id,p?.m_TargetGraphic?.nodeId);
   equal(path,'targetGraphic.component',graphic,p?.m_TargetGraphic?.componentType);
  }
  if(n.action)unverified.push({node:path,code:'interaction-contract-unavailable',expected:n.action});
  if(n.animationHost||n.type==='Progress'||n.role==='frame'||n.template==='ScrollPanel'||n.imageBorder||n.uvRect||n.imageTone||n.previewHidden)unverified.push({node:path,code:'unsupported-design-feature',message:'该节点包含当前比较适配器尚未覆盖的设计属性。'});
 }
 for(const path of actual.keys())if(!expected.has(path))diff(path,'node','absent','unexpected');
 return {policy:verificationPolicy,tolerance:numericTolerance,status:unverified.length?'incomplete':differences.length?'failed':'passed',passed:!unverified.length&&!differences.length,differences,unverified,bindings};
}
