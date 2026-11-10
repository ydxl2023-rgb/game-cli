import {resolve} from 'node:path';
import {mkdir,writeFile} from 'node:fs/promises';
import {isDeepStrictEqual} from 'node:util';
import {openDatabase} from '../server/database.mjs';
import {readSourcePackage} from '../server/source-package.mjs';
import {saveTemplateScripts,validateScripts} from '../server/template-scripts.mjs';

const args=process.argv.slice(2),manifest=args.find(a=>!a.startsWith('--'));
if(!manifest||args.some(a=>a.startsWith('--')&&a!=='--apply'))throw new Error('用法：npm run scripts:sync -- <manifest.json> [--apply]；默认仅预检。');
const source=await readSourcePackage(resolve(manifest)),db=await openDatabase(),client=await db.pool.connect();
try{
 await client.query('BEGIN');
 const template=(await client.query('SELECT id,name,category_id,version,scripts FROM gamecli_ui.templates WHERE id=$1 FOR UPDATE',[source.templateId])).rows[0];
 if(!template||template.category_id!=='system')throw new Error('清单关联的系统模板不存在。');
 const changed=!isDeepStrictEqual(validateScripts(template.scripts),source.scripts);
 let version=template.version;
 if(args.includes('--apply')&&changed){
  const folder=new URL('../.data/script-backups/',import.meta.url);await mkdir(folder,{recursive:true});
  await writeFile(new URL(`${Date.now()}-v${version}.json`,folder),JSON.stringify(template,null,2));
  const saved=await saveTemplateScripts(client,{templateId:template.id,version,scripts:source.scripts});
  const actual=(await client.query('SELECT scripts FROM gamecli_ui.templates WHERE id=$1',[template.id])).rows[0].scripts;
  if(!isDeepStrictEqual(actual,source.scripts))throw new Error('代码包回读校验失败。');
  version=saved.version;
 }
 await client.query('COMMIT');
 console.log(JSON.stringify({template:template.name,templateId:template.id,version,changed,applied:args.includes('--apply')&&changed,files:source.scripts.files.map(({path,sha256})=>({path,sha256}))},null,2));
}catch(e){await client.query('ROLLBACK');throw e;}finally{client.release();await db.close();}
