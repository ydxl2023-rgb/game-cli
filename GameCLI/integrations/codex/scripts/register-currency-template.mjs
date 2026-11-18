import assert from 'node:assert/strict';
import {readFile,writeFile,readdir,mkdir} from 'node:fs/promises';
import {join} from 'node:path';
import {createHash} from 'node:crypto';
import {isDeepStrictEqual} from 'node:util';
import {openDatabase} from '../server/database.mjs';
import {workspaceActor,createWorkspaces} from '../server/workspaces.mjs';
import {validateScripts} from '../server/template-scripts.mjs';
import {validateTemplate} from '../shared/templates.mjs';
const w=createWorkspaces(),folder='.data/currency-template',hash=b=>createHash('sha256').update(b).digest('hex');
try {
 const binding=await w.unityBindings.requireBinding('p-2d314388-18c4-42f5-80eb-f6729dd23f0e');
 const source=JSON.parse(await readFile('.data/imports/sv-home/fidelity-candidate/template.json','utf8'));
 const root=source.design.nodes.find(n=>n.name==='ItemToolTips');assert(root);
 const ids=new Set([root.id]);let size;
 do{size=ids.size;for(const n of source.design.nodes)if(ids.has(n.parent))ids.add(n.id);}while(size!==ids.size);
 const nodes=structuredClone(source.design.nodes.filter(n=>ids.has(n.id)));
 const standalone=nodes.find(n=>n.id===root.id);Object.assign(standalone,{parent:null,name:'CurrencyBar',x:0,y:0});
 standalone.rectTransform={anchorMin:{x:0,y:1},anchorMax:{x:0,y:1},pivot:{x:0,y:1},anchoredPosition:{x:0,y:0},sizeDelta:{x:root.width,y:root.height}};
 const payload=validateTemplate({root:root.id,design:{schemaVersion:1,name:'CurrencyBar',width:960,height:240,nodes,animations:[]},configurations:{schemaVersion:1,tables:[],dependencies:['com.oathx.widget'],integration:[
  'Currency is a UI component, not a RuntimeSystem. Install native materials and four Currency classes before dependent Home prefabs.',
  'Embed CurrencyBar.prefab; bind VMCurrencyBar with SyncMetadata and its three ordered Entries with existing SyncGroup.',
  'Create three entries and callbacks once before Ref. Update Amount, Icon, Visible, CanAdd and Interactable in place; do not replace callbacks or Entries on a bound instance.',
  'Host owns wallet data, formatting, red-dot rules and navigation. currencyId is a host-supplied identifier; Amount is a preformatted string.',
  'Native bar preserves Home top-right docking; browser preview normalizes only the root. Override docking in each host. CurrencyEntry.prefab is a single-slot reference; bar slots retain individual authored icon shapes.',
  'Bindings last one instance lifetime. Set the host CurrencyBar to null before Unref; OnDestroy also releases. Destroy and recreate after Unref. Existing SyncClick adds listeners on callback replacement.'
 ]}},'system');
 const files=[],materials=[],assets=new Map();
 async function walk(path){const all=[];for(const e of await readdir(path,{withFileTypes:true})){const p=join(path,e.name);if(e.isDirectory())all.push(...await walk(p));else all.push(p);}return all;}
 for(const path of ['UICurrencyBar.cs','UICurrencyEntry.cs','VM/VMCurrencyBar.cs','VM/VMCurrencyEntry.cs'])for(const suffix of ['', '.meta']){const relative='Assets/Scripts/Systems/Common/UI/'+path+suffix;files.push({path:relative,content:await readFile(join(binding.path,relative),'utf8')});}
 for(const p of [join(binding.path,'Assets/Data/Widgets/Currency.meta'),...await walk(join(binding.path,'Assets/Data/Widgets/Currency'))]){
  const body=await readFile(p),path=p.slice(binding.path.length+1).replaceAll('\\','/'),id=hash(body);
  if(/\.(png|ttf|ttc|otf)$/.test(path)){const mime=path.endsWith('.png')?'image/png':'application/octet-stream';assets.set(id,{body,path,mime});materials.push({path,assetId:id,sha256:id,mime});}
  else materials.push({path,encoding:'utf8',content:body.toString('utf8'),sha256:id});
 }
 files.push({path:'GameCLI/Templates/Currency/materials.json',content:JSON.stringify({schemaVersion:1,files:materials,download:'GET /api/studio/assets/<assetId>; verify SHA256 and preserve adjacent .meta GUIDs.'})});
 files.push({path:'GameCLI/Templates/Currency/README.md',content:payload.configurations.integration.join('\n\n')});
 const scripts=validateScripts({schemaVersion:1,files});
 await mkdir(folder,{recursive:true});await writeFile(folder+'/publication.json',JSON.stringify(payload,null,2));await writeFile(folder+'/scripts.json',JSON.stringify(scripts,null,2));
 const summary={template:'CurrencyBar',nodes:payload.design.nodes.length,files:files.length,materials:materials.length,assets:assets.size};
 if(!process.argv.includes('--apply'))console.log(JSON.stringify({...summary,dryRun:true}));
 else{
  const db=await openDatabase(),c=await db.pool.connect();
  try{
   await c.query('BEGIN');assert((await c.query('SELECT actor FROM gamecli_ui.template_admins WHERE actor=$1',[workspaceActor()])).rows.length,'Admin required');
   assert(!(await c.query("SELECT id FROM gamecli_ui.templates WHERE id='CurrencyBar'")).rows.length,'Existing template requires a reviewed candidate');
   for(const[id,a]of assets)await c.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT DO NOTHING',[id,a.mime,a.body,a.path]);
   for(const n of payload.design.nodes)for(const id of[n.image,n.fontAsset].filter(Boolean))assert((await c.query('SELECT id FROM gamecli_ui.assets WHERE id=$1',[id])).rows.length,'Missing preview asset '+id);
   await c.query("INSERT INTO gamecli_ui.template_groups(category_id,id,name,sort) VALUES('system','currency','Currency UI',60) ON CONFLICT DO NOTHING");
   await c.query("SELECT set_config('gamecli.publish','approved',true)");
   await c.query("INSERT INTO gamecli_ui.templates(id,category_id,group_id,name,description,sort,version,payload,scripts) VALUES('CurrencyBar','system','currency','Currency Bar','可复用货币栏 · UI组件 · 宿主提供数据与交互',90,1,$1::jsonb,$2::jsonb)",[JSON.stringify(payload),JSON.stringify(scripts)]);
   const saved=(await c.query("SELECT payload,scripts FROM gamecli_ui.templates WHERE id='CurrencyBar'")).rows[0];assert(isDeepStrictEqual(saved,{payload,scripts}));
   await c.query('COMMIT');console.log(JSON.stringify({...summary,version:1,verified:true}));
  }catch(e){await c.query('ROLLBACK');throw e;}finally{c.release();await db.close();}
 }
}finally{await w.close();}
