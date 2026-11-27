# 旅店系统开发与验收记录

## 本轮实现范围

InnSystem（10007）是独立 RuntimeSystem；Home 只负责 NPC 头顶入口、角色导航与建成场景显示。InnEntity 保存业务规则，HomeVillageEntity 延续现有事务聚合，原料、成品、英雄金币、旅店收入、行动力及恢复截止时间一次提交，避免两个存档分别写入。UIInn 与 VMInn 只负责展示和意图。

原作依据来自已恢复的客户端 C# 与配置，不是服务端源码：

- `UIConsumableShopView.OnShow`：Hotel 使用 `e_ui_bg_hotel_1/e_ui_bg_hotel_1`，与医院共用消耗品页面结构。
- `ConsumableProxy.GetGoodUseValue/GetGoodPrice/GetGoodUseTime`：恢复效果按 ResponseEffect 尾部索引，价格叠加建筑加成，时间扣除建筑减时。
- tbbuilding 60017，tbfeatureunlock 1009：60101 已领取后开放。tbbuildinglevel 60017001：280 金币修复，容量 3，减时 1 秒，鱼 77011 制作食物 77111，英雄消费 17 金币。
- 原旅店 prefab 分为 bg_1 idle、NPC_hotel idle、bg_2 idle2，背景比例 .641，老板比例 .85、位置 (-.024,.235)，前后层 y=-.3。三层各自恢复，没有沿用医院人物或只导入背景。
- 任务 60107/60109/60110 分别观察 BuildingUpLevelTo(60017)、CumulativeBuildItemTimes(77111)、UseCostItem(60017)；发送绝对计数，不写死目标任务 ID。购买鱼由商店通用商品观察接续。

## 明确的本地适配

本地 demo 没有原作服务端。配方每批 1 原料→1 成品、旧档首次行动力 0、制作表现 3 秒放在 `HomeInnSetting`，不是从缺失服务端推导的权威规则。行动力已满拒绝用餐，不为演示伪造扣能；探索扣能尚未实现。

当前操作通过“安排英雄用餐”接受请求，UI 展示英雄 Spine、恢复倒计时与完成行动力。没有声称实现原作多英雄自动队列或主场景英雄到旅店的消费导航。医院治疗与旅店用餐互斥，不允许同一英雄同时消费两处服务。关闭页面不取消已支付用餐；系统卸载停止时钟，重装或重新登录从持久 deadline 结算，不再次扣食物或英雄金币。Master 在每次 Home 会话安装 Inn，使不主动打开旅店也能完成离线恢复。

新字段 `innEnergyInitialized` 仅用于旧存档一次迁移，非系统重复激活标志。数据有明确可选含义：`innRestEndTicks == 0` 表示没有进行中的用餐。UseCostItem 本地观察在完成用餐后累加，和当前医院任务时点一致。

只开放首级修复、首档食物生产与单英雄恢复；升级和自动生产按钮禁用，不虚构未实现业务。高等级原配置存在 ResponseEffect 越界证据，保留 -1 并拒绝执行；不能将表已导入说成全部等级已可用。

## 配置与资源重建

1. `tools/hospital-ui/collect.py` 读取医院原表，再调用本目录 `configuration.py` 追加旅店数据。这样医院重建不会删除旅店配置。
2. `tools/hospital-ui/build.mjs` 使用既有 artifact workbook 工具写入 propHomeBuilding.xlsx 与 propCommonText.xlsx；本目录 `build.mjs` 合并系统注册 10007，不覆盖已有系统。
3. `tools/town/export-scene-config.ps1` 走正式 e2cs；验证原始字段、语言保留和任务表达式；新增 HomeInnSetting 进入客户端生成源码和 bytes。共 400 个建筑等级、2100 个商品配方；未解析恢复项 300 个明确保留，不兜底猜值。
4. `design.py` 生成 UIInn.json、独立资源 manifest 与原资源 SHA256 台账。`install.ps1` 执行 UGUI dry-run/write、组件、Metadata dry-run/write；完成 29 个字段绑定后写 bind.request，MiniwowInnSetup 用原生 AssetDatabase 配置字体、raycast、Spine、序列化引用及引导手指。
5. 主场景模型独立复现：`tools/main-scene/export_scene.py --variant hotel-built` 生成 `tools/main-scene/hotel-built/scene.json` 与 `asset-manifest.json`，2 mesh/1 material/1 texture，当前提取 0 issues；写本目录 scene.request，由 MiniwowInnSceneSetup 创建并绑定 `Assets/Data/Scenes/Home/Opening/hotel-built`。场景必须完成绑定后才可 Play。

UI 路径：`Assets/Data/Widgets/Inn/UIInn.prefab`；专属图像与 Spine 位于 Inn/Resources，图标复用 Common/Icons。资源原路径与哈希见 imports.json；源资源只读，npc_hotel atlas 的大写页名在导入副本归一为实际文件名。

Native bind 同时负责 Spine 网格裁切边界、按 bg1 MeshAttachment 顶点对齐房间、原九宫格、文本对比度以及响应式锚点。用餐、行动力与修复区按设计稿 rect 在中部对齐，背景铺满；设计节点从 JSON 名称清单选择，不把 Spine 自动生成 Renderer 节点当作设计节点。实际复验1080×1920、1080×2340，见总验收截图。

## 参考工程机制与必要差异

- development Shop Entity/System/UI/VM 分层 → 沿用实体业务校验、系统编排、Widget 投影，业务系统无具体系统类型引用。
- development RuntimeSystem、Master 事件装配 → 沿用数字系统 ID 和完成事件；Home 导航等 UI 真正加载完成后才报告成功，不把系统激活等同 UI 就绪。
- 现有 UISystem 管理异步加载取消与页面生命周期 → 不另建 generation/requestedActive。
- 本产品 1080×1920 竖屏、原作消耗品经营 UI 与本地存档 → 产品差异，不写回 development。

## 验收入口与记录边界

- `MiniwowInnProbe.VerifyRules`：使用临时实体与临时持久值，finally 恢复原 key 值和原有/不存在状态；检查金币不足、修复重复、缺料、鱼→食物、双系统互斥、重复用餐、未到期、到期重载、完成幂等、能量满拒绝。
- `MiniwowInnProbe.PrepareReplay`：从主验收保留的 `Library/OpeningQA/facility-user-checkpoint.json` 构造60107起始存档。仅用于隔离验收，结束后必须恢复 checkpoint 并重新启动 Play，避免实体缓存仍持有测试值。
- `Snapshot` 报告真实持久字段和当前任务；`Unload`/`Reload` 经生产调度器测试独立装卸。
- 本次正式 UI 导入完成 29/29 Metadata，并完成页面与场景原生绑定。全项目、Home、Task、Shop、Hospital、Inn、架构、Editor、Actor 九范围使用本地 code-verify 检查；最终检查结果、运行交互和截图以 [总验收记录](ACCEPTANCE.md) 为准，早期编译成功不代替最终画面与连续流程验收。
- 主验收最终证据以 `tools/hospital-ui/runtime-evidence.jsonl` 及本轮总验收文档为准。

### 本轮视觉复验修复

最初仅检查Skeleton资产/纹理非空，未发现实际Room三层被RectMask2D剔除，不能作为可见验收。运行态InspectVisuals确证cull=true：Spine网格按Canvas参考像素放大100倍，但RectTransform仍是普通设计框，缩放后裁切框落在房间外。现制作阶段使用Spine原生MatchRectTransformWithBounds匹配真实网格，并按原bg1 MeshAttachment.ComputeWorldVertices的可见背景顶边对齐房间顶部；整体GetBounds包含透明附件，不能当作可见背景边界。房间、老板、柜台保留原相对比例与位移，消除顶部漏底。

九宫格边框读取原FairyGUI证据；本地修复/用餐按钮采用已有原边框面板，不拉伸不支持九宫格的cons_bg_7。容量使用原small_person图标，行动力信息放在房间下部避开老板脸部。最终结果必须以实际ScreenCapture图像为准，资产探针通过不替代运行截图。

最终实际ScreenCapture见 visual-final.png：原房间覆盖顶部，老板位于柜台后，行动力信息移到用餐按钮下方，原容量图标已补齐。当前截图为非用餐态，英雄消费时显示由主验收继续验证；未为拍照改动任务或消费存档。
