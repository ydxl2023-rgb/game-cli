import {readFile,writeFile} from 'node:fs/promises';
import {analyzeCode} from '../../../../../../../integrations/codex/server/verify-code.mjs';
import {createHash} from 'node:crypto';
const root=new URL('../../../../client/',import.meta.url);
const paths=['LocalPackages/com.oathx.core/Runtime/XLauncher.cs','Assets/Scripts/PracticePlugin.cs','Assets/Scripts/Systems/RuntimeSystem.cs','Assets/Scripts/Systems/Master/MasterEntity.cs','Assets/Scripts/Systems/Master/MasterSystem.cs'];
const files=await Promise.all(paths.map(async path=>({path,content:await readFile(new URL(path,root),'utf8')})));
const result=await analyzeCode(files);
await writeFile(new URL('master-shared-r1-developer.json',import.meta.url),JSON.stringify({...result,sources:files.map(f=>({path:f.path,sha256:createHash('sha256').update(f.content).digest('hex')})),scope:'Supplemental direct analyzer; verified project scope from preceding verify code. No Unity Editor calls.'},null,2));
console.log(JSON.stringify({issues:result.issues.length}));


