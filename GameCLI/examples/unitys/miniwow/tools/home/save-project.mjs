import {readFile,writeFile,readdir} from 'node:fs/promises';
import {createHash} from 'node:crypto';
import {openDatabase} from '../../../../../integrations/codex/server/database.mjs';
import {validateDesign} from '../../../../../integrations/codex/shared/design.mjs';
const projectId='p-fac9bb57-0a72-4db3-9f48-7d42e95f37a3';
const expectedVersion=Number(process.argv[2]);
if(!Number.isInteger(expectedVersion)||expectedVersion<1)throw Error('Observed version required.');
const root=new URL('../../',import.meta.url);
const hash=data=>createHash('sha256').update(data).digest('hex');
const home=JSON.parse(await readFile(new URL('./home-payload.json',import.meta.url),'utf8'));
const environment=JSON.parse(await readFile(new URL('./environment-payload.json',import.meta.url),'utf8'));
const manifest=JSON.parse(await readFile(new URL('./asset-manifest.json',import.meta.url),'utf8'));
const paths=['Assets/Scripts/GamePlugin.cs','Assets/Editor/MiniwowHomeSetup.cs','Assets/Editor/MiniwowMainSceneSetup.cs','Assets/Editor/MiniwowMainSceneSetup.cs.meta','Assets/Scenes/Home.unity'];
for(const directory of ['Assets/Scripts/Systems/Home','Assets/Data/Widgets/Home']){
 async function collect(relative){
  for(const item of await readdir(new URL('client/'+relative+'/',root),{withFileTypes:true})){
   const path=relative+'/'+item.name;
   if(item.isDirectory())await collect(path);
   else if(/\.(cs|shader|prefab|anim|controller|meta)$/.test(path))paths.push(path);
  }
 }
 await collect(directory);
}
const files=[];
for(const path of paths){const content=await readFile(new URL('client/'+path,root),'utf8');files.push({path,content,sha256:hash(content)});}
for(const path of ['tools/main-scene/unity-imports.json','tools/main-scene/DELIVERY.md']){
 const content=await readFile(new URL(path,root),'utf8');files.push({path,content,sha256:hash(content)});
}
environment.design.nodes[0].active=false;
home.design.nodes[0].scripts={schemaVersion:1,files};
home.design.nodes[0].configurations=null;
const db=await openDatabase();const client=await db.pool.connect();
try{
 await client.query('BEGIN');
 const row=(await client.query("SELECT u.version,u.payload,m.role FROM gamecli_ui.project_ui u JOIN gamecli_ui.project_members m ON m.project_id=u.project_id WHERE u.project_id=$1 AND m.actor='local' FOR UPDATE OF u",[projectId])).rows[0];
 if(!row||row.role!=='owner'||row.version!==expectedVersion)throw Error('Project version/owner mismatch.');
 await writeFile(new URL('./project-before-v'+row.version+'.json',import.meta.url),JSON.stringify(row,null,2));
 const design=row.payload.design;
 const removed=new Set(['MiniwowHome','HomeEnvironment']);
 let changed=true;while(changed){changed=false;for(const node of design.nodes)if(removed.has(node.parent)&&!removed.has(node.id)){removed.add(node.id);changed=true;}}
 design.nodes=design.nodes.filter(node=>!removed.has(node.id));
 for(const node of design.nodes){for(const file of node.scripts?.files??[]){if(file.path==='Assets/Scripts/GamePlugin.cs')Object.assign(file,files.find(value=>value.path===file.path));}}
 design.nodes.push(...environment.design.nodes,...home.design.nodes);
 design.animations=[...(design.animations??[]).filter(a=>!removed.has(a.hostNodeId)),...home.design.animations];
 validateDesign(design);
 for(const asset of manifest){
  const bytes=await readFile(new URL('./assets/'+asset.file,import.meta.url));if(hash(bytes)!==asset.assetId)throw Error('Asset hash mismatch.');
  await client.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT(id) DO NOTHING',[asset.assetId,'image/png',bytes,asset.source]);
 }
 const font=await readFile(new URL('extracted-resources/assets/data/fonts/fangzhenglanting.ttf',root));
 await client.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT(id) DO NOTHING',[hash(font),'font/ttf',font,'miniwow/fangzhenglanting.ttf']);
 const payload={...row.payload,design,actor:'local',savedAt:new Date().toISOString()};
 const result=await client.query('UPDATE gamecli_ui.project_ui SET payload=$1::jsonb,version=version+1,updated_at=now() WHERE project_id=$2 AND version=$3 RETURNING version',[JSON.stringify(payload),projectId,row.version]);
 if(result.rowCount!==1)throw Error('Concurrent project update.');
 await client.query('COMMIT');
 await writeFile(new URL('./project-saved.json',import.meta.url),JSON.stringify({projectId,version:result.rows[0].version,design},null,2));
 console.log(JSON.stringify({version:result.rows[0].version,nodes:design.nodes.length,sourceFiles:files.length}));
}catch(error){await client.query('ROLLBACK');throw error;}finally{client.release();await db.close();}
