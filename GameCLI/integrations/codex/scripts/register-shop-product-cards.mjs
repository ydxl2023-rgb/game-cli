import assert from 'node:assert/strict';
import { openDatabase } from '../server/database.mjs';
import { validateTemplate } from '../shared/templates.mjs';
import { validateScripts } from '../server/template-scripts.mjs';

const db = await openDatabase();
const client = await db.pool.connect();
try {
  await client.query('BEGIN');
  const shop = (await client.query("SELECT * FROM gamecli_ui.templates WHERE id='SvShop' FOR SHARE")).rows[0];
  const files = shop.scripts.files;
  const read = suffix => JSON.parse(files.find(f => f.path.endsWith(suffix)).content);
  const catalog = read('/product-cards.json');
  const materials = read('/materials.json').files;
  const byPath = new Map(materials.map(f => [f.path, f]));
  const byGuid = new Map(materials.filter(f => f.path.endsWith('.meta')).map(f => [/^guid: (\w+)/m.exec(f.content)?.[1], f.path.slice(0,-5)]));
  await client.query("INSERT INTO gamecli_ui.template_groups(category_id,id,name,sort) VALUES('prefab','shop-products','商城商品卡',56) ON CONFLICT DO NOTHING");
  await client.query("SELECT set_config('gamecli.publish','approved',true)");
  for (const card of catalog.cards) {
    const id = card.root;
    const source = read('/Designs/' + card.kind.toLowerCase() + '.json').design;
    let parent = null;
    for (const name of card.cell.split('/')) {
      const node = source.nodes.find(n => n.parent === parent && (parent === null || n.name === name));
      assert(node, name);
      parent = node.id;
    }
    const ids = new Set([parent]);
    for (let size = -1; size !== ids.size;) {
      size = ids.size;
      for (const n of source.nodes) if (ids.has(n.parent)) ids.add(n.id);
    }
    const nodes = structuredClone(source.nodes.filter(n => ids.has(n.id)));
    const root = nodes.find(n => n.id === parent);
    Object.assign(root,{parent:null,name:id,x:0,y:0});
    const owned = new Map();
    function include(path) {
      if (owned.has(path)) return;
      const file = byPath.get(path);
      assert(file,path);
      owned.set(path,file);
      if (byPath.has(path+'.meta')) include(path+'.meta');
      for (const match of (file.content || '').matchAll(/guid: ([a-f0-9]{32})/g)) {
        const dep = byGuid.get(match[1]);
        if (dep && byPath.has(dep)) include(dep);
      }
    }
    include(card.card);
    const scripts = validateScripts({schemaVersion:1,files:[
      ...files.filter(f => new RegExp('/(UI|VM)'+id+'\\.cs(\\.meta)?$').test(f.path)).map(({path,content})=>({path,content})),
      {path:'GameCLI/Templates/'+id+'/materials.json',content:JSON.stringify({schemaVersion:1,dependencies:catalog.dependencies,files:[...owned.values()]})},
      {path:'GameCLI/Templates/'+id+'/bindings.json',content:JSON.stringify(card)}
    ]});
    assert.equal(scripts.files.length,6);
    const payload = validateTemplate({root:root.id,design:{schemaVersion:1,name:id,width:Math.ceil(root.width),height:Math.ceil(root.height),nodes,animations:(source.animations||[]).filter(a=>ids.has(a.hostNodeId))},configurations:{schemaVersion:1,tables:[],dependencies:shop.payload.configurations.dependencies.filter(d=>d.kind==='framework'||d.templateId==='ItemSlot'),integration:['Install ItemSlot before restoring the product card. Bind the corresponding product VM in the host list.']}},'prefab');
    const description = card.kind==='Gift'?'普通礼包商品卡':'充值商品卡';
    await client.query("INSERT INTO gamecli_ui.templates(id,category_id,group_id,name,description,sort,version,payload,scripts) VALUES($1,'prefab','shop-products',$2,$2,100,1,$3,$4) ON CONFLICT DO NOTHING",[id,description,payload,scripts]);
    const saved=(await client.query('SELECT payload,scripts FROM gamecli_ui.templates WHERE id=$1',[id])).rows[0];
    assert.deepEqual(saved.scripts,scripts);
    assert.deepEqual(saved.payload,JSON.parse(JSON.stringify(payload)));
    console.log(id,nodes.length,owned.size);
  }
  await client.query('COMMIT');
} catch(error) {
  await client.query('ROLLBACK');
  throw error;
} finally {
  client.release();
  await db.close();
}

