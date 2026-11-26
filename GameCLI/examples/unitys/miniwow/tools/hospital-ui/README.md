# 医院交互：原作证据与配置契约

日期：2026-11-26。范围：医院 60018；对应 MW-016、MW-017、MW-019、MW-020。

本目录已实现首级医院交互：场景修复气泡、蓝色进入按钮、原料生产、药剂库存、英雄治疗和任务领取。UIHomeHospital 与 UIHomeHospitalEntry 由 HomeSystem 管理。导入全部等级数据不表示已开放全部等级玩法。

## 原作实际入口

1. `tbbuilding[60018].Feature=1010`；`tbfeatureunlock[1010]` 要求 `ChapterTask(60004)`。`PlayerProxy.IsMeetCondition` 检查 `TaskProxy.FinishedChapterTasks`；TaskProxy 在收到任务状态 2 或初始化已完成集合时维护它。不能把“接取 60004”当作已经开放。当前离线任务适配继续按领取后的完成记录处理。
2. `BuildingInteractiveModule.Refresh` 对未建成医院调用 `OpenOpBubble("修复", cost.Id, cost.Num, ...)`。这是场景操作气泡，不是建筑详情弹窗。费用来自**下一等级** `60018001.BuildingUpCost`，为金币 70001 × 280。
3. 气泡点击先检查资源；足够时执行入口引导/导航，再播放金币飞行，然后 `ConstructBuilding(id, quick:true)`。不足时提示资源不足，没有建造结果。进入建筑触发区域时还有 Main 的功能按钮入口，两者不能各自扣一次钱。
4. `BuildingProxy.ConstructBuilding` 向服务器请求，成功响应更新数据并通知 ConstructStart。BaseBuildingController 的建造流程驱动效果和模型切换。**灭火后的 priest_initial 与正式建成的 priest/priest 是不同状态。** 当前本地版使用既有原子村庄事务，不伪造原服务器响应。
5. `BuildingData.IsUnlock()` 判断 Level > 0；已建成医院由 `OpenBuilding` 打开 `Consumable/ConsumableShopPage`。因此不能先制作一个原作不存在的通用“修复详情”页面。

## 页面、逻辑与字段

|页面部分|原类/方法|数据关系|当前实现要求|
|---|---|---|---|
|修复气泡|UIBuildingOpBubble.Init / OnItemChange|当前余额、下一等级费用、IsEnough|跟随场景锚点；不足反馈；成功后隐藏；不能因重绘再次扣费|
|建筑标题与升级|UIBuildingPanel.Refresh|当前等级、下一等级、条件、费用|未达条件显示条件；不存在下一等级才是原作满级；当前未开发升级须明确区分|
|店内场景|UIConsumableShopView.OnShow / LoadSpine|医院 style=1、e_ui_bg_priest_1/e_ui_bg_priest_1|恢复原医院 Spine 与英雄消费表现，不拿全屏截图替代 UI|
|收益/效率/容量|UIConsumableShopView.Refresh|CostEffect.PricePct / CutCdSec / PeopleNum|首级为 0%、1 秒、3 人；不是玩家金币、实际治疗秒数或当前人数|
|药剂列表|Refresh / RefreshDetail|CostEffect.Goods、TbItem、库存|保留商品次序；选中商品展示效果、使用时间、服务价格|
|原料按钮|OnRenderMaterial / OnClickMaterial|Goods.Cost、原料库存|点原料发起生产；不足时打开物品获取提示；不是扣金币购买药剂|
|生产表现|OnClickMaterial 成功回调|生产请求已成功后播放 3 秒进度、0.5 秒飞行|3 秒是客户端表现，不能当作已证实的服务器生产时间|
|治疗英雄|AddHero / HeroStartUseConsumable / HeroFinishUseConsumable|ConsumableProxy.Shops、英雄状态与服务收入|实体保存结果；页面打开/关闭不能重置或重新扣药剂|
|退出|OnHide / CloseView|页面生命周期|释放 Spine/英雄显示与订阅；不清空库存、建筑等级或治疗数据|

`ConsumableProxy.CraftConsumable` 只提交 buildingId、itemId。材料消耗数量、单次产量的权威服务器代码不在现有资料内。当前 DEMO 的 1:1 即时提交属于已记录的本地方案，不能从 Goods 的两个物品 ID 推断出来。

`CraftPrice` 的准确含义是**英雄消费服务的基础售价**。原算法：`RoundToInt(basePrice * (1 + PricePct / 100f))`。首级为 50 金币，来自英雄钱包并增加建筑待收收入；不应该再次扣玩家修复资金。

使用时间为药剂 UseEffect.Time 减建筑 CutCdSec，首级 5−1=4 秒。恢复比例不是 TbItem 中的固定值：原作按商品在当前等级 Goods 中的次序索引 Global.ResponseEffect，首级唯一商品取末项 100。

## 工程源表

维护文件：[propHomeBuilding.xlsx](../../config/propHomeBuilding.xlsx)。遵循已有 e2cs 三行表头格式，第一行中文说明、第二行字段名、第三行类型。所有主键为数字；名称和说明在 propCommonText.xlsx，原始资源标识不作为运行时 Unity 路径使用。

|表|行数|职责|
|---|---:|---|
|HomeBuilding|1|建筑身份、功能 ID、语言引用、初始模型和引导|
|HomeBuildingLevel|200|原等级 ID、模型、时间、服务价格加成、时间减免、容量|
|HomeBuildingCost|200|目标等级费用；不将费用硬编码在 UI|
|HomeBuildingCondition|21|功能/目标等级条件；保留原枚举类型与参数，多行按原作 AND 判断|
|HomeBuildingGoods|1050|每级有序商品、原料、服务基础售价及可验证的恢复比例|
|HomeBuildingItem|13|金币、6 类材料、6 类药剂；名称/说明语言 ID、原图标标识和基础使用时间|

HomeBuildingCondition.OwnerKind：1 为 FeatureID，2 为目标 LevelID。Type 为原 UnLockType 数字枚举，原作语义见 PlayerProxy.IsMeetCondition。MapId 是“区域已开放”，不能替换成“通过指定地图”。没有额外制造尚未注册的条件表达式函数。

子行 ID 使用原等级 ID×10＋一基序号，商品 Order 保留零基次序。此为本产品稳定数字键规则，不是原作新增的字符串 ID。

### 证据冲突必须保留

从 51 级起 Goods 有 6 项，而恢复的 ResponseEffect 只有 `[5,15,50,80,100]`。按当前反编译 `GetGoodUseValue`，首项索引是 −1，共影响 150 条商品记录。

这些行 RecoveryPercent 记为 −1，含义是**尚无有效结果，禁止用于业务计算**，不是恢复负生命或零恢复。原资料是否跨版本、原运行时是否覆写了全局配置，尚未确认；不能直接宣称原作运行必崩。其余原始字段完整保留。首级医院数据不受影响；未来升级实现必须先处理这些证据冲突，不能裁掉商品或钳制数组索引掩盖问题。

医院费用、商品、服务售价、使用时间和恢复比例已读取上述关联表。VillageSetting 已删除重复的 RepairCost/PotionID/UseSeconds/HealPercent/ServicePrice；保留本地配方与初始资产。HomeHospitalSetting（1 行）保存原作客户端表现时长 CraftSeconds=3、FlightSeconds=0.5。HomeBuildingItem.IconPath 保存 Unity 图标路径。

## 资源和布局

`original-ui-audit.json` 恢复 7 个组件的基础矩形：BuildingOpBubble、BuildingEntranceBubble、ConsumableShopPage、ShopScene、Material、Consumable、BuildingPanel。原页面设计尺寸为 **720×1560**；修复气泡为 **157×106**。工程仍以 **1080×1920** 为标准，需要按比例适配安全区和可滚动内容，不能将整页非等比拉伸。

`resource-index.json` 记录 47 条直接引用、资源资产 ID、原路径、用途和哈希，并记录医院 Spine 的已提取文件。unity-imports.json 记录 26 项实际导入素材、源资产 ID、用途、目标路径与一致性哈希。它是研究依赖表，不是 Unity 导入完成清单。控制器 gears、relations、动画和更深层组件尚需在制作阶段逐项恢复，基础矩形不等于运行时最终位置。

原入口资源：Building/BuildingOpBubble；建成页：Consumable/ConsumableShopPage；基础面板：Common/BuildingPanel；原料：CommonIcon/HpMaterial_1；药剂：CommonIcon/HpItem_1；医院内部动画：e_ui_bg_priest_1/e_ui_bg_priest_1。素材均来自已提取文件，没有调用原线上服务。

## UI 实现顺序和验收契约

1. 在 Home 的场景绑定中持有医院交互碰撞体和气泡锚点。按功能开放与建筑 Level 驱动修复/进入两种入口；关闭临时对话层对 60101 的直接修复。
2. BuildingEntity/HomeVillageEntity 校验并提交一次修复事务；HomeSystem 接收命名事件回调并更新表现；TaskSystem 只观察真实结果。不能以当前任务 ID 代替建筑的实际开放与建成状态。
3. 通过正式 UGUI CLI 创建 UIHomeHospital.prefab、VM 与元数据，恢复原料生产、库存、治疗状态和返回操作。页面使用实体快照和配置，不持有其他 System，不自行写存档。
4. 将 60103 的任务指引接到页面的原料操作，将 60104 接到真实到院治疗表现。完成任务和领取奖励保持独立，不把“打开 UI”当作任务完成。
5. 原作全等级升级、自动生产和完整多英雄调度尚未开发；不要创建可点击却无业务结果的按钮，也不要把只有 1 级实现标成满级。
6. 验收资源不足不扣款、连续点击不重复修复、模型替换、生产扣料及产出、离开页面治疗继续、重新登录恢复、任务奖励只领一次；保存用户当前进度后在隔离测试状态中重放并恢复。

## 复现与验证

在 miniwow 目录执行：

```powershell
python -X utf8 tools/hospital-ui/collect.py
node tools/hospital-ui/build.mjs
python -X utf8 tools/hospital-ui/audit.py
pwsh.exe -NoProfile -File tools/town/export-scene-config.ps1
```

导表链调用 verify.py，逐字段对比原映射、核对关联 ID、数字主键、原文件哈希及首级费用/效果，并保留 150 条未解证据。通过后才复制生成 C# 与 bytes。原有 1594 个任务引导表达式校验仍在同一条链上执行。

`configuration-validation.json` 的 exportStatus=passed 仅表示导表一致性通过，不表示 UI 验收或原作高等级疑点解决。源文件哈希见 configuration-source.json；隔离验收后已恢复用户原存档，任务 60105、金币 18740、草药 50、药剂 0、医院 1 级、英雄生命 100%。


## 本轮实现与验收（2026-11-26）

- BuildingEntity 读取等级、费用与功能条件；HomeVillageEntity 提交修复/生产事务；HeroEntity 保存治疗进度与消费。HomeSystem 仅编排事件和页面，HomeVillageRuntime 负责实际导航。没有新增玩法 System 或系统间直接调用。
- UIHomeHospitalEntry 跟随场景锚点。未建成显示修复费用，建成后使用原作 Building/hub_button_15_0 蓝色进入图标。地图打开/输入关闭会取消导航和未完成的金币飞行。
- UIHomeHospital 使用原作医院内部 Spine、英雄 Spine、图标和九宫格背景；原始布局适配 1080×1920，静态标签写入预制，动态字段通过 VM 绑定。治疗时隐藏场景英雄，仅显示店内英雄；关闭页面不终止治疗。
- 正式生成链：design.py → install.ps1（UGUI dry-run/写入、组件、元数据 dry-run/写入）→ bind.request（Editor API 原生引用绑定）。audit.py 从包数据恢复九宫格；record-imports.py 校验实际文件来源。
- runtime-evidence.jsonl：实际 UI 射线命中后 PointerDown/Up/Click，验证 280 金币修复、任务独立领取、扣原料产药、生产表现期间禁用重复点击、到院扣药/英雄金币 50/医院收入 50、4 秒治疗至 100%、页面关闭后继续治疗、地图中断导航、重新进入页面和用户存档恢复。实体边界测试 ValidateRules 全部通过。
- 最新同源检查器纯本地结果：code-local.json、architecture-local.json、editor-local.json。架构检查仅覆盖静态源符号和默认预处理分支，接口、反射与生成代码仍需人工复核；独立复核未发现首级医院阻断项。编译与运行另由 Unity 验证。
- 自动审批拒绝连接绑定数据库的 GameCLI 校验（192.168.63.29:22）。因此本轮数据库资源/任务进度同步尚未执行；本地资源清单和同步脚本已准备，不能将旧 database-progress.json 视为本轮同步完成证据。

尚未开放全等级升级、多商品切换、自动生产和完整多英雄经营。60102 商人仍使用现有交互层。本地 1:1 即时配方不是已恢复的服务器规则；原作 150 条高等级恢复值冲突保留待研究。


## 2026-11-26 后续显示修复

用户反馈两处角色和摇杆偏大：主场景调校记录见 ../player/README.md。医院当前 Spine 患者承载尺寸由170×190改为110×125，脚底固定在房间坐标(765,560)，不再按动画包围框中心摆放。该实现仍是本项目的Spine表现适配：本轮复核确认原作 UIConsumableHero 实际使用 TbHero.BodyIcon，而不是医院患者Spine；ShopHero/heroIcon源尺寸256×273、scale0.6，且BodyIcon带透明边框。不可将当前适配称作该原UI组件逐像素复刻。

任务条修复：取水/救火可操作阶段点击发起已有导航；村庄完成任务仍正常领取；当前60105探索尚未开发，点击由可见UIHome反馈“该系统尚未接入”，不再把反馈写入隐藏的对话层，不伪造任务进度。

本轮重新编译通过；code-local/architecture-local/editor-local/actor-local零错误。实际射线按钮点击看到提示，关闭提示后跨UI摇杆捕获通过，返回登录再进入位置一致。医院患者视觉预览使用Editor专用PreviewPatient，仅改变显示节点，不修改治疗、物品或任务数据；预览后恢复。


## 2026-11-26 建筑入口提示恢复

本轮修复医院入口示例，未开放其他建筑的业务界面：

- 原 BaseBuildingController.CreateTitleBubble 使用 NPCName，而不是建筑Name。医院绿色名称为“牧师”，来自 HomeBuilding.NpcNameTextID→CommonText；独立绑定原 Bubble 位置并叠加原(0,-0.5,0)偏移。蓝色进入绑定 EntranceBubble。
- 已解码 BuildingEntranceBubble 与 BuildingTitleBubble 的根pivot=(0.5,1,true)。Unity采用底部中心投影，避免以中心放置图标压住角色。蓝色图标从117×125校准为96×102（原78×83按1920/1560适配取整），属于当前1080×1920布局适配；不通过修改角色大小抵消入口错误。
- 已建成医院：玩家原AgentRadius与原BoxCollider重叠时才显示进入按钮，退出范围隐藏；NPC名称独立显示。修复气泡保持原先可导航行为。医院界面/地图遮挡输入时，入口及地圈隐藏。
- 绿色地圈使用原e_npc_xuanze_1.png、原TriggerEffect/effect位置、原粒子尺寸6、寿命2秒、三轴起始旋转、尺寸曲线及alpha曲线。采用Unity Sprites/Default适配原UberShader的透明显示；未宣称恢复完整原Shader。开放医院靠近时显示，离开时隐藏。
- 原始粒子源：extracted-resources/assets/data/effects/prefabs/_subassets/ParticleSystem/8209274776750613049__dc4fcf016c25.json。导入纹理和源ID/哈希追加至unity-imports.json，共27项；数据库未在本轮同步。
- 实际场景射线ClickHospitalScene→导航→医院页面→关闭，验证远处隐藏、近处显示、页面内隐藏。测试不改变任务60105、金币、物品或治疗存档。


### 角色与地圈错位的后续修正

此前入口显示虽然已修正，但访客落点误用了Collider节点位置(-26.65,35.71,0.01)，与原TriggerEffect(-26.63,35.2,-2.43)相差约2.44单位，导致加伦站在绿圈边缘。现将本地VillageSetting.HospitalXYZ改为地圈中心并同步build-village.mjs，治疗导航及Healing/Complete重进恢复均使用该配置，实际Y仍由原NavMesh落地。这里是本地访客站位方案，不宣称原作所有英雄固定站在圈中心。

玩家入口路线独立使用原Hospital/Collider位置，通过HomeScene.CalculateHospitalRoute提供；不再复用访客停靠点，避免玩家与访客重叠。修复没有变更任务状态、钱包、药剂或治疗收据。


### 其他已恢复 NPC 光圈

HomeNpcSelection使用原入口Transform、BoxCollider中心和尺寸，按角色半径/高度判断接近；NPC未显示或Home关闭输入时隐藏。当前绑定铁匠NPC_forgingtable_2、旅店NPC_hotel、商人NPC_auction_2，复用同一原始选中粒子。医院继续受功能开放与入口流程控制。普通行走英雄不自动附带入口光圈，也不因此开放尚未实现的商店业务。

原碰撞体来源、哈希与参数见npc-entrances.json；collect-npc-entrances.py可重建。三处按原NavMesh地面采样验证near/far/input-gated均通过，另实际走到铁匠旁确认绿圈显示（forge-selection.png）。不通过脚底点判断高度，避免铁匠1单位高的入口漏触发。角色速度与摇杆手感调整见../player/README.md。
# NPC 名称补齐（2026-11-26）

## 商人进入按钮补齐

### 全部已恢复交互 NPC 入口补齐

铁匠 60002、旅店老板 60017 与商人 60019 现共用 `RefreshResidentEntry` 投影/范围判定；各自 VM 点击发送带原建筑数字 ID 的 `ReqHomeResidentVisit`，替代此前仅商人的事件。三个未实现页面统一显示待接入反馈；医院仍走真实修复/进入逻辑。普通行走英雄不因此新增建筑入口。

本轮正式 UGUI/Metadata 与原生绑定通过；实走到铁匠、旅店、商人并逐个实际点击，远处和离开范围时隐藏；截图 `forge-entry.png`、`hotel-entry.png`。医院导航进入成功，页面打开时三个入口均隐藏。金币18740、宁神花50、购买次数1、任务60105 Active保持不变。本地代码/架构检查均0 errors，独立只读复核通过。

- 原作 `BuildingInteractiveModule.CreateBuildingEntranceBubble` 绑定 `EntranceBubble`，`TriggerEnterBuilding` 显示入口，`TriggerExitBuilding` 隐藏。此次补入 60019 商人按钮，共享原 `hub_button_15_0` 图片，96×102，底部中心 pivot；名称仍使用独立 `Bubble` 挂点。
- `HomeNpcSelection` 将原 Collider 范围判断抽为 `IsNearby`，光圈与入口共用；场景引用按建筑数字 ID 查找。按钮在商人可见、玩家进入范围且 Home 接受输入时显示。
- 场景标签 Widget 改为框架 `background` 层，位于 Home HUD 和反馈弹窗下方；实测 MerchantClick 点击、反馈遮挡和 CloseFeedbackClick 关闭通过，避免入口穿过弹窗。
- 正式 UGUI / Metadata dry-run 与写入、场景绑定和 Unity 编译完成。运行验证远处 false、靠近 true、离开 false；真实 GraphicRaycast 点击 MerchantClick 成功。截图 `merchant-entry.png` 与 `merchant-feedback.png`。
- **完整交易行页面尚未实现**。入口经 `ReqHomeMerchantVisit` → `HomeSystem.OnMerchantVisit` 显示现有语言表的待实现反馈，不调用购买或任务操作。运行核对金币 18740、宁神花 50、购买次数 1、任务 60105 Active 未改变。控制台 0 errors，本地代码/架构检查通过。

- 原作依据：`tbbuilding` 的 60002、60017、60019 `NPCName` 分别为铁匠、旅店老板、商人；`BaseBuildingController.CreateTitleBubble` 使用建筑 `Bubble` 挂点加世界坐标向下 0.5 的偏移。
- `collect.py` 将名称写入 `propCommonText.xlsx` 的 30224–30226，再经现有 e2cs 链路导出。固定文案生成到正式 UGUI 预制，不增加展示用 VM 字段。
- 沿用既有 UIWidget、序列化引用和场景投影机制。当前共享场景标签承载于 `UIHomeHospitalEntry`，三个居民标题位于根层，独立于医院 EntryVisible、available 和 built。没有增加系统或改变建筑开放规则。
- `npc-entrances.json` 同时决定场景引用与预制标题数组的顺序；`MiniwowHospitalSetup` 绑定各建筑原 `Bubble`，共用绿色文字、暗色描边和底部中心 pivot。居民隐藏或场景暂停输入时不显示名称。
- 正式 UGUI dry-run/write、Metadata dry-run/write、原生场景绑定、Unity 编译通过。实际移动到铁匠与旅店并重新登录验证名称，截图为 `forge-title.png`、`hotel-title.png`，后者同时可见旅店老板、牧师和商人名称。
- 本地编码、架构、Editor、Actor 检查均 0 errors；独立只读复核未发现阻断问题。任务保持 60105 Active，金币、库存、治疗记录等存档数据未改变。

