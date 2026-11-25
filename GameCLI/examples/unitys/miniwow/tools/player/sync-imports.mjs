import { readFile } from 'node:fs/promises';
import { createHash } from 'node:crypto';
import { openDatabase } from '../../../../../integrations/codex/server/database.mjs';

const records = JSON.parse(await readFile(new URL('./resources.json', import.meta.url), 'utf8'));
const db = await openDatabase();
const client = await db.pool.connect();
try {
  await client.query('BEGIN');
  await client.query("SELECT pg_advisory_xact_lock(hashtext('miniwow-resource-catalog'))");
  const marker = (await client.query('SELECT * FROM miniwow_resources.catalog_meta')).rows;
  if (marker.length !== 1 || marker[0].schema_version !== 1 || marker[0].owner_tag !== 'miniwow-resource-catalog') {
    throw Error('Catalog ownership mismatch');
  }
  for (const record of records) {
    const root = new URL('../../', import.meta.url);
    const sourceHash = createHash('sha256').update(await readFile(new URL(record.source, root))).digest('hex');
    const targetHash = createHash('sha256').update(await readFile(new URL(record.target, root))).digest('hex');
    if (record.sourceAssetIds.length !== 1 || sourceHash !== record.sha256 || targetHash !== record.targetSha256 || !record.conversion) {
      throw Error('Source identity or target hash mismatch: ' + record.target);
    }
    const id = createHash('sha256').update('player:' + record.target).digest('hex');
    const payload = { ...record, name: record.purpose, unityPath: record.destination, uses: ['TownPlayer / PlayerInputCanvas'] };
    await client.query('INSERT INTO miniwow_resources.scene_imports(import_id,source_asset_id,scene,payload) VALUES($1,$2,$3,$4::jsonb) ON CONFLICT(import_id) DO UPDATE SET source_asset_id=excluded.source_asset_id,scene=excluded.scene,payload=excluded.payload,updated_at=now()', [id, record.sourceAssetIds[0], 'Home / TownPlayer', JSON.stringify(payload)]);
  }
  await client.query('COMMIT');
  console.log(JSON.stringify({ indexed: records.length, annotationsPreserved: true, binaryUploaded: false }));
} catch (error) {
  await client.query('ROLLBACK');
  throw error;
} finally {
  client.release();
  await db.close();
}
