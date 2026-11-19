// Refresh only the just-created, unedited reference instance at the reviewed revision.
import assert from 'node:assert/strict';
import {writeFile} from 'node:fs/promises';
import {createWorkspaces} from '../server/workspaces.mjs';
import {openDatabase} from '../server/database.mjs';
import {instantiateTemplate} from '../shared/templates.mjs';
const projectId='p-2d314388-18c4-42f5-80eb-f6729dd23f0e',w=createWorkspaces(),db=await openDatabase();
try{
 const snapshot=await w.snapshot(projectId);assert.equal(snapshot.revision,59,'Preserve newer product edits');
 const old=snapshot.design.nodes.find(n=>n.templateSourceId==='SvMail');assert.equal(old.templateSourceVersion,2);
 const ids=new Set([old.id]);let changed=true;
 while(changed){changed=false;for(const n of snapshot.design.nodes)if(ids.has(n.parent)&&!ids.has(n.id)){ids.add(n.id);changed=true;}}
 await writeFile('.data/imports/sv-mail/project-refresh-before.json',JSON.stringify(snapshot));
 snapshot.design.nodes=snapshot.design.nodes.filter(n=>!ids.has(n.id));
 snapshot.design.animations=snapshot.design.animations.filter(a=>!ids.has(a.hostNodeId));
 const row=(await db.pool.query("SELECT id,category_id AS category,version,payload,scripts FROM gamecli_ui.templates WHERE id='SvMail'")).rows[0];
 assert.equal(row.version,4);
 const root=instantiateTemplate(snapshot.design,row,null,{x:960,y:540});assert.equal(root,old.id);
 snapshot.design.nodes.find(n=>n.id===root).relationPosition=old.relationPosition;
 const saved=await w.save(projectId,{projectId,revision:snapshot.revision,projectVersion:snapshot.project.version,design:snapshot.design});
 const actual=await w.snapshot(projectId);assert.deepEqual(actual.design.nodes.find(n=>n.id===root).scripts,row.scripts);
 const report={projectId,revision:saved.revision,root,version:row.version};
 await writeFile('reports/mail-template/project-sync.json',JSON.stringify(report,null,2));console.log(JSON.stringify(report));
}finally{await w.close();await db.close();}
