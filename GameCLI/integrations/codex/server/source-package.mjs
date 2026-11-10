import {readFile,realpath} from 'node:fs/promises';
import {dirname,resolve,relative,isAbsolute,sep} from 'node:path';
import {validateScripts} from './template-scripts.mjs';

export async function readSourcePackage(manifestPath){
 const manifest=JSON.parse(await readFile(manifestPath,'utf8'));
 if(typeof manifest.templateId!=='string'||!manifest.templateId.trim()||typeof manifest.projectRoot!=='string'||!Array.isArray(manifest.files)||!manifest.files.length)throw new Error('清单需要 templateId、projectRoot 和 files。');
 const root=await realpath(resolve(dirname(manifestPath),manifest.projectRoot)),files=[];
 for(const path of manifest.files){
  validateScripts({schemaVersion:1,files:[{path,content:''}]});
  const absolute=await realpath(resolve(root,path)),local=relative(root,absolute);
  if(isAbsolute(local)||local==='..'||local.startsWith('..'+sep))throw new Error('源码文件不能位于工程目录之外。');
  const bytes=await readFile(absolute),content=bytes.toString('utf8');
  if(!Buffer.from(content,'utf8').equals(bytes))throw new Error(`代码文件不是有效 UTF-8：${path}`);
  files.push({path,content});
 }
 return {templateId:manifest.templateId,scripts:validateScripts({schemaVersion:1,files})};
}
