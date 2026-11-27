# 商店与旅店入口联调说明（2026-11-26）

## 本次实现

- Home 的商人入口保持建筑 60019 → 系统 10005；旅店入口 60017 → 系统 10007。都通过 `SystemActive` 交 Master 装配，不直接引用其他业务系统。
- `Navigate(buildingId)` 对已实现的商人、旅店目的地复用 Home 原导航网格、角色输入和路径推进。到达后请求系统激活，页面完成绑定后才完成任务表现。导航成功不代表任务业务完成。
- 手动移动、取消引导及同建筑替换按 `TaskPresentationAction` 实例归属。导航完成事件、延后激活完成和页面打开结果均携带请求身份；不用 `generation` 字段。
- Shop/Inn 在途页面请求被所属 guide 取消时，使用现有 `CloseWidget` 取消资源加载。手动 NPC 请求不属于 guide；同来源重复打开合并，不同来源替换先关闭原请求。
- 商店任务栏与领取请求按 `TaskBuildingType.AuctionHouse` 过滤；整章结束、没有跟踪任务时隐藏。购买累计观察按商品 ItemID 汇总，可区分草药、鱼，不按任务 ID 编写分支。
- 商店固定文字从导出的 CommonText → Language 数据解析，沿用当前医院的制作流程；没有新增运行时语言切换功能。

## 原作证据与产品适配

`research/code-study/managed/decompiled/HotUpdate/Zombie/UITradeMarketView.cs` 的 `RefreshMaterials` 使用服务器 `materialItems`，按物品 Tier 过滤、按 `CfgShop.Sort` 排序，再赋值 `sellItems.numItems`。因此 `SellItems/0/BuyBtn` 是当时列表第一项，不能视为稳定的鱼商品 ID。

本地目录展示完整首档三种材料，故保留原引导 token，由当前任务 `Param1` 匹配 `ShopGoods.ItemID` 选择真实商品按钮。绑定依据见 `guide-target.json`。每张卡持有序列化 `TaskGuideTarget`；VM 只投影当前是否为目标，不保存实体或场景引用。

框架依据：development `SYSTEMS.md` 的 Entity/System/Widget 分工及 Shop 选择与确认事务；当前 `UISystem` 的同 key 在途请求与 `CloseWidget` 取消契约；`UIWidget`/`SyncScrollRect` 现有列表绑定。没有引入第二套 UI 加载器。

## 资产制作与验收入口

代码编译后，在非 Play 模式写入 `tools/shop-ui/bind.request`，内容为 `install`。原生 `MiniwowShopSetup` 为正式 UGUI 生成的商品 prefab 绑定原手指、按钮目标和卡片私有引用，再重建 Shop 的列表模板。VM Binding 字段未变，不需要手写 Metadata 或 prefab YAML。

`MiniwowFacilityProbe` 提供以下验收方法：

- `PlaceAtResident(60019/60017)`：明确的编辑器站位 fixture，只改变本次玩家逻辑位置，不改变任务、库存或存档。
- `InspectButton("MerchantClick"/"HotelClick")`：记录真实 graphic raycast 顺序、命中对象与按钮状态。
- `Click(name)`：经过 raycast 校验后派发真实按钮指针事件，不直接调业务方法。
- `Snapshot()`：记录活动 Widget、输入是否恢复、旅店建成模型/旧模型显隐及经济快照。
- `RestorePlayer()`：恢复当前同一 Home 会话的 fixture 前位置。
- `BeginCancellationRace()`，数帧后 `AssertCancelledActivation()`：检验旧到达、同建筑替换、Master 延后激活取消。
- 系统已安装后 `BeginPageReplacementRace(buildingId)`：原 guide 重复打开、同帧取消、手动重复打开，随后检查只保留一个初始化的手动页面。返回 `pendingWindowExercised`，为 false 时不得声称已运行覆盖异步加载中的取消窗口。

本文件记录实现与复现方法；运行结果以本轮总验收记录和原始 CLI 输出为准，不以静态检查代替真实入口验收。

## 最终运行复查补充

实际发现非缓存页面重复 OpenWidget 会产生多个实例，已通过 UISystem.TryGetWidget 查询实际打开栈复用商店与购买确认页；引导状态检查放在购买进度回放之后，保留同步取消的时序语义。重复入口与关闭清理已复验，详见 ../inn-ui/ACCEPTANCE.md。

MiniwowShopSetup.ImportBorders 读取 ../hospital-ui/slice-borders.json 中本系统实际使用的原始九宫格边界，ConfigureGraphics 将无九宫格的 cons_bg_7 控件框替换为同系统已导入的 common_bg_11，并设为 Sliced；导入布局后必须执行此原生绑定步骤。最终截图见 visual-final.png，不以旧拉伸边框截图验收。
