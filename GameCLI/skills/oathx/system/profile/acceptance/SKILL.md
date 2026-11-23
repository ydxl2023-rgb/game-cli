---
name: profile-acceptance
description: Independently verify shared avatar/profile and shortcut prefab dependencies, sibling order, host bindings and pointer interactions.
---

# Profile 公共组件验收

独立复验时按[模板提取与复验](../../../system-development/references/template-fidelity.md)对照来源、示例输入和运行证据，具体判定采用本系统用例。

先读 [开发规范](../../../development-rules/SKILL.md) 和 [开发契约](../development/SKILL.md)。独立 agent 只检查交付，缺陷交回开发，保留实际 CLI 证据。

1. 检查两个主要组合预制及 Parts 内部资源及共享代码、字体、shader、图片、标记和动画的 GUID 闭包，不允许依赖 Home 私有资源。Profile 根必须为 UIProfile/VMProfile，其 ProfileBody 主体内的 Avatar 仍为嵌套实例。
2. 核查 Home 的 Profile 与 ProfileShortcuts 为两个嵌套预制；兄弟顺序、名称、RectTransform 与原布局一致，Avatar 不被后来追加的 ClickArea 遮挡；TopLeft 原入场路径不变。
3. 独立实例绑定入口 VM，验证一次点击一次回调、解绑后回调移除、再次 Ref 被拒绝。根 VMProfile 通过内部 SyncMetadata 统一绑定资料和头像入口，宿主不再分别操作内部 Widget。
4. 检查宿主昵称、等级、五个红点目标和入口 Metadata 引用有效。它们由宿主提供，不能把无业务服务解释为预制应自动获取玩家信息或公告。
5. 在实际 Canvas 相机和分辨率下检查七个点击目标，保留真实 raycast 结果和截图。验证默认 NEW/广告，关闭重开与昵称/等级更新。红点仍由宿主系统控制。
6. 编译与 Console 无错误；数据库原生材料及源码 SHA256 与工程一致，Home 依赖版本与项目副本同步。浏览器卡片只验证静态预览，不替代 C# 运行验收。

GameCLI 示例可用 ProfilePrefabProbe.Check / CheckDefaults / CheckGroup / ReadHome、HomeEntryProbe.CheckPrefab / CheckInspectedBindings 和现有 test-home-entries.ps1。报告区分开发执行的运行证据与独立 agent 实际执行的检查；不要将既存 Home 静态布局差异宣称为通过。

必须验证：一次绑定组 VM 后五个入口均有效；拖动组根节点五个入口一起移动；组解绑后五个旧按钮均不再回调，重复绑定被拒绝；关闭重开 Home 后五个入口及红点恢复正常。

完整 Profile 必测：初始化与实时昵称/等级更新；有效路径加载、未赋值默认图、绑定前隐藏；运行时换路径→隐藏→恢复；资料与头像各一次回调；解绑后旧文本/图片/点击不再变化；绑定状态直接销毁无错误；选中 Profile 的 Metadata Inspector 后绑定类型与目标保持不变。

框架限制：现有 SyncSprite 的路径加载回调未保护请求乱序或销毁中回调；本次复用不等于已解决该公共能力。产品需要加载中关闭或快速换图时，应先完善框架统一生命周期保护，不重新实现业务 ISyncAction。
