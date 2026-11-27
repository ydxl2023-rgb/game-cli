# 招募至首次战斗入口

## 范围与证据

本次目标是 60106 → 60112 → 60113 → 60114 → 60107 → 60108 → 60109 → 60110 → 首关 101001 可选择。60111 的完成条件仍是 `PassedBattleId(101001)`；打开或选择关卡不冒充通关，不发放其 400 金币。

- 原作依据：`research/code-study/managed/decompiled/HotUpdate/Zombie/StoryManager.cs`、`UIHornAnimationView.cs`、`VideoManager.cs`、`UIExploreMapView.cs`、`UIBossListView.cs`。
- 配置依据：tbtask、tbconversation、tbhero、tbbattle、tbbattlearea、tbfeatureunlock；哈希和逐字段映射见 `configuration-source.json`。
- UI 依据：`original-layouts.json`、`map-layouts.json`、`mapcell-layout.json`、`detail-layouts.json`。这些记录是原 FGUI 基础矩形，未声称完整解析控制器与适配关系。
- 资源来源、用途、目标路径及哈希：`imports.json`。原始资源不覆盖；Unity 导入视频可使用兼容转码产物。

## 配置与职责

`config/propRecruit.xlsx` 包含 RecruitStep、RecruitDialogue、RecruitHero、BattleEntry。导表使用既有 `tools/town/export-scene-config.ps1`；`verify.py` 校验导出字段、任务引用与英雄条件，禁止手改生成 C# 或 bytes。

|模块|职责|
|---|---|
|RecruitSystem / RecruitEntity|配置驱动的对话、吹哨、免费接收；System 接收命名事件，Entity 校验和改变事务候选|
|HomeVillageEntity / RecruitSave|统一提交道具扣除、pendingHorn、交互回执和英雄所有权，先落盘后报告任务|
|HeroCollectionEntity|持有主英雄125与新增129、133的实体，场景显示不是拥有英雄的依据|
|HomeRecruitRuntime|原导航上的招募入场移动；到位后发事件，不发奖励或完成任务|
|HomeRecruitScene / ActorShape|序列化场景投影与 Spine 表现，不拥有招募事务|
|UIRecruit / VMRecruit|原对话素材、角色立绘、视频展示和显式点击意图|
|UIHomeRecruitEntry|原场景交互气泡，角色未走到位不显示接收按钮|
|BattleEntrySystem / BattleEntryEntity|原功能开放条件、真实英雄集合与首关选择；尚不执行战斗|

系统10010、10011由 Master 登录事件安装。模块不保存或调用其他具体 System。跨模块事件统一 `XPluginUtility`；HomeRuntime继续是场景执行器，不新建玩法场景System。

`FinishTalk` 的60106与60112原参数都是0，因此交互回执必须携带 `taskId`；不得把一次对话广播成整章所有对话完成。一般任务仍按条件类型/参数复用同一实现。

## 原作流程与本地边界

- 60106原对话3028，领取残破狮鹫哨83010×1。
- 60112原对话3029，先提交一次扣哨，再播放 `heroDrawVideo.mp4`。已支付但未结束的视频保留 `pendingHorn`，重进恢复播放，不再扣道具。
- 60113凯129 / hero_20029 / 对话3031；60114托比133 / hero_20033 / 对话3030。演员与英雄所有权分离，领取后实际加入集合。两名英雄初始健康、个人金币沿用本地归一化示例规则，非推断服务器公式。
- 60107–60110复用既有旅店、商店及任务实现。目前用餐是手动安排主英雄，并非完整原作多英雄自动消费。
- 原作探索功能1011在60101后开放；第一个关卡没有前置关卡。60111是后续引导，不能人为把60110当原作首关解锁硬门槛。
- 首战入口只提供配置中的第一关。完整探索地图、全关卡列表、阵容部署、战斗计算/播放/结算另行开发。
- `e_ui_shaozi_1` 前置吹哨特效与 `rally_map_1_1` 专属图暂未在已提取资源中找到，不能宣称已完整还原。当前入口使用原作地图预览与 MapCell 通用素材；非原作3D探索场景完全复刻。

## 制作与验收入口

1. `python tools/recruit/collect.py` 收集原表与原组件矩形。
2. 使用已配置的 artifact-tool 运行 `node tools/recruit/build.mjs`，再由既有 e2cs 导表。
3. `python tools/recruit/design.py` 生成三个布局与资源清单。
4. `pwsh.exe -File tools/recruit/install.ps1`：逐页 UGUI dry-run/write、添加Widget、Metadata dry-run/write。
5. `MiniwowRecruitSetup` 处理显式 `Library/OpeningQA/recruit-setup.request`：scene / widgets / video。全部经 Unity 原生资产API，未手写 Unity YAML。
6. `MiniwowRecruitProbe.VerifyRules` 为隔离规则验收；`PrepareReplay` 要求本轮不可覆盖的用户检查点，不能用于开发者日常篡改用户进度。

运行原始证据归档为本目录 `runtime-evidence.jsonl`。2026-11-27 已完成本轮“60106至首战入口可选”的连续验收，详见 [验收记录](ACCEPTANCE.md)。战斗执行与原作全量视觉还原不在该通过结论内。
