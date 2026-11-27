import {readFile} from 'node:fs/promises';
import {openDatabase} from '../../../../../integrations/codex/server/database.mjs';

const root = new URL('../../', import.meta.url);
const records = (await readFile(new URL('client/Library/OpeningQA/explore-evidence.jsonl', root), 'utf8'))
    .trim().split(/\r?\n/).map(line => JSON.parse(line));
const snapshots = records.filter(row => row.arguments.includes('MiniwowExploreProbe')
    && row.arguments.includes('Snapshot') && row.result.ok)
    .map(row => JSON.parse(row.result.data.returnSummary));
if (!snapshots.some(row => row.taskStatus === 'Claimed' && row.journey.counts['4'] === 1)
    || !snapshots.some(row => row.journey.phase === 3 && row.position.startsWith('(-41.97')))
    throw new Error('Claim and restored exit position evidence are required.');
const note = '2026-11-27：接入独立ExploreSystem及配置驱动首个本地探索事件。原NavMesh实际离村后执行采药，'
    + '绝对类型计数驱动60105，真实任务栏领取10金币验证；探索中重进从原出口恢复，记录防重复结算。'
    + '固定事件2、旅行8秒和返回医院为明确本地适配；原作完整地图、多英雄、随机事件、掉落及体力未完成，'
    + '因此MW-020继续in_progress，不将整个工单标为完成。';
const evidence = ['tools/explore/README.md', 'client/Library/OpeningQA/explore-evidence.jsonl',
    'client/Library/OpeningQA/explore-code.json', 'client/Library/OpeningQA/explore-architecture.json'];
const db = await openDatabase();
const client = await db.pool.connect();
try {
    await client.query('BEGIN');
    const owner = (await client.query("SELECT obj_description('miniwow_resources.development_tasks'::regclass) AS marker")).rows[0];
    if (owner.marker !== 'miniwow-development-tasks/v1') throw new Error('Task table ownership mismatch');
    const existing = (await client.query('SELECT status FROM miniwow_resources.development_tasks WHERE id=$1 FOR UPDATE', ['MW-020'])).rows[0];
    if (!['planned', 'in_progress', 'review'].includes(existing.status)) throw new Error('Unexpected task status');
    const result = await client.query(`UPDATE miniwow_resources.development_tasks SET status='in_progress',
        progress_notes=concat_ws(E'\n',nullif(progress_notes,''),$1::text),
        completion_evidence=completion_evidence || $2::jsonb,progress_updated_at=now()
        WHERE id=$3 RETURNING id,status`, [note, JSON.stringify(evidence), 'MW-020']);
    await client.query('COMMIT');
    console.log(JSON.stringify(result.rows));
} catch (error) {
    await client.query('ROLLBACK');
    throw error;
} finally {
    client.release();
    await db.close();
}
