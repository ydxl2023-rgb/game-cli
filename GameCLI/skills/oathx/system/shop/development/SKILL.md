---
name: oathx-shop-development
description: 独立 agent 参考 SV 大厅商城范式开发充值、普通礼包和本地模拟购买，交付 UI、VM、实体、原版动画与配置包。
---

# Shop 开发

先读[系统开发流程](../../../system-development/SKILL.md)、[开发规范](../../../development-rules/SKILL.md)、[Core](../../../core/SKILL.md)、[Entity](../../../entity/SKILL.md)、[Widget](../../../widget/SKILL.md)、[Widget Extension](../../../widget-extension/SKILL.md)。由不同 agent 按[本系统验收](../acceptance/SKILL.md)检查。

页面和子窗口的业务请求事件统一定义在 `UIShop.cs`。Widget 通过事件提交用户操作，System 调用 Widget 的 `Invalidate` 刷新实体数据。

## 设计来源与范围

用户选定 SV 大厅入口实际打开的 RechargeMainView；不得替换为旧 ShopTabView/ShopView 或普通道具商店。主预制只是动态容器，还必须交付充值页 Recharge、普通礼包页 NormalGiftView 和礼包弹窗 NormalGiftBagBuyView。GoodsBuy 不是此礼包流程的购买弹窗。

读取数据库实际版本、scripts、configurations、伴随页面和动画资料；未提出设计变更时保留其设计与行为。不要求产品 agent 再访问 SV 源工程。当前参考实现限定充值与普通礼包示例；壁纸、成长基金、特价礼包、运营推荐、活动、SDK 支付和客服是额外分支，不能声称已经实现完整 SV 运营商城。

## 实现契约

- ShopSystem 管理商城业务，UIShop 管理分页；ShopEntity 持有分类、待确认商品、模拟余额、奖励和限购状态。主窗口的 UI 和 VM 在 Shop/UI、Shop/VM，分页专属代码归各自的模块目录。
- 充值和礼包列表使用真实 Fancy 列表与独立 Widget Cell，保留主分类及二级页签结构；切换后数据、选中态和列表点击索引一致。禁止用固定按钮阵列替代列表。
- 本地模拟必须明确标识，不调用真实支付/充值接口，不把示例价格、余额和奖励视为正式商品。产品接入时由产品定义付款、订单和发货契约。
- 确认时重新验证商品、余额及限购；成功后消费待确认选择，重复确认不能重复扣款/发奖。取消、分类切换及系统停用清理待确认状态。Entity 返回枚举状态，展示层查表生成文案。
- 固定文案合并进 propCommonText；配置包按[配置交付规范](../../../system-development/references/configurations.md)独立保存源表、条目、哈希及导表依赖。先编辑 propShopOffer（可购买商品）、propShopPageItem（分页商品及顺序）和 propShopSetting（本地钱包与展示参数），再导出配置。展示卡片与模拟购买从配置读取，测试数据也随表维护。商品目录已初始化后保留交易状态。
- 商城页面统一位于 `Assets/Data/Widgets/Shop/`。商城主预制为 `Shop.prefab`，内嵌分页放在 `Pages/`；独立 Widget 及各自的配套资源放在 `ShopGift/`、`ShopPurchase/`、`ShopRecharge/` 子目录。主模板之外的伴随页面、列表项 Metadata 与图片必须随源码材料交付，不能只发布主卡片。

## 分页身份、代码和预制

- 每个标签用独立的 `ShopEntity.Page` 和唯一业务 ID 注册。布局与交互相同、仅 ID、标题或商品配置不同的标签共用分页类。静态布局使用 UIShopPage；按路由 ID 读取配置列表的分页使用 UIShopProductPage；礼包与充值由 UIShopCatalogPage 转交绑定的 IShopCatalog Widget 刷新和解绑。分页实例在 InitializePage 接收路由 ID。展示或交互行为分化时再拆类，预制可用 Variant 保留独立配置。
- `ShopEntity.Page` 是纯配置数据，持有 ID、标题、预制路径、所属一级标签和显示顺序。打开的 `UIShop` 提供 RegisterPage、RemovePage、SelectPage，也支持增删一级标签。页面实例按业务 ID 缓存在本次窗口内，注销分页时释放其缓存；删除选中项时选相邻项，最后一项删除后清空内容。关闭时解绑并忽略迟到回调。
- `Shop/UI/UIShopPage.cs` 定义初始化、显示、刷新、隐藏、释放钩子。代码按八个一级标签归档：Recommended、Newcomer、MonthCard、SpecialOffers、Growth、Cosmetics、ValueReturn、Recharge。二级分页类直接放在 `Shop/<一级标签>/UI/`，VM 放同级 VM 目录，预制直接放在 `Shop/Pages/<一级标签>/`。同类二级分页的共享实现放在所属一级标签目录。共享商品卡、列表和交易弹窗按实际复用范围归档。充值组件、卡片及 VM 集中在 Recharge 下。静态页无需空 VM。
- `UIShop` 负责导航，购买、领取等业务通过 Widget Send 交给 ShopSystem。发布时核对 navigation.json 中每个业务 ID 与 prefabs.json 中的独立模板版本，材料必须带上 variant、本页 MonoScript、父预制及每个文件的原始 meta。

配置商品列表在 RefreshPage 中接收 ShopEntity，通过 VList 和 SyncScrollRect 展示。四种商品卡布局使用各自的 Metadata 映射同一套 VM，覆盖价格、图片、数量和状态。验证每个配置分页的条目数量、重复刷新、成长礼包遮罩、壁纸点击与缓存返回。

## 动画与交互

不能只检查 openingAnimationClip。RechargeMainView 根 Animator、ShopTab、货币栏、返回栏、商品单元和礼包弹窗各有动画；商品还可能通过代码重播点击反馈。核对默认状态、运行时 Play 调用、目标路径、CanvasGroup 与 Graphic 的透明度语义。原版主界面有 0.5 秒交互门禁，结束与停用时必须恢复，不能留下永久遮挡。

原生动画/Controller、预制绑定和 design.animations 一起交付。检查首次打开、再次打开、分类切换与列表复用；最终布局不能漂移。未实现的动态物品转换/壁纸/粒子分支必须逐项说明，不能静默删动画后宣称还原完整。

## 工具与交付

制作工具位于 integrations/codex/scripts/import-sv-shop.mjs、prepare-shop-animations.mjs、build-shop-bindings.mjs、bind-sv-shop.ps1。这些是制作端工具，不是产品集成时必须重新提取的依赖。构建 VM 的脚本会重建本地生成副本，不得覆盖已人工编辑的产品代码。

先保留完整原生导出，再建立适配清单；Unity 资产只用 UnityCLI 生成与绑定。模板首次登记与后续版本候选分开处理。独立验收完成前如实标记未验证项；发布不是通过全部验收的替代证据。

## 当前参考适配边界

首次充值优惠与赠送计算尚未实现，保留的 FirstPay/Bonus 字段不能被描述为已接入业务。充值示例复用本地确认弹窗是有意适配，原 SV 充值由 SDK 发起。浏览器主卡片是动态外壳；四个设计及原生资源在 scripts 中，功能演示以 Unity 为准，不宣称浏览器执行 C# 商品列表。

UISystem 关闭类型数组必须取消尚未完成的加载，并用请求身份拒绝旧回调。只删除 loadingMap 后再重开不够，旧回调不能删除新请求或创建旧页。待确认商品存在时拒绝另一次选择；取消和切分类后可重新选择。独立产品必须验证这些行为，不能只检查当前 IsActive。

## 实体职责约定

ShopEntity 私有持有商品目录，并复制输入和查询结果中的可变商品元素；通过 GetProducts 获取当前分类快照。目录、选择、余额、限购及购买校验统一在实体中，System 只驱动页面、显示文本并发出购买结果。


## 独立商品卡与 ItemSlot

- 充值卡 `Widgets/Common/Micros/ShopRechargeItem.prefab` 与礼包卡 `Widgets/Common/Micros/ShopGiftItem.prefab` 是独立预制；页面只嵌套引用，并保持 Fancy.cellPrefab 和 ProductTemplate 指向实际嵌套实例。代码仍属于 Shop/UI 与 Shop/VM。
- 两种商品 VM 通过 `[Binding(typeof(SyncMetadata))] VMItemSlot item` 组合共享 [ItemSlot](../../item-slot/development/SKILL.md)。图标只传 string 资源路径；礼包数量使用 Item.Text。名称、价格、首充标记、限购、售罄由商品卡自身 VM 管理，交易规则继续归 ShopEntity。
- 保留商品卡原有外观与入场/点击动画。嵌套 ItemSlot 的皮肤差异通过商品卡实例 overrides 表达，不改共享槽位；充值大图不显示槽位底板、边框和数量，礼包保留原白底与数量。内层 Graphic 不拦截点击，商品卡仅保留外层购买点击。
- 列表解绑时先临时清空 Item 释放子订阅，父 Unref 后恢复相同 Item，避免复用销毁数据。不要永久清空模型；销毁时释放嵌套监听。该处理不表示共享图片加载器已具备取消或乱序保护。
- 当前材料包包含两张独立商品卡及它们在页面中的嵌套引用，依赖精确版本 ItemSlot；先恢复 ItemSlot 再恢复 Shop。`compose-shop-items.py` 是制作端迁移工具，不能把历史 `build-shop-bindings.mjs` 输出直接覆盖已编辑代码。
