import {mkdir,writeFile} from 'node:fs/promises';
import {fileURLToPath} from 'node:url';
import {join} from 'node:path';
import {randomUUID} from 'node:crypto';
import {validateTemplate} from '../shared/templates.mjs';

export function templateFromDraft(snapshot,target,current) {
  const source=snapshot.design,root=source.nodes.find(n=>n.id===target.rootId);
  if(!root||root.parent!==null)throw new Error('同步目标必须是 Canvas 下的模板根节点。');
  if(root.templateSourceId!==current.id&&current.payload.root!==root.id)throw new Error('节点未关联到此数据库模板。');
  if(current.payload.kind==='code'){
    if(root.systemKind!=='code')throw new Error('纯代码候选必须来自代码系统卡片。');
    return {...current.payload,...validateTemplate(current.payload,current.category_id)};
  }
  const ids=new Set([root.id]);let size;
  do{size=ids.size;for(const n of source.nodes)if(ids.has(n.parent))ids.add(n.id);}while(ids.size!==size);
  for(const n of source.nodes)if(ids.has(n.id)&&n.startupComplete?.target&&!ids.has(n.startupComplete.target))throw new Error('启动完成包含项目系统目标，发布标准模板前请清除目标并保留事件端口。');
  const design={...structuredClone(source),name:current.payload.design.name,nodes:structuredClone(source.nodes.filter(n=>ids.has(n.id)))};
  if(design.animations)design.animations=design.animations.filter(a=>ids.has(a.hostNodeId));
  for(const n of design.nodes)delete n.relationPosition;
  for(const n of design.nodes)if(n.action?.target&&!ids.has(n.action.target))throw new Error('模板包含跨系统跳转引用，无法独立保存：'+n.name);
  design.nodes.find(n=>n.id===root.id).templateSourceId=current.id;
  return {...current.payload,...validateTemplate({root:root.id,design},current.category_id),editedFrom:{draftRevision:snapshot.revision,savedAt:snapshot.savedAt}};
}

export async function publishTemplates(pool,snapshot,targets,backupDir=fileURLToPath(new URL('../.data/template-history/',import.meta.url))) {
  if(!Array.isArray(targets)||targets.length>64)throw new Error('同步目标无效。');
  const seen=new Set();
  for(const t of targets){if(!t||typeof t.templateId!=='string'||!Number.isInteger(t.version)||t.version<1||seen.has(t.templateId))throw new Error('同一模板存在多个实例或同步版本无效，请保留一个待同步实例。');seen.add(t.templateId);}
  const client=await pool.connect();
  try{
    await client.query('BEGIN');const results=[];
    for(const t of [...targets].sort((a,b)=>a.templateId.localeCompare(b.templateId))){
      const current=(await client.query('SELECT * FROM gamecli_ui.templates WHERE id=$1 FOR UPDATE',[t.templateId])).rows[0];
      if(!current)throw new Error('数据库模板不存在：'+t.templateId);
      if(current.version!==t.version)throw new Error(`模板 ${current.name} 已更新为 v${current.version}，请核对后刷新模板库再保存。`);
      const payload=templateFromDraft(snapshot,t,current);
      const images=[...new Set(payload.design.nodes.map(n=>n.image).filter(Boolean))];
      if(images.length&&(await client.query('SELECT id FROM gamecli_ui.assets WHERE id=ANY($1::text[])',[images])).rows.length!==images.length)throw new Error('数据库缺少模板图片资源。');
      await mkdir(backupDir,{recursive:true});await writeFile(join(backupDir,`${randomUUID()}-v${current.version}.json`),JSON.stringify(current,null,2));
      const result=await client.query('UPDATE gamecli_ui.templates SET payload=$1::jsonb,version=version+1,updated_at=now() WHERE id=$2 RETURNING version',[JSON.stringify(payload),current.id]);
      results.push({id:current.id,name:current.name,version:result.rows[0].version,rootId:t.rootId,payload:validateTemplate(payload,current.category_id),scripts:current.scripts??null});
    }
    await client.query('COMMIT');return results;
  }catch(e){await client.query('ROLLBACK');throw e;}finally{client.release();}
}
