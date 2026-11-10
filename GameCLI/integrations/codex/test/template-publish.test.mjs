import test from 'node:test';
import assert from 'node:assert/strict';
import {mkdtemp,rm} from 'node:fs/promises';
import {tmpdir} from 'node:os';
import {join} from 'node:path';
import {builtInTemplates} from '../shared/templates.mjs';
import {templateFromDraft,publishTemplates} from '../server/template-publish.mjs';
import {saveProject} from '../server/save.mjs';
const t=builtInTemplates()[0];
const current={id:t.id,name:t.name,category_id:t.category,version:3,payload:{...t.payload,source:{note:'preserve'}}};
const snapshot={revision:7,savedAt:'now',design:structuredClone(t.payload.design)};
const target={templateId:t.id,rootId:t.payload.root,version:3};
test('publish extracts one linked subtree and retains metadata',()=>{
 const s=structuredClone(snapshot);s.design.nodes.push({...s.design.nodes[0],id:'Other'});
 const p=templateFromDraft(s,target,current);assert.equal(p.design.nodes.length,t.payload.design.nodes.length);assert.deepEqual(p.source,current.payload.source);
 assert.equal(p.design.nodes[0].templateSourceId,t.id);
 s.design.nodes[1].action={type:'show',target:'Other'};assert.throws(()=>templateFromDraft(s,target,current),/跨系统/);
});
test('database failure returns saved local revision for retry',async()=>{
 const result=await saveProject({save:async()=>snapshot},{saveProject:async()=>{throw new Error('offline');}},{design:snapshot.design,revision:6,targets:[target]});
 assert.equal(result.revision,7);assert.equal(result.projectSave.ok,false);assert.equal(result.projectSave.error,'offline');
});
test('publish transaction commits version update, rejects stale and duplicate targets',async()=>{
 const dir=await mkdtemp(join(tmpdir(),'studio-sync-'));const queries=[];
 const client={release(){},async query(sql,args){queries.push(sql);if(sql.startsWith('SELECT *'))return {rows:[current]};if(sql.startsWith('UPDATE')){assert.equal(JSON.parse(args[0]).editedFrom.draftRevision,7);return {rows:[{version:4}]};}return {rows:[]};}};
 const pool={connect:async()=>client};
 try{
  assert.equal((await publishTemplates(pool,snapshot,[target],dir))[0].version,4);assert.equal(queries.at(-1),'COMMIT');
  await assert.rejects(()=>publishTemplates(pool,snapshot,[{...target,version:2}],dir),/已更新/);assert.equal(queries.at(-1),'ROLLBACK');
  await assert.rejects(()=>publishTemplates(pool,snapshot,[target,target],dir),/多个实例/);
 }finally{await rm(dir,{recursive:true,force:true});}
});
