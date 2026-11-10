import {createHash} from 'node:crypto';

export const templateScriptsSchema=`ALTER TABLE gamecli_ui.templates ADD COLUMN IF NOT EXISTS scripts jsonb;`;

// Keep source text unchanged, including its BOM, whitespace and line endings.
export function validateScripts(scripts) {
  if(scripts===null)return null;
  if(!scripts||scripts.schemaVersion!==1||!Array.isArray(scripts.files)||scripts.files.length<1||scripts.files.length>128)
    throw new Error('代码包必须包含 schemaVersion=1 和 1–128 个文件。');
  const paths=new Set();let bytes=0;
  const files=scripts.files.map(file=>{
    const path=file?.path;
    if(typeof path!=='string'||path.length>512||path.split('/').some(part=>!part||part==='.'||part==='..'||/[\\:\x00-\x1f]/.test(part)))
      throw new Error('代码文件必须使用安全的相对路径。');
    if(paths.has(path.toLowerCase()))throw new Error('代码包存在重复文件路径。');
    paths.add(path.toLowerCase());
    if(typeof file.content!=='string')throw new Error('代码文件 content 必须为 UTF-8 文本。');
    bytes+=Buffer.byteLength(file.content,'utf8');
    if(bytes>4*1024*1024)throw new Error('代码包不能超过 4 MiB。');
    const sha256=createHash('sha256').update(file.content,'utf8').digest('hex');
    if(file.sha256!==undefined&&file.sha256!==sha256)throw new Error('代码文件 SHA256 校验失败。');
    return {path,content:file.content,sha256};
  });
  return {schemaVersion:1,files};
}

export async function saveTemplateScripts(pool,{templateId,version,scripts}) {
  if(typeof templateId!=='string'||!templateId.trim()||!Number.isInteger(version)||version<1)
    throw new Error('模板 ID 或版本无效。');
  const validated=validateScripts(scripts);
  const result=await pool.query(`UPDATE gamecli_ui.templates SET scripts=$1::jsonb,version=version+1,updated_at=now()
    WHERE id=$2 AND version=$3 AND category_id='system' RETURNING id,version,scripts`,
    [validated===null?null:JSON.stringify(validated),templateId,version]);
  if(result.rows.length!==1)throw new Error('系统模板不存在、类别不匹配或版本已更新，请重新读取后保存。');
  return result.rows[0];
}
