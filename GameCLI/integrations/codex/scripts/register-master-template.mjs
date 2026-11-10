import {resolve} from 'node:path';
import {isDeepStrictEqual} from 'node:util';
import {openDatabase} from '../server/database.mjs';
import {workspaceActor} from '../server/workspaces.mjs';
import {readSourcePackage} from '../server/source-package.mjs';
import {validateTemplate} from '../shared/templates.mjs';

const args=process.argv.slice(2);
if(args.some(a=>a!=='--apply'))throw Error('Usage: node scripts/register-master-template.mjs [--apply]');
const source=await readSourcePackage(resolve('config/master-scripts.json'));
const payload=validateTemplate({kind:'code',root:null,design:{schemaVersion:1,name:'MasterSystem',width:1920,height:1080,nodes:[]}},'system');
const db=await openDatabase(),client=await db.pool.connect();
try {
 await client.query('BEGIN');
 if(!(await client.query('SELECT actor FROM gamecli_ui.template_admins WHERE actor=$1',[workspaceActor()])).rows.length)throw Error('Template administrator required.');
 const current=(await client.query('SELECT * FROM gamecli_ui.templates WHERE id=$1 FOR UPDATE',[source.templateId])).rows[0];
 if(current){
  if(!isDeepStrictEqual(current.scripts,source.scripts)||!isDeepStrictEqual(current.payload,payload))throw Error('Existing template differs; submit a reviewed candidate instead of overwriting.');
 }else if(args.includes('--apply')){
  await client.query("INSERT INTO gamecli_ui.template_groups(category_id,id,name,sort) VALUES('system','master','Master',30) ON CONFLICT DO NOTHING");
  await client.query("SELECT set_config('gamecli.publish','approved',true)");
  await client.query("INSERT INTO gamecli_ui.templates(id,category_id,group_id,name,description,sort,payload,scripts) VALUES($1,'system','master','Master','纯代码 · 玩家实体、登录事件与业务启动 · 无 UI',60,$2::jsonb,$3::jsonb)",[source.templateId,JSON.stringify(payload),JSON.stringify(source.scripts)]);
 }
 await client.query('COMMIT');
 if(args.includes('--apply')){
  const row=(await client.query('SELECT version,payload,scripts FROM gamecli_ui.templates WHERE id=$1',[source.templateId])).rows[0];
  if(!isDeepStrictEqual(row.scripts,source.scripts)||!isDeepStrictEqual(row.payload,payload))throw Error('Database readback mismatch.');
  console.log(JSON.stringify({id:source.templateId,version:row.version,files:row.scripts.files.length,nodes:row.payload.design.nodes.length,verified:true}));
 }else console.log(JSON.stringify({id:source.templateId,files:source.scripts.files.map(f=>f.path),exists:!!current,dryRun:true}));
}catch(e){await client.query('ROLLBACK');throw e;}finally{client.release();await db.close();}
