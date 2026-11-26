import {spawnSync} from 'node:child_process';
import {fileURLToPath} from 'node:url';
import {readFile,writeFile} from 'node:fs/promises';
import {collectStyleCode,collectCode} from '../../../../../integrations/codex/server/verify-code.mjs';
const project=new URL('../../client/',import.meta.url).pathname.replace(/^\/(\w:)/,'$1');
for(const [name,mode] of [['code-local','style'],['task-local','Task'],['shop-local','Shop'],['hospital-local','Hospital'],['architecture-local','architecture'],['editor-local','editor'],['actor-local','actor']]) {
 let scope;
 if(mode==='editor') {
  const files=[];
  for(const name of ['MiniwowHospitalSetup','MiniwowHospitalProbe','MiniwowShopSetup','MiniwowShopProbe','MiniwowTaskGuideSetup','MiniwowTaskGuideProbe']) {
   const path=`Assets/Editor/${name}.cs`;
   files.push({path,content:await readFile(`${project}/${path}`,'utf8')});
  }
  scope={files};
 } else if(mode==='actor') scope={files:[{path:'Assets/Scripts/Systems/Actor/ActorJoystick.cs',content:await readFile(`${project}/Assets/Scripts/Systems/Actor/ActorJoystick.cs`,'utf8')}]};
 else scope=mode==='architecture'?await collectCode(project,undefined,true):await collectStyleCode(project,['Task','Shop','Hospital'].includes(mode)?mode:'Home');
 const binary=fileURLToPath(new URL('../../../../../clis/code-verify/bin/Release/net8.0/CodeVerify.dll',import.meta.url));
 const run=spawnSync('dotnet',[binary],{input:JSON.stringify({files:scope.files,mode:mode==='architecture'?'architecture':undefined}),encoding:'utf8',windowsHide:true});
 if(run.status!==0)throw Error(run.stderr||run.stdout);
 const result=JSON.parse(run.stdout);
 const errors=result.issues.filter(x=>x.severity==='error');
 await writeFile(new URL(`./${name}.json`,import.meta.url),JSON.stringify({localOnly:true,...result,passed:errors.length===0},null,2));
 console.log(name,errors.length,'errors',JSON.stringify(errors));
 if(errors.length)process.exitCode=1;
}
