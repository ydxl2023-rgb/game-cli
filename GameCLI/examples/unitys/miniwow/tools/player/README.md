# 首版可控玩家

2026-11-25，miniwow/client，GameCLI 项目 `p-fac9bb57-0a72-4db3-9f48-7d42e95f37a3`，Unity 身份 `83c29c9f-21c1-4597-9691-6707ca146fa7`。

从 App 场景启动，进入营地后在无 UI 遮挡处按住鼠标左键或单指拖动。摇杆固定显示在底部中间，拖动位移控制方向与力度；松手停止并归中，镜头跟随。原来的场景拖镜头已关闭。点击旅店店员仍驱动访客往返，不代表玩家消费或建筑业务已经实现。

## 实现与所有权

- TownEntity 持有四个独立 ActorEntity：访客 1、旅店店员 2、铁匠 3、玩家 4。玩家配置别名为 `Config.Excel.ACTOR_TOWN_PLAYER`，实例 ID 与配置 ID 分开。
- ActorEntity 保存位置、方向、输入和阶段，确认导航结果；手动角色通过 IActorMovement 约束移动，原有 NPC 路线不变。GamePlugin → XEntityManager → TownEntity 是唯一逻辑时钟。
- ActorShape 继承 XActionStateMachine，玩家 Moving 对应 `run`，Idle 对应 `idle`。动画名称集中在 ActorAnimationNames；同状态不重启动画，隐藏/解绑 Shape 不停止实体。
- ActorJoystick 仅展示输入；TownScene 处理指针归属、屏幕到世界方向转换与镜头，TownSystem 转发并检查输入门禁。松手、地图切换、失焦、暂停、多指、触摸取消和离场会清空输入。
- TownNavigation 使用原包 30 个瓦片构成的 OriginalTownNavigation.asset，通过独立的 NavMeshAgent.Move 处理移动与沿边滑动。实体检查水平速度预算，坡面高度由原生导航计算。NavMeshData 注册属于场景，销毁时仅移除自身注册。

## 配置与资源

`config/propActor.xlsx` 为参数源表，通过 UnityCLI excel dry-run/set 写入；`tools/town/export-scene-config.ps1` 同时导出 Scene、Actor 表。GamePlugin 完成两表校验后才加载 Town/Home，缺表不回退硬编码。

原作研究见 `research/code-study/PLAYER-CONTROL-STUDY.md`。速度 9、横向系数 0.9、模型比例 1.89、跟随速率 20 来自原代码。源摇杆半径 48、源设计宽度 720 在 1080 宽 Canvas 上得到 72；源出生点恢复为 (-23.90721,35.17353,14.11279)，导航落地高度为 35.1613922。

2026-11-25 修正：之前自行烘焙场景和直线 Raycast 截断改变了原可行走范围、沿边行为，现已停用。原 NavMesh-mesh.asset.json 的 30 个 tile 数据及每项 16 字节 hash 与原生资产序列化回读逐项一致；Hero agentTypeId=-1923039037、radius=0.5、height=1.09，原代理类型加入 NavMeshAreas。原 GuideStoryController 初始化禁用 GuideObjs，Blocks 属新手引导开关，不在正常营地擅自启用额外障碍。

`resources.json` 记录七个资源的源 ID、路径、目标、用途、源/目标哈希和转换方式，已同步本地 SQLite town_imports 与内网 PostgreSQL miniwow_resources.scene_imports，未覆盖人工 annotations。

|资源|用途/转换|
|---|---|
|role_01.png / atlas.txt / skel.bytes|玩家 Spine，原文件不变|
|common_bg_1.png|原 214×214 底图，组件 scale=0.8|
|common_bg_2.png|原 atlas 裁片 74×74 放回 214×214 透明框，offset=(70,70)，禁止拉伸裁片冒充原框|
|common_bg_12.png|方向指示图 33×19 放回 37×23，offset=(2,2)，方向组件原尺寸 37×200|
|NavMesh-mesh.asset.json|原导航 30 tiles；MiniwowOriginalNavigation 使用 Unity 原生序列化恢复，不重新烘焙|

`restore-original-ui.py` 可复现透明边距恢复并逐像素验证、更新台账。目标 PNG 经正式 UGUI CLI 导入，预制和场景绑定由 MiniwowPlayerSetup 保存。旧 TownNavigation.asset 仅留作 CompareCoverage 的回归对照，不被 TownScene 引用。

## 复现与验证

1. 执行 `tools/town/export-scene-config.ps1`，Unity 刷新并编译。
2. 用 `joystick.json`、`materials.json` 执行 UnityCLI ugui UIActorJoystick dry-run，再正式生成 `Assets/Data/Widgets/Actor/UIActorJoystick.prefab`。
3. 在非 Play 模式向 `tools/town/setup.request` 写入 `player`，MiniwowPlayerSetup 安装玩家与导航。如果重建整个 Town，先写 `install`，完成后再写 `player`。
4. 向请求文件写 `app`，从配置表的 App 入口进入 Play。
5. 执行 `verify-player.ps1`，再调用 MiniwowOriginalPlayerProbe.CompareCoverage / VerifyWallSlide。旧 verify-town 的可见店员点击用例依赖旧镜头出生点，不用于本次验收。查看 `runtime-evidence.json`、`install-result.json`、`code-actor.json`、`code-town.json` 以及独立验收报告。

自测通过：配置/编译、非法输入、速度限制、独立实体、斜坡移动、导航边界、run/idle 与动画持续性、镜头跟随、隐藏 Shape、地图中断、焦点/暂停取消、指针归属、1080×1920 与 1080×2400和 Town 重建。原导航采样 872 点可行走，旧重烘焙仅 729；12 个边缘滑动案例通过。30 瓦片/哈希和摇杆帽像素由独立验收再次核对。独立验收见 [independent-review.md](independent-review.md)。

`player-moving.png`、`player-idle.png` 是运行对象的 Camera/Canvas 原生渲染预览。当前桌面截图工具返回旧帧且窗口访问不可用，因此**不将预览称为真实桌面操作录屏**；真实触屏设备与连续手感仍需设备验收。

未包含战斗、自动寻路、玩家建筑消费、解锁升级、存档、动态避障或水域行为。整个 Town 根禁用代表释放；恢复应走 TownSystem 激活流程，不能直接将根对象重新启用充当暂停恢复。

GameCLI 项目源码分组：UIHome 根节点持有 Home/UI 源码及 Home/Scene/Actor 配置；已有 CampVisible 节点承载 Town/Actor 源码。`tools/home-ui/prepare_studio.py` 生成两组有界源码包，不改变画布布局或标准模板。

本轮按用户确认改为固定摇杆：主界面可输入时常驻，底座使用原 MainPage JoystickPivot (359,1219) 按 720×1560 源尺寸、宽度1.5倍换算为底部锚点(-1.5,511.5)。Begin只记录手势起点，不移动底座；松手归中。原提取 UIJoystickCom.OnTouchBegin 存在触点重定位，本轮固定行为以用户最新要求为准，不把静态代码说成完全相同。

UI 只阻止新手势开始；已捕获拖动经过 UI 或视口边界不中断。在 UI 上释放只清空移动，不触发场景点击。地图切换、失焦、暂停和取消仍停止。新增 VerifyPointerCapture 使用实际 UI Graphic 命中点，验证 UI 起手拒绝、跨 UI 保持捕获、底座不变和 UI 上释放归中。见 fixed-joystick-review.md。


## 2026-11-26 显示比例与重进修复

按用户反馈做本项目屏幕占比调校：propActor.xlsx/Actor 10001 的 ModelScale 从原作证据值 1.89 调整为 1.5（约缩小 21%），经过 UnityCLI Excel dry-run/set 和正式导表。原作数值不改写为 1.5；该值是本项目适配参数。导航、速度和出生点未改。MiniwowPlayerSetup 从表安装，重新安装不会恢复旧比例。

固定摇杆 Stick 统一缩放 0.8，原始图片、透明边距和位置保持，ActorJoystick 半径同时乘显示缩放，避免缩小外观后帽子超出底座。原 321 设计单位根现在视觉宽 256.8，底座约205.44。原型资源与原版 UI 全局适配尚不是逐像素复刻。

HomeScene 初始化不再从场景中已有镜头位置猜测跟随偏移，而按当前玩家位置+3单位焦点重新计算，使重进不继承编辑时的旧焦点。实际返回登录再进入两次玩家(-23.90721,35.1613922,14.11279)，相机(22.0647,83.68479,60.0847168)一致。VerifyPointerCapture验证跨UI保持捕获通过。


## 2026-11-26 移速与摇杆敏感度校准

当前玩家 MoveSpeed=6，通过 propActor.xlsx 源表 dry-run/set 后正式导表；原作静态代码为9，6是用户反馈后的本项目手感调校，不改写原作证据。实际Unity计时2.283706秒、水平位移13.702249，测得6.000004单位/秒；松手停止通过。

摇杆满速手势恢复原48源单位×1080/720=72画布单位，不再乘外观缩放0.8。帽子在Stick本地用delta显示、随父节点缩放，仅影响外观；归一化输入仍按原手势半径计算。上一节将输入半径随外观缩小的写法已被本节替代，防止原有拖动距离更早达到满速。跨UI持续捕获回归通过。
