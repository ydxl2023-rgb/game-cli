// Correct the reference catalog to expose two composite integration surfaces.
import assert from 'node:assert/strict';
import {readFile,writeFile} from 'node:fs/promises';
import {createWorkspaces,workspaceActor} from '../server/workspaces.mjs';
import {openDatabase} from '../server/database.mjs';
const w=createWorkspaces(),templates=JSON.parse(await readFile('.data/profile/templates.json','utf8'));
try {
 const fresh=templates.find(t=>t.id==='ProfileShortcuts'),profile=templates.find(t=>t.id==='PlayerProfile');assert(fresh&&profile);
 const old=await w.templateEditor.published('PlayerProfile');assert.equal(old.revision,1,'This migration only applies to PlayerProfile v1');
 const db=await openDatabase(),c=await db.pool.connect();
 try{
  await c.query('BEGIN');assert((await c.query('SELECT actor FROM gamecli_ui.template_admins WHERE actor=$1',[workspaceActor()])).rows.length);
  assert(!(await c.query("SELECT id FROM gamecli_ui.templates WHERE id='ProfileShortcuts'")).rows.length);
  await c.query("SELECT set_config('gamecli.publish','approved',true)");
  await c.query("INSERT INTO gamecli_ui.templates(id,category_id,group_id,name,description,sort,version,payload,scripts) VALUES('ProfileShortcuts','system','profile','Profile Shortcuts','邮件、设置、公告、好友、好感度 · 整组复用',101,1,$1::jsonb,$2::jsonb)",[JSON.stringify(fresh.payload),JSON.stringify(fresh.scripts)]);
  await c.query("INSERT INTO gamecli_ui.template_groups(category_id,id,name,sort) VALUES('system','profile-legacy-parts','旧版 Profile 子项（仅迁移）',990) ON CONFLICT DO NOTHING");
  await c.query("UPDATE gamecli_ui.templates SET version=version+1,updated_at=now(),group_id='profile-legacy-parts',description='旧版拆分记录，仅供迁移。新页面请使用 Profile 或 Profile Shortcuts 组合模板。' WHERE id=ANY($1::text[])",[['AvatarFrame','MailButton','SettingsButton','NoticeButton','FriendsButton','AffinityButton']]);
  await c.query('COMMIT');
 }catch(e){await c.query('ROLLBACK');throw e;}finally{c.release();await db.close();}
 const root=profile.payload.design.nodes.find(n=>n.parent===null);root.scripts=profile.scripts;root.configurations=profile.payload.configurations;
 const opened=await w.templateEditor.open({templateId:'PlayerProfile'}),draft=await w.templateEditor.read(opened.id);assert.deepEqual(draft.design,old.design);
 const saved=await w.templateEditor.save(opened.id,{draftId:opened.id,revision:draft.revision,design:profile.payload.design});
 const candidate=await w.templateEditor.submit(opened.id,{draftId:opened.id,revision:saved.revision});
 const result=await w.publish({candidateId:candidate.id,confirm:true});
 const actual=await w.templateEditor.published('PlayerProfile');assert.deepEqual(actual.design.nodes.find(n=>n.parent===null).scripts,profile.scripts);
 const report={profileVersion:result.version,shortcutsVersion:1,legacyParts:6};await writeFile('.data/profile-group/publication.json',JSON.stringify(report));console.log(JSON.stringify(report));
}finally{await w.close();}
