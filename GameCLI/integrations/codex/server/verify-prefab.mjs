import {execFile} from 'node:child_process';
import {promisify} from 'node:util';
import {readFile,mkdir,writeFile,realpath,access} from 'node:fs/promises';
import {join,basename,relative,isAbsolute,resolve} from 'node:path';
import {createHash,randomUUID} from 'node:crypto';
import {selectPrefab,comparePrefab} from '../shared/prefab-verification.mjs';
import {nodeName} from '../shared/design.mjs';
const execute=promisify(execFile);
export async function verifyPrefab({projectId,prefabId,cliPath,assetPath,output},workspaces){
 const binding=await workspaces.unityBindings.requireBinding(projectId);
 const snapshot=await workspaces.snapshot(projectId);
 const design=selectPrefab(snapshot.design,prefabId);
 const root=design.nodes.find(n=>n.id===prefabId);
 let executable=cliPath??process.env.GAMECLI_UNITY_CLI;
 if(!executable){
  // A local bridge may be developed in another Unity project with its matching CLI build.
  const packages=JSON.parse(await readFile(join(binding.path,'Packages','manifest.json'),'utf8'));
  const source=packages.dependencies?.['com.oathx.unitycli'];
  if(source?.startsWith('file:')){
   const packagePath=resolve(binding.path,'Packages',source.slice(5));
   const candidate=resolve(packagePath,'..','..','Library','UnityCLI','UnityCLI.exe');
   try{await access(candidate);executable=candidate;}catch{}
  }
  executable??=join(binding.path,'Library','UnityCLI','unity-cli.exe');
 }
 async function cli(args){
  let stdout;
  try{({stdout}=await execute(executable,['--project',binding.path,'--format','json',...args],{timeout:120000,maxBuffer:32*1024*1024,windowsHide:true}));}
  catch(e){throw Error('UnityCLI 执行失败：'+(e.stderr||e.stdout||e.message));}
  const result=JSON.parse(stdout);if(!result.ok)throw Error(result.error?.message||result.message||'UnityCLI failed');return result.data;
 }
 const ping=await cli(['ping']);
 if(await realpath(ping.endpoint.projectPath)!==await realpath(binding.path))throw Error('UnityCLI 连接到了其他工程。');
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
