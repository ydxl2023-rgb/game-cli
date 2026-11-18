---
name: profile-development
description: Reuse the Profile and ProfileShortcuts composite prefabs; bind five shortcut entries as one movable group with host-owned callbacks.
---

# Profile 公共组件开发

先读 [开发规范](../../../development-rules/SKILL.md)、[Widget](../../../widget/SKILL.md) 和 [独立开发流程](../../../system-development/SKILL.md)。产品需求优先；未指定设计变更时保留模板布局、字体、点击范围、提示挂点和资源，不自行改变业务。

- 主要交付只有两个可拖入页面的组合预制：`Assets/Data/Widgets/Profile/Profile.prefab` 与 `ProfileShortcuts.prefab`。后者在一个 RectTransform 容器中包含 Mail、Settings、Notice、Friends、Affinity；移动、停靠、显示与复用以整个组为单位。
- 不再将五个单按钮作为页面集成的主要模板。AvatarFrame 与单按钮资产放在 `Profile/Parts/`，仅是组合内部实现。数据库主模板为 PlayerProfile（对应 Profile）与 ProfileShortcuts；早期单项模板移入历史子项组，仅供迁移。
- 组使用 `Common/UI/UIProfileShortcuts.cs` 与 `Common/UI/VM/VMProfileShortcuts.cs`。宿主只绑定一个组 VM，初始化其五个 VMActivityEntry；组内通过 SyncMetadata 沿用入口规则，保留各自点击、显隐、可交互和标记状态。禁止让宿主另写五套组件生命周期。
- 宿主解绑前把组 VM 设为 null；UIProfileShortcuts 先清空五个子 VM 释放入口订阅，再解绑自身，销毁时同样释放。创建后只绑定一次，更新原 VM，不重复注册回调。
- 通用 UIActivityEntry、UIActivityBadge、ActivityBadge、HomeTextOutline 位于 `Systems/Common/UI/`，VMActivityEntry、VMActivityBadge 位于其 `VM/`。保留现有公共实现，不为每个图标复制 Widget/VM 类。HomeTextOutline 是保留原字体效果的历史类名，无 HomeEntity/System 依赖。
- 公共原生字体、图片、标记预制及标记动画位于 `Assets/Data/Widgets/Common/`。各模板带相同共享路径时按内容/GUID 合并；禁止生成重复脚本类或重复 GUID。
- 每个入口绑定一个 VMActivityEntry：OpenClick、Visible、Interactable、Badges。宿主指定点击路由和可选 NEW/广告状态；UI 不访问业务实体。组件 id 是参考标识，产品可按自己的入口配置调整。
- Profile 统一使用 `Common/UI/UIProfile.cs` 和 `Common/UI/VM/VMProfile.cs`。宿主只绑定一个 VMProfile：PlayerName、PlayerLevel（宿主格式化字符串）、Portrait、Frame（资源路径 string）、PortraitVisible、FrameVisible、ProfileEntry、AvatarEntry。名称、等级、图片和两种入口的内部 Metadata 全由 Profile 负责，禁止再让 Home 的 VM 直接绑定内部 Text/Image。
- ProfileEntry/AvatarEntry 仍为 VMActivityEntry，保留点击、显隐、可交互和标记规则。UIProfile 解绑时释放两个子 VM 并解除自身订阅；同实例只绑定一次，刷新修改已有属性。原入口主体是 `Parts/ProfileBody.prefab`，保留原 GUID 与外观。
- 图片路径复用 SyncSprite，经 UIResourceManager 与 UILoader 加载；显隐复用 SyncVisible，不新增 ISyncAction。未赋值保留默认图；路径必须有效，不以 null/空字符串清图。通过 PortraitVisible / FrameVisible 隐藏和恢复，隐藏保留图片。加载与缓存沿用框架，UI 不查询玩家实体。
- MailTips、SetTips、AnnouncementTips、FriendTips、FavorTips 保留原节点。红点配置与 RedDotScope 归宿主，不在独立按钮内固化 Home 的配置 ID。提取后必须恢复宿主对嵌套节点的引用。
- 一实例只绑定一次，刷新修改现有 VM，不重设回调。当前框架 SyncClick 会追加监听、UIWidget 不清空缓存；解绑后销毁重建，UIActivityEntry 拒绝已有 VM 的实例再次 Ref。
- 原生停靠位置作为参考保留。更换宿主可覆写根 RectTransform。替换为嵌套预制须保留兄弟顺序、子节点名称和动画路径；Home 的 TopLeft CanvasGroup 入场动画仍由页面播放，按钮组不复制一份页面动画。
- 旧工程升级先按材料包 resource-moves.json 和代码位置移动原资源及 meta，再导入预制。保留 GUID，检查生成的 VM Metadata classGuid。不要留下旧路径副本。

验收交由另一 agent 按 [Profile 验收](../acceptance/SKILL.md) 执行。参考运行探针为 `Assets/Editor/GameCLI/ProfilePrefabProbe.cs`，不进入产品运行时源码包。

用户纠正优先：目标是拖入一个组合预制即可携带整组功能入口，不是只把单个按钮分别导出。

框架限制：现有 SyncSprite 的路径加载回调未保护请求乱序或销毁中回调；本次复用不等于已解决该公共能力。产品需要加载中关闭或快速换图时，应先完善框架统一生命周期保护，不重新实现业务 ISyncAction。
