// First registration of the independent code-only notification template.
import assert from 'node:assert/strict';
import {readFile,readdir,writeFile} from 'node:fs/promises';
import {join,resolve} from 'node:path';
import {createHash} from 'node:crypto';
import {isDeepStrictEqual} from 'node:util';
import {createWorkspaces,workspaceActor} from '../server/workspaces.mjs';
import {openDatabase} from '../server/database.mjs';
import {validateTemplate} from '../shared/templates.mjs';
import {validateScripts} from '../server/template-scripts.mjs';
import {validateConfigurations} from '../shared/configurations.mjs';
assert(process.argv.slice(2).every(a=>a==='--apply'));
const apply=process.argv.includes('--apply'),folder='.data/red-dot',w=createWorkspaces();
const hash=bytes=>createHash('sha256').update(bytes).digest('hex');
try{
 const binding=await w.unityBindings.requireBinding('p-2d314388-18c4-42f5-80eb-f6729dd23f0e'),project=binding.path;
 const files=[],root='Assets/Scripts/Systems/RedDot';
 for(const name of await readdir(join(project,root)))if(/\.(cs|meta)$/.test(name))files.push({path:root+'/'+name,content:await readFile(join(project,root,name),'utf8')});
 files.push({path:'GameCLI/Templates/RedDot/framework/GamePlugin.cs',content:await readFile(join(project,'Assets/Scripts/GamePlugin.cs'),'utf8')});
 for(const path of ['com.oathx.widget/Runtime/ResourceManager/IUIEventSender.cs','com.oathx.widget/Runtime/ResourceManager/IUIEventSender.cs.meta','com.oathx.widget/Runtime/ResourceManager/UIResourceManager.cs','com.oathx.widget.extension/Runtime/Scripts/UISystem.cs'])files.push({path:'GameCLI/Templates/RedDot/framework/'+path,content:await readFile(join(project,'LocalPackages',path),'utf8')});
 files.push({path:'Assets/Scripts/Systems/RedDot.meta',content:await readFile(join(project,'Assets/Scripts/Systems/RedDot.meta'),'utf8')});
 const rows=JSON.parse(await readFile(folder+'/config-query.json','utf8')).data.rows.map(row=>Object.fromEntries(row.cells.map(c=>[c.field,c.value])));
 assert.equal(rows.length,54);assert(rows.every(r=>Number(r.NumID)>0));
 const workbook=await readFile(resolve(project,'../config/propRedDot.xlsx')),startup=await readFile(folder+'/config/propFunctionSystem.xlsx');
 const configurations=validateConfigurations({schemaVersion:1,tables:[{
  name:'propRedDot',purpose:'example',primaryKey:'NumID',aliasField:'ID',
  fields:['ID','NumID','ParentID','RedDotCondition','RedDotDisplay','Enabled','RefreshOnOpen','RefreshOnClose'].map(name=>({name,type:name==='RedDotCondition'||name==='RedDotDisplay'?'enum':'int'})),entries:rows,
  source:{fileName:'propRedDot.xlsx',sheet:'RedDot',assetId:hash(workbook),sha256:hash(workbook)}
 },{name:'propFunctionSystem',purpose:'required',primaryKey:'NumID',aliasField:'ID',
  fields:[['ID','int'],['NumID','int'],['Name','string'],['TargetType','enum'],['ClassType','string'],['InstallOnStartup','int'],['Condtions','List<string>'],['Active','int'],['StartTime','string'],['StopTime','string'],['SceneId','int'],['Version','string']].map(([name,type])=>({name,type})),
  entries:[{ID:'SYS_RED_DOT',NumID:10020,Name:'RedDot',TargetType:'None',ClassType:'RedDotSystem',InstallOnStartup:1,Active:1}],source:{fileName:'propFunctionSystem.xlsx',sheet:'FunctionSystem',assetId:hash(startup),sha256:hash(startup)}
 }],dependencies:['RuntimeSystem','com.oathx.core','com.oathx.entity','com.oathx.widget.extension','Config.Excel'],integration:[
  'Merge propRedDot and SYS_RED_DOT into product configuration; export with the existing e2cs tool and load RedDotConfig before system startup. Do not replace the product enum/configuration files with reference outputs.',
  'RedDotCondition=None aggregates children; RedDotEntity.UpdateSignal validates inputs; Refresh evaluates SignalCount and parent aggregation; GetCount exposes calculated state. Add explicit enum detectors for product entity conditions. All signals start at zero; no mock unread mail or rewards.',
  'Attach RedDotScope to the widget root with serialized configuration IDs, targets and optional Count labels. Existing Home references this exact component GUID. The 53 Home bindings are an example, not mandatory product entries.',
  'Require IUIEventSender in the widget package. GamePlugin implements it; Startup(new ResourceConfigure { loader = ..., cacher = ..., eventSenders = new List<IUIEventSender> { this } }). OnWidgetOpened forwards DispatchEvent(new WidgetOpenedEvent { data = new WidgetOpened { widget = widget } }); OnWidgetClosing forwards DispatchEvent(new WidgetClosingEvent { data = new WidgetClosing { widget = widget } }). Framework files are merge references, not permission to overwrite product packages. Widget close is a start-closing event. Frame-end coalescing happens after open callbacks; disabling a page releases presentation, never clears business state.'
 ]});
 files.push({path:'GameCLI/Templates/RedDot/home-bindings.json',content:await readFile('config/home-red-dots.json','utf8')});
 files.push({path:'GameCLI/Templates/RedDot/README.md',content:configurations.integration.join('\n')+'\nBusiness message: XPluginUtility.FireEvent(new RedDotSignalChanged { id = Excel.RED_DOT_HOME_MAILTIPS, count = 3 });\n'});
 const scripts=validateScripts({schemaVersion:1,files});
 const payload=validateTemplate({kind:'code',root:null,design:{schemaVersion:1,name:'RedDotSystem',width:1920,height:1080,nodes:[]},configurations},'system');
 await writeFile(folder+'/scripts.json',JSON.stringify(scripts,null,2));await writeFile(folder+'/publication.json',JSON.stringify(payload,null,2));
 const db=await openDatabase(),client=await db.pool.connect();
 try{
  await client.query('BEGIN');assert((await client.query('SELECT actor FROM gamecli_ui.template_admins WHERE actor=$1',[workspaceActor()])).rows.length);
  const current=(await client.query('SELECT * FROM gamecli_ui.templates WHERE id=$1 FOR UPDATE',['RedDotSystem'])).rows[0];
  const changed=current&&(!isDeepStrictEqual(current.scripts,scripts)||!isDeepStrictEqual(current.payload,payload));
  if(apply&&!current){
   for(const body of [workbook,startup])await client.query('INSERT INTO gamecli_ui.configuration_assets(id,body) VALUES($1,$2) ON CONFLICT DO NOTHING',[hash(body),body]);
   await client.query("INSERT INTO gamecli_ui.template_groups(category_id,id,name,sort) VALUES('system','red-dot','RedDot',70) ON CONFLICT DO NOTHING");
   await client.query("SELECT set_config('gamecli.publish','approved',true)");
   await client.query("INSERT INTO gamecli_ui.templates(id,category_id,group_id,name,description,sort,payload,scripts) VALUES('RedDotSystem','system','red-dot','RedDot','配置条件驱动红点 · Widget 生命周期 · 本地事件 · 无独立窗口',100,$1::jsonb,$2::jsonb)",[JSON.stringify(payload),JSON.stringify(scripts)]);
  }
  await client.query('COMMIT');
  if(apply&&changed){
   const opened=await w.templateEditor.open({templateId:'RedDotSystem'}),draft=await w.templateEditor.read(opened.id),published=await w.templateEditor.published('RedDotSystem');
   assert.deepEqual(draft.design,published.design,'Preserve edited draft');
   const root=draft.design.nodes.find(n=>n.parent===null);root.scripts=scripts;root.configurations=configurations;
   const saved=await w.templateEditor.save(opened.id,{draftId:opened.id,revision:draft.revision,design:draft.design});
   const candidate=await w.templateEditor.submit(opened.id,{draftId:opened.id,revision:saved.revision});await w.publish({candidateId:candidate.id,confirm:true});
  }
  if(apply){const actual=(await client.query("SELECT payload,scripts,version FROM gamecli_ui.templates WHERE id='RedDotSystem'")).rows[0];assert.deepEqual(actual.payload,payload);assert.deepEqual(actual.scripts,scripts);console.log(JSON.stringify({id:'RedDotSystem',version:actual.version,files:files.length,rows:rows.length,verified:true}));}
  else console.log(JSON.stringify({dryRun:true,files:files.length,rows:rows.length}));
 }catch(error){await client.query('ROLLBACK');throw error;}finally{client.release();await db.close();}
}finally{await w.close();}
