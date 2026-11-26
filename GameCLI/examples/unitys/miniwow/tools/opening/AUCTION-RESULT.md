# 60001：救火后的场景结果

范围：只补齐交易行修复、商人出现、水井恢复及保存结果。不继续实现 60002 的旅店、医院。

## 原作顺序与当前落点

原作依据：`GuideStoryController.Story1_3`，喷水后 1.5 秒回调依次执行 `ExtinguisBuildings`（60019=1）、`ShowNPCs`（增加 NPC_auction_2）、完成任务请求、水井恢复与 3043 对话。修复发生在领取之前。

当前：`HomeOpening.Extinguish` 提交并保存 Thanks 阶段 → `HomeOpeningRuntime` 立即关闭水流、更新场景 → 发送任务完成观察 → 更新对白。领取仍由 TaskSystem 处理，领取后跟踪 60002。没有让 TaskSystem 直接操作建筑，也没有引入系统相互调用。

`HomeEntity.opening` 是本段剧情进度的唯一保存来源，`auctionRestored` 由 Thanks/Claim/NextTask 推导。场景重建后消费同一结果，不另存一份修复布尔值。此项属于固定剧情修复，不代表已经实现集合式 BuildingEntity、建筑升级或商店经济。

|场景内容|救火前|救火后（尚未领取也生效）|
|---|---|---|
|交易行 Model|原 auction_initial_1|原 auction_initial|
|NPC_auction_2|隐藏 Shape，保留 ActorEntity|显示 Shape，原 Spine idle|
|火与水流|原有本地火焰适配，救火时水流|关闭|
|GuideObjs/Money1、Money5|取水时切换为 Money5|恢复 Money1|

商人实例 ID 5、来源配置 ID 60019，原场景坐标不变。沿用已有 `TownActorBinding` → `HomeEntity.InitializeActors` → `ActorShape.Bind`，不是单独挂一个无实体的 SkeletonAnimation。

参考沿用：development HomeEntity 的数据/展示分离、当前 ActorEntity/ActorShape 契约、StorageKey 保存；`MiniwowMainSceneSetup` 原生 Mesh/Material 导入抽成可复用 `ImportAssets`，只添加交易行烧毁变体，未重建 Home，不改原导航。development 索引检查 current，559 文件。

## 资源与复现

- `python tools/main-scene/export_scene.py --auction`：只导出原烧毁交易行，30 节点、12 网格、1 材质、1 贴图，缺依赖 0。默认全场景导出行为保留。
- `python tools/opening/prepare-auction.py`：复制原商人 PNG/Atlas/Skel，生成 `auction-imports.json`，记录源资产 ID、路径、哈希和用途。
- Unity 编辑模式写入 `tools/opening/auction.request`：`MiniwowAuctionSetup` 用原生资产接口导入并绑定，结果为 `auction-install.json`。不手改 YAML。重复执行替换烧毁变体，不重复添加商人实体绑定。
- 原生资产放 `Assets/Data/Scenes/Home/Opening/Auction`；商人依照既有 Spine 导入器放 `Assets/Data/Scenes/Town/Spine/npc_auction_2`，目录历史名称不代表恢复 TownSystem。
- `node tools/opening/sync-auction.mjs`：资源台账入既有 `miniwow_resources.scene_imports`，不覆盖人工 annotations；开发任务仍保持未完成表现的边界。

## 验证与边界

`verify-auction.ps1` 使用真实按钮、Actor 输入和原导航：新档烧毁模型/商人隐藏 → 取水切换水井 → 救火后修复/商人 idle → 完成未领取时完整重启 → 领取 60002。`-RestoreOnly` 再核对领取后的完整重启。

证据：`auction-evidence.json`、`auction-evidence-restore.json`、`auction-editor-code.json`、`auction-gamecli-code.json`、`auction-gamecli-architecture.json`。静态检查的既有 review 项不等于完整动态覆盖。

本轮完整开场脚本在视频播放断言处未通过（VideoPlayer prepared=true、time=0、isPlaying=false，音乐播放中）；未更改视频逻辑，不能宣称本轮视频回归通过。专项脚本从实际跳过确认继续验证场景结果。

原取水/投水飞行动画、原喷水粒子、角色抱水动画与减速、世界气泡和完整对白布局仍按 `ORIGINAL-COMPARISON.md` 待修；这些不因建筑结果已接通而视为完成。
