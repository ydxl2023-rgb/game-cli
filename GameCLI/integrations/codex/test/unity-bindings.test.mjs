import test from 'node:test';
import assert from 'node:assert/strict';
import {mkdtemp,mkdir,writeFile,readFile,rm,realpath} from 'node:fs/promises';
import {tmpdir} from 'node:os';
import {join,relative} from 'node:path';
import {randomUUID} from 'node:crypto';
import {openDatabase} from '../server/database.mjs';
import {createUnityBindings} from '../server/unity-bindings.mjs';

test('Unity binding identity, owner access, relocation and mismatch protection',{skip:process.env.GAMECLI_TEST_DATABASE!=='1'},async()=>{
 const db=await openDatabase(),schema='binding_test_'+randomUUID().replaceAll('-',''),root=await mkdtemp(join(tmpdir(),'gamecli-binding-'));
 const replace=s=>s.replaceAll('gamecli_ui',schema),pool={query:(s,a)=>db.pool.query(replace(s),a),async connect(){const c=await db.pool.connect();return {query:(s,a)=>c.query(replace(s),a),release:()=>c.release()};}};
 const access=async id=>{if(!['A','B'].includes(id))throw Error('无权访问');return {id,role:'owner'};};
 const binding=createUnityBindings({connection:async()=>({pool}),access,hostId:'test-host'});
 async function project(name){const path=join(root,name);for(const d of ['Assets','Packages','ProjectSettings'])await mkdir(join(path,d),{recursive:true});await writeFile(join(path,'ProjectSettings/ProjectVersion.txt'),'m_EditorVersion: 2022.3');return path;}
 try{
  await db.pool.query(`CREATE SCHEMA ${schema}`);await pool.query('CREATE TABLE gamecli_ui.projects(id text PRIMARY KEY);INSERT INTO gamecli_ui.projects VALUES(\'A\'),(\'B\')');
  const path=await project('client');
  assert.equal((await binding.read('A')).status,'unbound');await assert.rejects(binding.requireBinding('A'),/未绑定/);
  await assert.rejects(binding.bind('A',{projectId:'B',path}),/不一致/);
  const editor=createUnityBindings({connection:async()=>({pool}),access:async()=>({role:'editor'})});await assert.rejects(editor.bind('A',{projectId:'A',path}),/所有者/);
  await assert.rejects(binding.bind('A',{projectId:'A',path:root}));
  const first=await binding.bind('A',{projectId:'A',path});assert.equal(first.status,'verified');
  assert.equal((await binding.bind('A',{projectId:'A',path})).unityProjectId,first.unityProjectId);
  await assert.rejects(binding.bind('B',{projectId:'B',path}),/其他项目/);
  const file=join(path,'ProjectSettings/GameCLIProject.json'),original=await readFile(file,'utf8');await writeFile(file,original.replace('"A"','"B"'));
  assert.equal((await binding.read('A')).status,'mismatch');await assert.rejects(binding.requireBinding('A'),/不一致/);await writeFile(file,original);
  const relocated=await project('moved');await writeFile(join(relocated,'ProjectSettings/GameCLIProject.json'),original);
  await assert.rejects(binding.bind('A',{projectId:'A',path:relocated}),/确认/);
  const moved=await binding.bind('A',{projectId:'A',path:relocated,confirm:true});assert.equal(moved.unityProjectId,first.unityProjectId);assert.equal(moved.path,await realpath(relocated));
  const otherHost=createUnityBindings({connection:async()=>({pool}),access,hostId:'other-host'});assert.equal((await otherHost.read('A')).status,'unavailable');
  await rm(join(relocated,'ProjectSettings/GameCLIProject.json'));await assert.rejects(binding.requireBinding('A'),/不一致/);
 }finally{await db.pool.query(`DROP SCHEMA ${schema} CASCADE`);await db.close();assert.ok(!relative(tmpdir(),root).startsWith('..'));await rm(root,{recursive:true,force:true});}
});
