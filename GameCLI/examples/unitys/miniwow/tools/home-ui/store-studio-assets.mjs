import {readFile} from 'node:fs/promises';
import {createHash} from 'node:crypto';
import {openDatabase} from '../../../../../integrations/codex/server/database.mjs';
const hash=b=>createHash('sha256').update(b).digest('hex');
const manifest=JSON.parse(await readFile(new URL('./production-resources.json',import.meta.url),'utf8'));
const db=await openDatabase();const client=await db.pool.connect();
try{
 await client.query('BEGIN');
 for(const a of manifest){const bytes=await readFile(new URL('./assets/'+a.assetId+'.png',import.meta.url));if(hash(bytes)!==a.assetId)throw Error('PNG hash mismatch');await client.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT(id) DO NOTHING',[a.assetId,'image/png',bytes,a.source]);}
 for(const font of ['fangzhenglanting','hanyijingkai']){const bytes=await readFile(new URL('../../extracted-resources/assets/data/fonts/'+font+'.ttf',import.meta.url));await client.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT(id) DO NOTHING',[hash(bytes),'font/ttf',bytes,'miniwow/'+font+'.ttf']);}
 const workbook=await readFile(new URL('../../config/Home.xlsx',import.meta.url));await client.query('INSERT INTO gamecli_ui.configuration_assets(id,body) VALUES($1,$2) ON CONFLICT(id) DO NOTHING',[hash(workbook),workbook]);
 await client.query('COMMIT');console.log(JSON.stringify({images:manifest.length,fonts:2,workbook:hash(workbook)}));
}catch(e){await client.query('ROLLBACK');throw e;}finally{client.release();await db.close();}
