import assert from 'node:assert/strict';
import {readFile,writeFile} from 'node:fs/promises';
import {createWorkspaces} from '../server/workspaces.mjs';
import {validateScripts} from '../server/template-scripts.mjs';
const w=createWorkspaces();
try {
 const published=await w.templateEditor.published('CurrencyBar');
 assert.equal(published.revision,1,'One-time v1 path migration; do not republish later versions');
 const design=structuredClone(published.design),root=design.nodes.find(n=>n.parent===null);
 const moves=JSON.parse(await readFile('.data/currency-path-migration.json','utf8'));
 const binding=await w.unityBindings.requireBinding('p-2d314388-18c4-42f5-80eb-f6729dd23f0e');
 const files=[];
 for(const file of root.scripts.files){
  if(file.path.startsWith('Assets/Scripts/Systems/Currency/')){
   const old=file.path.replace(/\.meta$/,'');
   if(!moves[old]){assert(file.path.endsWith('.meta'));continue;}
   const path=moves[old]+(file.path.endsWith('.meta')?'.meta':'');
   const content=await readFile(binding.path+'/'+path,'utf8');assert.equal(content,file.content);
   files.push({path,content});
  }else files.push(file);
 }
 root.scripts=validateScripts({schemaVersion:1,files});
 const readme=root.scripts.files.find(f=>f.path.endsWith('/README.md'));
 const suffix='\n\nCode location: Systems/Common/UI for UICurrencyBar and UICurrencyEntry; Systems/Common/UI/VM for VMCurrencyBar and VMCurrencyEntry. When upgrading v1, move existing scripts and their .meta files rather than keeping duplicate classes. GUIDs are unchanged.';
 root.scripts=validateScripts({schemaVersion:1,files:root.scripts.files.map(f=>f===readme?{path:f.path,content:f.content+suffix}:f)});
 const opened=await w.templateEditor.open({templateId:'CurrencyBar'}),draft=await w.templateEditor.read(opened.id);
 assert.deepEqual(draft.design,published.design);
 const saved=await w.templateEditor.save(opened.id,{draftId:opened.id,revision:draft.revision,design});
 const candidate=await w.templateEditor.submit(opened.id,{draftId:opened.id,revision:saved.revision});
 const result=await w.publish({candidateId:candidate.id,confirm:true});
 const actual=await w.templateEditor.published('CurrencyBar');assert.deepEqual(actual.design.nodes.find(n=>n.parent===null).scripts,root.scripts);
 await writeFile('.data/currency-template/path-publication.json',JSON.stringify(result));console.log(JSON.stringify({version:result.version,files:files.length}));
}finally{await w.close();}
