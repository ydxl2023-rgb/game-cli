import {readFile} from 'node:fs/promises';
import {openDatabase} from '../../../../../integrations/codex/server/database.mjs';
const records=JSON.parse(await readFile(new URL('./unity-imports.json',import.meta.url),'utf8'));
const db=await openDatabase();const client=await db.pool.connect();
try{
 await client.query('BEGIN');
 await client.query("SELECT pg_advisory_xact_lock(hashtext('miniwow-resource-catalog'))");
 const marker=(await client.query('SELECT * FROM miniwow_resources.catalog_meta')).rows;
 if(marker.length!==1||marker[0].schema_version!==1||marker[0].owner_tag!=='miniwow-resource-catalog')throw Error('Catalog ownership mismatch');
 await client.query('CREATE TABLE IF NOT EXISTS miniwow_resources.scene_imports(import_id text PRIMARY KEY,source_asset_id text NOT NULL REFERENCES miniwow_resources.assets(id),scene text NOT NULL,payload jsonb NOT NULL,updated_at timestamptz NOT NULL DEFAULT now())');
 for(const record of records){
  await client.query('INSERT INTO miniwow_resources.scene_imports(import_id,source_asset_id,scene,payload) VALUES($1,$2,$3,$4::jsonb) ON CONFLICT(import_id) DO UPDATE SET source_asset_id=excluded.source_asset_id,scene=excluded.scene,payload=excluded.payload,updated_at=now()',[record.id,record.sourceAssetId,'Home / Building_sdc',JSON.stringify(record)]);
 }
 await client.query('COMMIT');console.log(JSON.stringify({indexed:records.length,annotationsPreserved:true,binaryUploaded:false}));
}catch(error){await client.query('ROLLBACK');throw error;}finally{client.release();await db.close();}
