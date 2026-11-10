import {validateDesign} from '../shared/design.mjs';
export const projectUiSchema=`CREATE TABLE IF NOT EXISTS gamecli_ui.project_ui (
 project_id text PRIMARY KEY, project_name text NOT NULL,
 version integer NOT NULL DEFAULT 1 CHECK(version>0),
 payload jsonb NOT NULL, created_at timestamptz NOT NULL DEFAULT now(),
 updated_at timestamptz NOT NULL DEFAULT now()
);`;
export function projectIdentity(){return {id:process.env.GAMECLI_PROJECT_ID||'GameCLI',name:process.env.GAMECLI_PROJECT_NAME||'GameCLI'};}
export async function readProjectUi(pool){
 const project=projectIdentity();
 const row=(await pool.query('SELECT version,payload,updated_at FROM gamecli_ui.project_ui WHERE project_id=$1',[project.id])).rows[0];
 return {...project,version:row?.version??0,design:row?.payload.design,updatedAt:row?.updated_at??null};
}
export async function writeProjectUi(pool,snapshot,expectedVersion){
 if(!Number.isInteger(expectedVersion)||expectedVersion<0)throw new Error('请重新载入工程版本后保存。');
 const design=validateDesign(snapshot.design),project=projectIdentity();
 const payload=JSON.stringify({design,draftRevision:snapshot.revision,savedAt:snapshot.savedAt});
 const result=expectedVersion===0
  ?await pool.query('INSERT INTO gamecli_ui.project_ui(project_id,project_name,payload) VALUES($1,$2,$3::jsonb) ON CONFLICT DO NOTHING RETURNING version',[project.id,project.name,payload])
  :await pool.query('UPDATE gamecli_ui.project_ui SET project_name=$2,payload=$3::jsonb,version=version+1,updated_at=now() WHERE project_id=$1 AND version=$4 RETURNING version',[project.id,project.name,payload,expectedVersion]);
 if(result.rows.length!==1)throw new Error('工程已被其他窗口更新，本机草稿已保留，请核对数据库工程版本后重试。');
 return {...project,version:result.rows[0].version};
}
