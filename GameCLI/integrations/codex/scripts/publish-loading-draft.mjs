import {writeFile,mkdir} from 'node:fs/promises';
import {isDeepStrictEqual} from 'node:util';
import {createStore} from '../server/store.mjs';
import {openDatabase} from '../server/database.mjs';
import {validateTemplate} from '../shared/templates.mjs';
const store=createStore(),snap=await store.snapshot();
const roots=snap.design.nodes.filter(n=>n.name==='Loading'&&n.parent===null);if(roots.length!==1)throw new Error('Expected exactly one Loading root');const root=roots[0];
const ids=new Set([root.id]);let size;do{size=ids.size;for(const n of snap.design.nodes)if(ids.has(n.parent))ids.add(n.id);}while(ids.size!==size);
const design={...structuredClone(snap.design),name:'SvLoading',nodes:structuredClone(snap.design.nodes.filter(n=>ids.has(n.id)))};
if(design.animations)design.animations=design.animations.filter(a=>ids.has(a.hostNodeId));
for(const n of design.nodes)if(n.action?.target&&!ids.has(n.action.target))throw new Error('External action target: '+n.id);
const normalized=validateTemplate({root:root.id,design},'system');if(!isDeepStrictEqual(normalized.design.nodes,design.nodes))throw new Error('Template standardization would alter the edited design');
const db=await openDatabase(),client=await db.pool.connect();
try{
 await client.query('BEGIN');const current=(await client.query('SELECT * FROM gamecli_ui.templates WHERE id=$1 FOR UPDATE',['SvLoading'])).rows[0];if(!current)throw new Error('SvLoading not found');
 await mkdir('.data/template-history',{recursive:true});const backup=`.data/template-history/SvLoading-v${current.version}-${Date.now()}.json`;await writeFile(backup,JSON.stringify(current,null,2));
 const imageIds=[...new Set(design.nodes.map(n=>n.image).filter(Boolean))];const assets=await client.query('SELECT id FROM gamecli_ui.assets WHERE id=ANY($1::text[])',[imageIds]);if(assets.rowCount!==imageIds.length)throw new Error('Missing image resources');
 const payload={...current.payload,...normalized,editedFrom:{draftRevision:snap.revision,savedAt:snap.savedAt},standard:{...current.payload.standard,animationHost:'Animator',hierarchyVersion:1}};
 const latest=await store.snapshot();if(latest.revision!==snap.revision)throw new Error('Draft changed during publish; retry');
 const result=await client.query('UPDATE gamecli_ui.templates SET payload=$1::jsonb,description=$2,version=version+1,updated_at=now() WHERE id=$3 RETURNING version',[JSON.stringify(payload),`${design.width} × ${design.height} · Animator · 已编辑布局`,'SvLoading']);
 const saved=(await client.query('SELECT payload FROM gamecli_ui.templates WHERE id=$1',['SvLoading'])).rows[0].payload;if(!isDeepStrictEqual(saved.design,normalized.design))throw new Error('Database readback mismatch');
 await client.query('COMMIT');console.log(JSON.stringify({template:'SvLoading',database:'gamecli',version:result.rows[0].version,draftRevision:snap.revision,nodes:design.nodes.length,images:imageIds.length,width:design.width,height:design.height,backup,verified:true}));
}catch(e){await client.query('ROLLBACK');throw e;}finally{client.release();await db.close();}
