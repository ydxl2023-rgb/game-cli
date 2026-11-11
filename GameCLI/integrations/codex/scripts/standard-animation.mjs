import {readFile,writeFile} from 'node:fs/promises';
import {isDeepStrictEqual} from 'node:util';
import {openDatabase} from '../server/database.mjs';
import {workspaceActor} from '../server/workspaces.mjs';
import {standardAnimationsSchema,validateStandardAnimation,animationHash,bindStandardAnimation} from '../server/standard-animations.mjs';

const [action,...args]=process.argv.slice(2);
const publish=action==='publish',apply=args.includes('--apply');
const positional=args.filter(a=>a!=='--apply');
if(!['publish','list','read','bind'].includes(action)||(!publish&&apply)
 ||(publish&&positional.length!==1)||(action==='list'&&args.length)
 ||(action==='read'&&positional.length!==2)||(action==='bind'&&positional.length!==4))
 throw Error('Usage: standard-animation.mjs publish definition.json [--apply] | list | read id version | bind id version bindings.json output.json');
const db=await openDatabase();let client;
try{
 if(publish){
  const definition=validateStandardAnimation(JSON.parse(await readFile(positional[0],'utf8'))),sha256=animationHash(definition);
  client=await db.pool.connect();await client.query('BEGIN');
  const actor=workspaceActor();
  if(!(await client.query('SELECT actor FROM gamecli_ui.template_admins WHERE actor=$1',[actor])).rowCount)throw Error('Template administrator required.');
  await client.query("SELECT pg_advisory_xact_lock(hashtext('gamecli.standard_animations'))");
  await client.query(standardAnimationsSchema);
  const params=[definition.id,definition.version];
  const current=(await client.query('SELECT * FROM gamecli_ui.standard_animations WHERE id=$1 AND version=$2',params)).rows[0];
  if(current&&(!isDeepStrictEqual(current.definition,definition)||current.sha256!==sha256))throw Error('Version already exists with different content. Publish a new version.');
  if(!current&&apply)await client.query(`INSERT INTO gamecli_ui.standard_animations
   (id,version,name,category,status,definition,sha256,created_by) VALUES($1,$2,$3,$4,'published',$5::jsonb,$6,$7)`,
   [...params,definition.name,definition.category,JSON.stringify(definition),sha256,actor]);
  if(apply){
   const saved=(await client.query('SELECT definition,sha256 FROM gamecli_ui.standard_animations WHERE id=$1 AND version=$2',params)).rows[0];
   if(!isDeepStrictEqual(saved.definition,definition)||animationHash(saved.definition)!==saved.sha256)throw Error('Readback mismatch.');
  }
  await client.query(apply?'COMMIT':'ROLLBACK');
  console.log(JSON.stringify({id:definition.id,version:definition.version,sha256,exists:!!current,dryRun:!apply,verified:apply}));
 }else if(action==='list'){
  console.log(JSON.stringify((await db.pool.query('SELECT id,version,name,category,status,sha256 FROM gamecli_ui.standard_animations ORDER BY id,version DESC')).rows,null,2));
 }else{
  const version=Number(positional[1]);if(!Number.isInteger(version)||version<1)throw Error('Explicit positive version required.');
  const row=(await db.pool.query('SELECT definition,sha256 FROM gamecli_ui.standard_animations WHERE id=$1 AND version=$2',[positional[0],version])).rows[0];
  if(!row||animationHash(row.definition)!==row.sha256)throw Error('Animation missing or checksum mismatch.');
  if(action==='read')console.log(JSON.stringify(row.definition,null,2));
  else{
   const bindings=JSON.parse(await readFile(positional[2],'utf8'));
   await writeFile(positional[3],JSON.stringify(bindStandardAnimation(row.definition,bindings),null,2)+'\n',{flag:'wx'});
   console.log(JSON.stringify({id:positional[0],version,output:positional[3],verified:true}));
  }
 }
}catch(error){if(client)await client.query('ROLLBACK');throw error;}
finally{client?.release();await db.close();}
