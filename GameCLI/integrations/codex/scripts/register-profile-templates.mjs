// First registration of the independent profile/widget references; updates use candidates.
import assert from 'node:assert/strict';
import {readFile,writeFile,readdir,mkdir} from 'node:fs/promises';
import {join} from 'node:path';
import {createHash} from 'node:crypto';
import {isDeepStrictEqual} from 'node:util';
import {openDatabase} from '../server/database.mjs';
import {workspaceActor,createWorkspaces} from '../server/workspaces.mjs';
import {validateScripts} from '../server/template-scripts.mjs';
import {validateTemplate} from '../shared/templates.mjs';
const w=createWorkspaces(),folder='.data/profile',hash=b=>createHash('sha256').update(b).digest('hex');
try {
 const binding=await w.unityBindings.requireBinding('p-2d314388-18c4-42f5-80eb-f6729dd23f0e');
 const source=JSON.parse(await readFile('.data/imports/sv-home/fidelity-candidate/template.json','utf8'));
 const specs=[['PlayerProfile','ProfileView'],['ProfileShortcuts','ProfileShortcuts']];
 const files=[],materials=[],assets=new Map();
 for(const name of ['UIActivityEntry.cs','UIActivityBadge.cs','VM/VMActivityEntry.cs','VM/VMActivityBadge.cs','ActivityBadge.cs','HomeTextOutline.cs','UIProfileShortcuts.cs','VM/VMProfileShortcuts.cs','UIProfile.cs','VM/VMProfile.cs'])for(const suffix of ['', '.meta']){
  const path='Assets/Scripts/Systems/Common/UI/'+name+suffix;files.push({path,content:await readFile(join(binding.path,path),'utf8')});
 }
 async function walk(path){const all=[];for(const e of await readdir(path,{withFileTypes:true})){const p=join(path,e.name);if(e.isDirectory())all.push(...await walk(p));else all.push(p);}return all;}
 for(const dir of ['Profile','Common'])for(const p of [join(binding.path,'Assets/Data/Widgets/'+dir+'.meta'),...await walk(join(binding.path,'Assets/Data/Widgets/'+dir))]){
  const body=await readFile(p),path=p.slice(binding.path.length+1).replaceAll('\\','/'),id=hash(body);
  if(/\.(png|ttf|ttc|otf)$/.test(path)){const mime=path.endsWith('.png')?'image/png':'application/octet-stream';assets.set(id,{body,path,mime});materials.push({path,assetId:id,sha256:id,mime});}
  else materials.push({path,encoding:'utf8',content:body.toString('utf8'),sha256:id});
 }
 const integration=[
  "The two reusable components are Profile.prefab and ProfileShortcuts.prefab. Drag either root into a page to reuse the whole component. Avatar and individual shortcut prefabs live under Profile/Parts and are implementation pieces, not the primary integration surface.",
  "ProfileShortcuts contains Mail, Settings, Notice, Friends and Affinity. Bind one VMProfileShortcuts at the group root; its five VMActivityEntry properties bind the existing entry widgets through SyncMetadata. Host-supplied callbacks and state retain the entry contract. No system routing or entity lookup is embedded in the shared UI.",
  "Use the host SyncMetadata field once to assign VMProfileShortcuts with all five entries initialized. Refresh existing entry properties; do not replace callbacks or nested VMs. Set the group VM to null before host Unref. The group releases child bindings on Unref/OnDestroy, rejects rebinding and moves all five entries as one RectTransform.",
  "Profile binds one VMProfile with PlayerName, PlayerLevel, Portrait, Frame, ProfileEntry and AvatarEntry. UIProfile owns all inner Text/Image/entry bindings. The host supplies formatted text, valid asset paths and entry callbacks. Existing SyncSprite resolves paths through UIResourceManager and the configured UILoader; SyncVisible binds PortraitVisible and FrameVisible. Unassigned paths preserve authored artwork. Hide and restore through visibility, never null or empty paths. No custom image ISyncAction or player lookup is introduced. Framework limitation: path callbacks do not guard request ordering or destruction during loading; products requiring these cases must improve shared loader lifecycle protection.",
  "Keep original font, layout, click regions and tip nodes. RedDotScope and configuration remain host-owned. Home TopLeft entrance still animates both group roots; group extraction does not add a duplicate entrance animation.",
  "Native code lives in Systems/Common/UI and UI/VM. Shared assets remain Widgets/Common. Preserve GUIDs and merge identical shared paths. resource-moves.json records the original shared-resource upgrade. The former Profile prefab body is now Parts/ProfileBody.prefab with its original GUID; the new Profile root wraps it and owns VMProfile bindings. AvatarFrame and individual buttons remain internal Parts.",
  "Historical individually registered button templates are retained only in the legacy-parts catalog group for migration. New product work uses the two composite templates."
];
 const moves=JSON.parse(await readFile(folder+'/resource-moves.json','utf8'));
 files.push({path:'GameCLI/Templates/Profile/materials.json',content:JSON.stringify({schemaVersion:1,files:materials,download:'GET /api/studio/assets/<assetId>; verify SHA256 and preserve .meta GUIDs.'})});
 files.push({path:'GameCLI/Templates/Profile/resource-moves.json',content:JSON.stringify(moves,null,2)});
 files.push({path:'GameCLI/Templates/Profile/README.md',content:integration.join('\n\n')});
 const scripts=validateScripts({schemaVersion:1,files}),templates=[];
 for(const [id,name]of specs){
  const root=source.design.nodes.find(n=>n.name===name);assert(root,name);const ids=new Set([root.id]);let count;
  do{count=ids.size;for(const n of source.design.nodes)if(ids.has(n.parent))ids.add(n.id);}while(count!==ids.size);
  const nodes=structuredClone(source.design.nodes.filter(n=>ids.has(n.id))),r=nodes.find(n=>n.id===root.id);
  Object.assign(r,{parent:null,name:id,x:0,y:0});r.rectTransform={anchorMin:{x:0,y:1},anchorMax:{x:0,y:1},pivot:{x:0,y:1},anchoredPosition:{x:0,y:0},sizeDelta:{x:root.width,y:root.height}};
  const payload=validateTemplate({root:root.id,design:{schemaVersion:1,name:id,width:Math.max(240,Math.ceil(root.width)),height:Math.max(240,Math.ceil(root.height)),nodes,animations:[]},configurations:{schemaVersion:1,tables:[],dependencies:['com.oathx.widget'],integration}},'system');
  templates.push({id,payload,scripts});
 }
 await mkdir(folder,{recursive:true});await writeFile(folder+'/templates.json',JSON.stringify(templates));
 const summary={templates:templates.map(t=>t.id),files:files.length,materials:materials.length,assets:assets.size};
 if(!process.argv.includes('--apply'))console.log(JSON.stringify({...summary,dryRun:true}));
 else{
  const db=await openDatabase(),c=await db.pool.connect();
  try{
   await c.query('BEGIN');assert((await c.query('SELECT actor FROM gamecli_ui.template_admins WHERE actor=$1',[workspaceActor()])).rows.length,'Admin required');
   for(const t of templates)assert(!(await c.query('SELECT id FROM gamecli_ui.templates WHERE id=$1',[t.id])).rows.length,'Existing template requires candidate: '+t.id);
   for(const[id,a]of assets)await c.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT DO NOTHING',[id,a.mime,a.body,a.path]);
   for(const t of templates)for(const n of t.payload.design.nodes)for(const id of[n.image,n.fontAsset].filter(Boolean))assert((await c.query('SELECT id FROM gamecli_ui.assets WHERE id=$1',[id])).rows.length,'Missing preview asset '+id);
   await c.query("INSERT INTO gamecli_ui.template_groups(category_id,id,name,sort) VALUES('system','profile','Profile UI',61) ON CONFLICT DO NOTHING");
   await c.query("SELECT set_config('gamecli.publish','approved',true)");
   for(const [index,t]of templates.entries()){
    await c.query("INSERT INTO gamecli_ui.templates(id,category_id,group_id,name,description,sort,version,payload,scripts) VALUES($1,'system','profile',$1,'公共头像与快捷入口 UI · 宿主提供数据与交互',$2,1,$3::jsonb,$4::jsonb)",[t.id,100+index,JSON.stringify(t.payload),JSON.stringify(scripts)]);
    const saved=(await c.query('SELECT payload,scripts FROM gamecli_ui.templates WHERE id=$1',[t.id])).rows[0];assert(isDeepStrictEqual(saved,{payload:t.payload,scripts}));
   }
   await c.query('COMMIT');console.log(JSON.stringify({...summary,version:1,verified:true}));
  }catch(e){await c.query('ROLLBACK');throw e;}finally{c.release();await db.close();}
 }
}finally{await w.close();}
