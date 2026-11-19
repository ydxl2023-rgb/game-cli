import {openDatabase} from '../server/database.mjs';
import {writeFile,mkdir} from 'node:fs/promises';
const db=await openDatabase();try{await mkdir('.data/common-micros',{recursive:true});const rows=(await db.pool.query('SELECT * FROM gamecli_ui.templates ORDER BY id')).rows;await writeFile('.data/common-micros/templates-before.json',JSON.stringify(rows));console.log(JSON.stringify(rows.filter(r=>JSON.stringify([r.payload,r.scripts]).includes('Widgets/Micros')).map(r=>({id:r.id,version:r.version}))));}finally{await db.close();}
