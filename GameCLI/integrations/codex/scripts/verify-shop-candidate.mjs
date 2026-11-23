import {shopWidgetPath} from './shop-widget-path.mjs';
import {readFile,writeFile} from 'node:fs/promises';
import {createWorkspaces} from '../server/workspaces.mjs';
import {verifyPrefab} from '../server/verify-prefab.mjs';
const w=createWorkspaces(),projectId='p-2d314388-18c4-42f5-80eb-f6729dd23f0e';
try{
 const current=await w.snapshot(projectId),results=[];
 for(const key of ['main','recharge','gift','giftbuy']){
  const payload=JSON.parse(await readFile(`.data/imports/sv-shop/${key}/template.json`,'utf8'));
  const report=await verifyPrefab({projectId,prefabId:payload.root,cliPath:'G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe',assetPath:shopWidgetPath(payload.design.name)},{unityBindings:w.unityBindings,snapshot:async()=>({...current,design:payload.design})});
  report.baselineSource='local reviewed Shop candidate (not published project snapshot)';
  await writeFile(`.data/imports/sv-shop/acceptance/candidate-${key}.json`,JSON.stringify(report,null,2));
  results.push({page:key,status:report.status,differences:report.differences,unverifiedCount:report.unverified.length});
 }
 console.log(JSON.stringify(results,null,2));
}finally{await w.close();}
