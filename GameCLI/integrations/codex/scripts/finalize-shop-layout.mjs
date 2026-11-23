import {shopWidgetPath} from './shop-widget-path.mjs';
// Preserve source dynamic sizing/state defaults when replacing the SV runtime.
import {readFile,writeFile} from 'node:fs/promises';
const folder='.data/imports/sv-shop';
const contract=JSON.parse(await readFile(`${folder}/bindings.json`,'utf8'));
const names={main:'Shop',recharge:'ShopRecharge',gift:'ShopGift',giftbuy:'ShopPurchase'};
const operations=[];
for(const [key,name]of Object.entries(names)){
 const payload=JSON.parse(await readFile(`${folder}/${key}/template.json`,'utf8'));
 const nodes=payload.design.nodes,map=new Map(nodes.map(n=>[n.id,n]));
 const at=path=>map.get(name+path.replaceAll('/','_'));
 const native=id=>{const n=map.get(id);return n.parent?native(n.parent)+'/'+n.name:n.name;};
 const op=(node,component,property,value)=>operations.push({asset:shopWidgetPath(name),path:native(node.id),component,property,value:String(value)});
 for(const spec of contract.specs.filter(s=>s.page===key))for(const field of spec.fields.filter(f=>f.type==='bool'))for(const path of field.paths){const n=at(path);n.active=false;op(n,'GameObject','m_IsActive','false');}
 if(key==='main'){
  // The source hides secondary navigation when there is only one child category.
  for(const path of ['0/3/0/2','0/3/0/1','0/3/0/3','0/2']){at(path).active=false;op(at(path),'GameObject','m_IsActive','false');}
 }
 if(key==='main'){
  at('0/3/2/1/0').type='Image';at('0/3/2/1/1').type='Button';
  at('0/3/5').rectTransform.anchoredPosition={x:-700,y:980};
  at('0/3/5/0').rectTransform.sizeDelta={x:1000,y:60};
 }
 if(key==='giftbuy'){
  at('0/1/0/2').rectTransform.sizeDelta={x:450,y:90};
  for(const [path,x,width]of [['0/1/0/2/0/0',-105,180],['0/1/0/2/0/1',90,200]]){
   at(path).rectTransform.anchoredPosition={x,y:0};at(path).rectTransform.sizeDelta={x:width,y:65};
  }
  // The local simulation uses a formatted price, not the source gold currency.
  const icon=nodes.find(n=>n.name==='GoldIcon'&&n.parent===at('0/1/0/2/0').id);icon.opacity=0;
 }
 if(key==='recharge'){
  const arrow=nodes.find(n=>n.name==='Arrowup');if(arrow?.rectTransform?.localRotation)arrow.rectTransform.localRotation.z=-1;
  const price=at('0/1/0/0/0/1/2');price.rectTransform.sizeDelta.x=240;
  op(price,'RectTransform','m_SizeDelta','240,72.5');
 }
 // Mirror the actual Fancy conversion, including stripped descendant buttons.
 for(const grid of contract.grids.filter(g=>g.page===key)){
  const content=nodes.find(n=>native(n.id)===grid.content);
  Object.assign(content.rectTransform,{anchorMin:{x:0,y:0},anchorMax:{x:1,y:1},pivot:{x:0.5,y:0.5},sizeDelta:{x:0,y:0},anchoredPosition:{x:0,y:0}});
  const cell=nodes.find(n=>native(n.id)===grid.cell);cell.rectTransform.pivot={x:0.5,y:0.5};
  for(const n of nodes)if(native(n.id).startsWith(grid.cell+'/')&&n.type==='Button')n.type='Image';
 }
 for(const n of nodes)if(n.type==='Image'&&!n.image&&n.surface==='transparent')n.opacity=0;
 // Dynamic text originally relies on SV fitters. Keep a single-line overflow
 // allowance for values while retaining the source anchor and font size.
 for(const spec of contract.specs.filter(s=>s.page===key))for(const field of spec.fields.filter(f=>f.sync==='SyncText'))for(const path of field.paths){op(at(path),'Text','m_FontData.m_HorizontalOverflow','1');op(at(path),'Text','m_FontData.m_VerticalOverflow','1');}
 await writeFile(`${folder}/${key}/template.json`,JSON.stringify(payload,null,2));
}
await writeFile(`${folder}/layout-operations.json`,JSON.stringify(operations,null,2));
console.log(JSON.stringify({operations:operations.length}));
