# 原作配置字段字典

自动反射原作生成读取器；字段名保留原名。业务释义与案例见 README.md，不能以字段英文猜测单位。

## tbactivepassmain

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbActivePassMain.cs`；行类型：`Zombie.Table.CfgActivePassMain`；条目：4。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Stage|`System.Int32`|False|
|MidPayId|`System.Int32`|False|
|MidPayId_Ref|`Zombie.Table.CfgPay`|True|
|HighPayId|`System.Int32`|False|
|HighPayId_Ref|`Zombie.Table.CfgPay`|True|
|BannerIcon|`System.String`|False|
|PurchaseBtnSFX|`System.String`|False|
|RewardSFX|`System.String`|False|
|PriceBtnSFX|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ActivityProxy.cs:44`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ActivityProxy.cs:56`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ActivityProxy.cs:59`

## tbactivepassstage

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbActivePassStage.cs`；行类型：`Zombie.Table.CfgActivePassStage`；条目：60。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Stage|`System.Int32`|False|
|Score|`System.Int32`|False|
|FreeReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|MidReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|HighReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ActivityProxy.cs:70`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFundTaskPassView.cs:257`

## tbactivity

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbActivity.cs`；行类型：`Zombie.Table.CfgActivity`；条目：37。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`Zombie.Table.activity.ActivityId`|False|
|Name|`System.String`|False|
|Desc|`System.String`|False|
|Icon|`System.String`|False|
|Position|`Zombie.Table.activity.ActivityPos`|False|
|ShowParam|`Zombie.Table.activity.ShowParam`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ActivityProxy.cs:195`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ActivityProxy.cs:215`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ActivityProxy.cs:662`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ActivityProxy.cs:682`

## tbactivitybase

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbActivityBase.cs`；行类型：`Zombie.Table.CfgActivityBase`；条目：39。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|ActivityType|`Zombie.Table.activity.ActivityId`|False|
|MainId|`Zombie.Table.activity.MainId`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ActivityProxy.cs:53`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ChargeContinueCom.cs:288`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIGiftActivityPopupView.cs:62`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIWXSubscriptionEntryView.cs:74`

## tbadventureevent

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbAdventureEvent.cs`；行类型：`Zombie.Table.CfgAdventureEvent`；条目：80。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|AreaId|`System.Int32`|False|
|TypeEffect|`Zombie.Table.adventure.Effect`|False|
|Weight|`System.Int32`|False|
|EventType|`Zombie.Table.adventure.EventType`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIExploreEventCom.cs:733`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIExploreEventCom.cs:789`

## tbadventurepoint

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbAdventurePoint.cs`；行类型：`Zombie.Table.CfgAdventurePoint`；条目：1。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|AreaId|`System.Int32`|False|
|Name|`System.String`|False|
|Weight|`System.Int32`|False|
|ChildPoint|`System.Collections.Generic.List`1[System.Int32]`|False|
|RewardPreview|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Display|`System.Boolean`|False|
|Level|`System.Nullable`1[System.Int32]`|False|
|EventType|`Zombie.Table.adventure.EventType`|False|
|TypeEffect|`Zombie.Table.adventure.Effect`|False|
|BigIcon|`System.String`|False|
|SmallIcon|`System.String`|False|
|Title|`System.String`|False|
|Desc|`System.String`|False|
|BarTxt|`System.String`|False|

代码消费样本：

## tbadventurerandomevent

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbAdventureRandomEvent.cs`；行类型：`Zombie.Table.CfgAdventureRandomEvent`；条目：223。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|AreaId|`System.Int32`|False|
|EventType|`Zombie.Table.adventure.RandomEventType`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|TypeEffect|`Zombie.Table.adventure.RandomEventEffect`|False|
|Icon|`System.String`|False|
|Quality|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBattleSearchView.cs:159`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIExploreMapView.cs:846`

## tbadventurerandomeventlevel

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbAdventureRandomEventLevel.cs`；行类型：`Zombie.Table.CfgAdventureRandomEventLevel`；条目：193。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|AreaId|`System.Int32`|False|
|Level|`System.Int32`|False|
|UpgradeNeed|`Zombie.Table.item.IdValue`|False|
|Weight|`System.Collections.Generic.List`1[Zombie.Table.NormalWeight]`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIExploreMapView.cs:424`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIExploreMapView.cs:622`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIExploreUpgradeLevelTipsView.cs:44`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIExploreUpgradeLevelTipsView.cs:48`

## tbadventureyield

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbAdventureYield.cs`；行类型：`Zombie.Table.CfgAdventureYield`；条目：21。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Yield|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|ShowPointId|`System.Int32`|False|

代码消费样本：

## tbaichat

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbAIChat.cs`；行类型：`Zombie.Table.CfgAIChat`；条目：139。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|WordType|`Zombie.Table.WordType`|False|
|ReportEventType|`Zombie.Table.ReportEventType`|False|
|MoodRange|`System.Collections.Generic.List`1[System.Int32]`|False|
|Content|`System.String`|False|
|Response|`System.Collections.Generic.List`1[System.Int32]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingChatManager.cs:128`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingHero.cs:134`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ChatDatabase.cs:34`

## tbattribute

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbAttribute.cs`；行类型：`Zombie.Table.CfgAttribute`；条目：943。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Trade|`Zombie.Table.TradeType`|False|
|AttributePlayType|`Zombie.Table.AttributePlayType`|False|
|AttributeType|`Zombie.Table.AttributeType`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ForgeProxy.cs:307`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/HeroData.cs:213`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PetProxy.cs:99`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIAttributeCheckView.cs:66`

## tbattributepower

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbAttributePower.cs`；行类型：`Zombie.Table.CfgAttributePower`；条目：35。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`Zombie.Table.AttributeType`|False|
|Trust|`Zombie.Table.AttributeType`|False|
|Pct|`System.Single`|False|

代码消费样本：

## tbaura

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbAura.cs`；行类型：`Zombie.Table.CfgAura`；条目：8991。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|UpgradeNeed|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|AddAttribute|`System.Collections.Generic.Dictionary`2[System.Int32,System.Int64]`|False|
|UnlockNeed|`Zombie.Table.Common.UnlockCondition`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/AuraProxy.cs:51`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIAuraCom.cs:148`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIAuraCom.cs:153`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIAuraCom.cs:257`

## tbbattle

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBattle.cs`；行类型：`Zombie.Table.CfgBattle`；条目：1435。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|MainLevels|`System.String`|False|
|Sequence|`System.Int32`|False|
|BattleValue|`System.Int32`|False|
|Boss|`System.Boolean`|False|
|Scene|`System.String`|False|
|SceneName|`System.String`|False|
|BattleName|`System.String`|False|
|SceneIcon|`System.String`|False|
|TaskTips|`System.String`|False|
|BattleDrop|`Zombie.Table.Common.Drop`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|MonsterQualityDrop|`System.Collections.Generic.Dictionary`2[Zombie.Table.MonsterQuality,Zombie.Table.Common.Drop]`|False|
|RewardPreview|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|MonsterShow|`System.Int32`|False|
|Transform|`System.Collections.Generic.List`1[System.Single]`|False|
|NormalMonsterGroup|`System.Collections.Generic.List`1[System.Int32]`|False|
|NormalMonsterGroup_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgMonsterGroup]`|True|
|EliteMonsterGroup|`System.Collections.Generic.List`1[System.Int32]`|False|
|EliteMonsterGroup_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgMonsterGroup]`|True|
|WaveRule|`System.Collections.Generic.List`1[Zombie.Table.battle.WaveMonster]`|False|
|BossMonster|`System.Collections.Generic.List`1[System.Int32]`|False|
|BossMonster_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgMonster]`|True|
|BattleQualityType|`Zombie.Table.BattleQualityType`|False|
|RecommendPower|`System.Int32`|False|
|AreaId|`System.Int32`|False|
|BattleGroupCost|`Zombie.Table.item.IdValue`|False|
|DropTips|`System.Int32`|False|
|PassedNeedWinTeamNum|`System.Int32`|False|
|FailTips|`System.Collections.Generic.List`1[Zombie.Table.UI.BattleFailTip]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/GuideManager.cs:342`
- `research/code-study/managed/decompiled/HotUpdate/Dungeon/GroupBattleController.cs:85`
- `research/code-study/managed/decompiled/HotUpdate/Dungeon/UIGroupBattleCom.cs:85`
- `research/code-study/managed/decompiled/HotUpdate/Dungeon/UIGroupBattleMainView.cs:190`

## tbbattlearea

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBattleArea.cs`；行类型：`Zombie.Table.CfgBattleArea`；条目：20。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|AreaBattleMap|`System.String`|False|
|AreaDetailScene|`System.String`|False|
|AreaIcon|`System.String`|False|
|LevelRange|`System.Collections.Generic.List`1[System.Int32]`|False|
|RecommendCombatPower|`System.Int64`|False|
|AreaDesc|`System.String`|False|
|AreaDescNPC|`System.String`|False|
|RewardPreview|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Task|`System.Collections.Generic.List`1[System.Int32]`|False|
|InitAdventurePoint|`System.Int32`|False|
|SuitableLevel|`System.Collections.Generic.List`1[System.Int32]`|False|
|ExploreProgress|`System.Collections.Generic.List`1[Zombie.Table.adventure.ExploreProgress]`|False|
|BossName|`System.String`|False|
|BossIcon|`System.String`|False|
|Boss|`System.Collections.Generic.List`1[System.Int32]`|False|
|UnlockNeedCondition|`System.Collections.Generic.List`1[Zombie.Table.Common.UnlockCondition]`|False|
|NeedServerOpenDay|`System.Int32`|False|
|JumpType|`System.Int32`|False|
|FixedBossLocation|`Zombie.Table.Common.Location`|False|
|RandomEventLocation|`System.Collections.Generic.List`1[Zombie.Table.Common.Location]`|False|
|TaxRange|`System.Collections.Generic.List`1[System.Int64]`|False|
|QuickBuyNum|`System.Int32[]`|False|
|ConsumableLv|`Zombie.Table.ValueIntRange`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Dungeon/UIGroupBattleOverviewView.cs:122`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingGameMode.cs:1006`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingGameMode.cs:1519`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/LevelProxy.cs:107`

## tbbattlestage

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBattleStage.cs`；行类型：`Zombie.Table.CfgBattleStage`；条目：4。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|Icon|`System.String`|False|
|UnlockArea|`System.Collections.Generic.List`1[System.Int32]`|False|
|UnlockArea_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgBattleArea]`|True|
|LevelRange|`System.Collections.Generic.List`1[System.Int32]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIExploreBigMapView.cs:169`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIExploreBigMapView.cs:190`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIExploreBigMapView.cs:201`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIExploreBigMapView.cs:262`

## tbbattletask

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBattleTask.cs`；行类型：`Zombie.Table.CfgBattleTask`；条目：0。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|TaskDesc|`System.String`|False|
|ExploratoryDegree|`System.Int32`|False|
|BattleTaskType|`Zombie.Table.adventure.TaskType`|False|
|BattleTaskCondition|`Zombie.Table.adventure.task.Condition`|False|
|FinishValue|`System.Int32`|False|

代码消费样本：

## tbblesslv

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBlessLv.cs`；行类型：`Zombie.Table.CfgBlessLv`；条目：165。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|BlessType|`Zombie.Table.Common.BlessType`|False|
|Lv|`System.Int32`|False|
|Upgrade|`Zombie.Table.item.IdValue`|False|
|Attribute|`Zombie.Table.Common.Attribute`|False|
|ShowAttribute|`Zombie.Table.Common.Attribute`|False|

代码消费样本：

## tbblessrank

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBlessRank.cs`；行类型：`Zombie.Table.CfgBlessRank`；条目：5。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|Rank|`System.Int32`|False|
|Attribute|`System.Collections.Generic.List`1[Zombie.Table.Common.Attribute]`|False|
|Atttack|`System.Collections.Generic.List`1[System.Int32]`|False|
|Atttack_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgBlessLv]`|True|
|Defense|`System.Collections.Generic.List`1[System.Int32]`|False|
|Defense_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgBlessLv]`|True|
|HP|`System.Collections.Generic.List`1[System.Int32]`|False|
|HP_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgBlessLv]`|True|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BlessProxy.cs:55`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBlessCom.cs:80`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBlessCom.cs:90`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBlessCom.cs:106`

## tbbody

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBody.cs`；行类型：`Zombie.Table.CfgBody`；条目：29。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Icon|`System.String`|False|
|TowerCleared|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIDay3RankPlayer.cs:23`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIPlayerMainView.cs:98`

## tbbook

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBook.cs`；行类型：`Zombie.Table.CfgBook`；条目：42。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|Name|`System.String`|False|
|Quality|`Zombie.Table.BookQualityType`|False|
|Icon|`System.String[]`|False|
|HeroId|`System.Int32`|False|
|HeroId_Ref|`Zombie.Table.CfgHero`|True|
|Fragment|`System.Int32`|False|
|Fragment_Ref|`Zombie.Table.CfgItem`|True|
|ComposeNeedNum|`System.Int32`|False|
|TranslateNum|`System.Int32`|False|
|Talents|`Zombie.Table.Common.CommonId[]`|False|
|UnlockUniqueSkill|`Zombie.Table.Common.CommonId`|False|
|SkillPool|`System.Int32`|False|
|NewSkillStar|`System.Int32[]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BookProxy.cs:164`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BookProxy.cs:452`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BookProxy.cs:497`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBookCom.cs:63`

## tbbookdraw

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBookDraw.cs`；行类型：`Zombie.Table.CfgBookDraw`；条目：1。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|Type|`System.Int32`|False|
|CostID|`System.Int32`|False|
|CostNum|`System.Int32`|False|
|WishAddProb|`System.Int32`|False|
|WishUnlockNeed|`System.Int32`|False|
|WishLib|`System.Collections.Generic.Dictionary`2[System.Int32,System.Int32]`|False|
|InitWishBook|`System.Collections.Generic.HashSet`1[System.Int32]`|False|
|InitWishBook_Ref|`System.Collections.Generic.HashSet`1[Zombie.Table.CfgBook]`|True|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BookProxy.cs:15`

## tbbooklevel

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBookLevel.cs`；行类型：`Zombie.Table.CfgBookLevel`；条目：20160。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|BookId|`System.Int32`|False|
|BookId_Ref|`Zombie.Table.CfgBook`|True|
|Level|`System.Int32`|False|
|Attribute|`Zombie.Table.Common.Attribute[]`|False|
|TalentPoint|`System.Int32`|False|
|UpgradeNeedExp|`System.Int32`|False|
|Condition|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBookDetailLevelCom.cs:141`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBookDetailLevelCom.cs:142`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBookDetailLevelCom.cs:182`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBookDetailLevelCom.cs:196`

## tbbookskilllib

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBookSkillLib.cs`；行类型：`Zombie.Table.CfgBookSkillLib`；条目：80。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|LibId|`System.Int32`|False|
|HeroSkill|`System.Int32`|False|
|HeroSkill_Ref|`Zombie.Table.CfgHeroSkill`|True|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBookSkillPoolView.cs:40`

## tbbookstar

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBookStar.cs`；行类型：`Zombie.Table.CfgBookStar`；条目：1302。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|BookId|`System.Int32`|False|
|Star|`System.Int32`|False|
|Attribute|`Zombie.Table.Common.Attribute[]`|False|
|UpgradeNeedFragment|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BookProxy.cs:152`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BookProxy.cs:165`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBookDetailStarCom.cs:106`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBookDetailStarCom.cs:107`

## tbbooktalent

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBookTalent.cs`；行类型：`Zombie.Table.CfgBookTalent`；条目：20。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|Name|`System.String`|False|
|Attribute|`Zombie.Table.Common.Attribute`|False|
|Icon|`System.String`|False|
|UpgradeParam|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBookTalentView.cs:76`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBookTalentView.cs:135`

## tbborder

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBorder.cs`；行类型：`Zombie.Table.CfgBorder`；条目：9。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|AttrDesc|`System.String`|False|
|UnlockDesc|`System.String`|False|
|Icon|`System.String`|False|
|Effect|`System.String`|False|
|TowerCleared|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIPlayerHead.cs:52`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIPlayerHeadBorderCom.cs:178`

## tbbox

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBox.cs`；行类型：`Zombie.Table.CfgBox`；条目：4。

|字段|类型|生成引用字段|
|---|---|---|
|BoxID|`System.Int32`|False|
|Icon|`System.String`|False|
|LightIcon|`System.String`|False|
|Name|`System.String`|False|
|Color|`System.String`|False|
|Description|`System.String`|False|
|CostNumMax|`System.Int32`|False|
|PointNum|`System.Int32`|False|
|DropID|`System.Int32`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|ProbabilityInfo|`System.Collections.Generic.List`1[Zombie.Table.Pool.AllProbability]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBoxInfoView.cs:26`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBoxInfoView.cs:31`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBoxMainView.cs:109`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBoxMainView.cs:144`

## tbboxpoint

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBoxPoint.cs`；行类型：`Zombie.Table.CfgBoxPoint`；条目：9。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|AccumulatedPointsDemand|`System.Int32`|False|
|RewardID|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BoxProxy.cs:56`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBoxMainView.cs:107`

## tbbuffplay

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBuffPlay.cs`；行类型：`Zombie.Table.CfgBuffPlay`；条目：19。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Particle|`System.String`|False|
|Point|`Zombie.Table.Play.HitPoint`|False|
|Offset|`Zombie.Table.V3`|False|
|Type|`Zombie.Table.Play.BuffType`|False|
|Delay|`System.Single`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Battle/BattleBuff.cs:24`
- `research/code-study/managed/decompiled/HotUpdate/Battle/BattleCharacterEntity.cs:777`

## tbbuffshow

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBuffShow.cs`；行类型：`Zombie.Table.CfgBuffShow`；条目：58。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Color|`System.String`|False|
|Size|`System.Int32`|False|
|Name|`System.String`|False|
|Icon|`System.String`|False|
|ShowSort|`System.Int32`|False|
|ShowStack|`System.Boolean`|False|

代码消费样本：

## tbbuilding

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBuilding.cs`；行类型：`Zombie.Table.CfgBuilding`；条目：21。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|BuildingType|`Zombie.Table.BuildingType`|False|
|Feature|`Zombie.Table.Feature`|False|
|NPCName|`System.String`|False|
|NPCModel|`System.String`|False|
|NPCUiModel|`System.String`|False|
|NPCIcon|`System.String`|False|
|InitialModelPath|`System.String`|False|
|Effect|`System.String`|False|
|EffectTrans|`Zombie.Table.ObjectTransform`|False|
|BaseEffect|`Zombie.Table.building.Effect`|False|
|GuideConversationDelayTime|`System.Int32`|False|
|GuideConversationID|`System.Int32`|False|
|JumpType|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingData.cs:51`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingGameMode.cs:1017`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingHeroStateBuyEquipment.cs:43`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingProxy.cs:26`

## tbbuildinglevel

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbBuildingLevel.cs`；行类型：`Zombie.Table.CfgBuildingLevel`；条目：489。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Level|`System.Int32`|False|
|BuildingPower|`System.Int32`|False|
|ModelPath|`System.String`|False|
|BuildingUpCost|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|BuildingUpNeedTime|`System.Int64`|False|
|AttributeID|`System.Collections.Generic.List`1[Zombie.Table.Common.Attribute]`|False|
|LevelUpShow|`System.Collections.Generic.List`1[Zombie.Table.BuildingLevelUpShow]`|False|
|JumpType|`System.Int32`|False|
|BuildingUpCondition|`System.Collections.Generic.List`1[Zombie.Table.Common.UnlockCondition]`|False|
|Effect|`Zombie.Table.building.Effect`|False|
|BuyRates|`System.Collections.Generic.List`1[System.Single]`|False|
|PopupInterfaceDelayTime|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingData.cs:30`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingData.cs:32`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingData.cs:84`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ConsumableProxy.cs:99`

## tbchaptertask

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbChapterTask.cs`；行类型：`Zombie.Table.CfgChapterTask`；条目：114。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|TaskId|`System.Collections.Generic.List`1[System.Int32]`|False|
|TaskId_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgTask]`|True|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/StoryManager.cs:144`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/StoryManager.cs:163`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIChapterTaskView.cs:74`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMainView.cs:557`

## tbcontinuerecharge

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbContinueRecharge.cs`；行类型：`Zombie.Table.CfgContinueRecharge`；条目：2。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|NeedScore|`System.Int32`|False|
|FinalReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ChargeContinueCom.cs:291`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ChargeContinueCom.cs:294`

## tbconversation

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbConversation.cs`；行类型：`Zombie.Table.CfgConversation`；条目：61。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|GroupID|`System.Int32`|False|
|CharacterName|`System.String`|False|
|Direction|`System.Int32`|False|
|CharacterImg|`System.String`|False|
|ExImage|`System.String`|False|
|Content|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ConversationManager.cs:63`

## tbconvoy

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbConvoy.cs`；行类型：`Zombie.Table.CfgConvoy`；条目：4。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|Name|`System.String`|False|
|Quality|`Zombie.Table.QualityType`|False|
|Icon|`System.String`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.Common.ItemNumWeight]`|False|
|Weight|`System.Int32`|False|
|TeamNum|`System.Int32`|False|
|Duration|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIConvoyHelpView.cs:133`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIConvoyJoinView.cs:67`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIConvoyMainView.cs:341`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIConvoyMainView.cs:542`

## tbcrafting

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbCrafting.cs`；行类型：`Zombie.Table.CfgCrafting`；条目：4。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|RawMaterial|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBackPackView.cs:134`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UICommonItemBubble.cs:85`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UICraftItemView.cs:193`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRelicDetailView.cs:285`

## tbdailytask

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbDailyTask.cs`；行类型：`Zombie.Table.CfgDailyTask`；条目：2。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|UnlockNeedCondition|`System.Collections.Generic.List`1[Zombie.Table.Common.UnlockCondition]`|False|
|TaskId|`System.Collections.Generic.List`1[System.Int32]`|False|
|TaskId_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgTask]`|True|
|ProgressTaskID|`System.Collections.Generic.List`1[System.Int32]`|False|
|ProgressTaskID_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgProgressTask]`|True|

代码消费样本：

## tbday3militaryrank

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbDay3MilitaryRank.cs`；行类型：`Zombie.Table.CfgDay3MilitaryRank`；条目：14。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`Zombie.Table.activity.ActivityMilitary`|False|
|GNum|`System.Int32`|False|
|TNum|`System.Int32`|False|
|RScore|`Zombie.Table.ValueRange`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.Common.Reward]`|False|
|Name|`System.String`|False|
|Icon|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIDay3GroupRankCom.cs:113`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIDay3GroupRankCom.cs:150`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIDay3RankGatherCom.cs:35`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIDay3RankLoginView.cs:32`

## tbday3totalrank

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbDay3TotalRank.cs`；行类型：`Zombie.Table.CfgDay3TotalRank`；条目：8。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Min|`System.Int32`|False|
|Max|`System.Int32`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIDay3TotalRankCom.cs:120`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIDay3TotalRankCom.cs:132`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIDay3TotalRankCom.cs:168`

## tbdrawlib

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbDrawLib.cs`；行类型：`Zombie.Table.CfgDrawLib`；条目：3871。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|OrderID|`System.Int32`|False|
|PoolID|`System.Int32`|False|
|LibId|`System.Int32`|False|
|ItemId|`System.Int32`|False|
|ItemId_Ref|`Zombie.Table.CfgItem`|True|
|ItemNum|`System.Int32`|False|
|Weight|`System.Int32`|False|
|MaxTimes|`System.Int32`|False|
|ShowMinTimes|`System.Int32`|False|
|BigRewardReset|`System.Boolean`|False|
|RewardType|`System.Int32`|False|

代码消费样本：

## tbdungeon

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbDungeon.cs`；行类型：`Zombie.Table.CfgDungeon`；条目：1410。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Layer|`System.Int32`|False|
|Name|`System.String`|False|
|Heros|`System.Collections.Generic.List`1[System.Int32]`|False|
|Heros_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgDungeonHeros]`|True|
|FirstReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Reward|`Zombie.Table.item.IdValue`|False|
|Functions|`System.Collections.Generic.List`1[Zombie.Table.UI.Label]`|False|
|RewardPreview|`Zombie.Table.item.IdValue[]`|False|
|Condition|`Zombie.Table.Common.UnlockCondition`|False|
|NextReward|`Zombie.Table.Common.Cost`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/DungeonProxy.cs:68`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/DungeonProxy.cs:427`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PlayerProxy.cs:723`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIDungeonMainView.cs:85`

## tbdungeonchapter

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbDungeonChapter.cs`；行类型：`Zombie.Table.CfgDungeonChapter`；条目：5。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.Int32`|False|
|Scene|`System.String`|False|
|Icon|`System.String`|False|
|BattleIcon|`System.String`|False|
|PointType|`System.Int32`|False|
|Dungeon|`System.Collections.Generic.List`1[System.Int32]`|False|
|Dungeon_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgDungeon]`|True|

代码消费样本：

## tbdungeonheros

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbDungeonHeros.cs`；行类型：`Zombie.Table.CfgDungeonHeros`；条目：7050。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Hero|`System.Int32`|False|
|Hero_Ref|`Zombie.Table.CfgHero`|True|
|Lv|`System.Int32`|False|
|Star|`System.Int32`|False|
|Power|`System.Int64`|False|
|Attribute|`System.Collections.Generic.List`1[Zombie.Table.Common.Attribute]`|False|

代码消费样本：

## tbequipmentaffixlib

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbEquipmentAffixLib.cs`；行类型：`Zombie.Table.CfgEquipmentAffixLib`；条目：1125。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Lib|`System.Int32`|False|
|PackId|`System.Int32`|False|
|Weight|`System.Int32`|False|

代码消费样本：

## tbequipmentaffixpack

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbEquipmentAffixPack.cs`；行类型：`Zombie.Table.CfgEquipmentAffixPack`；条目：3000。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Pack|`System.Int32`|False|
|AttributeId|`System.Int32`|False|
|Weight|`System.Int32`|False|
|UpgradeNeed|`System.Int32`|False|
|AttributeValue|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ForgeProxy.cs:314`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/HeroData.cs:212`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIEquipmentDealView.cs:148`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIEquipmentDealView.cs:220`

## tbequipmentenhancelevel

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbEquipmentEnhanceLevel.cs`；行类型：`Zombie.Table.CfgEquipmentEnhanceLevel`；条目：259。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|EquipmentId|`System.Int32`|False|
|Level|`System.Int32`|False|
|Attribute|`System.Collections.Generic.List`1[Zombie.Table.Common.Attribute]`|False|
|UpgradeNeed|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|EquipmentDisassembly|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/HeroData.cs:203`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIEquipmentDealView.cs:183`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIEquipmentDealView.cs:280`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIEquipmentDealView.cs:292`

## tbequipmentshop

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbEquipmentShop.cs`；行类型：`Zombie.Table.CfgEquipmentShop`；条目：60。

|字段|类型|生成引用字段|
|---|---|---|
|EquipmentId|`System.Int32`|False|
|Price|`System.Int32`|False|
|NeedBuildingLevel|`Zombie.Table.NeedBuildingLevel`|False|
|EquipmentShopType|`Zombie.Table.EquipmentShopType`|False|
|HeroLevelCondition|`System.Int32`|False|
|EquipmentShopPos|`System.Int32`|False|
|AddExp|`System.Int32`|False|
|QualityRandom|`System.Collections.Generic.List`1[Zombie.Table.EquipmentQualityRandom]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingHeroThinkState.cs:170`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingHeroThinkState.cs:191`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingHeroThinkState.cs:211`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingHeroThinkState.cs:304`

## tbexplorationpack

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbExplorationPack.cs`；行类型：`Zombie.Table.CfgExplorationPack`；条目：2。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|PoolName|`System.String`|False|
|Icon|`System.String`|False|
|Banner|`System.String`|False|
|Bg|`System.String`|False|
|Type|`System.Int32`|False|
|CostID|`System.Int32`|False|
|CostNum|`System.Int32`|False|
|Name|`System.String`|False|
|RelicProb|`System.Collections.Generic.List`1[Zombie.Table.Relic.PackProbability]`|False|
|StoneProb|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|UnlockCondition|`Zombie.Table.Common.UnlockCondition`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRelicDrawCom.cs:63`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRelicMapView.cs:83`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRelicRollView.cs:115`

## tbexplorationpage

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbExplorationPage.cs`；行类型：`Zombie.Table.CfgExplorationPage`；条目：3。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|PoolName|`System.String`|False|
|Banner|`System.String`|False|
|RelicsIncluded|`System.Collections.Generic.List`1[System.Int32]`|False|
|UnlockCondition|`Zombie.Table.Common.UnlockCondition`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/RelicProxy.cs:227`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRelicCatalogueView.cs:203`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRelicDetailView.cs:165`

## tbfamilyactivity

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbFamilyActivity.cs`；行类型：`Zombie.Table.CfgFamilyActivity`；条目：2。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Type|`Zombie.Table.FamilyActivityType`|False|
|Name|`System.String`|False|
|Icon|`System.String`|False|
|Feature|`Zombie.Table.Feature`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/UIFamilyMainView.cs:204`
- `research/code-study/managed/decompiled/HotUpdate/UIFamilyMainView.cs:269`

## tbfamilyboss

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbFamilyBoss.cs`；行类型：`Zombie.Table.CfgFamilyBoss`；条目：7。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|Lv|`System.Int32`|False|
|HeadIcon|`System.String`|False|
|CommandPower|`System.Int64`|False|
|Damage|`System.Int64`|False|
|MonsterId|`System.Collections.Generic.List`1[System.Int32]`|False|
|MonsterId_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgMonster]`|True|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFamilyBossBattleView.cs:205`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFamilyBossHeroView.cs:112`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFamilyBossMainView.cs:130`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFamilyBossResultView.cs:67`

## tbfamilybosshp

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbFamilyBossHp.cs`；行类型：`Zombie.Table.CfgFamilyBossHp`；条目：999。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Hp|`System.Int64`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFamilyBossBattleView.cs:208`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFamilyBossBattleView.cs:346`

## tbfamilybossperson

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbFamilyBossPerson.cs`；行类型：`Zombie.Table.CfgFamilyBossPerson`；条目：30。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Damage|`System.Int64`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFamilyBossMainView.cs:45`

## tbfamilybossrank

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbFamilyBossRank.cs`；行类型：`Zombie.Table.CfgFamilyBossRank`；条目：10。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Min|`System.Int32`|False|
|Max|`System.Int32`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFamilyBossRankView.cs:82`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFamilyBossRankView.cs:85`

## tbfamilydonate

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbFamilyDonate.cs`；行类型：`Zombie.Table.CfgFamilyDonate`；条目：3。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|ShopType|`Zombie.Table.shop.ExchangeType`|False|
|GoodName|`System.String`|False|
|Goods|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Price|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Sort|`System.Int32`|False|
|Limit|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFamilyDonateView.cs:73`

## tbfamilyflag

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbFamilyFlag.cs`；行类型：`Zombie.Table.CfgFamilyFlag`；条目：10。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Type|`System.Int32`|False|
|ImagePath|`System.String`|False|
|ModelPath|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/UIFamilyMainView.cs:252`
- `research/code-study/managed/decompiled/HotUpdate/UIFamilyMainView.cs:253`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFamilyCreateView.cs:126`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFamilyFlag.cs:23`

## tbfamilylevel

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbFamilyLevel.cs`；行类型：`Zombie.Table.CfgFamilyLevel`；条目：30。

|字段|类型|生成引用字段|
|---|---|---|
|Level|`System.Int32`|False|
|ExpRequire|`System.Int32`|False|
|MemberAmount|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/UIFamilyMainView.cs:179`
- `research/code-study/managed/decompiled/HotUpdate/UIFamilyMainView.cs:181`
- `research/code-study/managed/decompiled/HotUpdate/UIFamilyMainView.cs:233`
- `research/code-study/managed/decompiled/HotUpdate/UIFamilyMainView.cs:235`

## tbfamilymanage

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbFamilyManage.cs`；行类型：`Zombie.Table.CfgFamilyManage`；条目：11。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Type|`Zombie.Table.FamilyManageType`|False|
|Name|`System.String`|False|
|ImagePath|`System.String`|False|
|Red|`System.Boolean`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFamilyManageView.cs:138`

## tbfamilypermission

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbFamilyPermission.cs`；行类型：`Zombie.Table.CfgFamilyPermission`；条目：5。

|字段|类型|生成引用字段|
|---|---|---|
|Rank|`System.Int32`|False|
|FamilyPermissionType|`System.Collections.Generic.List`1[Zombie.Table.FamilyPermissionType]`|False|
|MemberPermission|`System.Collections.Generic.List`1[Zombie.Table.FamilyPermissionType]`|False|
|SettingPermission|`System.Collections.Generic.List`1[Zombie.Table.FamilyPermissionType]`|False|
|ManagePermission|`System.Collections.Generic.List`1[System.Int32]`|False|
|ManagePermission_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgFamilyManage]`|True|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/UIFamilySettingView.cs:442`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFamilyManagerCom.cs:58`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFamilyManageView.cs:70`

## tbfamliyshop

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbFamliyShop.cs`；行类型：`Zombie.Table.CfgFamilyShop`；条目：5。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|ShopType|`Zombie.Table.shop.ShopType`|False|
|GoodName|`System.String`|False|
|Goods|`Zombie.Table.item.IdValue`|False|
|Price|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Sort|`System.Int32`|False|
|Limit|`System.Int32`|False|
|Discount|`System.Single`|False|
|FinalLimit|`System.Boolean`|False|
|FamilyLv|`System.Int32`|False|
|Desc|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFamilyShopView.cs:151`

## tbfeatureunlock

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbFeatureUnlock.cs`；行类型：`Zombie.Table.CfgFeatureUnlock`；条目：80。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`Zombie.Table.Feature`|False|
|UnlockNeedCondition|`System.Collections.Generic.List`1[Zombie.Table.Common.UnlockCondition]`|False|
|ShowPopup|`System.Boolean`|False|
|Name|`System.String`|False|
|Icon|`System.String`|False|
|UnlockDesc|`System.String`|False|
|UnlockGuideID|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PlayerProxy.cs:241`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PlayerProxy.cs:261`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PlayerProxy.cs:360`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFeatureUnlockView.cs:30`

## tbfirstrecharge

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbFirstRecharge.cs`；行类型：`Zombie.Table.CfgFirstRecharge`；条目：7。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|ExtReward|`Zombie.Table.item.IdValue`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFirstChargeView.cs:337`

## tbfundrebate

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbFundRebate.cs`；行类型：`Zombie.Table.CfgFundRebate`；条目：90。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|MainId|`System.Int32`|False|
|MainId_Ref|`Zombie.Table.CfgFundRebateMain`|True|
|BaseReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|HighReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Name|`System.String`|False|
|FinishValue|`System.Int32`|False|
|Type|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/FundItem1Com.cs:68`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFundManorBuyView.cs:129`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFundManorBuyView.cs:145`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFundManorView.cs:169`

## tbfundrebatemain

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbFundRebateMain.cs`；行类型：`Zombie.Table.CfgFundRebateMain`；条目：11。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|PayId|`System.Int32`|False|
|PayId_Ref|`Zombie.Table.CfgPay`|True|
|ConditionType|`Zombie.Table.activity.FundRebateConditionType`|False|
|Level|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIFundManorView.cs:141`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMallFundRebateCom.cs:195`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMallFundRebateCom.cs:289`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMallFundRebateCom.cs:305`

## tbglobal

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbGlobal.cs`；行类型：`Zombie.Table.CfgGlobal`；条目：1。

|字段|类型|生成引用字段|
|---|---|---|
|ProductInterval|`System.Int32`|False|
|ProfessionIcon|`System.Collections.Generic.List`1[System.String]`|False|
|ProfessionIconSmall|`System.Collections.Generic.List`1[System.String]`|False|
|TroopIcon|`System.Collections.Generic.List`1[System.String]`|False|
|TroopTab|`System.Collections.Generic.List`1[System.String]`|False|
|TroopTabUnchoose|`System.Collections.Generic.List`1[System.String]`|False|
|InitialIron|`System.Int32`|False|
|TimeFinishPrice|`Zombie.Table.time.priceSec`|False|
|InitialItem|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|InitialBuilding|`System.Collections.Generic.List`1[Zombie.Table.Common.BuildingInitial]`|False|
|Heroinitial|`System.Collections.Generic.List`1[Zombie.Table.Common.HeroInitial]`|False|
|HeroAttribute|`System.Collections.Generic.List`1[System.Int32]`|False|
|WorldChatSize|`System.Int32`|False|
|FamilyChatSize|`System.Int32`|False|
|CreatGuildCost|`System.Int32`|False|
|CreateGuildFree|`System.Int32`|False|
|HeroDrawRecordSize|`System.Int32`|False|
|ChangeFamilyName|`System.Int32`|False|
|ChangeFamilyShortName|`System.Int32`|False|
|FamilyInviteListSize|`System.Int32`|False|
|ConversationSpeed|`System.Single`|False|
|TaskGuideTime|`System.Int32`|False|
|OneStrengthRefreshTime|`System.Int32`|False|
|FirstCallNumber|`System.Int32`|False|
|CallNumber|`System.Int32`|False|
|HeroInitGold|`System.Int32`|False|
|CoinsId|`System.Int32`|False|
|Taxation|`System.Int32`|False|
|AttributeShowList|`System.Collections.Generic.List`1[Zombie.Table.AttributeType]`|False|
|HeroDetailAttrList|`System.Collections.Generic.List`1[Zombie.Table.AttributeType]`|False|
|HeroBaseAttrArray|`Zombie.Table.AttributeType[]`|False|
|HeroAdditiveAttrArray|`Zombie.Table.AttributeType[]`|False|
|HeroSpecialAttrArray|`Zombie.Table.AttributeType[]`|False|
|HeroEquipAttrArray|`Zombie.Table.AttributeType[]`|False|
|CemeteryMax|`System.Int32`|False|
|MoralDegradationChance|`System.Single`|False|
|BattleTaxation|`System.Int32`|False|
|DrawNum|`System.Collections.Generic.List`1[System.Int32]`|False|
|RelicLevelResetConsumption|`Zombie.Table.item.IdValue`|False|
|RelicLevelResetCooldown|`System.Int32`|False|
|EquipmentShopChangePriceMaxTimes|`System.Int32`|False|
|EquipmentShopChangeUpPriceProbability|`System.Collections.Generic.List`1[Zombie.Table.NormalWeight]`|False|
|EquipmentShopChangeDownPriceProbability|`System.Collections.Generic.List`1[Zombie.Table.NormalWeight]`|False|
|HeroUnderframeLevel|`System.Int32`|False|
|MaxLevel|`System.Int32`|False|
|DangerousLevel|`System.Int32`|False|
|MainBuildingID|`System.Int32`|False|
|Year|`System.Int32`|False|
|CombatPowerFactor|`System.Single`|False|
|FriendshipGroup|`System.Int32`|False|
|FriendshipWave|`System.Int32`|False|
|FriendshipBattle|`System.Int32`|False|
|FriendshipFlee|`System.Int32`|False|
|FriendshipChatType|`Zombie.Table.ValueRange`|False|
|MoodValueBattle|`Zombie.Table.ValueRange`|False|
|MoodValueFlee|`Zombie.Table.ValueRange`|False|
|MoodValueAdd|`Zombie.Table.KeyValue`|False|
|MoodValueSub|`Zombie.Table.KeyValue`|False|
|MoodAddBySpa|`System.Int32`|False|
|LegionExpId|`System.Int32`|False|
|AtkEvaluateRange|`Zombie.Table.ValueRange`|False|
|HpEvaluateRange|`Zombie.Table.ValueRange`|False|
|DefEvaluateRange|`Zombie.Table.ValueRange`|False|
|MoralEvaluateRange|`Zombie.Table.ValueRange`|False|
|TradeEquipmentUpgradeNeed|`System.Collections.Generic.List`1[System.Int32]`|False|
|MaxRound|`System.Int32`|False|
|BattleGroupCost|`Zombie.Table.item.IdValue`|False|
|TradeEquipmentEquipUpgradeFactor|`System.Collections.Generic.List`1[System.Single]`|False|
|TradeEquipmentPassiveUpgradeFactor|`System.Collections.Generic.List`1[System.Single]`|False|
|EquipmentForceQualityWeight|`System.Int32`|False|
|EquipmentForceLevelWeight|`System.Int32`|False|
|SummonPoolExp5|`System.Int32`|False|
|SummonItem|`System.Int32`|False|
|SummonItem_Ref|`Zombie.Table.CfgItem`|True|
|SummonPoolExp30|`System.Int32`|False|
|PointID|`System.Int32`|False|
|ResourceExchangeExperience|`System.Int32`|False|
|ManorLandRebuildTime|`System.Int32`|False|
|ManorRes|`System.Collections.Generic.List`1[System.Int32]`|False|
|HeroGeneralFragments|`System.Collections.Generic.Dictionary`2[System.Int32,System.Int32]`|False|
|HeroStarExchangeScore|`System.Collections.Generic.Dictionary`2[System.Int32,System.Int32]`|False|
|HeroArchiveProgress|`System.Collections.Generic.List`1[System.Int32]`|False|
|GuildExp|`System.Int32`|False|
|MainRes|`System.Collections.Generic.List`1[System.Int32]`|False|
|AdventureInitPower|`System.Int32`|False|
|EventTime|`System.Int32`|False|
|ExploreMoveSpeed|`System.Single`|False|
|ExploreDiscussTime|`System.Single`|False|
|HeroCombatPowerLevelCoefficient|`System.Collections.Generic.List`1[Zombie.Table.Common.LevelCoefficient]`|False|
|HeroCombatPowerQualityCoefficient|`System.Collections.Generic.Dictionary`2[Zombie.Table.HeroQualityType,System.Single]`|False|
|HeroCombatPowerPow|`System.Single`|False|
|SprintTaskScoreItem|`System.Int32`|False|
|AdventureYieldMinTime|`System.Int32`|False|
|AdventureYieldMaxTime|`System.Int32`|False|
|RankReduction|`System.Int32`|False|
|DailyTaskScoreItem|`System.Int32`|False|
|PresidentExpItemId|`System.Int32`|False|
|MinimumRankingScore|`System.Int32`|False|
|MaximumItemCount|`System.Int32`|False|
|AdventureEventExecuteTime|`System.Collections.Generic.Dictionary`2[Zombie.Table.adventure.EventType,System.Int32]`|False|
|AdventureAreaCoordinateRange|`Zombie.Table.RegionalCoordinates`|False|
|HeroStars|`System.Collections.Generic.List`1[System.String]`|False|
|AreaMaxHeroNum|`System.Int32`|False|
|ReviveSpeedUpCost|`System.Int32`|False|
|SoulSanctuary|`System.Int32`|False|
|ResponseEffect|`System.Collections.Generic.List`1[System.Int32]`|False|
|AutoProductionQty|`System.Int32`|False|
|DungeonJumps|`System.Collections.Generic.List`1[Zombie.Table.JumpInfoPro]`|False|
|HospitalID|`System.Int32`|False|
|HotalID|`System.Int32`|False|
|GraveyardID|`System.Int32`|False|
|FinalCountdown|`System.Int32`|False|
|QuickRecoverDelay|`System.Single`|False|
|MaxWave|`System.Int32`|False|
|HeroActionNeedSupply|`System.Int32`|False|
|SupplyItemId|`System.Int32`|False|
|SupplyRefreshTime|`System.Int32`|False|
|SupplyFreeReceiveList|`System.Collections.Generic.List`1[Zombie.Table.supply.FreeValue]`|False|
|SupplyBuyList|`System.Collections.Generic.List`1[Zombie.Table.supply.BuyValue]`|False|
|SupplyBuyItemId|`System.Int32`|False|
|AdventureRandomEventRefreshStage|`System.Collections.Generic.List`1[Zombie.Table.Common.HourAndMinute]`|False|
|AdventureRandomEventRefreshNum|`System.Int32`|False|
|AdventureRandomEventAliveTime|`System.Int32`|False|
|LoginGuideConversation|`System.Int32`|False|
|HotelGuideConversation|`System.Int32`|False|
|HospitalGuideConversation|`System.Int32`|False|
|ForgeGuideConversation|`System.Int32`|False|
|ExploreGuideConversation|`System.Int32`|False|
|DefeatGuideConversation|`System.Int32`|False|
|AdventureRandomEventShowNum|`System.Int32`|False|
|MaterialShopRefreshStage|`System.Collections.Generic.List`1[Zombie.Table.Common.HourAndMinute]`|False|
|HeroInitialGold|`System.Int32`|False|
|DiamondId|`System.Int32`|False|
|HeroCallBase|`Zombie.Table.drop.HeroCallBase`|False|
|HeroCallOnceAgain|`Zombie.Table.drop.HeroCallOnceAgain`|False|
|HeroCallRefresh|`Zombie.Table.drop.HeroCallRefresh`|False|
|HeroCallQualityWeight|`System.Collections.Generic.Dictionary`2[Zombie.Table.HeroQualityType,System.Int32]`|False|
|EquipBuildingMap|`System.Collections.Generic.Dictionary`2[Zombie.Table.EquipmentType,System.Int32]`|False|
|InitialRelic|`System.Collections.Generic.List`1[System.Int32]`|False|
|InitialRelic_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgTotem]`|True|
|AuraAttrs|`System.Int32[]`|False|
|AuraLevel|`System.Int32[]`|False|
|HeroCallFirst|`Zombie.Table.drop.HeroCallFirst`|False|
|SevenDaySign|`Zombie.Table.Common.SevenDaySignConfig`|False|
|GatherTipsGuideID|`System.Int32`|False|
|PvpRankChallengeCount|`System.Int32`|False|
|PvpRankChallengeCost|`Zombie.Table.Common.CostDay`|False|
|PvpRankShopItem|`System.Int32`|False|
|PvpRankRefresCost|`Zombie.Table.Common.Cost`|False|
|PvpRankCrushPct|`System.Int32`|False|
|PvpMatchConfig1|`Zombie.Table.Common.PvpMatchConfig`|False|
|PvpMatchConfig2|`Zombie.Table.Common.PvpMatchConfig`|False|
|PvpMatchConfig3|`Zombie.Table.Common.PvpMatchConfig`|False|
|PvpMatchConfig4|`Zombie.Table.Common.PvpMatchConfig`|False|
|PvpMatchConfig5|`Zombie.Table.Common.PvpMatchConfig`|False|
|VictoryBasePoints|`System.Int32`|False|
|VictoryBonusCoefficient|`System.Int32`|False|
|DefeatBasePoints|`System.Int32`|False|
|DefeatDeductionCoefficient|`System.Int32`|False|
|DailyResetTime|`System.Int64`|False|
|PVPVictoryRewards|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|PVPDefeatRewards|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|PvPInitialPoint|`System.Int32`|False|
|ActualPvPBattleCount|`System.Int32`|False|
|ActualPvPBattlePoints|`System.Int32`|False|
|EquipmentSoldInvalidTime|`System.Int32`|False|
|MaterialShopTierLimit|`System.Int32[]`|False|
|AuctionHouseInitial|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Bread|`System.Int32`|False|
|InitHead|`System.Int32`|False|
|InitHead_Ref|`Zombie.Table.CfgHead`|True|
|InitBody|`System.Int32`|False|
|InitBody_Ref|`Zombie.Table.CfgBody`|True|
|InitBorder|`System.Int32`|False|
|InitBorder_Ref|`Zombie.Table.CfgBorder`|True|
|NameChangeConsumption|`Zombie.Table.item.IdValue`|False|
|TotemDecomposReward|`System.Int32`|False|
|PayScoreItemId|`System.Int32`|False|
|PayScoreItemId_Ref|`Zombie.Table.CfgItem`|True|
|DmageOverTimeBaseToShield|`System.Int32`|False|
|TurfWarProtectTime|`System.Int32`|False|
|TurfWarStamina|`Zombie.Table.Common.ItemRefresh`|False|
|TurfWarProtector|`Zombie.Table.Common.ItemDayRefresh`|False|
|TurfWarDeployUnlockCondition|`System.Collections.Generic.List`1[Zombie.Table.Common.UnlockCondition]`|False|
|TurfWarOrdinaryMaxVelocity|`System.Collections.Generic.List`1[System.Single]`|False|
|TurfWarOrdinaryMinVelocity|`System.Single`|False|
|TurfWarFamilyMaxVelocity|`System.Single`|False|
|TurfWarFamilyMinVelocity|`System.Single`|False|
|TurfWarBoxRefreshCD|`System.Int32`|False|
|DungeonBattleNeedItem|`Zombie.Table.item.IdValue`|False|
|DungeonBattleStamina|`Zombie.Table.Common.ItemRefresh`|False|
|TurfWarSearchRefreshCD|`System.Int32`|False|
|TurfWarSearchRefreshCost|`Zombie.Table.item.IdValue`|False|
|AdventureEventRefreshNeedItem|`Zombie.Table.item.IdValue`|False|
|AdventureRandomEventStamina|`Zombie.Table.Common.ItemRefresh`|False|
|TurfWarMatchConfig1|`Zombie.Table.Common.PvpMatchConfig`|False|
|TurfWarMatchConfig2|`Zombie.Table.Common.PvpMatchConfig`|False|
|TurfWarMatchConfig3|`Zombie.Table.Common.PvpMatchConfig`|False|
|TurfWarMatchConfig4|`Zombie.Table.Common.PvpMatchConfig`|False|
|TurfWarMatchConfig5|`Zombie.Table.Common.PvpMatchConfig`|False|
|TurfWarMonsterPowerCoefficient|`Zombie.Table.ValueRange`|False|
|AutoDeployedHeroes|`System.Int32`|False|
|SpecialCallHeroRes|`Zombie.Table.item.IdValue`|False|
|FreeHero|`System.Collections.Generic.List`1[System.Int32]`|False|
|FreeHero_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgHero]`|True|
|RuneSlotUnlock1|`System.Collections.Generic.List`1[System.Int32]`|False|
|RuneSlotUnlock2|`System.Collections.Generic.List`1[System.Int32]`|False|
|RuneSlotUnlock3|`System.Collections.Generic.List`1[System.Int32]`|False|
|RuneSlotUnlock4|`System.Collections.Generic.List`1[System.Int32]`|False|
|RuneSlotUnlock5|`System.Collections.Generic.List`1[System.Int32]`|False|
|MaterialShopFreeRefresh|`Zombie.Table.Common.ItemRefresh`|False|
|MaterialShopDiamondRefresh|`System.Collections.Generic.List`1[System.Int32]`|False|
|ChatShowDegree|`System.Int32`|False|
|FirstJoinReward|`Zombie.Table.item.IdValue`|False|
|WorldChatInterval|`System.Int32`|False|
|FirstChargeHighLevelRewards|`System.Collections.Generic.List`1[System.Int32]`|False|
|RandomBossRate|`System.Collections.Generic.List`1[Zombie.Table.Common.RandomBossRate]`|False|
|PvpRankChallengeStamina|`Zombie.Table.Common.ItemRefresh`|False|
|StatuePartUp|`System.Int32`|False|
|StatuePromotionUp|`System.Int32`|False|
|FishMaterialRefresh|`Zombie.Table.Common.ItemRefresh`|False|
|FlowerMaterialRefresh|`Zombie.Table.Common.ItemRefresh`|False|
|BattleCombatPowerPct|`System.Collections.Generic.List`1[Zombie.Table.Common.MinMaxValue]`|False|
|JoinFamilyTime|`System.Int32`|False|
|IdleProductionMinTime|`System.Int32`|False|
|IdleProductionMaxTime|`System.Int32`|False|
|BookExpExchange|`Zombie.Table.book.BookExpExchange`|False|
|TalentCost|`System.Int32[]`|False|
|BookSkillRefreshNeed|`Zombie.Table.item.IdValue[]`|False|
|BookSkillLockNeed|`System.Collections.Generic.Dictionary`2[System.Int32,Zombie.Table.item.IdValue[]]`|False|
|BookCommonFragment|`System.Int32`|False|
|BookQualityName|`System.Collections.Generic.Dictionary`2[System.Int32,System.String]`|False|
|BookExpItemList|`System.Int32[]`|False|
|BookExpItem|`System.Int32`|False|
|EquipmentAffixRandomGrade|`System.Collections.Generic.List`1[Zombie.Table.NormalWeight]`|False|
|FamilyBossEndTime|`System.Int32`|False|
|PvpDamgeSuppress|`System.Single`|False|
|GameClubDailyTask|`System.Collections.Generic.List`1[System.Int32]`|False|
|GameClubDailyTask_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgTask]`|True|
|GameClubPermanentTask|`System.Collections.Generic.List`1[System.Int32]`|False|
|GameClubPermanentTask_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgTask]`|True|
|WeChatPublishNeed|`Zombie.Table.item.IdValue`|False|
|WeChatTag|`System.Int32[]`|False|
|WeChatTagSelectMaxNum|`System.Int32`|False|
|MysticRealmInitAp|`System.Int32`|False|
|MysticRealmCostAp|`System.Int32`|False|
|MysticRealmDailyExploreLimit|`System.Int32`|False|
|VoucherItemId|`System.Int32`|False|
|VoucherItemId_Ref|`Zombie.Table.CfgItem`|True|
|MysticRealmTicketId|`System.Int32`|False|
|MysticRealmTicketId_Ref|`Zombie.Table.CfgItem`|True|
|APBadgeId|`System.Int32`|False|
|APBadgeId_Ref|`Zombie.Table.CfgItem`|True|
|RealRechargeItemId|`System.Int32`|False|
|ConvoyRefreshItem|`Zombie.Table.Common.ItemDayRefresh`|False|
|ConvoyRefreshTime|`System.Int32`|False|
|ConvoyCostGem|`System.Int32`|False|
|ConvoyDayExpeditionTimes|`System.Int32`|False|
|ConvoyDayAssistTimes|`System.Int32`|False|
|WeChatUploadTimes|`System.Int32`|False|
|ConvoyAutoRefreshTime|`System.Int32`|False|
|PetMax|`System.Int32`|False|
|RelicFreeMax|`System.Int32`|False|
|ShareGiftShareParam|`Zombie.Table.Common.WxShareParam`|False|
|AuraTotalResources|`System.Int32[]`|False|
|AuraShowResources|`System.Collections.Generic.HashSet`1[System.Int32]`|False|
|JoinFamilyText|`System.String[]`|False|
|BookUpStarNeedBookNum|`System.Int32`|False|
|BookUpStarNeedBookStar|`System.Collections.Generic.Dictionary`2[System.Int32,System.Int32]`|False|
|MysticRealmSweepRecordValidTime|`System.Int32`|False|
|HeroRandGold|`System.Int32[]`|False|
|HeroExpAddValue1|`System.Int32`|False|
|HeroExpAddValue2|`System.Int32`|False|

代码消费样本：

## tbguide

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbGuide.cs`；行类型：`Zombie.Table.CfgGuide`；条目：659。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|GroupID|`System.Int32`|False|
|Condition|`System.String`|False|
|CondParam|`System.Collections.Generic.List`1[System.String]`|False|
|InitLoad|`System.Int32`|False|
|Action|`System.String`|False|
|ActionParam|`System.Collections.Generic.List`1[System.String]`|False|
|NextID|`System.Int32`|False|
|FNextID|`System.Int32`|False|
|SkipGroupID|`System.Int32`|False|
|SkipTime|`System.Single`|False|
|ParallelID|`System.Collections.Generic.List`1[System.Int32]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/GuideSystem.cs:73`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/GuideSystem.cs:87`

## tbguidetask

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbGuideTask.cs`；行类型：`Zombie.Table.CfgGuideTask`；条目：1594。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|GroupID|`System.Int32`|False|
|GuideType|`Zombie.Table.GuideType`|False|
|NeedSave|`System.Boolean`|False|
|Action|`System.String`|False|
|ActionParam|`System.Collections.Generic.List`1[System.String]`|False|
|NextID|`System.Int32`|False|
|FNextID|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/GuideManager.cs:113`
- `research/code-study/managed/decompiled/HotUpdate/GuideManager.cs:134`

## tbguidetrigger

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbGuideTrigger.cs`；行类型：`Zombie.Table.CfgGuideTrigger`；条目：15。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|TriggerType|`Zombie.Table.task.TaskTriggerType`|False|
|PreTriggerID|`System.Int32`|False|
|Param|`System.Collections.Generic.List`1[System.String]`|False|
|NeedSave|`System.Boolean`|False|
|GuideType|`System.Int32`|False|
|GuideID|`System.Int32`|False|
|JumpParam|`System.Collections.Generic.List`1[System.Int32]`|False|

代码消费样本：

## tbguildlevel

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbGuildLevel.cs`；行类型：`Zombie.Table.CfgGuildLevel`；条目：39。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|RankIcon|`System.String`|False|
|RankLevelIcon|`System.String`|False|
|Rank|`System.Int32`|False|
|UpgradeCondition|`System.Collections.Generic.List`1[Zombie.Table.Common.UnlockCondition]`|False|
|NeedItem|`Zombie.Table.item.IdValue`|False|
|HeroInitLevel|`System.Int32`|False|
|AdventureTeamNum|`System.Int32`|False|
|HeroMaxLevel|`System.Int32`|False|
|SupplyMax|`System.Int32`|False|
|DeployHero|`System.Int32`|False|
|Icons|`System.Collections.Generic.List`1[Zombie.Table.UI.Label]`|False|
|UnlockDesc|`System.Collections.Generic.List`1[System.String]`|False|
|VllageName|`System.String`|False|
|Tax|`System.Int32`|False|
|Goods|`System.Int32[]`|False|
|Goods_Ref|`Zombie.Table.CfgShop[]`|True|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingGameMode.cs:926`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/HeroProxy.cs:992`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PlayerProxy.cs:401`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PlayerProxy.cs:428`

## tbhead

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbHead.cs`；行类型：`Zombie.Table.CfgHead`；条目：29。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Icon|`System.String`|False|
|TowerCleared|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|SourceDesc|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIPlayerHead.cs:67`

## tbhero

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbHero.cs`；行类型：`Zombie.Table.CfgHero`；条目：57。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|ChipID|`System.Int32`|False|
|UnlockChip|`System.Int32`|False|
|TranslateChip|`System.Int32`|False|
|TradeType|`Zombie.Table.TradeType`|False|
|HeroQuality|`Zombie.Table.HeroQualityType`|False|
|InitStar|`System.Int32`|False|
|RecommendedSkills|`System.Collections.Generic.List`1[System.Int32]`|False|
|MaxLevel|`System.Int32`|False|
|LevelGroup|`System.Int32`|False|
|MaxStar|`System.Int32`|False|
|StarGroup|`System.Int32`|False|
|Column|`System.Int32`|False|
|Age|`System.Int32`|False|
|MaxAttribute|`System.Collections.Generic.List`1[Zombie.Table.Common.Attribute]`|False|
|SoundID|`System.Int32`|False|
|HeroTypeIcon|`System.String`|False|
|BodyIcon|`System.String`|False|
|HeadIcon|`System.String`|False|
|HalfIcon|`System.String`|False|
|Model|`System.Collections.Generic.List`1[System.String]`|False|
|FightModel|`System.Collections.Generic.List`1[System.String]`|False|
|AttackPlayId|`System.Int32`|False|
|RebirthPlayId|`System.Int32`|False|
|TurfBody|`System.String`|False|
|TurfHead|`System.String`|False|
|Limit|`System.Boolean`|False|
|GeneralFragment|`System.Int32`|False|
|GeneralFragment_Ref|`Zombie.Table.CfgItem`|True|
|Sort|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Battle/BattleHeroEntity.cs:25`
- `research/code-study/managed/decompiled/HotUpdate/Dungeon/GroupBattleController.cs:301`
- `research/code-study/managed/decompiled/HotUpdate/Dungeon/UIGroupBattleHeroInfo.cs:66`
- `research/code-study/managed/decompiled/HotUpdate/Dungeon/UIGroupBattleOverviewView.cs:246`

## tbheroarchivelevel

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbHeroArchiveLevel.cs`；行类型：`Zombie.Table.CfgHeroArchiveLevel`；条目：29。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|NeedStar|`System.Int32`|False|
|PassiveEffectBonus|`System.Collections.Generic.List`1[Zombie.Table.Common.Attribute]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/HeroProxy.cs:271`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/HeroProxy.cs:275`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIHeroArchiveMasterView.cs:58`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIHeroArchiveMasterView.cs:59`

## tbherocalllevel

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbHeroCallLevel.cs`；行类型：`Zombie.Table.CfgHeroCallLevel`；条目：100。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Drop|`System.Collections.Generic.List`1[Zombie.Table.NormalWeight]`|False|
|NeedExp|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIHeroCallLevelView.cs:49`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIHeroCallLevelView.cs:50`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIHeroCallLvUpView.cs:29`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIHeroCallLvUpView.cs:30`

## tbherocallpool

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbHeroCallPool.cs`；行类型：`Zombie.Table.CfgHeroCallPool`；条目：41。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|PoolId|`System.Int32`|False|
|HeroId|`System.Int32`|False|
|HeroId_Ref|`Zombie.Table.CfgHero`|True|
|Probability|`System.Int32`|False|
|NeedActive|`System.Boolean`|False|
|WishWeight|`System.Int32`|False|

代码消费样本：

## tbheroequip

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbHeroEquip.cs`；行类型：`Zombie.Table.CfgHeroEquip`；条目：196。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|EquipmentName|`System.String`|False|
|FormulaName|`System.String`|False|
|FormulaCost|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|ShowType|`Zombie.Table.ItemShowType`|False|
|EquipmentDescription|`System.String`|False|
|Price|`System.Int32`|False|
|AuctionPrice|`System.Int32`|False|
|EquipmentDisassembly|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|OrnamentType|`System.Nullable`1[Zombie.Table.OrnamentType]`|False|
|EquipmentType|`Zombie.Table.EquipmentType`|False|
|Icon|`System.String`|False|
|Trade|`Zombie.Table.TradeType`|False|
|Tier|`System.Int32`|False|
|Star|`System.Int32`|False|
|Quality|`Zombie.Table.QualityType`|False|
|QualityParam|`System.Collections.Generic.List`1[Zombie.Table.NormalWeight]`|False|
|NewSkillStar|`System.Int32[]`|False|
|InitLib|`System.Int32[]`|False|
|UpgradeLib|`System.Collections.Generic.List`1[Zombie.Table.NormalWeight]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingHeroStateBuyEquipment.cs:108`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingHeroThinkState.cs:44`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingHeroThinkState.cs:75`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ForgeProxy.cs:107`

## tbherolevel

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbHeroLevel.cs`；行类型：`Zombie.Table.CfgHeroLevel`；条目：3420。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|LevelGroup|`System.Int32`|False|
|Level|`System.Int32`|False|
|CostID|`System.Int32`|False|
|CostValue|`System.Int32`|False|
|Attribute|`System.Collections.Generic.List`1[Zombie.Table.Common.Attribute]`|False|
|TroopMax|`System.Int32`|False|
|NeedBuilding|`Zombie.Table.BuildingRequestLevel`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/HeroData.cs:245`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/HeroData.cs:246`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIExploreEventCom.cs:195`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIHeroArchiveDetailView.cs:98`

## tbheropool

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbHeroPool.cs`；行类型：`Zombie.Table.CfgHeroPool`；条目：2。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|Banner|`System.String`|False|
|Icon|`System.String`|False|
|CostID|`System.Int32`|False|
|CostNum|`System.Int32`|False|
|PointID|`System.Int32`|False|
|PointNum|`System.Int32`|False|
|ExchagePool|`System.Int32`|False|
|ResIds|`System.Collections.Generic.List`1[System.Int32]`|False|
|HeroProb|`System.Collections.Generic.List`1[Zombie.Table.Pool.AllProbability]`|False|
|RewardView|`System.Collections.Generic.List`1[Zombie.Table.Pool.HeroProbability]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIHeroDrawProbabilityView.cs:34`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIHeroDrawResultView.cs:124`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIHeroDrawView.cs:182`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIHeroDrawView.cs:196`

## tbheroskill

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbHeroSkill.cs`；行类型：`Zombie.Table.CfgHeroSkill`；条目：1222。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|NextLevelId|`System.Int32`|False|
|Name|`System.String`|False|
|TradeType|`Zombie.Table.TradeType`|False|
|SkillType|`Zombie.Table.skill.SkillType`|False|
|CD|`System.Int32`|False|
|InitCD|`System.Int32`|False|
|Order|`System.Int32`|False|
|AddAct|`System.Int32`|False|
|Trigger|`Zombie.Table.skill.Trigger`|False|
|Target|`Zombie.Table.skill.Target`|False|
|Effects|`System.Collections.Generic.List`1[System.Int32]`|False|
|Effects_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgHeroSkillEffect]`|True|
|Quality|`Zombie.Table.skill.QualityType`|False|
|Level|`System.Int32`|False|
|Icon|`System.String`|False|
|Description|`System.String`|False|
|PlayId|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Battle/BattleCharacterEntity.cs:156`
- `research/code-study/managed/decompiled/HotUpdate/Battle/BattleCharacterEntity.cs:744`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/HeroData.cs:154`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/MagicShopCom.cs:348`

## tbheroskillbuff

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbHeroSkillBuff.cs`；行类型：`Zombie.Table.CfgHeroSkillBuff`；条目：1800。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|Icon|`System.String`|False|
|Bufftype|`Zombie.Table.skill.BuffType`|False|
|Buffstate|`System.Nullable`1[Zombie.Table.skill.BuffState]`|False|
|Finish|`Zombie.Table.skill.BuffFinish`|False|
|Mutex|`System.Boolean`|False|
|MutexGroup|`System.Int32`|False|
|MutexPriority|`System.Int32`|False|
|TriggerType|`Zombie.Table.skill.BuffTrigger`|False|
|Description|`System.String`|False|
|EffectType|`System.Nullable`1[Zombie.Table.skill.BuffEffectType]`|False|
|Effect|`Zombie.Table.skill.effect.BaseEffect`|False|
|Stack|`System.Int32`|False|
|MaxStack|`System.Int32`|False|
|AddType|`Zombie.Table.skill.BuffAddType`|False|
|CanAddStack|`System.Boolean`|False|
|CanDispel|`System.Boolean`|False|
|RoundReduce|`System.Int32`|False|
|PlayId|`System.Int32`|False|
|ShowId|`System.Int32`|False|
|ShowId_Ref|`Zombie.Table.CfgBuffShow`|True|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Battle/BattleBuff.cs:23`
- `research/code-study/managed/decompiled/HotUpdate/Battle/BattleCharacterEntity.cs:774`
- `research/code-study/managed/decompiled/HotUpdate/Dungeon/UIGroupBattleCom.cs:188`
- `research/code-study/managed/decompiled/HotUpdate/Dungeon/UIGroupBattleCom.cs:223`

## tbheroskilleffect

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbHeroSkillEffect.cs`；行类型：`Zombie.Table.CfgHeroSkillEffect`；条目：1447。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|EffectType|`Zombie.Table.skill.EffectType`|False|
|Effect|`Zombie.Table.skill.effect.BaseEffect`|False|

代码消费样本：

## tbherosound

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbHeroSound.cs`；行类型：`Zombie.Table.CfgHeroSound`；条目：6。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|AttackSound|`System.Collections.Generic.List`1[System.String]`|False|
|AttackVolume|`System.Single`|False|
|BlockSound|`System.Collections.Generic.List`1[System.String]`|False|
|BlockVolume|`System.Single`|False|
|CritSound|`System.Collections.Generic.List`1[System.String]`|False|
|CritVolume|`System.Single`|False|
|DieSound|`System.Collections.Generic.List`1[System.String]`|False|
|DieVolume|`System.Single`|False|
|EvadeSound|`System.Collections.Generic.List`1[System.String]`|False|
|EvadeVolume|`System.Single`|False|
|FleeSound|`System.Collections.Generic.List`1[System.String]`|False|
|FleeVolume|`System.Single`|False|
|MissSound|`System.Collections.Generic.List`1[System.String]`|False|
|MissVolume|`System.Single`|False|
|ClickSound|`System.Collections.Generic.List`1[System.String]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Battle/BattleHeroEntity.cs:26`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingHero.cs:565`

## tbherostar

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbHeroStar.cs`；行类型：`Zombie.Table.CfgHeroStar`；条目：96。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|StarGroup|`System.Int32`|False|
|Star|`System.Int32`|False|
|CostValue|`System.Int32`|False|
|Attribute|`System.Collections.Generic.List`1[Zombie.Table.Common.Attribute]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/HeroProxy.cs:780`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIHeroCallResultView.cs:66`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIHeroDetailStarCom.cs:120`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIHeroDetailStarCom.cs:121`

## tbherostarmaptoskilllist

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbHeroStarMapToSkillList.cs`；行类型：`Zombie.Table.CfgHeroStarMapToSkillList`；条目：57。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|StarMapToSkillList|`System.Collections.Generic.Dictionary`2[System.Int32,System.Collections.Generic.List`1[System.Int32]]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/HeroProxy.cs:427`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/MagicShopCom.cs:348`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UICommonShowHeroBubble.cs:47`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIHeroArchiveDetailView.cs:80`

## tbidleproduction

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbIdleProduction.cs`；行类型：`Zombie.Table.CfgIdleProduction`；条目：90。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Production|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|UpTxt|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIIdleView.cs:63`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UITaxMainView.cs:112`

## tbitem

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbItem.cs`；行类型：`Zombie.Table.CfgItem`；条目：5100。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|Name|`System.String`|False|
|Icon|`System.String`|False|
|SmallIcon|`System.String`|False|
|Quality|`Zombie.Table.QualityType`|False|
|Display|`System.Boolean`|False|
|Tag|`System.String`|False|
|Tier|`System.Int32`|False|
|Star|`System.Int32`|False|
|Tips|`System.String`|False|
|ItemType|`Zombie.Table.ItemType`|False|
|ShowType|`Zombie.Table.ItemShowType`|False|
|LimitLevel|`System.Int32`|False|
|ShowAble|`System.Boolean`|False|
|UseAble|`System.Boolean`|False|
|MultiUseAble|`System.Boolean`|False|
|UseEffect|`Zombie.Table.item.Effect`|False|
|Desc|`System.String`|False|
|Loglevel|`System.Int32`|False|
|RemainingDate|`System.Nullable`1[System.Int64]`|False|
|RemainingTime|`System.Int32`|False|
|GainType|`System.String`|False|
|Notice|`System.Boolean`|False|
|Sort|`System.Int32`|False|
|Price|`System.Int32`|False|
|Goldprice|`Zombie.Table.item.IdValue`|False|
|SellAble|`System.Boolean`|False|
|AutoUse|`System.Boolean`|False|
|Jump|`System.Collections.Generic.List`1[Zombie.Table.JumpInfo]`|False|
|BuyoutPrice|`System.Int32`|False|
|Decompose|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|CraftTarget|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/FamilyWelcomeView.cs:59`
- `research/code-study/managed/decompiled/HotUpdate/UIFamilySettingView.cs:139`
- `research/code-study/managed/decompiled/HotUpdate/UIFamilySettingView.cs:222`
- `research/code-study/managed/decompiled/HotUpdate/Dungeon/UIGroupBattleDropItem.cs:31`

## tblanguage

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbLanguage.cs`；行类型：`Zombie.Table.CfgLanguage`；条目：1225。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.String`|False|
|SimpleChinese|`System.String`|False|
|English|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIUtils.cs:134`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIUtils.cs:144`

## tbluckywheel

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbLuckyWheel.cs`；行类型：`Zombie.Table.CfgLuckyWheel`；条目：4。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|Des|`System.String`|False|
|Prob|`Zombie.Table.item.IdValueRate[]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UILuckyWheelRewardRateView.cs:41`

## tbmagicnightmain

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMagicNightMain.cs`；行类型：`Zombie.Table.CfgMagicNightMain`；条目：2。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|DrawNeedItem|`Zombie.Table.item.IdValue`|False|
|CardLucky|`System.Int32`|False|
|LuckyMax|`System.Int32`|False|
|LuckyIcon|`Zombie.Table.item.IdValue`|False|
|RewardPreview|`System.Collections.Generic.List`1[Zombie.Table.Common.Reward]`|False|
|FeaturedHero|`System.Int32`|False|
|FeaturedHero_Ref|`Zombie.Table.CfgHero`|True|
|HeroShop|`System.Int32`|False|
|HeroShop_Ref|`Zombie.Table.CfgShop`|True|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/MagicFaceView.cs:187`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/MagicFaceView.cs:192`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/MagicGranPrizeView.cs:142`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/MagicGranPrizeView.cs:147`

## tbmainactivity

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMainActivity.cs`；行类型：`Zombie.Table.CfgMainActivity`；条目：20。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|Type|`Zombie.Table.activity.ActivityId`|False|
|Icon|`System.String`|False|
|Effect|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMainActivitiesCom.cs:210`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMainActivitiesCom.cs:212`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMainActivitiesCom.cs:240`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMainActivitiesCom.cs:242`

## tbmainfunctions

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMainFunctions.cs`；行类型：`Zombie.Table.CfgMainFunctions`；条目：8。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`Zombie.Table.UI.MainFunctionType`|False|
|Name|`System.String`|False|
|Icon|`System.String`|False|
|Feature|`Zombie.Table.Feature`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMainFunctionBtnsCom.cs:52`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMainFunctionBtnsCom.cs:54`

## tbmanorbattle

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbManorBattle.cs`；行类型：`Zombie.Table.CfgManorBattle`；条目：500。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|ShortName|`System.String`|False|
|Name|`System.String`|False|
|RewardItem|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Type|`System.Int32`|False|
|Pop|`System.Int32`|False|
|Attribute|`System.Collections.Generic.List`1[Zombie.Table.Common.Attribute]`|False|
|CombatPower|`System.Int64`|False|
|RecommendPower|`System.Int64`|False|
|Kill|`System.Int32`|False|
|Map|`System.String`|False|
|MaxWarLevelID|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ManorProxy.cs:522`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorMainView.cs:405`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorUpLvView.cs:122`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorWarBattleEndView.cs:53`

## tbmanorcardgift

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbManorCardGift.cs`；行类型：`Zombie.Table.CfgManorCardGift`；条目：2。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`Zombie.Table.pay.PayType`|False|
|TitleIcon|`System.String`|False|
|StyleIcon|`System.String`|False|
|DescIcon|`System.String`|False|
|DescStr|`System.String`|False|
|LvStr|`System.String`|False|

代码消费样本：

## tbmanorchat

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbManorChat.cs`；行类型：`Zombie.Table.CfgManorChat`；条目：42。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Type|`System.Boolean`|False|
|Content|`System.String`|False|
|Page|`System.Int32`|False|
|PageName|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorChatView.cs:49`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorMainView.cs:321`

## tbmanorclothes

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbManorClothes.cs`；行类型：`Zombie.Table.CfgManorClothes`；条目：6。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Type|`Zombie.Table.manor.Clothes`|False|
|Name|`System.String`|False|
|Icon|`System.String`|False|
|Model|`System.String`|False|
|CollectAttrs|`Zombie.Table.Common.Attribute[]`|False|
|SetAttrs|`Zombie.Table.Common.Attribute[]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ManorResourceEntity.cs:140`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorAirplaneCom.cs:71`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorQueueView.cs:178`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorResWorkerView.cs:303`

## tbmanorfunction

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbManorFunction.cs`；行类型：`Zombie.Table.CfgManorFunction`；条目：5。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`Zombie.Table.manor.FunctionType`|False|
|Name|`System.String`|False|
|Icon|`System.String`|False|
|Feature|`Zombie.Table.Feature`|False|
|Position|`System.Boolean`|False|
|OnlySelf|`System.Boolean`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorMainView.cs:571`

## tbmanorglobal

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbManorGlobal.cs`；行类型：`Zombie.Table.CfgManorGlobal`；条目：1。

|字段|类型|生成引用字段|
|---|---|---|
|ResMax|`System.Int32`|False|
|ResProtect|`System.Int32`|False|
|RecordMax|`System.Int32`|False|
|RefreshItem|`System.Int32`|False|
|RefreshItem_Ref|`Zombie.Table.CfgItem`|True|
|RefreshCostA|`System.Single`|False|
|RefreshItemPrice|`System.Int32`|False|
|DailyAdFreeRefreshCount|`System.Int32`|False|
|FreeRefreshInterval|`System.Int32`|False|
|StrangerFirst|`Zombie.Table.ValueIntRange`|False|
|StrangerSecond|`Zombie.Table.ValueIntRange`|False|
|StrangerThird|`Zombie.Table.ValueIntRange`|False|
|LootOpenLevelLimit|`System.Int32`|False|
|StopRefreshOfflineTime|`System.Int32`|False|
|TopEnemyQuantitativeLimitation|`System.Int32`|False|
|EnemyExpired|`System.Int32`|False|
|ExhaustionCheck|`System.Collections.Generic.List`1[Zombie.Table.manor.ResourceIdRange]`|False|
|Resources|`System.Collections.Generic.List`1[System.Int32]`|False|
|LvResources|`System.Collections.Generic.List`1[System.Int32]`|False|
|Fatigue|`System.Int32`|False|
|FatigueRange|`System.Collections.Generic.List`1[Zombie.Table.Common.CommonId]`|False|
|FatigueTxt|`System.Collections.Generic.List`1[System.String]`|False|
|SuperRefreshItem|`System.Int32`|False|
|SuperRefreshItem_Ref|`Zombie.Table.CfgItem`|True|
|ResAutoRefresh|`System.Int32`|False|
|DefaultWorkerTemplate|`System.Int32`|False|
|DefaultWorkerTemplate_Ref|`Zombie.Table.CfgManorWorkers`|True|
|BattleCardRate|`System.Int32`|False|
|BattleCardLimit|`System.Int32`|False|
|PackageList|`System.Int32[]`|False|
|PackageList_Ref|`Zombie.Table.CfgPay[]`|True|
|HelpLimitTime|`System.Int32`|False|
|ResHelpMax|`System.Int32`|False|
|DailyAdFreeRefreshGapTime|`System.Int32`|False|
|MaxPullSpeedParameter|`System.Int32`|False|
|SpeedMax|`System.Single`|False|
|SpeedMin|`System.Single`|False|
|CarryResourceTrapTime|`System.Int32`|False|
|WorkerDefaultMaxLevel|`System.Int32`|False|
|CancelMinNumOfWorker|`System.Int32`|False|
|TalentBaseCoe|`System.Single`|False|
|InitWorkerNum|`System.Int32`|False|
|WokerExpItem|`System.Int32`|False|
|WokerExpItem_Ref|`Zombie.Table.CfgItem`|True|
|FatigueItem|`Zombie.Table.Common.ItemCostValue`|False|
|PowerCard|`System.Int32`|False|
|PowerCard_Ref|`Zombie.Table.CfgItem`|True|
|KickCard|`System.Int32`|False|
|KickCard_Ref|`Zombie.Table.CfgItem`|True|
|InsertCard|`System.Int32`|False|
|InsertCard_Ref|`Zombie.Table.CfgItem`|True|
|SeatCard|`System.Int32`|False|
|SeatCard_Ref|`Zombie.Table.CfgItem`|True|
|WarCard|`System.Int32`|False|
|WarCard_Ref|`Zombie.Table.CfgItem`|True|
|BigSeatCard|`System.Int32`|False|
|BigSeatCard_Ref|`Zombie.Table.CfgItem`|True|
|InsertGoodId|`Zombie.Table.Common.ShopShowPrice`|False|
|HolderGoodId|`Zombie.Table.Common.ShopShowPrice`|False|
|PowerGoodId|`Zombie.Table.Common.ShopShowPrice`|False|
|KickGoodId|`Zombie.Table.Common.ShopShowPrice`|False|
|AirplaneTimes|`System.Collections.Generic.List`1[System.Int32]`|False|
|KickCardProtect|`System.Int32`|False|
|HelperLimit|`System.Int32`|False|
|InsertCardProtect|`System.Int32`|False|
|InsertCardRestLimit|`System.Int32`|False|
|KickCardRestLimit|`System.Int32`|False|
|InsertNoProtectTime|`System.Int32`|False|
|ResChallengeDuration|`System.Int32`|False|
|InsertCardIncognitoModeCost|`System.Int32`|False|
|VisitorTimeout|`System.Int32`|False|
|AirPlaneOpenLevelLimit|`System.Int32`|False|
|ChatTimes|`System.Int32`|False|
|SearchTimes|`System.Int32`|False|
|BigSeatCardLimit|`System.Int32`|False|
|InitClothes|`System.Collections.Generic.Dictionary`2[Zombie.Table.manor.Clothes,System.Int32]`|False|
|AirplaneLimit|`System.Int32`|False|
|BattleHelpTimesLimit|`System.Int32`|False|
|BattleRecordLimit|`System.Int32`|False|
|BattlePlayerNum|`System.Int32`|False|
|BattleWaitTime|`System.Int32`|False|
|BattleMaxRound|`System.Int32`|False|
|BattlePartyShareParam|`Zombie.Table.Common.WxShareParam`|False|
|StoneOutputRange|`Zombie.Table.ValueRange[]`|False|
|StoneOutputLvLimit|`System.Int32`|False|
|CollectHelpShareParam|`Zombie.Table.Common.WxShareParam`|False|
|RobLowDecay|`Zombie.Table.Common.MinMaxValue[]`|False|
|RobHighBonus|`Zombie.Table.Common.MinMaxValue[]`|False|
|NoDiffYieldTypes|`Zombie.Table.manor.ResourceType[]`|False|
|ShopTypes|`Zombie.Table.shop.ShopType[]`|False|
|SmallHolderCard|`System.String`|False|
|BigHolderCard|`System.String`|False|
|WokerrCallGuideID|`System.Int32`|False|
|WorkerAutoLockQuality|`System.Int32`|False|
|InitManorBattleID|`System.Int32`|False|
|InitManorBattleID_Ref|`Zombie.Table.CfgManorBattle`|True|
|InitManorResource|`System.Collections.Generic.List`1[Zombie.Table.manor.InitResource]`|False|
|StrangerRange|`System.Int32`|False|
|LogJump|`System.Collections.Generic.HashSet`1[System.String]`|False|
|BuyWokerNumCost|`Zombie.Table.Common.ItemCostValue[]`|False|
|ResChallengeDefaultPosition|`System.Int32`|False|
|EqualPowerSpeed|`System.Single`|False|
|WorkerFetterReward|`Zombie.Table.item.IdValue`|False|
|WorkerPowerRankMinReq|`System.Int32`|False|
|BattleLevelRankMinReq|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ManorProxy.cs:69`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ManorWarProxy.cs:54`

## tbmanorlv

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbManorLv.cs`；行类型：`Zombie.Table.CfgManorLv`；条目：100。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|NeedItem|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|ResourceLimitLevel|`System.Collections.Generic.Dictionary`2[Zombie.Table.manor.ResourceType,Zombie.Table.manor.OutResourceLvWeight]`|False|
|BuyWokerLimit|`System.Int32`|False|
|BossLevelLimit|`System.Int32`|False|
|NeedItem2|`Zombie.Table.item.IdValue`|False|
|WorkerMaxLv|`System.Int32`|False|
|TalentLimit|`System.Single`|False|
|BigSeatCardResource|`System.Int32`|False|
|BigSeatCardResource_Ref|`Zombie.Table.CfgManorResource`|True|
|SuperCard|`System.Int32`|False|
|SuperCard_Ref|`Zombie.Table.CfgItem`|True|
|SuperCardResource|`System.Collections.Generic.List`1[Zombie.Table.manor.RefreshResource]`|False|
|GiftLv|`System.Int32`|False|
|GiftDesc|`System.String`|False|
|GiftInfo|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ManorProxy.cs:512`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ManorProxy.cs:536`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ManorProxy.cs:1140`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ManorProxy.cs:1246`

## tbmanorresource

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbManorResource.cs`；行类型：`Zombie.Table.CfgManorResource`；条目：481。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|Desc|`System.String`|False|
|Icon|`System.String`|False|
|LootIcon|`System.String`|False|
|SpecialIcon|`System.String`|False|
|Type|`Zombie.Table.manor.ResourceType`|False|
|Level|`System.Int32`|False|
|DropItemNum|`System.Collections.Generic.List`1[Zombie.Table.Common.ItemNumRange]`|False|
|HelpCombatPower|`System.Int32`|False|
|SpeedWeight|`System.Int32`|False|
|MaxWorkerCount|`System.Int32`|False|
|Quality|`Zombie.Table.QualityType`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ManorProxy.cs:1304`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ManorResourceEntity.cs:117`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorAirplaneView.cs:84`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorCollectHelpBubble.cs:47`

## tbmanorworkerfetter

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbManorWorkerFetter.cs`；行类型：`Zombie.Table.CfgManorWorkerFetter`；条目：10。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|WorkerId|`System.Collections.Generic.List`1[System.Int32]`|False|
|WorkerId_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgManorWorkers]`|True|
|EffectType|`System.Int32`|False|
|EffectType_Ref|`Zombie.Table.CfgAttribute`|True|
|EffectCoeA|`System.Single`|False|
|Desc|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorWorkerGuideBookView.cs:114`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorWorkerGuideBookView.cs:118`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorWorkerGuideBookView.cs:154`

## tbmanorworkerquality

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbManorWorkerQuality.cs`；行类型：`Zombie.Table.CfgManorWorkerQuality`；条目：7。

|字段|类型|生成引用字段|
|---|---|---|
|Quality|`Zombie.Table.QualityType`|False|
|Name|`System.String`|False|
|UpCoeA|`System.Single`|False|
|UpCoeB|`System.Single`|False|
|UpCoeC|`System.Single`|False|
|HelpCoeA|`System.Single`|False|
|HelpCoeB|`System.Single`|False|
|HelpCoeC|`System.Single`|False|
|UpgradeCeoA|`System.Single`|False|
|UpgradeCoeB|`System.Single`|False|
|BaseExp|`System.Single`|False|
|BaseCoe|`System.Single`|False|
|BottomCoe|`System.Single`|False|
|TopCoe|`System.Single`|False|
|ShowCoe|`System.Single`|False|
|BreakExp|`Zombie.Table.item.IdValue`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ManorProxy.cs:981`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ManorProxy.cs:1215`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorWorkerGuideBookView.cs:110`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorWorkerInfoView.cs:649`

## tbmanorworkers

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbManorWorkers.cs`；行类型：`Zombie.Table.CfgManorWorkers`；条目：21。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|Quality|`Zombie.Table.QualityType`|False|
|Icon|`System.String`|False|
|Head|`System.String`|False|
|Model|`System.String`|False|
|ModelOffset|`Zombie.Table.Common.Position`|False|
|ModelScale|`System.Int32`|False|
|VerticalPivot|`System.Single`|False|
|TalentInitRanges|`Zombie.Table.Common.MinMaxFloat`|False|
|HelpCoe|`System.Single`|False|
|PersonNum|`System.Int32`|False|
|UnLockReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Desc|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ManorProxy.cs:504`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ManorProxy.cs:980`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ManorProxy.cs:1052`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ManorProxy.cs:1215`

## tbmessagesubscribe

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMessageSubscribe.cs`；行类型：`Zombie.Table.CfgMessageSubscribe`；条目：2。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Title|`System.String`|False|
|Desc|`System.String`|False|
|Page|`System.String`|False|
|TemplateId|`System.String`|False|
|Fields|`System.String[]`|False|
|TtTemplateId|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIWXSubscriptionSettingView.cs:59`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIWXSubscriptionSettingView.cs:73`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIWXSubscriptionSettingView.cs:79`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIWXSubscriptionSettingView.cs:92`

## tbminewarfamilybox

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMineWarFamilyBox.cs`；行类型：`Zombie.Table.CfgMineWarFamilyBox`；条目：2。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|SuperCart|`System.Boolean`|False|
|AttackNum|`System.Int32`|False|
|DefendNum|`System.Int32`|False|
|JoinReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|WinReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|AttackFailReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|AttackWinReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Icon|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/MineWarBox.cs:138`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/MineWarProxy.cs:80`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIChatContentPlayerShareMineBox.cs:43`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIChatContentSystemShareMineBox.cs:39`

## tbminewarglobal

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMineWarGlobal.cs`；行类型：`Zombie.Table.CfgMineWarGlobal`；条目：1。

|字段|类型|生成引用字段|
|---|---|---|
|PersonalScore|`System.Int32`|False|
|PersonalScore_Ref|`Zombie.Table.CfgItem`|True|
|FamilyScore|`System.Int32`|False|
|FamilyScore_Ref|`Zombie.Table.CfgItem`|True|
|RewardPreview|`System.Int32[]`|False|
|FamilyLevelReq|`System.Int32`|False|
|FamilyMemberReq|`System.Int32`|False|
|PlayerLevelReq|`System.Int32`|False|
|FamilyRankReq|`System.Int32`|False|
|PersonRankRewardLv|`System.Int32[]`|False|
|NightStartHour|`System.String`|False|
|NightEndHour|`System.String`|False|
|NightSpeedReduceRatio|`System.Int32`|False|
|NightCartRefreshEnabled|`System.Boolean`|False|
|NightStaminaRecoverEnabled|`System.Boolean`|False|
|NightAttackRecoverEnabled|`System.Boolean`|False|
|FatigueAdd|`System.Int32`|False|
|FatigueMax|`System.Int32`|False|
|FatigueDesc|`System.String[][]`|False|
|SuperCartPerStamina|`System.Int32`|False|
|SuperCartMax|`System.Int32`|False|
|SuperCartOpenPermission|`System.Collections.Generic.List`1[System.Int32]`|False|
|SuperCartOpenPermission_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgFamilyPermission]`|True|
|MineWarProtectTime|`System.Int32`|False|
|MineWarProtector|`Zombie.Table.Common.ItemDayRefresh`|False|
|MineWarStamina|`Zombie.Table.Common.ItemAndPrivilegeRefresh`|False|
|MineWarAttack|`Zombie.Table.Common.ItemAndPrivilegeRefresh`|False|
|MineWarOrdinaryMaxVelocity|`System.Collections.Generic.List`1[System.Single]`|False|
|MineWarOrdinaryMinVelocity|`System.Single`|False|
|MineWarFamilyMaxVelocity|`System.Single`|False|
|MineWarFamilyMinVelocity|`System.Single`|False|
|MineWarBoxRefreshCD|`System.Int32`|False|
|MineWarSearchRefreshCD|`System.Int32`|False|
|MineWarSearchRefreshCost|`Zombie.Table.item.IdValue`|False|
|MineWarMonsterPowerCoefficient|`Zombie.Table.ValueRange`|False|
|Resources|`System.Int32[]`|False|
|LeaderBattleWaitTime|`System.Int32`|False|
|GroupRankEloK1|`System.Int32`|False|
|GroupRankEloK2|`System.Int32`|False|
|GroupInitMember|`System.Int32`|False|
|KayaMineItem|`System.Int32`|False|
|KayaMineItem_Ref|`Zombie.Table.CfgItem`|True|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/MineWarProxy.cs:13`

## tbminewarmilestone

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMineWarMilestone.cs`；行类型：`Zombie.Table.CfgMineWarMilestone`；条目：11。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|FamilyScore|`System.Int32`|False|
|FamilyReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|PersonScore|`System.Int32`|False|
|PerseonReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMineWarMilestoneView.cs:49`

## tbminewarpersonbox

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMineWarPersonBox.cs`；行类型：`Zombie.Table.CfgMineWarPersonBox`；条目：3。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|Quality|`System.Int32`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Icon|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/MineWarBox.cs:133`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMineWarBoxCom.cs:120`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMineWarNeighborView.cs:112`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMineWarPersonBoxView.cs:113`

## tbminewarrankfamily

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMineWarRankFamily.cs`；行类型：`Zombie.Table.CfgMineWarRankFamily`；条目：5。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Min|`System.Int32`|False|
|Max|`System.Int32`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMineWarRankRewardView.cs:110`

## tbminewarrankleader

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMineWarRankLeader.cs`；行类型：`Zombie.Table.CfgMineWarRankLeader`；条目：6。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Min|`System.Int32`|False|
|Max|`System.Int32`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMineWarLeaderView.cs:131`

## tbminewarrankperson

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMineWarRankPerson.cs`；行类型：`Zombie.Table.CfgMineWarRankPerson`；条目：15。

|字段|类型|生成引用字段|
|---|---|---|
|FamilyScoreLv|`System.Int32`|False|
|Min|`System.Int32`|False|
|Max|`System.Int32`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMineWarRankRewardView.cs:102`

## tbminewarscorelevel

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMineWarScoreLevel.cs`；行类型：`Zombie.Table.CfgMineWarScoreLevel`；条目：25。

|字段|类型|生成引用字段|
|---|---|---|
|Level|`System.Int32`|False|
|Score|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMineWarMainView.cs:350`

## tbmonster

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMonster.cs`；行类型：`Zombie.Table.CfgMonster`；条目：15017。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|MonsterType|`Zombie.Table.MonsterType`|False|
|Column|`System.Int32`|False|
|MonsterQuality|`Zombie.Table.MonsterQuality`|False|
|Age|`System.Int32`|False|
|BaseAttributeRange|`System.Collections.Generic.List`1[Zombie.Table.Common.AttributeRange]`|False|
|Gold|`System.Int32`|False|
|Fame|`System.Int32`|False|
|Exp|`System.Int32`|False|
|Model|`System.Collections.Generic.List`1[System.String]`|False|
|Skill|`System.Collections.Generic.List`1[System.Int32]`|False|
|Skill_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgHeroSkill]`|True|
|SoundID|`System.Int32`|False|
|SoundID_Ref|`Zombie.Table.CfgMonsterSound`|True|
|MonsterLevel|`System.Int32`|False|
|Head|`System.String`|False|
|DropItem|`System.Collections.Generic.List`1[Zombie.Table.item.IdValueRate]`|False|
|AttackPlayId|`System.Int32`|False|
|Power|`System.Int64`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Battle/BattleFamilyBossEntity.cs:10`
- `research/code-study/managed/decompiled/HotUpdate/Battle/BattleGroupBossEntity.cs:13`
- `research/code-study/managed/decompiled/HotUpdate/Battle/BattleMonsterEntity.cs:18`
- `research/code-study/managed/decompiled/HotUpdate/Dungeon/GroupBattleController.cs:335`

## tbmonstergroup

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMonsterGroup.cs`；行类型：`Zombie.Table.CfgMonsterGroup`；条目：3863。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Monster|`System.Collections.Generic.List`1[Zombie.Table.NormalWeight]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UILevelConditionView.cs:56`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UILevelConditionView.cs:67`

## tbmonstersound

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMonsterSound.cs`；行类型：`Zombie.Table.CfgMonsterSound`；条目：12。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|AttackSound|`System.Collections.Generic.List`1[System.String]`|False|
|AttackVolume|`System.Single`|False|
|BlockSound|`System.Collections.Generic.List`1[System.String]`|False|
|BlockVolume|`System.Single`|False|
|CritSound|`System.Collections.Generic.List`1[System.String]`|False|
|CritVolume|`System.Single`|False|
|DieSound|`System.Collections.Generic.List`1[System.String]`|False|
|DieVolume|`System.Single`|False|
|EvadeSound|`System.Collections.Generic.List`1[System.String]`|False|
|EvadeVolume|`System.Single`|False|
|MissSound|`System.Collections.Generic.List`1[System.String]`|False|
|MissVolume|`System.Single`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Battle/BattleFamilyBossEntity.cs:11`
- `research/code-study/managed/decompiled/HotUpdate/Battle/BattleGroupBossEntity.cs:14`
- `research/code-study/managed/decompiled/HotUpdate/Battle/BattleMonsterEntity.cs:19`

## tbmysticrealmchoice

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMysticRealmChoice.cs`；行类型：`Zombie.Table.CfgMysticRealmChoice`；条目：300。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Title|`System.String`|False|
|Desc|`System.String`|False|
|Choice|`Zombie.Table.manor.NodeChoice[]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorDecisionView.cs:42`

## tbmysticrealmfixlayout

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMysticRealmFixLayout.cs`；行类型：`Zombie.Table.CfgMysticRealmFixLayout`；条目：1129。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Node|`Zombie.Table.manor.MysticRealmNodeType[]`|False|

代码消费样本：

## tbmysticrealmmain

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMysticRealmMain.cs`；行类型：`Zombie.Table.CfgMysticRealmMain`；条目：100。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorDungeonBackpackView.cs:58`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorDungeonMainView.cs:179`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorDungeonMainView.cs:308`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorDungeonOverviewView.cs:53`

## tbmysticrealmrandomlayout

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMysticRealmRandomLayout.cs`；行类型：`Zombie.Table.CfgMysticRealmRandomLayout`；条目：143。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|NodeNum|`System.Int32`|False|
|Nodes|`System.Collections.Generic.List`1[Zombie.Table.manor.RandomNodeWeight]`|False|

代码消费样本：

## tbmysticrealmresource

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMysticRealmResource.cs`；行类型：`Zombie.Table.CfgMysticRealmResource`；条目：1000。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|NodeData|`Zombie.Table.manor.MysticRealmNode[]`|False|

代码消费样本：

## tbmysticrealmscene

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMysticRealmScene.cs`；行类型：`Zombie.Table.CfgMysticRealmScene`；条目：1000。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|MainId|`System.Int32`|False|
|MainId_Ref|`Zombie.Table.CfgMysticRealmMain`|True|
|Level|`System.Int32`|False|
|Type|`Zombie.Table.manor.MysticRealmLayoutType`|False|
|LayoutEffect|`Zombie.Table.manor.MysticRealmLayout`|False|
|ResourceId|`System.Int32`|False|
|ResourceId_Ref|`Zombie.Table.CfgMysticRealmResource`|True|
|BattleId|`System.Int32`|False|
|BattleId_Ref|`Zombie.Table.CfgBattle`|True|
|BattleNeedPower|`System.Nullable`1[System.Int32]`|False|

代码消费样本：

## tbmysticrealmworker

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMysticRealmWorker.cs`；行类型：`Zombie.Table.CfgMysticRealmWorker`；条目：21。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|ID_Ref|`Zombie.Table.CfgManorWorkers`|True|
|Ap|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorDungeonWorkerEventView.cs:73`

## tbmysticrealmworkerprobabilitypreview

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbMysticRealmWorkerProbabilityPreview.cs`；行类型：`Zombie.Table.CfgMysticRealmWorkerProbabilityPreview`；条目：10。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|MinDiff|`System.Int32`|False|
|MaxDiff|`System.Int32`|False|
|Probability|`System.Collections.Generic.List`1[Zombie.Table.manor.WorkerProbabilityPreview]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorDungeonStartView.cs:162`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorDungeonWorkerPreviewView.cs:33`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorDungeonWorkerPreviewView.cs:42`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorDungeonWorkerPreviewView.cs:44`

## tbnewserverrankmain

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbNewServerRankMain.cs`；行类型：`Zombie.Table.CfgNewServerRankMain`；条目：4。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Threshold|`System.Int32`|False|
|PreView|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UINewRankMainCom.cs:119`

## tbopenstory

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbOpenStory.cs`；行类型：`Zombie.Table.CfgOpenStory`；条目：4。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Txt|`System.String`|False|
|Bg|`System.String`|False|

代码消费样本：

## tbordertaskcontent

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbOrderTaskContent.cs`；行类型：`Zombie.Table.CfgOrderTaskContent`；条目：175。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|ID_Ref|`Zombie.Table.CfgTask`|True|
|NPCName|`System.String`|False|
|NPCHead|`System.String`|False|
|TargetDesc|`System.String`|False|
|TaskDesc1|`System.String`|False|
|TaskDesc2|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIOrderTaskView.cs:144`

## tbpay

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbPay.cs`；行类型：`Zombie.Table.CfgPay`；条目：195。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|Description|`System.String`|False|
|PayType|`Zombie.Table.pay.PayType`|False|
|Discount|`System.Int32`|False|
|Effect|`Zombie.Table.pay.PayEffect`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Tag|`System.String`|False|
|Slogan|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ActivityPackCom.cs:109`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ChargeGiftListCell.cs:55`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PushCommonItemCom.cs:64`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PushGiftLimitCom.cs:83`

## tbpet

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbPet.cs`；行类型：`Zombie.Table.CfgPet`；条目：17。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|Icon|`System.String`|False|
|Model|`System.String`|False|
|Quality|`Zombie.Table.QualityType`|False|
|StarLimit|`System.Int32`|False|
|Attribute|`System.Collections.Generic.List`1[Zombie.Table.Common.Attribute]`|False|
|LevelParam|`System.Int32`|False|
|SourceDesc|`System.String`|False|
|AttributeFactor|`System.Collections.Generic.Dictionary`2[System.Int32,System.Int32]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PetProxy.cs:56`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PetProxy.cs:255`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PetProxy.cs:293`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIBatchDecomposeView.cs:96`

## tbpetdraw

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbPetDraw.cs`；行类型：`Zombie.Table.CfgPetDraw`；条目：1。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|PoolName|`System.String`|False|
|Icon|`System.String`|False|
|Banner|`System.String`|False|
|Bg|`System.String`|False|
|Type|`System.Int32`|False|
|CostID|`System.Int32`|False|
|CostNum|`System.Int32`|False|
|Name|`System.String`|False|
|Prob|`Zombie.Table.item.IdValue[]`|False|
|FreeTimes|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PetProxy.cs:21`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PetProxy.cs:25`

## tbpetlv

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbPetLv.cs`；行类型：`Zombie.Table.CfgPetLv`；条目：720。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Quality|`Zombie.Table.QualityType`|False|
|Lv|`System.Int32`|False|
|Upgrade|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Attribute|`System.Collections.Generic.List`1[Zombie.Table.Common.Attribute]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PetProxy.cs:96`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PetProxy.cs:262`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PetProxy.cs:298`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIPetDetailAndUpgradeView.cs:233`

## tbpetstar

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbPetStar.cs`；行类型：`Zombie.Table.CfgPetStar`；条目：303。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|PetID|`System.Int32`|False|
|StarLv|`System.Int32`|False|
|UpgradeLimit|`System.Int32`|False|
|Skill|`System.Collections.Generic.List`1[System.Int32]`|False|
|Skill_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgHeroSkill]`|True|
|Material|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PetProxy.cs:266`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIPetChangeView.cs:81`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIPetDeployView.cs:62`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIPetDetailAndUpgradeView.cs:111`

## tbplaymode

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbPlayMode.cs`；行类型：`Zombie.Table.CfgPlayMode`；条目：5。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Type|`Zombie.Table.PlayModeType`|False|
|Name|`System.String`|False|
|Banner|`System.String`|False|
|Unlock|`Zombie.Table.Feature`|False|
|Rewards|`System.Collections.Generic.List`1[System.Int32]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIPlayModeView.cs:68`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIPlayModeView.cs:85`

## tbprivilegecard

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbPrivilegeCard.cs`；行类型：`Zombie.Table.CfgPrivilegeCard`；条目：8。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Day|`System.Int32`|False|
|ActivityId|`Zombie.Table.activity.ActivityId`|False|
|Name|`System.String`|False|
|Description|`System.String`|False|
|Banner|`System.String`|False|
|Bg|`System.String`|False|
|DailyReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMallPrivilegeCardCom.cs:93`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIMallSupplyCardCom.cs:123`

## tbproficiencyexpreward

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbProficiencyExpReward.cs`；行类型：`Zombie.Table.CfgProficiencyExpReward`；条目：105。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|EquipmentShopType|`Zombie.Table.EquipmentShopType`|False|
|Exp|`System.Int32`|False|
|Reward|`Zombie.Table.item.IdValue`|False|
|Level|`System.Int32`|False|
|LevelTitle|`System.String`|False|
|LevelDesc|`System.String`|False|
|RewardTitle|`System.String`|False|
|RewardDesc|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ShopProficiencyItem.cs:19`

## tbprogresstask

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbProgressTask.cs`；行类型：`Zombie.Table.CfgProgressTask`；条目：157。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|ProgressItemID|`System.Int32`|False|
|ProgressValue|`System.Int32`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|BoxIcon|`System.String`|False|
|HightCondition|`Zombie.Table.Common.UnlockCondition`|False|
|HightReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ActivityTaskCom.cs:103`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/LuckyWheelActivityRedDotHelper.cs:20`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/LuckyWheelMainCom.cs:51`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/LuckyWheelMainCom.cs:412`

## tbpushgiftpack

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbPushGiftPack.cs`；行类型：`Zombie.Table.CfgPushGiftPack`；条目：2。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|TriggerCondition|`Zombie.Table.push.TriggerCondition[]`|False|
|CommonCondiotn|`Zombie.Table.push.TriggerCondition[]`|False|
|Close|`System.Boolean`|False|
|Duration|`System.Int32`|False|
|Type|`System.Int32`|False|
|Banner|`System.String`|False|
|Icon|`System.String`|False|
|BgName|`System.String`|False|
|Slogan|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PushGiftHeroCom.cs:39`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PushGiftLimitCom.cs:82`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PushGiftPackProxy.cs:84`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/PushGiftPackProxy.cs:112`

## tbrank

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbRank.cs`；行类型：`Zombie.Table.CfgRank`；条目：115。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|RankType|`Zombie.Table.Common.RankType`|False|
|RepeatType|`Zombie.Table.Common.RepeatType`|False|
|Min|`System.Int32`|False|
|Max|`System.Int32`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UINewRankPreviewView.cs:185`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIPvPArenaRewardView.cs:52`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRankMainView.cs:191`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRankRewardView.cs:56`

## tbrankpvpreward

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbRankPvpReward.cs`；行类型：`Zombie.Table.CfgRankPvpReward`；条目：500。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|HistoryRank|`System.Int32`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：

## tbrankpvprobot

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbRankPvpRobot.cs`；行类型：`Zombie.Table.CfgRankPvpRobot`；条目：1053。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|Head|`System.Int32`|False|
|Head_Ref|`Zombie.Table.CfgHead`|True|
|Body|`System.Int32`|False|
|Body_Ref|`Zombie.Table.CfgBody`|True|
|Border|`System.Int32`|False|
|Border_Ref|`Zombie.Table.CfgBorder`|True|
|Heros|`System.Collections.Generic.List`1[System.Int32]`|False|
|Heros_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgRankPvpRobotHero]`|True|
|Scope|`System.Int32`|False|

代码消费样本：

## tbrankpvprobothero

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbRankPvpRobotHero.cs`；行类型：`Zombie.Table.CfgRankPvpRobotHero`；条目：1914。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Hero|`System.Int32`|False|
|Hero_Ref|`Zombie.Table.CfgHero`|True|
|Lv|`System.Int32`|False|
|Star|`System.Int32`|False|
|Attribute|`System.Collections.Generic.List`1[Zombie.Table.Common.Attribute]`|False|
|Power|`System.Int32`|False|

代码消费样本：

## tbrankshow

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbRankShow.cs`；行类型：`Zombie.Table.CfgRankShow`；条目：9。

|字段|类型|生成引用字段|
|---|---|---|
|RankType|`Zombie.Table.Common.RankType`|False|
|Name|`System.String`|False|
|TabIcon|`System.String`|False|
|IsWorld|`System.Boolean`|False|
|HaveReward|`System.Boolean`|False|
|RewardRedDot|`System.Int32`|False|
|RepeatType|`Zombie.Table.Common.RepeatType`|False|
|NoRankReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Tips|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRankMainView.cs:140`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRankMainView.cs:168`

## tbreddot

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbRedDot.cs`；行类型：`Zombie.Table.CfgRedDot`；条目：141。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Parent|`System.Int32`|False|
|Register2Server|`System.Boolean`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/RedDotManager.cs:149`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/RedDotManager.cs:154`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/RedDotProxy.cs:20`

## tbrelic

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbRelic.cs`；行类型：`Zombie.Table.CfgRelic`；条目：46。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|RelicName|`System.String`|False|
|Icon|`System.String`|False|
|Quality|`Zombie.Table.RelicQualityType`|False|
|FragNum|`Zombie.Table.item.IdValue`|False|
|CompoundSFX|`System.String`|False|
|RelicDes|`System.String`|False|
|BasicStrength|`System.Int32`|False|
|MaxLv|`System.Int32`|False|
|MaxStar|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/RelicProxy.cs:112`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/RelicProxy.cs:123`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/RelicProxy.cs:145`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/RelicProxy.cs:392`

## tbreliclevelup

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbRelicLevelUp.cs`；行类型：`Zombie.Table.CfgRelicLevelUp`；条目：1426。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|ResonanceLevel|`System.Int32`|False|
|LevelUpConsumption|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|PassiveEffectBonus|`System.Collections.Generic.List`1[Zombie.Table.Common.Attribute]`|False|
|StrengthBonus|`System.Int32`|False|
|Revert|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|RelicLevelUpCondition|`Zombie.Table.Common.UnlockCondition`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/RelicProxy.cs:150`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UINewRelicView.cs:46`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRelicDetailView.cs:223`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRelicDetailView.cs:328`

## tbrelicmaterialconvert

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbRelicMaterialConvert.cs`；行类型：`Zombie.Table.CfgRelicMaterialConvert`；条目：3。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|ID_Ref|`Zombie.Table.CfgItem`|True|
|RelicIds|`System.Collections.Generic.List`1[System.Int32]`|False|
|RelicIds_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgRelic]`|True|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UICommonItemBubble.cs:86`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRelicDetailView.cs:285`

## tbrelicset

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbRelicSet.cs`；行类型：`Zombie.Table.CfgRelicSet`；条目：16。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|SetName|`System.String`|False|
|NeedRelic|`System.Int32[]`|False|
|NeedRelic_Ref|`Zombie.Table.CfgRelic[]`|True|
|UnlockCondition|`Zombie.Table.Common.UnlockCondition`|False|
|UnlockStarAttribute|`System.Collections.Generic.Dictionary`2[System.Int32,System.Collections.Generic.List`1[Zombie.Table.Common.Attribute]]`|False|
|UnlockDesc|`System.Collections.Generic.Dictionary`2[System.Int32,System.Collections.Generic.List`1[System.String]]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRelicSetCom.cs:29`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRelicSetDetailView.cs:51`

## tbrelicstarup

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbRelicStarUp.cs`；行类型：`Zombie.Table.CfgRelicStarUp`；条目：276。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|StarTier|`System.Int32`|False|
|StarUpConsumption|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|StarEffects|`System.Collections.Generic.List`1[Zombie.Table.Common.Attribute]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/RelicProxy.cs:128`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRelicDetailView.cs:253`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRelicDetailView.cs:354`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRelicDetailView.cs:363`

## tbrobot

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbRobot.cs`；行类型：`Zombie.Table.CfgRobot`；条目：3。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|Name|`System.String`|False|
|Hero|`System.Collections.Generic.List`1[Zombie.Table.robot.HeroData]`|False|
|Task|`System.Int32`|False|
|Task_Ref|`Zombie.Table.CfgTask`|True|
|Battle|`System.Int32`|False|
|Battle_Ref|`Zombie.Table.CfgBattle`|True|
|Dungeon|`System.Int32`|False|
|Dungeon_Ref|`Zombie.Table.CfgDungeon`|True|
|GuildLevel|`System.Int32`|False|
|GuildLevel_Ref|`Zombie.Table.CfgGuildLevel`|True|
|Building|`System.Collections.Generic.List`1[Zombie.Table.robot.BuildingData]`|False|
|Equipment|`System.Collections.Generic.List`1[Zombie.Table.robot.HeroEquipmentData]`|False|
|Item|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Statue|`System.Collections.Generic.List`1[System.Int32]`|False|
|Statue_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgStatuePart]`|True|
|Totem|`System.Collections.Generic.List`1[System.Int32]`|False|
|Totem_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgTotem]`|True|
|Pet|`System.Collections.Generic.List`1[Zombie.Table.robot.PetData]`|False|
|Aura|`System.Int32`|False|
|Book|`System.Collections.Generic.List`1[Zombie.Table.robot.BookData]`|False|
|Relic|`System.Collections.Generic.List`1[Zombie.Table.robot.RelicData]`|False|
|Feature|`System.Collections.Generic.List`1[Zombie.Table.Feature]`|False|

代码消费样本：

## tbshop

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbShop.cs`；行类型：`Zombie.Table.CfgShop`；条目：258。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|ShopType|`Zombie.Table.shop.ShopType`|False|
|GoodName|`System.String`|False|
|Goods|`Zombie.Table.item.IdValue`|False|
|Price|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Sort|`System.Int32`|False|
|Limit|`System.Int32`|False|
|Discount|`System.Single`|False|
|Desc|`System.String`|False|
|RepeatType|`Zombie.Table.Common.RepeatType`|False|
|UnlockCondition|`Zombie.Table.Common.UnlockCondition`|False|
|LockHide|`System.Boolean`|False|
|Group|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ShopProxy.cs:199`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UICardExchangeView.cs:107`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIConvoyShopView.cs:144`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIManorShopView.cs:260`

## tbshopshow

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbShopShow.cs`；行类型：`Zombie.Table.CfgShopShow`；条目：2。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|ShopType|`Zombie.Table.shop.ShopType`|False|
|Feature|`Zombie.Table.Feature`|False|
|Name|`System.String`|False|
|Icon|`System.String`|False|
|Resources|`System.Int32[]`|False|
|Component|`System.String`|False|
|DescTitle|`System.String`|False|
|DescInfo|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIShopMainView.cs:87`

## tbskillplay

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbSkillPlay.cs`；行类型：`Zombie.Table.CfgSkillPlay`；条目：339。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Animations|`Zombie.Table.Play.Animation[]`|False|
|Particles|`Zombie.Table.Play.Particle[]`|False|
|ParticlePoint|`System.String`|False|
|Sounds|`Zombie.Table.Play.Sound[]`|False|
|HideModel|`Zombie.Table.Play.Hide`|False|
|HitTimes|`System.Single[]`|False|
|DamageTimes|`System.Single`|False|
|HitPosition|`Zombie.Table.Play.HitPosition`|False|
|HitPoint|`Zombie.Table.Play.HitPoint`|False|
|HitParticle|`Zombie.Table.Play.Particle`|False|
|AdditionalTime|`System.Single`|False|
|FireBullet|`System.Boolean`|False|
|BulletParam|`Zombie.Table.Play.BulletParam`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Battle/BattleCharacterEntity.cs:156`
- `research/code-study/managed/decompiled/HotUpdate/Battle/BattleCharacterEntity.cs:159`
- `research/code-study/managed/decompiled/HotUpdate/Battle/BattleCharacterEntity.cs:166`
- `research/code-study/managed/decompiled/HotUpdate/Battle/BattleCharacterEntity.cs:168`

## tbsoldier

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbSoldier.cs`；行类型：`Zombie.Table.CfgSoldier`；条目：30。

|字段|类型|生成引用字段|
|---|---|---|
|Id|`System.Int32`|False|
|Name|`System.String`|False|
|Type|`Zombie.Table.SoldierType`|False|
|Level|`System.Int32`|False|
|Attack|`System.Int32`|False|
|Defense|`System.Int32`|False|
|Hp|`System.Int32`|False|
|Carry|`System.Int32`|False|
|BuildingId|`System.Int32`|False|
|BuildingLevel|`System.Int32`|False|
|TrainNeedResource|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|TrainNeedTime|`System.Int32`|False|
|CombatPower|`System.Single`|False|

代码消费样本：

## tbstatuepart

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbStatuePart.cs`；行类型：`Zombie.Table.CfgStatuePart`；条目：2000。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Part|`Zombie.Table.Common.StatuePart`|False|
|Icon|`System.String`|False|
|Lv|`System.Int32`|False|
|Slot|`System.Int32`|False|
|Cost|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Attribute|`System.Collections.Generic.List`1[Zombie.Table.Common.Attribute]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/StatueProxy.cs:79`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/StatueProxy.cs:101`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/StatueProxy.cs:325`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRuneChangeView.cs:131`

## tbsummonpool

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbSummonPool.cs`；行类型：`Zombie.Table.CfgSummonPool`；条目：37。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|Lv|`System.Int32`|False|
|PoolId|`System.Int32`|False|
|DropID|`System.Int32`|False|
|Cost5|`Zombie.Table.item.IdValueSummon`|False|
|Cost30|`Zombie.Table.item.IdValueSummon`|False|
|Exp|`System.Int32`|False|
|Sort|`System.Int32`|False|
|Rate|`System.Collections.Generic.List`1[Zombie.Table.QualityRate]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/SummonProxy.cs:52`

## tbtask

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbTask.cs`；行类型：`Zombie.Table.CfgTask`；条目：1397。

|字段|类型|生成引用字段|
|---|---|---|
|TaskID|`System.Int32`|False|
|TaskName|`System.String`|False|
|Icon|`System.String`|False|
|TaskDescription|`System.String`|False|
|FinishConditionType|`Zombie.Table.task.FinishConditionType`|False|
|FinishConditionValues|`Zombie.Table.task.Condition`|False|
|FinishValue|`System.Int32`|False|
|ShowTips|`System.Boolean`|False|
|JumpType|`System.Int32`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|QuestValue|`System.Int32`|False|
|BuildingType|`Zombie.Table.BuildingType`|False|
|NPCTalkID|`System.Int32`|False|
|Priority|`System.Int32`|False|
|TipsDelayTime|`System.Int32`|False|
|GuideConversationDelayTime|`System.Int32`|False|
|GuideConversationID|`System.Int32`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ActivityTaskCom.cs:82`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingInteractiveModule.cs:566`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingInteractiveModule.cs:582`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ChargeTaskCom.cs:67`

## tbtaskorientedactivity

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbTaskOrientedActivity.cs`；行类型：`Zombie.Table.CfgTaskOrientedActivity`；条目：3。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|ActivityType|`System.Int32`|False|
|ShowHero|`System.Int32`|False|
|ShowRelic|`System.Int32`|False|
|TitleBG|`System.String`|False|
|BG|`System.String`|False|
|GiftTips|`System.String`|False|
|PopupTitle|`System.String`|False|
|PopupRewards|`System.Int32[]`|False|
|TaskField|`System.Collections.Generic.Dictionary`2[System.Int32,Zombie.Table.task.TaskList]`|False|
|ProgressBarField|`System.Collections.Generic.List`1[System.Int32]`|False|
|ProgressBarField_Ref|`System.Collections.Generic.List`1[Zombie.Table.CfgProgressTask]`|True|
|ProgressBarDescription|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UITaskActivityView.cs:440`

## tbtaskunlock

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbTaskUnlock.cs`；行类型：`Zombie.Table.CfgTaskUnlock`；条目：6。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|UnlockCondition|`System.Collections.Generic.List`1[Zombie.Table.Common.UnlockCondition]`|False|
|TaskId|`System.Int32`|False|
|TaskId_Ref|`Zombie.Table.CfgTask`|True|
|TaskType|`Zombie.Table.task.TaskType`|False|
|TriggerReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|

代码消费样本：

## tbtotalattribute

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbTotalAttribute.cs`；行类型：`Zombie.Table.CfgTotalAttribute`；条目：138。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`Zombie.Table.AttributeType`|False|
|Name|`System.String`|False|
|ShortName|`System.String`|False|
|DataType|`Zombie.Table.DataType`|False|
|Icon|`System.String`|False|
|SmallIcon|`System.String`|False|
|Addition|`Zombie.Table.AttributeType`|False|
|Desc|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/ForgeProxy.cs:307`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/HeroData.cs:173`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIAttributeCheckView.cs:66`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIAuraCom.cs:192`

## tbtotem

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbTotem.cs`；行类型：`Zombie.Table.CfgTotem`；条目：1600。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|TotemType|`System.Int32`|False|
|NextId|`System.Int32`|False|
|Icon|`System.String`|False|
|Part|`Zombie.Table.Common.StatuePart`|False|
|Tier|`System.Int32`|False|
|Quality|`System.Int32`|False|
|MaxQuality|`System.Int32`|False|
|Slot|`System.Int32`|False|
|Disassembly|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|PromotionCost|`Zombie.Table.item.IdValue`|False|
|Attribute|`System.Collections.Generic.List`1[Zombie.Table.Common.Attribute]`|False|
|AddNum|`System.Int32`|False|
|AttrRandom|`System.Collections.Generic.List`1[Zombie.Table.NormalWeight]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/StatueProxy.cs:60`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/StatueProxy.cs:118`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/StatueProxy.cs:232`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UICommonStatue.cs:46`

## tbtotemattributelib

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbTotemAttributeLib.cs`；行类型：`Zombie.Table.CfgTotemAttributeLib`；条目：300。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|AttributeID|`System.Int32`|False|
|AttributeID_Ref|`Zombie.Table.CfgAttribute`|True|
|Tier|`System.Int32`|False|
|QualityParam|`System.Collections.Generic.List`1[Zombie.Table.NormalWeight]`|False|

代码消费样本：

## tbtotemenergyattribute

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbTotemEnergyAttribute.cs`；行类型：`Zombie.Table.CfgTotemEnergyAttribute`；条目：350。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|EnergyMaxValue|`System.Int32`|False|
|Description|`System.String`|False|
|Attribute|`System.Collections.Generic.List`1[Zombie.Table.Common.Attribute]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRunePowerView.cs:50`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIRunePowerView.cs:106`

## tbtotemrune

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbTotemRune.cs`；行类型：`Zombie.Table.CfgTotemRune`；条目：1600。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|EntryGroup|`System.Int32`|False|
|TradeGroup|`Zombie.Table.TradeType`|False|
|Icon|`System.String`|False|
|Part|`Zombie.Table.Common.StatuePart`|False|
|Quality|`Zombie.Table.QualityType`|False|
|Energy|`System.Int32`|False|
|Description|`System.String`|False|
|Attribute|`Zombie.Table.Common.Attribute`|False|
|SkillBuff|`System.Int32`|False|
|SkillBuff_Ref|`Zombie.Table.CfgHeroSkillBuff`|True|
|EffectHeroes|`System.Collections.Generic.List`1[System.Int32]`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/StatueProxy.cs:307`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UICommonStatue.cs:67`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIComposeRuneView.cs:84`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UIComposeRuneView.cs:105`

## tbtrialofgrowth

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbTrialOfGrowth.cs`；行类型：`Zombie.Table.CfgTrialOfGrowth`；条目：15。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.String`|False|
|SimpleChinese|`System.String`|False|
|English|`System.String`|False|

代码消费样本：

## tbturfwarfamilybox

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbTurfWarFamilyBox.cs`；行类型：`Zombie.Table.CfgTurfWarFamilyBox`；条目：3。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|AttackNum|`System.Int32`|False|
|DefendNum|`System.Int32`|False|
|JoinReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|WinReward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Icon|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UITurfWarBoxCom.cs:90`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UITurfWarFamilyBoxView.cs:117`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UITurfWarHelpView.cs:51`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UITurfWarNeighborView.cs:122`

## tbturfwarfamilyrobot

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbTurfWarFamilyRobot.cs`；行类型：`Zombie.Table.CfgTurfWarFamilyRobot`；条目：8。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|Head|`System.Int32`|False|
|Head_Ref|`Zombie.Table.CfgHead`|True|
|Body|`System.Int32`|False|
|Body_Ref|`Zombie.Table.CfgBody`|True|
|Border|`System.Int32`|False|
|Border_Ref|`Zombie.Table.CfgBorder`|True|
|Box1ID|`System.Int32`|False|
|Box1ID_Ref|`Zombie.Table.CfgTurfWarPersonBox`|True|
|Box2ID|`System.Int32`|False|
|Box2ID_Ref|`Zombie.Table.CfgTurfWarPersonBox`|True|
|Power|`System.Int32`|False|

代码消费样本：

## tbturfwarpersonbox

读取器：`research/code-study/managed/decompiled/HotUpdate/Zombie.Table/TbTurfWarPersonBox.cs`；行类型：`Zombie.Table.CfgTurfWarPersonBox`；条目：185。

|字段|类型|生成引用字段|
|---|---|---|
|ID|`System.Int32`|False|
|Name|`System.String`|False|
|Weight|`System.Int32`|False|
|Reward|`System.Collections.Generic.List`1[Zombie.Table.item.IdValue]`|False|
|Icon|`System.String`|False|

代码消费样本：
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UITurfWarBoxCom.cs:118`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UITurfWarNeighborView.cs:111`
- `research/code-study/managed/decompiled/HotUpdate/Zombie/UITurfWarTeamCom.cs:68`

