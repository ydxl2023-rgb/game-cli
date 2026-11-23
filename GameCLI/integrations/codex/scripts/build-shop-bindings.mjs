import {shopWidgetPath} from './shop-widget-path.mjs';
// Build explicit VM/Metadata contracts from reviewed native paths.
import {readFile,writeFile,mkdir} from 'node:fs/promises';
const folder='.data/imports/sv-shop',pages={},specs=[];
for(const key of ['main','recharge','gift','giftbuy'])pages[key]=JSON.parse(await readFile(`${folder}/${key}/template.json`,'utf8'));
const pageName={main:'Shop',recharge:'ShopRecharge',gift:'ShopGift',giftbuy:'ShopPurchase'};
const f=(name,type,sync,paths,component)=>({name,type,sync,paths:Array.isArray(paths)?paths:[paths],component});
const text=(name,paths)=>f(name,'string','SyncText',paths,'Text');
const image=(name,path)=>f(name,'string','SyncImage',path,'Image');
const visible=(name,path)=>f(name,'bool','SyncVisible',path,'RectTransform');
const click=(name,path)=>f(name,'ButtonClick','SyncClick',path,'Button');
const list=(name,type,path)=>f(name,`VList<${type}>`,'SyncFancyGridScrollView',path,'FancyGridScrollView');
const gridClick=(name,path)=>f(name,'FancyGridClick','SyncFancyGridClick',path,'FancyGridScrollView');
function add(name,page,root,fields,cell=false){specs.push({name,page,root,fields,cell});}
const group='0/3/0/0/0/0/0/0',groupView='0/3/0/0/0/0',tab='0/3/0/2/2',tabView='0/3/0/2/0';
add('Shop','main','0',[
 click('closeClick','0/3/1/1/0'),click('helpClick','0/3/1/1/2'),click('previousClick','0/3/0/1'),click('nextClick','0/3/0/3'),
 ...[0,1,2].flatMap(i=>[text('currency'+(i+1),`0/3/2/${i}/2`),click('currency'+(i+1)+'Click',`0/3/2/${i}/${i===1?1:0}`),click('addCurrency'+(i+1)+'Click',`0/3/2/${i}/3`)]),
 text('message','0/3/5/0'),visible('messageVisible','0/3/5'),list('groups','VMShopGroup',groupView),gridClick('groupClick',groupView),visible('groupTemplate',group),
 list('tabs','VMShopTab',tabView),gridClick('tabClick',tabView),visible('tabTemplate',tab)
]);
add('ShopGroup','main',group,[text('title',[group+'/0/0/1',group+'/0/1/1']),visible('selected',group+'/0/1'),visible('unselected',group+'/0/0'),visible('redPoint',group+'/0/0/2'),visible('selectedRedPoint',group+'/0/1/2')],true);
add('ShopTab','main',tab,[text('title',[tab+'/0/0/0',tab+'/0/1/0']),visible('selected',tab+'/0/1'),visible('unselected',tab+'/0/0')],true);
const recharge='0/1/0/0/0',gift='0/3/0/0/0';
add('ShopRecharge','recharge','0',[list('products','VMShopRechargeItem','0/1/0'),gridClick('productClick','0/1/0'),visible('productTemplate',recharge)]);
add('ShopRechargeItem','recharge',recharge,[text('price',recharge+'/1/2'),text('title',recharge+'/1/4/0'),text('bonus',recharge+'/1/6'),image('icon',recharge+'/1/3'),visible('firstPay',recharge+'/1/5'),text('firstPayText',recharge+'/1/5/0')],true);
add('ShopGift','gift','0',[list('products','VMShopGiftItem','0/3/0'),gridClick('productClick','0/3/0'),visible('productTemplate',gift)]);
add('ShopGiftItem','gift',gift,[text('price',gift+'/0/1/1'),text('title',gift+'/0/3/0'),text('limit',gift+'/0/4'),image('icon',gift+'/0/2/0/0/2'),text('count',gift+'/0/2/0/6/0'),visible('soldOut',gift+'/0/6'),text('soldOutText',gift+'/0/6/0/0')],true);
add('ShopPurchase','giftbuy','0',[
 click('closeClick',['0/0','0/1/1']),click('confirmClick','0/1/0/2'),text('confirmText','0/1/0/2/0/1'),text('price','0/1/0/2/0/0'),text('title','0/1/2/0'),image('icon','0/1/2/2'),
 text('rewardTitle','0/1/3/0/0/0/0'),text('rewardName','0/1/3/0/0/1/0/2'),text('rewardCount','0/1/3/0/0/1/0/3'),image('rewardIcon','0/1/3/0/0/1/0/1/0/0/0/2'),
 visible('alternatePurchase','0/1/0/3'),visible('freeText','0/1/0/2/0/2'),visible('randomTitle','0/1/3/0/0/2'),visible('randomRewards','0/1/3/0/0/3'),visible('discount','0/1/2/4'),visible('labels','0/1/2/5')
]);
function node(page,path){return pages[page].design.nodes.find(n=>n.id===pageName[page]+path.replaceAll('/','_'));}
function nativePath(page,id){const n=pages[page].design.nodes.find(n=>n.id===id);return n.parent?nativePath(page,n.parent)+'/'+n.name:n.name;}
for(const spec of specs){
 spec.nativeRoot=nativePath(spec.page,node(spec.page,spec.root).id);
 for(const field of spec.fields)field.targets=field.paths.map(path=>{const n=node(spec.page,path);if(!n)throw Error(`${spec.name}.${field.name} missing ${path}`);return nativePath(spec.page,n.id);});
 const vm=`using Oathx.Widget;\n\n/// <summary>Presentation bindings for ${spec.name}.</summary>\npublic partial class VM${spec.name} : VM\n{\n`+spec.fields.map(field=>`    /// <summary>Bound ${field.name} presentation value.</summary>\n    [Binding${field.type==='ButtonClick'?'':`(typeof(${field.sync}))`}]\n    ${field.type} ${field.name};`).join('\n\n')+'\n}\n';
 const ui=`using Oathx.Widget;\n${spec.cell?'using UnityEngine;\n':''}\n/// <summary>Displays the ${spec.name} view through serialized Metadata.</summary>\n`+(spec.cell?'[RequireComponent(typeof(FancyGridScrollViewCell), typeof(Metadata))]':`[Config("${shopWidgetPath(pageName[spec.page])}", WidgetLayer.topmost, cacheWidget: false)]`)+`\npublic class UI${spec.name} : UIWidget<VM${spec.name}>\n{\n}\n`;
 for(const [dir,content]of [[`VM/VM${spec.name}.cs`,vm],[`UI/UI${spec.name}.cs`,ui]]){await mkdir(`${folder}/code/${dir.split('/')[0]}`,{recursive:true});await writeFile(`${folder}/code/${dir}`,content);}
}
const grids=[
 {page:'main',view:groupView,content:group+'/..',cell:group,widget:'ShopGroup',size:[261.8235,109.3212],columns:1,spacing:8},
 {page:'main',view:tabView,content:tabView+'/0',cell:tab,widget:'ShopTab',size:[280,68],columns:5,spacing:0},
 {page:'recharge',view:'0/1/0',content:'0/1/0/0',cell:recharge,widget:'ShopRechargeItem',size:[345.0117,416.8375],columns:4,spacing:20},
 {page:'gift',view:'0/3/0',content:'0/3/0/0',cell:gift,widget:'ShopGiftItem',size:[300.7105,408.3737],columns:4,spacing:20}
];
grids[0].content=group.slice(0,group.lastIndexOf('/'));
for(const grid of grids)for(const field of ['view','content','cell'])grid[field]=nativePath(grid.page,node(grid.page,grid[field]).id);
await writeFile(`${folder}/bindings.json`,JSON.stringify({specs,grids},null,2));
console.log(JSON.stringify({widgets:specs.length,fields:specs.reduce((sum,s)=>sum+s.fields.length,0)}));
