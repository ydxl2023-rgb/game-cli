# 商店与医院独立系统（2026-11-26）

本轮实现交易行首档材料购买 → 医院制作药剂 → 治疗 → 任务推进。

## 职责与事件

- ShopSystem（10005）负责商品列表、数量确认、购买结果及所属窗口。
- HospitalSystem（10006）负责医院交互、制作、治疗时钟及所属窗口。
- HomeSystem 负责 NPC、导航、修复表现和场景输入；不处理医院页面或商店购买命令。
- Master 登录时激活 Hospital，HomeReady 后开始治疗调度；Shop 从场景入口按需激活。返回登录统一卸载业务系统。
- Hospital 独立重载通过 HomeSessionRequested / HomeSessionAvailable 查询当前场景是否可用，HomeSessionClosing 负责会话关闭通知；系统之间没有具体 System 引用。
- HomeVillageEntity 暂保留既有库存、消费、制作、治疗及存档聚合，保证同一次消费和所得统一保存。未拆为多个非原子存档。

## 配置与原作依据

原作 UITradeMarketView、ShopBuyPage、BuildingInteractiveModule 的 AuctionHouse 入口及 tbshop、tbfeatureunlock 为依据。configuration-source.json、original-ui-audit.json 保存本轮证据；imports.json 逐项记录资源原路径、资源 ID、目标位置、SHA256 与用途。

config/propShop.xlsx 提供 ShopGoods 和 ShopSetting。首档商品 6001 草药、6006 鱼、6011 铜，价格分别 30、10、10；按原排序展示。交易行解锁要求任务 60101 已领取。数字 ID 引用；显示文本进入语言表。每单最多 100，购买确认重新验证余额与剩余数量，取消不扣款，重复确认不重复购买。

原作服务端动态库存、刷新、其他档位和装备商店不在本轮范围。当前是本地首档适配，不能声称还原线上商店全部行为。

## 资产与重建

医院脚本迁至 Systems/Hospital，UIHomeHospital 名称和资产 GUID 保留；预制位于 Widgets/Hospital，图片及 Spine 位于 Widgets/Hospital/Resources。Home 仅保留场景医院入口 UI。

商店预制为 Widgets/Shop/UIShop.prefab、UIShopPurchase.prefab，以及 Widgets/Common/Micros/UIShopOffer.prefab。

重建顺序：collect.py → build.mjs → ../town/export-scene-config.ps1 → design.py → install.ps1 → bind-metadata.ps1。正式 UGUI 导入和元数据绑定完成后，必须等待 MiniwowShopSetup 原生绑定完成，恢复列表模板、ScrollRect 和 Spine 比例。不可手改 prefab YAML。

## 验证与边界

- Unity 编译通过，运行检查无错误日志。
- 实际按钮购买草药，领取购买任务，进入医院制作药剂，领取制作任务，完成治疗并达到任务完成状态。
- 余额不足、售罄、每单数量上限、重复确认及重新读取存档均通过探针验证。
- 治疗中独立卸载/重载 Hospital 完成治疗，无重复扣药；返回登录清理逻辑通过独立源码复核。
- Home、Shop、Hospital、本轮 Editor 工具及系统架构的本地 CodeVerify 检查通过。证据在 ../hospital-ui/*-local.json 和 runtime-evidence.jsonl。
- 已恢复用户原存档：60105 Active、金币 18740、草药 50、药剂 0、已领取 60101–60104、健康 100。
- 本轮任务数据库与在线模板服务不可用，未更新数据库任务状态或宣称数据库验收通过；资源导入记录先保存在 imports.json。
