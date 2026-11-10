---
name: widget
description: 使用 com.oathx.widget 编写 UIWidget 与 VM，维护 Binding、Metadata 和 UI 数据同步；用于 Oathx 页面视图开发。
---

# Oathx Widget

必须遵守 [开发规范](../development-rules/SKILL.md)。工程定位见 [Oathx](../SKILL.md)。

先读包内 `Runtime/UIWidget.cs`、`IWidget.cs`、`Metadata/Metadata.cs` 和 `WidgetGenerator~` 中的生成器；VM 蓝本为业务侧 `VMSevenDayLogin`。先确认当前程序集内 VM、Binding 与 Sync 类型的定义，不凭名称补造接口。

- 页面使用 `UIWidget<TVM>`；VM 使用 `partial` 与私有 camelCase `[Binding]` 字段。生成器提供的数据属性属于绑定机制，不是公开 Unity UI 引用；不要手工重复生成成员。
- 按绑定目标选 Sync 类型：文本、可交互状态和显隐分别对照现有 SyncText、SyncInteractable、SyncVisible；按钮对照 ButtonClick。
- UIWidget.Ref 依赖 prefab Metadata；字段存在不代表绑定完成。通过 UnityCLI widget-metadata 先 dry-run 再生成，检查字段覆盖和真实目标组件。
- 固定 UI 引用采用序列化绑定，禁止用运行时节点路径搜索替代 Metadata 或序列化引用。
- 检查 Ref/Unref 的绑定与监听释放；重开页面不能重复注册点击事件。不要仅销毁可见对象而遗留绑定。
- 业务 Entity 是数据来源，VM 是展示状态；按系统已授权范围刷新，不在视图层新增网络管理。

页面加载与关闭同时遵循 [Widget Extension](../widget-extension/SKILL.md)。验证生成代码编译、字段绑定、展示更新与监听释放；资源修改遵循现有 UnityCLI 工具链。
