import {writeFile,mkdir} from 'node:fs/promises';
import {resolve,join} from 'node:path';
import {randomUUID,createHash} from 'node:crypto';
import {analyzeCode,collectCode} from './verify-code.mjs';
export async function verifyArchitecture({projectId,output},workspaces){
 const binding=await workspaces.unityBindings.requireBinding(projectId);
 const scope=await collectCode(binding.path,undefined,true);
 const result=await analyzeCode(scope.files,'architecture');
 const violations=result.issues.some(i=>i.rule==='OXA001');
 const incomplete=result.issues.some(i=>i.rule!=='OXA001')||result.statistics.systems===0;
 const report={schemaVersion:'gamecli-architecture-verification/1',projectId,unityProjectId:binding.unityProjectId,
  ...result,status:violations?'failed':incomplete?'incomplete':'passed',passed:!violations&&!incomplete,excluded:scope.excluded,
  limitations:['Source-symbol analysis without Unity assemblies; compilation and runtime behavior require separate verification.','All handwritten Assets/Scripts sources supply context; Systems/<module> defines the enforced ownership boundary. Composition outside Systems is outside this rule.','Generated code, interface indirection and arbitrary reflection/dynamic dispatch are not fully covered. Only active default preprocessor branches are analyzed.'],
  sources:scope.files.map(f=>({path:f.path,sha256:createHash('sha256').update(f.content).digest('hex')}))};
 if(JSON.stringify(await collectCode(binding.path,undefined,true))!==JSON.stringify(scope))throw Error('Source changed during verification; rerun.');
 const current=await workspaces.unityBindings.requireBinding(projectId);
 if(current.path!==binding.path||current.unityProjectId!==binding.unityProjectId)throw Error('Unity binding changed during verification.');
 const directory=join(binding.path,'Library','GameCLI','Verify',randomUUID());
 await mkdir(directory,{recursive:true});report.evidenceDirectory=directory;
 await writeFile(join(directory,'report.json'),JSON.stringify(report,null,2));
 if(output)await writeFile(resolve(output),JSON.stringify(report,null,2),{flag:'wx'});
 return report;
}
