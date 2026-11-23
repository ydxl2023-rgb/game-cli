---
name: oathx-item-slot-development
description: Reuse the foundational UIItemSlot/VMItemSlot control, preserve legacy binding order and compose product widgets such as rewards through a child item VM.
---

# ItemSlot 基础控件

开发前读取[系统模板开发流程](../../../system-development/SKILL.md)，交付本模块的场景基准与复现数据。

先读[开发规范](../../../development-rules/SKILL.md)、[Widget](../../../widget/SKILL.md)。由另一 agent 使用[本控件验收](../acceptance/SKILL.md)复核。

- `UIItemSlot` 位于 `Systems/Common/UI`，保留 `Oathx.Widget` 命名空间；`VMItemSlot` 位于 `Common/UI/VM`。槽位没有系统、配置查询或装备业务。
- 类型信息为 `public int type { get; set; }`，按规范换行。整数含义由产品定义，不在基础控件引用 EquipPartType，也不保留 PartType 兼容别名。不要修改产品配置或装备系统自己的同名枚举。
- 原八个 Binding 索引保持不变：Back、Icon、Text、Locked、Amount、Select、Arrow、Edge；IconVisible 追加索引8。Icon 为 string 资源路径，由已有 SyncSprite 与统一 UILoader 加载；不新增图片同步器，不通过 null 清图。Text 是宿主格式化数量/标签，不让控件查物品表。
- `Widgets/Common/Micros/ItemSlot/ItemSlot.prefab` 是当前 SV 外观基础预制。`Widgets/Common/Micros/ItemSlot.prefab` 保留旧外观和 GUID，Metadata 同步为九字段，仍保留 TMP 标签绑定。旧槽位使用方无需更换外观；不可因提取模板替换所有旧页面。
- SV 外观继承当前图标素材和动画，Lock/Mask/Arrow 复用旧基础槽位控件。旧 Arrow 引用的 Controller 在工程中已缺失，新控件只保留静态箭头，不携带失效 Animator；不声称恢复了缺失动画。原八字段在当前预制均有真实目标，不用空绑定冒充支持。
- 新标准的资源闭包包含 ItemSlot/Resources、Parts、实际使用的 Common 字体与通用贴图。不得反向依赖 Reward/Mail。资产迁移使用 UnityCLI 保留 GUID，字符串图标路径同步改写。
- Reward 用 `SyncMetadata VMItemSlot item` 组合这个槽位，自己只管理 Claimed 展示。宿主写 `reward.Item.Text/Icon/IconVisible/type`；业务领取、保存、物品规则留在实体/系统。
- 复合 Widget 解绑前清空子 VM，触发 SyncMetadata 释放；它的 UnSync 本身不做释放。静态实例 Ref 一次后更新属性；Fancy 重绑沿用框架契约，不能宣称此次修复了 UIWidget 内部绑定缓存累积。

制作端工具：`compose-item-slot.py`、`move-item-slot-resources.py`。模板 `ItemSlot` 属于预制控件；修改后通过候选发布，同步 RewardEntry、SvMail 和实际项目副本的源码、材料及精确依赖版本。产品安装先恢复 ItemSlot，再 Reward，再宿主。

列表复用时不得永久清空模型 Item：保存子 VM，临时置 null 解除子订阅，解绑父 VM 后恢复原 Item；恢复发生在父订阅解除之后。验证同一模型重绑和两个模型交替复用，确保模型数据保留且解绑期间不更新控件。

图片路径规范：ItemSlot.Icon 固定为 string，优先传有效资源路径，由现有 SyncSprite/UILoader 加载。Mail 不再自行 LoadAsync<Sprite> 或维护图片回调；IconVisible 表示宿主是否要求显示图标，并非加载完成状态。路径加载的取消/乱序保护属于共享框架能力，不能把旧业务回调保护的验收结论沿用于此实现。
