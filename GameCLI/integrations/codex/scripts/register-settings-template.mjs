// First registration only; later edits must use version candidates.
import assert from 'node:assert/strict';
import {readFile,writeFile,readdir,mkdir} from 'node:fs/promises';
import {join,resolve,basename} from 'node:path';
import {createHash} from 'node:crypto';
import {gzipSync} from 'node:zlib';
import {isDeepStrictEqual} from 'node:util';
import {openDatabase} from '../server/database.mjs';
import {workspaceActor,createWorkspaces} from '../server/workspaces.mjs';
import {validateScripts} from '../server/template-scripts.mjs';
import {validateConfigurations} from '../shared/configurations.mjs';
import {validateTemplate} from '../shared/templates.mjs';
assert(process.argv.slice(2).every(a=>a==='--apply'));
const apply=process.argv.includes('--apply'),folder=resolve('.data/imports/sv-settings');
const w=createWorkspaces();
const {path:project}=await w.unityBindings.requireBinding('p-2d314388-18c4-42f5-80eb-f6729dd23f0e');
const hash=v=>createHash('sha256').update(v).digest('hex');
const json=async p=>JSON.parse((await readFile(p,'utf8')).replace(/^\uFEFF/,''));
const payload=await json(join(folder,'main/template.json'));
const source=await json(join(folder,'source/main/unity-prefab.json'));
const fonts=await json('.data/imports/sv-home/fidelity-candidate/fonts.json');
const assets=new Map(),resources=[],files=[];
async function binary(path,body,mime){const id=hash(body);assets.set(id,{body,path,mime});return id;}
for(const node of source.nodes){
 const target=payload.design.nodes.find(n=>n.id==='Settings'+node.path.replaceAll('/','_'));
 if(!target)continue;
 const font=node.components.flatMap(c=>c.references||[]).find(r=>r.property==='m_FontData.m_Font');
 if(font){
  const face=fonts[font.path]||(!font.path.startsWith('Assets/')?Object.entries(fonts).find(([path])=>path.endsWith('/msyh.ttc'))?.[1]:null);assert(face,'Missing source font '+font.path);
  const body=await readFile('.data/imports/sv-home/fidelity-candidate/fonts/'+face.fileName);
  target.fontAsset=await binary(face.fileName,body,'font/woff2');
  assert.equal(target.fontAsset,face.assetId);
 }
}
const query=await json(join(folder,'config/query.json'));
const types=await json(join(folder,'config/types.json'));
const entries=query.data.rows.map(r=>Object.fromEntries(r.cells.map(c=>[c.field,c.value])));
assert.equal(entries.length,1);assert.equal(entries[0].ID,'SYS_SETTINGS');
const workbook=await readFile(join(project,'../config/propFunctionSystem.xlsx'));
payload.configurations=validateConfigurations({schemaVersion:1,tables:[{
 name:'propFunctionSystem',purpose:'required',primaryKey:'NumID',aliasField:'ID',
 fields:types.data.rows[0].cells.map(c=>({name:c.field,type:c.value||'string'})),entries,
 source:{assetId:hash(workbook),sha256:hash(workbook),fileName:'propFunctionSystem.xlsx',sheet:'FunctionSystem'},
 layout:{descriptionRow:1,headerRow:2,typeRow:3,dataStartRow:4},
 merge:'Merge SYS_SETTINGS only, resolve product NumID collisions and regenerate using the product exporter. Never replace the complete product table.'
}],dependencies:[{name:'FunctionSystemConfig',kind:'generated',required:true,reason:'Regenerate the registry before coordinator activation.'}],integration:[
 'SettingsEntity owns local device preferences. Restore materials and meta GUIDs and compile the Widget source generator.',
 'SettingsChanged supplies an independent snapshot to product audio/render/gameplay adapters. Saving an option does not implement SV-specific effects, remote privacy state or OS push registration.',
 'SettingsActionRequested is an explicit unimplemented product service entry. No SDK, external URL, account deletion, account switching or resource download is executed.',
 'Merge SYS_SETTINGS only. Master Home routing is an optional reference, not a requirement to install Home or overwrite product coordination.',
 'Fixed Chinese labels and fonts are on the prefab. Runtime product messages belong in propCommonText. Language switching is not wired in this reference.',
 'The source tab glow mesh/particle branches and dormant empty StaticList remain archived; active UGUI controls and both source animation clips are restored.'
]});
const contract={...await json(join(folder,'source-inventory.json')),status:'reference',scope:'Five original SV settings pages with 30 local preferences, persistence and explicit product service events',differences:[
 'No SV SDK, networking, account operation, renderer, audio mixer or OS notification service is imported. Product consumers implement SettingsChanged and SettingsActionRequested.',
 'Source StaticList is inactive, empty and has negative width; it is excluded. Original tab mesh/particle descriptions remain in the source archive; native UGUI selected artwork is retained.',
 'Unknown product server label is blank. Original platform-hidden rows remain hidden; their controls and saved preference mappings are retained.',
 'The Chinese service groups are selected. The overlapping overseas account Box is preserved inactive; products select their regional variant instead of displaying both.',
 'Model supports only Low/High. Medium preset maps Model to Low. Custom is derived from individual options, not a fourth preset.',
 'Browser previews supported default entrance curves only. Native Sound playback and preference interactions require Unity runtime.'
],animations:await json(join(folder,'animation-inventory.json')),nativeAnimations:await json(join(folder,'native-animation-inventory.json'))};
contract.differences.push('User-requested revision: Tencent service entry and RiceRoll, Power, Weekend HeadquartersMatch, ExtremeTrain and HomeDaily notifications are removed from current UI and code. Raw source archives are historical references only.');
payload.source=contract;
payload.acceptance=await json('reports/settings-template/result.json');
async function walk(dir){const all=[];for(const entry of await readdir(dir,{withFileTypes:true})){const path=join(dir,entry.name);if(entry.isDirectory())all.push(...await walk(path));else all.push(path);}return all;}
for(const absolute of await walk(join(project,'Assets/Scripts/Systems/Settings'))){
 const path=absolute.slice(project.length+1).replaceAll('\\','/');files.push({path,content:await readFile(absolute,'utf8')});
}
files.push({path:'Assets/Scripts/Systems/RuntimeSystem.cs',content:await readFile(join(project,'Assets/Scripts/Systems/RuntimeSystem.cs'),'utf8')});
files.push({path:'Assets/Scripts/Systems/Common/Utility/StorageKey.cs',content:await readFile(join(project,'Assets/Scripts/Systems/Common/Utility/StorageKey.cs'),'utf8')});
files.push({path:'GameCLI/Templates/Settings/Integration/MasterSystem.cs',content:await readFile(join(project,'Assets/Scripts/Systems/Master/MasterSystem.cs'),'utf8')});
const native=[join(project,'Assets/Data/Widgets/Settings.meta'),...await walk(join(project,'Assets/Data/Widgets/Settings'))];
for(const font of ['msyh.ttc','msyhbd.ttc'])for(const suffix of ['','.meta'])native.push(join(project,'Assets/Data/Widgets/Common/Resources',font+suffix));
for(const absolute of native){
 const path=absolute.slice(project.length+1).replaceAll('\\','/'),body=await readFile(absolute);
 if(/\.(png|ttc)$/.test(path)){
  const mime=path.endsWith('.png')?'image/png':'font/collection';
  const id=await binary(path,body,mime);resources.push({path,assetId:id,sha256:id,mime});
 }else resources.push({path,encoding:'utf8',sha256:hash(body),content:body.toString('utf8')});
}
for(const id of new Set(payload.design.nodes.map(n=>n.image).filter(Boolean))){const body=await readFile(join(folder,'assets',id+'.png'));assert.equal(await binary('Source/Images/'+id+'.png',body,'image/png'),id);}
for(const node of payload.design.nodes)for(const id of [node.image,node.fontAsset].filter(Boolean))assert(assets.has(id),'Missing asset '+id);
for(const name of ['bindings.json','animation-inventory.json','native-animation-inventory.json','Open-native.json','Sound-native.json'])files.push({path:'GameCLI/Templates/Settings/'+name,content:await readFile(join(folder,name),'utf8')});
const archive=gzipSync(Buffer.from(JSON.stringify(source)));
const archiveId=await binary('Source/unity-prefab.json.gz',archive,'application/gzip');
resources.push({path:'GameCLI/Templates/Settings/Source/unity-prefab.json.gz',assetId:archiveId,sha256:archiveId,mime:'application/gzip'});
files.push({path:'GameCLI/Templates/Settings/materials.json',content:JSON.stringify({schemaVersion:1,files:resources,download:'GET /api/studio/assets/<assetId>; verify SHA256 and restore adjacent meta files.'})});
files.push({path:'GameCLI/Templates/Settings/contract.json',content:JSON.stringify(contract,null,2)});
files.push({path:'GameCLI/Templates/Settings/README.md',content:await readFile('reports/settings-template/source-scope.md','utf8')});
const checked=validateTemplate(payload,'system');
const scripts=validateScripts({schemaVersion:1,files});
await writeFile(join(folder,'publication.json'),JSON.stringify(checked,null,2));
await writeFile(join(folder,'scripts.json'),JSON.stringify(scripts,null,2));
const summary={template:'SvSettings',nodes:checked.design.nodes.length,assets:assets.size,scriptFiles:files.length,materials:resources.length,configRows:entries.length};
if(!apply){console.log(JSON.stringify({...summary,dryRun:true}));await w.close();process.exit(0);}
const db=await openDatabase(),client=await db.pool.connect();
try{
 await client.query('BEGIN');
 assert((await client.query('SELECT actor FROM gamecli_ui.template_admins WHERE actor=$1',[workspaceActor()])).rows.length,'Template admin required');
 assert(!(await client.query("SELECT id FROM gamecli_ui.templates WHERE id='SvSettings'")).rows.length,'Settings exists; use a reviewed version candidate');
 for(const[id,a]of assets)await client.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT DO NOTHING',[id,a.mime,a.body,a.path]);
 await client.query('INSERT INTO gamecli_ui.configuration_assets(id,body) VALUES($1,$2) ON CONFLICT DO NOTHING',[hash(workbook),workbook]);
 await client.query("INSERT INTO gamecli_ui.template_groups(category_id,id,name,sort) VALUES('system','settings','Settings',70) ON CONFLICT DO NOTHING");
 await client.query("SELECT set_config('gamecli.publish','approved',true)");
 await client.query("INSERT INTO gamecli_ui.templates(id,category_id,group_id,name,description,sort,version,payload,scripts) VALUES('SvSettings','system','settings','SV Settings','设置 · 五页签 · 本地偏好 · 产品服务入口 · 原版动画',90,1,$1::jsonb,$2::jsonb)",[JSON.stringify(checked),JSON.stringify(scripts)]);
 const saved=(await client.query("SELECT payload,scripts FROM gamecli_ui.templates WHERE id='SvSettings'")).rows[0];assert(isDeepStrictEqual(saved,{payload:checked,scripts}));
 for(const[id,a]of assets){const row=(await client.query('SELECT body FROM gamecli_ui.assets WHERE id=$1',[id])).rows[0];assert.equal(hash(row.body),hash(a.body));}
 await client.query('COMMIT');
 const result={...summary,version:1,verified:true};await writeFile('reports/settings-template/publication.json',JSON.stringify(result,null,2));console.log(JSON.stringify(result));
}catch(error){await client.query('ROLLBACK');throw error;}finally{client.release();await db.close();await w.close();}
