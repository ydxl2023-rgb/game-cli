import {readFile,writeFile} from 'node:fs/promises';
import {openDatabase} from '../../../../../integrations/codex/server/database.mjs';

const read=async name=>JSON.parse(await readFile(new URL(name,import.meta.url),'utf8'));
const records=await read('./village-imports.json');
const evidence=await read('./village-evidence.json');
const config=await read('./village-config-evidence.json');
const ruleCall=evidence.find(row=>row.arguments.includes('ValidateRules'));
const claimCall=evidence.find(row=>row.arguments.includes('RepeatClaims'));
if(config.status!=='passed' || !ruleCall?.result.ok || !claimCall?.result.ok
    || claimCall.result.data.returnSummary!=='True'
    || !Object.values(JSON.parse(ruleCall.result.data.returnSummary)).every(Boolean))
    throw Error('Successful current config and runtime evidence is required before updating task status.');
const db=await openDatabase();
const client=await db.pool.connect();
try {
    await client.query('BEGIN');
    const marker=(await client.query('SELECT * FROM miniwow_resources.catalog_meta')).rows;
    if(marker.length!==1 || marker[0].owner_tag!=='miniwow-resource-catalog' || marker[0].schema_version!==1)
        throw Error('Resource catalog ownership mismatch');
    const owner=(await client.query("SELECT obj_description('miniwow_resources.development_tasks'::regclass) AS marker")).rows[0];
    if(owner.marker!=='miniwow-development-tasks/v1') throw Error('Development task ownership mismatch');
    for(const record of records){
        await client.query(`INSERT INTO miniwow_resources.scene_imports(import_id,source_asset_id,scene,payload)
            VALUES($1,$2,$3,$4::jsonb) ON CONFLICT(import_id) DO UPDATE SET
            source_asset_id=excluded.source_asset_id,scene=excluded.scene,payload=excluded.payload,updated_at=now()`,
            [record.id,record.sourceAssetId,'Home village / tasks 60004 to 60104',JSON.stringify(record)]);
    }
    const shared='2026-11-26 五任务实现与验收：60004招募→60101医院修复→60102购买宁神花→60103生产药剂→60104治疗→跟踪60105。实际按钮、多次重启、治疗中退出、重复领奖、资源不足和Actor生命周期测试通过，Console零错误。费用、奖励与建成模型有原作证据；生命百分比、即时1:1配方和初始资产采用明确记录的本地方案。UI复用Home对话层，未宣称原作全部经营页已复刻。';
    const updates=[
        ['MW-014','review','英雄125真实归属、获得确认后领取、重进不重复授予已验证。'],
        ['MW-016','review','医院60018正式修复扣280并显示priest/priest；与灭火后的priest_initial分离。'],
        ['MW-018','in_progress','本轮完成宁神花77001购买；工单中的小鱼77011及任务60108仍待后续实现。'],
        ['MW-019','review','原料扣除、药剂产出、任务累计和持久化已验证；服务器完整生产规则未恢复。'],
        ['MW-020','in_progress','本轮完成60104真实到院消费与治疗；60105探索行为不在本轮五项内，仍待实现。']
    ];
    const paths=['tools/opening/FIVE-TASKS.md','tools/opening/village-evidence.json',
        'tools/opening/village-imports.json','tools/opening/village-config-evidence.json'];
    for(const[id,status,detail] of updates){
        const note=shared+detail;
        await client.query(`UPDATE miniwow_resources.development_tasks SET
            status=$1,progress_notes=concat_ws(E'\n',nullif(progress_notes,''),$2::text),
            completion_evidence=completion_evidence || $3::jsonb,progress_updated_at=now()
            WHERE id=$4 AND position($2 in coalesce(progress_notes,''))=0`,
            [status,note,JSON.stringify(paths),id]);
    }
    await client.query('COMMIT');
    const result={imports:records.length,annotationsPreserved:true,binaryUploaded:false,
        tasks:(await client.query('SELECT id,status FROM miniwow_resources.development_tasks WHERE id=ANY($1::text[]) ORDER BY id',
            [updates.map(row=>row[0])])).rows};
    await writeFile(new URL('./village-database.json',import.meta.url),JSON.stringify(result,null,2));
    console.log(JSON.stringify(result));
} catch(error){
    await client.query('ROLLBACK');
    throw error;
} finally {
    client.release();
    await db.close();
}
