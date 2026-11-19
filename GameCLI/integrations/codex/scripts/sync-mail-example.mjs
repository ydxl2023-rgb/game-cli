// Add the published Mail reference once; never overwrite an edited instance.
import assert from 'node:assert/strict';
import {writeFile} from 'node:fs/promises';
import {createWorkspaces} from '../server/workspaces.mjs';
import {openDatabase} from '../server/database.mjs';
import {instantiateTemplate} from '../shared/templates.mjs';
assert(process.argv.slice(2).every(a=>a==='--apply'));
const projectId='p-2d314388-18c4-42f5-80eb-f6729dd23f0e';
const w=createWorkspaces(),db=await openDatabase();
try{
  const snapshot=await w.snapshot(projectId);
  assert(!snapshot.design.nodes.some(n=>n.parent===null&&n.templateSourceId==='SvMail'),'Mail already present; review its edits before updating.');
  const row=(await db.pool.query("SELECT id,category_id AS category,version,payload,scripts FROM gamecli_ui.templates WHERE id='SvMail'")).rows[0];
  assert(row,'Published Mail missing');
  await writeFile('.data/imports/sv-mail/project-before.json',JSON.stringify(snapshot));
  const root=instantiateTemplate(snapshot.design,row,null,{x:960,y:540});
  snapshot.design.nodes.find(n=>n.id===root).relationPosition={x:1900,y:500};
  if(!process.argv.includes('--apply')){console.log(JSON.stringify({dryRun:true,root,nodes:snapshot.design.nodes.length}));}
  else{
    const saved=await w.save(projectId,{projectId,revision:snapshot.revision,projectVersion:snapshot.project.version,design:snapshot.design});
    const actual=await w.snapshot(projectId);
    assert(actual.design.nodes.some(n=>n.id===root&&n.templateSourceVersion===row.version));
    console.log(JSON.stringify({projectId,revision:saved.revision,root,templateVersion:row.version,nodes:actual.design.nodes.length}));
  }
}finally{await w.close();await db.close();}

