// Native Unity export -> production Start template. Never changes the source prefab.
import {readFile,writeFile} from 'node:fs/promises';
import {join} from 'node:path';
import {createHash} from 'node:crypto';
import {isDeepStrictEqual} from 'node:util';
import {normalizeRect,resolveLayout} from '../shared/rect-transform.mjs';
import {validateTemplate} from '../shared/templates.mjs';
import {openDatabase} from '../server/database.mjs';
const project='G:/Works/SV/Development/Project',folder='.data/imports/sv-start';
const source=JSON.parse(await readFile(join(folder,'unity/unity-prefab.json'),'utf8'));
if(source.sourcePath!=='Assets/data/@00Launch/Start.prefab')throw new Error('Wrong source prefab');
const excluded=source.nodes.filter(n=>n.path==='0/1'||n.path.startsWith('0/1/'));
if(excluded[0]?.name!=='DebugGMPanel')throw new Error('Source hierarchy changed');
const production=source.nodes.filter(n=>!excluded.includes(n)),assets=new Map(),nodes=[];
const names={BackgroundView:'Background',BackgroundImage:'Image',BackgroundSpine:'Spine',BackgroundVideo:'Video',NoticeLst:'Notices',_RImage_LoginBg:'Backdrop',NoticeCN:'China',NoticeJP:'Japan',NoticeKR:'Korea',NoticeTW:'Taiwan',LoadingView:'Loading',UpdateView:'Update',VideoView:'Intro',PrivacyAgreementView:'Privacy',eff_jiantou_01:'Arrow',_TipText:'Tip',HAIKYU:'Haikyu',ButtomGather:'Buttons',MSDK_CN:'Agreement'};
const id=path=>'Start'+path.replaceAll('/','_');
const notes=[];
for(const n of production){
 const components=n.components.map(c=>({...c,data:Object.values(JSON.parse(c.json))[0]}));
 const graphic=components.find(c=>c.data?.m_Color&&('m_FontData'in c.data||'m_Sprite'in c.data||'m_Texture'in c.data));
 const button=components.find(c=>c.type.startsWith('UnityEngine.UI.Button,'));
 const g=graphic?.data,r=normalizeRect(n.rectTransform);
 const name=names[n.name]??n.name.replace(/[^a-zA-Z0-9]+(.)?/g,(_,c)=>c?c.toUpperCase():'').replace(/^./,c=>c.toUpperCase());
 const node={id:id(n.path),name,parent:n.path==='0'?null:id(n.parent),type:g?.m_FontData?'Text':button?'Button':'Container',x:0,y:0,width:1,height:1,color:g?'#'+['r','g','b'].map(k=>Math.round(g.m_Color[k]*255).toString(16).padStart(2,'0')).join(''):'#ffffff',text:(g?.m_Text??'').replace(/<[^>]*>/g,''),fontSize:g?.m_FontData?.m_FontSize??20,active:n.activeSelf,rectTransform:r,localScale:{x:n.rectTransform.localScale.x,y:n.rectTransform.localScale.y},rotation:n.rectTransform.localEulerAngles.z};
 if(graphic&&!g.m_FontData&&!button&&!production.some(v=>v.parent===n.path))node.type='Image';
 if(!graphic)node.surface='transparent';
 if(button)node.interactable=button.data.m_Interactable;
 if(g){node.opacity=g.m_Color.a;if(g.m_FontData){node.textAlign=['left','center','right'][g.m_FontData.m_Alignment%3];node.verticalAlign=['start','center','end'][Math.floor(g.m_FontData.m_Alignment/3)];}}
 const ref=graphic?.references.find(r=>['m_Sprite','m_Texture'].includes(r.property)&&r.path);
 if(ref){
  if(!ref.path.endsWith('.png'))throw new Error('Unsupported image: '+ref.path);
  const body=await readFile(join(project,ref.path)),hash=createHash('sha256').update(body).digest('hex');
  assets.set(hash,{body,path:ref.path});node.image=hash;node.imageFit=g.m_PreserveAspect?'contain':'100% 100%';
  if(g.m_Type===1){const meta=await readFile(join(project,ref.path+'.meta'),'utf8');const border=meta.match(/spriteBorder: \{x: ([\d.]+), y: ([\d.]+), z: ([\d.]+), w: ([\d.]+)\}/);if(!border)throw new Error('Missing sprite border: '+ref.path);node.imageBorder=Object.fromEntries(['x','y','z','w'].map((k,i)=>[k,Number(border[i+1])]));node.imageType='sliced';}
  if(g.m_UVRect)node.uvRect=Object.fromEntries(['x','y','width','height'].map(k=>[k,g.m_UVRect[k]]));
 }
 if(n.path==='0'){node.localScale={x:1,y:1};node.rectTransform={anchorMin:{x:0,y:0},anchorMax:{x:1,y:1},pivot:{x:.5,y:.5},anchoredPosition:{x:0,y:0},sizeDelta:{x:0,y:0}};}
 // Launcher scene's non-root production override (NoticeCN RectTransform).
 if(n.path==='0/0/3/1')node.rectTransform.anchoredPosition.y=79.62781;
 nodes.push(node);
}
let payload=validateTemplate({root:id('0'),design:resolveLayout({schemaVersion:1,name:'SvStart',width:1920,height:1080,nodes})},'system');
const evidence=JSON.parse(await readFile('.data/imports/sv-loading/loading-animation.json','utf8'));
const clipBytes=await readFile(join(project,evidence.source.clip));
if(createHash('sha256').update(clipBytes).digest('hex')!==evidence.source.clipSha256)throw new Error('Loading animation changed; extract again');
const host=payload.design.nodes.find(n=>n.animationHost);
const resolvePath=(parent,path)=>{let node=production.find(n=>n.path===parent);for(const name of path.split('/'))node=production.find(n=>n.parent===node?.path&&n.name===name);return node;};
payload.design.animations=[];
for(const base of ['0/2','0/3/2']){
 const a=structuredClone(evidence.designAnimationsProposal[0]);delete a.hostSelector;a.id=id(base)+'Loop';a.hostNodeId=host.id;a.source={clip:evidence.source.clip,clipGuid:evidence.source.clipGuid,originalHost:base};
 for(const curve of a.clip.curves){const target=resolvePath(base,curve.sourcePath);if(!target)throw new Error('Missing animation target');curve.targetNodeId=id(target.path);delete curve.bindingPathPreview;}
 payload.design.animations.push(a);
}
notes.push('Root Canvas is normalized to a full-stretch system root at CanvasScaler reference resolution 1920x1080; original prefab root has zero scale/size outside its Canvas context.');
notes.push('Launcher NoticeCN anchoredPosition.y=79.62781 applied; DebugGMPanel Menu override is excluded with debug tooling.');
payload={...validateTemplate(payload,'system'),source:{project,path:source.sourcePath,scene:'Assets/Launcher.unity',guid:source.sourceGuid,mode:'unity-native-production-hierarchy',nodeMap:production.map(n=>({id:id(n.path),path:n.path,name:n.name,rectTransform:n.rectTransform})),excluded:excluded.map(n=>({path:n.path,name:n.name,reason:'Development-only DebugGMPanel'})),normalization:notes},limitations:[
 'Preserves prefab activeSelf flags: Loading, Update, Intro and Privacy are initially hidden and activated by original startup scripts.',
 'Uses the original Jp/LauncherBg.png reference; build-time locale resource replacement is not executed.',
 'Spine/video players, SDK, downloads, privacy consent, hyperlink navigation, dynamic layouts and localization scripts are not executed in Studio.',
 'Text uses browser fonts and plain text; Unity rich text, masks and scroll behavior are not fully reproduced for hidden subviews.',
 'Two valid LoadingLoop bindings are extracted. The Update root controller has unmatched Ball/RateBg paths; the hidden crow sprite-frame animation is retained only in native source evidence.'
 ]};
await writeFile(join(folder,'template.json'),JSON.stringify(payload,null,2));
if(!process.argv.includes('--publish')){console.log(JSON.stringify({nodes:payload.design.nodes.length,images:assets.size,animations:payload.design.animations.length,excluded:excluded.length}));process.exit(0);}
const db=await openDatabase(),client=await db.pool.connect();
try{
 await client.query('BEGIN');
 await client.query("INSERT INTO gamecli_ui.template_groups(category_id,id,name,sort) VALUES('system','startup','启动',5) ON CONFLICT DO NOTHING");
 for(const [hash,a]of assets)await client.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT(id) DO NOTHING',[hash,'image/png',a.body,a.path]);
 await client.query('INSERT INTO gamecli_ui.templates(id,category_id,group_id,name,description,sort,version,payload) VALUES($1,$2,$3,$4,$5,$6,1,$7::jsonb)',['SvStart','system','startup','SV Start','1920 × 1080 · 启动界面 · 原始资源与阶段子界面',5,JSON.stringify(payload)]);
 const saved=(await client.query('SELECT payload FROM gamecli_ui.templates WHERE id=$1',['SvStart'])).rows[0].payload;if(!isDeepStrictEqual(saved,payload))throw new Error('Database readback mismatch');
 await client.query('COMMIT');console.log(JSON.stringify({template:'SvStart',version:1,nodes:payload.design.nodes.length,images:assets.size,animations:2,verified:true}));
}catch(e){await client.query('ROLLBACK');throw e;}finally{client.release();await db.close();}
