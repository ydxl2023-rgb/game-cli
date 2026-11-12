# GameCLI 参考实现记录

本文件保存本轮整理前的实现说明，属于参考材料，不是新产品的强制需求或最新数据库状态。仅在维护 GameCLI 示例、复用具体实现细节时读取；可能包含历史目录、版本和绑定数量，使用前核对实际代码与数据库。产品开发入口见 [SKILL](../SKILL.md)。


# Loading

必须遵守 [开发规范](../../../development-rules/SKILL.md)、[Core](../../../core/SKILL.md)、[Widget](../../../widget/SKILL.md) 和 [Widget Extension](../../../widget-extension/SKILL.md)。本 skill 目录向上四级是 GameCLI 根目录，以下路径相对此根目录。

## 当前结构

- 仅保留 `examples/unitys/development/client/Assets/Scripts/Systems/Loading/UI/UILoading.cs` 与 `VMLoading.cs`。沿用现有模块目录，不创建独立 RuntimeSystem；skill 的分目录组织不代表运行时必须有对应系统类。
- 复用 `Assets/Data/Widgets/Loading/Loading.prefab`，不重建美术或替换动画。它与 Launch 的 Loading 子面板是不同界面。
- `Loading/Animator/RateBg/Rate` 是进度 Text；`TipText` 是提示 Text。预制体没有进度条，不把 RateBg、Ball 或 Arrow 当作填充条。保留原来的旋转及 UV 动画。
- VMLoading 使用两个私有 `[Binding(typeof(SyncText))]` 字段 rate、tipText。由生成器产生 Rate、TipText 属性，通过 Metadata 绑定，不使用运行时路径查找。
- UILoading 使用 WidgetLayer.topmost、cacheWidget=false。页面覆盖业务 UI，实际打开由 UISystem 管理。

## 功能边界

只实现基础页面：打开、关闭、真实进度和提示更新。调用方负责工作任务、完成条件及何时关闭。不模拟自动增长，不接登录通信，不改 LoginSystem，不在达到 100% 时自动卸载，不自动安装到启动配置。

- 调用方通过 UISystem.OpenWidget<UILoading> 打开，必须提供显式完成回调，在回调中初始化进度和提示。调用方若属于 RuntimeSystem，调用前检查 IsActive。异步打开期间的数据缓存及任务生命周期由调用方负责，UI 不新增独立状态管理层。
- UpdateProgress(progress, message="Loading...") 接受 0–1 进度；NaN 视为 0，越界值限制到 0–1。输出例如 `Loading...(35%)`。bool 表示已更新绑定的 VM；VM 尚未绑定时返回 false。
- SetTip(text) 更新提示，传 null 清空；首次打开没有调用 SetTip 时保留预制体已编辑的提示。
- 调用方完成任务或退出时先声明局部 `System.Type[] widgets` 数组，将 `typeof(UILoading)` 单独写一行，再调用 `UISystem.GetSingleton().CloseWidget(widgets)`，禁止内联数组，沿用框架 Unref 清理绑定。UILoading 不提供独立 OnActive/OnDetive，也不管理请求代次。

业务接入直接使用 UISystem 和 UILoading 的展示方法，不通过 dispatcher.Load/Unload 加载 Loading 模块。当前不新增实际加载任务或配置表启动项。

## 绑定与验证

修改后 Unity 编译并检查 Console。使用 UnityCLI `widget-metadata UILoading --vm VMLoading --prefab Assets/Data/Widgets/Loading/Loading.prefab --dry-run`，通过后正式生成；应匹配两项字段。不要手写 prefab YAML。

验证进度 0、部分、1、越界和 NaN、空提示；确认关闭后绑定释放，保留原美术和动画。此前独立系统的缓冲验证不再代表当前 UI 能力。基础页面离线检查不代表任何真实场景或资源任务已接入。数据库代码发布仅在明确请求时执行。

## 标准模板代码包

数据库 `SvLoading v15` 已归档 UILoading、VMLoading 及其 .meta，共四个文件，使用 `Assets/Data/Widgets/Loading/Loading.prefab`。同步清单为 `integrations/codex/config/loading-scripts.json`。后续更新读取实际最新版本；仅更新代码时保留标准模板布局，通过候选审核发布并核对 SHA256。

