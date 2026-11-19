// Package the shared reward and the Mail reference that consumes it. Dry-run by default.
import assert from 'node:assert/strict';
import {readFile,writeFile,readdir,mkdir} from 'node:fs/promises';
import {join} from 'node:path';
import {createHash} from 'node:crypto';
import {isDeepStrictEqual} from 'node:util';
import {openDatabase} from '../server/database.mjs';
import {createWorkspaces,workspaceActor} from '../server/workspaces.mjs';
import {validateTemplate} from '../shared/templates.mjs';
import {validateScripts} from '../server/template-scripts.mjs';
assert(process.argv.slice(2).every(a=>a==='--apply'));
const apply=process.argv.includes('--apply'),out='.data/reward-entry';
await mkdir(out,{recursive:true});
const w=createWorkspaces(),db=await openDatabase(),hash=b=>createHash('sha256').update(b).digest('hex');
const json=async p=>JSON.parse((await readFile(p,'utf8')).replace(/^\uFEFF/,''));
const moves=await json('reports/reward-entry/resource-moves.json');
const projectId='p-2d314388-18c4-42f5-80eb-f6729dd23f0e';
try{
 const {path:project}=await w.unityBindings.requireBinding(projectId);
 const mail=(await db.pool.query("SELECT * FROM gamecli_ui.templates WHERE id='SvMail'")).rows[0];assert.equal(mail.version,4,'Review newer Mail before packaging');
 const assets=new Map();
 async function walk(dir){const result=[];for(const e of await readdir(dir,{withFileTypes:true})){const p=join(dir,e.name);result.push(...e.isDirectory()?await walk(p):[p]);}return result;}
 async function materials(paths){const result=[];for(const absolute of paths){const path=absolute.slice(project.length+1).replaceAll('\\','/'),body=await readFile(absolute),id=hash(body);
  if(/\.(png|ttc|ttf|otf)$/.test(path)){const mime=path.endsWith('.png')?'image/png':'application/octet-stream';assets.set(id,{path,body,mime});result.push({path,assetId:id,sha256:id,mime});}
  else result.push({path,encoding:'utf8',content:body.toString('utf8'),sha256:id});
 }return result;}
 const sharedFonts=[];for(const font of ['msyh.ttc','msyhbd.ttc'])for(const suffix of ['','.meta'])sharedFonts.push(join(project,'Assets/Data/Widgets/Common/Resources',font+suffix));
 const rewardMaterials=await materials([join(project,'Assets/Data/Widgets/Reward.meta'),...await walk(join(project,'Assets/Data/Widgets/Reward')),...sharedFonts]);
 const files=[];
 for(const name of ['UIRewardEntry.cs','VM/VMRewardEntry.cs'])for(const suffix of ['','.meta']){const path='Assets/Scripts/Systems/Common/UI/'+name+suffix;files.push({path,content:await readFile(join(project,path),'utf8')});}
 const codeMoves={
  'Assets/Scripts/Systems/Mail/UI/UIMailReward.cs':'Assets/Scripts/Systems/Common/UI/UIRewardEntry.cs',
  'Assets/Scripts/Systems/Mail/VM/VMMailReward.cs':'Assets/Scripts/Systems/Common/UI/VM/VMRewardEntry.cs'
 };
 const migration={schemaVersion:1,assets:moves,scripts:codeMoves,instruction:'Move existing files and their meta through UnityCLI before importing the renamed sources. Do not retain old classes or duplicate GUIDs.'};
 const integration=[
  'Reusable UI only: UIRewardEntry/VMRewardEntry have no mail, inventory, persistence or network dependency.',
  'Drag Assets/Data/Widgets/Reward/RewardEntry.prefab into a page. Bind through SyncMetadata or Ref; use VList<VMRewardEntry> with the existing Fancy list for repeated items.',
  'Four bindings in order: Icon (Sprite), Count (formatted string), Claimed (bool), IconVisible (bool). The host decides values and owns claims.',
  'Explicitly initialize IconVisible=false while loading, then assign Icon and IconVisible=true. Reject stale asynchronous callbacks in the host. Do not clear SyncSprite by assigning null.',
  'For static instances bind once, update existing VM properties and Unref before destruction. Fancy cell recycling follows the existing framework contract; its cache limitation is not fixed by extraction.',
  'Restore materials and adjacent meta files, merging shared Common fonts by hash/GUID. Resource migration preserves GUIDs and removes Mail-private asset dependencies.',
  'The native root contains a standard FancyGridScrollViewCell but standalone rendering does not require a list. Root cell size is 152x152; placement belongs to the host.'
 ];
 files.push({path:'GameCLI/Templates/Reward/materials.json',content:JSON.stringify({schemaVersion:1,files:rewardMaterials})});
 files.push({path:'GameCLI/Templates/Reward/migration.json',content:JSON.stringify(migration,null,2)});
 files.push({path:'GameCLI/Templates/Reward/README.md',content:integration.join('\n\n')});
 const source=mail.payload.design,root=source.nodes.find(n=>n.id==='Mail0_3_0_2_1');assert(root);
 const ids=new Set([root.id]);let count;do{count=ids.size;for(const n of source.nodes)if(ids.has(n.parent))ids.add(n.id);}while(count!==ids.size);
 const nodes=structuredClone(source.nodes.filter(n=>ids.has(n.id))),r=nodes.find(n=>n.id===root.id);
 Object.assign(r,{parent:null,name:'RewardEntry',x:0,y:0,width:152,height:152});
 r.rectTransform={anchorMin:{x:.5,y:.5},anchorMax:{x:.5,y:.5},pivot:{x:.5,y:.5},anchoredPosition:{x:0,y:0},sizeDelta:{x:152,y:152}};
 const rewardPayload=validateTemplate({root:r.id,design:{schemaVersion:1,name:'RewardEntry',width:240,height:240,nodes,animations:structuredClone((source.animations??[]).filter(a=>ids.has(a.hostNodeId)))},configurations:{schemaVersion:1,tables:[],dependencies:[{name:'com.oathx.widget',kind:'framework',required:true,reason:'UIWidget, VM, Metadata and Sync actions'},{name:'com.oathx.widget.extension',kind:'framework',required:true,reason:'Existing Fancy cell/list integration'}],integration}},'prefab');
 const rewardScripts=validateScripts({schemaVersion:1,files});
 const mailPayload=structuredClone(mail.payload);
 mailPayload.configurations.dependencies.push({name:'RewardEntry',kind:'template',templateId:'RewardEntry',version:1,required:true,reason:'Shared UIRewardEntry/VMRewardEntry, nested prefab and reward artwork; install before Mail.'});
 mailPayload.configurations.integration.push('Mail uses VList<VMRewardEntry> and a nested RewardEntry.prefab. Install RewardEntry v1 first; migrate old UIMailReward/VMMailReward scripts with their meta GUIDs using its migration.json. Do not keep duplicate old class files.');
 const mailMaterials=await materials([join(project,'Assets/Data/Widgets/Mail.meta'),...await walk(join(project,'Assets/Data/Widgets/Mail')),...sharedFonts]);
 const oldMaterials=JSON.parse(mail.scripts.files.find(f=>f.path==='GameCLI/Templates/Mail/materials.json').content);
 mailMaterials.push(...oldMaterials.files.filter(f=>f.path.startsWith('GameCLI/Templates/Mail/Source/')));
 const mailFiles=mail.scripts.files.filter(f=>!f.path.startsWith('Assets/Scripts/Systems/Mail/')&&!['GameCLI/Templates/Mail/materials.json','GameCLI/Templates/Mail/bindings.json'].includes(f.path)).map(f=>({path:f.path,content:f.content}));
 for(const path of await walk(join(project,'Assets/Scripts/Systems/Mail')))mailFiles.push({path:path.slice(project.length+1).replaceAll('\\','/'),content:await readFile(path,'utf8')});
 mailFiles.push({path:'GameCLI/Templates/Mail/materials.json',content:JSON.stringify({schemaVersion:1,dependencies:[{templateId:'RewardEntry',version:1}],files:mailMaterials})});
 mailFiles.push({path:'GameCLI/Templates/Mail/bindings.json',content:await readFile('.data/imports/sv-mail/bindings.json','utf8')});
 mailFiles.push({path:'GameCLI/Templates/Mail/reward-migration.json',content:JSON.stringify(migration,null,2)});
 const readme=mailFiles.find(f=>f.path==='GameCLI/Templates/Mail/README.md');readme.content+='\n\nShared reward: install RewardEntry v1 first. UIRewardEntry and VMRewardEntry live in Common/UI, not Mail. Mail owns claim rules and asynchronous icon requests. The nested reward cell and source-template visibility references are restored in Mail.prefab. See reward-migration.json for old-to-new paths.';
 const mailScripts=validateScripts({schemaVersion:1,files:mailFiles});
 await writeFile(out+'/reward.json',JSON.stringify({payload:rewardPayload,scripts:rewardScripts},null,2));
 await writeFile(out+'/mail.json',JSON.stringify({payload:mailPayload,scripts:mailScripts},null,2));
 const summary={rewardNodes:nodes.length,rewardFiles:files.length,rewardMaterials:rewardMaterials.length,mailFiles:mailFiles.length,mailMaterials:mailMaterials.length,assets:assets.size};
 if(!apply){console.log(JSON.stringify({...summary,dryRun:true}));}
 else{
  const c=await db.pool.connect();
  try{
   await c.query('BEGIN');assert((await c.query('SELECT actor FROM gamecli_ui.template_admins WHERE actor=$1',[workspaceActor()])).rows.length);
   assert(!(await c.query("SELECT id FROM gamecli_ui.templates WHERE id='RewardEntry'")).rows.length,'Existing RewardEntry requires candidate update');
   for(const[id,a]of assets)await c.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT DO NOTHING',[id,a.mime,a.body,a.path]);
   for(const n of rewardPayload.design.nodes)for(const id of[n.image,n.fontAsset].filter(Boolean))assert((await c.query('SELECT id FROM gamecli_ui.assets WHERE id=$1',[id])).rows.length,'Missing browser asset '+id);
   await c.query("INSERT INTO gamecli_ui.template_groups(category_id,id,name,sort) VALUES('prefab','reward','奖励展示',55) ON CONFLICT DO NOTHING");
   await c.query("SELECT set_config('gamecli.publish','approved',true)");
   await c.query("INSERT INTO gamecli_ui.templates(id,category_id,group_id,name,description,sort,version,payload,scripts) VALUES('RewardEntry','prefab','reward','Reward Entry','公共奖励展示 · 图标、数量、领取标记 · 独立或列表复用',105,1,$1::jsonb,$2::jsonb)",[JSON.stringify(rewardPayload),JSON.stringify(rewardScripts)]);
   const actual=(await c.query("SELECT payload,scripts FROM gamecli_ui.templates WHERE id='RewardEntry'")).rows[0];assert(isDeepStrictEqual(actual,{payload:rewardPayload,scripts:rewardScripts}));
   await c.query('COMMIT');
  }catch(e){await c.query('ROLLBACK');throw e;}finally{c.release();}
  const published=await w.templateEditor.published('SvMail');assert.equal(published.revision,4);
  const mailRoot=mailPayload.design.nodes.find(n=>n.parent===null);mailRoot.templateSourceId='SvMail';mailRoot.scripts=mailScripts;mailRoot.configurations=mailPayload.configurations;
  const opened=await w.templateEditor.open({templateId:'SvMail'}),draft=await w.templateEditor.read(opened.id);assert.deepEqual(draft.design,published.design,'Preserve existing draft');
  const saved=await w.templateEditor.save(opened.id,{draftId:opened.id,revision:draft.revision,design:mailPayload.design});
  const candidate=await w.templateEditor.submit(opened.id,{draftId:opened.id,revision:saved.revision});const result=await w.publish({candidateId:candidate.id,confirm:true});
  const current=await w.snapshot(projectId);await writeFile(out+'/project-before.json',JSON.stringify(current));
  const instance=current.design.nodes.find(n=>n.parent===null&&n.templateSourceId==='SvMail');assert.equal(instance.templateSourceVersion,4);
  assert.deepEqual(instance.scripts,mail.scripts,'Preserve edited Mail instance code');
  instance.scripts=mailScripts;instance.configurations=mailPayload.configurations;instance.templateSourceVersion=result.version;
  const projectSaved=await w.save(projectId,{projectId,revision:current.revision,projectVersion:current.project.version,design:current.design});
  const verified=(await db.pool.query("SELECT scripts FROM gamecli_ui.templates WHERE id='SvMail'")).rows[0];assert.deepEqual(verified.scripts,mailScripts);
  const report={...summary,rewardVersion:1,mailVersion:result.version,projectRevision:projectSaved.revision,verified:true};
  await writeFile('reports/reward-entry/publication.json',JSON.stringify(report,null,2));console.log(JSON.stringify(report));
 }
}finally{await w.close();await db.close();}
