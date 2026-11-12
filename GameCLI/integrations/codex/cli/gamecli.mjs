#!/usr/bin/env node
import {createWorkspaces} from '../server/workspaces.mjs';
import {verifyPrefab} from '../server/verify-prefab.mjs';
import {verifyCode} from '../server/verify-code.mjs';
const usage='gamecli verify prefab --project <项目ID> --prefab <项目根节点ID> [--asset Assets/...prefab] [--unity-cli <exe>] [--output <新报告.json>]\ngamecli verify code --project <项目ID> --system <系统目录名> [--output <新报告.json>]';
const args=process.argv.slice(2);let workspaces;
try{
 if(args.length===1&&['--help','-h'].includes(args[0])){console.log(usage);}
 else{
  if(args.shift()!=='verify')throw Error(usage);
  const kind=args.shift();if(!['prefab','code'].includes(kind))throw Error(usage);
  const names=kind==='code'?{'--project':'projectId','--system':'system','--output':'output'}:{'--project':'projectId','--prefab':'prefabId','--asset':'assetPath','--unity-cli':'cliPath','--output':'output'},options={};
  while(args.length){const key=names[args.shift()],value=args.shift();if(!key||!value||value.startsWith('--')||options[key])throw Error(usage);options[key]=value;}
  if(!options.projectId||!(kind==='code'?options.system:options.prefabId))throw Error(usage);
  workspaces=createWorkspaces();const report=await (kind==='code'?verifyCode:verifyPrefab)(options,workspaces);
  console.log(JSON.stringify(report,null,2));process.exitCode=report.passed?0:report.status==='failed'?1:2;
 }
}catch(error){console.error(JSON.stringify({status:'error',message:error.message}));process.exitCode=2;}
finally{await workspaces?.close();}
