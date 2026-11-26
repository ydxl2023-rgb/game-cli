import {readFile, writeFile} from 'node:fs/promises';
import {openDatabase} from '../../../../../integrations/codex/server/database.mjs';

const records = JSON.parse(await readFile(new URL('./third-imports.json', import.meta.url), 'utf8'));
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
        [record.id, record.sourceAssetId, 'Home opening / task 60003', JSON.stringify(record)]);
    }
    const owner = (await client.query("SELECT obj_description('miniwow_resources.development_tasks'::regclass) AS marker")).rows[0];
    if (owner.marker !== 'miniwow-development-tasks/v1') throw Error('Development task ownership mismatch');
    const note = '2026-11-26 60003：原医院目标导航，清理3秒→priest_initial_3→独立救人1秒→自动领取60003→跟踪60004并切回priest_initial。清理过程中退出、清理后重启、救人移动中退出、自动领取后重启均通过。烟尘使用原贴图的本地粒子适配，原动画和世界气泡未完全还原；未实现60004英雄招募。';
    const evidence = ['tools/opening/THIRD-RESCUE.md', 'tools/opening/third-imports.json',
        'tools/opening/third-evidence.json', 'tools/opening/third-export.log'];
    await client.query(`UPDATE miniwow_resources.development_tasks
        SET status='review',progress_notes=concat_ws(E'\n',nullif(progress_notes,''),$1::text),
            completion_evidence=completion_evidence || $2::jsonb,progress_updated_at=now()
        WHERE id=ANY($3::text[]) AND position($1 in coalesce(progress_notes,''))=0`,
    [note, JSON.stringify(evidence), ['MW-013']]);
    await client.query('COMMIT');
    const result = {imports: records.length, annotationsPreserved: true, binaryUploaded: false};
    await writeFile(new URL('./third-database.json', import.meta.url), JSON.stringify(result, null, 2));
    console.log(JSON.stringify(result));
} catch (error) {
    await client.query('ROLLBACK');
    throw error;
} finally {
    client.release();
    await db.close();
}
