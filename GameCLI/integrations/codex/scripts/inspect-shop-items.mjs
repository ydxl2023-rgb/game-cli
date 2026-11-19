import {openDatabase} from '../server/database.mjs';
import {writeFile,mkdir} from 'node:fs/promises';
const db=await openDatabase();try{await mkdir('.data/shop-item-slot',{recursive:true});const row=(await db.pool.query("SELECT * FROM gamecli_ui.templates WHERE id='SvShop'")).rows[0];await writeFile('.data/shop-item-slot/before.json',JSON.stringify(row));console.log(JSON.stringify({version:row.version,paths:row.scripts.files.map(f=>f.path),config:row.payload.configurations.dependencies}));}finally{await db.close();}
