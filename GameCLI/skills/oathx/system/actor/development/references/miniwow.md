# Miniwow Actor 实现参考

2026-11-25 整理，已提交基线 `582d0070`。从本文件向上六级为 GameCLI 根目录；以下路径均相对该根目录。使用时读取目标工程当前源码，历史基线不是版本锁。

- `examples/unitys/miniwow/client/Assets/Scripts/Systems/Actor/ActorEntity.cs`：ActorSpawn 输入、路线值复制、移动阶段与到达次数；实体不持有显示对象。
- `examples/unitys/miniwow/client/Assets/Scripts/Systems/Actor/ActorShape.cs`：类前 ActorAnimationNames、XActionStateMachine、RegisterStates、绑定/解绑与单向刷新。
- `examples/unitys/miniwow/client/Assets/Scripts/Systems/Town/TownEntity.cs`：按实例 ID 管理 ActorEntity 集合。
- 同目录 `TownSystem.cs`、`TownScene.cs`：宿主场景生命周期、配置查表、作者路点转值与序列化 Shape 绑定。
- `examples/unitys/miniwow/client/Assets/Scripts/GamePlugin.cs`：配置先于系统加载，框架实体更新入口。
- `examples/unitys/miniwow/client/Assets/Editor/MiniwowTownProbe.cs`：VerifyEntity、VerifyActorMachine、HideMovingShape、VerifyShapeSeparation；探针要求按说明准备就绪状态和初始计数，不能在任意场景状态直接套用。
- `examples/unitys/miniwow/tools/town/README.md`：运行步骤、资源台账与配置导出。

此产品使用三名角色、Spine、短作者路线和 Idle/Moving/Waiting/Returning/Stopped 五阶段。它没有任意寻路、战斗、建筑经营或角色存档；不得据此假设这些能力已经存在。其他产品按实际角色行为定义状态，Unity Animator 等方案可以保持相同数据隔离契约。

此前参照过 RTS 工程 ActorEntity / BaseShape / ActorShape 的分层。本规范采用 Entity 权威位置，不能因为旧参考的 Shape 会回写 Transform 而恢复双向数据所有权。使用本 Skill 不依赖访问用户电脑上的旧 RTS 路径。
