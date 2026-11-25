# Actor 数据与显示分离：独立审查

日期：2026-11-25（Asia/Shanghai）。审查者：`/root/town_review`，未参与本轮实现；未修改客户端源码或场景。

## 范围与结论

依据 miniwow 项目规范、共享开发规范和 system-development 的独立审查要求，检查 `ActorEntity`、`ActorShape`、`TownEntity`、`TownScene`、`TownSystem`、`GamePlugin.Update` 与编辑器迁移/探针代码，并读取用户指定参考工程中的 `Entitys/ActorEntity.cs`、`Fights/Shapes/ActorShape.cs`。

**静态职责与生命周期审查 passed；本次独立执行的有限运行检查 passed。** 未发现当前短路线 Town 流程中的新增阻断缺陷。完整 Town 回归及隐藏/替换 Shape 实验由主 agent 执行，不能算本审查者独立重跑。

## 静态复核

- `ActorEntity` 拥有 ID、配置 ID、位置、方向、速度、移动阶段和到达次数；不持有 GameObject、Transform、ActorShape 或 Spine 引用。
- 初始化先校验后赋值，路线数组复制；外部修改初始化数组不会修改内部路线。更新消耗本帧剩余距离，可跨过多个路点；重复出发、提前返回、非法 elapsed 均有拒绝分支。
- `ActorShape` 单向读取实体，通过 LateUpdate 更新 Transform 与动画；仅在动画名称变化时切换播放。Unbind/OnDestroy 不调用实体 Destroy 或修改实体位置。
- `TownEntity` 拥有私有 Actor 集合。重复 ID/无效定义时清理候选，不提交部分集合；Clear/Destroy 停止所有旧 Actor，再移除集合元素。
- `TownScene` 只持有序列化场景绑定，将 Transform 路线复制为值数据；`TownSystem` 负责创建/清理实体集合和场景绑定。原跨实例场景操作门禁保留。
- 当前 miniwow 客户端中 `XEntityManager.Update` 只有 GamePlugin 一处驱动；TownEntity 标记为每帧更新，其子 Actor 由集合统一更新，不再依赖 NPC MonoBehaviour.Update 推进逻辑。
- ActorShape 隐藏不停止逻辑是本轮明确的设计结果；Town 停用通过 Entity.Clear 结束逻辑，不依靠隐藏表现对象模拟业务生命周期。

## 数值边界修正

初审指出：单个坐标有限并不保证相邻路线差值及长度有限。修订后 `Initialize` 在提交任何字段前验证每段 `(next - previous).magnitude` 有限，拒绝由巨大有限坐标造成的 Infinity/NaN 路段。已静态确认修正。当前 VerifyEntity 未包含该极值用例，因此不将其描述为已独立运行验证。

## 运行证据的归属

### 本审查者独立执行

在主 agent 完成回归并交还 Editor 后，使用明确的 miniwow `--project` 路径调用现有 UnityCLI，原始输出保存于 `actor-independent-evidence.json`：

- `MiniwowTownProbe.VerifyEntity` 返回 True：脱离场景验证重复初始化拒绝、外部路线修改隔离、非法时间拒绝、跨路点推进、到达只计一次、小步长返回、销毁后停止、重复 ID 原子拒绝。
- `MiniwowTownProbe.Snapshot`：ready=true，场景与 Home 正常，1 个 TownScene、3 个 ActorEntity、3 个 ActorShape，访客 Idle；三个 SkeletonAnimation 均 valid 且 idle，顶点数分别 244、8、261。
- 未停止播放、未修改场景、未触发访问/返回，保持交还时的营地 Idle 状态。

### 主 agent 执行，本审查者复核代码和结果

- `actor-separation-evidence.json` 的 VerifyShapeSeparation 返回 True。对应探针先隐藏整个访客 GameObject，等待实体独立到达，再解绑、改变 Shape Transform、重绑、实例化替代 Shape 并销毁，断言实体位置/状态/到达次数不变，Actor 集合保持 3 个。
- 主 agent 报告完整 Town 回归通过；本轮未独立重跑全部点击、屏幕比例和场景重启测试，原始数据以更新后的 `runtime-evidence.json` 为准。
- `code-actor.json`、`code-town.json` 均 passed、0 issues；本审查已核对报告中的全部 5 个源码哈希与磁盘一致。

## 审查版本

| 文件 | SHA256 |
| --- | --- |
| ActorEntity.cs | `60CF7228E559E683B109204B3C781C9319E3B8F952B0D0DDF7AEBB7AEC7D3211` |
| ActorShape.cs | `7437143CDDF5C5709A4DF2CF1F909EED03439A223648AB4205F776217EE3DC05` |
| TownEntity.cs | `674CDEC627CB2369307A5A97D4B77C1A3C0D48619AB7ED72A427EA4754097F24` |

## 边界

本轮为内存实体与场景表现拆分；未实现存档、任意寻路、建筑经营、解锁升级或战斗数值。用户参考工程被用于职责对照，没有整套复制其战斗/导航/装备依赖。本报告不授权提交、推送或发布模板，也不沿用重构前 independent-review.md 的源码哈希作为本轮证明。
