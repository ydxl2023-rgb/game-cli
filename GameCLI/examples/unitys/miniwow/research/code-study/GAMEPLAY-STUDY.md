# 小骑士冲冲冲：从客户端代码学习玩法与设计

研究日期：2026-11-24。本文依据本地 HotUpdate.dll 的 ILSpy 反编译输出与既有语言表，不代表取得原开发工程，也没有运行原作或观察服务器。代码位置统一相对于 `managed/decompiled/HotUpdate/`，行号指本次导出，重新反编译后可能变化。包含 Unknown result type 的注释是缺引用造成的反编译提示，不是原作者注释。

## 1. 首先理解：画面正在做什么，数据是谁决定的

最重要的发现是，本次检查的据点/副本战斗链路消费服务器返回的战报，再播放动作。不能从飘字代码推断伤害公式。

证据链：

- `Zombie/DungeonProxy.cs:167` 遍历应答中的 `battleMeta`，170 行调用 `BattleUtility.BattleMetaToData`；392、407 行也从应答构造副本战斗数据。
- `Battle/BattleUtility.cs:225` 的 `BattleMetaToData` 将协议转换为播放数据；298 行建立动作队列，309 行 `ActionMetaToData` 转换每个动作。
- `Dungeon/GroupBattleController.cs:79` 取出一次战斗，127 行取回合；`ToNextAction` 从动作队列取数据，播放结束回调再推进，队列耗尽调用 `ToNextRound`。
- `Battle/BattleActionData.cs:6` 同时携带选择目标、动作前状态、效果列表、动作后状态。

**可以确认**：这条客户端链路是“网络战报 → 战斗/回合/动作队列 → 动画与状态表现”。它支持回合与动作的结构化播放。**不能确认**：服务器如何排行动序、随机数如何产生、伤害公式、所有战斗模式是否共用一个服务端模拟器。不要把客户端视觉连续播放直接叫作实时战斗，也不要把有 round 字段推导成玩家每回合手动下指令。

适合学习的技巧：把计算结果与播放结果分开。练习时可以先写独立的离线规则计算器，产生动作记录，再让一个播放器消费这些记录。同一份记录应该可以慢放、加速、跳过动画而得到相同最终状态。这是我们自己的练习设计，不是声称原作拥有全部这些功能。

## 2. 探索用状态与时间点组织，而不是一堆 Update 条件

`ExploreSystem.cs:12` 定义 Moving、Execute、Leave 三种探索表现状态。

1. `ExploreHeroMovingState.OnEnter`（43 行附近）重置曲线采样、生成移动路径并发送开始探索/移动通知。
2. `OnUpdate` 沿曲线路径推进，完成后 `OnMoveFinish`（78 行附近）把当前事件切换成 NextExploreHeroEventState，再发送执行事件通知。
3. `ExploreHeroExecuteState.OnEnter`（99 行附近）按采药、钓鱼、采矿、狩猎查询 `GlobalTable.Data.AdventureEventExecuteTime`。
4. 它计算 `NextEventTime = duration + Utility.CurrentTime()`，注册定时器；时间到，先 `SettleDelayModify()`，再 `LevelProxy.RequestHeroArrive(heroId)`。
5. `OnExit` 停止该状态的定时器，通知表现结束。

`Zombie/LevelProxy.cs:336` 的 RequestHeroArrive 实际发送 HeroArriveRequest。`OnAdventureHuntingNotice`（355 行附近）接收战斗结果、血量、行动力、经验与等级，把修改放进 `delayModify`；其余表现随后播放。

这里的技巧是“数据到达”和“玩家看到结果”分离：网络可以先带来结果，画面在合适时刻应用，避免英雄还没攻击血条就跳变。但延期表现不能被当成新的权威结算。离线练习可把它分为 SimulationState 与 DisplayState，并明确何时同步。

退出状态清理定时器值得优先学。练习验收：英雄在采集中途离开后，旧定时回调不能再次发出到达或奖励事件。

## 3. 旅店是容量、库存、购买能力共同约束的服务队列

`Zombie/BuildingHeroStateRestQueue.cs:23` 的 OnEnter 依次检查建筑存在、是否解锁、当前人数是否达到最大人数、库存和价格条件。满足条件才结束 RestQueue 并添加 Rest；否则显示等待原因。它不是“缺行动力立即加满”的简单按钮。

`Zombie/BuildingHeroStateRest.cs:25` 的 OnEnter 增加店内人数，移动到休息点后发起消费。部分分组分支直接请求消费，其他分支重新检查库存/价格；检查失败减少人数并返回 GotoRest。需要注意，这种二次检查用于处理排队后条件变化，不能把第一次判断视为永远有效。

`Zombie/ConsumableProxy.cs:199` 的 StartUseConsumable 先检查同一英雄是否已在 Shops 中，随后发 HeroCostUseRequest。应答处理更新英雄状态、记录服务器 startTime/nextTime，再以 EndTime 安排完成请求。这里本地防重复依赖已登记的 Shops，不能仅凭这一段保证网络请求在途期间绝不重复；若自己实现，应补在途状态或事务幂等。

可学习设计：把“去店里”“排队”“使用服务”拆成不同状态，让移动、等待和消费分别有进入与退出动作；容量属于店，需求属于英雄，商品库存属于经济数据，避免用一个巨大角色类管理全部事情。

## 4. 制造与补给的客户端边界

`Zombie/ConsumableProxy.cs:184` 的 CraftConsumable 发出 HeroCostCraftIRequest，携带 buildingId、itemId；应答成功后触发回调与 CraftConsumable 通知。`RequestGetAutoCraft`（287 行）与 `RequestSetAutoCraft`（300 行）同样通过网络请求管理自动制造。

`Zombie/PlayerProxy.cs:407` 的 RequestReceiveSupply 用 buy 区分领取方式，成功应答更新免费/付费次数。139、290 行附近也从登录/通知数据同步补给次数。

这支持旧文档中的“探索和生产/补给互相影响”的方向，但制造耗材扣除、补给恢复速度、离线补偿公式不能靠这些请求方法恢复。配置字段与语言说明给出了规则声明，具体服务端校验仍未取得。

练习时把制造做成一项领域事务：校验配方和建筑 → 校验材料与容量 → 一次性扣材料并产生商品 → 保存 → 发变更事件。不要让 UI 自己扣材料。这里是独立设计建议，不是原作服务端代码。

## 5. 技能与 Buff：逻辑效果和视听效果分层

`Battle/BattleCharacterEntity.cs:368` 的 BeHit 判断 blocked、evaded、missed 等战报标记；正常伤害展示值取 Health 与 Shield_Value 的 changeValue，随后调用 Injured。治疗取 Health 的 changeValue，最终调用 BeAffected 应用状态。这一段没有展示攻击减护甲之类的通用伤害计算公式。

`Battle/BattleBuff.cs:20` 的 Init 先由 buffId 取 CfgHeroSkillBuff，再用 PlayId 取 CfgBuffPlay；OnAwake 按播放配置延迟挂载，Trigger 播放动画，OnDestroy 清理循环粒子。角色侧 466 行附近按 BuffActionType.Add/Remove/Trigger 处理战报动作。

因此不能把 BattleBuff 当作完整的 Buff 规则引擎。它展示了非常有价值的分层：技能/Buff 的业务身份与它的粒子、挂点、偏移和播放延迟分开配置。同一个逻辑可以换皮肤，同一种表现也可以复用。

练习时先实现“效果数据 + 状态变更”，再接视觉。建议用一个持续伤害 Buff 验证：每个逻辑 tick 只产生一次伤害记录；暂停/加速画面不应改变总伤害；移除 Buff 后不应遗留循环粒子或计时器。

## 6. 对原机制文档的修订

`../MECHANICS.md` 主要基于语言表。现在可补充三项代码证据：探索执行时长由事件配置驱动并发送到达请求；恢复服务有容量/库存/价格与排队状态；所查据点/副本战斗消费服务端战报并按回合、动作播放。

旧文档所述每区域 6 名英雄、16 小时离线金币上限、招募刷新次数等仍是文本规则证据，本次未逐项核对对应配置数值及服务端执行。尤其“每区域 6 人”不能当作所有战斗上阵人数。原文关于战斗公式未验证的边界继续有效。

## 7. 学生阅读顺序与练习路线

1. 先读 ExploreSystem 的三个状态，画出事件触发和退出清理，再实现最小计时状态机。
2. 读 RestQueue、Rest、ConsumableProxy，把玩家看到的排队故事还原为容量和库存约束。
3. 读 BattleData、BattleActionData、BattleUtility 的协议转换，再读 GroupBattleController，理解一个纯播放器如何工作。
4. 最后读 BattleCharacterEntity 与 BattleBuff，辨别状态值、规则结果和动画资源，避免一开始陷进 Unity/Tween 大量调用。

每阶段写一页记录：输入是什么、谁拥有状态、何时修改、失败怎样处理、退出如何清理。比照抄完整工程更能提高设计能力。后续映射到 GameCLI 时采用 System/Entity/VM/UI 职责分工：领域状态、规则、校验与事务进 Entity，System 编排流程和通知，Widget 接收刷新并维护 VM；不要将原作 Proxy 单例或 FairyGUI 代码机械搬入框架。

## 尚未解决

服务端源码、权威伤害与掉落算法、全部离线结算公式、完整网络容错以及运行时表现一致性均未取得/未验证。本篇是对指定客户端链路的静态学习报告，并非全产品行为认证。干净 Unity 工程未被本研究修改。

