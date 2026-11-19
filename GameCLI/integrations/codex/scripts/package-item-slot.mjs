// Publish the reviewed ItemSlot -> RewardEntry -> Mail composition through candidates.
import assert from 'node:assert/strict';
import {readFile,writeFile,readdir,mkdir} from 'node:fs/promises';
import {join} from 'node:path';
import {createHash} from 'node:crypto';
import {openDatabase} from '../server/database.mjs';
import {createWorkspaces} from '../server/workspaces.mjs';
import {validateTemplate} from '../shared/templates.mjs';
import {validateScripts} from '../server/template-scripts.mjs';
assert(process.argv.slice(2).every(a=>a==='--apply'));
const apply=process.argv.includes('--apply'),out='.data/item-slot',projectId='p-2d314388-18c4-42f5-80eb-f6729dd23f0e';
const w=createWorkspaces(),db=await openDatabase(),hash=b=>createHash('sha256').update(b).digest('hex');
const json=async p=>JSON.parse((await readFile(p,'utf8')).replace(/^\uFEFF/,''));
await mkdir(out,{recursive:true});
try{
 const {path:project}=await w.unityBindings.requireBinding(projectId);
 const rows=(await db.pool.query("SELECT * FROM gamecli_ui.templates WHERE id IN ('ItemSlot','RewardEntry','SvMail')")).rows;
 const records=Object.fromEntries(rows.map(r=>[r.id,r]));
 for(const[id,version]of Object.entries({ItemSlot:1,RewardEntry:1,SvMail:5}))assert.equal(records[id].version,version,'Review newer '+id);
 await writeFile(out+'/before.json',JSON.stringify(rows));
 const assets=new Map();
 async function walk(dir){const result=[];for(const e of await readdir(dir,{withFileTypes:true})){const p=join(dir,e.name);result.push(...e.isDirectory()?await walk(p):[p]);}return result;}
 const relative=p=>p.slice(project.length+1).replaceAll('\\','/');
 async function materials(paths){const result=[];for(const path of [...new Set(paths)]){const body=await readFile(join(project,path)),id=hash(body);
  if(/\.(png|ttc|ttf|otf)$/.test(path)){const mime=path.endsWith('.png')?'image/png':'application/octet-stream';assets.set(id,{path,body,mime});result.push({path,assetId:id,sha256:id,mime});}
  else result.push({path,encoding:'utf8',content:body.toString('utf8'),sha256:id});
 }return result;}
 async function folder(name){const root='Assets/Data/Widgets/'+name;return [root+'.meta',...(await walk(join(project,root))).map(relative)];}
 async function code(names){const files=[];for(const name of names)for(const suffix of ['','.meta']){const path='Assets/Scripts/Systems/Common/UI/'+name+suffix;files.push({path,content:await readFile(join(project,path),'utf8')});}return files;}
 const closure=await json('reports/item-slot/dependencies.json');
 const external=closure.filter(p=>!p.startsWith('Assets/Scripts/')&&!p.startsWith('Assets/Data/Widgets/ItemSlot/'));
 const slotMaterials=await materials([...await folder('ItemSlot'),...external.flatMap(p=>[p,p+'.meta'])]);
 const slotFiles=await code(['UIItemSlot.cs','VM/VMItemSlot.cs']);
 const moves=await json('reports/item-slot/resource-moves.json');
 const intro=[
  'Base item display with product-defined public int type. No EquipPartType, inventory, mail or reward service dependency.',
  'Use UIItemSlot/VMItemSlot. Original eight binding indices are preserved: Back, Icon, Text, Locked, Amount, Select, Arrow, Edge. IconVisible is appended at index 8.',
  'Icon accepts either a valid string path or a loaded Sprite through existing SyncSprite. Never assign null to clear it; explicitly assign IconVisible=false while loading. Host guards asynchronous requests.',
  'Text is a host-formatted quantity or label. type is an unbound integer property whose meaning belongs to the product.',
  'Assets/Data/Widgets/ItemSlot/ItemSlot.prefab retains the SV item appearance. Its Lock/Mask/Arrow controls reuse the legacy slot. The old Arrow controller is missing from the source project; the new Arrow is static and has no broken Animator reference.',
  'Existing Widgets/Micros/ItemSlot.prefab keeps its old art and GUID. Upgrade its icon Metadata to SyncSprite and append IconVisible binding to Icon; preserve its TMP text binding. See bindings.json. Do not overwrite unrelated old pages.',
  'Restore native materials/meta first, preserving GUIDs. Included external dependencies are only the required Common fonts and shared UI textures. No Reward or Mail assets are required.',
  'Browser preview retains default item artwork. Radial Mask fill is native-only and previewHidden; three additional controls are serialized in the native prefab. Dynamic behavior must be checked in Unity.'
 ];
 slotFiles.push({path:'GameCLI/Templates/ItemSlot/materials.json',content:JSON.stringify({schemaVersion:1,files:slotMaterials})});
 slotFiles.push({path:'GameCLI/Templates/ItemSlot/resource-moves.json',content:JSON.stringify(moves,null,2)});
 slotFiles.push({path:'GameCLI/Templates/ItemSlot/bindings.json',content:await readFile('reports/item-slot/bindings.json','utf8')});
 slotFiles.push({path:'GameCLI/Templates/ItemSlot/README.md',content:intro.join('\n\n')});
 const rewardPayload=structuredClone(records.RewardEntry.payload),root=rewardPayload.design.nodes.find(n=>n.name==='BagIconNew');assert(root);
 const ids=new Set([root.id]);let length;do{length=ids.size;for(const n of rewardPayload.design.nodes)if(ids.has(n.parent))ids.add(n.id);}while(length!==ids.size);
 const slotNodes=structuredClone(rewardPayload.design.nodes.filter(n=>ids.has(n.id))),slotRoot=slotNodes.find(n=>n.id===root.id);
 Object.assign(slotRoot,{name:'ItemSlot',parent:null,x:0,y:0});
 // Read serialized geometry for the three retained legacy controls; never write YAML.
 const controls=[];
 for(const name of ['Lock','Mask','Arrow']){
  const body=await readFile(join(project,'Assets/Data/Widgets/ItemSlot/Parts/'+name+'.prefab'),'utf8');
  const vector=key=>{const m=body.match(new RegExp(key+': \\{x: ([^,]+), y: ([^}]+)\\}'));assert(m,key);return {x:Number(m[1]),y:Number(m[2])};};
  const sprite=body.match(/m_Sprite: \{fileID: \d+, guid: (\w+)/);assert(sprite);
  let asset;
  for(const path of external.filter(p=>p.endsWith('.png'))){if((await readFile(join(project,path+'.meta'),'utf8')).includes('guid: '+sprite[1]))asset=slotMaterials.find(f=>f.path===path);}
  assert(asset,'Missing control sprite '+name);
  controls.push({id:root.id+name,parent:root.id,name,type:'Image',x:0,y:0,width:152,height:152,color:'#ffffff',text:'',fontSize:20,image:asset.assetId,imageFit:'100% 100%',active:name==='Mask',...(name==='Mask'?{previewHidden:true}:{}),rectTransform:{anchorMin:vector('m_AnchorMin'),anchorMax:vector('m_AnchorMax'),pivot:vector('m_Pivot'),anchoredPosition:vector('m_AnchoredPosition'),sizeDelta:vector('m_SizeDelta')}});
 }
 slotNodes.push(...structuredClone(controls));rewardPayload.design.nodes.push(...structuredClone(controls));
 const slotPayload=validateTemplate({root:slotRoot.id,design:{schemaVersion:1,name:'ItemSlot',width:240,height:240,nodes:slotNodes,animations:structuredClone((rewardPayload.design.animations??[]).filter(a=>ids.has(a.hostNodeId)))},configurations:{schemaVersion:1,tables:[],dependencies:[{name:'com.oathx.widget',kind:'framework',required:true,reason:'UIWidget, VM and standard Sync actions'}],integration:intro}},'prefab');
 rewardPayload.configurations.dependencies.push({name:'ItemSlot',kind:'template',templateId:'ItemSlot',version:2,required:true,reason:'Shared base slot, VM and item artwork; install before RewardEntry.'});
 rewardPayload.configurations.integration=[
  'UIRewardEntry composes a child ItemSlot.prefab. VMRewardEntry contains Item (VMItemSlot/SyncMetadata) and Claimed (bool/SyncVisible), in that order.',
  'Host assigns Item.Text, Item.Icon, Item.IconVisible and product-defined Item.type. Reward only displays its claim marker; it never owns grant rules.',
  'Install ItemSlot v2 first. Keep BagIconNew child name so existing animation paths remain valid. Reward-native materials retain only claim-marker and outer animation assets.',
  'UIRewardEntry.Unref sets Item=null before base.Unref, releasing the child subscription. Host must guard Item against stale callbacks. Static instances should not repeatedly Ref; Fancy reuse retains the existing framework cache limitations.'
 ];
 const rewardFiles=await code(['UIRewardEntry.cs','VM/VMRewardEntry.cs']);
 rewardFiles.push({path:'GameCLI/Templates/Reward/materials.json',content:JSON.stringify({schemaVersion:1,dependencies:[{templateId:'ItemSlot',version:2}],files:await materials(await folder('Reward'))})});
 rewardFiles.push({path:'GameCLI/Templates/Reward/README.md',content:rewardPayload.configurations.integration.join('\n\n')});
 const mailPayload=structuredClone(records.SvMail.payload);
 mailPayload.design.nodes.push(...structuredClone(controls));
 mailPayload.configurations.dependencies=mailPayload.configurations.dependencies.map(d=>d.templateId==='RewardEntry'?{...d,version:2}:d);
 mailPayload.configurations.integration=mailPayload.configurations.integration.map(s=>s.replaceAll('RewardEntry v1','RewardEntry v2'));
 mailPayload.configurations.integration.push('RewardEntry v2 requires ItemSlot v2. Mail constructs reward.Item and updates Item.Text/Icon/IconVisible; old reward Count/Icon fields no longer exist.');
 const old=records.SvMail.scripts.files;
 const mailFiles=old.filter(f=>!f.path.startsWith('Assets/Scripts/Systems/Mail/')&&!['GameCLI/Templates/Mail/materials.json','GameCLI/Templates/Mail/bindings.json'].includes(f.path)).map(f=>({path:f.path,content:f.content}));
 for(const path of await walk(join(project,'Assets/Scripts/Systems/Mail')))mailFiles.push({path:relative(path),content:await readFile(path,'utf8')});
 const oldMaterials=JSON.parse(old.find(f=>f.path.endsWith('/materials.json')).content);
 const native=await materials([...await folder('Mail'),...['msyh.ttc','msyhbd.ttc'].flatMap(f=>['',' .meta'].map(s=>'Assets/Data/Widgets/Common/Resources/'+f+s.trim()))]);
 native.push(...oldMaterials.files.filter(f=>!f.path.startsWith('Assets/')));
 mailFiles.push({path:'GameCLI/Templates/Mail/materials.json',content:JSON.stringify({schemaVersion:1,dependencies:[{templateId:'RewardEntry',version:2}],files:native})});
 mailFiles.push({path:'GameCLI/Templates/Mail/bindings.json',content:await readFile('.data/imports/sv-mail/bindings.json','utf8')});
 const readme=mailFiles.find(f=>f.path.endsWith('/README.md'));readme.content=readme.content.replaceAll('RewardEntry v1','RewardEntry v2')+'\n\nReward now composes ItemSlot v2. Mail assigns reward.Item.Text, reward.Item.Icon and reward.Item.IconVisible. Item.type is a host-defined integer.';
 const candidates=[{id:'ItemSlot',payload:slotPayload,scripts:validateScripts({schemaVersion:1,files:slotFiles})},{id:'RewardEntry',payload:validateTemplate(rewardPayload,'prefab'),scripts:validateScripts({schemaVersion:1,files:rewardFiles})},{id:'SvMail',payload:validateTemplate(mailPayload,'system'),scripts:validateScripts({schemaVersion:1,files:mailFiles})}];
 for(const t of candidates)await writeFile(out+'/'+t.id+'.json',JSON.stringify(t,null,2));
 if(!apply){console.log(JSON.stringify({dryRun:true,templates:candidates.map(t=>({id:t.id,files:t.scripts.files.length})),assets:assets.size}));}
 else{
  for(const[id,a]of assets)await db.pool.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT DO NOTHING',[id,a.mime,a.body,a.path]);
  const results=[];
  for(const t of candidates){
   const published=await w.templateEditor.published(t.id);assert.equal(published.revision,records[t.id].version);
   const r=t.payload.design.nodes.find(n=>n.parent===null);r.templateSourceId=t.id;r.scripts=t.scripts;r.configurations=t.payload.configurations;
   const opened=await w.templateEditor.open({templateId:t.id}),draft=await w.templateEditor.read(opened.id);assert.deepEqual(draft.design,published.design,'Preserve draft edits '+t.id);
   const saved=await w.templateEditor.save(opened.id,{draftId:opened.id,revision:draft.revision,design:t.payload.design});
   const candidate=await w.templateEditor.submit(opened.id,{draftId:opened.id,revision:saved.revision});const result=await w.publish({candidateId:candidate.id,confirm:true});
   const actual=(await db.pool.query('SELECT scripts FROM gamecli_ui.templates WHERE id=$1',[t.id])).rows[0];assert.deepEqual(actual.scripts,t.scripts);results.push({id:t.id,version:result.version});
  }
  const snapshot=await w.snapshot(projectId);await writeFile(out+'/project-before.json',JSON.stringify(snapshot));
  for(const t of candidates.filter(t=>t.id!=='ItemSlot'))for(const root of snapshot.design.nodes.filter(n=>n.parent===null&&n.templateSourceId===t.id)){
   assert.deepEqual(root.scripts,records[t.id].scripts,'Preserve edited instance code');
   const prefix=root.id.slice(0,root.id.length-records[t.id].payload.root.length);
   for(const n of controls){const node=structuredClone(n);node.id=prefix+n.id;node.parent=prefix+n.parent;assert(!snapshot.design.nodes.some(x=>x.id===node.id));snapshot.design.nodes.push(node);}
   root.scripts=t.scripts;root.configurations=t.payload.configurations;root.templateSourceVersion=results.find(r=>r.id===t.id).version;
  }
  const saved=await w.save(projectId,{projectId,revision:snapshot.revision,projectVersion:snapshot.project.version,design:snapshot.design});
  const report={verified:true,templates:results,projectRevision:saved.revision};await writeFile('reports/item-slot/publication.json',JSON.stringify(report,null,2));console.log(JSON.stringify(report));
 }
}finally{await w.close();await db.close();}
