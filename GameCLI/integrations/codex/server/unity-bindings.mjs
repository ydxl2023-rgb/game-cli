import {readFile,writeFile,realpath,stat} from 'node:fs/promises';
import {join,isAbsolute,relative} from 'node:path';
import {hostname} from 'node:os';
import {randomUUID} from 'node:crypto';
export const unityBindingSchema=`
CREATE TABLE IF NOT EXISTS gamecli_ui.unity_bindings(project_id text PRIMARY KEY REFERENCES gamecli_ui.projects(id),unity_id uuid UNIQUE NOT NULL,updated_at timestamptz NOT NULL DEFAULT now());
CREATE TABLE IF NOT EXISTS gamecli_ui.unity_locations(project_id text NOT NULL REFERENCES gamecli_ui.unity_bindings(project_id),host_id text NOT NULL,path text NOT NULL,path_key text NOT NULL,PRIMARY KEY(project_id,host_id),UNIQUE(host_id,path_key));`;
const markerName='GameCLIProject.json';
async function unityRoot(path){
 if(typeof path!=='string'||!isAbsolute(path))throw Error('请提供 Unity 工程绝对路径。');
 const root=await realpath(path);
 for(const name of ['Assets','Packages','ProjectSettings']){const folder=await realpath(join(root,name));if(!(await stat(folder)).isDirectory()||relative(root,folder).startsWith('..')||isAbsolute(relative(root,folder)))throw Error('Unity 工程目录无效：'+name);}
 await stat(join(root,'ProjectSettings','ProjectVersion.txt'));return root;
}
async function marker(path){try{const v=JSON.parse(await readFile(join(path,'ProjectSettings',markerName),'utf8'));if(v.schemaVersion!==1||typeof v.projectId!=='string'||!/^[a-f0-9-]{36}$/i.test(v.unityProjectId))throw Error('关联文件格式无效');return v;}catch(e){if(e.code==='ENOENT')return null;throw e;}}
export function createUnityBindings({connection,access,hostId=hostname().toLowerCase()}){
 let init;async function pool(){const p=(await connection()).pool;await(init??=p.query(unityBindingSchema).catch(e=>{init=null;throw e;}));return p;}
 async function read(id){await access(id);const row=(await (await pool()).query('SELECT b.unity_id,l.path FROM gamecli_ui.unity_bindings b LEFT JOIN gamecli_ui.unity_locations l ON l.project_id=b.project_id AND l.host_id=$2 WHERE b.project_id=$1',[id,hostId])).rows[0];
 if(!row)return {status:'unbound',hostId};
 const result={projectId:id,unityProjectId:row.unity_id,path:row.path,hostId};
 if(!row.path)return {...result,status:'unavailable',message:'此机器尚未登记工程目录。'};
 try{const root=await unityRoot(row.path),file=await marker(root);if(!file||file.projectId!==id||file.unityProjectId!==row.unity_id)throw Error('数据库与 Unity 工程关联文件不一致。');return {...result,path:root,status:'verified'};}catch(e){return {...result,status:'mismatch',message:e.message};}
 }
 async function requireBinding(id){const binding=await read(id);if(binding.status!=='verified')throw Error('Unity 工程绑定校验失败：'+(binding.message??'项目尚未绑定 Unity 工程。'));return binding;}
 async function bind(id,args){
 if(args.projectId!==id)throw Error('绑定请求项目不一致。');const project=await access(id,true);if(project.role!=='owner')throw Error('仅项目所有者可绑定 Unity 工程。');
 const root=await unityRoot(args.path),pathKey=process.platform==='win32'?root.toLowerCase():root,p=await pool(),c=await p.connect();
 try{await c.query('BEGIN');await c.query("SELECT pg_advisory_xact_lock(hashtext('gamecli_ui.unity_bindings'))");
 const existing=(await c.query('SELECT unity_id FROM gamecli_ui.unity_bindings WHERE project_id=$1',[id])).rows[0];
 const current=(await c.query('SELECT path FROM gamecli_ui.unity_locations WHERE project_id=$1 AND host_id=$2',[id,hostId])).rows[0];
 if(current&&current.path!==root&&args.confirm!==true)throw Error('变更本机工程目录需要确认。');
 let file=await marker(root);if(file&&file.projectId!==id)throw Error('该 Unity 工程已关联其他项目，不能覆盖。');
 if(existing&&file&&existing.unity_id!==file.unityProjectId)throw Error('Unity 工程唯一 ID 不匹配，不能更换为另一工程。');
 if(existing&&!file)throw Error('已绑定项目的目标目录缺少关联文件；迁移工程时请保留 ProjectSettings/GameCLIProject.json。');
 const unityId=existing?.unity_id??file?.unityProjectId??randomUUID();
 const conflict=(await c.query('SELECT project_id FROM gamecli_ui.unity_bindings WHERE unity_id=$1 AND project_id<>$2',[unityId,id])).rows[0];if(conflict)throw Error('Unity 工程 ID 已被其他项目使用。');
 if(!file){file={schemaVersion:1,projectId:id,unityProjectId:unityId};await writeFile(join(root,'ProjectSettings',markerName),JSON.stringify(file,null,2)+'\n',{flag:'wx'});}
 await c.query('INSERT INTO gamecli_ui.unity_bindings(project_id,unity_id) VALUES($1,$2) ON CONFLICT(project_id) DO UPDATE SET updated_at=now()',[id,unityId]);
 await c.query('INSERT INTO gamecli_ui.unity_locations(project_id,host_id,path,path_key) VALUES($1,$2,$3,$4) ON CONFLICT(project_id,host_id) DO UPDATE SET path=excluded.path,path_key=excluded.path_key',[id,hostId,root,pathKey]);
 await c.query('COMMIT');return await requireBinding(id);
 }catch(e){await c.query('ROLLBACK');throw e;}finally{c.release();}
 }
 return {read,bind,requireBinding};
}
