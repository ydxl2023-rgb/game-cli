# 模板分类规范

系统模板必须拥有独立的 RuntimeSystem 入口与生命周期（允许间接继承）。不能因为源码包带有其他系统依赖、界面全屏或名称包含 System 就认定为系统模板。

Launch / SvStart 为用户确认的唯一框架例外：由 XLauncher 承担框架启动流程，独立归入系统模板。

预制控件可以包含完整 UI、VM、动画及交互代码，但不拥有独立 RuntimeSystem 生命周期。货币、头像、资料、快捷按钮、Loading 及当前只有 UI 的背包归入此类。

2026-11-18 已整理数据库：

- 系统：SvStart、MasterSystem、SvLogin、SvNotice、SvHome、SvShop、RedDotSystem。
- 预制：ItemSlot、ScrollPanel、ConfirmDialog、CurrencyBar、AvatarFrame、PlayerProfile、ProfileShortcuts、AffinityButton、FriendsButton、MailButton、NoticeButton、SettingsButton、InventorySystem、SvLoading。

迁移脚本 `scripts/classify-template-library.mjs` 默认 dry-run，`--apply` 才执行。遇到未审核 ID 会停止，不自动猜测。分类变更遵循数据库发布守卫并递增版本，历史分类不重写；内容、源码、配置及项目实例通过摘要比对保持不变。旧版资料子控件仍置于明确标记的迁移分组。

本次落实的是已审核目录的数据库分类迁移。任意新模板的 RuntimeSystem 继承关系自动分析与发布拦截尚未实现，不能将本脚本视为通用源码分类器。
