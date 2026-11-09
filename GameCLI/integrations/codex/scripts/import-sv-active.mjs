import {restoreLoadingBackground} from '../shared/loading-background.mjs';
import {groupLoadingFooter} from '../shared/loading-hierarchy.mjs';
import {readFile,writeFile} from 'node:fs/promises';
import {createHash} from 'node:crypto';
import {join} from 'node:path';
import {normalizeRect,resolveLayout} from '../shared/rect-transform.mjs';
import {validateTemplate} from '../shared/templates.mjs';
import {openDatabase} from '../server/database.mjs';
import {createStore} from '../server/store.mjs';
const project='G:/Works/SV/Development/Project';
const source=JSON.parse(await readFile('.data/imports/sv-loading/unity/unity-prefab.json','utf8'));
const map=new Map(source.nodes.map(n=>[n.path,n]));
function active(n){return n.activeSelf && (!map.has(n.parent)||active(map.get(n.parent)));}
const visible=source.nodes.filter(active), assets=[],nodes=[];
for(const n of visible){
 const graphic=n.components.map(c=>({...c,data:JSON.parse(c.json).MonoBehaviour})).find(c=>c.data?.m_Color && (c.data.m_FontData || c.type.startsWith('UnityEngine.UI.Image,') || c.type.startsWith('UnityEngine.UI.RawImage,')));
 const g=graphic?.data,children=visible.some(v=>v.parent===n.path);
 const name=({'_TipText':'TipText','eff_jiantou_01':'Arrow','HAIKYU':'Haikyu'})[n.name]??n.name;
 const node={id:'N'+n.path.replaceAll('/','_'),name,parent:map.has(n.parent)?'N'+n.parent.replaceAll('/','_'):null,type:g?.m_FontData?'Text':children||!graphic?'Container':'Image',x:0,y:0,width:1,height:1,color:g?'#'+['r','g','b'].map(k=>Math.round(g.m_Color[k]*255).toString(16).padStart(2,'0')).join(''):'#ffffff',text:g?.m_Text??'',fontSize:g?.m_FontData?.m_FontSize??20,rectTransform:normalizeRect(n.rectTransform),localScale:{x:n.rectTransform.localScale.x,y:n.rectTransform.localScale.y}};
 if(!graphic)node.surface='transparent';
 if(g){node.opacity=g.m_Color.a;if(g.m_FontData){node.textAlign=['left','center','right'][g.m_FontData.m_Alignment%3];node.verticalAlign=['start','center','end'][Math.floor(g.m_FontData.m_Alignment/3)];}}
 const ref=graphic?.references.find(r=>['m_Sprite','m_Texture'].includes(r.property)&&r.path);
 if(ref){const body=await readFile(join(project,ref.path)),id=createHash('sha256').update(body).digest('hex');assets.push({id,body,path:ref.path});node.image=id;node.imageFit='100% 100%';
  if(g.m_Type===1){const meta=await readFile(join(project,ref.path+'.meta'),'utf8');const m=meta.match(/spriteBorder: \{x: ([\d.]+), y: ([\d.]+), z: ([\d.]+), w: ([\d.]+)\}/);if(!m)throw new Error('Missing sprite border');node.imageBorder=Object.fromEntries(['x','y','z','w'].map((k,i)=>[k,Number(m[i+1])]));}
  if(g.m_UVRect)node.uvRect=Object.fromEntries(['x','y','width','height'].map(k=>[k,g.m_UVRect[k]]));
 }
 nodes.push(node);
}
const design=resolveLayout({schemaVersion:1,name:'SvLoading',width:1920,height:1080,nodes});
groupLoadingFooter(design);
const previewPath='Assets/data/UI/Public/@Texture/Loading[ASTC6x6]/bg_loading_cylogin.png';
const previewBody=await readFile(join(project,previewPath)),previewId=createHash('sha256').update(previewBody).digest('hex');
assets.push({id:previewId,body:previewBody,path:previewPath});
restoreLoadingBackground(design,source.nodes.find(n=>n.name==='LoadingBg'),previewId);
const payload={...validateTemplate({root:'N0',design},'system'),source:{project,path:source.sourcePath,mode:'unity-active-hierarchy',runtimePreview:{node:'LoadingBg',texture:previewPath,reason:'Explicit preview sample for loadingCfg.picName; activated by LoadingViewLogic.LoadBg at runtime.'},hierarchyOverride:'TipText, Ball, Haikyu and Rate grouped under visible RateBg; visual bounds preserved.',nodeMap:visible.map(n=>({id:'N'+n.path.replaceAll('/','_'),path:n.path,name:n.name})),excluded:source.nodes.filter(n=>!active(n)).map(n=>n.path)},limitations:['Static prefab active hierarchy; runtime scripts and animation are not executed.','Text uses browser fonts; Unity best-fit and localization are not simulated.']};
await writeFile('.data/imports/sv-loading/active.json',JSON.stringify(payload,null,2));
if(!process.argv.includes('--publish')){console.log(JSON.stringify({nodes:nodes.length,excluded:source.nodes.length-nodes.length,rateBg:nodes.find(n=>n.name==='RateBg')}));process.exit(0);}
const db=await openDatabase();const client=await db.pool.connect();
try{const current=(await client.query('SELECT * FROM gamecli_ui.templates WHERE id=$1',['SvLoading'])).rows[0];await writeFile(`.data/imports/sv-loading/before-active-v${current.version}.json`,JSON.stringify(current,null,2));await client.query('BEGIN');for(const a of assets)await client.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT(id) DO NOTHING',[a.id,'image/png',a.body,a.path]);const result=await client.query('UPDATE gamecli_ui.templates SET payload=$1::jsonb,description=$2,version=version+1,updated_at=now() WHERE id=$3 AND version=$4 RETURNING version',[JSON.stringify(payload),'1920 × 1080 · 原始可见节点 · RectTransform','SvLoading',current.version]);if(result.rowCount!==1)throw new Error('Template changed');await client.query('COMMIT');console.log('Published',result.rows[0]);}catch(e){await client.query('ROLLBACK');throw e;}finally{client.release();await db.close();}
if(process.argv.includes('--draft')){
 const store=createStore(),snapshot=await store.snapshot();await writeFile(`.data/imports/sv-loading/draft-before-active-${snapshot.revision}.json`,JSON.stringify(snapshot,null,2));
 const d=snapshot.design,root=d.nodes.find(n=>n.id==='T1Loading');if(!root)throw new Error('Expected Loading instance missing; draft unchanged');const remove=new Set([root.id]);let changed=true;while(changed){changed=false;for(const n of d.nodes)if(remove.has(n.parent)&&!remove.has(n.id)){remove.add(n.id);changed=true;}}
 const replacement=structuredClone(payload.design.nodes);const remap=new Map(replacement.map(n=>[n.id,n.id==='N0'?root.id:'SvVisible'+n.id]));for(const n of replacement){n.id=remap.get(n.id);n.parent=n.parent===null?root.parent:remap.get(n.parent);if(d.nodes.some(old=>!remove.has(old.id)&&old.id===n.id))throw new Error('ID collision');}
 d.nodes=d.nodes.filter(n=>!remove.has(n.id));d.nodes.push(...replacement);for(const n of d.nodes)if(remove.has(n.action?.target)&&n.action.target!==root.id)delete n.action;
 await store.save(d,snapshot.revision);console.log('Draft updated');
}
