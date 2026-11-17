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
 payload.configurations=validateConfigurations(configurations);
 const files=[],materials=[],assets=new Map();
 async function walk(path){const result=[];for(const item of await readdir(path,{withFileTypes:true})){const p=join(path,item.name);if(item.isDirectory())result.push(...await walk(p));else result.push(p);}return result;}
 for(const path of await walk(join(project,'Assets/Scripts/Systems/Home')))files.push({path:path.slice(project.length+1).replaceAll('\\','/'),content:await readFile(path,'utf8')});
 for(const name of ['RuntimeSystem.cs','CommonText.cs'])files.push({path:'Assets/Scripts/Systems/'+name,content:await readFile(join(project,'Assets/Scripts/Systems',name),'utf8')});
 for(const absolute of await walk(join(project,'Assets/Data/Widgets/Home'))){
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
 files.push({path:'GameCLI/Templates/Home/README.md',content:'Home reference presentation. Local example data restores nine source activity entries, independent NEW/ad indicators and a Triple reward state. Replace HomeEntity values for the product and call HomeSystem.Refresh. Retain source fonts, HomeTextOutline, the serialized outline shader reference and its UGUI clipping compatibility fix, activity GridLayoutGroup and the serialized UIHome indicators. NEW and drop animations loop separately; entrance includes Left. Merge only configurations entries. No live ad SDK or activity service is implemented. Character and seasonal presentation remain extension points. The source screenshot is an example, not an active operations schedule. Production materials include native font originals and browser WOFF2 derivatives.\n'});
 const scripts=validateScripts({schemaVersion:1,files});
 await writeFile(folder+'/scripts.json',JSON.stringify(scripts,null,2));await writeFile(folder+'/publication.json',JSON.stringify(payload,null,2));
 if(process.argv.includes('--upload-assets')){
  const db=await openDatabase();try{for(const [id,a]of assets)await db.pool.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT DO NOTHING',[id,a.mime,a.bytes,a.path]);await db.pool.query('INSERT INTO gamecli_ui.configuration_assets(id,body) VALUES($1,$2) ON CONFLICT DO NOTHING',[hash(workbook),workbook]);}finally{await db.close();}
 }
 console.log(JSON.stringify({nodes:payload.design.nodes.length,files:files.length,assets:assets.size,texts:entries.length,scriptBytes:files.reduce((sum,f)=>sum+Buffer.byteLength(f.content),0)}));
}finally{await w.close();}
