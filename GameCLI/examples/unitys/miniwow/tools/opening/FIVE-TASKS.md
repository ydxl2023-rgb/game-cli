# 加伦招募至医院治疗：五个后续任务

日期：2026-11-26。范围为 60004、60101、60102、60103、60104，完成后跟踪 60105；本轮不实现探索事件或旅店生产。

## 体验与实际业务

|任务|交互与完成依据|经济结果|
|---|---|---|
|60004 招募加伦|显示原剧情 3044 文本，接收英雄 125；获得确认视图关闭后自动领取|真实英雄归属；重复进入获得确认视图不重复授予|
|60101 修复医院|使用原剧情 3045 文本；医院 60018 等级由 0 到 1|扣 280 金币；领取任务奖励 20 金币|
|60102 购买宁神花|购买 1 份物品 77001；累计购买次数为 1|扣 30 金币，先获得 1 份；领取后另发 50 份及 10 金币|
|60103 生产微效药剂|医院已修复且有原料；执行本地生产事务|1 份宁神花变为 1 份药剂 77101；领取任务奖励 10 金币|
|60104 加伦疗伤|前项领取后，加伦自动沿原 NavMesh 走向医院；实际抵达才消费；治疗结束报告完成|扣 1 份药剂；英雄付 50，记入医院待收收入；治疗 4 秒；领取任务奖励 10 金币|

本轮复用 UIHomeOpening 的原生 UGUI 对话与确认面板承载操作，按钮和库存文案均来自 CommonText。完整原作英雄获得页、交易行页和医院生产页未逐页复刻。治疗期间按钮禁用，界面显示生命百分比及剩余时间；面板期间隐藏摇杆，避免与对话框重叠。五项结束后恢复 Home 操作。

## 原作依据与本地适配

原始依据是已恢复的反编译 C# 与配置，不是完整客户端开发工程或服务端源码。

- `GuideStoryController.Story4/Story5`：英雄 125、FirstHeroPoint、原剧情 3044/3045；Story4 的 20/100 是演出血条。
- `tbhero[125]`：石盾加伦，模型 hero_20025。复用现有 Actor 实例 1，配置身份改为 125；不新增第二个同样的访客。
- `tbbuildinglevel[60018001]`：医院一级 `priest/priest`，修复费用 70001×280，Goods.Craft=77101、Cost=77001、CraftPrice=50、CutCdSec=1。
- `tbshop[Id=6001]`：77001×1，价格 70001×30。
- `ConsumableProxy.CraftConsumable` / `UIConsumableShopView.OnClickMaterial`：材料请求交服务端，不能从客户端推断完整生产比例、容量规则。
- `ConsumableProxy.GetGoodUseTime/GetGoodUseValue/GetGoodPrice`：77101.Time=5 减医院 CutCdSec=1 得 4 秒；单档 Goods 对应 ResponseEffect 最后一个值 100；50 是英雄消费售价，不是玩家制作费。
- 任务完成条件与奖励继续沿用 `propTask.xlsx`，未直接写入任务完成状态。

明确的本地规则：

1. 初始玩家余额沿用已有 `Home.xlsx → Home.json` 配置，本机为 19000；没有伪造一笔原作“退伍金”赠送。原作该笔初始资产来源仍未证明。
2. 离线治疗使用独立的生命百分比，初始 20%，不把演出数值写成英雄战斗 HP 或成长属性。
3. 生产暂定即时 1 材料→1 药剂，通过 `VillageSetting.MaterialCost/OutputCount` 显式配置；没有声称这就是原作服务器配方。
4. 英雄本地个人余额取 `tbglobal.HeroInitialGold=500000`；该值是否就是原作正常服务器初始化结果仍未知。
5. 本地 UseCostItem 在治疗结束报告；原作服务端计数确切时点未知。医院收入单独保存，本轮不实现收入收取功能。
6. 治疗使用持久化 UTC 截止时间，离开游戏仍经过时间；重新进入后结算一次。NPC 行走退出后从初始招募位置重新寻路，不保存每帧位置。

配置证据和源文件哈希：`village-config-evidence.json`。验证器 `verify-village-config.py` 对照原表的费用、商品、治疗参数、任务顺序和语言引用。

## 结构与生命周期

|组件|职责|
|---|---|
|HomeEntity.village|拥有 HomeVillageEntity 的生命周期；原有 Actor 集合仍统一推进移动|
|HomeVillageEntity|库存、购买/生产计数、任务奖励收据与跨子实体事务；单个持久化记录同时保存成本、结果、收据|
|BuildingEntity|集合式建筑等级查询和医院构造规则；当前实例键与原建筑配置 ID 同值 60018，不把它当成数组下标|
|HeroEntity|英雄身份、生命百分比、个人货币与招募/治疗规则；不持有 Shape|
|HomeSystem|具名事件订阅、用户操作、奖励接收方注册/注销、刷新 Widget；不引用其他具体业务 System|
|HomeVillageRuntime|HomeRuntime 内的场景执行类；安装真实 NPC 路线、确认抵达、驱动治疗时钟和业务结果通知；不是新玩法系统|
|ActorEntity / ActorShape|逻辑实体接收完整路径；Shape 状态机单向显示。FollowRoute 拒绝停止实体、手控实体和无效路径|
|HomeOpeningScene|按真实建筑等级显示建成医院，按开场阶段显示加伦；不改变业务等级或英雄归属|
|UIHomeOpening|拥有 VM、固定序列化引用、意图发送及语言格式化|

参考 `development/Systems/Shop/ShopEntity` 的“提交时重新校验费用与输出”、Mail 领取收据，以及现有 Widget/Entity 机制。产品差异是当前五项共用 Home 内的本地村庄事务，不复制样例商品、Proxy 单例或另建 TownSystem。

HomeVillageEntity 先计算独立事务副本，持久化成功后更新状态。HeroEntity、BuildingEntity 在普通交易中保持实例身份，原位恢复提交数据。任务奖励先持久化经济收据，再由 TaskEntity/TaskSystem 保存 Claimed；中间重启后再次领取不会重复发放。

村庄键由 `StorageKey.For<HomeSystem, HomeEntity>(e => e.village)` 生成。退出 Home 停止 Runtime、解绑 Actor、关闭自有 UI、注销奖励接收方。治疗的业务截止时间仍保留；下次入场重新建立运行时。

## 资源与配置链路

- 新配置源：`config/propVillage.xlsx`，含 VillageSetting 和 VillageTask；语言补充到 propCommonText。
- `build-village.mjs` 用 artifact-tool 生成源表，`tools/town/export-scene-config.ps1` 调用项目 e2cs 导表。
- 原语言生成器会按文本去重；本轮新增按钮文本与旧任务描述相同，可能改变引用的语言键。校验器要求旧文本仍在导出语言表中，且原任务逐条文本校验继续通过，不允许丢失或改写文本。
- `export_scene.py --variant hospital-built` 只提取选定的原医院模型。`MiniwowVillageSetup` 使用 AssetDatabase、SerializedObject、PrefabUtility 增量绑定，不重建 Home，不手写 YAML。
- `priest_initial` 是救火救人后的未修复外观；真正修复后改为 `priest/priest`，二者不能混同。
- `FullBody/Conversation_hero_20025_2` 未恢复到本地。同名 FairyGUI Conversation_hero_20025 的图片视觉不符，已弃用；当前明确使用与模型一致的 `ui://CharacterIcon/hero_20025` 原作立绘作为替代。
- `village-imports.json` 记录 9 个模型依赖和 1 张立绘的来源、用途与哈希。原 Spine 和 NavMesh 继续使用此前已验证的 Town/玩家导入结果。

## 验收与复现

`verify-village.ps1` 通过实际 ContinueClick 按钮执行流程；没有通过注入任务完成事件跳过业务。测试先使用保留的玩家检查点建立五任务隔离回放；完成后恢复检查点，不把验收档当作用户新进度。

运行证据：`village-evidence.json`。截图：`village-recruit.png`、`village-hospital.png`、`village-craft.png`、`village-healed.png`。

覆盖：招募后确认前重启；修复后领取前重启；购买后领取前重启；生产后领取前重启；治疗过程中返回登录并重启；最终领取后重启；奖励接收方收据恢复；重复领取；金币或材料不足；实体引用稳定；已销毁 Actor 与无效路径被拒绝。

`MiniwowVillageProbe.ValidateRules` 只用于 Editor 隔离测试，临时数据在 finally 中恢复；不是正式游戏操作入口。原生产规则、完整经营页、泛化英雄成长和任务 60105 探索均不在此验收范围。

本轮结果：五项实际任务均达到 Claimed，下一项 60105；恢复玩家检查点后为 60104 Completed、待领取。GameCLI 最终 code/architecture 均 passed，运行 Console 零错误。Editor 代码检查零 error；唯一 OX015 review 是编辑器作者脚本查找节点后写入序列化引用，不是运行时 UI 查找。项目已有的七项 review 未新增错误。详见 `village-acceptance.json`、`village-final-code.json`、`village-final-architecture.json`、`village-editor-code.json`。

再次验收前，正常进入 Home 后执行 `MiniwowVillageProbe.SaveCheckpoint` 保存当时的三项存档，再运行 `verify-village.ps1`。不要在回放进行中重新保存检查点，否则会把测试进度当作玩家原进度。若测试中断，使用 `RestoreCheckpoint` 后停止并重新进入恢复；脚本的正常成功路径会自动恢复。

独立 Actor 静态审查发现并修复了 FollowRoute 可重新激活已销毁实例、交易重建英雄实体的问题；静态复核通过，运行证据单独保存，不能混称为完整系统独立验收。

数据库工单：MW-014、MW-016、MW-019 对本轮交付进入 review；MW-018 仍有小鱼购买、MW-020 仍有探索行为未覆盖，保持 in_progress。其余旧工单状态不因本轮五任务回归自动标记完成。
