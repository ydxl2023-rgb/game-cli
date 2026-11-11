import {projectUiSchema,readProjectUi,writeProjectUi} from './project-ui.mjs';
import {publishTemplates} from './template-publish.mjs';
import {templateScriptsSchema,saveTemplateScripts} from './template-scripts.mjs';
import {standardAnimationsSchema} from './standard-animations.mjs';
import {templateGroups,initialTemplateGroups} from '../shared/template-groups.mjs';
import {openDatabase,databaseConfig} from './database.mjs';
import {builtInTemplates,categories,validateTemplate} from '../shared/templates.mjs';

export const schemaSql=`
CREATE SCHEMA IF NOT EXISTS gamecli_ui;
CREATE TABLE IF NOT EXISTS gamecli_ui.template_categories (
 id text PRIMARY KEY, name text NOT NULL, sort integer NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS gamecli_ui.templates (
 id text PRIMARY KEY, category_id text NOT NULL REFERENCES gamecli_ui.template_categories(id),
 name text NOT NULL, description text NOT NULL DEFAULT '', sort integer NOT NULL DEFAULT 0,
 version integer NOT NULL DEFAULT 1 CHECK(version>0), payload jsonb NOT NULL,
 created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NOT NULL DEFAULT now()
);`;

export async function migrateTemplateGroups(client){
 await client.query(`CREATE TABLE IF NOT EXISTS gamecli_ui.template_groups (
 category_id text NOT NULL REFERENCES gamecli_ui.template_categories(id),
 id text NOT NULL, name text NOT NULL, sort integer NOT NULL DEFAULT 0,
 PRIMARY KEY(category_id,id));`);
 for(const g of templateGroups)await client.query('INSERT INTO gamecli_ui.template_groups(category_id,id,name,sort) VALUES($1,$2,$3,$4) ON CONFLICT DO NOTHING',[g.category,g.id,g.name,g.sort]);
 await client.query("INSERT INTO gamecli_ui.template_groups(category_id,id,name,sort) SELECT id,'general','未分类',999 FROM gamecli_ui.template_categories ON CONFLICT DO NOTHING");
 await client.query("ALTER TABLE gamecli_ui.templates ADD COLUMN IF NOT EXISTS group_id text NOT NULL DEFAULT 'general'");
 await client.query(`DO $$ BEGIN
 IF NOT EXISTS(SELECT 1 FROM pg_constraint WHERE conname='templates_group_fk' AND conrelid='gamecli_ui.templates'::regclass) THEN
 ALTER TABLE gamecli_ui.templates ADD CONSTRAINT templates_group_fk FOREIGN KEY(category_id,group_id) REFERENCES gamecli_ui.template_groups(category_id,id);
 END IF; END $$`);
 for(const [id,group]of Object.entries(initialTemplateGroups))await client.query("UPDATE gamecli_ui.templates SET group_id=$1 WHERE id=$2 AND group_id='general' AND EXISTS(SELECT 1 FROM gamecli_ui.template_groups g WHERE g.category_id=templates.category_id AND g.id=$1)",[group,id]);
 await client.query('CREATE INDEX IF NOT EXISTS templates_group_sort_idx ON gamecli_ui.templates(category_id,group_id,sort)');
}

export async function seedTemplates(pool) {
  const client=await pool.connect();
  try {
    await client.query('BEGIN');await client.query(schemaSql);
    for(const c of categories)await client.query('INSERT INTO gamecli_ui.template_categories(id,name,sort) VALUES($1,$2,$3) ON CONFLICT(id) DO NOTHING',[c.id,c.name,c.sort]);
    await migrateTemplateGroups(client);await client.query(projectUiSchema);await client.query(templateScriptsSchema);await client.query(standardAnimationsSchema);
    for(const t of builtInTemplates())await client.query('INSERT INTO gamecli_ui.templates(id,category_id,name,description,sort,payload,group_id) SELECT $1,$2,$3,$4,$5,$6::jsonb,$7 WHERE NOT EXISTS (SELECT 1 FROM gamecli_ui.templates WHERE id=$1)',[t.id,t.category,t.name,t.description,t.sort,JSON.stringify(t.payload),t.group]);
    await client.query('COMMIT');
  } catch(e){await client.query('ROLLBACK');throw e;}finally{client.release();}
}
export function createTemplateStore({connect=openDatabase}={}) {
  let connection;
  async function list() {
    const config=await databaseConfig();
    if(!config.PGDATABASE)return {source:'builtin',categories,groups:templateGroups,templates:builtInTemplates()};
    try {
      connection??=await connect(config);
      const categoryRows=await connection.pool.query('SELECT id,name,sort FROM gamecli_ui.template_categories ORDER BY sort,id');
      const groups=await connection.pool.query('SELECT category_id AS category,id,name,sort FROM gamecli_ui.template_groups ORDER BY sort,id');
      const rows=await connection.pool.query('SELECT id,category_id AS category,group_id AS "group",name,description,sort,version,payload,scripts,updated_at AS "updatedAt" FROM gamecli_ui.templates ORDER BY sort,id');
      return {source:'postgresql',categories:categoryRows.rows,groups:groups.rows,templates:rows.rows.map(t=>({...t,payload:validateTemplate(t.payload,t.category)}))};
    } catch(e){if(connection){await connection.close().catch(()=>{});connection=null;}throw new Error('模板数据库连接或读取失败，请检查 SSH 隧道与数据库配置。');}
  }
  async function asset(id) {
    if(!/^[a-f0-9]{64}$/.test(id))throw new Error('资源 ID 无效。');
    connection??=await connect(await databaseConfig());
    const result=await connection.pool.query('SELECT mime,body FROM gamecli_ui.assets WHERE id=$1',[id]);
    return result.rows[0]??null;
  }
  async function publish(snapshot,targets){connection??=await connect(await databaseConfig());return publishTemplates(connection.pool,snapshot,targets);}
  async function project(){connection??=await connect(await databaseConfig());return readProjectUi(connection.pool);}
  async function saveProject(snapshot,version){connection??=await connect(await databaseConfig());return writeProjectUi(connection.pool,snapshot,version);}
  async function saveScripts(args){connection??=await connect(await databaseConfig());return saveTemplateScripts(connection.pool,args);}
  return {list,asset,publish,project,saveProject,saveScripts,async close(){await connection?.close();connection=null;}};
}
