// First registration only. Existing standards must use reviewed version candidates.
import assert from 'node:assert/strict';
import {readFile,writeFile,readdir} from 'node:fs/promises';
import {join,resolve} from 'node:path';
import {createHash} from 'node:crypto';
import {isDeepStrictEqual} from 'node:util';
import {openDatabase} from '../server/database.mjs';
import {workspaceActor,createWorkspaces} from '../server/workspaces.mjs';
import {validateScripts} from '../server/template-scripts.mjs';
import {validateConfigurations} from '../shared/configurations.mjs';
import {validateTemplate} from '../shared/templates.mjs';
assert(process.argv.slice(2).every(a=>a==='--apply'),'Only --apply is supported');
const apply=process.argv.includes('--apply'),folder=resolve('.data/imports/sv-shop');
const w=createWorkspaces(),binding=await w.unityBindings.requireBinding('p-2d314388-18c4-42f5-80eb-f6729dd23f0e');
const project=binding.path,hash=v=>createHash('sha256').update(v).digest('hex');
const readJson=async path=>JSON.parse((await readFile(path,'utf8')).replace(/^\uFEFF/,''));
const payload=validateTemplate(await readJson(join(folder,'main/template.json')),'system');
const inventory=await readJson(join(folder,'source-inventory.json'));
const texts=await readJson('config/shop-texts.json');
const workbook=await readFile(join(folder,'config/propCommonText.xlsx'));
const query=await readJson(join(folder,'config/query.json'));
const entries=query.data.rows.map(r=>Object.fromEntries(r.cells.map(c=>[c.field,c.value])));
assert.equal(entries.length,texts.length);
for(const text of texts)assert(entries.some(e=>e.ID===text.ID&&e.Context===text.Context),'Missing Shop text '+text.ID);
payload.configurations=validateConfigurations({schemaVersion:1,tables:[{
 name:'propCommonText',purpose:'required',primaryKey:'NumID',aliasField:'ID',
 fields:[{name:'ID',type:'int',description:'Stable alias'},{name:'NumID',type:'int',description:'Numeric identifier'},{name:'Context',type:'string',description:'Localized display text'}],entries,
 source:{assetId:hash(workbook),sha256:hash(workbook),fileName:'propCommonText.xlsx',sheet:'CommonText'},
 layout:{descriptionRow:1,headerRow:2,typeRow:3,dataStartRow:4},
 merge:'Merge only SHOP aliases; resolve numeric collisions, retain product translations and regenerate with the product exporter.'
}],dependencies:[{name:'propLanguage',kind:'generated',required:true,reason:'Generate and load CommonText/Language before Shop opens.'}],integration:[
 'Register ShopEntity and ShopSystem; allocate a product FunctionSystem identifier. Do not overwrite product startup configuration.',
 'Use the product coordinator for SystemActive/SystemDetive. HomeEntryRequested.Shop may activate Shop; Home is an optional integration, not an installation dependency.',
 'UISystem CloseWidget(Type[]) must cancel pending asynchronous loads by request identity. Include the reviewed cancellation fix from FrameworkReference when the installed package lacks it.',
 'DemoCatalog is explicit sample data, not an SDK price catalog or real wallet. Product payment/receipt verification/grant interfaces must be supplied by the product.',
 'Restore all four companion prefabs and Metadata. Main card is the dynamic shell; list items are populated by VM at runtime.'
]});
const contract={...inventory,status:'reference',scope:'User-selected Home RechargeMainView; recharge and normal gifts with local simulated confirmation',
 differences:[
 'Recharge normally invokes an SDK; this reference deliberately uses the same local confirmation page as gifts.',
 'First-recharge promotions and bonus computation are not implemented; their source UI hooks remain. Demo products do not promise a first-pay bonus.',
 'Wallpaper, growth fund, special-price gift, operator recommendation, activities, SDK/payment, remote catalog and customer service remain product extensions.',
 'Mesh/particle branches and inactive item-conversion RGB/active effects are excluded; default entrance curves and gift click scale are retained.',
 'Browser main card is a dynamic shell. Companion designs and native materials are included in scripts; browser preview does not execute C# or local purchases.'
 ],animations:await readJson(join(folder,'animation-inventory.json'))};
let acceptance={status:'incomplete',unverified:['Independent runtime acceptance is pending.']};
try{acceptance=await readJson(join(folder,'acceptance/result.json'));}catch{}
payload.source=contract;payload.acceptance=acceptance;
const files=[],assets=new Map(),resources=[];
async function walk(dir){const list=[];for(const entry of await readdir(dir,{withFileTypes:true})){const path=join(dir,entry.name);if(entry.isDirectory())list.push(...await walk(path));else list.push(path);}return list;}
for(const absolute of await walk(join(project,'Assets/Scripts/Systems/Shop'))){const path=absolute.slice(project.length+1).replaceAll('\\','/');files.push({path,content:await readFile(absolute,'utf8')});}
for(const name of ['RuntimeSystem.cs','Common/Utility/CommonText.cs'])files.push({path:'Assets/Scripts/Systems/'+name,content:await readFile(join(project,'Assets/Scripts/Systems',name),'utf8')});
files.push({path:'GameCLI/Templates/Shop/FrameworkReference/UISystem.cs',content:await readFile(join(project,'LocalPackages/com.oathx.widget.extension/Runtime/Scripts/UISystem.cs'),'utf8')});
const root='Assets/Data/Widgets/Shop';
for(const absolute of [join(project,root+'.meta'),...await walk(join(project,root))]){
 const path=absolute.slice(project.length+1).replaceAll('\\','/'),body=await readFile(absolute);
 if(path.endsWith('.png')){assets.set(hash(body),{body,path});resources.push({path,assetId:hash(body),sha256:hash(body),mime:'image/png'});}
 else resources.push({path,encoding:'utf8',sha256:hash(body),content:body.toString('utf8')});
}

for(const key of ['main','recharge','gift','giftbuy']){
 const design=await readJson(join(folder,key,'template.json'));
 for(const id of design.design.nodes.map(n=>n.image).filter(Boolean))assert(assets.has(id),'Missing resource '+id);
 files.push({path:`GameCLI/Templates/Shop/Designs/${key}.json`,content:JSON.stringify(design)});
}
for(const name of ['bindings.json','animation-inventory.json','gift-click.json'])files.push({path:'GameCLI/Templates/Shop/'+name,content:await readFile(join(folder,name),'utf8')});
files.push({path:'GameCLI/Templates/Shop/materials.json',content:JSON.stringify({schemaVersion:1,files:resources,download:'GET /api/studio/assets/<assetId>; verify SHA256, restore adjacent .meta GUIDs.'})});
files.push({path:'GameCLI/Templates/Shop/contract.json',content:JSON.stringify(contract,null,2)});
files.push({path:'GameCLI/Templates/Shop/acceptance.json',content:JSON.stringify(acceptance,null,2)});
files.push({path:'GameCLI/Templates/Shop/README.md',content:await readFile('reports/shop-template/source-scope.md','utf8')});
const scripts=validateScripts({schemaVersion:1,files});
await writeFile(join(folder,'scripts.json'),JSON.stringify(scripts,null,2));
await writeFile(join(folder,'publication.json'),JSON.stringify(payload,null,2));
const summary={template:'SvShop',nodes:payload.design.nodes.length,images:assets.size,files:files.length,scriptBytes:files.reduce((n,f)=>n+Buffer.byteLength(f.content),0),configRows:entries.length};
if(!apply){console.log(JSON.stringify({...summary,dryRun:true}));await w.close();process.exit(0);}
const db=await openDatabase(),client=await db.pool.connect();
try{
 await client.query('BEGIN');
 assert((await client.query('SELECT actor FROM gamecli_ui.template_admins WHERE actor=$1',[workspaceActor()])).rows.length,'Template admin required');
 assert(!(await client.query("SELECT id FROM gamecli_ui.templates WHERE id='SvShop'")).rows.length,'Shop already exists; use a reviewed candidate');
 for(const [id,a]of assets)await client.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT DO NOTHING',[id,'image/png',a.body,a.path]);
 await client.query('INSERT INTO gamecli_ui.configuration_assets(id,body) VALUES($1,$2) ON CONFLICT DO NOTHING',[hash(workbook),workbook]);
 await client.query("INSERT INTO gamecli_ui.template_groups(category_id,id,name,sort) VALUES('system','shop','Shop',50) ON CONFLICT DO NOTHING");
 await client.query("SELECT set_config('gamecli.publish','approved',true)");
 await client.query("INSERT INTO gamecli_ui.templates(id,category_id,group_id,name,description,sort,version,payload,scripts) VALUES('SvShop','system','shop','SV Shop','大厅商城 · 充值与普通礼包 · 本地模拟购买 · 四页面完整资料',80,1,$1::jsonb,$2::jsonb)",[JSON.stringify(payload),JSON.stringify(scripts)]);
 const saved=(await client.query("SELECT payload,scripts FROM gamecli_ui.templates WHERE id='SvShop'")).rows[0];
 assert(isDeepStrictEqual(saved,{payload,scripts}),'Database readback mismatch');
 await client.query('COMMIT');console.log(JSON.stringify({...summary,version:1,verified:true}));
}catch(error){await client.query('ROLLBACK');throw error;}finally{client.release();await db.close();await w.close();}
