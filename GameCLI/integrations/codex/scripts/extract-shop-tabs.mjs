import {writeFile} from 'node:fs/promises';
import {rows,label} from './sv-shop-config.mjs';
const tabs = (await rows('malltypeconfig')).map(row=>({
  id:row.get(1)[0], title:label(row.get(2)?.[0]), sort:row.get(3)?.[0], icon:String(row.get(6)?.[0] ?? ''), pages:[]
}));
for (const row of await rows('mallconfig')) {
  const id = row.get(1)[0], first = row.get(10)?.[0];
  const tab = tabs.find(t=>t.id === (first > 0 ? first : Math.floor(id/100)));
  if (tab) tab.pages.push({id,title:label(row.get(8)?.[0]),window:String(row.get(6)?.[0] ?? ''),sort:row.get(4)?.[0],unlock:row.get(7)?.[0]});
}
await writeFile('reports/shop-item-slot/source-tabs.json',JSON.stringify(tabs,null,2)+'\n');
for (const t of tabs) console.log(t.id,t.title,[...new Set(t.pages.map(p=>p.window))].join(','));

