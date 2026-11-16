// First registration only. Existing Home versions must use reviewed candidates.
import assert from 'node:assert/strict';
import {readFile, writeFile, readdir} from 'node:fs/promises';
import {join, resolve} from 'node:path';
import {createHash} from 'node:crypto';
import {isDeepStrictEqual} from 'node:util';
import {openDatabase} from '../server/database.mjs';
import {workspaceActor, createWorkspaces} from '../server/workspaces.mjs';
import {validateScripts} from '../server/template-scripts.mjs';
import {validateConfigurations} from '../shared/configurations.mjs';

assert(process.argv.slice(2).every(a=>a==='--apply'), 'Only --apply is supported.');
const apply=process.argv.includes('--apply');
const folder=resolve('.data/imports/sv-home');
const projectId='p-2d314388-18c4-42f5-80eb-f6729dd23f0e';
const w=createWorkspaces();
const binding=await w.unityBindings.requireBinding(projectId);
const project=binding.path;
const hash=value=>createHash('sha256').update(value).digest('hex');
const payload=JSON.parse(await readFile(join(folder,'template.json'),'utf8'));
const report=JSON.parse(await readFile(join(folder,'report.json'),'utf8'));
const workbook=await readFile(join(folder,'config/propCommonText.xlsx'));
const query=JSON.parse(await readFile(join(folder,'config/query.json'),'utf8'));
const entries=query.data.rows.map(r=>Object.fromEntries(r.cells.map(c=>[c.field,c.value])));
assert.equal(entries.length,report.texts.length);
for(const text of report.texts)assert(entries.some(e=>e.ID===text.alias&&e.Context===text.text),'Missing Home text');
payload.configurations=validateConfigurations({schemaVersion:1,tables:[{
  name:'propCommonText',purpose:'required',primaryKey:'NumID',aliasField:'ID',
  fields:[{name:'ID',type:'int',description:'Stable alias'},{name:'NumID',type:'int',description:'Numeric identifier'},{name:'Context',type:'string',description:'Localized display text'}],
  entries,source:{assetId:hash(workbook),sha256:hash(workbook),fileName:'propCommonText.xlsx',sheet:'CommonText'},
  layout:{descriptionRow:1,headerRow:2,typeRow:3,dataStartRow:4},
  merge:'Merge these Home aliases only. Resolve numeric collisions before generation; preserve product data and translations.'
}],dependencies:[{name:'propLanguage',kind:'generated',required:true,reason:'Generate with the product exporter and load CommonText/Language before opening Home.'}],integration:[
  'HomeEntity must be registered with the entity manager.',
  'RuntimeSystem.LoadSystem must activate newly created inactive modules for explicit navigation, independently of startup Active configuration.',
  'Register HomeSystem in the product FunctionSystem table using the existing product ID allocation; do not copy a reference numeric ID.',
  'The product coordinator consumes HomeEntryRequested; all destinations remain unconnected in this basic reference.',
  'HomePresentationRequested exposes optional keys; no Spine, season animation, remote activity or SDK implementation is included.'
]});
payload.source={...report.source,scope:report.scope,excluded:report.exclusions,limitations:report.limitations};
payload.acceptance={status:'incomplete',passed:['Unity compilation','46 Metadata bindings','17 data/localized text values','29 raycast-directed pointer clicks','close and reopen'],unverified:['deactivation during pending asynchronous load','full product integration'],evidence:'Independent Home reviewer; raw CLI evidence archived with the source contract.'};
const files=[];
for(const path of ['HomeSystem.cs','HomeEntity.cs','HomeEvents.cs','UI/UIHome.cs','VM/VMHome.cs']){
  for(const suffix of ['', '.meta']){
    const relative='Assets/Scripts/Systems/Home/'+path+suffix;
    files.push({path:relative,content:await readFile(join(project,relative),'utf8')});
  }
}
for(const name of ['RuntimeSystem.cs','CommonText.cs']){
  const path='Assets/Scripts/Systems/'+name;
  files.push({path,content:await readFile(join(project,path),'utf8')});
}
const resources=[],assets=new Map();
for(const path of ['Assets/Data/Widgets/Home/Home.prefab','Assets/Data/Widgets/Home/Home.prefab.meta','Assets/Data/Widgets/Home/HomeAssets.meta',
  ...(await readdir(join(project,'Assets/Data/Widgets/Home'))).filter(n=>/\.(anim|controller)(\.meta)?$/.test(n)).map(n=>'Assets/Data/Widgets/Home/'+n),
  ...(await readdir(join(project,'Assets/Data/Widgets/Home/HomeAssets'))).map(n=>'Assets/Data/Widgets/Home/HomeAssets/'+n)]){
  const body=await readFile(join(project,path));
  if(path.endsWith('.png')){
    assets.set(hash(body),{body,path});resources.push({path,assetId:hash(body),sha256:hash(body),mime:'image/png'});
  }else resources.push({path,encoding:'utf8',sha256:hash(body),content:body.toString('utf8')});
}
const imageIds=new Set(payload.design.nodes.map(n=>n.image).filter(Boolean));
for(const id of imageIds)assert(assets.has(id),'Prefab resource missing '+id);
files.push({path:'GameCLI/Templates/Home/materials.json',content:JSON.stringify({schemaVersion:1,files:resources,download:'GET /api/studio/assets/<assetId>; verify SHA256 and restore the adjacent .meta to preserve prefab GUIDs.'})});
files.push({path:'GameCLI/Templates/Home/contract.json',content:JSON.stringify(report,null,2)});
files.push({path:'GameCLI/Templates/Home/README.md',content:'Basic Home template. Install UIHome, VMHome, HomeSystem and HomeEntity. Reuse existing shared RuntimeSystem/CommonText; never duplicate them. RuntimeSystem must activate explicit system loads. Merge configurations and generate/load CommonText and Language. Restore materials and their GUIDs, or regenerate with UnityCLI and rebuild all Metadata. The 29 buttons emit local HomeEntryRequested events, not completed navigation. Character/season presentation requires a product renderer. Empty avatar and profile values require product data. Native SV effects, activities, advertisements and SDK are outside the approved basic scope. See contract.json for the exact exclusions.\n'});
const scripts=validateScripts({schemaVersion:1,files});
await writeFile(join(folder,'scripts.json'),JSON.stringify(scripts,null,2));
await writeFile(join(folder,'publication.json'),JSON.stringify(payload,null,2));
const summary={template:'SvHome',nodes:payload.design.nodes.length,images:imageIds.size,files:files.length,configRows:entries.length};
if(!apply){console.log(JSON.stringify({...summary,dryRun:true}));await w.close?.();process.exit(0);}
const db=await openDatabase(),client=await db.pool.connect();
try{
  await client.query('BEGIN');
  assert((await client.query('SELECT actor FROM gamecli_ui.template_admins WHERE actor=$1',[workspaceActor()])).rows.length,'Template admin required');
  assert(!(await client.query('SELECT id FROM gamecli_ui.templates WHERE id=$1',['SvHome'])).rows.length,'Home already exists; use a reviewed candidate');
  for(const [id,a]of assets)await client.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT DO NOTHING',[id,'image/png',a.body,a.path]);
  await client.query('INSERT INTO gamecli_ui.configuration_assets(id,body) VALUES($1,$2) ON CONFLICT DO NOTHING',[hash(workbook),workbook]);
  await client.query("INSERT INTO gamecli_ui.template_groups(category_id,id,name,sort) VALUES('system','home','Home',40) ON CONFLICT DO NOTHING");
  await client.query("SELECT set_config('gamecli.publish','approved',true)");
  await client.query("INSERT INTO gamecli_ui.templates(id,category_id,group_id,name,description,sort,version,payload,scripts) VALUES('SvHome','system','home','SV Home','基础大厅 · 本地导航事件 · 角色与季节展示为扩展接口',70,1,$1::jsonb,$2::jsonb)",[JSON.stringify(payload),JSON.stringify(scripts)]);
  const saved=(await client.query("SELECT payload,scripts FROM gamecli_ui.templates WHERE id='SvHome'")).rows[0];
  assert(isDeepStrictEqual(saved,{payload,scripts}),'Database readback mismatch');
  await client.query('COMMIT');console.log(JSON.stringify({...summary,version:1,verified:true}));
}catch(error){await client.query('ROLLBACK');throw error;}finally{client.release();await db.close();await w.close?.();}
