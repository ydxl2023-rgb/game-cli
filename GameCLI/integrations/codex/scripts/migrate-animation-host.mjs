import {writeFile} from 'node:fs/promises';
import {openDatabase} from '../server/database.mjs';
import {createStore} from '../server/store.mjs';
import {ensureAnimationHost} from '../shared/animation-host.mjs';
import {validateTemplate} from '../shared/templates.mjs';
const db=await openDatabase(),client=await db.pool.connect();
try{await client.query('BEGIN');const rows=(await client.query("SELECT * FROM gamecli_ui.templates WHERE category_id='system' FOR UPDATE")).rows;for(const old of rows){await writeFile(`.data/imports/sv-loading/before-animator-${old.id}-v${old.version}.json`,JSON.stringify(old,null,2));const normalized=validateTemplate(old.payload,'system');const payload={...old.payload,...normalized,standard:{...old.payload.standard,animationHost:'Animator',hierarchyVersion:1}};if(JSON.stringify(payload)!==JSON.stringify(old.payload)){await client.query('UPDATE gamecli_ui.templates SET payload=$1::jsonb,version=version+1,updated_at=now() WHERE id=$2',[JSON.stringify(payload),old.id]);console.log('Standardized',old.id);}}await client.query('COMMIT');}catch(e){await client.query('ROLLBACK');throw e;}finally{client.release();await db.close();}
const store=createStore(),snap=await store.snapshot();await writeFile(`.data/imports/sv-loading/draft-before-animator-${snap.revision}.json`,JSON.stringify(snap,null,2));const before=structuredClone(snap.design.nodes);for(const root of snap.design.nodes.filter(n=>n.name==='Loading'&&n.parent===null))ensureAnimationHost(snap.design,root.id);
for(const n of before){const now=snap.design.nodes.find(v=>v.id===n.id);for(const key of ['x','y','width','height','text','image','rectTransform'])if(JSON.stringify(now[key])!==JSON.stringify(n[key]))throw new Error('Unexpected change '+n.id+' '+key);}
await store.save(snap.design,snap.revision);console.log('Draft saved; original geometry and content unchanged');
