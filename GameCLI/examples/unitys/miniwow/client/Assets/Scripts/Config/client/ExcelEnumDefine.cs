using System.Collections.Generic;

namespace Config
{
    /// <summary>
    /// TaskConditionType
    /// </summary>
    public enum ETaskConditionType
    {
        None = 0,
        AdventureEventTimes = 1,
        AllStatueLevelTo = 2,
        AnyWorkerReachLevel = 3,
        AreaExploreLevelTo = 4,
        AssembleAreaQualityRandomBoss = 5,
        AssembleHeroUpStar = 6,
        AssembleRandomBossTimes = 7,
        AssembleTargetFixBossTimes = 8,
        AssembleTeamTimes = 9,
        BookDraw = 10,
        BuildEquipmentTimes = 11,
        BuildingUpLevelTo = 12,
        BuildItemTimes = 13,
        CampaignCompleteStage = 14,
        ChallengeDungeonTimes = 15,
        ChapterTaskRewardCumulativeTimes = 16,
        ClearDesignatedDungeonChapter = 17,
        CommentCount = 18,
        CumulativeAdventureSelectArea = 19,
        CumulativeBuildEquipmentConditionTimes = 20,
        CumulativeBuildItemTimes = 21,
        CumulativeConsume = 22,
        CumulativeExpeditionRefreshTimes = 23,
        CumulativeItemPurchaseCount = 24,
        CumulativePVPTimes = 25,
        CumulativeRecharge = 26,
        CumulativeStatueUpGrade = 27,
        CumulativeTradeEquipmentConditionTimes = 28,
        DispatchExplorationTimes = 29,
        EquipHeroTimes = 30,
        EquipmentEnhancementCount = 31,
        FinishTalk = 32,
        ForgeAnyEquipment = 33,
        GuildLevelTo = 34,
        HaveHeroNumber = 35,
        HaveRelicLevelNumber = 36,
        HaveRelicQualityNumber = 37,
        HaveStatueNumber = 38,
        HeroBreakthrougCount = 39,
        HeroDrawCumulativeNumber = 40,
        HeroDrawTimesWithTargetId = 41,
        HeroEquipmentLevel = 42,
        HeroRecruitmentCount = 43,
        ItemPurchaseCount = 44,
        JoinGameClub = 45,
        LikeCount = 46,
        Login = 47,
        ManorCheerOthersWinTimes = 48,
        ManorGatherResourceTimes = 49,
        ManorHasGatheredResource = 50,
        ManorPlunderResourceTimes = 51,
        ManorReachLevel = 52,
        MysticRealmCompleteStage = 53,
        MysticRealmSweepTimes = 54,
        OpenSystem = 55,
        OrderTaskRewardCumulativeTimes = 56,
        OrderTaskTakeCumulativeTimes = 57,
        PassedBattleId = 58,
        PetDraw = 59,
        PostCount = 60,
        PVPTimes = 61,
        ReceiveBuildingGold = 62,
        ReceiveFreeHero = 63,
        ReceiveSevenDaySignReward = 64,
        ReceiveTrialOfGrowthReward = 65,
        RelicDrawTimes = 66,
        ShopTypeBuyCumulativeTimes = 67,
        SprintActivityUpdate = 68,
        StarCountForDesignatedBattles = 69,
        SweepDungeon = 70,
        TargetBattleTimes = 71,
        TaxTime = 72,
        TotalHeroRecruitmentCount = 73,
        TradeEquipmentTimes = 74,
        TurfWarBegin = 75,
        UpBlessTotalTimes = 76,
        UseCostItem = 77,
        YieldRewardCumulativeTimes = 78,
    }

    /// <summary>
    /// TaskConditionShape
    /// </summary>
    public enum ETaskConditionShape
    {
        None = 0,
        EmptyCondition = 1,
        ListParamCondition = 2,
        OneParamCondition = 3,
        ThreeParamCondition = 4,
        TwoParamCondition = 5,
    }

    /// <summary>
    /// TaskBuildingType
    /// </summary>
    public enum ETaskBuildingType
    {
        None = 0,
        AccessoriesStore = 1,
        Armor = 2,
        AuctionHouse = 3,
        Bless = 4,
        Bro = 5,
        Explore = 6,
        Horn = 7,
        Hospital = 8,
        Hotel = 9,
        Money = 10,
        OrderTask = 11,
        OrderTask2 = 12,
        Rally = 13,
        WeaponStore = 14,
    }

    /// <summary>
    /// TaskCategory
    /// </summary>
    public enum ETaskCategory
    {
        None = 0,
        SideTask = 1,
    }

    /// <summary>
    /// TaskGuideType
    /// </summary>
    public enum ETaskGuideType
    {
        None = 0,
        Strong = 1,
        Weak = 2,
    }
}