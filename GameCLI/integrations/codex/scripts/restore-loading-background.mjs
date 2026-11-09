import {readFile,writeFile} from 'node:fs/promises';
import {createHash} from 'node:crypto';
import {openDatabase} from '../server/database.mjs';
import {createStore} from '../server/store.mjs';
import {restoreLoadingBackground} from '../shared/loading-background.mjs';
import {validateTemplate} from '../shared/templates.mjs';
const source=JSON.parse(await readFile('.data/imports/sv-loading/unity/unity-prefab.json','utf8')).nodes.find(n=>n.name==='LoadingBg');
const path='Assets/data/UI/Public/@Texture/Loading[ASTC6x6]/bg_loading_cylogin.png';
const body=await readFile('G:/Works/SV/Development/Project/'+path),image=createHash('sha256').update(body).digest('hex');
const db=await openDatabase(),client=await db.pool.connect();
try{const old=(await client.query('SELECT * FROM gamecli_ui.templates WHERE id=$1',['SvLoading'])).rows[0];await writeFile(`.data/imports/sv-loading/before-background-v${old.version}.json`,JSON.stringify(old,null,2));const payload=old.payload,count=restoreLoadingBackground(payload.design,source,image);validateTemplate(payload);payload.source.runtimePreview={node:'LoadingBg',texture:path,reason:'LoadingViewLogic.LoadBg assigns loadingCfg.picName and activates the node; this texture is an explicit preview sample.'};await client.query('BEGIN');await client.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT(id) DO NOTHING',[image,'image/png',body,path]);const result=await client.query('UPDATE gamecli_ui.templates SET payload=$1::jsonb,description=$2,version=version+1,updated_at=now() WHERE id=$3 AND version=$4 RETURNING version',[JSON.stringify(payload),'1920 × 1080 · SV 原图预览 · RectTransform','SvLoading',old.version]);if(result.rowCount!==1)throw new Error('Template version changed');await client.query('COMMIT');await writeFile('.data/imports/sv-loading/active.json',JSON.stringify(payload,null,2));console.log('Template',result.rows[0],'added',count);}catch(e){await client.query('ROLLBACK');throw e;}finally{client.release();await db.close();}
const store=createStore(),snap=await store.snapshot();await writeFile(`.data/imports/sv-loading/draft-before-background-${snap.revision}.json`,JSON.stringify(snap,null,2));const before=structuredClone(snap.design.nodes);const count=restoreLoadingBackground(snap.design,source,image);
for(const n of before){const after=snap.design.nodes.find(v=>v.id===n.id);if(JSON.stringify(n)!==JSON.stringify(after))throw new Error('Existing node changed: '+n.id);}
if(count)await store.save(snap.design,snap.revision);console.log('Draft added',count,'existing nodes unchanged',before.length);
const response=await fetch('http://127.0.0.1:18091/api/studio/assets/'+image);if(!response.ok)throw new Error('Image HTTP '+response.status);console.log('Image HTTP',response.status,'bytes',(await response.arrayBuffer()).byteLength);
