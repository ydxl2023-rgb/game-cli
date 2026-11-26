import {readFile,writeFile} from 'node:fs/promises';
import {openDatabase} from '../../../../../integrations/codex/server/database.mjs';

const validation=JSON.parse(await readFile(new URL('./configuration-validation.json',import.meta.url),'utf8'));
if(validation.exportStatus!=='passed') throw Error('Verified configuration required.');
const imports=JSON.parse(await readFile(new URL('./unity-imports.json',import.meta.url),'utf8'));
const db=await openDatabase();
const client=await db.pool.connect();
try {
    await client.query('BEGIN');
    const marker=(await client.query("SELECT obj_description('miniwow_resources.development_tasks'::regclass) AS marker")).rows[0];
    if(marker.marker!=='miniwow-development-tasks/v1') throw Error('Task table ownership mismatch.');
    const ownership=(await client.query('SELECT * FROM miniwow_resources.catalog_meta')).rows;
    if(ownership.length!==1 || ownership[0].owner_tag!=='miniwow-resource-catalog' || ownership[0].schema_version!==1)
        throw Error('Resource catalog ownership mismatch.');
    for(const record of imports) {
        await client.query(`INSERT INTO miniwow_resources.scene_imports(import_id,source_asset_id,scene,payload)
            VALUES($1,$2,$3,$4::jsonb) ON CONFLICT(import_id) DO UPDATE SET
            source_asset_id=excluded.source_asset_id,scene=excluded.scene,payload=excluded.payload,updated_at=now()`,
            [record.id,record.sourceAssetId,'Home / Hospital UI',JSON.stringify(record)]);
    }
    const note='2026-11-26 首级医院UI已实现：原作修复气泡、蓝色进入图标、内部Spine、生产和治疗；七张建筑配置表接入业务，删除VillageSetting重复权威价格。实际指针测试修复、产药、治疗、关闭页面继续、地图中断通过，用户存档恢复。26项原始资源导入清单见tools/hospital-ui/unity-imports.json。完整升级、多商品、多英雄经营未实现；任务状态保持不变，详见tools/hospital-ui/README.md。';
    await client.query(`UPDATE miniwow_resources.development_tasks
        SET progress_notes=concat_ws(E'\n',nullif(progress_notes,''),$1::text),progress_updated_at=now()
        WHERE id=ANY($2::text[]) AND position($1 in coalesce(progress_notes,''))=0`,
        [note,['MW-016','MW-017','MW-019','MW-020']]);
    const result=(await client.query('SELECT id,status FROM miniwow_resources.development_tasks WHERE id=ANY($1::text[]) ORDER BY id',
        [['MW-016','MW-017','MW-019','MW-020']])).rows;
    await client.query('COMMIT');
    await writeFile(new URL('./database-progress.json',import.meta.url),JSON.stringify({statusUnchanged:true,importedResources:imports.length,tasks:result},null,2));
    console.log(JSON.stringify(result));
} catch(error) {
    await client.query('ROLLBACK');
    throw error;
} finally {
    client.release();
    await db.close();
}
