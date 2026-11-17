---
name: widget-extension
description: 使用 com.oathx.widget.extension 的 UISystem 和 Fancy 列表扩展，处理页面异步打开、关闭、缓存和列表绑定。
---

# Oathx Widget Extension

必须遵守 [开发规范](../development-rules/SKILL.md) 与 [Widget](../widget/SKILL.md)。工程定位见 [Oathx](../SKILL.md)。

源码入口为包内 `Runtime/Scripts/UISystem.cs`、`Components` 和 `Syncs`。UISystem 属于 extension 包，不要在基础 widget 包中重复实现页面管理器。

- 页面通过 Config 声明资源路径、WidgetLayer 与缓存策略，加载交给 UISystem。
- 系统调用 OpenWidget 前，在回调外检查 IsActive；未激活返回 false，不发起打开请求。
- OpenWidget 必须提供显式完成回调，禁止传 null。即时 bool 返回不是异步加载完成；页面就绪后的初始化放进回调，不用回调里的关闭代替调用前检查。
- OnDetive 负责关闭系统页面；是否立即关闭、是否缓存根据该系统规则明确选择。检查当前 UISystem 的 UnloadWidget、CloseWidget 与 Unref 路径，不假定关闭一定销毁。
- 类型数组先声明为局部变量 `System.Type[] widgets`，初始化时每个类型独占一行，再空一行调用 `CloseWidget(widgets, ...)`，禁止内联数组，单页也遵守此格式。
- 系统类统一使用 `CloseWidget(System.Type[] types, System.Action complete = null, bool immediate = false)`，由调用方传入类型数组，单页同样如此；同一次退出的多个页面合并为一次调用。当前数组重载的 complete 表示已逐个发出关闭调用，不表示所有异步关闭动画已结束，不能据此判断动画完成。
- 使用 Fancy 列表时对照现有 View、Cell 与 Sync 类型，确认数据、cellPrefab、复用和点击绑定。不能把列表替换为一组静态文本。
- 网格 Cell 根 pivot 与中心排布一致，通常为 `(0.5,0.5)`；实际布局与组件实现一起核对。

验证异步打开、关闭、重开及所修改的列表行为；对业务系统的额外需求查阅对应 skill，不把 SV Login 的空业务回调复制为所有页面的初始化逻辑。

## 按需获取页面和 VM

- 系统类不使用成员变量缓存页面 VM；需要时通过 UISystem 查询已打开的 Widget，再以方法内局部变量使用其 vm。打开回调已提供 Widget 时可直接使用，不必重复查询。
- `TryGetWidget<T>(out T widget)` 查询打开栈，支持 cacheWidget=false；未打开或已关闭时返回 false。先检查查询结果与 vm 是否有效，不通过场景搜索兜底。
- 现有 `GetWidget<T>()` 仅查缓存字典，不能用于保证获取非缓存页面。不得为省查询添加另一份 VM 缓存或强制开启页面缓存。

## 应用生命周期通知

- 通过 ResourceConfigure.eventSenders 在 Startup 注入基础 Widget 包的 IUIEventSender；接口只使用 IWidget，不引用 Core、GamePlugin 或业务事件。未传入时保持 UI 打开/关闭功能兼容。
- 应用实现负责转发事件；GamePlugin 示例通过 DispatchEvent 发送 WidgetOpened 和 WidgetClosing。系统使用 Subscribe/UnsubscribeAll，不订阅 UISystem 静态事件。
- OnWidgetOpened 在 widget.Active 之后、调用方完成回调之前；OnWidgetClosing 在关闭动画/立即清理开始之前，不代表关闭完成。Shutdown 清理界面期间仍可发送通知，结束后释放发送器引用。

WidgetOpened、WidgetClosing 数据类统一声明在 UISystem.cs 中，不实现 IEvent。GamePlugin.cs 声明 WidgetOpenedEvent / WidgetClosingEvent : IEvent，转发包装后的数据；业务系统订阅 WidgetOpenedEvent / WidgetClosingEvent。禁止因此给 Widget 包增加 Core 引用，也不另建 WidgetEvents.cs。

生命周期派发支持多个 IUIEventSender：Startup 复制配置列表，运行时通过 RegisterEventSender / UnregisterEventSender 管理，按实例去重。事件按注册顺序发送给快照，回调内增删从下一次通知生效；单个适配器异常记录到 Console，不中断其他适配器和 UI 流程。Shutdown 释放整个列表。

当前 Core 事件按 Type.Name 路由；应用事件必须使用不同的具体类型名，禁止以同一泛型事件的不同类型参数区分打开与关闭，否则会发生路由冲突和 InvalidCastException。

## 列表能力优先复用

新增列表前必须检查当前包的 SyncGroup、SyncScrollRect、FancyView 与 FancyGridScrollView 的实际绑定、模板和生命周期实现。固定异构条目组可用 SyncGroup 配合显隐；统一模板动态/虚拟列表优先用现有列表组件。只有具体、已验证的能力缺口才允许扩展，不按系统复制列表创建、销毁和绑定机制。SyncGroup 按直接子节点顺序绑定，不支持任意长度增删；必须保证顺序与数量，避免重复 Ref。
