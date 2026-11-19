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
const apply=process.argv.includes('--apply'),folder=resolve('.data/imports/sv-mail');
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
 const target=payload.design.nodes.find(n=>n.id==='Mail'+node.path.replaceAll('/','_'));
 if(!target)continue;
 const font=node.components.flatMap(c=>c.references||[]).find(r=>r.property==='m_FontData.m_Font');
 if(font){
  const face=fonts[font.path];assert(face,'Missing source font '+font.path);
  const body=await readFile('.data/imports/sv-home/fidelity-candidate/fonts/'+face.fileName);
  target.fontAsset=await binary(face.fileName,body,'font/woff2');
  assert.equal(target.fontAsset,face.assetId);
 }
}
// Intentional adaptations are reflected in browser design as well as native materials.
const badge=payload.design.nodes.find(n=>n.id==='Mail0_3_1_1_0_4');
badge.x+=20;badge.rectTransform.anchoredPosition.x=-224.2;
for(const suffix of ['1','3']){
 const node=payload.design.nodes.find(n=>n.id==='Mail0_1_3_3_'+suffix);
 node.text='\u200b';
}
// Cell roots own the click. Nested legacy buttons are presentation-only.
for(const node of payload.design.nodes){
 if(node.type==='Button'&&(node.id.startsWith('Mail0_3_0_2_1_')||node.id.startsWith('Mail0_3_1_1_'))){
  node.type='Image';delete node.action;
 }
}
payload.design.nodes.find(n=>n.id==='Mail0_3_0_1_2_1').text='天后过期';
const query=await json(join(folder,'config/query.json'));
const types=await json(join(folder,'config/types.json'));
const entries=query.data.rows.map(r=>Object.fromEntries(r.cells.map(c=>[c.field,c.value])));
assert.equal(entries.length,1);assert.equal(entries[0].ID,'SYS_MAIL');
const workbook=await readFile(join(project,'../config/propFunctionSystem.xlsx'));
payload.configurations=validateConfigurations({schemaVersion:1,tables:[{
 name:'propFunctionSystem',purpose:'required',primaryKey:'NumID',aliasField:'ID',
 fields:types.data.rows[0].cells.map(c=>({name:c.field,type:c.value||'string'})),entries,
 source:{assetId:hash(workbook),sha256:hash(workbook),fileName:'propFunctionSystem.xlsx',sheet:'FunctionSystem'},
 layout:{descriptionRow:1,headerRow:2,typeRow:3,dataStartRow:4},
 merge:'Merge SYS_MAIL only, resolve product NumID collisions and regenerate using the product exporter. Never replace the complete product table.'
}],dependencies:[{name:'FunctionSystemConfig',kind:'generated',required:true,reason:'Regenerate and load the function registry before activating MailSystem.'}],integration:[
 'Install MailEntity and MailSystem using existing Oathx entity/system discovery. Use SystemActive/SystemDetive through the product coordinator.',
 'Home Mail routing in Integration/MasterSystem.cs is a reference only. Do not overwrite product Master or require Home to install Mail.',
 'Restore materials and adjacent meta GUIDs. Common font files may already exist: compare hashes before reuse; resolve conflicting GUIDs through Unity asset tooling.',
 'MailExample.json is optional and explicitly local. Supply ReplaceInbox data before first activation to replace the example, or clear UIMail.example for an empty inbox.',
 'Claims record in-memory receipts only. No inventory grant, service connection, persistence, timer-driven expiry or account-switch contract is provided.',
 'Fixed labels are serialized in the prefab. Runtime fixed product messages belong in propCommonText; message bodies are host data.'
]});
const contract={...await json(join(folder,'source-inventory.json')),status:'reference',scope:'SV NewMailView with local inbox, detail, read state, claims and deletion',differences:[
 'No networking, inventory grant or persisted inbox. Demonstration timestamps are rebased on initial activation.',
 'Capacity 50 indicator is hidden because no capacity policy is implemented. NEW badge moved 20 native units right to fit the Fancy list viewport.',
 'Mesh/particle and special-item RGB/active effects are outside the simple attachment reference; original exports retain their source descriptions.',
 'Browser preview is a static shell with supported default curves; it does not execute C# lists or the native Close/envelope state machine. Full native clips and controllers are included.'
],animations:await json(join(folder,'animation-inventory.json')),nativeAnimations:await json(join(folder,'native-animation-inventory.json'))};
payload.source=contract;
payload.acceptance=await json('reports/mail-template/result.json');
async function walk(dir){const all=[];for(const entry of await readdir(dir,{withFileTypes:true})){const path=join(dir,entry.name);if(entry.isDirectory())all.push(...await walk(path));else all.push(path);}return all;}
for(const absolute of await walk(join(project,'Assets/Scripts/Systems/Mail'))){
 const path=absolute.slice(project.length+1).replaceAll('\\','/');files.push({path,content:await readFile(absolute,'utf8')});
}
files.push({path:'Assets/Scripts/Systems/RuntimeSystem.cs',content:await readFile(join(project,'Assets/Scripts/Systems/RuntimeSystem.cs'),'utf8')});
files.push({path:'GameCLI/Templates/Mail/Integration/MasterSystem.cs',content:await readFile(join(project,'Assets/Scripts/Systems/Master/MasterSystem.cs'),'utf8')});
const native=[join(project,'Assets/Data/Widgets/Mail.meta'),...await walk(join(project,'Assets/Data/Widgets/Mail'))];
for(const font of ['msyh.ttc','msyhbd.ttc'])for(const suffix of ['','.meta'])native.push(join(project,'Assets/Data/Widgets/Common/Resources',font+suffix));
for(const absolute of native){
 const path=absolute.slice(project.length+1).replaceAll('\\','/'),body=await readFile(absolute);
 if(/\.(png|ttc)$/.test(path)){
  const mime=path.endsWith('.png')?'image/png':'font/collection';
  const id=await binary(path,body,mime);resources.push({path,assetId:id,sha256:id,mime});
 }else resources.push({path,encoding:'utf8',sha256:hash(body),content:body.toString('utf8')});
}
for(const node of payload.design.nodes)for(const id of [node.image,node.fontAsset].filter(Boolean))assert(assets.has(id),'Missing asset '+id);
for(const name of ['bindings.json','animation-inventory.json','native-animation-inventory.json','Open-native.json','Close-native.json','EnvelopeOpen-native.json','EnvelopeClose-native.json','EntryOpen-native.json'])files.push({path:'GameCLI/Templates/Mail/'+name,content:await readFile(join(folder,name),'utf8')});
const archive=gzipSync(Buffer.from(JSON.stringify(source)));
const archiveId=await binary('Source/unity-prefab.json.gz',archive,'application/gzip');
resources.push({path:'GameCLI/Templates/Mail/Source/unity-prefab.json.gz',assetId:archiveId,sha256:archiveId,mime:'application/gzip'});
files.push({path:'GameCLI/Templates/Mail/materials.json',content:JSON.stringify({schemaVersion:1,files:resources,download:'GET /api/studio/assets/<assetId>; verify SHA256 and restore adjacent meta files.'})});
files.push({path:'GameCLI/Templates/Mail/contract.json',content:JSON.stringify(contract,null,2)});
files.push({path:'GameCLI/Templates/Mail/README.md',content:await readFile('reports/mail-template/source-scope.md','utf8')});
const checked=validateTemplate(payload,'system');
const scripts=validateScripts({schemaVersion:1,files});
await writeFile(join(folder,'publication.json'),JSON.stringify(checked,null,2));
await writeFile(join(folder,'scripts.json'),JSON.stringify(scripts,null,2));
const summary={template:'SvMail',nodes:checked.design.nodes.length,assets:assets.size,scriptFiles:files.length,materials:resources.length,configRows:entries.length};
if(!apply){console.log(JSON.stringify({...summary,dryRun:true}));await w.close();process.exit(0);}
const db=await openDatabase(),client=await db.pool.connect();
try{
 await client.query('BEGIN');
 assert((await client.query('SELECT actor FROM gamecli_ui.template_admins WHERE actor=$1',[workspaceActor()])).rows.length,'Template admin required');
 assert(!(await client.query("SELECT id FROM gamecli_ui.templates WHERE id='SvMail'")).rows.length,'Mail exists; use a reviewed version candidate');
 for(const[id,a]of assets)await client.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT DO NOTHING',[id,a.mime,a.body,a.path]);
 await client.query('INSERT INTO gamecli_ui.configuration_assets(id,body) VALUES($1,$2) ON CONFLICT DO NOTHING',[hash(workbook),workbook]);
 await client.query("INSERT INTO gamecli_ui.template_groups(category_id,id,name,sort) VALUES('system','mail','Mail',60) ON CONFLICT DO NOTHING");
 await client.query("SELECT set_config('gamecli.publish','approved',true)");
 await client.query("INSERT INTO gamecli_ui.templates(id,category_id,group_id,name,description,sort,version,payload,scripts) VALUES('SvMail','system','mail','SV Mail','邮件 · 本地示例 · 列表与详情 · 附件领取记录 · 原版动画',90,1,$1::jsonb,$2::jsonb)",[JSON.stringify(checked),JSON.stringify(scripts)]);
 const saved=(await client.query("SELECT payload,scripts FROM gamecli_ui.templates WHERE id='SvMail'")).rows[0];assert(isDeepStrictEqual(saved,{payload:checked,scripts}));
 for(const[id,a]of assets){const row=(await client.query('SELECT body FROM gamecli_ui.assets WHERE id=$1',[id])).rows[0];assert.equal(hash(row.body),hash(a.body));}
 await client.query('COMMIT');
 const result={...summary,version:1,verified:true};await writeFile('reports/mail-template/publication.json',JSON.stringify(result,null,2));console.log(JSON.stringify(result));
}catch(error){await client.query('ROLLBACK');throw error;}finally{client.release();await db.close();await w.close();}
