// Publish the reviewed v1 icon-initialization correction through the candidate workflow.
import assert from 'node:assert/strict';
import {readFile,writeFile} from 'node:fs/promises';
import {createWorkspaces} from '../server/workspaces.mjs';
const w=createWorkspaces();
try{
 const payload=JSON.parse(await readFile('.data/imports/sv-mail/publication.json','utf8'));
 const scripts=JSON.parse(await readFile('.data/imports/sv-mail/scripts.json','utf8'));
 const published=await w.templateEditor.published('SvMail');assert.equal(published.revision,3);
 const root=payload.design.nodes.find(n=>n.parent===null);root.templateSourceId='SvMail';root.scripts=scripts;root.configurations=payload.configurations;
 const opened=await w.templateEditor.open({templateId:'SvMail'}),draft=await w.templateEditor.read(opened.id);
 assert.deepEqual(draft.design,published.design,'Preserve existing draft edits');
 const saved=await w.templateEditor.save(opened.id,{draftId:opened.id,revision:draft.revision,design:payload.design});
 const candidate=await w.templateEditor.submit(opened.id,{draftId:opened.id,revision:saved.revision});
 const result=await w.publish({candidateId:candidate.id,confirm:true});
 const actual=await w.templateEditor.published('SvMail');assert.deepEqual(actual.design.nodes.find(n=>n.parent===null).scripts,scripts);
 const report={template:'SvMail',version:result.version,verified:true};
 await writeFile('reports/mail-template/publication.json',JSON.stringify(report,null,2));console.log(JSON.stringify(report));
}finally{await w.close();}
