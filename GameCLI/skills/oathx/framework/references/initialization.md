# 最小运行时初始化

## 生成器与代码约定

Widget VM 使用 partial 类和私有 `[Binding]` 字段，属性由 WidgetGenerator 生成，不能手工伪造生成成员解决编译错误。

生成器源码位于 `LocalPackages/com.oathx.widget/WidgetGenerator~/WidgetGenerator`，当前目标 netstandard2.0、Roslyn 3.8.0。**构建前读取 csproj**：当前 `CopyToUnityAssets` AfterTargets 根据 SolutionDir 计算 Assets 路径并覆盖 `.meta`。不能直接在未知工作目录 build 导致写入另一个工程或改变 GUID。

可在 `Library` 下的隔离构建目录复制生成器源码，仅在该构建副本移除自动复制 target，再构建并手动部署 DLL 到目标 Assets；不改共享包原始构建脚本。已有 DLL 比较哈希与版本；保留目标 GUID。通过 Unity 的插件导入设置/AssetDatabase 工具设置 `RoslynAnalyzer` 标签及正确平台导入状态。只保留一份生成器，排除重复 DLL、重复 GUID 和无效 meta；不能直接照搬参考工程异常的导入配置。

验证一个真实 VM 的生成属性可编译且 Metadata 能绑定。没有生成属性时查编译器兼容、Analyzer 标签、程序集作用范围和插件导入状态。

旧版 Text UI 按目标工程约定启用 `USE_OLD_TEXT`（检查 PlayerSettings 与响应文件，不盲目重复追加）。使用 `UnityEngine.UI.Text`、SyncText；TMP 项目按明确需求保留其现有模式。不要仅凭参考工程文件是否存在判断宏已生效。

## 启动链与基础场景

先读 `XApp.cs`、`XAppConfigure.cs`、`XPlugin.cs`、`XPluginManager.cs`、`UISystem.cs`。参考 `Assets/Scripts/GamePlugin.cs` 了解接口调用，但它包含 SDK、通信、配置表与具体游戏系统，**不能整份复制作为空工程模板**。

新项目建立最小插件（名称避免与已有插件冲突）：

- 继承 XPlugin，实现必需的 Install、Startup、Update、Shutdown/Uninstall 签名，以当前基类为准。
- UI loader 按现有 UILoader 契约通过 XRes 加载；启动 UISystem 时配置 `ResourceConfigure { loader, cacher = new UIDefaultCacher() }`。
- 只注册实际使用的 Fancy Sync；实体和 RPC 仅在验收页面确实依赖时初始化，不自动启动旧 Host/Login 流程。
- Startup 载入最小 RuntimeSystem，按 Core 的 Active/OnActive、Detive/OnDetive 生命周期工作。退出清理系统、UI 和由插件拥有的资源。

通过现有 Editor/UnityCLI 资源操作建立并保存：

1. 一个 XApp、所需相机、Canvas/UISystem、EventSystem；检查重复组件和输入模块配置。
2. UISystem 的层级容器按实际 WidgetLayer 枚举及实现命名。不能凭记忆只创建 center 而遗漏背景层。
3. `Assets/Resources/XAppConfigure.asset`，由 ScriptableObject 的创建接口或 Editor 工具创建；plugins 填真实可加载的插件类型全名。
4. 本地初始化通常选 `AppRuntime.Mono`、`AppBundleSource.Local`、hotfix=false，关闭 `clearLocalEntityArchiveOnStartup`；不是把生产凭证复制进新配置。先核对 XApp 当前本地模式路径确实跳过远端版本/下载请求，缺少配置不得伪装成功。
5. Launch 的背景、Loading/Update 及 XLauncher 绑定按 Launch skill，通过工具生成/配置，XApp 私有 launcher/mainCamera 引用必须有效。原 XBootstartup 不是替代入口。
6. 一个最小 UIWidget/VM 页面，用于验证生成属性、文本和按钮绑定。只使用本地已授权资源，不自动导入 SV Login 的游戏美术和业务。

XApp 从 Resources 按类型名读取 XAppConfigure，并根据 conf.plugins 查找插件类型。资源存在但插件名错误仍会无法启动；检查实际日志与页面。

原工程接入时复用现有插件/场景，不另建第二个启动链。编辑资产使用 CLI 或受控 Editor/AssetDatabase 接口；不手写 YAML。工具能力不足时明确补充所需 Editor 操作及验证，不能用运行时全场景查找代替固定引用。

## 可见行为

先显示 Launch；最小页面异步打开成功后调用 XApp.ShowLaunchBackground，只保留背景。需要离开背景的正式页面使用 HideLaunch。OpenWidget 必须传完成回调，系统调用前检查 IsActive，OnDetive 清理订阅并关闭页面。

SV Login 属于可选业务系统，若任务明确接入则读取 [对应 skill](../../system/sv-login/SKILL.md)。当前 LoginSystem/UILogin/VMLogin 已重命名，“开始游戏”是等待接入的占位入口，不应作为新环境联网成功的证明。
