import assert from 'node:assert/strict';
import {mkdir,writeFile} from 'node:fs/promises';
import {createHash} from 'node:crypto';
import {openDatabase} from '../server/database.mjs';

// Explicitly reviewed catalog. Unknown templates require review, never a guessed category.
const systems=new Set(['SvStart','MasterSystem','SvLogin','SvNotice','SvHome','SvShop','RedDotSystem']);
const prefabs={ItemSlot:'item',ScrollPanel:'scroll',ConfirmDialog:'dialog',CurrencyBar:'currency',AvatarFrame:'profile',PlayerProfile:'profile',ProfileShortcuts:'profile',AffinityButton:'profile-legacy-parts',FriendsButton:'profile-legacy-parts',MailButton:'profile-legacy-parts',NoticeButton:'profile-legacy-parts',SettingsButton:'profile-legacy-parts',InventorySystem:'inventory',SvLoading:'loading'};
const groups=[['currency','货币',40],['profile','头像与玩家资料',50],['loading','加载界面',60],['inventory','背包界面',70],['profile-legacy-parts','旧版资料子控件（仅迁移）',990]];
const hash=value=>createHash('sha256').update(JSON.stringify(value)).digest('hex');
const content=row=>hash([row.payload,row.scripts,row.configurations]);
const apply=process.argv.includes('--apply');
const db=await openDatabase(),c=await db.pool.connect();
try{
 await c.query('BEGIN');
 const rows=(await c.query('SELECT * FROM gamecli_ui.templates ORDER BY id FOR UPDATE')).rows;
 const changes=[];
 for(const row of rows){
  assert(systems.has(row.id)||Object.hasOwn(prefabs,row.id),'Unreviewed template: '+row.id);
  const category=systems.has(row.id)?'system':'prefab',group=systems.has(row.id)?row.group_id:prefabs[row.id];
  if(row.category_id!==category||row.group_id!==group)changes.push({id:row.id,from:row.category_id,to:category,group,version:row.version,nextVersion:row.version+1});
 }
 const projects=()=>c.query('SELECT project_id,version,payload FROM gamecli_ui.project_ui ORDER BY project_id');
 const beforeProjects=hash((await projects()).rows);
 const beforeHistory=(await c.query('SELECT * FROM gamecli_ui.template_versions ORDER BY template_id,version')).rows;
 if(!apply){await c.query('ROLLBACK');console.log(JSON.stringify({dryRun:true,changes},null,2));}
 else{
  const directory=new URL('../.data/template-classification-'+new Date().toISOString().replaceAll(':','-')+'/',import.meta.url);
  await mkdir(directory,{recursive:true});
  await writeFile(new URL('before.json',directory),JSON.stringify(rows,null,2),{flag:'wx'});
  for(const [id,name,sort]of groups)await c.query("INSERT INTO gamecli_ui.template_groups(category_id,id,name,sort) VALUES('prefab',$1,$2,$3) ON CONFLICT(category_id,id) DO NOTHING",[id,name,sort]);
  // Authorized category-only publication; retain the existing guard and archive triggers.
  await c.query("SELECT set_config('gamecli.publish','approved',true)");
  for(const change of changes){
   const result=await c.query('UPDATE gamecli_ui.templates SET category_id=$1,group_id=$2,version=version+1,updated_at=now() WHERE id=$3 AND version=$4 RETURNING *',[change.to,change.group,change.id,change.version]);
   assert.equal(result.rowCount,1);assert.equal(content(result.rows[0]),content(rows.find(r=>r.id===change.id)));
  }
  assert.equal(hash((await projects()).rows),beforeProjects,'Project instances changed');
  const afterHistory=(await c.query('SELECT * FROM gamecli_ui.template_versions ORDER BY template_id,version')).rows;
  const oldKeys=new Set(beforeHistory.map(r=>r.template_id+':'+r.version));
  assert.equal(hash(afterHistory.filter(r=>oldKeys.has(r.template_id+':'+r.version))),hash(beforeHistory),'Historical versions changed');
  assert.equal(afterHistory.length,beforeHistory.length+changes.length);
  await c.query('COMMIT');
  const result={applied:true,changes,contentUnchanged:true,projectsUnchanged:true,historyPreserved:true,frameworkException:'SvStart (XLauncher)',backup:directory.pathname};
  await writeFile(new URL('result.json',directory),JSON.stringify(result,null,2));console.log(JSON.stringify(result,null,2));
 }
}catch(e){await c.query('ROLLBACK');throw e;}finally{c.release();await db.close();}
