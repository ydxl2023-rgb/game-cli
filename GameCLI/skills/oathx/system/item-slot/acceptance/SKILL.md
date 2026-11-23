---
name: oathx-item-slot-acceptance
description: Independently verify generic item type, legacy-slot compatibility, nine presentation bindings, reward composition and dependency direction.
---

# ItemSlot 验收

独立复验时按[模板提取与复验](../../../system-development/references/template-fidelity.md)对照来源、示例输入和运行证据，具体判定采用本系统用例。

由非实现 agent 执行。读取[开发契约](../development/SKILL.md)、[开发规范](../../../development-rules/SKILL.md)，先核实工程绑定；同一 Unity 串行操作，不修改业务交付。

- 检查 type 为整数属性、无 EquipPartType 依赖和遗留 PartType 调用；产品自己的装备配置/枚举未误改。
- 原八字段顺序不变，IconVisible 只追加；新旧预制实际 Metadata 与组件类型正确，旧 TMP 标签继续可用，脚本与旧预制 GUID 保持。
- 独立实例测试 type、字符串路径、Text、Locked、Amount、Select、Arrow、Edge、IconVisible；检查真实控件，并区分“接受路径字符串”和“实际异步路径完成”。
- Reward 的 Item 指向同一个子 VM；图标和数量不再在 Reward 重复绑定。解绑后修改旧子 VM，控件不得继续更新；测试换快照和销毁。Mail 不维护图片回调；共享加载器的异步生命周期需要独立验证。
- 检查原生 ItemSlot 无 Reward/Mail 私有资源依赖、没有缺失引用，完整资源/meta 已打包。旧 Arrow 缺失动画与新静态处理应明确记录。
- 复测 Mail 实际奖励图标、数量、领取/删除/批量/空态，确认外观保持。数据库 ItemSlot/Reward/Mail 源码与材料哈希、依赖版本和项目副本一致。

保存原始检查和运行证据。当前嵌套预制不展开的比较失败必须保留，不将其改写为通过；绑定和运行补证分别记录。未覆盖异步竞态或长期列表复用不得推定通过。临时探针归档并从 Unity 移除，记录最终 Play 状态。

列表复用时不得永久清空模型 Item：保存子 VM，临时置 null 解除子订阅，解绑父 VM 后恢复原 Item；恢复发生在父订阅解除之后。验证同一模型重绑和两个模型交替复用，确保模型数据保留且解绑期间不更新控件。

图片路径规范：ItemSlot.Icon 固定为 string，优先传有效资源路径，由现有 SyncSprite/UILoader 加载。Mail 不再自行 LoadAsync<Sprite> 或维护图片回调；IconVisible 表示宿主是否要求显示图标，并非加载完成状态。路径加载的取消/乱序保护属于共享框架能力，不能把旧业务回调保护的验收结论沿用于此实现。
