---
name: widget
description: 使用 com.oathx.widget 编写 UIWidget 与 VM，维护 Binding、Metadata 和 UI 数据同步；用于 Oathx 页面视图开发。
---

# Oathx Widget

必须遵守 [开发规范](../development-rules/SKILL.md)。工程定位见 [Oathx](../SKILL.md)。

先读包内 `Runtime/UIWidget.cs`、`IWidget.cs`、`Metadata/Metadata.cs` 和 `WidgetGenerator~` 中的生成器；VM 蓝本为业务侧 `VMSevenDayLogin`。先确认当前程序集内 VM、Binding 与 Sync 类型的定义，不凭名称补造接口。

- 页面使用 `UIWidget<TVM>`；VM 使用 `partial` 与私有语义的 camelCase `[Binding]` 字段；`[Binding]` 必须独占一行，字段声明另起一行，`private` 可以省略，不强制补写。具体格式遵循[开发规范的 VM 绑定字段格式](../development-rules/SKILL.md#vm-绑定字段格式)。生成器提供的数据属性属于绑定机制，不是公开 Unity UI 引用；不要手工重复生成成员。
- 系统代码目录遵循[共享目录规范](../development-rules/SKILL.md#系统代码目录)：所有 VM 放 `<System>/VM/`，UIWidget 实现放 `<System>/UI/`，其余代码放系统根目录。
- 页面 prefab 和配套 UI 资源统一位于 `Assets/Data/Widgets/<模块>/`；`[Config]` 与模板源码必须指向实际 prefab 的完整资产路径。
- 按绑定目标选 Sync 类型：文本、可交互状态和显隐分别对照现有 SyncText、SyncInteractable、SyncVisible；按钮对照 ButtonClick。
- UIWidget.Ref 依赖 prefab Metadata；字段存在不代表绑定完成。通过 UnityCLI widget-metadata 先 dry-run 再生成，检查字段覆盖和真实目标组件。
- 固定 UI 引用采用序列化绑定，禁止用运行时节点路径搜索替代 Metadata 或序列化引用。
- 检查 Ref/Unref 的绑定与监听释放；重开页面不能重复注册点击事件。不要仅销毁可见对象而遗留绑定。
- 业务 Entity 是数据来源，VM 是展示状态；按系统已授权范围刷新，不在视图层新增网络管理。

页面加载与关闭同时遵循 [Widget Extension](../widget-extension/SKILL.md)。验证生成代码编译、字段绑定、展示更新与监听释放；资源修改遵循现有 UnityCLI 工具链。

## 列表能力优先复用

新增列表前必须检查当前包的 SyncGroup、SyncScrollRect、FancyView 与 FancyGridScrollView 的实际绑定、模板和生命周期实现。固定异构条目组可用 SyncGroup 配合显隐；统一模板动态/虚拟列表优先用现有列表组件。只有具体、已验证的能力缺口才允许扩展，不按系统复制列表创建、销毁和绑定机制。SyncGroup 按直接子节点顺序绑定，不支持任意长度增删；必须保证顺序与数量，避免重复 Ref。

## 复合入口与可选标记

外观不同但交互契约相同的功能按钮优先共用 UIWidget / VM，固定入口用 SyncMetadata、异构静态列表用 SyncGroup。复用现有同步器处理点击、显隐和可点击状态；只有现有同步器缺少的可选附件创建／释放能力才补充窄接口。附件状态来自 VM，UI 不查业务 Entity。

NEW、广告等标记使用组合枚举及序列化挂点／预制引用，按需创建、取消或隐藏时释放、恢复时按最新状态重新创建。原图片与动画进入独立标记预制，标记 Graphic 禁止拦截点击。自定义同步器必须遵循 VM 的 Boxed<T> 值类型约定，且不能依赖某个业务 System 已启动才注册。当前框架解绑不清空内部绑定缓存，同一存活条目只绑定一次，刷新仅修改已有 VM 属性，不重建嵌套 VM。

Metadata 的 type 字符串是同步器身份，hash 仅作 Inspector 缓存；不能在重绘时根据旧 hash 改写 type。修改绑定后必须实际选中页面及子 Widget 验证 Inspector，确认类型、组件引用不变且 Console 无错误；单纯运行与静态绑定检查不能代替该验证。

## 图片绑定优先复用

图片先检查现有 SyncImage / SyncSprite：SyncImage 接收路径，SyncSprite 支持路径或 Sprite；路径经 UIResourceManager 和宿主 UILoader 加载。按实际生命周期选用，不为单个业务另写 ISyncAction。显隐复用 SyncVisible，不以 null 或空路径替代隐藏。仅在确认已有同步器无法满足明确需求时扩展，并记录能力缺口。
