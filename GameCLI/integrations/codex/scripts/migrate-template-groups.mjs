import {writeFile} from 'node:fs/promises';
import {openDatabase} from '../server/database.mjs';
import {seedTemplates} from '../server/templates.mjs';
const db=await openDatabase();try{
 const before=(await db.pool.query('SELECT * FROM gamecli_ui.templates ORDER BY id')).rows;
 await writeFile(`.data/template-history/before-groups-${Date.now()}.json`,JSON.stringify(before,null,2));
 await seedTemplates(db.pool);
 const after=(await db.pool.query('SELECT * FROM gamecli_ui.templates ORDER BY id')).rows;
 for(const old of before){const n=after.find(n=>n.id===old.id);if(JSON.stringify(old.payload)!==JSON.stringify(n.payload)||old.version!==n.version)throw new Error('Template content changed: '+old.id);}
 console.log(JSON.stringify((await db.pool.query('SELECT t.id,c.name AS category,g.name AS group_name,t.version FROM gamecli_ui.templates t JOIN gamecli_ui.template_categories c ON c.id=t.category_id JOIN gamecli_ui.template_groups g ON g.category_id=t.category_id AND g.id=t.group_id ORDER BY c.sort,g.sort,t.sort')).rows));
 await seedTemplates(db.pool);console.log('Migration rerun passed; payload and versions unchanged');
 const client=await db.pool.connect();try{await client.query('BEGIN');let rejected=false;try{await client.query("UPDATE gamecli_ui.templates SET group_id='item' WHERE id='SvLoading'");}catch(e){if(e.code==='23503')rejected=true;else throw e;}if(!rejected)throw new Error('Cross-category group accepted');console.log('Cross-category foreign key verified');}finally{await client.query('ROLLBACK');client.release();}
}finally{await db.close();}
