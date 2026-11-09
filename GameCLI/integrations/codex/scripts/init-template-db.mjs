import {openDatabase} from '../server/database.mjs';
import {seedTemplates} from '../server/templates.mjs';
const db=await openDatabase();
try {
  await seedTemplates(db.pool);
  const result=await db.pool.query('SELECT c.name AS category,t.name,t.version,jsonb_array_length(t.payload->\'design\'->\'nodes\') AS nodes FROM gamecli_ui.templates t JOIN gamecli_ui.template_categories c ON c.id=t.category_id ORDER BY c.sort,t.sort');
  console.log(JSON.stringify(result.rows,null,2));
} finally {await db.close();}
