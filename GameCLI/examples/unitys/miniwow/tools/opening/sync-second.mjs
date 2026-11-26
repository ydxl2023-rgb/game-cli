import {readFile, writeFile} from 'node:fs/promises';
import {openDatabase} from '../../../../../integrations/codex/server/database.mjs';

const records = JSON.parse(await readFile(new URL('./second-imports.json', import.meta.url), 'utf8'));
const db = await openDatabase();
const client = await db.pool.connect();
try {
    await client.query('BEGIN');
    const marker = (await client.query('SELECT * FROM miniwow_resources.catalog_meta')).rows;
    if (marker.length !== 1 || marker[0].owner_tag !== 'miniwow-resource-catalog' || marker[0].schema_version !== 1)
        throw Error('Resource catalog ownership mismatch');
    for (const record of records) {
        await client.query(`INSERT INTO miniwow_resources.scene_imports(import_id,source_asset_id,scene,payload)
            VALUES($1,$2,$3,$4::jsonb) ON CONFLICT(import_id) DO UPDATE SET
            source_asset_id=excluded.source_asset_id,scene=excluded.scene,payload=excluded.payload,updated_at=now()`,
        [record.id, record.sourceAssetId, 'Home opening / task 60002', JSON.stringify(record)]);
    }
    const owner = (await client.query("SELECT obj_description('miniwow_resources.development_tasks'::regclass) AS marker")).rows[0];
    if (owner.marker !== 'miniwow-development-tasks/v1') throw Error('Development task ownership mismatch');
    const note = '2026-11-26 60002：原水井导航取水10→旅店5→原3053对白→医院5→领取60003；旅店原初始模型、店主ActorShape、医院priest_initial_2已接通。旅店对白重启保留5水、医院途中退出不扣水、未领取和已领取重启均通过。原资源飞行动画、世界气泡及全身对白布局尚未还原。';
    const evidence = ['tools/opening/SECOND-RESCUE.md', 'tools/opening/second-imports.json',
        'tools/opening/second-evidence.json', 'tools/opening/second-export.log'];
    await client.query(`UPDATE miniwow_resources.development_tasks
        SET status='review',progress_notes=concat_ws(E'\n',nullif(progress_notes,''),$1::text),
            completion_evidence=completion_evidence || $2::jsonb,progress_updated_at=now()
        WHERE id=ANY($3::text[]) AND position($1 in coalesce(progress_notes,''))=0`,
    [note, JSON.stringify(evidence), ['MW-012']]);
    await client.query('COMMIT');
    const result = {imports: records.length, annotationsPreserved: true, binaryUploaded: false};
    await writeFile(new URL('./second-database.json', import.meta.url), JSON.stringify(result, null, 2));
    console.log(JSON.stringify(result));
} catch (error) {
    await client.query('ROLLBACK');
    throw error;
} finally {
    client.release();
    await db.close();
}
