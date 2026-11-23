import {rows,fields,label} from './sv-shop-config.mjs';
import {readdir,copyFile,writeFile,mkdir} from 'node:fs/promises';
import path from 'node:path';
const shop='../../examples/unitys/development/client/Assets/Data/Widgets/Shop';
const src=process.env.SV_PROJECT??'G:/Works/SV/Development/Project';
const directory=shop+'/Demo/MonthCard';
const sharedIcons=path.join(shop,'../Common/Icons');
await mkdir(sharedIcons,{recursive:true});
await mkdir(directory,{recursive:true});
const items=new Map((await rows('item')).map(r=>[r.get(1)[0],r]));
const paths=new Map();
async function index(dir){for(const f of await readdir(dir,{withFileTypes:true})){const p=path.join(dir,f.name);if(f.isDirectory())await index(p);else if(/\.png$/i.test(f.name))paths.set(f.name.toLowerCase(),p);}}
await index(path.join(src,'Assets/data/UI'));
await index(path.join(src,'Assets/data/Language/Cn/UI'));
async function reward(id,count){
 const name=String(items.get(id).get(3)[0]);
 const file=paths.get(name.toLowerCase());
 if(!file)throw Error(name);
 await copyFile(file,path.join(sharedIcons,name.toLowerCase()));
 return {id,count,icon:'Assets/Data/Widgets/Common/Icons/'+name.toLowerCase()};
}
const cards=[];
for(const r of (await rows('mallrecharge')).filter(r=>[11040111,11040121].includes(r.get(1)[0]))){
 const immediate=[await reward(109,r.get(21)[0])],daily=[],total=[];
 for(const b of r.get(31)){const v=fields(b);immediate.push(await reward(v.get(1)[0],v.get(2)[0]));}
 for(const b of r.get(33)){const v=fields(b);daily.push(await reward(v.get(1)[0],v.get(2)[0]));total.push(await reward(v.get(1)[0],v.get(2)[0]*r.get(29)[0]));}
 cards.push({id:r.get(1)[0],price:label(r.get(14)[0]),immediate,total,daily});
}
await writeFile(directory+'/MonthCard.json',JSON.stringify({cards},null,2));
