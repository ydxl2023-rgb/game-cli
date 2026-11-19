---
name: oathx-reward-development
description: Reuse RewardEntry as a standalone or Fancy-list reward display with host-owned icons, quantity and claim state; no inventory or mail business.
---

# Reward 公共展示组件

先读[开发规范](../../../development-rules/SKILL.md)、[Widget](../../../widget/SKILL.md)与[Widget Extension](../../../widget-extension/SKILL.md)。验收由另一 agent 使用[本模块验收](../acceptance/SKILL.md)。

- 数据库模板 `RewardEntry` 归入预制控件，不拥有 RuntimeSystem。原生资源是 `Assets/Data/Widgets/Common/Micros/Reward/RewardEntry.prefab`；UI 为 `Systems/Common/UI/UIRewardEntry.cs`，VM 为 `Systems/Common/UI/VM/VMRewardEntry.cs`。
- `VMRewardEntry` 只包含 Item（VMItemSlot/SyncMetadata）和 Claimed（bool/SyncVisible）。ItemSlot 是基础展示，Reward 是组合控件；宿主设置 Item.Text、Item.Icon、Item.IconVisible 和整数 Item.type。组件不查询业务实体，不领取、保存或发奖。
- 单独放置时使用宿主的 SyncMetadata 嵌套 VM 或显式 Ref；Fancy 列表使用 VList<VMRewardEntry> 和现有 FancyGridScrollViewCell。根含标准 Fancy cell，独立展示不要求存在列表。不要给每个业务复制 UI/VM。
- 初始化 Item 并显式赋 Item.Text、Claimed、Item.IconVisible；Item.Icon 使用有效字符串路径，Item.IconVisible 表示显示意图，不代表加载完成。默认 backing bool 不等于触发同步；不要通过 null 清图。异步路径解析和过期回调保护归共享加载服务，本组件复用已有 SyncSprite，不新增 ISyncAction。
- 静态实例只 Ref 一次，更新原 VM，宿主销毁前 Unref；通过 SyncMetadata 集成时先清空子 VM 再解绑宿主。现有 Fancy 复用沿用框架契约，框架绑定缓存限制不能被描述为本次已修复。
- 新产品保留模板外观与动画，具体物品规则由产品指定。当前标准只表达图标、数量、已领取标记和图标显隐；品质、选中、提示弹窗等若需要，先检查已有控件能力并按产品需求扩展。VMItemSlot 原八字段顺序保留，新增 IconVisible 追加末尾；VMGridItemSlot 不随本次迁移改写。
- 图片、Controller 和 AnimationClip 在 Widgets/Common/Micros/Reward/Resources；共享字体使用 Widgets/Common/Resources。材料包带完整实际依赖、meta 和迁移清单，不依赖 Mail 的资源目录。安装时按哈希/GUID 合并公共资源，不生成重复 GUID。
- Mail 引用嵌套 RewardEntry；替换时保留 Cell 名、尺寸和序号，并重接 Fancy.cellPrefab 与宿主 Metadata 目标。源文件重命名通过 UnityCLI 迁移，保留脚本 GUID。发布独立 RewardEntry 后同步 Mail 源码、依赖版本、原生材料和 GameCLIApp 副本。

- 先安装 [ItemSlot](../../item-slot/development/SKILL.md)。Reward 内 BagIconNew 是嵌套 ItemSlot.prefab，保持节点名以保留动画路径。图标主体资源归 ItemSlot；Reward 只保留自己的领取标记和外层动画。Unref 时先将 Item 置 null 释放子订阅，再解绑父 VM。

列表复用时不得永久清空模型 Item：保存子 VM，临时置 null 解除子订阅，解绑父 VM 后恢复原 Item；恢复发生在父订阅解除之后。验证同一模型重绑和两个模型交替复用，确保模型数据保留且解绑期间不更新控件。

图片路径规范：ItemSlot.Icon 固定为 string，优先传有效资源路径，由现有 SyncSprite/UILoader 加载。Mail 不再自行 LoadAsync<Sprite> 或维护图片回调；IconVisible 表示宿主是否要求显示图标，并非加载完成状态。路径加载的取消/乱序保护属于共享框架能力，不能把旧业务回调保护的验收结论沿用于此实现。
