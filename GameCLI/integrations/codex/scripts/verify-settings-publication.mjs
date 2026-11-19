import assert from 'node:assert/strict';
import {readFile,writeFile} from 'node:fs/promises';
import {join} from 'node:path';
import {createHash} from 'node:crypto';
import {openDatabase} from '../server/database.mjs';
import {createWorkspaces} from '../server/workspaces.mjs';
const hash=v=>createHash('sha256').update(v).digest('hex');
const expectedVersion=Number(process.argv[2]||1);
const db=await openDatabase(),w=createWorkspaces();
try{
 const projectId='p-2d314388-18c4-42f5-80eb-f6729dd23f0e';
 const binding=await w.unityBindings.requireBinding(projectId);
 const row=(await db.pool.query("SELECT version,payload,scripts FROM gamecli_ui.templates WHERE id='SvSettings'")).rows[0];
 assert.equal(row.version,expectedVersion);
 for(const file of row.scripts.files){
  assert.equal(hash(file.content),file.sha256);
  if(file.path.startsWith('Assets/'))assert.equal(hash(await readFile(join(binding.path,file.path))),file.sha256);
 }
 const material=JSON.parse(row.scripts.files.find(f=>f.path.endsWith('/materials.json')).content);
 for(const file of material.files){
  const bytes=file.assetId?(await db.pool.query('SELECT body FROM gamecli_ui.assets WHERE id=$1',[file.assetId])).rows[0].body:Buffer.from(file.content);
  assert.equal(hash(bytes),file.sha256);
  if(file.path.startsWith('Assets/'))assert.equal(hash(await readFile(join(binding.path,file.path))),file.sha256);
 }
 const table=row.payload.configurations.tables[0];assert.equal(table.entries[0].ClassType,'SettingsSystem');
 const workbook=(await db.pool.query('SELECT body FROM gamecli_ui.configuration_assets WHERE id=$1',[table.source.assetId])).rows[0].body;
 assert.equal(hash(workbook),hash(await readFile(join(binding.path,'../config/propFunctionSystem.xlsx'))));
 const snapshot=await w.snapshot(projectId);
 const root=snapshot.design.nodes.find(n=>n.parent===null&&n.templateSourceId==='SvSettings');
 assert(root);assert.equal(root.templateSourceVersion,row.version);assert.deepEqual(root.scripts,row.scripts);
 assert(snapshot.design.nodes.some(n=>n.name==='Settings'&&n.type==='Button'&&n.action?.target===root.id));
 const result={template:'SvSettings',version:row.version,scriptFiles:row.scripts.files.length,materials:material.files.length,configRows:table.entries.length,root:root.id,projectRevision:snapshot.revision,hashesVerified:true};
 await writeFile('reports/settings-template/'+(expectedVersion===1?'publication-readback':'v'+expectedVersion+'-publication-readback')+'.json',JSON.stringify(result,null,2));console.log(JSON.stringify(result));
}finally{await db.close();await w.close();}
