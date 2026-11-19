import assert from 'node:assert/strict';
import {mkdir,writeFile} from 'node:fs/promises';
import {openDatabase} from '../server/database.mjs';

const ids=['NoticeButton','AffinityButton','FriendsButton','MailButton','SettingsButton'];
const db=await openDatabase(),c=await db.pool.connect();
try{
 await c.query('BEGIN');
 await c.query('ALTER TABLE gamecli_ui.templates ADD COLUMN IF NOT EXISTS deprecated_at timestamptz');
 const before=(await c.query('SELECT * FROM gamecli_ui.templates WHERE id=ANY($1::text[]) ORDER BY id FOR UPDATE',[ids])).rows;
 assert.equal(before.length,ids.length);
 const directory=new URL('../.data/deprecated-controls-'+Date.now()+'/',import.meta.url);
 await mkdir(directory,{recursive:true});
 await writeFile(new URL('before.json',directory),JSON.stringify(before,null,2));
 await c.query("SELECT set_config('gamecli.publish','approved',true)");
 const result=await c.query('UPDATE gamecli_ui.templates SET deprecated_at=now(),version=version+1,updated_at=now() WHERE id=ANY($1::text[]) AND deprecated_at IS NULL RETURNING *',[ids]);
 for(const row of result.rows){const previous=before.find(b=>b.id===row.id);for(const key of ['payload','scripts','configurations'])assert.deepEqual(row[key],previous[key]);}
 await c.query('COMMIT');
 console.log(JSON.stringify({deprecated:result.rows.map(r=>({id:r.id,version:r.version})),retained:before.length,backup:directory.pathname}));
}catch(e){await c.query('ROLLBACK');throw e;}finally{c.release();await db.close();}
