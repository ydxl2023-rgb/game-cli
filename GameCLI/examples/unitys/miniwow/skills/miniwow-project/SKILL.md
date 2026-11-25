---
name: miniwow-project
description: 在 GameCLI 的 miniwow 项目中开展小骑士冲冲冲机制学习、资源台账维护、系统设计和获授权的 Unity DEMO 开发。维护本项目的证据、目录和产品适配约定，不替代 Oathx 共享框架规范。
---

# Miniwow 项目规范

## 屏幕与布局约定

Miniwow 统一采用 **1080×1920 竖屏**设计分辨率。Studio 项目画布、Unity CanvasScaler 参考尺寸、启动页和后续系统页面保持一致。1920×1080 标准模板仅作结构参考，适配时重新布局，不能非等比拉伸整页。背景按比例铺满并检查裁切，关键文字和交互适配安全区域；宽高比变化须验证。Launch 是普通启动组件，不为它新增 System/Entity。

## 定位与修改范围

项目根目录是本 Skill 向上两级的 `miniwow/`。目标是学习《小骑士冲冲冲》的机制和设计技巧，以 GameCLI/Oathx 实现自己的可玩 DEMO，再从实际实现中提炼可复用系统模板。

`client/` 从干净 Unity 基线建立，现已按用户授权接入 Launch 与 Home；从 `App.unity` 启动后加法加载 `Home.unity`。研究、资源恢复、文档和规范任务仍只操作相应研究目录。明确的客户端开发任务授权其指定范围，不要求用户重复确认。未明确的产品规则记录为待定；先推进不依赖这些规则的工作。

Home 当前环境来自 `Building_sdc` 静态场景和 6 个初始建筑模型；查阅 [主场景还原记录](../../tools/main-scene/DELIVERY.md)与[资源用途索引](../../tools/main-scene/RESOURCE-INDEX.md)。`HomeEnvironmentCanvas` 为已禁用的历史背景，不要重新启用遮住 3D 场景。初始模型不代表建筑业务已解锁；NPC、粒子、导航及原 UberShader 尚未恢复。更新场景采用 `MiniwowMainSceneSetup` 原生资产流程，UI 继续走正式 UGUI CLI。

MainPage 主界面现使用 `client/Assets/Data/Widgets/Home/UIHome.prefab`（旧 Home.prefab 已保留 GUID 改名）。开发入口为 [MainPage 实现记录](../../tools/home-ui/IMPLEMENTATION.md)：配置源 `config/Home.xlsx`，制作脚本 `tools/home-ui/build_uihome.py`，正式 UGUI 导入后执行 `bind-uihome.ps1`；旧 `tools/home/bind-home.ps1` 已停用。HomeEntity 管理本地状态，HomeSystem 管理场景与路由；未接入业务必须保留明确反馈，不能将入口事件当作系统已实现。

Town 第一阶段已接入，见 [Town 开发与验收记录](../../tools/town/README.md)。**场景生命周期现归 TownSystem，HomeSystem 只管理主 UI 与路由并等待 Town 就绪**，取代上段历史场景归属。TownScene 持有序列化场景引用；TownEntity 管理 ActorEntity 集合并统一推进逻辑；ActorEntity 保存身份、位置、路线与阶段，ActorShape 继承 XActionStateMachine，每个角色拥有独立表现状态机，单向读取实体并驱动 Transform 和 Spine，隐藏或重建显示不得重置实体。旅店与铁匠恢复 idle 表现；访客使用 hero_20025。首版点击旅店店员走向入口，点击到达的访客返回；不代表建筑已业务解锁或消费成功。Spine Runtime 属第三方代码，不纳入自有代码格式化。

可控玩家首版已接入，见 [玩家实现与验收](../../tools/player/README.md)。玩家实例 4 使用原作 role_01，与访客和店员分离；通过固定摇杆、ActorEntity 与静态 NavMesh 约束移动，ActorShape 状态机播放 idle/run，镜头跟随实体。参数源为 propActor.xlsx，导航已恢复原包 30 瓦片并逐项校验字节/哈希，使用原生 NavMeshAgent.Move；禁止再次以可见模型重烘焙替代原数据；未实现水域、战斗或玩家建筑消费。

建筑业务继续采用用户确认的集合式 BuildingEntity：内部持有实际建筑状态，实例 ID 与配置 ID 分离，对外提供快照和校验操作，不暴露可变集合、不保存场景 GameObject。Town 只消费业务结果；本轮尚未实现该实体及升级经济。

最近提出的“探索 → 行动力消耗 → 旅店恢复 → 制造补给 → 再探索”是首个闭环建议，不是已经批准的完整功能清单。不要自行扩展为全部养成、商业化或在线服务开发。

|目录|职责|
|---|---|
|`client/`|独立 Unity 客户端；`LocalPackages` 为独立框架快照，并非 SVN 分支或与 development 共用的可写目录|
|`config/`|本产品配置源文件；开发前检查现有内容与导表链路|
|`server/`|服务实现位置；目录存在不代表已有原作服务器或已决定联网方案|
|`wxecf0e106809a477d/`|原始参考包；保留原文件与哈希|
|`extracted-resources/`|按证据恢复目录的资源及 `path-manifest.json`|
|`research/`|机制证据、资源目录、反编译代码和学习文档；不作为 Unity 编译目录|
|`tools/`|提取、索引、台账与验证工具；复用既有脚本，避免重复实现|

## 按任务读取资料

- **理解机制或代码**：先读 [代码研究入口](../../research/code-study/README.md)，再选 [玩法证据](../../research/code-study/GAMEPLAY-STUDY.md) 或 [架构证据](../../research/code-study/ARCHITECTURE-STUDY.md)。需要更早的文本规则时查看 [MECHANICS.md](../../research/MECHANICS.md)。
- **定位资源及用途**：读 [资源提取记录](../../research/RESOURCE-EXTRACTION.md)；使用 `tools/find_resource.py`、资源目录和映射清单。维护数据库时读 [资源数据库说明](../../tools/RESOURCE-DATABASE.md)。
- **编写或审查 Unity C#**：必须读取并遵循 [共享开发规范](../../../../../skills/oathx/development-rules/SKILL.md)，再通过 [Oathx 索引](../../../../../skills/oathx/SKILL.md) 选择实际涉及的模块。
- **制作产品系统或模板**：再读 [系统开发流程](../../../../../skills/oathx/system-development/SKILL.md) 和所选系统的开发 Skill；用 GameCLI 读取模板当前版本、源码和配置，不以历史示例代替当前标准。
- **Unity CLI 操作**：读取当前环境的 `unity-cli` Skill 并确认工具可用。直接任务以明确的 miniwow 路径为目标；涉及 UI Studio 项目时必须先验证其 Unity 绑定，不能把 development 当作写入目标。

不要为了文档编辑加载全部代码、启动 Unity 或重新跑资源提取。生成资料通常被 Git 忽略；文件缺失时按已有记录复现或注明缺失，不能假定其他机器也有同样产物。

## 研究证据与产品设计分开记录

结论标明依据属于：包内文本/配置、静态代码、实际运行观察，还是本项目设计建议。引用文件、方法、必要的行号或资源 ID；记录版本/哈希及尚未覆盖的条件。

- `managed/decompiled/` 是反编译 C#，不是原始开发源码；存在外部类型解析提示。有歧义时对照原 DLL、IL 和 PDB，不直接修饰输出后宣称等同原作。
- IL2CPP 元数据名称不是方法实现；WAT 不是原始 C#。当前资料缺服务端实现、部分分包与插件，不能声称获得产品全部代码。
- 已检查的副本战斗链路是服务端战报驱动客户端动作播放；不能从飘字或血条代码推导权威伤害公式，也不能扩大到所有战斗模式。
- 新设计的伤害、掉落、离线结算或本地存档规则必须标明是本项目方案。用户选择离线实现时，通过独立规则计算产生结果，再交给表现层，不伪装成原作服务器返回。
- 阅读原代码时关注输入、状态所有者、变更时机、失败处理、生命周期清理；教学文档解释这些设计取舍，不只是罗列类名。

## 资源与台账

选用资源前核对文件内容、类型、原始路径依据、用途、哈希和依赖。恢复的 JSON/序列化数据不等于可直接导入的 prefab、材质或动画。

`_subassets`、`_unmapped` 表示原始路径证据不足，不为整齐而冒称已恢复原目录。新增提取/转换结果保留源资源 ID、工具与参数、输出位置、校验信息，并维护对应索引。人工用途确认与自动推断分别记录。

数据库使用既有 `miniwow_resources` schema 和连接助手；凭证不复制进项目。自动同步不得覆盖人工 annotations；数据库保存路径与元数据，不默认上传二进制。资源在台账中存在不代表已导入 Unity 或已完成可见效果验证。

## GameCLI 实现边界

以下是项目应用共享规范的要点，详细规则以当前共享 Skill 为准：

- **Entity** 拥有业务状态、校验、规则、条件计算与事务；**System** 编排流程和通知；**UIWidget** 接收用户意图及刷新、维护 **VM** 的展示绑定。System 不读取或缓存 VM。
- 产品业务系统统一继承 `Assets/Scripts/Systems/RuntimeSystem.cs`，由它继承 XEventModule；HomeSystem、TownSystem 已采用此结构。基类沿用 development 的 LoadSystem/UnloadSystem 调度接口，帧末回调不代表场景或 UI 异步就绪，TownReadyChanged 等业务就绪通知必须保留。GamePlugin 仍负责初始装配，不为使用基类而重复加载系统。
- 手写系统代码按 `<System>/<System>System.cs`、`<System>Entity.cs`、其他逻辑、`VM/`、`UI/` 组织。按用户约定，Home 的非业务通用组件（HomeSafeArea、HomeSceneCamera 等）放在 `Home/Common/`。GamePlugin 只承担插件生命周期与依赖装配，UISender 使用独立 GameUISender 类实现，不由 GamePlugin 直接实现；GameUISender 与 GameUILoader 均放在 GamePlugin.cs 中，独立职责不要求拆分文件。不要照搬原作 Proxy 单例或 FairyGUI 结构。
- 数值、配方、开放条件、奖励和可配置展示先建立本产品 Excel 源表与稳定 ID，再导表并接入。固定文案遵循共享多语言规则；不要直接修改 development 的业务数据作为本产品实现，也不以运行时硬编码掩盖缺表。
- 复杂构造与对象初始化器使用多行排版，每个成员赋值独占一行。配置对象先用有意义的局部变量构造，再传给 Startup 等方法；不要把嵌套构造、多项初始化与方法调用挤在同一行。
- 优先复用现有框架的生命周期、事件、资源加载、Widget/VM 和列表能力。业务需求先在本产品实现；框架包修改须有具体缺口与影响范围，不顺手改其他项目。
- UI、prefab、Metadata 和资产变更使用正式 UnityCLI/AssetDatabase 流程，不手写 Unity YAML。导入只覆盖已选资源及依赖，不能把研究目录或完整反编译工程放入 Assets。
- 明确每个系统的输入、输出事件、配置、状态/存档所有权、依赖与失败结果。模板和产品实例分别管理；仅在用户要求发布时执行模板发布流程。

本项目 Skill 放在项目内，是用户指定的独立产品规范。可跨产品复用的框架/系统 Skill 仍维护在 GameCLI 的 `skills/oathx/`，不在这里复制另一套共享规范。

## 验证与交付

研究任务校验提取覆盖、文件哈希及证据引用；声明未改客户端时核对实际变更范围。文档任务检查链接与约定一致性即可，不启动 Unity。

获授权的开发任务按改动执行 Unity 编译、必要运行交互和状态验证。探索重点检查退出后计时器清理；恢复/制造检查容量、库存和重复请求；战报播放检查播放速度或跳过表现后最终状态一致。具体用例随已实现范围确定，不将这些建议当作已经通过的测试。

交付说明实际改动、验证结果及缺口，区分“导出成功”“编译成功”“运行验证”“模板验收”。原有基线文档里的历史通过记录不能代替本次验证。

沿用 `.gitignore`：Unity Library/Temp/Logs/Obj、构建产物、局部工具及大体积研究输出不提交；保留正式 Assets/.meta、Packages、ProjectSettings、产品配置源文件、研究文档和复现脚本。Git 提交/推送、数据库模板发布以用户当次授权为准。

玩家摇杆按用户确认固定于主界面底部；只在按下时检查 UI 阻挡，已捕获手势经过其他 UI 不取消，松手归中。页面主动关闭输入、失焦及暂停仍取消。
