# Miniwow 开发任务数据库

创建日期：2026-11-26。任务定义来自原作体验研究，执行状态由数据库维护。本轮只创建规划与查询工具，未开发这些玩法或修改 Unity。

## 查询入口

复用 GameCLI 现有 PostgreSQL/SSH 连接配置，不复制凭证。独立表为 **miniwow_resources.development_tasks**，与资源 assets、annotations 等表分开。不是游戏运行时 tbtask，也不参与客户端导表。

在 miniwow 根目录运行：

```powershell
node tools/development-tasks/tasks-db.mjs --list
node tools/development-tasks/tasks-db.mjs --list M1
node tools/development-tasks/tasks-db.mjs --get MW-011
node tools/development-tasks/tasks-db.mjs --ready
node tools/development-tasks/tasks-db.mjs --check
```

--ready 只筛选 planned 且依赖全部 done 的任务；不代表待确认事项已解决，也不是自动开发授权。初始所有31项为 planned，已有功能仍需当前版本核验，不能据历史记录直接标 done。

## 里程碑与任务

- M0：基线、初始状态、配置、任务/存档、任务UI和场景引导。
- M1：开场、救火、救人、加伦招募及验收。
- M2：医院、生产、探索、免费英雄、旅店与首次关卡闭环。
- M3：第二至六章及跨模式验收。
- M4：第七至114章进一步研究并拆成内容批次；尚未逐一形成实施工单。

P0优先建立开场可玩片段；P1完成首章闭环；P2后续扩展。优先级不能覆盖依赖顺序。754个游戏内任务位置不等于754个开发工单，应按共享能力和内容批次拆分。

|编号|阶段|优先级|任务|依赖|
|---|---|---|---|---|
|MW-001|M0|P0|校准现有入口与运行时基线|—|
|MW-002|M0|P0|确定新手初始状态与本地规则边界|MW-001|
|MW-003|M0|P0|建立章节、任务、引导配置源表|MW-002|
|MW-004|M0|P0|实现任务状态与领奖事务|MW-003|
|MW-005|M0|P0|实现存档与任务恢复|MW-004|
|MW-006|M0|P0|实现任务栏和章节任务页|MW-004|
|MW-007|M0|P0|实现场景目标和交互气泡|MW-001, MW-003|
|MW-008|M0|P0|实现任务导航与引导节点执行|MW-006, MW-007|
|MW-009|M1|P0|接入开场剧情与首次进入分支|MW-005|
|MW-010|M1|P0|制作火灾村庄与剧情阶段表现|MW-005, MW-007|
|MW-011|M1|P0|实现取水和交易行救火|MW-008, MW-010|
|MW-012|M1|P0|实现旅店与医院连续救火|MW-011|
|MW-013|M1|P0|实现清理横梁和救援|MW-012|
|MW-014|M1|P0|实现英雄接收与加伦招募|MW-013, MW-004|
|MW-015|M1|P0|验收登录至加伦招募体验|MW-009, MW-014|
|MW-016|M2|P1|实现建筑集合与医院修复|MW-014, MW-003|
|MW-017|M2|P1|接入功能开放和入口显示条件|MW-016|
|MW-018|M2|P1|实现商店原料购买与库存|MW-016|
|MW-019|M2|P1|实现医院药剂生产|MW-018|
|MW-020|M2|P1|实现英雄治疗与探索行为|MW-019, MW-014|
|MW-021|M2|P1|实现兄弟对话与两名免费英雄|MW-020|
|MW-022|M2|P1|实现旅店修复、食物和恢复|MW-021, MW-018, MW-016|
|MW-023|M2|P1|定义首战规则与战斗结果接口|MW-003, MW-002|
|MW-024|M2|P1|实现首次关卡与返回主城|MW-022, MW-023|
|MW-025|M2|P1|验收第一章经营与战斗闭环|MW-017, MW-024, MW-015|
|MW-026|M3|P2|实现第二章收入和常规招募|MW-025|
|MW-027|M3|P2|实现第三章村庄升级与旧山道通关|MW-026|
|MW-028|M3|P2|实现第四章副本、税金与装备交易|MW-027|
|MW-029|M3|P2|实现第五至六章阵容扩展|MW-028|
|MW-030|M4|P2|拆解第七至114章后续内容|MW-029|
|MW-031|M3|P2|验收前六章与跨模式恢复|MW-029|

每项完整定义见 [tasks.json](tasks.json) 或 --get：scope、acceptance、openQuestions、originalTaskIds、evidence、deliverables。原作行为是参考证据，模块职责和本地规则方案遵守项目约定。

## 存储与维护约定

|字段|职责|
|---|---|
|id / title / milestone / priority / sort_order|稳定工单ID、标题、阶段、优先级和显示顺序|
|depends_on|其他开发工单ID，不能放原作游戏任务ID|
|original_task_ids|关联原作tbtask.TaskID|
|definition|JSONB完整范围、逐项验收、证据、交付物、待确认事项|
|definition_sha256 / definition_updated_at|本地定义同步校验与更新时间|
|status / assignee / progress_notes|数据库维护的执行状态、负责人、进度|
|completion_evidence / progress_updated_at|验收证据路径与进度更新时间|
|created_at|首次建档时间|

状态：planned → in_progress → review → done；受阻使用 blocked，取消使用 cancelled。恢复后返回适当状态。只有验收条件有当前证据才标 done；依赖为 cancelled 不会视为通过。当前不提供自动派发执行功能。

新定义或修订放入 tasks.json，先 --validate，再 --sync，最后 --check。同步只新增/更新定义，**不覆盖状态、负责人、进度和完成证据，不删除不在种子里的历史任务**；定义未变化时不更新时间。表所有权通过已有schema标记和专属表注释检查。更新定义后应人工检查已完成任务是否需要重新验收，工具不会自动重置状态。

执行状态以数据库为准；tasks.json中的initialStatus只说明建档初始值，不是当前状态。修改状态时同时更新 progress_updated_at，并写明事实。数据库存在额外人工建档工单时 --check 保留它们；依赖数组没有外键，手工编辑应自行核对环和缺失项，正式定义同步已校验依赖无环。

```sql
-- 根据原作任务查开发工作
SELECT id,title,status,milestone
FROM miniwow_resources.development_tasks
WHERE 60001 = ANY(original_task_ids);

-- 搜索描述或验收条件（小规模任务表）
SELECT id,title,status
FROM miniwow_resources.development_tasks
WHERE title ILIKE '%医院%' OR definition::text ILIKE '%医院%'
ORDER BY sort_order;

-- 查看某项验收条件和未决问题
SELECT id,definition->'acceptance' AS acceptance,
       definition->'openQuestions' AS open_questions
FROM miniwow_resources.development_tasks WHERE id='MW-011';

-- 实际开始执行后记录进度；不要仅为了查询就运行
UPDATE miniwow_resources.development_tasks
SET status='in_progress', progress_notes='记录实际开始的工作与证据',
    progress_updated_at=now()
WHERE id='MW-001' AND status='planned';
```

## 来源与验证

- [体验分析](../../research/config-study/PLAYER-JOURNEY.md)
- [章节与导航索引](../../research/config-study/CHAPTER-EXPERIENCE.md)
- [当前运行时规范](../../skills/miniwow-project/references/runtime-conventions.md)
- [资源数据库说明](../RESOURCE-DATABASE.md)

建档检查：31个唯一ID、依赖无环且无缺失；同步事务提交前逐项回读定义SHA-256。后续 --check 检查本地定义与数据库一致、缺失任务和数据库悬空依赖。这里的数据库建档验证不等于游戏开发验收。
