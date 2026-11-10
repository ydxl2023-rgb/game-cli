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
- 使用 Fancy 列表时对照现有 View、Cell 与 Sync 类型，确认数据、cellPrefab、复用和点击绑定。不能把列表替换为一组静态文本。
- 网格 Cell 根 pivot 与中心排布一致，通常为 `(0.5,0.5)`；实际布局与组件实现一起核对。

验证异步打开、关闭、重开及所修改的列表行为；对业务系统的额外需求查阅对应 skill，不把 SV Login 的空业务回调复制为所有页面的初始化逻辑。
