import test from 'node:test';
import assert from 'node:assert/strict';
import {mkdtemp,mkdir,writeFile,rm} from 'node:fs/promises';
import {tmpdir} from 'node:os';
import {join} from 'node:path';
import {createHash} from 'node:crypto';
import {inspectIntegration} from '../server/verify-integration.mjs';
test('integration inventory distinguishes missing, identical and adapted files and rejects traversal',async()=>{
 const root=await mkdtemp(join(tmpdir(),'integration-test-'));
 try{await mkdir(join(root,'Assets'));await writeFile(join(root,'Assets/A.cs'),'actual');
 const sha256=createHash('sha256').update('actual').digest('hex');
 const result=await inspectIntegration(root,[{path:'Assets/A.cs',sha256},{path:'Assets/A.cs',sha256:'different'},{path:'Assets/B.cs',sha256}]);
 assert.deepEqual(result.map(i=>i.status),['present','adapted-review-required','missing']);
 const mapped=await inspectIntegration(root,[{path:'Assets/Old.cs',sha256}],{'Assets/Old.cs':'Assets/A.cs'});
 assert.equal(mapped[0].status,'adapted-review-required');assert.equal(mapped[0].targetPath,'Assets/A.cs');
 await assert.rejects(inspectIntegration(root,[{path:'Assets/Old.cs',sha256}],{'Assets/Old.cs':'Assets/../../outside'}),/Unsafe/);
 await assert.rejects(inspectIntegration(root,[{path:'Assets/../../outside',sha256}]),/Unsafe/);
 }finally{await rm(root,{recursive:true,force:true});}
});
