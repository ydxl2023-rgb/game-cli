---
name: oathx-mail-development
description: 独立开发邮件系统，参考 SvMail 的列表、详情、已读、附件领取、删除与原版动画，交付实体、UI、VM 和配置资料。
---

# Mail 开发

先读[系统开发流程](../../../system-development/SKILL.md)、[开发规范](../../../development-rules/SKILL.md)、[Entity](../../../entity/SKILL.md)、[Widget](../../../widget/SKILL.md)、[Widget Extension](../../../widget-extension/SKILL.md)。由不同 agent 按[邮件验收](../acceptance/SKILL.md)检查。

读取数据库 SvMail 的实际版本、scripts、configurations 和材料清单。模板是产品范式；没有明确设计变更时保留界面、字体、状态和动画，不用旧 EmailSystem 替换。产品 agent 不需要访问 SV 原工程。

## 当前参考范围

SV NewMailView：邮件列表、正文与附件、已读、单封/全部领取、删除/删除已读、空态。仅使用明确标识的本地示例，领取记录保存在实体内存中，不接背包、网络、持久化或正式发奖。清空后重开不能重新注入示例；退出运行后重置。产品提供收件箱后不加载示例。

MailEntity 拥有收件箱、选择和领取凭据，校验时间与附件、返回枚举结果、复制输入和查询输出。过期邮件不展示/领取；未读及含未领取附件的邮件不能删除。重复领取不能新增凭据，同 ID 重新导入不能重复领取。产品若要求账号切换、正式邮件协议、跨会话幂等、过期刷新计时或容量上限，应单独设计。

MailSystem 只协调、调用实体并更新 VM。UI 在 Mail/UI，VM 在 Mail/VM，其余代码在 Mail 根目录。列表与附件使用已有 FancyGridScrollView 和 Widget cell，禁止自制重复列表框架。新 cell 使用 string 路径交给现有 SyncSprite 加载；异步取消与乱序能力需单独验证，不在业务内重复实现加载。

## 资源与集成

附件使用公共 [RewardEntry](../../reward/development/SKILL.md)：`Common/UI/UIRewardEntry.cs`、`Common/UI/VM/VMRewardEntry.cs` 和 `Widgets/Common/Micros/Reward/RewardEntry.prefab`。Mail 的 Rewards 为 VList<VMRewardEntry>，领取规则由 MailSystem/Entity 管理，图标加载由共享 SyncSprite/UILoader 管理。不得重新创建 VMMailReward/UIMailReward；恢复 Mail 前先安装其 configurations 声明的 RewardEntry 依赖并保留 GUID。

抽离后的制作端使用 `package-reward-entry.mjs` 打包共同交付，`verify-reward-publication.mjs` 核对本地与数据库源码/材料。`register-mail-template.mjs` 是抽离前首次登记脚本，不作为当前模板重打包入口。嵌套预制可能不被当前 verify prefab 导出器展开；保留原始失败结果，并通过独立预制 Metadata、实际运行和依赖闭包补证，不为通过比较而拆散嵌套预制。

- 资源路径 Assets/Data/Widgets/Mail；材料包包含预制、图片、原生动画/Controller、示例 JSON 和共享字体及 meta。按哈希复用已有 Common 字体，冲突时走目标工程资源迁移，不能盲目覆盖。
- 保留 Open/Close、EnvelopeOpen/EnvelopeClose、EntryOpen。关闭交给协调器的 SystemDetive，系统用局部 Type[] 调 CloseWidget，等待原版关闭动画。检查重开和快速切换。
- 固定标签存预制；动态邮件标题/正文来自数据，不放进 VM 固定文本字段。确需运行时代码固定文案时走 propCommonText。示例时间按首次打开时刻平移，不把制作日期当永久有效期。
- configurations 只合并 propFunctionSystem 的 SYS_MAIL 行，处理产品 NumID 冲突并重新导表。源表可包含其他行，但不授权覆盖产品全表。Master 中的 Home Mail 路由是可选集成示例，不要求产品安装 Home 或覆盖 Master。

## 制作端与交付

制作工具在 integrations/codex/scripts：import-sv-mail.mjs、prepare-mail-animations.mjs、build-mail-bindings.mjs、bind-sv-mail.ps1、import-mail-native-animations.py。构建绑定脚本从维护中的源码复制，不覆盖行为。Unity 资产使用 UnityCLI，不手写 YAML。恢复发布包可直接使用 materials.json，不必重新执行源提取流程。

浏览器设计仅保留支持的默认动画与静态容器，不执行 C# 动态列表及所有 Animator 状态。完整状态保存在原生资料和动画清单中；不得把浏览器预演等同 Unity 功能验收。粒子/网格与未实现特殊物品效果的差异必须保留在契约中。

邮件附件现在采用 RewardEntry → ItemSlot 两层组合，宿主通过 reward.Item 更新数量和图标。图标直接传资源路径，不维护业务图片回调。安装按 ItemSlot、RewardEntry、Mail 的精确依赖版本顺序进行。

图片路径规范：ItemSlot.Icon 固定为 string，优先传有效资源路径，由现有 SyncSprite/UILoader 加载。Mail 不再自行 LoadAsync<Sprite> 或维护图片回调；IconVisible 表示宿主是否要求显示图标，并非加载完成状态。路径加载的取消/乱序保护属于共享框架能力，不能把旧业务回调保护的验收结论沿用于此实现。
