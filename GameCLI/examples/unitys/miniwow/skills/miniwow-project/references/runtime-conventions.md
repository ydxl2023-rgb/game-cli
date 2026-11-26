# Miniwow 当前运行时约定

2026-11-25 整理。这里记录当前产品决定，旧 IMPLEMENTATION/研究记录仍可追溯，但不能作为恢复旧架构的依据。

## 入口链路

Launch → GamePlugin 读取配置 → 按 FunctionSystem 的 InstallOnStartup 安装 Master 与 Login。Home 不标启动安装。Master 常驻是产品职责；InstallOnStartup 本身不是常驻标志。

Login 发 LoginSucceed，Master 按配置 ID 激活 Home，保留 Login。HomeSystem.OnActive 订阅事件后 `return InstallHome()`；InstallHome 持有 OpenHome 协程，协程创建 HomeRuntime 并等 Load 结束。场景、Home 配置、Actor 初始化及 UIHome 初始化完成后发送 HomeReady；Master 再卸载 Login。异步安装返回 true 不代表 IsReady。

OnDetive 取消订阅后调用 UninstallHome，取消初始化协程、关闭所属 Widget（使用局部 widgets 数组）、请求 RuntimeManager 销毁 HomeRuntime。失败回到保留的 Login，不能假装已进入游戏。Master 监听 LoginSucceed、HomeReady、ReturnToLogin、SystemActive、SystemDetive，不访问 Home 的内部 VM 或角色。

## 所有权

|类型|职责|不能承担|
|---|---|---|
|GamePlugin|配置和基础设施装配，系统加载/卸载|具体 Home/战斗业务阶段|
|MasterSystem|常驻事件协调、按配置系统ID调度|缓存 LoginSystem/HomeSystem 实例、平行 MasterPhase 状态机|
|HomeSystem : RuntimeSystem|入口事件、页面、场景生命周期编排|VM 字段修改、角色数据规则|
|HomeEntity|Home 配置快照、入口规则、Actor 集合和逻辑推进|场景对象/表现引用|
|ActorEntity|单角色数据、位置及行动规则|Spine/Transform/UI|
|ActorShape : XActionStateMachine|每个角色独立表现状态机|重置实体、权威结算|
|HomeRuntime : GameRuntime|Load/Ready/Unload/Stopped 中管理场景资源|成为 FunctionSystem 玩法模块|
|GameRuntime : XActionStateMachine|公共状态机钩子及推进|预建无需求战斗逻辑|
|RuntimeManager : XSimpleSingleton|按类型创建、查询、释放 Runtime|替代业务 System、永久保留退出场景|
|HomeScene/HomeNavigation|序列化场景引用、输入/导航适配|复制实体规则或跨系统访问|

CreateRuntime 复用活动实例；DestroyRuntime 先移出活动注册，再 Shutdown，待 IsStopped 后销毁组件。DontDestroyOnLoad 保障异步清理，不表示返回登录后继续运行。GamePlugin 关闭清理剩余 Runtime。未来 FightRuntime 继承 GameRuntime；目前不预建战斗实现。

扩展 Runtime 时，重写 InitStateMachine 必须先调用基类注册公共状态，再注册业务状态。启动失败、加载中取消及正常退出最终都必须到达 Stopped，否则管理器会永久等待销毁。加载和清理所需场景路径在启动时从配置取得并由 Runtime 持有，不依赖退出时配置表仍可用；验收必须包含失败启动和加载中退出。

HomeEntity 合并原 TownEntity 的角色集合，ClearActors 只释放角色，不重置 HomeSnapshot。HomeScene、HomeNavigation 放 Home/Scene；HomeSafeArea、HomeSceneCamera 放 Home/Common。迁移保留 GUID；导航资源的历史 Town 目录不等于还存在 TownSystem。

## 配置、路由和事件

- Scene/Actor/FunctionSystem/公共文案使用产品源表和生成 ID；资源与配置无证据不得硬编码兜底。
- Home.xlsx 的目的地在读取边界经 StringEnumConverter 转成 HomeEntryRoute，实体和 HomeEntryRequested 全程枚举。Settings/Task/Chat 为 HomeShortcut。None 不是合法配置目的地。
- 固定反馈来自 propCommonText.xlsx → e2cs → LanguageConfig → CommonText.Get。propLanguage.xlsx 是当前导表链生成物；有翻译时先解决保留/合并，不能直接覆盖。
- UIHome 是原 MainPage 的产品预制，位于 Assets/Data/Widgets/Home/UIHome.prefab。配置目的地存在或发出了事件不表示目标业务已实现，未接入仍显示语言表提示。
- 应用事件只通过 XPluginUtility。FireEvent 立即派发，PostEvent 入队。GameUISender 与 Loader 同文件但独立类；不直接继承到 GamePlugin，不缓存 dispatcher。
- HomeReady 当前采用 FireEvent；历史提出 PostEvent 后已按时序选择纠正。任何改变排队策略都需检查返回/卸载时的行为。

## 已废止方案

TownSystem、TownEntity、IGameRuntime、MasterPhase、SystemCompositionRequested、LocalGamePrepared、SessionGameplayChanged 不属于当前架构。不能因历史文档或旧截图重新引入。没有独立业务数据时不创建 MasterEntity 来维护会话阶段。

不增加 requestedActive/ready/generation 来重复框架已有状态。Home 通过 Coroutine 句柄取消、UISystem 取消加载及 Runtime 状态释放；具体不可取消回调的问题需单独说明，不将代次计数器当标准模板。

## 输入与尚未完成范围

角色使用原作资源与恢复导航；摇杆固定位置，按下检查 UI 阻挡，已捕获手势跨其他 UI 继续移动，松手归中。暂停、失焦、页面关闭释放输入。Spine 表现已接入的范围见 tools/town 与 tools/player；不代表完整原作 AI、战斗或建筑经济。

BuildingEntity 作为建筑集合的设计已确认；建筑解锁、升级、交互经济与战斗循环仍须独立实现和验收。资源用途索引、数据库 annotation、原始路径和哈希继续维护；已提取不等于已导入或已可玩。

## 验证入口

- tools/login/verify-session.ps1：登录交接、失败重试、返回与取消。
- tools/home-ui/verify-uihome.ps1：入口、地图往返、锁定/未接入反馈；脚本末尾快速重启需再查最终状态。
- tools/town/verify-runtime.ps1：Runtime 注册与失败释放。
- tools/player/verify-player.ps1、tools/town/verify-town.ps1：输入、角色及场景行为。
- GameCLI verify code + verify architecture：配合编译；不能用以前的通过记录替代本次验证。

这些是复现入口，不表示本次文档整理重新执行了全部 Unity 运行验收。
