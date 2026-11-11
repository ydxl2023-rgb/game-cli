// Native Unity export -> standard login Notice template. Never changes the source prefab.
import {readFile,writeFile} from 'node:fs/promises';
import {join} from 'node:path';
import {createHash} from 'node:crypto';
import {isDeepStrictEqual} from 'node:util';
import {normalizeRect,resolveLayout} from '../shared/rect-transform.mjs';
import {validateTemplate} from '../shared/templates.mjs';
import {openDatabase} from '../server/database.mjs';
import {workspaceActor} from '../server/workspaces.mjs';
import {validateScripts} from '../server/template-scripts.mjs';
import {readdir} from 'node:fs/promises';
import {readChineseLanguage} from '../server/sv-language.mjs';
const project='G:/Works/SV/Development/Project',folder='.data/imports/sv-notice';
const source=JSON.parse(await readFile(join(folder,'unity/unity-prefab.json'),'utf8'));
if(source.sourcePath!=='Assets/data/UI/Announcement/@AnnouncementView/Prefab/SystemAnnouncementView.prefab')throw new Error('Wrong source prefab');
const resolved=JSON.parse(await readFile(join(folder,'localized/unity-prefab.json'),'utf8'));
const languagePath=join(project,'Assets/data/@Cfg/language_cfg.bytes');
const language=await readChineseLanguage(languagePath),texts=[];
for(const n of source.nodes)for(const component of n.components){
 const value=Object.values(JSON.parse(component.json))[0]?.m_Text,match=value?.match(/^\[LID:(\d+)\]$/);
 if(match){if(!language.has(Number(match[1])))throw Error('Missing Chinese language ID: '+match[1]);texts.push({path:n.path,text:language.get(Number(match[1]))});}
}
const locale={locale:'zh-CN',source:languagePath,texts};
await writeFile(join(folder,'locale.json'),JSON.stringify(locale,null,2));
if(resolved.nodes[0].rectTransform.sizeDelta.x!==1920)throw Error('Unity layout export not evaluated');
const production=resolved.nodes,assets=new Map(),nodes=[];
const names={SystemAnnouncementView:'Notice',_TitleImageSell:'Title',CloseBtn:'Close',ScrollCell:'Item',WeekNomoreImgChick:'Selected'};
const id=path=>'Notice'+path.replaceAll('/','_');
const notes=[];
for(const n of production){
 const components=n.components.map(c=>({...c,data:Object.values(JSON.parse(c.json))[0]}));
 const graphic=components.find(c=>c.data?.m_Color&&('m_FontData'in c.data||'m_Sprite'in c.data||'m_Texture'in c.data));
 const button=components.find(c=>c.type.startsWith('UnityEngine.UI.Button,'));
 const g=graphic?.data,r=normalizeRect(n.rectTransform);
 const name=names[n.name.trim()]??n.name.replace(/[^a-zA-Z0-9]+(.)?/g,(_,c)=>c?c.toUpperCase():'').replace(/^./,c=>c.toUpperCase());
 const node={id:id(n.path),name,parent:n.path==='0'?null:id(n.parent),type:g?.m_FontData?'Text':button?'Button':'Container',x:0,y:0,width:1,height:1,color:g?'#'+['r','g','b'].map(k=>Math.round(g.m_Color[k]*255).toString(16).padStart(2,'0')).join(''):'#ffffff',text:(locale.texts.find(t=>t.path===n.path)?.text??g?.m_Text??'').replace(/<[^>]*>/g,''),fontSize:g?.m_FontData?.m_FontSize??20,active:n.activeSelf,rectTransform:r,localScale:{x:n.rectTransform.localScale.x,y:n.rectTransform.localScale.y},rotation:n.rectTransform.localEulerAngles.z};
 if(graphic&&!g.m_FontData&&!button&&!production.some(v=>v.parent===n.path))node.type='Image';
 if(!graphic)node.surface='transparent';

 if(button)node.interactable=button.data.m_Interactable;
 if(g){node.opacity=g.m_Color.a;if(g.m_FontData){node.textAlign=['left','center','right'][g.m_FontData.m_Alignment%3];node.verticalAlign=['start','center','end'][Math.floor(g.m_FontData.m_Alignment/3)];}}
 const ref=graphic?.references.find(r=>['m_Sprite','m_Texture'].includes(r.property)&&r.path);
 if(ref&&ref.path!=='Resources/unity_builtin_extra'){
  if(!ref.path.endsWith('.png'))throw new Error('Unsupported image: '+ref.path);
  const body=await readFile(join(project,ref.path)),hash=createHash('sha256').update(body).digest('hex');
  assets.set(hash,{body,path:ref.path});node.image=hash;node.imageFit=g.m_PreserveAspect?'contain':'100% 100%';
  if(g.m_Type===1){const meta=await readFile(join(project,ref.path+'.meta'),'utf8');const border=meta.match(/spriteBorder: \{x: ([\d.]+), y: ([\d.]+), z: ([\d.]+), w: ([\d.]+)\}/);if(!border)throw new Error('Missing sprite border: '+ref.path);node.imageBorder=Object.fromEntries(['x','y','z','w'].map((k,i)=>[k,Number(border[i+1])]));node.imageType='sliced';}
  if(g.m_UVRect)node.uvRect=Object.fromEntries(['x','y','width','height'].map(k=>[k,g.m_UVRect[k]]));
 }
 if(n.path==='0'){node.localScale={x:1,y:1};node.rectTransform={anchorMin:{x:0,y:0},anchorMax:{x:1,y:1},pivot:{x:.5,y:.5},anchoredPosition:{x:0,y:0},sizeDelta:{x:0,y:0}};}
 if(ref?.path==='Resources/unity_builtin_extra')notes.push({node:id(n.path),reason:'Unity built-in sprite retained in native evidence; Studio uses the original graphic tint.',fileId:ref.fileId});
 if(graphic&&g.m_Enabled===false){node.surface='transparent';delete node.image;delete node.imageBorder;delete node.imageType;if(node.type==='Text')node.text='';notes.push({node:node.id,reason:'Disabled source graphic omitted visually; original component retained in native evidence.'});}
 if(graphic&&g.m_Enabled===false)node.opacity=1;
 const mask=components.find(c=>c.type.startsWith('UnityEngine.UI.Mask,'));
 if(mask?.data.m_ShowMaskGraphic===false){node.surface='transparent';delete node.image;delete node.imageBorder;delete node.imageType;notes.push({node:node.id,reason:'Mask graphic is hidden in the source; descendants remain visible.'});}
 // UGUI Graphic alpha never fades descendants. Solid parent graphics become explicit background layers.
 const descendants=production.some(child=>child.parent===n.path);
 if(descendants&&node.opacity<1&&!node.image){
  const alpha=node.opacity;node.opacity=1;node.surface='transparent';nodes.push(node);
  if(alpha>0&&g.m_Enabled!==false)nodes.push({id:node.id+'Surface',name:'Background',parent:node.id,type:'Image',x:0,y:0,width:1,height:1,color:node.color,text:'',fontSize:20,opacity:alpha,rectTransform:{anchorMin:{x:0,y:0},anchorMax:{x:1,y:1},pivot:{x:.5,y:.5},anchoredPosition:{x:0,y:0},sizeDelta:{x:0,y:0}}});
 }else nodes.push(node);
}

// Local preview actions only; live notice content remains server-owned.
for(const node of nodes)if(node.type==='Button')node.action={type:'none'};
for(const [path,target,type] of [['0/2/2','0','hide'],['0/3/0/0/0/2/0','0/3/0/0/0/1','toggle'],['0/3/0/0/0/2/3','0/3/0/0/0/1','toggle'],['0/4/0','0/4/0/0','toggle'],['0/4/2','0/4/0/0','toggle']]){
 const node=nodes.find(n=>n.id===id(path));if(!node)throw Error('Notice binding missing: '+path);node.action={type,target:id(target)};
}
const validated=validateTemplate({root:id('0'),design:resolveLayout({schemaVersion:1,name:'SvNotice',width:1920,height:1080,nodes})},'system');
const payload={...validated,source:{project,path:source.sourcePath,guid:source.sourceGuid,mode:'unity-native-layout-zh-CN',sha256:createHash('sha256').update(await readFile(join(project,source.sourcePath))).digest('hex'),native:source,resolved,locale},limitations:[
 'Login Announcement opens LoginNoticeView / SystemAnnouncementView; GameAnnouncementView is a different in-game window.',
 'Notice titles, text and images are supplied dynamically by the server. The authored list item is retained as a template; no live announcement content is fabricated.',
 'Resolved 1920x1080 prefab layout snapshot; UIStaticList, scroll masks, rich text and dynamic content height are preserved in native evidence but not fully simulated in Studio.',
 'Close, content expansion and checkbox are local preview actions; one-week persistence, SDK reporting and external links are not executed.',
 'The original prefab has no AnimatorController or AnimationClip dependencies; button scale tween and audio remain native component evidence.',
 ...notes]};
const files=[],codeRoot='Assets/HotFix_Project/Features/CommonFeature/Notice/UI/LoginNoticeView';
for(const entry of await readdir(join(project,codeRoot),{recursive:true}))if(/\.cs(?:\.meta)?$/.test(entry)){const path=codeRoot+'/'+entry.replaceAll('\\','/');files.push({path,content:await readFile(join(project,path),'utf8')});}
for(const d of source.dependencies)if(/\.(anim|controller)$/.test(d.path)){for(const path of [d.path,d.path+'.meta'])files.push({path,content:await readFile(join(project,path),'utf8')});}
const scripts=validateScripts({schemaVersion:1,files});
await writeFile(join(folder,'template.json'),JSON.stringify(payload,null,2));await writeFile(join(folder,'scripts.json'),JSON.stringify(scripts,null,2));
const report={template:'SvNotice',name:'SV Notice',nodes:payload.design.nodes.length,images:assets.size,sourceFiles:scripts.files.length,limitations:payload.limitations};
await writeFile(join(folder,'report.json'),JSON.stringify(report,null,2));
if(!process.argv.includes('--publish')){console.log(JSON.stringify(report));process.exit(0);}
// Explicit administrator-only initial registration; existing standards cannot be overwritten.
const db=await openDatabase(),client=await db.pool.connect();
try{
 await client.query('BEGIN');
 if(!(await client.query('SELECT actor FROM gamecli_ui.template_admins WHERE actor=$1',[workspaceActor()])).rows.length)throw Error('Template administrator required');
 if((await client.query('SELECT id FROM gamecli_ui.templates WHERE id=$1',['SvNotice'])).rows.length)throw Error('SvNotice already exists; submit a reviewed candidate for future updates');
 await client.query("INSERT INTO gamecli_ui.template_groups(category_id,id,name,sort) VALUES('system','notice','公告',7) ON CONFLICT DO NOTHING");
 for(const [hash,a]of assets)await client.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT(id) DO NOTHING',[hash,'image/png',a.body,a.path]);
 await client.query("SELECT set_config('gamecli.publish','approved',true)");
 await client.query('INSERT INTO gamecli_ui.templates(id,category_id,group_id,name,description,sort,version,payload,scripts) VALUES($1,$2,$3,$4,$5,$6,1,$7::jsonb,$8::jsonb)',['SvNotice','system','notice','SV Notice','1920 × 1080 · 登录公告 · 原始列表与资源',7,JSON.stringify(payload),JSON.stringify(scripts)]);
 const saved=(await client.query('SELECT payload,scripts FROM gamecli_ui.templates WHERE id=$1',['SvNotice'])).rows[0];if(!isDeepStrictEqual(saved,{payload,scripts}))throw Error('Database readback mismatch');
 if((await client.query('SELECT version FROM gamecli_ui.template_versions WHERE template_id=$1',['SvNotice'])).rows.length!==1)throw Error('Immutable version missing');
 await client.query('COMMIT');console.log(JSON.stringify({...report,version:1,verified:true}));
}catch(e){await client.query('ROLLBACK');throw e;}finally{client.release();await db.close();}
