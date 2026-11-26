# 开场首段与原作的差异核对

核对日期：2026-11-26。结论：当前任务业务顺序可走通，但取水、携水、消耗演出、火灾场景及 UI 挂载方式存在明确偏差。前次的流程通过不能作为原作表现还原通过。本次只研究和整理证据，没有改动客户端或玩家存档。

## 证据边界

使用包内恢复的反编译 C#、Building_sdc 场景数据、实际 FairyGUI 二进制包，以及本项目源码/预制制作输入/前次运行截图。反编译代码不是原始工程源码；没有本次原作运行录像，不宣称所有动态效果已逐帧验证。

新增 `audit_original_ui.py` 按已保存的 FairyGUI `UIPackage`、`GComponent`、`GObject.Setup_BeforeAdd` 读取组件初始位置、大小、缩放和素材引用，输出 `original-ui-audit.json`，记录源包 SHA256。它**不执行 relations、controller gears、transitions**，其坐标是原始设计坐标，不能直接当成 1080×1920 的最终屏幕坐标。

## 已确认差异

|环节|原作证据|当前实现|影响|
|---|---|---|---|
|开始场景|Story1 隐藏任务栏，只显示牧师；医院/旅店/交易行切到火灾模型，显示 GuideObjs 火灾区域和 Blocks|保留正常场景、既有 NPC 和建筑初始模型，只加一组交易行前的火焰|缺少“正在救灾”的上下文，救前救后建筑变化不成立|
|取水引导|水井 Bubble 上挂水图标气泡；路线指向 GetWaterPoint；摇杆上出现手指|顶部大横条、地面黄色圈；圈使用取水碰撞盒中心|玩家分不清水源位置与判定范围，缺少路线/摇杆指引|
|取水演出|触发 ReceiveResAnim：5 个水图标从井口飞向玩家；随后切携水状态、井变空、玩家气泡显示 10|Bounds.Contains 成立后立刻 water=10 并切阶段|缺少动作因果，像走到区域就凭空获得数字|
|携水角色|idle_water/run_water；移动倍率 0.7|仍为 idle/run，移速不变|角色没有携水状态，手感和原作节奏不同|
|携水 UI|BuildingResTipsBubble 跟随玩家模型，世界偏移 (0,3,0)，组件缩放 0.8；Count 为 10|固定屏幕顶部显示“救火的水 10/10”|把角色携带状态改成了固定进度条；镜头移动时位置关系错误|
|救火交互|BuildingOpBubble 挂建筑 Bubble，剧情偏移 (2.92,0.5,-0.53)，有水图标、消耗10和箭头|固定设计位置 (380,1280) 的宽320按钮|按钮不属于建筑，缺少交互目标与费用的空间关系|
|到达后的消耗|先关闭移动并转向目标，再飞出最多5个水图标，结束后播放 e_jiaoshui_1|进入1.2半径立即画固定蓝色曲线并开始1.5秒计时|遗漏了消耗演出阶段，节奏过快、动作不完整|
|喷水定位|e_jiaoshui_1 位于 AuctionHouse_60019/PeopleBubble；原飞出图标也以 PeopleBubble 为目标|曲线终点是手工放置的火焰位置+Vector3.up|特效落点与原作不同，当前画面像在往角色脚边洒水|
|火灾恢复|特效开始后1.5秒恢复交易行模型、显示商人、完成任务；医院/旅店仍保持火灾状态，井恢复|1.5秒结束后只隐藏新增火焰，并打开商人头像对话|缺少建筑/人物/井状态变化，救火结果缺乏场景反馈|
|任务栏时机|Story1_3，即取水表现完成后显示任务栏；对话3043结束指向任务栏领取|FetchWater 阶段就显示任务栏，另加屏幕中央领取按钮|出现时间偏早，主操作区域与原作不同|
|对话|全屏遮罩/点击推进、左右立绘、姓名栏/分隔线、打字机、下一句箭头；未打完时点击先补全文字|小 NPC 图标、一次性全文、独立“继续”按钮，无全屏下一句操作|布局、头像比例、交互方式与节奏均不一致|

## 精确的坐标混用

以下原场景节点及父节点已核对；对应父级均为单位变换，所列可作为世界坐标。

|用途|原作节点|位置|
|---|---|---|
|取水判定|GetWaterCollider|中心 (-17.23,35.3,14.68)，大小 (7,1,6)|
|取水导航终点|GetWaterPoint|(-20.41,35.3,14.46)|
|井口 UI/资源飞出起点|Tax_60020/Bubble|(-16.10,40.00,15.19)|
|玩家救火站位|AuctionPoint|(-9.38,35.3,9.21)|
|消耗飞行/喷水目标|AuctionHouse_60019/PeopleBubble|(-3.75,37.01,7.34)|

当前 HomeOpeningScene 把取水目标圈放在 WaterX/Y/Z，实际这三个字段保存的是碰撞盒中心。两点水平距离约3.19；在靠近区域边缘时就触发取水，但玩家仍离目标圈较远。修正时需要**分别配置/绑定判定范围、导航点和表现点**，不能继续共用同一个坐标。

当前 MiniwowOpeningSetup 将火焰放在 (-8.7,35.4,8.4)，HomeOpeningScene 又把水流终点设为该点加 (0,1,0)。这不是原作 PeopleBubble 的位置。原火灾效果 `e_guide1_initial_1_fire` 是放在世界零点的整组效果，内部子节点承载实际布局，不能用一个任意单火点宣称等价。

## 原作取水与救火时序

1. 3042 对话结束 → 激活 GetWaterCollider → 井口水气泡 → GetWaterPoint 路线与摇杆手指。
2. 进入判定范围 → ReceiveResAnim，每0.1秒生成一个，共5个；每个飞行0.25秒，二次贝塞尔曲线。井口起点先减1单位Y，动态目标为玩家位置+1单位Y，曲线中点上抬2。
3. 原取水函数按 `0.1 × items.Count` 安排业务回调，即配置参数为0.5秒；最后一个图标可能仍在飞行，不能误写为“等待所有飞行结束”。回调切携水状态、井空、显示玩家上方数量10，再进入 Story1_3。
4. 显示任务栏与建筑救火气泡；导航线每0.3秒重新计算。引导模式下路线提示本身不会自动移动角色，点击救火气泡才执行 MoveTo(AuctionPoint)。
5. 到达 → FlyItemAnim：禁用移动、转向目标；每0.1秒飞出一个水图标，最多5个，单个0.25秒；回调参数为 `0.1×5+0.25=0.75秒`。
6. 飞出阶段完成 → PeopleBubble 位置播放 e_jiaoshui_1 → 关闭路线 → 取消携水动画/减速、隐藏角色水气泡 → 操作气泡完成动画。
7. 从该特效开始再等1.5秒 → 恢复交易行初始模型并显示商人 → 请求完成60001 → 恢复水井 → 3043对话 → 指向原任务栏领取。

当前“到达后直接等1.5秒”遗漏了前面的约0.75秒消耗阶段。上一轮把“水在1.5秒结束时归零”作为断点恢复设计，也不等于原作携水表现消失的时点。后续修正需区分持久业务提交和表现状态，不用一个布尔字段控制全部过程。

## 原 UI 布局核对

原包基准720×1560，当前项目基准1080×1920，宽高比不同。应恢复原对象职责及关系，再做等比缩放、底部锚定与安全区域适配，不能把Y坐标直接乘1.5。

原 ConversationPage 主要初始矩形（见 JSON 完整证据）：

- 全屏 nextBtn：(0,-1)，720×1560。
- 左立绘 characterImgLeft：(-10,909)，350×300；右立绘有镜像缩放，由 direction 控制。
- 对话底图 hub_bg_19：(6,1187)，708×337。
- 姓名栏 hub_title_1：(0,1174)，317×66；姓名左对齐区域(40,1182)，196×44。
- 分隔线：(23,1249)，659×22；正文：(60,1282)，608×189。
- 下一句箭头：(600,1413)，25×23；跳过按钮：(548,207)，166×53。
- 原 TaskCom：MainPage 内(0,1259)，398×86；内含任务按钮、章节按钮及用于手指定位的 Pivot。
- BuildingResTipsBubble：101×111；BuildingOpBubble：157×106。这两个组件的位置由 UIMountCom 按世界坐标每帧投影，不是屏幕固定坐标。

当前对话框(35,1190)，1010×480，头像210×230、姓名居中、正文和继续按钮另排；虽用了原底图，布局没有按原组件还原。当前头像来自 CharacterIcon/NPC，而3042/3043配置引用 FullBody 立绘，资源类别也有偏差。精确立绘显示范围仍需核验对应资源和 GLoader 模式。

## 下一轮修正顺序与验收基准

1. **恢复空间关系**：分开井判定/导航/井口锚点，增加建筑 Bubble 和 PeopleBubble 序列化引用；将固定顶部水条、固定救火按钮改为原挂载气泡；取水前隐藏任务栏，领取只走原任务栏与手指指引。
2. **补齐真实动作链**：取水飞入 → 携水 idle_water/run_water 与0.7速度 → 到达/转向/飞出 → 原喷水表现 → 模型和 NPC 恢复。角色数据在 ActorEntity，动画仍由 ActorShape 单向读取；不在 HomeSystem 操作 Spine，也不复活 TownSystem。
3. **恢复场景与对话**：烧毁模型、井空/满、剧情 NPC；恢复对话立绘、姓名栏、逐字显示和点击语义。精确适配先解码 relations/controllers，不再凭截图感觉填坐标。
4. 验收逐阶段截图/录屏：取水前、飞入中、携水静止/移动、救火按钮随镜头移动、飞出中、喷水中、建筑恢复、领取提示。对齐顺序/挂点/尺寸/动画/显隐后再验收恢复、卸载和任务结果；原来的状态断言保留但不足以单独判通过。

## 主要定位

- 原作：`research/code-study/managed/decompiled/HotUpdate/Zombie/GuideStoryController.cs`，Story1/Story1_2/Story1_3/OnGetWater/ExtinguisBuildings/WellState。
- 原作：同目录 `BuildingPlayer.cs`，CreateResTipsBubble、SetWaterState、FlyItemAnim、ReceiveResAnim、EnablePlayerNavigation。
- 原作：同目录 `BuildingInteractiveModule.cs`、`BaseBuildingController.cs`、`UIMountCom.cs`、`UIBuildingOpBubble.cs`、`UIConversationView.cs`。
- 数据：`tools/main-scene/scene.json`、`extracted-resources/assets/data/ui/atlas/{guide,building,main}/*_fui.bytes`。
- 当前：`client/Assets/Scripts/Systems/Home/HomeOpening.cs`、HomeOpeningRuntime、Scene/HomeOpeningScene、UI/UIHomeOpening、UI/UIHome，以及 `client/Assets/Editor/MiniwowOpeningSetup.cs`、`tools/opening/design.py`。
- 当前视觉证据：`tools/opening/dialogue-1080x1920.png`；前次过程截图路径见 play-evidence.json，位于本机 client/Library/OpeningQA。
