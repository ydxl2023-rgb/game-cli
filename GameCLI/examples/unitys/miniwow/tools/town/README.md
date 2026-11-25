# TownSystem 第一阶段

日期：2026-11-25。目标工程：`../../client`。Studio 项目 `p-fac9bb57-0a72-4db3-9f48-7d42e95f37a3`，Unity identity `83c29c9f-21c1-4597-9691-6707ca146fa7`。

本轮实现主城场景生命周期、一个可交互建筑入口、访客往返移动，并按用户要求恢复原作 Spine 表现。不开发建筑经济、战斗或奖励结算，不发布标准模板。

## 操作

从 `Assets/Scenes/App.unity` 运行。Launch 结束后，镜头对准旅店与访客所在街道。保留原相机角度和距离，初始焦点为本轮交互区域的产品适配。

1. 点击旅店门口的店员，绿色衣服的访客沿街道走向旅店，播放原作 `walk`。
2. 到达入口后停下，切换 `idle`，状态为 Waiting。
3. 点击已到达的访客，沿原路线返回起点。
4. 可重复以上操作。移动中重复点击不会重启路线或增加到达次数。
5. 拖动场景仍可平移相机；拖动不算点击。地图预览打开后暂停场景点击与相机输入，返回营地后恢复。

Waiting 只表示已抵达，不代表旅店已解锁、消费成功或行动力已恢复。本轮旅店交互是明确配置的场景验收入口，未接入 BuildingEntity 的业务准入。

## 职责与接口

|代码|职责|
|---|---|
|`Town/TownSystem.cs`|唯一管理 `Home.unity` 的加载、卸载与就绪状态；协调场景交互|
|`Town/TownScene.cs`|序列化引用相机、访客、建筑碰撞体；鼠标/触摸意图；UI阻挡、拖动和屏幕边界检查|
|`Actor/ActorEntity.cs`|独立身份、位置、方向、路线副本、移动阶段与到达次数，不引用场景对象|
|`Actor/ActorShape.cs`|单向读取实体更新 Transform 与 Spine，解绑或销毁不修改实体|
|`Town/TownEntity.cs`|管理三个 ActorEntity，统一更新与释放|
|`Home/HomeSystem.cs`|配置和主页面生命周期；等待 Town 就绪后隐藏 Launch；不再加载或卸载场景|
|`GamePlugin.cs`|先装配 Town 后装配 Home；提供允许模块缺席的 GetTown 查询|

`TownSystem.IsReady()` 是加载完成状态；OnActive 返回只表示请求被接受。`VisitBuilding(instanceId)` 和 `ReturnNpc(npcId)` 返回是否接受此次动作，拒绝不存在的 ID 和不合适的移动状态。`TownReadyChanged` 通知 Home 关闭或重新准备页面；Home 重新读取当前模块，避免使用旧实例的过期通知。`SetInputEnabled` 同时控制点击与相机手势。

首版配置绑定：旅店实例 ID `60017` 来自原 `tbbuilding` 提取记录；访客实例 ID `1` 与美术配置 `hero_20025` 分离。Route/速度/碰撞体属于本轮场景表现参数，通过 Inspector 序列化。没有把价格、升级条件或奖励硬编码在场景组件内。

Town 不拥有建筑等级、英雄属性、库存或存档，TownEntity 仅管理本场景角色集合。后续 BuildingEntity 持有建筑集合及状态，BuildingSystem 编排升级；Town 根据其快照刷新模型和交互，并在提交业务操作时重新校验条件。

## 生命周期

同实例的 generation 和 requestedActive 合并相互冲突的请求；跨实例门禁将旧实例卸载和新实例加载串行化。停用立即 Release 输入和移动，再异步卸载场景。Home 停用只关闭 UI、暂停输入，Town 保持独立生命周期。

框架 `Unload` 将模块删除放入下一帧队列。要替换模块，应等待 `GetTown()==null` 后再 `Load<TownSystem>()`，不能同一帧 Unload 后立即 Load 并假设得到新实例。日常进出使用同一模块的 Active/Detive 即可。

场景根节点只发现一个显式 TownScene 契约，内部引用全部序列化；不会全场景查找 UI 或运行时按名称补绑定。路点为人工明确配置的短距离街道路线，尚未接入 NavMesh、动态避障、多访客排队或随机漫游。

## 原作资源与复现

- `hero_20025`：原作 4.2.43 骨骼，使用真实 `walk`/`idle`；没有合成行走动画。
- `npc_hotel`、`npc_forgingtable_2`：原作 4.2.36 骨骼，恢复在原场景挂点，仅存在 `idle`。
- 原始 9 个骨骼、图集和贴图文件内容未变。PNG 资产文件名大小写匹配图集页名，否则 Spine 运行时无法找到纹理。
- [spine-imports.json](spine-imports.json) 记录原资源台账 ID、源/目标路径、用途与双端 SHA256；本地资源数据库新增 `town_imports` 表。远端资源数据库未同步。
- [spine-runtime.json](spine-runtime.json) 记录本机既有 `spine-unity-4.2-2024-07-16` Runtime 来源和文件哈希。只导入 Runtime，保留原许可头；没有改写第三方运行库。
- 运行库版本选择依据 [Spine 4.2 官方说明](https://raw.githubusercontent.com/EsotericSoftware/spine-runtimes/4.2/spine-unity/README.md)。

资产位于 `Assets/Data/Scenes/Town/Spine`，脚本位于 `Assets/Scripts/Systems/Town`。所有 Atlas/SkeletonData/Material 和场景引用由 `Assets/Editor/MiniwowTownSetup.cs` 使用 AssetDatabase、SerializedObject、EditorSceneManager 创建保存，没有手写 Unity YAML。

需要重新安装时，先保证资源、Runtime 和脚本已导入、Unity 编译通过，在非 Play 状态向 `tools/town/setup.request` 写入 `install`，检查 `install-result.json` 与 Console。只写空文件则生成 `inspection.json`，不重新安装场景。重建旧静态主城后，应再次执行 Town 安装以恢复相应绑定。

Studio 中 Home 的源码包包含 Town 集成源码与场景快照。它不是完整独立安装包：原生 Spine 资产和第三方 Runtime 仍随本地 Unity 工程版本管理，不能仅导出网页源码包后宣称可独立运行。

## 验证

- Unity 编译完成，运行 Console 0 error。
- `verify-town.ps1`：3套骨骼有效，屏幕内点击、拖动拒绝、重复请求拒绝、walk/到达/idle/返回、地图输入门禁、移动中停用、重进、快速重启、跨实例替换、1080×2400交互全部通过。记录在 `runtime-evidence.json`。
- Home 原回归：20次刷新无重复监听、数据快照隔离、非法资源拒绝、入口重排、地图往返、锁定/未接入提示通过。脚本在反射调用后等待渲染帧，避免异步显隐尚未应用就点击下一入口。
- GameCLI `verify code --system Town`：passed，0 issue；`--system Home`：passed，保留原有 FormatAmount 的“万”单位文案人工审查提示。
- 修复代码检查器 Console UTF-8 输入/输出编码；含中文和转义引号的大输入回归、现有规则检查与源范围检查共3项通过。
- 独立审查与实际操作结果见 [independent-review.md](independent-review.md)，独立证据 `independent-runtime.json`。

本轮没有实现建筑解锁/升级、服务消费、英雄成长、战斗、奖励和本地进度持久化，也不将本地交互入口解释为原作账号已经开放的功能。

## Actor 数据与表现重构

参考 `D:/Works/rts3x/developments/client/client/Assets/Scripts/Entitys/ActorEntity.cs` 和 `Fights/Shapes/{ActorShape,BaseShape}.cs`。参考工程由 Shape 回写位置，本项目进一步改为 Entity 权威位置，Shape 不回写数据；未修改参考工程。

GamePlugin.Update 调用 XEntityManager.Update，注册为帧更新的 TownEntity 推进全部角色。TownScene 仅将路点 Transform 复制为初始化数值，并绑定 Shape。访客、旅店店员、铁匠均有实体；Town 停用先解除显示绑定，再清理实体，使旧引用进入 Stopped。

编辑器测试入口：MiniwowTownProbe.VerifyEntity（无 GameObject 的往返、非法输入、重复初始化及集合原子性）；HideMovingShape 后等待逻辑 Waiting，再运行 VerifyShapeSeparation（隐藏期间移动、重绑、克隆销毁显示不重置实体）。普通交互和生命周期回归仍用 verify-town.ps1。

### ActorShape 状态机

ActorShape 继承 Oathx.Core.XActionStateMachine。每个实例通过 RegisterStates 注册 Idle、Moving、Waiting、Returning、Stopped；进入回调切换 Spine，LateUpdate 调用 UpdateMachine。Refresh 使用枚举值比较避免每帧重复进入状态。Unbind 清除表现状态但不修改实体；RegisterStates 为后续角色差异化扩展点。实体阶段目前决定表现状态，新增战斗等阶段时应同时定义实体规则与对应表现回调。

专项运行入口为 MiniwowTownProbe.VerifyActorMachine，需在 Town 就绪后调用。

### 场景配置

场景唯一配置源为 `config/propScene.xlsx`，沿用 development 的 Scene 表结构与 SCENE_APP/SCENE_HOME 别名，仅引入本产品两条场景。运行 `pwsh -File tools/town/export-scene-config.ps1` 调用既有 e2cs（development 中的工具只读），生成 Config.Excel 和 SceneConfig.bytes，不手改生成代码。

GamePlugin 在加载 Town/Home 模块前通过 XRes 读取表，校验必需条目后按框架范式注册到 Config.Excel。Shutdown 会使在途加载失效。TownSystem 通过别名查询 Path；缺表、缺条目、空路径明确失败，没有业务硬编码场景后备值。Unity Build Settings 的场景列表仍是构建清单，修改表路径时需同步对应资产和构建清单。
