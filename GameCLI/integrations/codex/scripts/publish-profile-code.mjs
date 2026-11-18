import assert from 'node:assert/strict';
import {readFile,writeFile} from 'node:fs/promises';
import {createWorkspaces} from '../server/workspaces.mjs';
const w=createWorkspaces();
try {
 const templates=JSON.parse(await readFile('.data/profile/templates.json','utf8')),versions={PlayerProfile:3,ProfileShortcuts:2},results=[];
 for(const t of templates){
  const published=await w.templateEditor.published(t.id);assert.equal(published.revision,versions[t.id],'This candidate migrates the reviewed composite baseline');
  const root=t.payload.design.nodes.find(n=>n.parent===null);root.templateSourceId=t.id;root.scripts=t.scripts;root.configurations=t.payload.configurations;
  const opened=await w.templateEditor.open({templateId:t.id}),draft=await w.templateEditor.read(opened.id);assert.deepEqual(draft.design,published.design,'Preserve edited drafts');
  const saved=await w.templateEditor.save(opened.id,{draftId:opened.id,revision:draft.revision,design:t.payload.design});
  const candidate=await w.templateEditor.submit(opened.id,{draftId:opened.id,revision:saved.revision});
  const result=await w.publish({candidateId:candidate.id,confirm:true});
  const actual=await w.templateEditor.published(t.id);assert.deepEqual(actual.design.nodes.find(n=>n.parent===null).scripts,t.scripts);
  results.push({id:t.id,version:result.version});
 }
 await writeFile('.data/profile-path/publication.json',JSON.stringify(results,null,2));console.log(JSON.stringify(results));
}finally{await w.close();}
