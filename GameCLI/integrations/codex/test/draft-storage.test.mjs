import test from 'node:test';
import assert from 'node:assert/strict';
import {createDraftStorage} from '../ui/draft-storage.mjs';
function fixture(limit=5000){
 const values=new Map(),data=new Map();let sequence=0;
 const storage={getItem:k=>values.get(k)??null,setItem(k,v){const next=new Map(values);next.set(k,v);if([...next.values()].join('').length>limit)throw Error('QuotaExceededError');values.set(k,v);},removeItem:k=>values.delete(k),get length(){return values.size;},key:i=>[...values.keys()][i]};
 const backend={get:async k=>data.get(k),put:async(k,v)=>data.set(k,v),remove:async k=>data.delete(k)};
 return {values,data,storage,backend,drafts:createDraftStorage(storage,backend,()=>String(++sequence))};
}
test('large legacy drafts migrate before archiving without a second full sessionStorage copy',async()=>{
 const {drafts,storage,data}=fixture(),draft={projectVersion:14,design:{scripts:'x'.repeat(4700)}};
 storage.setItem('project.draft',JSON.stringify(draft));
 assert.throws(()=>storage.setItem('project.draft.conflict.14',JSON.stringify(draft)),/Quota/);
 assert.deepEqual(await drafts.read('project.draft'),draft);
 await drafts.archive('project.draft',14);
 assert.equal(storage.getItem('project.draft'),null);assert.ok(storage.getItem('project.draft.conflict.14').length<100);
 assert.deepEqual(await drafts.read('project.draft.conflict.14'),draft);assert.equal(data.size,1);
});
test('new drafts can exceed session quota and later writes cannot resurrect a cleared draft',async()=>{
 const {drafts,storage,data}=fixture(1000),draft={design:{scripts:'x'.repeat(10000)}};
 const writing=drafts.write('draft',draft);draft.design.scripts='changed';
 await writing;assert.equal((await drafts.read('draft')).design.scripts.length,10000);
 const pending=drafts.write('draft',draft),clear=drafts.remove('draft');await Promise.all([pending,clear]);
 assert.equal(storage.getItem('draft'),null);assert.equal(data.size,0);
});
test('unavailable IndexedDB leaves the original legacy draft recoverable',async()=>{
 const {drafts,storage,backend}=fixture(),draft={projectVersion:14,design:{name:'old'}};
 storage.setItem('draft',JSON.stringify(draft));backend.put=async()=>{throw Error('Unavailable');};
 assert.deepEqual(await drafts.read('draft'),draft);await assert.rejects(drafts.archive('draft',14));
 assert.deepEqual(JSON.parse(storage.getItem('draft')),draft);
});
test('same-version conflicts are preserved separately and references survive reload',async()=>{
 const {drafts,storage,backend}=fixture();
 for(const name of ['first','second']){await drafts.write('draft',{projectVersion:14,design:{name}});await drafts.archive('draft',14);}
 const restored=createDraftStorage(storage,backend),keys=restored.conflictKeys('draft');assert.equal(keys.length,2);
 assert.deepEqual(await Promise.all(keys.map(async k=>(await restored.read(k)).design.name)),['first','second']);
});
