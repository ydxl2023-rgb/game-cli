import {readFile,writeFile} from 'node:fs/promises';
import {openDatabase} from '../../../../../integrations/codex/server/database.mjs';
const records=JSON.parse(await readFile(new URL('./unity-imports.json',import.meta.url),'utf8'));
const db=await openDatabase();
const client=await db.pool.connect();
try {
    await client.query('BEGIN');
    const marker=(await client.query('SELECT * FROM miniwow_resources.catalog_meta')).rows;
    if(marker.length!==1||marker[0].schema_version!==1||marker[0].owner_tag!=='miniwow-resource-catalog')
        throw Error('Catalog ownership mismatch');
    const ownership=(await client.query("SELECT obj_description('miniwow_resources.development_tasks'::regclass) AS marker")).rows[0];
    if(ownership.marker!=='miniwow-development-tasks/v1')throw Error('Development task ownership mismatch');
    for(const record of records){
        await client.query(`INSERT INTO miniwow_resources.scene_imports(import_id,source_asset_id,scene,payload)
            VALUES($1,$2,$3,$4::jsonb) ON CONFLICT(import_id) DO UPDATE SET source_asset_id=excluded.source_asset_id,
            scene=excluded.scene,payload=excluded.payload,updated_at=now()`,
            [record.id,record.sourceAssetId,'Home opening / task 60001',JSON.stringify(record)]);
    }
    const progress=[
        ['MW-009','done','原视频29.75秒、独立原音乐、自然结束、确认跳过、取消继续、老档不重播已实测。视频中退出无播放器/纹理/页面/场景/Runtime残留，重进可播放。'],
        ['MW-010','in_progress','本轮只实现交易行火焰、目标圈和喷水表现，使用原火焰贴图的原生适配。完整烧毁模型、原粒子、其他火灾阶段与商人场景Spine生成尚未恢复。'],
        ['MW-011','in_progress','可玩60001流程已通过：原水井范围取水10、原NavMesh移动、交易行1.5秒救火扣10、3043对话、显式领取、跟踪60002。中断保留水且可恢复。原模型切换/场景商人生成/原喷水粒子验收仍待，不能标记完整原作表现完成。'],
        ['MW-005','in_progress','新增TaskCollection本地进度/领取收据和Home开场阶段保存，沿用StorageKey；重启恢复60002已验证。未实现全产品经济存档、多账号、云同步或NeedSave引导节点检查点。']
    ];
    const evidence=['tools/opening/README.md','tools/opening/play-evidence.json','tools/opening/play-evidence-restore.json','tools/opening/interruption-evidence.json','tools/opening/unity-imports.json'];
    for(const [id,status,note] of progress){
        await client.query(`UPDATE miniwow_resources.development_tasks SET status=$2,
            progress_notes=concat_ws(E'\n',nullif(progress_notes,''),$3::text),
            completion_evidence=completion_evidence || $4::jsonb,progress_updated_at=now() WHERE id=$1`,
            [id,status,'2026-11-26 开场首段：'+note,JSON.stringify(evidence)]);
    }
    const result={imports:records.length,binaryUploaded:false,annotationsPreserved:true,
        tasks:(await client.query('SELECT id,status,progress_notes FROM miniwow_resources.development_tasks WHERE id=ANY($1::text[]) ORDER BY id',[progress.map(row=>row[0])])).rows};
    await client.query('COMMIT');
    await writeFile(new URL('./database-evidence.json',import.meta.url),JSON.stringify(result,null,2)+'\n');
    console.log(JSON.stringify(result));
} catch(error) {await client.query('ROLLBACK');throw error;}
finally {client.release();await db.close();}

