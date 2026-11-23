---
name: oathx-reward-acceptance
description: Independently verify RewardEntry resource independence, VM binding, standalone rendering and host Fancy-list integration.
---

# Reward 验收

独立复验时按[模板提取与复验](../../../system-development/references/template-fidelity.md)对照来源、示例输入和运行证据，具体判定采用本系统用例。

由未参与实现的 agent 执行；读[开发契约](../development/SKILL.md)、[开发规范](../../../development-rules/SKILL.md)与[Widget](../../../widget/SKILL.md)。先验证工程 ID，串行使用目标 Unity，不替开发修改业务或资源。

1. 检查 UIRewardEntry/VMRewardEntry 无 MailEntity/System、物品业务查询、网络、保存和发奖。旧槽位类型不能被顺带改写。
2. 检查 Item/Claimed 两字段及子槽位九字段顺序、实际 Metadata 类型及目标、脚本 GUID；选择组件 Inspector 后确认无错误。
3. 不打开邮件，单独实例化预制：通过 Item 绑定字符串图标路径、Text、IconVisible，单独绑定 Claimed，更新数量、隐藏/恢复图标、切换标记并解绑；检查真实 Image/Text/activeSelf，不只读 VM。
4. 检查 Mail 的附件列表使用公共预制且 Fancy.cellPrefab、源模板显隐引用有效；运行原领取、重复领取、删除、批量和空态流程。快速切换、旧图回调和长列表仍需独立用例，不能从普通流程推定通过。
5. 递归读取原生依赖及材料包。Reward 只能向下依赖 ItemSlot，不能引用 Mail 私有资源；必要公共字体、图片、动画、代码和 meta 必须齐全。Mail 声明 RewardEntry 精确版本依赖；迁移目录和示例图标路径同步，大小写差异不能遗漏。
6. 比较数据库和本地源码/材料哈希、GameCLIApp 引用版本。模板分类应为预制控件。独立动画和嵌套后外观须保留；记录浏览器预演不执行 C# 的边界。

保存原始 CLI、编译/Console、运行和截图证据。报告区分 passed / failed / incomplete，缺证不得判通过；临时探针不进入业务源码包，清理后记录最终 Play 状态。验收不自动授权提交推送或额外发布。
