import {rows,fields,label} from './sv-shop-config.mjs';
import {mkdir,writeFile,copyFile,readdir} from 'node:fs/promises';
import path from 'node:path';
const root=process.env.SV_PROJECT??'G:/Works/SV/Development/Project';
const dest='../../examples/unitys/development/client/Assets/Data/Widgets/Shop/Demo';
const get=(r,k)=>r?.get(k)?.[0];
const items=new Map((await rows('item')).map(r=>[get(r,1),r]));
const recharge=new Map((await rows('mallrecharge')).filter(r=>get(r,5)===1).map(r=>[get(r,27),r]));
const goods=await rows('goodsconfig'), tabs=await rows('mallconfig');
const icons=new Map();
async function index(dir){for(const f of await readdir(dir,{withFileTypes:true})){const p=path.join(dir,f.name);if(f.isDirectory())await index(p);else if(/\.png$/i.test(f.name))icons.set(f.name.toLowerCase(),p);}}
await index(path.join(root,'Assets/data/UI'));
await index(path.join(root,'Assets/data/Language/Cn/UI'));
const sharedIcons=path.resolve(dest,'../../Common/Icons');
await mkdir(sharedIcons,{recursive:true});
await mkdir(dest+'/Images',{recursive:true});
const image=name=>path.basename(String(name??'')).toLowerCase();
const catalog={level:60,passPrice:"¥1600",tracks:[]};
for(const tab of tabs){
 const id=get(tab,1),title=label(get(tab,8)).trim(),group=get(tab,10),window=String(get(tab,6));
 if(!(group===103&&window==='DailyTabView'||group===119||id===11001))continue;
 let entries=[];
 for(const r of goods.filter(r=>get(r,3)===id).sort((a,b)=>(get(a,18)??0)-(get(b,18)??0))){
  const offer=recharge.get(get(r,9)),item=items.get(get(r,2));
  if(!item&&!offer)continue;
  const name=label(offer?get(offer,4):get(item,2));
  const icon=image(offer && id!==11001?get(offer,9):get(item,3));
  if(!icons.has(icon))continue;
  let price=get(r,7)>0?get(r,7):Math.max(0,get(r,5)||0);
  const currency=price ? image(get(items.get(get(r,6)),3)) : '';
  let preview='';
  if(id===11001){preview=icon.replace('icon_bizhi_','wallpaper_');if(preview===icon || !icons.has(preview) || entries.some(e=>e.preview===preview))continue;}
  if(entries.some(e=>e.name===name && e.icon===icon && e.price===String(price) && e.count===get(r,10)))continue;
  entries.push({id:get(r,1),name,icon,preview,price:offer?label(get(offer,14)):price?String(price):'免费',currency:offer?'':currency,count:Math.max(1,get(r,10)||1),limit:Math.max(0,get(r,11)||0),discount:get(tab,1)===11915 && get(r,18)===2?"8折":label(get(offer,20)),state:id===11915&&get(r,18)===1?'已购买':id===11915&&get(r,18)===2?'合宿远征15-15':''});
 }
 if(id===11001) entries.sort((a,b)=>Number(b.icon.includes('anniversary2d5'))-Number(a.icon.includes('anniversary2d5'))||a.id-b.id);
 if(id===10302){
  entries=entries.filter(e=>e.id!==10901045 && e.name!=='超级招募券');
  entries.sort((a,b)=>Number(b.name==='金币')-Number(a.name==='金币'));
  const adItem=[...items.values()].find(r=>label(get(r,2))==='广告补给礼包');
  if(adItem && !entries.some(e=>e.name==='广告补给礼包')) entries.unshift({id:-10302,name:'广告补给礼包',icon:image(get(adItem,3)),count:1,limit:3,price:'广告获取'});
}
if(entries.length)catalog.tracks.push({id,title,entries:entries.slice(0,id===11001?8:group===119?8:18)});
}
const milestones=(await rows('growthfundconfig')).slice(0,12).map(r=>{
 const reward=k=>{const kv=fields(get(r,k));return {icon:image(get(items.get(get(kv,1)),3)),count:get(kv,2)};};
 return {id:get(r,1),name:String(get(r,2)),icon:reward(3).icon,count:reward(3).count,preview:image(get(r,5)),price:String(reward(4).count),state:get(r,2)<=catalog.level?'已领取':''};
});
catalog.tracks.push({id:11301,title:'超值返还',entries:milestones});
for(const track of catalog.tracks)for(const e of track.entries)for(const k of ['icon','preview','currency']){
 if(!e[k])continue;
 if(!icons.has(e[k])){e[k]='';continue;}
 const directory=e[k].startsWith('wallpaper_') ? path.join(dest,'Images') : sharedIcons;
 await copyFile(icons.get(e[k]),path.join(directory,e[k]));
}
await writeFile(dest+'/ShopDemo.json',JSON.stringify(catalog,null,2));
console.log(catalog.tracks.map(t=>[t.id,t.title,t.entries.length]));



