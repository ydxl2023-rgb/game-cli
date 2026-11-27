import {openDatabase} from '../../../../../integrations/codex/server/database.mjs';

const db = await openDatabase();
const client = await db.pool.connect();
const evidence = ['tools/recruit/README.md', 'tools/recruit/ACCEPTANCE.md',
  'tools/recruit/runtime-evidence.jsonl', 'tools/recruit/code-report.json',
  'tools/recruit/architecture-report.json', 'tools/recruit/restore-result.json'];
const changes = [
  ['MW-021', 'done', '60106→60112→60113→60114连续实测并领取；哨子仅扣一次，视频中断恢复后真实播完，英雄129/133加入数据集合与场景，重进保持。18项隔离规则及独立复核通过。尚缺吹哨前置特效，见验收边界。'],
  ['MW-022', 'review', '已连续实测60107修复60017、60108购买小鱼、60109制作77111、60110真实计时进食并领取；库存及任务重进一致。沿用既有手动主英雄消费，完整多英雄自动消费未实现；保留review等待旅店专项视觉验收。'],
  ['MW-024', 'in_progress', '本轮范围仅首战解锁入口：原配置三级引导可选择101001；60111保持Active，未发400金币。FightRuntime、胜负结算、返回主城战斗生命周期尚未实现，不标完成。']
];
try {
  await client.query('BEGIN');
  const marker = (await client.query("SELECT obj_description('miniwow_resources.development_tasks'::regclass) AS marker")).rows[0].marker;
  if (marker !== 'miniwow-development-tasks/v1') throw new Error('Task table ownership mismatch');
  for (const [id, status, note] of changes) {
    const row = (await client.query('SELECT status FROM miniwow_resources.development_tasks WHERE id=$1 FOR UPDATE', [id])).rows[0];
    if (!['planned', 'in_progress', 'review', status].includes(row.status)) throw new Error('Unexpected task status: ' + id);
    await client.query("UPDATE miniwow_resources.development_tasks SET status=$1, progress_notes=concat_ws(E'\n',nullif(progress_notes,''),$2::text), completion_evidence=$3::jsonb, progress_updated_at=now() WHERE id=$4",
      [status, '2026-11-27：' + note, JSON.stringify(evidence), id]);
  }
  await client.query('COMMIT');
  console.log(JSON.stringify(changes.map(([id, status]) => ({id, status}))));
} catch (error) {
  await client.query('ROLLBACK');
  throw error;
} finally {
  client.release();
  await db.close();
}
