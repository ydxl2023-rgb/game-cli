---
name: currency-development
description: Develop reusable Oathx currency bar and currency entry widgets, including host VM integration and independent template materials. No wallet or RuntimeSystem implementation.
---

# Currency 展示组件开发

先读 [开发规范](../../../development-rules/SKILL.md)、[Widget](../../../widget/SKILL.md) 与 [独立开发流程](../../../system-development/SKILL.md)。以产品需求为准；未指定变更时保留标准模板外观和行为，明确记录扩展点，不臆造钱包、充值或货币数据。

- 数据库模板 ID 为 `CurrencyBar`。虽归入系统模板库以携带 scripts，它只是 UI 组件，不注册 CurrencySystem。
- 原生预制在 `Assets/Data/Widgets/Currency/`，代码为 `Assets/Scripts/Systems/Common/UI/UICurrencyBar.cs`、`UICurrencyEntry.cs` 和 `VM/VMCurrencyBar.cs`、`VMCurrencyEntry.cs`。图片、字体与 GUID 都由 Currency 自己携带，不依赖 Home。
- 宿主嵌套引用 CurrencyBar.prefab，通过自己的 `[Binding(typeof(SyncMetadata))] VMCurrencyBar` 绑定。三项固定顺序使用已有 SyncGroup；不自写列表。CurrencyEntry.prefab 是单项复用参考，Bar 中三个项保留各自原始图片尺寸。
- 宿主一次创建三个 Entry VM 和点击回调，再绑定。更新现有 Amount、Icon、Visible、CanAdd、Interactable。Amount 是宿主格式化后的字符串，currencyId 是宿主标识；钱包查询、保存、购买、跳转、红点规则由宿主实现。
- 当前框架 SyncClick 在重新赋回调时追加监听；SyncGroup 在替换 Entries 时重新 Ref。禁止刷新时重建 Entries 或重新赋回调。每个组件实例只 Ref 一次，Unref 后销毁重建。需要不同生命周期时先调整相应框架契约并验证，不能静默累积绑定。
- 宿主 Unref 前将嵌套 VM 设为 null，主动释放 SyncMetadata；组件 OnDestroy 提供销毁释放。回调绑定和释放必须配对。
- 原生 CurrencyBar 保留 Home 原位置，宿主可覆盖根 RectTransform 停靠位置。修改共用外观在原预制进行；页面位置使用嵌套实例覆盖。
- 发布需携带四类代码、两个预制及所有图片、字体、meta。引用该组件的模板声明 CurrencyBar 依赖，不复制其实现；安装端应先恢复依赖及 GUID 再导入宿主。数据库依赖清单不等于安装器已经自动递归安装。

验收交给另一 agent，使用 [Currency 验收](../acceptance/SKILL.md)。GameCLI 参考探针为 `Assets/Editor/GameCLI/CurrencyProbe.cs`，不是产品运行时代码。
