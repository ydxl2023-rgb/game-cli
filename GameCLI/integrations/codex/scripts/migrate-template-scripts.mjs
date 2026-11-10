import {openDatabase} from '../server/database.mjs';
import {templateScriptsSchema} from '../server/template-scripts.mjs';

const db=await openDatabase();
try {
  await db.pool.query(templateScriptsSchema);
  console.log('gamecli_ui.templates.scripts is ready (nullable jsonb).');
} finally {await db.close();}
