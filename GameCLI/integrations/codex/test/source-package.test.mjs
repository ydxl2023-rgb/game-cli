import test from 'node:test';
import assert from 'node:assert/strict';
import {mkdtemp,mkdir,writeFile,rm} from 'node:fs/promises';
import {tmpdir} from 'node:os';
import {join} from 'node:path';
import {createHash} from 'node:crypto';
import {readSourcePackage} from '../server/source-package.mjs';

test('source manifest resolves relative to itself and archives exact source bytes',async()=>{
 const root=await mkdtemp(join(tmpdir(),'gamecli-source-'));
 try{
  await mkdir(join(root,'project'));const manifest=join(root,'launch.json');
  const content='\uFEFF// Original source\r\nclass Launcher {}\r\n';
  await writeFile(join(root,'project','Launcher.cs'),content);
  await writeFile(manifest,JSON.stringify({templateId:'Launch',projectRoot:'project',files:['Launcher.cs']}));
  const result=await readSourcePackage(manifest);
  assert.equal(result.scripts.files[0].content,content);
  assert.equal(result.scripts.files[0].sha256,createHash('sha256').update(content).digest('hex'));
  await writeFile(manifest,JSON.stringify({templateId:'Launch',projectRoot:'project',files:['../secret.cs']}));
  await assert.rejects(readSourcePackage(manifest),/相对路径/);
  await writeFile(manifest,JSON.stringify({templateId:'Launch',projectRoot:'project',files:['Launcher.cs']}));
  await writeFile(join(root,'project','Launcher.cs'),Buffer.from([0xff,0xfe]));
  await assert.rejects(readSourcePackage(manifest),/UTF-8/);
 }finally{await rm(root,{recursive:true,force:true});}
});
