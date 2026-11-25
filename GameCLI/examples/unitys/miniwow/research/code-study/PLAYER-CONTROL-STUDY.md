# 主城玩家与摇杆控制研究

日期：2026-11-25。结论：原作有独立的主城玩家 BuildingPlayer，由浮动摇杆控制；自动探索和排队的 BuildingHero 是另一类角色。下一步可以制作主城可控玩家，但不能直接把当前演示访客视为原作玩家。

## 证据范围

本次读取已提取 HotUpdate.dll 的反编译 C#，并用 REA 确认 UIJoystickCom、BuildingPlayer、JoystickData 三类均存在，选中范围为 3 类型、42 方法。DLL SHA256：`744389e4e7176fd4244b874bc4da95b0e863b04dc563b24e9c966a3dca77a415`；MVID：`e4680c84-286e-403b-b0d0-861b52dbef4d`。文件哈希记录见 `evidence/joystick-study.json`。

REA 摘要证据 ID：`ev_6521603d5956b10dd4c26a9797b04112d8a8070ecf9f6496b8d6d3b4d0be0f8d`，父记录 `ev_59c5160d07479e0e32d731825caafa240285aa6c73ad9e041ec00a028c48cfe4`。随后完整成员请求遇到 Transport closed，未据此宣称完成全部 IL 审计；停止重试，使用本地已有 ILSpy 核对关键停止分支，输出 `evidence/building-player.il`。本轮未执行原作、未修改 Unity 客户端；静态行为不等于已验证真机手感。

以下代码路径均相对于 `managed/decompiled/HotUpdate/Zombie/`；行号是当前提取副本的位置。

## 原作角色身份与更新

- `BuildingGameMode.cs:1097` 的 InitPlayer 单独创建 BuildingPlayer、设置相机、加载模型并初始化。`OnUpdate:232` 更新玩家，随后独立更新 heroes 集合；NPC 模型也有自己的动画更新。
- `BuildingPlayer.cs:277` 的 InitModel 加载 Spine `role_01/role_01`，不是 `hero_20025`。初始位置在 `:56`为 `(-23.90721, 35.17353, 14.11279)`，模型旋转 `(30,-135,0)`、缩放 `1.89`。
- 获取模型子节点 NavMeshAgent，关闭 updateUpAxis/updateRotation，Warp 后绑定触发器。逻辑类同时负责输入、导航、模型、相机和资源气泡；这套职责不能原样搬入新的 ActorEntity。

## 摇杆输入链

`UIMainView.OnInitialize:185–200` 取得 Joystick、JoystickPivot、JoystickDirCom 和 TouchLayer，把触摸层交给 UIJoystickCom.InitGesture。

`UIJoystickCom.cs`：

1. `OnTouchBegin:176` 把摇杆根节点放到当前触摸点，ShowType 设为 1。它是浮动触点摇杆，默认归位点来自 JoystickPivot，不是始终固定左下角。
2. `OnSwipeBegin:102` 记录本次起始触点。`OnSwipeMove:112` 计算当前触点减起点得到二维方向与距离。
3. 距离限制为 48 个原 FairyGUI UI 单位；方向归一化，强度 `distRatio = min(distance,48)/48`。零位移发出零输入并隐藏方向指示。
4. 移动时发送 MoveJoystick；方向指示旋转并跟随玩家世界位置投影到 UI（HandleNotifiction）。
5. `OnSwipeEnd:164` 将方向和强度置零，发送 StopJoystick。`OnTouchEnd:187` 将摇杆放回 pivot、隐藏指示，并在帧末重置 FairyGUI 输入状态、恢复触摸拖动灵敏度。

48 是原 UI 坐标单位，不能直接当作 1080×1920 工程的屏幕像素。实际 UGUI 半径应按 Canvas 坐标和原资源比例适配。

### 停止分支的细节

JoystickData 是 class，UIJoystickCom 重用同一实例。BuildingPlayer.HandleNotification:118 直接保存该实例引用，StopJoystick 分支本身没有显式停止操作。IL 在消息 27 分支直接返回，与反编译结果一致。因此这条链依赖 UI 先把共享对象强度归零，下一帧 MovePlayer 读到零值后停下。

这不是推荐的新实现契约。我们应传递输入值并明确处理结束/取消、UI 关闭和失焦，清零实体输入；不能依赖共享可变消息对象的副作用。

## 角色移动、朝向与相机

`BuildingPlayer.MovePlayer:309`：

- enableMove=false 或正在自动导航时不执行手动移动。
- 输入为零时播放 `idle`，水中为 `idle_water`；非零播放 `run`，水中为 `run_water`，仅当动画名改变才切换。
- 依据相机 forward/right 投影到 XZ 平面，以屏幕方向转换世界方向。FairyGUI Y 轴方向取反，横向系数为 0.9。
- 位移公式为 `deltaTime × 9 × moveSpeedTimes × distRatio × (-dir.y × forwardXZ + 0.9 × dir.x × rightXZ)`，交给 `NavMeshAgent.Move`。不能用只改 Transform 的无边界移动宣称复原原作。
- `joystickData.distRatio >= 0.01` 出现在手动输入打断导航判断中；MovePlayer 的停止判断是零值比较。0.01 不能直接描述为原作移动死区。
- 以输入 x 的符号翻转模型；移动时 CameraFocus 跟随位置加 `(0,3,0)`，SmoothFocus 使用公共缩放及 20 的平滑参数。
- `SetWaterState:563` 使用 0.7/1 的速度倍率；已找到 GuideStoryController 的调用，不据此断言所有水域均有自动检测。

原代码的横向系数、反向 Y、场景比例要结合目标相机和 UI 坐标理解。GameCLI 的适配参数应进入配置，不能照搬魔法数或在 Sprite 镜像时顺便改变碰撞体缩放。

## 自动导航与建筑触发

- `HandleNotification:133` 的 EnablePlayerNavigation 计算 NavMesh 路径，非引导模式调用 MoveTo；每 0.3 秒刷新导航线。
- 手动输入达到阈值且不处于引导模式时发送关闭导航消息，并发送 BreakNavigationGuide。引导模式存在不同分支，不应写成无条件手动优先。
- `MoveTo:442` 取得路径拐点；`NavigatingPlayer:471` 沿拐点移动，距目标小于 0.15 后进入下一点；结束回调可通知 PlayerNavigatingFinish。
- `InitColliderTrigger:303` 注册进入/退出；`OnEnterBuilding:398` 向 Building/Guide 层派发进入事件。`BuildingGameMode:565` 检查建筑存量及领取中标记后播放资源飞行动画，再请求领取建筑金币。
- 这说明“控制玩家接近建筑”是经营交互的一条入口，不能把进入触发区等同于升级、解锁或交易成功。

本轮未验证原作 NavMesh 数据是否可完整移植，也未证明碰撞层和触发区域已在当前 Home 场景恢复。

## 可定位的资源

角色目录：`extracted-resources/assets/data/spines/role_01/`，已有 PNG、atlas、skel、材质 JSON 和 prefab 结构 JSON。skel 为 53,036 字节，头部版本串为 4.2.43，SHA256 `3a7c0a86d6d3c1d54565ef39deb0126dbf84f2c65d65cc73b57933e8e0bb82ca`。其中检索到 idle/run/idle_water/run_water 字节串，与代码引用吻合；尚未在 Unity 加载验证动画表、材质和画面。

原 UI 结构已记录于 `../../tools/home-ui/COMPONENTS.md:553`，比早期只含 item 元数据的导出更完整：

| 用途 | UI URL / 原始资源 |
| --- | --- |
| 摇杆组件，源尺寸 214×214 | `ui://1pyb9685myzx4n`，Main/JoystickCom，含 ShowType 控制器 |
| 摇杆底图 | `ui://1pyb9685myzx4o` → `extracted-resources/fairygui/Main/Images/common_bg_1.png` |
| 摇杆帽 | `ui://1pyb9685myzx4p` → `extracted-resources/fairygui/Main/Images/common_bg_2.png` |
| 方向指示组件，源尺寸 37×200 | `ui://1pyb9685bmzw4x`，Main/JoystickDirectionCom |
| 方向指示图 | `ui://1pyb9685bmzw4w` → `extracted-resources/fairygui/Main/Images/common_bg_12.png` |
| 触摸层，源尺寸 720×1560 | Main/TouchLayer，`ui://1pyb9685op293m` |

资源用途来自组件引用与代码交叉核对；本轮没有将这些资源导入客户端，也没有修改资源库人工标注。

## GameCLI 下一步建议

以下是适配方案，不是原作已有的 Oathx 架构，也不是本轮已实现功能：

1. 在 TownEntity 中新增独立的玩家 ActorEntity，保留现有演示访客与驻留 NPC；玩家使用 role_01。
2. UIJoystick 只采集指针并发送方向/强度/停止意图；TownSystem 路由到当前玩家，不让 UI 直接移动 Shape。只有出现独立跨场景玩家生命周期时再评估 PlayerSystem。
3. 为 ActorEntity 增加可控移动输入与规则；现有 Visit/Return 路线能力只适用于演示访客，不能冒充自由移动。导航/碰撞作为移动结果适配层，由实体接收合法位置；ActorShape 保持 XActionStateMachine 单向表现。
4. 明确输入所有权：摇杆接管游戏区域单指拖动，按钮与弹窗阻挡摇杆；当前 HomeSceneCamera 同一手势的拖动需要禁用或另设模式，相机在玩家移动时按规则跟随。
5. 首个可玩验收仅做浮动摇杆、玩家 idle/run、可行走范围、停止/取消/失焦清零、镜头跟随、UI 阻挡、多角色隔离。自动导航、水中状态和建筑经济按后续范围接入。
6. 导入角色/摇杆资源并核对导航可行走区后，先完成“可控制地走到建筑附近”的闭环，再开发建筑交互、解锁及战斗入口。

尚待运行验证：真机多点触摸、弹窗/引导中的输入覆盖规则、原作相机实际手感、完整 NavMesh 与触发区域，以及 skel 正式导入后的全部动画。下一轮实现前应确认这些适配边界，不能把本报告静态研究称为原作运行复现。

## 2026-11-25 实现纠偏

上述未导入/导航待验证为研究时的历史状态。现已原生导入 role_01 与原导航 30 tiles，恢复原出生点、Hero 代理参数和 NavMeshAgent.Move；逐 tile 字节/hash 一致，12 个沿边滑动案例通过。摇杆帽裁片 74×74 必须按 offset(70,70)恢复至原框214×214；方向图33×19按offset(2,2)恢复至37×23。详见 ../../tools/player/README.md 与 independent-review.md。原引导 GuideObjs 在 GuideStoryController 构造时禁用，不能把其 Blocks 直接当正常营地常开障碍。完整自动导航、引导、触发区与真机触摸仍不在本轮验证范围。
