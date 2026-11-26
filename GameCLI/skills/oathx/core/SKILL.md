---
name: core
description: 使用或维护 com.oathx.core 的 XApp、插件、XEventModule 生命周期与事件机制，接入基于 RuntimeSystem 的业务系统。
---

# Oathx Core

必须遵守 [开发规范](../development-rules/SKILL.md)。工程定位见 [Oathx](../SKILL.md)。

## 源码入口

必须先读 [development 学习入口](../references/development/README.md)、[配置与启动](../references/development/CONFIGURATION.md) 和 [框架契约](../references/development/FRAMEWORK.md)。执行源码漂移检查；配置读取必须沿用 cfgBundle/bundleSource → 资源 → IConfigData → Excel 字段的机制，不按类型硬拼资源目录。

开发、重构或审查 GamePlugin 时，先读 [GamePlugin 参考工程分析与标准](references/game-plugin-standard.md)。学习 development 的安装职责分解，同时核对目标框架重载契约；不得把参考工程的历史直接引用或拼写视作用户最新规范的例外。

阅读包内 `Runtime/XApp.cs`、`XPlugin.cs`、`XPluginManager.cs`、`XEventModule.cs`、`XEventDispatcher.cs`；业务侧对照 `Assets/Scripts/Systems/RuntimeSystem.cs` 及目标系统的配置入口。

## 系统契约

- 应用事件统一经 `XPluginUtility`（定义在框架 XPluginManager.cs 内）。FireEvent 同步派发、PostEvent 入队；GameUISender 的 Send/Post 分别使用这两个入口，不保留仅用于派发的 dispatcher 引用。事件入口统一不授权改变原有时序。
- 系统的初始化事务封装为带业务名的 InstallX/UninstallX，避免隐藏框架 Install/Uninstall；OnActive 订阅后直接返回 bool InstallX 的结果。只有订阅的系统不增加空安装方法。完整准则见共享开发规范。

- 当前 RuntimeSystem 继承 XEventModule。Active 先设置激活状态，再调用 OnActive；Detive 清除激活状态，再调用 OnDetive。不要绕过框架入口手动模拟状态。
- 保持 `OnActive(): bool`、`OnDetive(): void` 等继承签名。异步操作的发起与完成分开，OnActive 返回不代表资源或网络已经完成。
- 按当前系统的订阅时机配对清理；OnDetive 处理该系统页面及资源。`UnsubscribeAll` 与页面关闭之间空一行。
- 业务系统类名应与现有配置加载入口匹配；修改类型名时检查配置源表与导出数据，不能只改 C#。
- XApp 管理启动，业务系统管理自身行为，不为新页面复制应用启动或网络管理代码。
- XLauncher 是普通 UGUI 组件，XApp 使用私有序列化引用。具体要求见 [Launch](../system/launch/development/SKILL.md)，不能因 Core 存在单例基类而把 XLauncher 改为单例。

修改后编译并检查 Console；生命周期变更应验证激活、停用及重复进入时的资源和订阅行为。
