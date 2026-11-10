import test from 'node:test';
import assert from 'node:assert/strict';
import {saveProject,projectSnapshot} from '../server/save.mjs';
import {writeProjectUi} from '../server/project-ui.mjs';
import {starter,validateDesign} from '../shared/design.mjs';
test('project save ignores legacy template targets and never calls publish',async()=>{
 const snapshot={revision:2,design:starter(),savedAt:'now'};let saved=false;
 const db={publish(){assert.fail('Save must never publish templates');},async saveProject(s,v){assert.equal(s,snapshot);assert.equal(v,0);saved=true;return {id:'GameCLI',version:1};}};
 const result=await saveProject({save:async()=>snapshot},db,{design:snapshot.design,revision:1,projectVersion:0,targets:[{templateId:'SvLoading'}]});
 assert.ok(saved);assert.equal(result.projectSave.version,1);
});
test('project writes use only project_ui and enforce optimistic version',async()=>{
 const snapshot={revision:2,design:starter(),savedAt:'now'};const queries=[];
 const pool={async query(sql,args){queries.push({sql,args});return {rows:[{version:2}]};}};
 await writeProjectUi(pool,snapshot,0);await writeProjectUi(pool,snapshot,1);
 assert.match(queries[0].sql,/INSERT INTO gamecli_ui.project_ui/);assert.match(queries[1].sql,/AND version=\$4/);assert.equal(queries[1].args[3],1);
 assert.deepEqual(JSON.parse(queries[0].args[2]).design,validateDesign(snapshot.design));
 await assert.rejects(()=>writeProjectUi({query:async()=>({rows:[]})},snapshot,1),/其他窗口/);
});
test('empty local workspace restores the project; an existing draft wins',async()=>{
 const dbDesign=starter();dbDesign.name='DatabaseUi';const localDesign=starter();
 const db={project:async()=>({id:'GameCLI',name:'GameCLI',version:4,design:dbDesign})};
 assert.equal((await projectSnapshot({snapshot:async()=>({revision:0,design:localDesign})},db)).design.name,'DatabaseUi');
 assert.equal((await projectSnapshot({snapshot:async()=>({revision:1,design:localDesign})},db)).design.name,localDesign.name);
});
