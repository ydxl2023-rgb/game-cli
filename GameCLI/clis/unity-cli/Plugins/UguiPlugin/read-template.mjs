// Read raw database rows, not validateTemplate()'s normalized response. No writes to DB.
import { pathToFileURL } from 'node:url';
import { writeFile, mkdir } from 'node:fs/promises';
import { join } from 'node:path';
const [integration, id, output] = process.argv.slice(2);
const { openDatabase } = await import(pathToFileURL(join(integration, 'server/database.mjs')));
const db = await openDatabase();
const client = await db.pool.connect();
try {
  await client.query('BEGIN TRANSACTION ISOLATION LEVEL REPEATABLE READ READ ONLY');
  const result = await client.query('SELECT * FROM gamecli_ui.templates WHERE id=$1', [id]);
  if (result.rowCount !== 1) throw new Error('Template not found: ' + id);
  const record = result.rows[0];
  await mkdir(output, {recursive:true});
  const ids = [...new Set(record.payload.design.nodes.map(n=>n.image).filter(Boolean))];
  const assets = await client.query('SELECT id,mime,body,source_path FROM gamecli_ui.assets WHERE id=ANY($1::text[])', [ids]);
  if (assets.rowCount !== ids.length) throw new Error('Template has missing database assets.');
  for (const asset of assets.rows) {
    if (!/^[a-f0-9]{64}$/.test(asset.id) || asset.mime !== 'image/png') throw new Error('Unsupported database image: ' + asset.id);
    await writeFile(join(output, asset.id + '.png'), asset.body);
  }
  await writeFile(join(output, 'asset-names.json'), JSON.stringify(Object.fromEntries(assets.rows.map(a=>[a.id,a.source_path])),null,2));
  await writeFile(join(output, 'template.json'), JSON.stringify(record, null, 2));
  await client.query('COMMIT');
} finally { client.release(); await db.close(); }
