import {openDatabase} from '../../../../../integrations/codex/server/database.mjs';
const db=await openDatabase();
const client=await db.pool.connect();
try {
 await client.query('BEGIN');
 const marker=(await client.query("SELECT obj_description('miniwow_resources.development_tasks'::regclass) AS marker")).rows[0].marker;
 if(marker!=='miniwow-development-tasks/v1')throw new Error('Task table ownership mismatch');
 const row=(await client.query('SELECT status FROM miniwow_resources.development_tasks WHERE id=$1 FOR UPDATE',['MW-021'])).rows[0];
 if(!['planned','in_progress','review'].includes(row.status))throw new Error('Unexpected task status');
 const note='2026-11-27：按原作接入RecruitStep/Dialogue/Hero与RecruitSystem、HeroCollectionEntity和场景Actor。18项隔离规则通过；实际60106对话和领奖、60112扣哨与中断保存通过。视频播放尚在排查，完整招募→旅店→首战入口连续验收未通过，保持in_progress。实现与原作差异见tools/recruit/README.md。';
 await client.query("UPDATE miniwow_resources.development_tasks SET status='in_progress',progress_notes=concat_ws(E'\\n',nullif(progress_notes,''),$1::text),progress_updated_at=now() WHERE id=$2",[note,'MW-021']);
 await client.query('COMMIT');console.log(JSON.stringify({id:'MW-021',status:'in_progress'}));
} catch(e){await client.query('ROLLBACK');throw e;}finally{client.release();await db.close();}
