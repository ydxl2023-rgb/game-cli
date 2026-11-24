import {readFile,writeFile} from 'node:fs/promises';
import {createHash} from 'node:crypto';
import {openDatabase} from '../../../../../integrations/codex/server/database.mjs';
import {validateDesign} from '../../../../../integrations/codex/shared/design.mjs';
const projectId='p-fac9bb57-0a72-4db3-9f48-7d42e95f37a3';
const expectedVersion=Number(process.argv[2]??1);
if(!Number.isInteger(expectedVersion)||expectedVersion<1)throw Error('Provide the observed project version.');
const root=new URL('../../',import.meta.url);
const payload=JSON.parse(await readFile(new URL('./launch-payload.json',import.meta.url),'utf8'));
const manifest=JSON.parse(await readFile(new URL('./asset-manifest.json',import.meta.url),'utf8'));
const fontPath='extracted-resources/assets/data/fonts/fangzhenglanting.ttf';
const font=await readFile(new URL(fontPath,root));
const hash=bytes=>createHash('sha256').update(bytes).digest('hex');
for(const node of payload.design.nodes)if(node.type==='Text')node.fontAsset=hash(font);
const files=[];
for(const path of ['LocalPackages/com.oathx.core/Runtime/XLauncher.cs','LocalPackages/com.oathx.core/Runtime/XApp.cs','Assets/Scripts/GamePlugin.cs']){
 const content=await readFile(new URL('client/'+path,root),'utf8');files.push({path,content,sha256:hash(content)});
}
payload.design.nodes[0].scripts={schemaVersion:1,files};
const design=validateDesign(payload.design);
const db=await openDatabase();const client=await db.pool.connect();
try{
 await client.query('BEGIN');
 const row=(await client.query("SELECT u.version,u.payload,m.role FROM gamecli_ui.project_ui u JOIN gamecli_ui.project_members m ON m.project_id=u.project_id WHERE u.project_id=$1 AND m.actor='local' FOR UPDATE OF u",[projectId])).rows[0];
 if(!row||row.role!=='owner'||row.version!==expectedVersion)throw Error('Project version or owner differs; reload before writing.');
 await writeFile(new URL('./project-before-v'+row.version+'.json',import.meta.url),JSON.stringify(row,null,2));
 for(const asset of manifest){
  const bytes=await readFile(new URL('./assets/'+asset.file,import.meta.url));if(hash(bytes)!==asset.assetId)throw Error('Asset hash mismatch');
  await client.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT(id) DO NOTHING',[asset.assetId,'image/png',bytes,asset.source]);
 }
 await client.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT(id) DO NOTHING',[hash(font),'font/ttf',font,fontPath]);
 const result=await client.query('UPDATE gamecli_ui.project_ui SET payload=$1::jsonb,version=version+1,updated_at=now() WHERE project_id=$2 AND version=$3 RETURNING version',[JSON.stringify({design,actor:'local',savedAt:new Date().toISOString()}),projectId,row.version]);
 if(result.rowCount!==1)throw Error('Project version changed');
 await client.query('COMMIT');
 await writeFile(new URL('./project-saved.json',import.meta.url),JSON.stringify({projectId,version:result.rows[0].version,design},null,2));
 console.log(JSON.stringify({projectId,version:result.rows[0].version,resolution:[design.width,design.height],nodes:design.nodes.length}));
}catch(error){await client.query('ROLLBACK');throw error;}finally{client.release();await db.close();}


