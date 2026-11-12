import {randomUUID} from 'node:crypto';
import {validateDesign} from '../shared/design.mjs';
import {instantiateTemplate} from '../shared/templates.mjs';
import {templateFromDraft} from './template-publish.mjs';
import {validateScripts} from './template-scripts.mjs';

export const templateDraftSchema=`CREATE TABLE IF NOT EXISTS gamecli_ui.template_drafts (
 id uuid PRIMARY KEY,template_id text NOT NULL REFERENCES gamecli_ui.templates(id),actor text NOT NULL,
 base_version integer NOT NULL,revision integer NOT NULL DEFAULT 1,payload jsonb NOT NULL,scripts jsonb,
 submitted_revision integer,candidate_id uuid,updated_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(template_id,actor,base_version));`;
export function createTemplateEditor({connection,admin,actor}){
 let init;async function pool(){const p=(await connection()).pool;await(init??=p.query(templateDraftSchema).catch(e=>{init=null;throw e;}));return p;}
 function snapshot(row,draft){
  let design=structuredClone(row.payload.design);
  if(row.payload.kind==='code'){design={schemaVersion:1,name:row.name,width:1920,height:1080,nodes:[]};instantiateTemplate(design,{id:row.template_id??row.id,category:row.category_id,name:row.name,version:row.base_version??row.version,payload:row.payload,scripts:row.scripts},null,{x:0,y:0},{allowCode:true});}
  const root=design.nodes.find(n=>n.parent===null);if(root){root.templateSourceId=row.template_id??row.id;root.templateSourceVersion=row.base_version??row.version;root.scripts=row.scripts??null;}
  return {design:validateDesign(design),revision:draft?row.revision:row.version,workspace:{id:row.id,name:row.name,role:draft?'editor':'viewer',version:draft?row.revision:row.version,kind:draft?'template-draft':'template',templateId:row.template_id??row.id,baseVersion:row.base_version??row.version},storage:draft?'模板草稿数据库':'已发布模板（只读）'};
 }
 async function published(id){const r=(await(await connection()).pool.query('SELECT * FROM gamecli_ui.templates WHERE id=$1',[id])).rows[0];if(!r)throw Error('模板不存在。');return snapshot(r,false);}
 async function row(id){await admin();const r=(await(await pool()).query('SELECT d.*,t.name,t.category_id FROM gamecli_ui.template_drafts d JOIN gamecli_ui.templates t ON t.id=d.template_id WHERE d.id=$1 AND d.actor=$2',[id,actor])).rows[0];if(!r)throw Error('草稿不存在或无权访问。');return r;}
 async function read(id){return snapshot(await row(id),true);}
 async function open({templateId}){
  await admin();const p=await pool(),id=randomUUID();
  const r=await p.query(`INSERT INTO gamecli_ui.template_drafts(id,template_id,actor,base_version,payload,scripts) SELECT $1,id,$2,version,payload,scripts FROM gamecli_ui.templates WHERE id=$3 ON CONFLICT(template_id,actor,base_version) DO UPDATE SET actor=EXCLUDED.actor RETURNING id`,[id,actor,templateId]);
  if(!r.rows.length)throw Error('模板不存在。');return {id:r.rows[0].id};
 }
 async function save(id,args){
  if(args.draftId!==id||args.projectId)throw Error('仅接受当前模板草稿，不能保存项目数据。');const r=await row(id);
  const design=validateDesign(args.design),roots=design.nodes.filter(n=>n.parent===null);if(roots.length!==1)throw Error('模板草稿必须且只能包含一个根节点。');
  const payload=templateFromDraft({design,revision:args.revision,savedAt:new Date().toISOString()},{rootId:roots[0].id},{id:r.template_id,category_id:r.category_id,payload:r.payload});
  const scripts=validateScripts(roots[0].scripts??null);for(const n of payload.design.nodes){delete n.scripts;delete n.templateSourceVersion;}
  const p=await pool(),images=[...new Set(payload.design.nodes.map(n=>n.image).filter(Boolean))];if(images.length&&(await p.query('SELECT id FROM gamecli_ui.assets WHERE id=ANY($1::text[])',[images])).rows.length!==images.length)throw Error('草稿缺少图片资源。');
  const saved=await p.query('UPDATE gamecli_ui.template_drafts SET payload=$1,scripts=$2,revision=revision+1,updated_at=now() WHERE id=$3 AND actor=$4 AND revision=$5 RETURNING revision',[JSON.stringify(payload),JSON.stringify(scripts),id,actor,args.revision]);if(!saved.rows.length)throw Error('模板草稿已更新，请重新加载核对。');
  return {revision:saved.rows[0].revision,draftSave:{ok:true,id,name:r.name,version:saved.rows[0].revision}};
 }
 async function submit(id,args){
  if(args.draftId!==id||args.projectId)throw Error('模板草稿身份不一致。');await admin();const c=await(await pool()).connect();
  try{await c.query('BEGIN');const r=(await c.query('SELECT * FROM gamecli_ui.template_drafts WHERE id=$1 AND actor=$2 FOR UPDATE',[id,actor])).rows[0];if(!r||r.revision!==args.revision)throw Error('草稿版本已变化，请先保存并刷新。');
   const current=(await c.query('SELECT version FROM gamecli_ui.templates WHERE id=$1 FOR SHARE',[r.template_id])).rows[0];if(current.version!==r.base_version)throw Error('标准模板已有新版本，请从模板页打开最新版本的新草稿。');
   if(r.submitted_revision===r.revision){await c.query('COMMIT');return {id:r.candidate_id};}
   const candidateId=randomUUID();await c.query('INSERT INTO gamecli_ui.template_candidates(id,template_id,base_version,actor,payload,scripts) VALUES($1,$2,$3,$4,$5,$6)',[candidateId,r.template_id,r.base_version,actor,r.payload,r.scripts]);
   await c.query('UPDATE gamecli_ui.template_drafts SET submitted_revision=revision,candidate_id=$1 WHERE id=$2',[candidateId,id]);await c.query('COMMIT');return {id:candidateId};
  }catch(e){await c.query('ROLLBACK');throw e;}finally{c.release();}
 }
 return {published,read,open,save,submit};
}
