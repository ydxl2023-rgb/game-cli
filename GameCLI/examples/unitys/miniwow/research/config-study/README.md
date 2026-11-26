# 小骑士冲冲冲：原作配置数据分析

分析日期：2026-11-26。对象是本地已提取的客户端配置版本，不代表原作最新线上数据。此次只研究、解码、建索引，未修改 Unity 工程，未连接原作服务端。

## 结论和证据范围

原作不是把玩法全部塞进一张总表，而是分为**内容定义、数值成长、触发与效果、表现播放、开放与运营**几层。最值得学习的是这些层之间用稳定标识关联；不能把表里的 ID、效果类型或客户端播放代码直接当作完整服务器规则。

本轮完成：

|项目|结果|
|---|---:|
|ConfigHead 中表数|166|
|成功解码表数|166|
|总记录数|109,500|
|原 Configs.bytes 大小|11,900,446 字节|
|生成代码中的类型|766，其中枚举138|
|明确列出的引用检查次数|39,921|
|待解释引用记录|3,422，分为3类，不等于3,422个原作缺陷|
|直接表名消费代码位置|1,260，词法引用不等于运行覆盖|

每张表通过读取器完整消耗对应切片；166 个已提取 `.bytes` 与原包切片逐字节一致，切片连续覆盖整个 Configs.bytes。原 `Tables` 构造和 `ResolveRef` 可以执行完成。另与此前独立 Python 建筑解析结果比对21条建筑的名字和模型路径，一致。没有取得策划 Excel、同版服务端源码或运行时战斗样本。

读取流程由 [StartGame.InitConfig](../code-study/managed/decompiled/HotUpdate/Zombie/StartGame.cs) 证明：ConfigHead 的名字/偏移/长度 → Configs 切片 → Luban ByteBuf → [Tables](../code-study/managed/decompiled/HotUpdate/Zombie.Table/Tables.cs) → 各 Tb/Cfg 类型。研究工具编译这些生成读取器，补最小 Luban 读取适配；这不是恢复了原版 Luban 实现，也不是完整游戏可重编译的证明。

## 如何查数据

- [进入游戏与新手体验](PLAYER-JOURNEY.md)：登录分支、救火救人、第一章18步、任务领奖与引导机制，区分静态证据和设计推断。
- [完整章节体验索引](CHAPTER-EXPERIENCE.md)：114章、754个任务位置及第一章导航分支；由 `tools/config-study/index_experience.py` 生成，机器数据与来源哈希见 `experience-index.json`。
- [全表目录](CATALOG.md)：166张表、分类、行数、主键和代码引用数量。
- [字段字典](FIELDS.md)：全部顶层字段类型，包含 `_Ref` 生成引用字段和消费代码样本。
- [结构与枚举](schema.json)：嵌套Bean、继承关系、枚举数值和名称；多态效果查看这里。
- [解码清单](inventory.json)：每表偏移、长度、行数、哈希及读取消耗长度。
- [输入来源哈希](provenance.json)：766份生成 C#、原配置头/包、HotUpdate DLL，共769份输入。
- [统计](summary.json)、[待解释引用](unresolved-references.json)；完整关系位于本地 references.json。
- `tables/<表名>.json`：完整原始字段值。枚举同时保存数值/名称，多态Bean保留 `$type`，不递归展开 `_Ref`。
- `configs.sqlite`：本地可重建查询库，不修改资源台账 PostgreSQL。

在 miniwow 根目录执行：

```powershell
python -X utf8 tools/config-study/query_config.py --table tbheroskill --id 1250101
python -X utf8 tools/config-study/query_config.py --table tbbuilding --search 旅店
python -X utf8 tools/config-study/query_config.py --table tbfeatureunlock --limit 10
```

数据库表为 config_tables、config_rows、config_fields、config_references、config_consumers。例如：

```sql
SELECT table_name, row_key, label FROM config_rows
WHERE table_name = 'tbhero';
SELECT * FROM config_references
WHERE source = 'tbheroskill' AND source_key = '1250101';
SELECT source, line, code FROM config_consumers
WHERE table_name = 'tbherolevel';
```

完整 JSON 约230MB，含格式化空白；已作为可重建本地产物忽略，不默认上传或提交。复现步骤见 [工具说明](../../tools/config-study/README.md)。

## 一、战斗与探索配置

|表|条数|从结构和消费代码确认的作用|
|---|---:|---|
|tbbattlestage|4|大陆/大章节，UnlockArea 引用区域|
|tbbattlearea|20|区域名、等级范围、推荐战力、任务、探索进度奖励、Boss与开放条件|
|tbbattle|1,435|战斗地点、怪物组、波次规则、奖励/预览、消耗和推荐战力|
|tbmonstergroup|3,863|怪物ID和权重项，供遭遇内容组织|
|tbmonster|15,017|怪物身份、属性范围及模型等定义，不能等同15,017种独立美术|
|tbdungeon / tbdungeonchapter / tbdungeonheros|1,410 / 5 / 7,050|另一套副本层数、章节、敌方英雄内容；不能混同普通探索表|
|tbadventureevent / tbadventurerandomevent|80 / 223|探索事件与随机事件内容|
|tbadventurerandomeventlevel / tbadventureyield|193 / 21|事件等级参数和探索产出相关定义|
|tbrankpvprobot / tbrankpvprobothero|1,053 / 1,914|竞技场机器人及英雄配置|
|tbbattletask|0|本版本空表；有类名不代表玩法启用|

一条已验证的组织链：

```text
tbbattlestage(1，东部大陆)
  → UnlockArea [1..5]
  → tbbattlearea(1，新生平原)

tbbattle(9001，场景音山矿洞)
  → NormalMonsterGroup [90001]
  → tbmonstergroup(90001).Monster [{Id:90001, Num:100}]
  → tbmonster(90001，1级野猪)
```

9001 的 WaveRule 为第1波、Normal、数量范围1–1；还有 EliteMonsterGroup、BossMonster、BattleDrop、RewardPreview 等独立字段。**预览奖励不是权威掉落结果**，权重100也不能脱离候选集合直接理解为百分之百。

`tbglobal.MaxRound=15`，客户端 Boss 页面使用它显示剩余回合；这证明有配置上限及显示消费，不足以证明所有玩法服务端都采用相同回合上限。时间字段也不能一概按秒：`tbglobal.ProductInterval=5000` 的单位需要追消费端，不能只看数值命名转换。

战斗结算边界由 [BattleUtility.BattleMetaToData](../code-study/managed/decompiled/HotUpdate/Battle/BattleUtility.cs) 的225行起确认：它读取 battleMeta.win、fights、前后角色快照、rounds，转为表现队列。客户端的 [BattleCharacterEntity](../code-study/managed/decompiled/HotUpdate/Battle/BattleCharacterEntity.cs) 根据 skill PlayId、HitTimes 等安排动画和命中表现。**恢复配置与播放流程，不等于恢复服务器伤害、选敌、随机数、结算及反作弊规则。**

设计启发：战斗输入配置、权威战斗结果和显示时间轴分离。未来本地 DEMO 可以自行实现结算器并生成事件序列，但其公式必须标为产品设计，不能称作原作公式。

## 二、技能、效果、Buff与表现

|表|条数|职责|
|---|---:|---|
|tbheroskill|1,222|技能等级、下一级ID、职业、主动/被动、CD、触发、目标、效果列表、播放ID|
|tbheroskilleffect|1,447|效果类型与多态参数；不是一列伤害系数能概括|
|tbheroskillbuff|1,800|Buff/Debuff、触发/结束条件、互斥、叠层、驱散、效果、显示/播放关联|
|tbskillplay|339|动画、粒子、音效、命中时刻、隐藏模型、弹道参数|
|tbbuffplay / tbbuffshow|19 / 58|Buff表现与展示信息|

配置中技能 Active=281、Passive=941；这是**记录数**，包含等级变体，不是独立技能数。Buff=1,735、Debuff=65，同样不能直接当作独立可见状态数量。

`CfgHeroSkill` 的逻辑字段：NextLevelId、CD、InitCD、Order、AddAct、Trigger、Target、Effects。`PlayId` 单独连到表现。效果参数由 [BaseEffect.DeserializeBaseEffect](../code-study/managed/decompiled/HotUpdate/Zombie.Table.skill.effect/BaseEffect.cs) 按类型标识构造，存在 EffectPercentValue、EffectBuffValue、EffectBuffAttrValue 等24个分支。研究导出保留类型，不能将多态参数拍平成一个不明用途的 Value。

**具体例子：石盾加伦（Hero 125）的“沉重打击”**

|配置位置|实际值及意义|
|---|---|
|tbhero[125].RecommendedSkills|1250101、1250102、1250103|
|tbheroskill[1250101]|Active，Level=1，CD=3，InitCD=0，NextLevelId=1250201|
|Trigger / Target|OnCast；RandomEnemy，数量1|
|Effects|125010101、125010102，顺序保留|
|Effect 125010101|SelfApplyState，参数为 Buff 125010101，Rate=10000，Statck=1|
|Effect 125010102|ImmediateDamageBase，EffectPercentValue.Value=10000|
|Buff 125010101|OnAdd 触发，DynamicStackBonus，引用 BuffId=10006，SkillActDamageIncreased，Value=800|
|文本交叉验证|对随机敌人造成攻击100%伤害，每层“坚固”提高8%，冷却3回合|
|PlayId|2002501，链接独立表现表|

在这个例子中，10000与100%、800与8%的对应有文案与类型双重证据；**不能据此把所有 Num、Value、Rate 字段都统一除以10000**。互斥优先级、触发先后和结束语义还需要规则执行端验证。尤其描述文本和配置可能存在历史差异，不能让文案代替数值字段。

当前效果记录中 ApplyState=1,102，ImmediateDamageBase=130，其余包括治疗、护盾、移除叠层和斩杀等；大量效果通过状态实现，后续技能设计应优先明确效果与状态职责，而不是给每个技能写独立 System。

表现示例 tbskillplay[20001]：attack 动画时长0.533，粒子延迟0.15，HitTimes=[0.3]。BattleCharacterEntity 按动作 effectDataList 及命中时刻播放，不能把动画时间当战斗规则 CD。

## 三、英雄成长、属性和外围养成

|层|配置与数量|要点|
|---|---|---|
|英雄定义|tbhero 57|职业、品质、初始星、等级组/星级组、推荐技能、模型与碎片|
|等级成长|tbherolevel 3,420|57组×60级；属性、经验阈值、兵力上限、建筑字段|
|星级成长|tbherostar 96|4组×24星；碎片数与加成属性|
|技能随星级变化|tbherostarmaptoskilllist 57|每英雄的星级→完整技能列表|
|属性映射|tbattribute 943、tbtotalattribute 138|配置属性ID→属性枚举→展示/附加属性类型|
|战力系数|tbattributepower 35|系数字段存在不代表已恢复完整战力算法|
|装备|tbheroequip 196、tbequipmentenhancelevel 259|配方、品质、职业、属性、强化消耗等|
|词缀|tbequipmentaffixlib 1,125、tbequipmentaffixpack 3,000|属性候选与组合资源|
|遗物|tbrelic 46、tbreliclevelup 1,426、tbrelicstarup 276|独立等级、升星、回收与条件|
|书籍|tbbook 42、tbbooklevel 20,160、tbbookstar 1,302|大规模等级曲线与天赋相关配置|
|宠物|tbpet 17、tbpetlv 720、tbpetstar 303|另一路成长与升级资源|

全部57个英雄记录的 MaxLevel=60、MaxStar=24；本版本四种品质为 Rare、Epic、Legendary、Star。不要仅按等级表行数估算角色数量。

### 等级与星级索引

[HeroData](../code-study/managed/decompiled/HotUpdate/Zombie/HeroData.cs) 第92/94行明确计算：

```text
LevelCfgId = Hero.LevelGroup × 1000 + 当前等级
StarCfgId  = Hero.StarGroup  × 1000 + 当前星级
```

石盾加伦 LevelGroup=125、StarGroup=1。因此等级30使用125030，星级12使用1012。当前57名英雄的所有声明等级/星级范围都能在对应表找到记录。

### CostValue 是累计经验阈值

HeroData.GetLevelExp（243行起）计算“本级经验需求 = 当前 CostValue − 上一级 CostValue”，“当前进度 = Exp − 上一级 CostValue”。所以不能把各行 CostValue 逐级累加当总养成成本。

|加伦等级|CostValue|由相邻阈值计算的本级区间|
|---:|---:|---:|
|1|800|800|
|2|1,700|900|
|10|12,600|1,800|
|20|44,600|5,000|
|30|94,600|5,000|
|59|239,600|5,000|
|60|0|满级边界，不能按普通相邻差得到负经验需求|

本例 CostID=70005。表末值0不意味着可免费继续升级；应结合 MaxLevel 及消费端处理。原代码存在不适合直接照抄的边界写法，迁移时需要自己的满级测试。

### 属性不是直接使用全部原始值

属性配置4100→Base_damage（攻击）、4107→Armor、4101→Health；4112/4113/4114分别映射攻击/生命/护甲加成。HeroData.GetAttribute（167行起）根据 TotalAttribute.Addition 应用 `base × (1 + addition / 10000)`，然后转 long。这只是查到的客户端取值逻辑。

|等级|基础攻击4100|基础护甲4107|基础生命4101|
|---:|---:|---:|---:|
|1|60|48|576|
|30|118|106|1,041|
|60|227|210|1,904|

星级组1在12星的三项加成均8400，24星均18000。这不能直接计算完整角色最终面板：HeroData.SetData 从服务端 HeroMeta.attr 取得属性，装备、遗物、书籍等有独立来源。tbhero.MaxAttribute 又用于图鉴展示，不可直接当作1级属性。

### 技能不是仅通过等级解锁

加伦的星级技能映射：1星只有1250101；2星增加1250102；3星增加1250103；4星第二技能变1250202；7星第一技能变1250201；10星第三技能变1250203；22星第一技能变1250401。中间多星会保持相同技能列表。

[HeroProxy.GetSkillUnlockStar](../code-study/managed/decompiled/HotUpdate/Zombie/HeroProxy.cs) 第425行起扫描映射寻找最早出现星级。因此 **星级列表决定拥有的技能版本，NextLevelId 描述技能链**，两者不能互相替代。

## 四、系统开放、建筑、经济和运营

|配置组|主要表及数量|职责|
|---|---|---|
|系统开放|tbfeatureunlock 80、tbplaymode 5|功能标识、条件描述、开放引导、玩法入口|
|主界面入口|tbmainfunctions 8、tbmainactivity 20|入口名字、图标、功能门槛；不是 GameCLI 的 FunctionSystem 装配表|
|建筑|tbbuilding 21、tbbuildinglevel 489|建筑身份/表现/交互关联；每级模型、消耗、时间、条件和效果|
|物品及生产|tbitem 5,100、tbcrafting 4、tbidleproduction 90|道具定义、部分合成配方、生产相关参数；合成表不等于全部制造玩法|
|任务|tbtask 1,397、tbchaptertask 114、tbprogresstask 157|任务目标、章节推进与进度奖励|
|引导|tbguide 659、tbguidetask 1,594、tbguidetrigger 15|触发、步骤与UI/场景引导|
|运营|tbactivity 37、tbactivitybase 39、tbpay 195|活动内容、基础定义和支付配置；记录存在不等于当前在线可用|
|全局|tbglobal 1|混合了启动资源、展示顺序、经济常量与部分战斗参数，不能原封搬成新工程的巨型管理器|

建筑使用 [BuildingData.LevelId](../code-study/managed/decompiled/HotUpdate/Zombie/BuildingData.cs) 的 `建筑ID×1000+等级`，与英雄的分组索引相似，但生命周期不同。tbbuildinglevel[60001001] 是酒馆1级，模型 Tavern/Tavern，BuildingUpCost 含金币70001×900，BuildingUpNeedTime=0。实际升级应检查消费代码取当前级还是下一等级条目，不能仅凭名称认为此成本一定属于“从1升2”。

当前建筑定义包括酒馆、铁匠铺、炼金商店、号角、灵魂圣所、温泉、副本入口、钱堆、集结、告示牌、订单点、领地、祝福、探险、裁缝店、旅店、医院、交易行、征税点等。不同建筑包含不同 BaseEffect/Effect 类型；数据实体可以统一集合管理，行为仍按效果契约分派，不能为每座建筑另造一个玩法 System。

**开放条件与当前开放状态分开。** [PlayerProxy.IsFeatureUnlock](../code-study/managed/decompiled/HotUpdate/Zombie/PlayerProxy.cs) 第344行查询 featureUnlock 集合；OnFeatureUnlockNotice 第234行起接收服务端开放ID并广播变化。配置中的 UnlockNeedCondition 还用于提示文案。因此客户端有条件表，不等于所有开放条件由客户端独立权威判定。做离线 DEMO 时需要我们实现条件求值和状态持久化，并通过事件通知 Home。

tbfeatureunlock[1002 Box] 的条件是 ChapterTask=60101。它表达任务进度驱动解锁，而不是所有系统都“玩家等级达到某数”。建筑的 Feature.None 与有门槛的 Feature 也不同。

原作 MainFunctions/Feature 的业务目的地和 GameCLI FunctionSystem 的 `ClassType/InstallOnStartup` 装配职责不同；迁移时分别建入口配置、开放条件配置和框架系统配置，禁止按表名相似直接合并。

## 五、引用核对及待解释问题

本轮检查15种明确或标为候选的关系，共39,921次。英雄推荐技能、技能效果/下一级/播放、英雄普攻播放、等级/星级完整范围、星级技能映射、章节区域、普通/精英怪物组及组内怪物等关系在本版检查中均可找到目标。

发现以下3类待解释项：

|来源|目标|记录数|当前判断|
|---|---|---:|---|
|tbmainfunctions[8].Feature|tbfeatureunlock[1055]|1|未找到解锁表行；可能由服务器状态/平台或其他流程控制，不能直接判为坏配置|
|tbbuilding[60013领地].Feature|tbfeatureunlock[1027]|1|同上；仍需追查对应功能消费及版本|
|全部 tbherolevel.NeedBuilding.Type|tbbuilding[60023]|3,420|候选建筑关联不成立；当前建筑表止于60021，该字段在所查客户端目录未找到直接消费，不能直接据此给新工程补造建筑|

`ResolveRef` 完成并不会抛出所有缺引用，因为很多生成代码用 GetOrDefault。以上检查也不是所有嵌套和动态引用的完整证明；详细关系证据在 SQLite 和 references.json。空表、旧字段、平台入口、服务端控制、不同版本残留都应分别判断。

## 六、结合 GameCLI 的学习与实现建议

以下为新产品设计建议，不声称是原作的内部架构：

1. **先做小闭环数据集。** 从原作案例选一组角色、1场战斗、3个技能及相关Buff/表现、少量等级、1座恢复建筑；先把依赖展开，避免直接导入109,500条数据。
2. **分开规则与显示。** ActorEntity 保存权威数据；ActorShape 只表现。战斗配置与 SkillPlay 表分别处理，技能不注册为 System。
3. **战斗运行交给 Runtime。** 后续 FightRuntime 走已有 GameRuntime/RuntimeManager；玩法 System 通过事件协调，不直接引用 HomeSystem。
4. **稳定ID和枚举边界。** 表间关系用稳定ID；固定效果/目标/条件用枚举或明确类型。原作多态结构不能退化成散落字符串判断。
5. **成长做独立规则。** 明确累计经验阈值、满级终止、星级碎片、技能版本替换、属性加成来源，先验证再通知表现。
6. **建筑和功能分开。** BuildingEntity 负责集合状态及升级事务；条件求值负责开放规则；Home 只消费结果，避免重新引入 TownSystem 或 MasterPhase。
7. **继续沿用产品语言表与配置链。** 原表是研究输入，不覆盖 development，不绕过本产品 Excel→生成代码→运行加载流程。

建议下一步优先明确本地战斗的规则范围，再建立首个闭环配置设计。恢复缺失服务端公式、完整经济平衡和全系统迁移不属于本次已完成内容。
