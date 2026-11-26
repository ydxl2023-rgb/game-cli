#!/usr/bin/env node
import {inspectRenderOrder} from '../shared/render-order.mjs';
import {selectPrefab} from '../shared/prefab-verification.mjs';
import {createWorkspaces} from '../server/workspaces.mjs';
import {verifyPrefab} from '../server/verify-prefab.mjs';
import {verifyCode} from '../server/verify-code.mjs';
import {verifyArchitecture} from '../server/verify-architecture.mjs';
import {verifyIntegration} from '../server/verify-integration.mjs';
const usage='gamecli verify layout --project <项目ID> --prefab <项目根节点ID>\ngamecli verify integration --project <项目ID> --template <模板ID> [--mapping <路径适配.json>] [--scenarios <系统用例.json>] [--unity-cli <exe>] [--acceptance <运行验收.json>] [--output <新报告.json>]\ngamecli verify prefab --project <项目ID> --prefab <项目根节点ID> [--asset Assets/...prefab] [--unity-cli <exe>] [--output <新报告.json>]\ngamecli verify code --project <项目ID> [--system <系统目录名>] [--output <新报告.json>]';
const architectureUsage='\ngamecli verify architecture --project <项目ID> [--output <新报告.json>]';
const args=process.argv.slice(2);let workspaces;
try{
 if(args.length===1&&['--help','-h'].includes(args[0])){console.log(usage+architectureUsage);}
 else{
  if(args.shift()!=='verify')throw Error(usage);
  const kind=args.shift();if(!['prefab','code','integration','layout','architecture'].includes(kind))throw Error(usage+architectureUsage);
  const names=kind==='integration'?{'--project':'projectId','--template':'templateId','--scenarios':'scenariosPath','--unity-cli':'cliPath','--mapping':'mappingPath','--acceptance':'acceptancePath','--output':'output'}:kind==='code'?{'--project':'projectId','--system':'system','--output':'output'}:{'--project':'projectId','--prefab':'prefabId','--asset':'assetPath','--unity-cli':'cliPath','--output':'output'},options={};
  const allowed=kind==='architecture'?{'--project':'projectId','--output':'output'}:names;
  while(args.length){const key=allowed[args.shift()],value=args.shift();if(!key||!value||value.startsWith('--')||options[key])throw Error(usage+architectureUsage);options[key]=value;}
  if(!options.projectId||!(kind==='architecture'?true:kind==='integration'?options.templateId:kind==='code'?true:options.prefabId))throw Error(usage+architectureUsage);
  workspaces=createWorkspaces();const report=kind==='layout'?inspectRenderOrder(selectPrefab((await workspaces.snapshot(options.projectId)).design,options.prefabId)):await (kind==='architecture'?verifyArchitecture:kind==='integration'?verifyIntegration:kind==='code'?verifyCode:verifyPrefab)(options,workspaces);
  console.log(JSON.stringify(report,null,2));process.exitCode=report.passed?0:report.status==='failed'?1:2;
 }
}catch(error){console.error(JSON.stringify({status:'error',message:error.message}));process.exitCode=2;}
finally{await workspaces?.close();}
