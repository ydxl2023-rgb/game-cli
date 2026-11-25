# ActorShape 状态机独立复核

日期：2026-11-25。审查者：独立子 agent town_review。

结论：本轮静态审查与有限运行验收通过，未发现阻断缺陷。未修改业务代码或框架。

## 静态检查

- ActorShape 继承 XActionStateMachine。基类 stateList/stateMap 是实例字段，各 Shape 持有自己的状态机。
- ActorEntity.phase 保持业务权威；Shape 将逻辑状态映射为 Idle、Moving、Waiting、Returning、Stopped 的表现状态。解绑或销毁 Shape 不修改实体。
- 使用 Enum.Equals 判断当前状态，避免框架 Enum 引用比较导致重复 SetState；同状态刷新不会重新设置 Spine TrackEntry。
- XActionStateMachine 无自动 Unity Update 驱动，ActorShape.LateUpdate 调用 UpdateMachine 不造成双重更新。
- ClearOrBlockState(null) 清除活动状态；Unbind 清理动画轨道。Stopped 的进入回调停止动画。
- RegisterStates 可供派生表现类扩展；派生实现应保留所需基础 ActorPhase 注册。当前无相关派生类。

## 独立运行

以明确的 miniwow client 绝对路径调用 tools/home-ui/cli/UnityCLI.exe，执行 MiniwowTownProbe.VerifyActorMachine。

返回：ok=true，returnSummary=True，respondedAtUtc=2026-11-25T03:10:00.0901261Z。

探针临时克隆 Shape，并创建独立 ActorEntity，验证五种状态、同状态刷新不重启动画、解绑不改实体、重绑恢复当前状态及销毁实体后清动画；finally 清除临时对象。没有操作正式角色的路线或状态，没有停止播放或修改场景资产。

第一次受限账号连接被 named-pipe allowlist 拒绝；通过授权执行环境重新调用成功，没有修改管道账号配置。

## 主 agent 证据与边界

已读取 actor-machine-evidence.json 与 actor-separation-evidence.json，分别记录主 agent 的 VerifyActorMachine=True、VerifyShapeSeparation=True。完整 Town 回归由主 agent 执行并报告 PASS，本审查者未重复完整回归。

本轮独立运行限于上述克隆探针；没有独立测试派生状态类的 Update 回调，也未增加两个实例互不影响的显式运行断言。实例字段隔离经静态确认。

审查版本：Assets/Scripts/Systems/Actor/ActorShape.cs，SHA256 `1C3F182D8BDBC2991C29353192FEA3B3D41DC089738E4D0692525265061D91C3`。

此前 actor-review.md 的 ActorShape 哈希对应继承状态机之前的版本；本文件记录本轮增量复核，不替代其数据/表现分离分析。
