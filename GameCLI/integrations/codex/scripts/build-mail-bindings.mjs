// Build explicit VM/Metadata contracts from reviewed native paths.
import {readFile,writeFile,mkdir} from 'node:fs/promises';
const folder='.data/imports/sv-mail',pages={},specs=[];
for(const key of ['main'])pages[key]=JSON.parse(await readFile(`${folder}/${key}/template.json`,'utf8'));
const pageName={main:'Mail'};
const f=(name,type,sync,paths,component)=>({name,type,sync,paths:Array.isArray(paths)?paths:[paths],component});
const text=(name,paths)=>f(name,'string','SyncText',paths,'Text');
const image=(name,path)=>f(name,'Sprite','SyncSprite',path,'Image');
const visible=(name,path)=>f(name,'bool','SyncVisible',path,'RectTransform');
const click=(name,path)=>f(name,'ButtonClick','SyncClick',path,'Button');
const list=(name,type,path)=>f(name,`VList<${type}>`,'SyncFancyGridScrollView',path,'FancyGridScrollView');
const gridClick=(name,path)=>f(name,'FancyGridClick','SyncFancyGridClick',path,'FancyGridScrollView');
function add(name,page,root,fields,cell=false){specs.push({name,page,root,fields,cell});}

const item='0/3/1/1', frame=item+'/0', reward='0/3/0/2/1', icon=reward+'/0/0/0';
const interact=(name,path)=>f(name,'bool','SyncInteractable',path,'Button');
add('Mail','main','0',[
 click('closeClick','0/4'),click('claimClick','0/3/0/0/1'),click('deleteClick','0/3/0/0/2'),click('claimAllClick','0/3/2/1'),click('deleteReadClick','0/3/2/0'),
 list('messages','VMMailEntry','0/3/1'),gridClick('messageClick','0/3/1'),visible('messageTemplate',item),
 list('rewards','VMRewardEntry','0/3/0/2'),visible('rewardTemplate',reward),
 text('title','0/3/0/1/0'),text('sentAt','0/3/0/1/1'),text('body','0/3/0/1/3/0/0/0'),text('expiry','0/3/0/1/2/0'),
 text('total','0/1/3/3/2'),text('unread','0/1/3/3/4'),
 visible('detailVisible','0/3/0'),visible('emptyVisible',['0/1/3/9']),
 visible('claimVisible','0/3/0/0/1'),visible('deleteVisible','0/3/0/0/2'),
 interact('canClaimAll','0/3/2/1'),interact('canDeleteRead','0/3/2/0'),
 f('envelopeState','string','SyncAnimator','0/1/3','Animator')
]);
add('MailEntry','main',item,[
 text('title',[frame+'/0/0',frame+'/1/0',frame+'/2/0']),text('sentAt',[frame+'/0/1',frame+'/1/1',frame+'/2/1']),
 visible('unselected',frame+'/0'),visible('received',frame+'/1'),visible('selected',frame+'/2'),visible('unread',frame+'/4'),
 visible('hasAttachment',frame+'/3'),visible('claimed',frame+'/3/0'),visible('pending',frame+'/3/1')
],true);
add('RewardEntry','main',reward,[
 f('item','VMItemSlot','SyncMetadata',reward+'/0','Oathx.Widget.UIItemSlot'),visible('claimed',reward+'/1')
],true);
function node(page,path){return pages[page].design.nodes.find(n=>n.id===pageName[page]+path.replaceAll('/','_'));}
function nativePath(page,id){const n=pages[page].design.nodes.find(n=>n.id===id);return n.parent?nativePath(page,n.parent)+'/'+n.name:n.name;}
for(const spec of specs){
 spec.nativeRoot=nativePath(spec.page,node(spec.page,spec.root).id);
 for(const field of spec.fields)field.targets=field.paths.map(path=>{const n=node(spec.page,path);if(!n)throw Error(`${spec.name}.${field.name} missing ${path}`);return nativePath(spec.page,n.id);});
 const vm=`using Oathx.Widget;\n\n/// <summary>Presentation bindings for ${spec.name}.</summary>\npublic partial class VM${spec.name} : VM\n{\n`+spec.fields.map(field=>`    /// <summary>Bound ${field.name} presentation value.</summary>\n    [Binding${field.type==='ButtonClick'?'':`(typeof(${field.sync}))`}]\n    ${field.type} ${field.name};`).join('\n\n')+'\n}\n';
 const ui=`using Oathx.Widget;\n${spec.cell?'using UnityEngine;\n':''}\n/// <summary>Displays the ${spec.name} view through serialized Metadata.</summary>\n`+(spec.cell?'[RequireComponent(typeof(FancyGridScrollViewCell), typeof(Metadata))]':`[Config("Assets/Data/Widgets/${pageName[spec.page]}/${pageName[spec.page]}.prefab", WidgetLayer.topmost, cacheWidget: false)]`)+`\npublic class UI${spec.name} : UIWidget<VM${spec.name}>\n{\n}\n`;
 // Maintained runtime sources are authoritative; never regenerate their behavior.
 for(const dir of [`VM/VM${spec.name}.cs`,`UI/UI${spec.name}.cs`]){
  const source=spec.name==='RewardEntry'?`Common/UI/${dir.startsWith('UI/')?dir.slice(3):dir}`:`Mail/${dir}`;
  const content=await readFile(`../../examples/unitys/development/client/Assets/Scripts/Systems/${source}`,'utf8');
  await mkdir(`${folder}/code/${dir.split('/')[0]}`,{recursive:true});await writeFile(`${folder}/code/${dir}`,content);
 }
}

const grids=[
 {page:'main',view:'0/3/1',content:'0/3/1/0/0',cell:item,widget:'MailEntry',size:[545.1753,112],columns:1,spacing:8},
 {page:'main',view:'0/3/0/2',content:'0/3/0/2/0/0',cell:reward,widget:'RewardEntry',size:[152,152],columns:5,spacing:8}
];
for(const grid of grids)for(const field of ['view','content','cell'])grid[field]=nativePath(grid.page,node(grid.page,grid[field]).id);
await writeFile(folder+'/bindings.json',JSON.stringify({specs,grids},null,2));
console.log(JSON.stringify({widgets:specs.length,fields:specs.reduce((sum,s)=>sum+s.fields.length,0)}));
