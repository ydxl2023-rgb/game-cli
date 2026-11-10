import {openDatabase} from '../server/database.mjs';
import {workspaceSchema} from '../server/workspace-schema.mjs';
import {workspaceActor} from '../server/workspaces.mjs';
import {readFile,mkdir,writeFile} from 'node:fs/promises';
const db=await openDatabase(),c=await db.pool.connect();
try{
 await c.query('BEGIN');
 const projects=(await c.query('SELECT * FROM gamecli_ui.project_ui FOR UPDATE')).rows;
 const templates=(await c.query('SELECT * FROM gamecli_ui.templates FOR UPDATE')).rows;
 const backup=new URL('../.data/workspace-migration/',import.meta.url);await mkdir(backup,{recursive:true});await writeFile(new URL(`${Date.now()}.json`,backup),JSON.stringify({projects,templates},null,2));
 await c.query(workspaceSchema);
 await c.query('INSERT INTO gamecli_ui.template_admins(actor) VALUES($1) ON CONFLICT DO NOTHING',[workspaceActor()]);
 for(const t of templates)await c.query('INSERT INTO gamecli_ui.template_versions(template_id,version,name,category_id,payload,scripts) VALUES($1,$2,$3,$4,$5,$6) ON CONFLICT DO NOTHING',[t.id,t.version,t.name,t.category_id,t.payload,t.scripts]);
 for(const p of projects){
  await c.query('INSERT INTO gamecli_ui.projects(id,name) VALUES($1,$2) ON CONFLICT DO NOTHING',[p.project_id,p.project_name]);
  await c.query("INSERT INTO gamecli_ui.project_members VALUES($1,$2,'owner') ON CONFLICT DO NOTHING",[p.project_id,workspaceActor()]);
  await c.query('INSERT INTO gamecli_ui.project_ui_revisions(project_id,version,payload) VALUES($1,$2,$3) ON CONFLICT DO NOTHING',[p.project_id,p.version,p.payload]);
  let changed=false;for(const n of p.payload.design.nodes){const t=templates.find(t=>t.id===n.templateSourceId);if(t&&n.templateSourceVersion===undefined){n.templateSourceVersion=t.version;n.scripts=structuredClone(t.scripts??null);changed=true;}}
  if(changed)await c.query('UPDATE gamecli_ui.project_ui SET payload=$1,version=version+1,updated_at=now() WHERE project_id=$2',[p.payload,p.project_id]);
 }
 // Retain the legacy local draft separately; never replace the saved project with it.
 try{const draft=await readFile(new URL('../.data/draft.json',import.meta.url),'utf8');await writeFile(new URL('legacy-draft.json',backup),draft);}catch(e){if(e.code!=='ENOENT')throw e;}
 await c.query('COMMIT');console.log(JSON.stringify({projects:projects.map(p=>p.project_id),templateSnapshots:templates.length,actor:workspaceActor(),backup:backup.pathname}));
}catch(e){await c.query('ROLLBACK');throw e;}finally{c.release();await db.close();}
