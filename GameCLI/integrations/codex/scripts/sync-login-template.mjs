import assert from 'node:assert/strict';
import {readFile,writeFile,mkdir,readdir,realpath} from 'node:fs/promises';
import {dirname,join,resolve,relative,isAbsolute} from 'node:path';
import {fileURLToPath} from 'node:url';
import {createHash} from 'node:crypto';
import {isDeepStrictEqual} from 'node:util';
import {readSourcePackage} from '../server/source-package.mjs';
import {validateScripts} from '../server/template-scripts.mjs';
import {createWorkspaces} from '../server/workspaces.mjs';

// Archive the existing Unity files without rewriting prefabs or generating UI.
// Publication uses the same draft/candidate/version guards as the Studio editor.
const apply=process.argv.includes('--apply');
assert(process.argv.slice(2).every(a=>a==='--apply'),'Only --apply is supported; default is read-only preflight.');
const directory=dirname(fileURLToPath(import.meta.url));
const manifestPath=resolve(directory,'../config/login-scripts.json');
const manifest=JSON.parse(await readFile(manifestPath,'utf8'));
const project=await realpath(resolve(dirname(manifestPath),manifest.projectRoot));
const workspaces=createWorkspaces();
const hash=bytes=>createHash('sha256').update(bytes).digest('hex');
const archivePrefix='GameCLI/Templates/Login/';
async function walk(path){
 const entries=await readdir(path,{withFileTypes:true}),files=[];
 for(const entry of entries.sort((a,b)=>a.name.localeCompare(b.name))){
  assert(!entry.isSymbolicLink(),'Resource symlinks are not supported.');
  const next=join(path,entry.name);
  if(entry.isDirectory())files.push(...await walk(next));else if(entry.isFile())files.push(next);
 }
 return files;
}
try{
 const binding=await workspaces.unityBindings.requireBinding(manifest.projectId);
 assert.equal(await realpath(binding.path),project,'Manifest must use the verified source project.');
 const source=await readSourcePackage(manifestPath);
 const materials=[];
 for(const folder of ['Assets/Data/Widgets/SVLogin','Assets/Data/Widgets/ServerSelect']){
  for(const absolute of [join(project,folder+'.meta'),...await walk(join(project,folder))]){
   const real=await realpath(absolute),path=relative(project,real).replaceAll('\\','/');
   assert(!isAbsolute(path)&&!path.startsWith('../'),'Resource escaped the source project.');
   const bytes=await readFile(real);
   materials.push({scope:'unity',path,encoding:'base64',sha256:hash(bytes),content:bytes.toString('base64')});
  }
 }
 const workbook=await readFile(resolve(project,'../config/propCommonText.xlsx'));
 materials.push({scope:'configuration',path:'propCommonText.xlsx',encoding:'base64',sha256:hash(workbook),content:workbook.toString('base64')});
 for(const path of ['Assets/Scripts/Protocols/Protocols.cs','Assets/Scripts/Protocols/Protocols.cs.meta']){
  const bytes=await readFile(join(project,path));
  materials.push({scope:'reference',path,encoding:'base64',sha256:hash(bytes),content:bytes.toString('base64')});
 }
 for(const file of materials)assert.equal(hash(Buffer.from(file.content,'base64')),file.sha256);
 const requirements={
  schemaVersion:1,template:'SvLogin',source:'development',
  pages:[{widget:'UILogin',asset:'Assets/Data/Widgets/SVLogin/SVLogin.prefab'},{widget:'UIServerSelect',asset:'Assets/Data/Widgets/ServerSelect/ServerSelect.prefab'}],
  inlineCells:['UIServerGroupItem / VMServerGroupItem','UIServerItem / VMServerItem'],
  syncActions:['SyncFancyGridScrollView','SyncFancyGridClick'],
  sharedTypes:['RuntimeSystem : XEventModule','SystemActive.type : System.Type','LoginSucceed.pid : int','NoticeSystem','MasterEntity.GetAttr(MasterPropertyDef.PID)','Oathx.HttpProtocol.LoginResponse (pid, loginAt, tmcreate)'],
  legacyDependency:'LoginEntity.Refresh(LoginResponse) is still present in development. Its original Protocols.cs is archived in reference scope; do not install unrelated protocol types blindly. The local login path does not invoke a network service.',
  entityInitialization:['LoginEntity','MasterEntity'],
  configuration:{source:'propCommonText.xlsx',ids:{LOGIN_RECENT_SERVER:1001,LOGIN_RECOMMENDED_SERVER:1002,LOGIN_SERVER_RANGE:1003,LOGIN_LOCAL_SERVER_NAME:1004,LOGIN_SERVER_NORMAL:1005,LOGIN_SERVER_BUSY:1006,LOGIN_SERVER_MAINTENANCE:1007},requires:['Config.Excel generated aliases','Excel.propCommonTextById','Excel.propLanguageById','loaded CommonText and Language tables']},
  semantics:'Local test server catalog and local LoginSucceed event; no server authentication.',
  packageDependencies:JSON.parse(await readFile(join(project,'Packages/manifest.json'),'utf8')).dependencies,
  resourcePackage:archivePrefix+'materials.json'
 };
 // Do not publish machine-local package locations as installation instructions.
 for(const [name,value]of Object.entries(requirements.packageDependencies))if(value.startsWith('file:'))requirements.packageDependencies[name]='Target-compatible local package required';
 const instructions=`# Login standard source and production materials\n\nThe C# files are exact development sources, not the reduced practice trial implementation.\n\nRead dependencies.json before integration. This module owns Login and the ServerSelect subpage; it does not introduce a ServerSelectSystem. The standard editor has one design root, so the ServerSelect prefab, inline Fancy cells and their serialized Metadata are delivered as companion materials, not appended as visible children of the Login screen. The existing published main-screen design and animation are preserved.\n\nmaterials.json uses schema gamecli-source-materials/1. Each entry has a unity, configuration or reference scope, relative path, base64 content and SHA256 of the original decoded bytes. It includes both original prefab trees, images, animation/controller files, importer metadata and the source workbook. Verify every decoded SHA256 before use; preserve original GUIDs. Never overwrite a target product's edited resources or replace its entire CommonText table. Merge the required aliases, preserve translations, then run the product's normal table exporter. Do not copy generated config classes from development.\n\nFor product adaptation use UnityCLI for prefab generation, component/Metadata binding and animation editing. These native files are the exact source reference/archive, not permission to hand-edit YAML. Inspect both root Metadata and the two inline cell bindings; VM field order is part of the delivery. Register SyncFancyGridScrollView and SyncFancyGridClick before opening the selector.\n\nReuse the target's shared RuntimeSystem, Master/events and Notice contracts, or explicitly adapt them to the product requirements. LoginEntity still declares Refresh(LoginResponse); the original Protocols.cs is archived as reference, not automatically installed. Preserve or adapt this dependency explicitly, and do not restore network calls merely to satisfy its type name. Do not automatically install another full system to resolve a type name. The local catalog is test data, not a real service. This synchronization proves source/material fidelity; it is not a fresh runtime acceptance of a new product.\n`;
 const extras=[['materials.json',JSON.stringify({schema:'gamecli-source-materials/1',files:materials})],['dependencies.json',JSON.stringify(requirements,null,2)],['README.md',instructions]].map(([name,content])=>({path:archivePrefix+name,content}));
 const scripts=validateScripts({schemaVersion:1,files:[...source.scripts.files,...extras]});
 const published=await workspaces.templateEditor.published(source.templateId);
 const design=structuredClone(published.design),root=design.nodes.find(n=>n.parent===null);
 const projectSnapshot=await workspaces.snapshot(source.projectId);
 const projectRoots=projectSnapshot.design.nodes.filter(n=>n.parent===null&&n.templateSourceId===source.templateId&&n.name===source.rootName);
 assert.equal(projectRoots.length,1,'Expected one Login source-project card.');
 const summary={apply,template:source.templateId,baseVersion:published.workspace.baseVersion,sourceCSharpFiles:source.scripts.files.filter(f=>f.path.endsWith('.cs')).length,packageFiles:scripts.files.length,materialFiles:materials.length,packageBytes:Buffer.byteLength(JSON.stringify(scripts)),mainDesignPreserved:true};
 assert(source.scripts.files.some(f=>f.path.endsWith('/UIServerSelect.cs')),'Server selection source is missing.');
 if(!apply){console.log(JSON.stringify(summary,null,2));}
 else{
  const backup=resolve(directory,'../.data/template-history',`login-sync-${Date.now()}`);await mkdir(backup,{recursive:true});
  await writeFile(join(backup,'before-template.json'),JSON.stringify(published));
  await writeFile(join(backup,'before-project.json'),JSON.stringify(projectSnapshot));
  const opened=await workspaces.templateEditor.open({templateId:source.templateId});
  const draft=await workspaces.templateEditor.read(opened.id);
  assert.deepEqual(draft.design,published.design,'Existing edited draft must not be overwritten.');
  root.scripts=scripts;
  const saved=await workspaces.templateEditor.save(opened.id,{draftId:opened.id,revision:draft.revision,design});
  const candidate=await workspaces.templateEditor.submit(opened.id,{draftId:opened.id,revision:saved.revision});
  const detail=await workspaces.candidateDetail(candidate.id);
  assert.deepEqual(detail.scripts,scripts,'Candidate source package changed.');
  const result=await workspaces.publish({candidateId:candidate.id,confirm:true});
  const readback=await workspaces.templateEditor.published(source.templateId);
  assert.deepEqual(readback.design.nodes.find(n=>n.parent===null).scripts,scripts);
  const strip=d=>{const copy=structuredClone(d);for(const n of copy.nodes){delete n.scripts;delete n.templateSourceVersion;}return copy;};
  assert.deepEqual(strip(readback.design),strip(published.design),'Main design must be unchanged.');
  // Bring only the source project's Login package up to date; preserve its layout and all other cards.
  projectRoots[0].scripts=scripts;
  projectRoots[0].templateSourceVersion=result.version;
  const projectSave=await workspaces.save(source.projectId,{projectId:source.projectId,revision:projectSnapshot.revision,projectVersion:projectSnapshot.project.version,design:projectSnapshot.design});
  const verifiedProject=await workspaces.snapshot(source.projectId);
  assert(isDeepStrictEqual(verifiedProject.design.nodes.find(n=>n.id===projectRoots[0].id).scripts,scripts));
  const report={...summary,...result,candidateId:candidate.id,projectRevision:projectSave.revision,readbackVerified:true,backup,files:scripts.files.map(({path,sha256})=>({path,sha256})),materials:materials.map(({scope,path,sha256})=>({scope,path,sha256}))};
  await writeFile(join(backup,'report.json'),JSON.stringify(report,null,2));
  console.log(JSON.stringify({...summary,...result,projectRevision:projectSave.revision,readbackVerified:true,backup},null,2));
 }
}finally{await workspaces.close();}
