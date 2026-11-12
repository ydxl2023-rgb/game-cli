import {randomUUID} from 'node:crypto';
import {runDesignAgent} from './design-agent.mjs';
import {compose} from '../shared/composition.mjs';

export const designChatSchema=`CREATE TABLE IF NOT EXISTS gamecli_ui.design_chat (
 id uuid PRIMARY KEY,project_id text NOT NULL REFERENCES gamecli_ui.projects(id),project_version integer NOT NULL,
 prompt text NOT NULL,attachments jsonb NOT NULL DEFAULT '[]',reply text NOT NULL DEFAULT '',operations jsonb NOT NULL DEFAULT '[]',
 status text NOT NULL CHECK(status IN ('thinking','complete','failed','stopped','applied')),error text,applied_version integer,
 created_at timestamptz NOT NULL DEFAULT now());
 CREATE UNIQUE INDEX IF NOT EXISTS design_chat_one_active ON gamecli_ui.design_chat(project_id) WHERE status='thinking';`;
export function validateAttachments(files=[]){
 if(!Array.isArray(files)||files.length>4)throw Error('最多添加 4 个文本参考文件。');
 let size=0;return files.map(f=>{if(typeof f?.name!=='string'||f.name.length>200||typeof f.content!=='string')throw Error('参考文件格式无效。');size+=f.content.length;if(size>100000)throw Error('参考文件内容总计不能超过 100,000 字符。');return {name:f.name,content:f.content};});
}
export function validateProposal(result,design,templates){
 if(typeof result?.reply!=='string'||!result.reply.trim()||result.reply.length>40000)throw Error('Agent 返回的方案内容无效。');
 if(!Array.isArray(result.operations))throw Error('Agent 未返回有效操作列表。');
 for(const o of result.operations){if(!o||typeof o!=='object')throw Error('方案操作无效。');if(o.type==='connect'){const n=design.nodes.find(n=>n.id===o.sourceId);if(!n||(n.type!=='Button'&&!(n.parent===null&&n.systemKind==='code')))throw Error('连线必须从真实按钮或逻辑系统发起，请先添加系统，再补充按钮连线。');}}
 compose(design,result.operations,templates);return {reply:result.reply,operations:result.operations};
}
export function createDesignChat({connection,access,snapshot,composition,agent=runDesignAgent}){
 let initialized;const running=new Map();
 async function pool(){const p=(await connection()).pool;await(initialized??=p.query(designChatSchema).catch(e=>{initialized=null;throw e;}));return p;}
 async function list(id){await access(id);const p=await pool();await p.query("UPDATE gamecli_ui.design_chat SET status='failed',error='设计响应超时或服务已重启，请重新发送。' WHERE project_id=$1 AND status='thinking' AND created_at < now()-interval '5 minutes'",[id]);return {turns:(await p.query('SELECT * FROM (SELECT * FROM gamecli_ui.design_chat WHERE project_id=$1 ORDER BY created_at DESC,id DESC LIMIT 100) t ORDER BY created_at,id',[id])).rows};}
 async function send(id,args){
  await access(id,true);if(typeof args.text!=='string'||!args.text.trim()||args.text.length>12000)throw Error('请输入 1–12,000 字符的需求。');
  const attachments=validateAttachments(args.attachments),s=await snapshot(id);if(s.revision!==args.projectVersion)throw Error('项目版本已变化，请刷新后发送。');
  const history=await list(id),p=await pool(),turnId=randomUUID();
  const templates=(await p.query("SELECT id,name,version,category_id AS category,payload FROM gamecli_ui.templates WHERE category_id='system' ORDER BY id")).rows;
  try{await p.query("INSERT INTO gamecli_ui.design_chat(id,project_id,project_version,prompt,attachments,status) VALUES($1,$2,$3,$4,$5,'thinking')",[turnId,id,s.revision,args.text.trim(),JSON.stringify(attachments)]);}catch(e){if(e.code==='23505')throw Error('当前项目正在生成方案，请等待或停止后再发送。');throw e;}
  const controller=new AbortController();running.set(turnId,controller);
  const context={project:{id,version:s.revision,unityBinding:s.project?.unityBinding,nodes:s.design.nodes.map(n=>({id:n.id,name:n.name,parent:n.parent,type:n.type,systemKind:n.systemKind,templateId:n.templateSourceId,action:n.action}))},catalog:templates.map(t=>({id:t.id,name:t.name,version:t.version,kind:t.payload.kind??'ui',buttons:t.payload.design?.nodes?.filter(n=>n.type==='Button').map(n=>n.name)})),history:history.turns.slice(-20).map(t=>({user:t.prompt,assistant:t.reply,status:t.status,attachments:t.attachments})),request:args.text,attachments};
  const timer=setTimeout(()=>controller.abort(),180000);
  void (async()=>{try{const result=validateProposal(await agent(context,{signal:controller.signal}),s.design,templates);await p.query("UPDATE gamecli_ui.design_chat SET reply=$1,operations=$2,status='complete' WHERE id=$3 AND status='thinking'",[result.reply,JSON.stringify(result.operations),turnId]);}catch(e){await p.query("UPDATE gamecli_ui.design_chat SET status='failed',error=$1 WHERE id=$2 AND status='thinking'",[controller.signal.aborted?'设计已停止或超时。':e.message,turnId]);}finally{clearTimeout(timer);running.delete(turnId);}})().catch(()=>{});
  return {id:turnId,status:'thinking'};
 }
 async function stop(id,args){await access(id,true);const p=await pool();const r=await p.query("UPDATE gamecli_ui.design_chat SET status='stopped' WHERE project_id=$1 AND id=$2 AND status='thinking' RETURNING id",[id,args.turnId]);if(r.rows.length)running.get(args.turnId)?.abort();return {stopped:r.rows.length>0};}
 async function apply(id,args){
  await access(id,true);const p=await pool(),t=(await p.query('SELECT * FROM gamecli_ui.design_chat WHERE project_id=$1 AND id=$2',[id,args.turnId])).rows[0];
  if(!t||t.status!=='complete'||!t.operations.length)throw Error('此消息没有可应用的方案。');
  if(t.project_version!==args.projectVersion)throw Error('方案已过期，请基于最新项目重新讨论。');
  const result=await composition.apply(id,{projectId:id,projectVersion:args.projectVersion,operations:t.operations});
  await p.query("UPDATE gamecli_ui.design_chat SET status='applied',applied_version=$1 WHERE id=$2",[result.revision,t.id]);return result;
 }
 return {list,send,stop,apply,close(){for(const c of running.values())c.abort();}};
}
