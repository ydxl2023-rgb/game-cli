# 交易行原作布局还原（2026-11-27）

## 用户要求与修复原因

原作页面不得自行设计。旧页面虽然接通购买，但把三列竖牌替换成横向列表、增加金币文字和已拥有数量、把底部图标导航替换成大文字按钮，不能作为原作 UI 交付。

本轮按用户提供的交易行截图、原作 UITradeMarketView、TradeMarketPage、ShopScene、MarketBigItem、TierBox、BuildingPanel 还原主页面。原作 FGUI 基准为 720×1560，组件关系会按实际屏幕调整；本项目仍为 1080×1920。微信宿主顶栏不属于游戏 Canvas，不复制。

## 结构和资产

- 三列 UIShopOffer，根尺寸 260×650，保留原有 ScrollRect/SyncScrollRect、VList 与独立 Widget 绑定；HorizontalLayoutGroup 对齐三张固定类型商品牌。
- 原始 goldshop_bg_11 竖牌、goldshop_bg_12 库存底、common_button_3 价格按钮、common_bg_21 道具边框及 shop_title_1 等级签。
- 原始 goldshop_bg_10 木质面板，九宫格来自 Common 包 scale9Grid，记录于 slice-borders.json。面板像素倍率结合 1080×1920 的实际截图校准。
- 原始 shop_button_2_1 档位框、cons_button_1 日报图标、common_button_7 返回箭头；底部分页使用 FunctionIcon 包的独立图标，按用户截图排列。
- 房间保留原始 e_ui_bg_trading_1 Spine，补齐 role_01 玩家及 NPC 名称；房间延伸到半透明等级栏后面。世界玩家数据和场景角色不被 UI 表现修改。
- 文案进入 propCommonText.xlsx，经既有 e2cs 导出；保留旧语言项，新增 30324–30329。没有写死截图中的库存或更改用户档位。
- 项目 Skill 已加入“原作 UI 禁止自行设计替代布局”的持续约束。

## 参考机制与产品差异

development 源码漂移检查 current（559 文件）。沿用已有 UIWidget/VM/Metadata、ShopSystem 请求事件与 HomeVillageEntity 原子购买，不新建跨系统引用。原作以 FairyGUI 组件关系布局，本项目以正式 UGUI 导入和 Unity 原生编辑器绑定实现同等页面结构。

正式流程：collect.py → update-ui-texts.mjs → ../town/export-scene-config.ps1 → design.py → UGUI dry-run/正式生成 → MiniwowShopSetup → widget-metadata dry-run/正式生成 → 再次绑定。购买弹窗保留既有布局，仅统一字体并保留浅底上的深色文字；本轮不声称还原该弹窗。

## 运行检查和边界

- UIShop 13 个字段、13 个目标绑定，metadata dry-run 通过。
- 三列商品为原配置 6006、6001、6011；价格 10、30、10。参考截图为 20 级，本地仍为已实现的 10 级，名称、品质框、价格和库存按真实配置及存档显示。
- 真实按钮检查：档位展开/选择、购买弹窗、取消、确认购买、装备锁定反馈、日报入口反馈、返回均执行成功。
- 购买 6006 一份：金币 18430→18420，鱼 0→1，库存使用量增加 1；验收后恢复三个用户存档键，与备份逐字一致。
- C# 检查无 error，OX015 为既有 Editor 预制制作时查找指引节点，已人工核对不属于运行时业务查找；架构检查 72 文件、8 系统，0 issue。
- Unity Console 无 error；多轮实际截图和原图并排复核。界面结构和素材已恢复，未声称逐像素自动验收通过。
- 日报尚未接入真实记录、装备交易尚未开放；保留原作入口外观，点击明确反馈，不伪造服务端记录。档位选择仅列出当前已实现的 10 级，不宣称所有材料档位已接入。

本机原始运行证据在 client/Library/OpeningQA/shop-layout-evidence.jsonl；代码/架构报告、shop-layout-final.png、shop-layout-comparison.png 和不可混用的 shop-layout-user-checkpoint.json 同目录保存。素材导入追溯在 imports.json；新增组件审计在 original-components.json。
