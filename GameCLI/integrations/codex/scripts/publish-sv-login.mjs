import {readFile,writeFile} from 'node:fs/promises';
import {randomUUID} from 'node:crypto';
import {isDeepStrictEqual} from 'node:util';
import {openDatabase} from '../server/database.mjs';
import {createWorkspaces,workspaceActor} from '../server/workspaces.mjs';
import {validateTemplate} from '../shared/templates.mjs';
import {validateScripts} from '../server/template-scripts.mjs';
const folder=new URL('../.data/imports/sv-login/',import.meta.url);
const raw=JSON.parse(await readFile(new URL('template.json',folder),'utf8'));
const payload={...raw,...validateTemplate(raw,'system')};
const scripts=validateScripts(JSON.parse(await readFile(new URL('scripts.json',folder),'utf8')));
if(payload.source?.path!=='Assets/data/UI/NewLogin/@NewLoginView/Prefab/LoginView.prefab')throw Error('Wrong source');
const db=await openDatabase(),workspaces=createWorkspaces({connect:async()=>db});
try{
 const current=(await db.pool.query('SELECT * FROM gamecli_ui.templates WHERE id=$1',['SvLogin'])).rows[0];
 if(!current)throw Error('Register initial template first');
 if(isDeepStrictEqual(current.payload,payload)&&isDeepStrictEqual(current.scripts,scripts)){console.log(JSON.stringify({version:current.version,unchanged:true}));}
 else if(!process.argv.includes('--publish'))console.log(JSON.stringify({template:'SvLogin',baseVersion:current.version,nodes:payload.design.nodes.length,ready:true}));
 else{
  if(!(await db.pool.query('SELECT actor FROM gamecli_ui.template_admins WHERE actor=$1',[workspaceActor()])).rows.length)throw Error('Template administrator required');
  const projects=(await db.pool.query('SELECT project_id,version,md5(payload::text) AS hash FROM gamecli_ui.project_ui ORDER BY project_id')).rows;
  await writeFile(new URL(`before-v${current.version+1}.json`,folder),JSON.stringify({current,projects},null,2));
  const candidateId=randomUUID();
  await db.pool.query('INSERT INTO gamecli_ui.template_candidates(id,template_id,base_version,actor,payload,scripts) VALUES($1,$2,$3,$4,$5::jsonb,$6::jsonb)',[candidateId,'SvLogin',current.version,workspaceActor(),JSON.stringify(payload),JSON.stringify(scripts)]);
  const result=await workspaces.publish({candidateId,confirm:true});
  const saved=(await db.pool.query('SELECT payload,scripts FROM gamecli_ui.templates WHERE id=$1',['SvLogin'])).rows[0];
  if(!isDeepStrictEqual(saved,{payload,scripts}))throw Error('Published readback mismatch');
  const after=(await db.pool.query('SELECT project_id,version,md5(payload::text) AS hash FROM gamecli_ui.project_ui ORDER BY project_id')).rows;
  if(!isDeepStrictEqual(projects,after))throw Error('Project state changed during publication; inspect concurrent edits');
  console.log(JSON.stringify({...result,candidateId,verified:true,projectsUnchanged:true}));
 }
}finally{await db.close();}
