import {runScenarios,validateSuite} from './scenarios.mjs';
import {inspectRenderOrder} from '../shared/render-order.mjs';
import {selectPrefab} from '../shared/prefab-verification.mjs';
import {integrationGate} from '../shared/integration-gate.mjs';
import {readFile,writeFile,mkdir,realpath} from 'node:fs/promises';
import {resolve,relative,isAbsolute,join} from 'node:path';
import {createHash,randomUUID} from 'node:crypto';
const hash=b=>createHash('sha256').update(b).digest('hex');
export async function inspectIntegration(root,files,mappings={}){
 const base=await realpath(root),items=[];
 for(const f of files){
  if(! /^(Assets|LocalPackages)\//.test(f.path??'')||f.path.includes('\\')||f.path.split('/').includes('..'))throw Error('Unsafe material path');
  const target=mappings[f.path]??f.path;
  if(! /^(Assets|LocalPackages)\//.test(target)||target.includes('\\')||target.split('/').includes('..'))throw Error('Unsafe mapped path');
  const path=resolve(base,target);let data;
  try{const actual=await realpath(path),rel=relative(base,actual);if(rel.startsWith('..')||isAbsolute(rel))throw Error('Material escapes project');data=await readFile(actual);}catch(e){if(e.code!=='ENOENT')throw e;}
  items.push({path:f.path,targetPath:target,status:!data?'missing':target===f.path&&hash(data)===f.sha256?'present':'adapted-review-required',sha256:data?hash(data):null});
 }
 return items;
}
export async function verifyIntegration({projectId,templateId,output,acceptancePath,mappingPath,scenariosPath,cliPath},workspaces){
 const binding=await workspaces.unityBindings.requireBinding(projectId);
 const template=await workspaces.templateEditor.published(templateId),scripts=template.design.nodes.find(n=>n.parent===null)?.scripts?.files??[];
 const files=scripts.filter(f=>/^(Assets|LocalPackages)\//.test(f.path)).map(f=>({path:f.path,sha256:hash(f.content)}));
 const references=[];
 for(const file of scripts.filter(f=>f.path.endsWith('/materials.json'))){
  const materials=JSON.parse(file.content);
  const legacy=materials.schema==null && materials.schemaVersion===1;
  if(!legacy && materials.schema!=='gamecli-source-materials/1')throw Error('Unsupported material schema');
  for(const f of materials.files){
   const blob=legacy && f.encoding==null && /^[a-f0-9]{64}$/.test(f.assetId) && f.assetId===f.sha256;
   const inline=f.encoding==='base64' || (legacy && f.encoding==='utf8');
   if(!blob && !(inline && typeof f.content==='string' && hash(Buffer.from(f.content,f.encoding))===f.sha256))
    throw Error('Invalid material hash: '+f.path);
   if(legacy ? /^(Assets|LocalPackages)\//.test(f.path) : f.scope==='unity')files.push(f);
   else references.push({path:f.path,scope:f.scope??'reference',status:'review-required'});
  }
 }
 const mapping=mappingPath?JSON.parse(await readFile(resolve(mappingPath),'utf8')):null;
 if(mapping&&(mapping.templateId!==templateId||mapping.templateVersion!==template.revision||!mapping.paths||typeof mapping.paths!=='object'||Array.isArray(mapping.paths)))throw Error('Mapping must match the current template identity and version');
 const items=await inspectIntegration(binding.path,[...new Map(files.map(f=>[f.path,f])).values()],mapping?.paths??{});
 const placeholders=[];
 for(const item of items.filter(i=>i.path.endsWith('.cs')&&i.status!=='missing')){
  const lines=(await readFile(join(binding.path,item.targetPath),'utf8')).split(/\r?\n/);
  lines.forEach((line,index)=>{if(/(?:=>|return).*ReportPending\s*\(|throw new NotImplementedException/.test(line))placeholders.push({path:item.path,line:index+1,text:line.trim()});});
 }
 const checks=['Unity 编译与 Console','组件及 Metadata 引用','全部入口与独立子页面','关闭与返回流程','动画与布局','数据回填与持久化'].map(name=>({name,status:'not-verified'}));
 const report={schemaVersion:'gamecli-integration-verification/1',projectId,unityProjectId:binding.unityProjectId,templateId,templateVersion:template.revision,passed:false,status:items.some(i=>i.status==='missing')||placeholders.length?'failed':'incomplete',items,references,placeholders,checks,limitations:['文件路径缺失可能是产品适配，需要核实映射；内容一致不代表功能已验收。','不会自动安装资源、覆盖工程或将占位功能标记为完成。']};
 const snapshot=await workspaces.snapshot(projectId);
 report.projectVersion=snapshot.revision;
 const roots=snapshot.design.nodes.filter(n=>n.parent===null&&n.templateSourceId===templateId&&n.systemKind!=='code');
 report.renderOrder=roots.map(n=>({rootId:n.id,...inspectRenderOrder(selectPrefab(snapshot.design,n.id))}));
 report.designSha256=hash(JSON.stringify(snapshot.design));
 report.fingerprint=hash(JSON.stringify({projectId,unityProjectId:binding.unityProjectId,templateId,templateVersion:template.revision,projectVersion:report.projectVersion,designSha256:report.designSha256,renderOrder:report.renderOrder,items,references,placeholders}));
 const directory=join(binding.path,'Library/GameCLI/Verify',randomUUID());
 await mkdir(directory,{recursive:true});
 report.evidenceDirectory=directory;
 if(scenariosPath){
  const suite=JSON.parse(await readFile(resolve(scenariosPath),'utf8'));
  validateSuite(suite,templateId);
  report.scenarioSuiteSha256=hash(JSON.stringify(suite));
  report.fingerprint=hash(JSON.stringify([report.fingerprint,report.scenarioSuiteSha256]));
  report.scenarios=await runScenarios(suite,binding,directory,cliPath,report.fingerprint);
  const current=await workspaces.snapshot(projectId);
  const currentBinding=await workspaces.unityBindings.requireBinding(projectId);
  if(current.revision!==snapshot.revision || currentBinding.path!==binding.path || currentBinding.unityProjectId!==binding.unityProjectId)
   throw Error('工程版本或绑定在场景执行期间发生变化，请重新验收。');
 }
 const acceptance=acceptancePath?JSON.parse(await readFile(resolve(acceptancePath),'utf8')):null;
 report.workflow=integrationGate(report,acceptance);report.passed=report.workflow.passed;report.status=report.passed?'passed':report.workflow.stage==='repair'?'failed':'incomplete';
 report.acceptance=acceptance?.fingerprint===report.fingerprint?acceptance:null;
 await writeFile(join(directory,'integration.json'),JSON.stringify(report,null,2));
 const {openDatabase}=await import('./database.mjs');const db=await openDatabase();
 try{await db.pool.query('CREATE TABLE IF NOT EXISTS gamecli_ui.integration_checks(project_id text NOT NULL,template_id text NOT NULL,report jsonb NOT NULL,updated_at timestamptz NOT NULL DEFAULT now(),PRIMARY KEY(project_id,template_id))');await db.pool.query('INSERT INTO gamecli_ui.integration_checks(project_id,template_id,report) VALUES($1,$2,$3) ON CONFLICT(project_id,template_id) DO UPDATE SET report=EXCLUDED.report,updated_at=now()',[projectId,templateId,JSON.stringify(report)]);}finally{await db.close();}
 if(output)await writeFile(resolve(output),JSON.stringify(report,null,2),{flag:'wx'});return report;
}
