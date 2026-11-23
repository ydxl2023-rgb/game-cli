import {readFile,mkdir,writeFile,realpath} from 'node:fs/promises';
import {join,basename,relative,isAbsolute,resolve} from 'node:path';
import {createHash,randomUUID} from 'node:crypto';
import {selectPrefab,comparePrefab} from '../shared/prefab-verification.mjs';
import {nodeName} from '../shared/design.mjs';
import {connectUnityCli} from './unity-cli.mjs';
export async function verifyPrefab({projectId,prefabId,cliPath,assetPath,output},workspaces){
 const binding=await workspaces.unityBindings.requireBinding(projectId);
 const snapshot=await workspaces.snapshot(projectId);
 const design=selectPrefab(snapshot.design,prefabId);
 const root=design.nodes.find(n=>n.id===prefabId);
 const cli=await connectUnityCli(binding.path,cliPath);
 if(!assetPath){
  const result=await cli(['asset','search',nodeName(root),'--type','Prefab','--count','200']);
  const matches=(result.assets??[]).filter(a=>basename(a.path??a.assetPath??'')===nodeName(root)+'.prefab');
  if(matches.length!==1)throw Error('无法唯一确定 Unity 预制资源，请使用 --asset Assets/...prefab 显式指定。');
  assetPath=matches[0].path??matches[0].assetPath;
 }
 if(!/^Assets\/.+\.prefab$/.test(assetPath)||assetPath.includes('\\')||assetPath.split('/').some(p=>p==='..'||p==='.'||!p))throw Error('Unity 资源必须为 Assets 下的 prefab 路径。');
 const realAsset=await realpath(join(binding.path,assetPath)),assets=await realpath(join(binding.path,'Assets'));
 const rel=relative(assets,realAsset);if(rel.startsWith('..')||isAbsolute(rel))throw Error('预制资源越过工程边界。');
 const directory=join(binding.path,'Library','GameCLI','Verify',randomUUID());
 await mkdir(directory,{recursive:true});
 await writeFile(join(directory,'baseline.json'),JSON.stringify({projectId,prefabId,revision:snapshot.revision,design},null,2));
 const exported=join(directory,'unity');
 await cli(['prefab','export','--prefab',assetPath,'--output',exported,'--source-key',binding.unityProjectId]);
 const manifest=JSON.parse(await readFile(join(exported,'manifest.json'),'utf8'));
 if(manifest.schemaVersion!=='ui-export/1'||manifest.sourceKey!==binding.unityProjectId)throw Error('Unity 导出身份或格式不符。');
 const entry=manifest.entries.find(e=>e.assetPath===assetPath);if(!entry)throw Error('Unity 导出缺少目标预制。');
 const documentPath=resolve(exported,entry.file),docRel=relative(exported,documentPath);
 if(docRel.startsWith('..')||isAbsolute(docRel))throw Error('Unity 导出文件路径无效。');
 const document=JSON.parse(await readFile(documentPath,'utf8'));
 const report={schemaVersion:'gamecli-prefab-verification/1',projectId,prefabId,projectVersion:snapshot.revision,unityProjectId:binding.unityProjectId,assetPath,
  baselineSha256:createHash('sha256').update(JSON.stringify(design)).digest('hex'),unitySourceRevision:entry.sourceRevision,
  ...comparePrefab(design,document,entry.diagnostics),evidenceDirectory:directory};
 const current=await workspaces.snapshot(projectId);
 if(current.revision!==snapshot.revision){report.unverified.push({code:'project-changed-during-verification',actual:current.revision});report.status='incomplete';report.passed=false;}
 const finalBinding=await workspaces.unityBindings.requireBinding(projectId);
 if(finalBinding.unityProjectId!==binding.unityProjectId||finalBinding.path!==binding.path)throw Error('验收期间工程绑定发生变化。');
 await writeFile(join(directory,'report.json'),JSON.stringify(report,null,2));
 if(output)await writeFile(resolve(output),JSON.stringify(report,null,2),{flag:'wx'});
 return report;
}
