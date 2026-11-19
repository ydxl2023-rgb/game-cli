import assert from 'node:assert/strict';
import {readFile,writeFile,mkdir} from 'node:fs/promises';
import {createHash} from 'node:crypto';
import {createWorkspaces} from '../server/workspaces.mjs';
import {openDatabase} from '../server/database.mjs';
import {validateScripts} from '../server/template-scripts.mjs';
const w=createWorkspaces(),db=await openDatabase(),projectId='p-2d314388-18c4-42f5-80eb-f6729dd23f0e';
try{
 const {path:project}=await w.unityBindings.requireBinding(projectId);
 const migration=JSON.parse(await readFile('reports/micros-flat/migration.json','utf8'));
 const row=(await db.pool.query("SELECT * FROM gamecli_ui.templates WHERE id='SvShop'")).rows[0];assert.equal(row.version,6);
 await mkdir('.data/micros-flat',{recursive:true});await writeFile('.data/micros-flat/before.json',JSON.stringify(row));
 const rewrite=s=>{for(const [a,b]of Object.entries(migration.moves))s=s.replaceAll(a,b);return s;};
 const payload=JSON.parse(rewrite(JSON.stringify(row.payload)));
 const files=row.scripts.files.map(f=>({path:f.path,content:/(?:migration|resource-moves)\.json$/.test(f.path)?f.content:rewrite(f.content)}));
 const mf=files.find(f=>f.path.endsWith('/materials.json')),m=JSON.parse(mf.content);
 m.files=m.files.filter(f=>!migration.removedEmptyFolderMeta.includes(f.path));
 for(const f of m.files)if(f.content)f.sha256=createHash('sha256').update(f.content).digest('hex');
 mf.content=JSON.stringify(m);
 const note='A single reusable prefab without companion assets is stored directly in Widgets/Micros. Product cards are Micros/ShopGiftItem.prefab and Micros/ShopRechargeItem.prefab. Use a module subdirectory only for multiple prefabs, Parts or companion resources.';
 payload.configurations.integration.push(note);files.find(f=>f.path.endsWith('/README.md')).content+='\n\n'+note;
 files.push({path:'GameCLI/Templates/Shop/micros-flat-migration.json',content:JSON.stringify(migration,null,2)});
 const scripts=validateScripts({schemaVersion:1,files});
 const snapshot=await w.snapshot(projectId);await writeFile('.data/micros-flat/project-before.json',JSON.stringify(snapshot));
 const instance=snapshot.design.nodes.find(n=>n.parent===null&&n.templateSourceId==='SvShop');assert.equal(instance.templateSourceVersion,6);assert.deepEqual(instance.scripts,row.scripts);
 const published=await w.templateEditor.published('SvShop');assert.equal(published.revision,6);
 const root=payload.design.nodes.find(n=>n.parent===null);root.templateSourceId='SvShop';root.scripts=scripts;root.configurations=payload.configurations;
 const opened=await w.templateEditor.open({templateId:'SvShop'}),draft=await w.templateEditor.read(opened.id);assert.deepEqual(draft.design,published.design);
 const saved=await w.templateEditor.save(opened.id,{draftId:opened.id,revision:draft.revision,design:payload.design});
 const candidate=await w.templateEditor.submit(opened.id,{draftId:opened.id,revision:saved.revision});const result=await w.publish({candidateId:candidate.id,confirm:true});assert.equal(result.version,7);
 instance.scripts=scripts;instance.configurations=payload.configurations;instance.templateSourceVersion=7;
 const updated=await w.save(projectId,{projectId,revision:snapshot.revision,projectVersion:snapshot.project.version,design:snapshot.design});
 const actual=(await db.pool.query("SELECT * FROM gamecli_ui.templates WHERE id='SvShop'")).rows[0];assert.deepEqual(actual.scripts,scripts);
 const readback=await w.snapshot(projectId);assert.deepEqual(readback.design.nodes.find(n=>n.id===instance.id).scripts,scripts);
 for(const [old,path]of Object.entries(migration.moves))for(const suffix of ['', '.meta']){
  const f=m.files.find(f=>f.path===path+suffix);assert(f);assert.equal(createHash('sha256').update(await readFile(project+'/'+path+suffix)).digest('hex'),f.sha256);
  assert(!m.files.some(f=>f.path===old+suffix));
 }
 await writeFile('.data/micros-flat/final.json',JSON.stringify(actual));
 const report={verified:true,template:'SvShop',version:7,projectRevision:updated.revision,prefabs:Object.values(migration.moves),emptyFolderEntriesRemoved:true};
 await writeFile('reports/micros-flat/readback.json',JSON.stringify(report,null,2));console.log(JSON.stringify(report));
}finally{await w.close();await db.close();}
