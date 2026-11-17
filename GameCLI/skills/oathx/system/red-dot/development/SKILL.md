---
name: oathx-red-dot-development
description: 独立开发配置驱动的 RedDotSystem、条件检测和 Widget 红点展示绑定；不承担 NEW、广告、活动显隐或业务网络请求。
---

# RedDot 开发

先读 [开发规范](../../../development-rules/SKILL.md)、[系统开发流程](../../../system-development/SKILL.md)、[Core](../../../core/SKILL.md)、[Entity](../../../entity/SKILL.md) 和 [Widget](../../../widget/SKILL.md)。交付由另一 agent 按 [验收](../acceptance/SKILL.md) 检查。

## 契约

- RedDotSystem 是独立 RuntimeSystem，由系统配置安装；RedDotEntity 持有 signals、evaluated 和 counts，页面关闭不清空业务状态。
- propRedDot 定义 ID/NumID、ParentID、RedDotCondition、RedDotDisplay、Enabled、RefreshOnOpen、RefreshOnClose；条件和显示方式由导表生成枚举。表不存 GameObject 路径或可执行代码。
- 当前 None 仅作父级汇总，SignalCount 读取业务消息提交的数量。默认零，不虚构未读邮件或可领取任务。新增业务检测器必须读取对应 Entity，提供明确枚举处理分支、验证及测试；不在检测中发网络请求或修改业务状态。
- RedDotSignalChanged 携带 ID/count，0 清除条件，负数与未知 ID 不接受。RedDotRefreshRequested 请求重新检测，id=0 为全部。RedDotChanged 只在计算结果变化时通知。
- WidgetOpened 在 Widget 激活后、调用方 done 回调前发送；条件计算延至帧末，合并重复请求。WidgetClosing 是开始关闭，不是销毁完成，不得声称它是关闭动画完成事件。
- RedDotScope 在 Widget 根节点序列化绑定 RedDotBinding[]，每条包含配置 ID、目标 GameObject 和可选数字 Text。不搜索对象路径，不把组件挂在会被自身隐藏的红点目标上。动态 UI 由产品创建并绑定 Scope，系统不凭空生成图标。
- Scope 注册表用于服务晚启动/重新激活；停用系统必须解除框架 Widget 事件、业务订阅、协程及显示引用，隐藏显示但保留 Entity 数据。
- 校验重复 ID、未知条件、缺失父级、循环关系及显示类型。父级汇总子节点与自身检测值，数量溢出饱和；禁用节点不贡献数量。
- Home 仅保留 NEW、广告、掉落和活动显隐。所有真正红点从 HomeIndicator/UIHome.indicators 移除，由 RedDotScope 唯一控制，避免双重写入。

## 交付

交付配置源表、结构化条目、导表生成契约、SYS_RED_DOT 启动条目、代码及 .meta GUID。现有 Home 预制引用 RedDotScope 时必须声明 RedDot 模板依赖，禁止只带预制不带代码。业务代码使用 Excel 生成 ID 常量。
保留原布局和图像。通过 UnityCLI 写入组件、数组与对象引用；检查新旧数组无交集、无遗漏。新产品按自身红点清单配置，Home 的 53 个入口只是当前示例。

## Widget 事件注入

UISystem 不依赖 Core 或游戏事件；基础 Widget 包提供 IUIEventSender，ResourceConfigure.eventSenders 在 Startup 注入。GamePlugin 实现接口，通过自身 DispatchEvent 发送 WidgetOpened/WidgetClosing；RedDotSystem 统一 Subscribe，OnDetive 使用 UnsubscribeAll。禁止恢复 UISystem 静态事件。未注入发送器 UI 仍正常运行，但不会发送应用生命周期事件。安装模板须确认包具备此接口，并合并 GamePlugin.cs 中的 WidgetOpenedEvent / WidgetClosingEvent 包装与注入实现。

WidgetOpened、WidgetClosing 数据类统一声明在 UISystem.cs 中，不实现 IEvent。GamePlugin.cs 声明 WidgetOpenedEvent / WidgetClosingEvent : IEvent，转发包装后的数据；业务系统订阅 WidgetOpenedEvent / WidgetClosingEvent。禁止因此给 Widget 包增加 Core 引用，也不另建 WidgetEvents.cs。

生命周期派发支持多个 IUIEventSender：Startup 复制配置列表，运行时通过 RegisterEventSender / UnregisterEventSender 管理，按实例去重。事件按注册顺序发送给快照，回调内增删从下一次通知生效；单个适配器异常记录到 Console，不中断其他适配器和 UI 流程。Shutdown 释放整个列表。
