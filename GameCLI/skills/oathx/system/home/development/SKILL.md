---
name: oathx-home-development
description: 独立 agent 参考 SvHome 基础大厅模板开发 Home 的玩家信息、货币、功能入口及角色和季节扩展契约，不携带 SV 网络或活动业务。
---

# Home 开发

先读 [系统开发流程](../../../system-development/SKILL.md)、[编码规范](../../../development-rules/SKILL.md)、[Core](../../../core/SKILL.md)、[Entity](../../../entity/SKILL.md)、[Widget](../../../widget/SKILL.md) 和 [Widget Extension](../../../widget-extension/SKILL.md)。由另一名 agent 按 [Home 验收](../acceptance/SKILL.md) 独立检查。

页面和子窗口的业务请求事件统一定义在 `UIHome.cs`。Widget 通过事件提交用户操作，System 调用 Widget 的 `Invalidate` 刷新实体数据。

## 默认范围

读取数据库 SvHome 的实际版本、scripts、configurations 和生产资料。未提出设计变更时保留所选版本的布局、独立图片、入口和行为；不依赖原 SV 工程重新提取。模板是产品开发范式，不把其静态夏季背景或示例数值强加给产品。

首版为用户批准的基础大厅：玩家信息、三组货币、聊天摘要和主要导航入口。用户明确暂缓的是角色和季节动态展示。活动入口、NEW、红点、掉落倍率和商城广告提示属于大厅展示，不得因暂不接运营服务而删除。平台 SDK 和远端运营逻辑不随模板自动安装。源 prefab 的 activeSelf 是编辑状态，不是完整的运行时状态，禁止只提取激活节点。

- 提取时同时核对原版截图、预制、运行时代码及配置换图；默认图片不一定是实际活动图片，Cn/Tw 资源必须区分。截图中的活动集合是可替换的本地示例，不是永久运营日程。
- 按入口独立控制 NEW、红点、是否展示及图片；活动网格须随可见项重排。掉落状态使用 None/Double/Triple，三倍使用其独立原图。商城蓝色广告提示与底部广告轮播是不同展示，不能混用。
- 保留字体文件、字号、BestFit、描边颜色／宽度、材质和 shader 依赖，不使用默认字体静默替代。保存独立生产资料和引用哈希，确保未来 agent 无需原 SV 工程。
- 未完成的字体绑定、状态绑定、动画和运行验证写入候选限制，不发布为已完成标准。导出节点数量增加不等于恢复成功。

## 实现契约

- HomeSystem 管理打开、刷新、关闭；HomeEntity 持有玩家、货币、聊天及展示资源键。Home 专属 VM 放 Home/VM，UIWidget 放 Home/UI，其余业务代码在 Home 根目录；公共组件沿用 Common/UI 与 Common/UI/VM。
- UIHome 使用 Assets/Data/Widgets/Home/Home.prefab。按需获取 vm，不缓存成员；元数据按实际字段生成，逐项检查目标和点击命中。
- 基础大厅必须保留入场动画，不能将其归入暂缓的角色／季节动态。当前 HomeOpen 采用 SV 原版区域滑入及左上信息淡入，保留分段时序，默认 Open 状态不循环；背景保持静止。原生 .anim/.controller、预制 Animator 引用及数据库 design.animations 必须一起交付，确保重新打开及网页预演均播放。曲线完成时回到基础布局，禁止最终位置漂移。
- HomeSystem.Refresh 在实体数据更新后由产品调用；不推测数据来源，不创建登录、不自动进入 Fight、不发 RPC。
- HomeEntryRequested 只表示入口被请求，不能解释为目标系统已打开。产品协调者将其映射为 SystemActive/SystemDetive，由 Master 统一管理系统。Notice 入口是否使用登录公告或游戏公告必须按产品契约确定。
- HomePresentationRequested 携带角色和季节资源键，供产品渲染器接入；空键保持无角色及静态背景。不得将事件存在描述为 Spine 已安装或季节已切换。
- 保持所有已有基础入口可交互；未接业务的入口发明确事件，不统一禁用或变灰。嵌套头像/玩家、聊天频道/聊天按钮必须分别验证命中。
- 固定 Text 文案直接保存在预制，不生成 Entity 属性、VM Binding 或 HomeSystem 赋值。VM 文本仅保留昵称、等级、三组货币和聊天；未接入的活动倒计时/FID 占位保持空文本，不生成 Label 加路径数字的属性。
- 已有 propCommonText 条目保留作为本地化源资料，Home 当前固定文本不在运行时查表；需要多语言时由文本组件按 ID 接入，不通过 Entity 中转。图片内文字仍是美术本地化依赖，当前不能宣称支持运行时语言切换。

## 资料与交付

遵循 [配置包规范](../../../system-development/references/configurations.md)。交付独立源表、结构化条目、代码别名、导表及加载依赖；只合并本系统条目，不覆盖产品整表。

当前提取工具为 integrations/codex/scripts/import-sv-home.mjs；完整 SV 原生导出仅为制作证据，基础版排除路径和原因记录于 report.json。透明 Graphic 不得通过父节点 opacity 隐藏后代；原 UGUI Graphic alpha 不影响子控件。

交付包含版本、工程身份、适配差异、源码/资源哈希、入口映射、配置和运行证据。更新标准须走候选与版本流程；未完成的验收如实标注，不把开发自测当独立验收。

## 红点所有权

真正红点由独立 [RedDotSystem](../../red-dot/development/SKILL.md) 配置和计算，Home 不保存或控制红点字典。宿主根节点通过 RedDotScope 序列化配置 ID 和目标。NEW、广告和掉落由 Home 展示；活动入口是否开放只读取 FunctionManagerEntity。

## 配置驱动活动列表

- AcitiveList 保留原节点名。复用 SyncGroup 将直接子节点 UIActivityEntry 按顺序绑定 VList；UIHome 序列化条目引用，条目保存配置 ID。SyncVisible 控制显隐，原布局自动重排，不创建模板克隆。
- VMHome.acitiveList 使用 VList<VMActivityEntry> 与 SyncGroup，数量与顺序必须等于预制直接子节点。页面只绑定一次，后续仅更新 Visible、Interactable、Badges，避免重复 Ref/点击监听。原图、NEW 动画保留；新增不同外观入口需先添加预制条目，不自动生成任意新类型。
- FunctionManagerSystem 负责等级、UTC 时间窗、本地开关，参见 [Function 开发](../../function/development/SKILL.md)。Home 监听 FunctionOpenChanged 和输入已写入后的 FunctionInputsUpdated，不直接监听原始等级输入进行刷新。
- Condtions 采用 `{Switch:1,Level:10}`，条件 AND；本地开关只覆盖默认 Switch，不能绕过等级或时间。空条件默认开放，空时间端点不受限。未知条件、时间格式错误或重复 ClassType 拒绝开放。
- 点击时重新检查条件，经 SystemActive 请求 Master；目标业务关闭由 Master 协调。ClassType 为空只保留展示，不伪造服务、跳转或接入旧 RPC。旧 SevenDayLoginSystem 不作为参考活动目标，活动使用独立配置 ID。
- Item 的 RedDotScope 随真实实例注册/释放；根 Home 的红点引用与显隐引用排除活动子树。UIActivityEntry 只根据 VM 显示和释放装饰，不拥有系统开放状态。
- 同一存活 Item 不重复赋 OpenClick（当前 SyncClick 每次赋值会追加监听）；SyncGroup 列表只初始化一次，页面关闭由框架解除条目绑定。时间检查用 unscaled 时钟，最长约一秒反映边界，点击及激活会即时复核。
- 制作链维护真实 C# 源码，build-home-source 只导出 verified 工程代码，build-home-presentation 只整理展示资料；最终执行 bind-home-functions 更新子 Widget 和根 Metadata。migrate-home-entries 仅用于本次已导出旧基线的一次性迁移及断点续跑；新项目恢复标准包中的已迁移预制，不重新运行历史提取。不要从节点路径自动生成业务属性。

## 通用入口与独立标记

- 全部功能入口复用 UIActivityEntry / VMActivityEntry：Visible、Interactable、OpenClick、Badges。异构活动列表继续使用 SyncGroup；固定导航通过 SyncMetadata 绑定子 VM。不为按钮外观另写业务类。
- ActivityBadge 是组合标志 None / New / Advertisement，同一入口可以同时显示 NEW 与广告。HomeEntity 通过 GetBadges / SetBadges 管理入口状态；UI 不查实体、不解释开放规则、不推断节点名。
- UIActivityEntry 序列化挂点和独立 UIActivityBadge 预制。SyncActivityBadges 只补充现有绑定缺少的附件生命周期能力，不代替框架列表。请求显示才创建；重复刷新不重复创建；取消、隐藏或解除绑定时释放；再次显示按最新 VM 恢复。
- 原图与标记动画保存在独立预制；原位置保留为空挂点。挂点用序列化引用，禁止运行时路径搜索。没有专用挂点的入口使用默认角标预制。所有标记 Graphic 必须关闭 RaycastTarget，不改变原点击目标。
- 不再使用 HomeIndicator、HomeVisibility 或按节点生成状态枚举。掉落倍率保留 HomeDropState；TaskEffectTips 是原固定局部效果，不冒充 NEW 或广告。
- 固定导航 ID 使用 HomeEntry，活动使用 FunctionSystem 配置 ID，二者不能冲突。实体持有数据及合法性操作，System 负责将数据更新到既有 VM；UI 不产生产品规则。
- 制作清单为 integrations/codex/config/home-entry-badges.json，保存入口、按钮、挂点和预制映射。交付须包含所有引用的标记预制及其字体、图片、动画依赖。

## 货币栏依赖
Home 的 ItemToolTips 使用 CurrencyBar 嵌套预制，先按 [Currency 开发规范](../../currency/development/SKILL.md) 安装 CurrencyBar 模板。Home 仅创建一次嵌套 VM 与回调，并在 Refresh 中更新 Amount；不复制 Currency 实现与资源。当前大厅功能项为 64 个，货币栏另含 3 个 CurrencyEntry。

## 头像与快捷入口依赖
左上角采用 Profile 公共预制族，见 [Profile 开发规范](../../profile/development/SKILL.md)。Profile 为独立组合，邮件、设置、公告、好友、好感度合并在一个 ProfileShortcuts 组合预制中，宿主通过一个 VMProfileShortcuts 绑定五个入口。入口 Widget/VM、标记和描边代码迁入 Common/UI；公共原生资源在 Widgets/Common，Home 保留数据、路由、红点配置和页面动画；昵称/等级经 VMProfile 赋值，内部控件绑定归 UIProfile。


Profile 完整展示通过单个 VMProfile 提供名称、等级、头像、头像框及两个入口状态；VMHome 不再声明 PlayerName、PlayerLevel、ProfileEntry、AvatarEntry 的独立绑定。

掉落展示由 HomeSystem 将状态写入 VMHome.DropSprite、DropSize、DropVisible，分别复用 SyncSprite、SyncSizeDelta、SyncVisible。UIHome 只读取序列化图片与 Canvas 原生尺寸，不接收 HomeEntity，也不直接改图片、尺寸或显隐。绑定保留原节点、锚点及掉落动画。
