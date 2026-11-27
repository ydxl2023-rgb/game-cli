# 铁匠系统接入（2026-11-27）

## 交付范围与原作依据

独立 `BlacksmithSystem`（10008）按需激活；建筑 60002 的 NPC 入口和任务导航都通过事件进入系统。原作 `UIEquipmentShopView`、`ForgeProxy`、`UIForgeView`、`UIForgingView` 和配置表是业务依据，完整源文件摘要保存在 `configuration-source.json`。`original-ui-audit.json` 记录解码的原作页面区块、尺寸和直接引用。

保留原作任务 60329 领取后解锁、首级修复 1100 金币、五种首级配方、每次 1–10 件、材料费用、售价和英雄职业限制。大剑每件需要铜锭 12、微光碎片 5、金币 67，售价 113。首级配方为 110001–110005，新增同类配方不需要编写任务或交易分支。

当前是可执行的离线首级接入，不是原作全部铁匠功能：装备使用唯一实例 ID，暂不生成随机词缀，也不计算战斗属性；本地确认后直接完成英雄交易，未接入原作英雄走到柜台的交易状态机；替换下来的武器回货架，收入单独收取。高等级升级、图纸消耗、拆解、锁定、推荐英雄、强化及原作打造 Spine 弹窗不在本轮实现范围。打造保留原作 1 秒提交后表现时长，当前为页面反馈。缺少的微光碎片应来自任务/探索，不能在生产逻辑中自动补给。

## 职责与生命周期

- BlacksmithEntity：配方选择、数量、装备实例选择、解锁、材料/职业/金币规则。
- HomeVillageSave.blacksmith：货架、装备归属、生产/销售累计、收入；与玩家材料、金币、建筑等级、英雄金币一起原子保存。旧存档缺少字段时初始化为空铁匠数据。
- HomeVillageEntity.ExecuteBlacksmith：隔离快照 → 规则验证 → 一次 Commit → 清除待确认选择。重复确认不再扣款。
- BlacksmithSystem：独立订阅事件，创建/关闭所属页面，提交实体事务、发布通用任务观察；没有引用其他具体玩法 System。
- HomeSystem/HomeRuntime/HomeScene：入口、任务导航、玩家输入、镜头和建成模型。UI 打开时聚焦场景铁匠，关闭时恢复玩家跟随。
- UIBlacksmith/UIBlacksmithConfirm/UIBlacksmithEntry 与各自 VM：显示、列表和用户意图；没有经济写入。

非缓存页面必须先 TryGetWidget 复用。异步打开保留 TaskPresentationAction 所有权，取消时使用框架 CloseWidget 取消加载。成功加载才报告 HomeFacilityOpened；卸载和返回登录关闭确认页与主页面。

当前 SyncScrollRect 的 Sync 会追加行，替换整个 VList 不会清理旧行，因此主页面只绑定一个 VList，按稳定身份判断是否需要 Clear/Add。正式代码不能靠重新分配 VList 刷新列表。

## 配置与资源

`config/propBlacksmith.xlsx` 包含 BlacksmithSetting、BlacksmithRecipe、BlacksmithCost、BlacksmithMaterial、BlacksmithHero。数值 ID 引用，名称和反馈进入 propCommonText；原始语言键先通过 tblanguage.SimpleChinese 解析。

`collect.py → build.mjs → ../town/export-scene-config.ps1` 为配置重建流程。`verify.py` 检查源摘要、全部导出字段、外键和语言文本。已有非任务语言变更通过显式 before/after 清单验收，禁止放宽全局语言保护检查。

预制位于 `Assets/Data/Widgets/Blacksmith`，复用行位于 `Widgets/Common/Micros/UIBlacksmithEntry.prefab`，物品图标位于 `Widgets/Common/Icons`。`imports.json` 记录原路径、目标路径、资源 ID、SHA256 和用途。建成模型 `WeaponStore/WeaponStore` 的来源在 `../main-scene/weapon-built/asset-manifest.json`，由 Unity 原生 API 绑定 Home 场景，不编辑 YAML。

UI 重建顺序：`design.py → install.ps1 → bind-metadata.ps1`；每次正式导入和元数据绑定先 dry-run。随后等待 `bind-result.json` 成功。长屏背景锚点必须从顶部 570 延展到底部，不使用仅底部固定尺寸；列表模板必须保持 inactive，ScrollRect 绑定正确 Viewport/Content。

## 任务观察

已连接 BuildEquipmentTimes、ForgeAnyEquipment、TradeEquipmentTimes。BuildEquipmentTimes 和 TradeEquipmentTimes 通过 itemCounts 发布各配方的绝对累计，TaskEntity 按 ConditionItems 中的装备 ID 集合去重求和；ForgeAnyEquipment 使用总产量。重复报告不会重复增加，生产逻辑未绑定特定任务 ID。当前列表项 Num 必须为0，其余语义显式拒绝；带装备品质、档位、星级参数的累计条件仍需在对应装备规则实现后接入，不伪造随机品质证据。

## 验收与用户进度

运行原始证据：`runtime-evidence.jsonl`。配置证据：`configuration-validation.json`。代码检查：`../hospital-ui/blacksmith-local.json`、`project-local.json`、`architecture-local.json`、`editor-local.json`。

用户进度备份位于工程 Library/OpeningQA/blacksmith-user-checkpoint.json；禁止覆盖。测试夹具解锁 60329、提供测试材料，仅用于 Editor 验收，不能作为正常入口。交付前必须恢复备份并再次验证锁定入口，不能把测试存档交给用户。

本轮已完成原档恢复和锁定入口复核。用户金币18460、旅店/医院等级1、英雄金币499950等进度保持原值，恢复后重新登录保存的三个偏好键与原始 checkpoint 文件逐字一致；结果见 user-restore.json。Unity 最终停在非运行状态。
