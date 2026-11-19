import assert from 'node:assert/strict';
import {readFile,writeFile,readdir} from 'node:fs/promises';
import {createHash} from 'node:crypto';
import {join} from 'node:path';
import {createWorkspaces} from '../server/workspaces.mjs';
import {openDatabase} from '../server/database.mjs';
import {validateScripts} from '../server/template-scripts.mjs';
import {validateConfigurations} from '../shared/configurations.mjs';
import {validateTemplate} from '../shared/templates.mjs';
const folder='.data/imports/sv-home/fidelity-candidate';
const hash=bytes=>createHash('sha256').update(bytes).digest('hex');
const json=async path=>JSON.parse(await readFile(path,'utf8'));
const w=createWorkspaces();
try{
 const binding=await w.unityBindings.requireBinding('p-2d314388-18c4-42f5-80eb-f6729dd23f0e');
 const project=binding.path,report=await json(folder+'/report.json'),payload=validateTemplate(await json(folder+'/template.json'),'system');
 const current=await w.templateEditor.published('SvHome');
 const configurations=structuredClone(current.design.nodes.find(n=>n.parent===null).configurations);
 const query=await json(folder+'/common-after.json');
 const aliases=new Set(report.texts.map(t=>t.alias));
 const entries=query.data.rows.map(r=>Object.fromEntries(r.cells.map(c=>[c.field,c.value]))).filter(r=>aliases.has(r.ID));
 assert.equal(entries.length,aliases.size);
 for(const text of report.texts)assert(entries.some(r=>r.ID===text.alias&&r.Context===text.text),'Missing localized text: '+text.alias);
 const workbook=await readFile(folder+'/config/propCommonText.xlsx');
 const table=configurations.tables.find(t=>t.name==='propCommonText');table.entries=entries;
 table.source={...table.source,assetId:hash(workbook),sha256:hash(workbook)};
 if(!configurations.dependencies.includes('RedDotSystem'))configurations.dependencies.push('RedDotSystem');
 const functionWorkbook=await readFile(folder+'/config/propFunctionSystem.xlsx');
 const functionQuery=await json(folder+'/function-query.json');
 const functionEntries=functionQuery.data.rows.map(r=>Object.fromEntries(r.cells.map(c=>[c.field,c.value])));
 const functionTable=configurations.tables.find(t=>t.name==='propFunctionSystem');
 const functionDefinition={name:'propFunctionSystem',purpose:'required',primaryKey:'NumID',aliasField:'ID',
  fields:[['ID','int'],['NumID','int'],['Name','string'],['TargetType','enum'],['ClassType','string'],['InstallOnStartup','int'],['Condtions','List<string>'],['Active','int'],['StartTime','string'],['StopTime','string'],['SceneId','int'],['Version','string']].map(([name,type])=>({name,type})),
  entries:functionEntries,source:{fileName:'propFunctionSystem.xlsx',sheet:'FunctionSystem',assetId:hash(functionWorkbook),sha256:hash(functionWorkbook)}};
 if(functionTable)Object.assign(functionTable,functionDefinition);else configurations.tables.push(functionDefinition);
 configurations.integration=configurations.integration.filter(text=>!text.startsWith('Function availability:'));
 configurations.integration.push('Function availability: merge the included Function sources and SYS_FUNCTION_OPEN startup row. Conditions use {Switch:1,Level:10}; UTC time windows are inclusive-start/exclusive-end. Empty ClassType is presentation-only. Each nonempty ClassType is unique. Merge the reference GamePlugin.Load gate and MasterSystem event handlers without replacing product files. Home authored items bind stable FunctionSystem IDs through existing SyncGroup and SyncVisible; retain local artwork, animations and per-item RedDotScope.');
 if(!configurations.dependencies.includes('CurrencyBar'))configurations.dependencies.push('CurrencyBar');
 configurations.integration=configurations.integration.filter(text=>!text.startsWith('Currency component:'));
 configurations.integration.push('Currency component: install CurrencyBar template v2 before Home, preserving native asset GUIDs. Home embeds its nested CurrencyBar.prefab and supplies three VMCurrencyEntry values and callbacks once. Update existing Amount properties when refreshing. No Currency source or materials are duplicated in Home.');
 if(!configurations.dependencies.includes('PlayerProfile'))configurations.dependencies.push('PlayerProfile');
 configurations.integration=configurations.integration.filter(text=>!text.startsWith('Profile widgets:'));
 configurations.integration.push('Profile widgets: install PlayerProfile v4 bundle before Home. It carries Profile and ProfileShortcuts as two composite roots; avatar and individual buttons are internal Parts. Shared code lives in Common/UI. Bind the five shortcut entries through one VMProfileShortcuts; preserve Common resource GUIDs. Keep all GUIDs. Home supplies one VMProfile for name, level, portrait, frame and two entry interactions; all inner bindings belong to UIProfile. Valid asset paths are supplied by the host and loaded through SyncSprite; PortraitVisible and FrameVisible use SyncVisible. The component does not query HomeEntity.');
 payload.configurations=validateConfigurations(configurations);
 const files=[],materials=[],assets=new Map();
 files.push({path:'GameCLI/Templates/Home/dependencies.json',content:JSON.stringify({schemaVersion:1,templates:[{id:'CurrencyBar',version:2,required:true,installBefore:'SvHome',prefab:'Assets/Data/Widgets/Currency/CurrencyBar.prefab'},{id:'PlayerProfile',version:4,required:true,installBefore:'SvHome',prefab:'Assets/Data/Widgets/Profile/Profile.prefab'}]})});
 async function walk(path){const result=[];for(const item of await readdir(path,{withFileTypes:true})){const p=join(path,item.name);if(item.isDirectory())result.push(...await walk(p));else result.push(p);}return result;}
 for(const path of [...await walk(join(project,'Assets/Scripts/Systems/Home')),...await walk(join(project,'Assets/Scripts/Systems/Function'))])files.push({path:path.slice(project.length+1).replaceAll('\\','/'),content:await readFile(path,'utf8')});
 for(const relative of ["UIActivityEntry.cs", "UIActivityBadge.cs", "VM/VMActivityEntry.cs", "VM/VMActivityBadge.cs", "Components/CustomTextOutline.cs", "UIProfileShortcuts.cs", "VM/VMProfileShortcuts.cs", "UIProfile.cs", "VM/VMProfile.cs"])for(const suffix of ['', '.meta']){const path='Assets/Scripts/Systems/Common/UI/'+relative+suffix;files.push({path,content:await readFile(join(project,path),'utf8')});}
 files.push({path:'GameCLI/Templates/Home/framework/WidgetMetadataInspector.cs',content:await readFile(join(project,'LocalPackages/com.oathx.widget/Editor/WidgetMetadataInspector.cs'),'utf8')});
 for(const relative of ['Assets/Scripts/GamePlugin.cs','Assets/Scripts/Systems/Master/MasterSystem.cs'])files.push({path:'GameCLI/Templates/Home/framework/'+relative.split('/').pop(),content:await readFile(join(project,relative),'utf8')});
 files.push({path:'GameCLI/Templates/Home/entry-badges.json',content:await readFile('config/home-entry-badges.json','utf8')});
 files.push({path:'GameCLI/Templates/Home/function-templates.json',content:await readFile('config/home-functions.json','utf8')});
 for(const name of ['RuntimeSystem.cs'])files.push({path:'Assets/Scripts/Systems/'+name,content:await readFile(join(project,'Assets/Scripts/Systems',name),'utf8')});
 for(const absolute of [...await walk(join(project,'Assets/Data/Widgets/Home')),...await walk(join(project,'Assets/Data/Widgets/Common'))]){
  const path=absolute.slice(project.length+1).replaceAll('\\','/');
  if(path.includes('HomeResourceImport')||path.endsWith('/OutlineMat.mat')||path.endsWith('/OutlineMat.mat.meta'))continue;
  const bytes=await readFile(absolute),id=hash(bytes);
  const mime=path.endsWith('.png')?'image/png':/\.(ttc|ttf|otf)$/.test(path)?'application/octet-stream':null;
  if(mime){assets.set(id,{bytes,mime,path});materials.push({path,assetId:id,sha256:id,mime});}
  else materials.push({path,encoding:'utf8',sha256:id,content:bytes.toString('utf8')});
 }
 for(const font of Object.values(await json(folder+'/fonts.json'))){const bytes=await readFile(folder+'/fonts/'+font.fileName);assert.equal(hash(bytes),font.assetId);assets.set(font.assetId,{bytes,mime:'font/woff2',path:'Home/Fonts/'+font.fileName});}
 for(const node of payload.design.nodes)for(const id of [node.image,node.fontAsset].filter(Boolean))assert(assets.has(id),'Missing resource: '+id);
 files.push({path:'GameCLI/Templates/Home/materials.json',content:JSON.stringify({schemaVersion:1,files:materials,download:'GET /api/studio/assets/<assetId>; verify SHA256 and restore adjacent native .meta GUIDs.'})});
 for(const name of ['report.json','presentation.json','fonts.json'])files.push({path:'GameCLI/Templates/Home/'+name,content:await readFile(folder+'/'+name,'utf8')});
 files.push({path:'GameCLI/Templates/Home/example.json',content:await readFile('config/home-presentation.json','utf8')});
 files.push({path:'GameCLI/Templates/Home/README.md',content:'Home reference presentation. Activity entries use FunctionSystem availability and the existing SyncGroup with a fixed ordered VList of authored UIActivityEntry items. SyncVisible controls availability and the existing layout reflows visible items. Bind once per page; update item visibility without repeated Ref. No custom list or cloned activity templates are used. Fixed navigation uses SyncMetadata and the same VMActivityEntry contract. NEW/ad widgets are instantiated only on request and released on hide or unbind; include all Badges prefabs and their resource dependencies. New artwork requires an authored item and matching configuration. Function inputs are local events, with no server implementation. Framework files are merge references only. Merge the WidgetMetadataInspector compatibility fix: serialized sync type names are authoritative; stale hash caches must never rewrite bindings when selecting a widget. Fixed Text values are serialized in the prefab without Entity or VM forwarding; only player, level, currencies and chat remain dynamic text bindings. Unimplemented activity placeholders remain empty. The retained propCommonText entries are localization source material, not a runtime Home dependency; language switching needs a UI localization component. Local example data restores nine source activity entries, independent composable NEW/ad widgets; notification dots are owned by the RedDotSystem template and propRedDot and a Triple reward state. Replace HomeEntity values for the product and call HomeSystem.Refresh. Retain source fonts, CustomTextOutline, the serialized outline shader reference and its UGUI clipping compatibility fix, activity GridLayoutGroup and serialized entry badge mounts. NEW and drop animations loop separately; entrance includes Left. Merge only configurations entries. No live ad SDK or activity service is implemented. Character and seasonal presentation remain extension points. The source screenshot is an example, not an active operations schedule. Production materials include native font originals and browser WOFF2 derivatives. Static source-layout equality intentionally reports the migrated marker nodes: their graphics now live in optional badge prefabs, and their pulse scale moved from mounts to badge roots. Validate the compiled bindings, entry-badges.json references and runtime attach/release evidence separately; do not report the static comparison as passed.\n'});
 const scripts=validateScripts({schemaVersion:1,files});
 await writeFile(folder+'/scripts.json',JSON.stringify(scripts,null,2));await writeFile(folder+'/publication.json',JSON.stringify(payload,null,2));
 if(process.argv.includes('--upload-assets')){
  const db=await openDatabase();try{for(const [id,a]of assets)await db.pool.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT DO NOTHING',[id,a.mime,a.bytes,a.path]);await db.pool.query('INSERT INTO gamecli_ui.configuration_assets(id,body) VALUES($1,$2) ON CONFLICT DO NOTHING',[hash(workbook),workbook]);await db.pool.query('INSERT INTO gamecli_ui.configuration_assets(id,body) VALUES($1,$2) ON CONFLICT DO NOTHING',[hash(functionWorkbook),functionWorkbook]);}finally{await db.close();}
 }
 console.log(JSON.stringify({nodes:payload.design.nodes.length,files:files.length,assets:assets.size,texts:entries.length,scriptBytes:files.reduce((sum,f)=>sum+Buffer.byteLength(f.content),0)}));
}finally{await w.close();}
