# 60105 本地探索接入

本轮范围是 MW-020 中的首次离村探索事件链。加伦完成治疗并领取对应前置任务后，按配置走到原作 `ExplorePoint`；到达后执行本地旅行和采药事件，事件结束才报告 `AdventureEventTimes` 的绝对计数，随后返回医院外。点击探索入口、切换地图、显示引导手指均不增加计数。任务奖励仍由 TaskSystem 与村庄奖励收据提交。

## 原作证据与本地适配

|项目|依据与边界|
|---|---|
|60105 条件|tbtask：AdventureEventTimes / Param1=4 / FinishValue=1，奖励金币10|
|事件2|tbadventureevent：AreaId=1，HerbPicking，事件类型4；条件匹配事件类型，不是事件ID|
|采药执行时间|tbglobal.AdventureEventExecuteTime.HerbPicking=1599毫秒，导为1.599秒|
|离村|BuildingHeroStateExplore.OnEnter 先移动到场景出口，再调用 ExploreSystem.NewHero；当前复用原NavMesh和ActorEntity|
|出口|tools/main-scene/scene.json 中 ExplorePoint=(-41.97,35.3,6.18)，运行时投影到原导航面|
|本地适配|首次固定事件2、旅行8秒、一次事件后返回医院外。这不是原作服务器行为的完整恢复；参数在propExplore.xlsx中|
|未实现|原作完整探索地图表现、事件随机派发、多英雄队列、掉落、体力扣除、英雄经验和战斗；没有伪造这些业务结果|

原作 ExploreSystem.ExploreHeroExecuteState 在执行时间结束后请求 LevelProxy.RequestHeroArrive；下一事件、体力、经验和掉落来自服务端通知。因此本轮仅结算本地事件完成记录，不根据客户端动画猜测服务器经济规则。现有地图仍是旧预览页，不以本轮完成任务计数宣称原版探索界面已经还原。

## 职责和扩展

- ExploreSystem 是独立 RuntimeSystem（10009）。Master 登录后按数字ID装配；系统监听 HomeReady/HomeSessionAvailable/HomeSessionClosing，装卸不直接调用其他玩法系统。
- ExploreEntity 管理旅程条件、阶段和结算规则；HomeVillageEntity 将 ExploreSave 与经济、任务奖励收据保存在同一村庄事务中。
- HomeVillageRuntime 只执行场景路线；真实到达发送 ExploreWorldArrived，不让计时器代替世界导航。
- TaskSystem 按条件类型和参数消费绝对观察。同类事件不需要新的任务分支。
- ExploreSave.completed 保存已完成旅程ID；同一英雄的下一条同类旅程可通过新配置与前置任务接续，不必更改执行器。当前场景只有一个受支持的英雄Actor绑定，多英雄集合是后续独立工作。
- Traveling/Executing/Returning 重进时按出口恢复世界Actor；返回路径以原导航计算。隐藏Shape不影响实体计时。
- 旅行和执行使用持久UTC截止时间，重装系统不重置倒计时；Returning阶段已有事件收据，不重复累计。
- 加伦探索期间不能同时安排旅店用餐或医院治疗。现有已支付用餐会阻止出发。

## 配置链路

`tools/explore/build.mjs` 使用桌面提供的 artifact-tool 生成 `config/propExplore.xlsx`，并将10009合并到系统表。环境变量 `CODEX_WORKSPACE_NODE_MODULES` 指向桌面工具返回的依赖目录。脚本仅写对应源表，不直接生成客户端C#。

`tools/town/export-scene-config.ps1` 使用既有e2cs导表，并执行 `tools/explore/verify.py`：验证英雄/任务外键、原事件与区域及类型映射、原执行时长、原出口、有限正数的本地旅行时长。新增配置由GamePlugin现有资源名/Excel字段装配机制读取。

## 沿用参考机制

development 的 RuntimeSystem、Shop Entity/System 分工及当前 GamePlugin 配置读取机制继续沿用；现行事件具名回调规范优先于参考工程历史lambda写法。HomeScene.CreateActorDefinitions 负责从持久状态重建Actor；不在Shape中修改权威数据，不添加新的系统激活代次或平行生命周期标记。

## 验收和用户进度

本轮运行证据：`client/Library/OpeningQA/explore-evidence.jsonl`；配置导出：`explore-export.log`；静态检查：`explore-code.json`、`explore-architecture.json`。

`MiniwowExploreProbe.VerifyRules` 使用隔离数据验证前置领取、服务互斥、真实到达、计时、存档序列化后一次结算、重复到达、下一条同类配置和无虚构物品收益。`Snapshot` 只读真实Actor位置、旅程和60105状态。`PrepareResumeFixture` 仅用于验收，覆盖测试存档，使用前必须保存不可覆盖的用户checkpoint，结束后必须恢复。

用户本轮开始时的真实存档备份是 `client/Library/OpeningQA/explore-user-checkpoint.json`，与旧商店验收备份不同。禁止用旧备份覆盖用户本轮新增的旅店与购买进度。

后续 MW-021 的兄弟对话、吹哨、凯和托比接收未在本轮实现；不能把任务60106进入跟踪当作这些业务已经接通。

## 本轮验收结果（2026-11-27）

- 20项隔离业务断言通过，包括第二条同类型配置可启动。
- 原场景实际到达出口 `(-41.97,35.16,6.18)` 时60105仍为Active/0；事件执行后成为Completed/1，返程实际到达 `(-26.63,35.16,-2.43)`。
- 真实按钮返回营地并点击TaskClick，60105成为Claimed，金币18440→18450；截图确认任务栏接续60106。
- Executing存档重进，角色逻辑位置正确恢复为出口，task仍0/1；独立卸载重装ExploreSystem后原截止时间不变，最终Completed、事件类型4计数1。
- 75文件编码检查0 errors（5项既存review项保留），9系统架构检查0 issues，最终Unity Console 0 errors。独立review发现的重进位置、后续配置、轮询克隆和缺少导表验证4项已修复并复审。
- 最新用户checkpoint三个键逐值完全恢复；测试后停止Play，未替用户领取探索奖励，未提交或推送。
- 稳定审查证据复制至本目录 `runtime-evidence.jsonl`、`code-report.json`、`architecture-report.json`、`restore-result.json`。截图保留在Library/OpeningQA。
- 数据库MW-020追加本轮进展，状态仍in_progress；完整地图和后续招募不冒充完成。
