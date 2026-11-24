import {readFile} from 'node:fs/promises';
import {createHash} from 'node:crypto';
import {openDatabase} from '../../../../integrations/codex/server/database.mjs';

const mode=process.argv[2];
if(!['--check','--sync'].includes(mode)) throw new Error('Usage: node resource-db.mjs --check|--sync [catalog.json]');
let db,client;
async function inspect() {
  const found=await client.query("SELECT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname='miniwow_resources') AS exists");
  if(!found.rows[0].exists)return false;
  const marker=await client.query("SELECT to_regclass('miniwow_resources.catalog_meta') AS marker");
  if(!marker.rows[0].marker)throw new Error('Existing schema has no ownership marker; refusing changes.');
  const meta=await client.query('SELECT * FROM miniwow_resources.catalog_meta');
  if(meta.rows.length!==1 || meta.rows[0].schema_version!==1 || meta.rows[0].owner_tag!=='miniwow-resource-catalog')throw new Error('Existing catalog schema is incompatible.');
  return true;
}
function validate(catalog) {
  const sets={};
  for(const name of ['sources','assets','exports']) {
    if(!Array.isArray(catalog[name]))throw new Error(`Missing ${name}`);
    sets[name]=new Set();
    for(const row of catalog[name]) {
      if(typeof row.id!=='string'||!row.id||sets[name].has(row.id))throw new Error(`Invalid/duplicate ${name} ID`);
      sets[name].add(row.id);
    }
  }
  for(const row of catalog.assets)if(!sets.sources.has(row.source_id))throw new Error('Dangling asset source');
  for(const row of catalog.exports)if(!sets.assets.has(row.asset_id))throw new Error('Dangling export asset');
}
try {
  let raw,catalog;
  if(mode==='--sync') {
    raw=await readFile(process.argv[3]||new URL('../research/resource-catalog/catalog.json',import.meta.url),'utf8');
    catalog=JSON.parse(raw);validate(catalog);
  }
  db=await openDatabase();client=await db.pool.connect();
  await client.query(mode==='--check'?'BEGIN READ ONLY':'BEGIN');
  if(mode==='--sync')await client.query("SELECT pg_advisory_xact_lock(hashtext('miniwow-resource-catalog'))");
  const exists=await inspect();
  if(mode==='--check') {
    const result={connected:true,schemaExists:exists,schemaVersion:exists?1:null};
    if(exists)result.counts=(await client.query('SELECT (SELECT count(*) FROM miniwow_resources.sources) AS sources,(SELECT count(*) FROM miniwow_resources.assets) AS assets,(SELECT count(*) FROM miniwow_resources.exports) AS exports,(SELECT count(*) FROM miniwow_resources.annotations) AS annotations')).rows[0];
    console.log(JSON.stringify(result));
  } else {
    if(!exists)await client.query(await readFile(new URL('./resource-schema.sql',import.meta.url),'utf8'));
    for(const name of ['sources','assets','exports']) {
      for(let offset=0;offset<catalog[name].length;offset+=250) {
        const rows=catalog[name].slice(offset,offset+250);
        const link=name==='assets'?'source_id':name==='exports'?'asset_id':null;
        const columns=link?`id,${link},payload`:'id,payload';
        const selection=link?`x->>'id',x->>'${link}',x`:`x->>'id',x`;
        const update=link?`${link}=excluded.${link},payload=excluded.payload`:'payload=excluded.payload';
        await client.query(`INSERT INTO miniwow_resources.${name} (${columns}) SELECT ${selection} FROM jsonb_array_elements($1::jsonb) x ON CONFLICT(id) DO UPDATE SET ${update}`,[JSON.stringify(rows)]);
      }
    }
    await client.query('INSERT INTO miniwow_resources.runs(catalog_sha256,summary) VALUES($1,$2)',[createHash('sha256').update(raw).digest('hex'),JSON.stringify(catalog.summary||{})]);
    console.log(JSON.stringify({synchronized:true,sources:catalog.sources.length,assets:catalog.assets.length,exports:catalog.exports.length,annotationsPreserved:true}));
  }
  await client.query('COMMIT');
} catch(error) {
  if(client)await client.query('ROLLBACK').catch(()=>{});
  // Connection details and SQL parameter values are intentionally not printed.
  console.error(JSON.stringify({ok:false,code:error.code||null,message:error.message.replace(/(?:postgres(?:ql)?:\/\/)[^\s]+/gi,'[redacted]')}));
  process.exitCode=1;
} finally {client?.release();if(db)await db.close();}
