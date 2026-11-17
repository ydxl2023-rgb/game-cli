using Oathx.Entity;
using System.Collections.Generic;

/// <summary>Product-owned lobby data; no remote service or sample login is implied.</summary>
[Initialize]
public partial class HomeEntity : XEntity
{
    /// <summary>Current playerName value supplied by the product.</summary>
    public string playerName
    { get; set; }

    /// <summary>Current playerLevel value supplied by the product.</summary>
    public int playerLevel
    { get; set; }

    /// <summary>Current currencyOneAmount value supplied by the product.</summary>
    public long currencyOneAmount
    { get; set; }

    /// <summary>Current currencyTwoAmount value supplied by the product.</summary>
    public long currencyTwoAmount
    { get; set; }

    /// <summary>Current currencyThreeAmount value supplied by the product.</summary>
    public long currencyThreeAmount
    { get; set; }

    /// <summary>Current chatMessage value supplied by the product.</summary>
    public string chatMessage
    { get; set; }

    /// <summary>Current characterKey value supplied by the product.</summary>
    public string characterKey
    { get; set; }

    /// <summary>Current seasonKey value supplied by the product.</summary>
    public string seasonKey
    { get; set; }

    /// <summary>Local reference drop state; the product may replace it.</summary>
    public HomeDropState dropState
    { get; set; } = HomeDropState.Triple;

    /// <summary>Independent indicator states; missing keys are hidden.</summary>
    private readonly Dictionary<HomeIndicator, bool> indicators = new Dictionary<HomeIndicator, bool> {
        { HomeIndicator.FavorityNewTag, false },
        { HomeIndicator.ItemToolTips, true },
        { HomeIndicator.TaskButton, true },
        { HomeIndicator.TaskEffectTips, false },
        { HomeIndicator.TaskNewTag, false },
        { HomeIndicator.SevendaysLogin, true },
        { HomeIndicator.SevendaysTask, false },
        { HomeIndicator.NewTag, false },
        { HomeIndicator.FourteendaysTask, true },
        { HomeIndicator.NewTag0620301, false },
        { HomeIndicator.DirectPurchaseButton, false },
        { HomeIndicator.DirectPurchaseNewTag, false },
        { HomeIndicator.DirectPurchaseButton1, true },
        { HomeIndicator.DirectPurchase1NewTag, false },
        { HomeIndicator.CNBattlePassButton, false },
        { HomeIndicator.CNBattlePassNewTag, false },
        { HomeIndicator.TimeLimitGiftButton, false },
        { HomeIndicator.TimeLimitGiftNewTag, false },
        { HomeIndicator.UpInProButton, false },
        { HomeIndicator.UpInProNewTag, false },
        { HomeIndicator.ComeBackButton, true },
        { HomeIndicator.ComeBackNewTag, false },
        { HomeIndicator.PowerfulBait, false },
        { HomeIndicator.KingSetter, false },
        { HomeIndicator.ZhanShuNode, false },
        { HomeIndicator.TacticStudyNewTag, false },
        { HomeIndicator.FateBattleNode, false },
        { HomeIndicator.FateBattleNewTag, true },
        { HomeIndicator.YeJiuNode, false },
        { HomeIndicator.PlayerTheme, false },
        { HomeIndicator.BtnTurnCard, true },
        { HomeIndicator.ImgNewTagTurnCard, false },
        { HomeIndicator.AliPayPackage, false },
        { HomeIndicator.AliPayPackageNewTag, false },
        { HomeIndicator.QuestionNode, false },
        { HomeIndicator.QuestionNewTag, false },
        { HomeIndicator.BtnPreOrder, false },
        { HomeIndicator.ImgNewTagPreOrder, false },
        { HomeIndicator.BtnMatchGuess, false },
        { HomeIndicator.ImgNewTagMatchGuess, false },
        { HomeIndicator.BtnTopBattle, false },
        { HomeIndicator.ImgNewTagTopBattle, false },
        { HomeIndicator.BtnServeGame, false },
        { HomeIndicator.ImgNewTagServeGame, false },
        { HomeIndicator.BtnNewYear, true },
        { HomeIndicator.ImgNewTagNewYear, false },
        { HomeIndicator.BtnChainGift, false },
        { HomeIndicator.ImgNewTagChainGift, false },
        { HomeIndicator.BtnSPTopic, true },
        { HomeIndicator.ImgNewTagSPTopic, false },
        { HomeIndicator.BtnAnniversary, false },
        { HomeIndicator.ImgNewTagAnniversary, false },
        { HomeIndicator.BtnFreeMatch, false },
        { HomeIndicator.ImgNewTagFreeMatch, false },
        { HomeIndicator.BtnRunningGame, false },
        { HomeIndicator.ImgNewTagRunningGame, false },
        { HomeIndicator.Btn15AnniversaryActive, false },
        { HomeIndicator.ImgNewTag15Anniversary, false },
        { HomeIndicator.BtnLuckyDraw, false },
        { HomeIndicator.ImgNewTagLuckyDraw, false },
        { HomeIndicator.BtnWuYeCanteen, false },
        { HomeIndicator.ImgNewTagWuYeCanteen, false },
        { HomeIndicator.BtnAceDuel, false },
        { HomeIndicator.ImgNewTagAceDuel, false },
        { HomeIndicator.BtnCampTraining, true },
        { HomeIndicator.ImgNewTagCampTraining, false },
        { HomeIndicator.BtnSpecialOfferPackage, false },
        { HomeIndicator.ImgNewTagSpecialOfferPackage, false },
        { HomeIndicator.BtnBarbecue, false },
        { HomeIndicator.ImgNewTagBarbecue, false },
        { HomeIndicator.BtnCombinationGift, false },
        { HomeIndicator.ImgNewTagCombinationGift, false },
        { HomeIndicator.BtnSelectSelfGift, false },
        { HomeIndicator.ImgNewTagSelectSelfGift, false },
        { HomeIndicator.BtnSelectSelfGacha, false },
        { HomeIndicator.ImgNewTagSelectSelfGacha, false },
        { HomeIndicator.BtnSkillResonance, false },
        { HomeIndicator.ImgNewTagSkillResonance, false },
        { HomeIndicator.BtnNationalTournament, false },
        { HomeIndicator.ImgNewTagNationalTournament, false },
        { HomeIndicator.AdsNode, true },
        { HomeIndicator.PaiHangNewTag, false },
        { HomeIndicator.NewTagBag, false },
        { HomeIndicator.TuJianNewTag, false },
        { HomeIndicator.HonorNewTag, false },
        { HomeIndicator.JuqingNewTag, false },
        { HomeIndicator.SaishiNewTag, false },
        { HomeIndicator.HesujidiNewTag, false },
        { HomeIndicator.ZhaomuNewTag, true },
        { HomeIndicator.DuiyuanNewTag, true },
        { HomeIndicator.DuiwuNewTag, false },
        { HomeIndicator.SheTuanNewTag, true },
        { HomeIndicator.HuodongNewTag, false },
    };

    /// <summary>Read an indicator state without exposing the owned dictionary.</summary>
    /// <param name="indicator">Stable presentation identifier.</param>
    /// <returns>True when the indicator is enabled.</returns>
    public bool IsIndicatorVisible(HomeIndicator indicator)
    {
        return indicators.TryGetValue(indicator, out bool visible) && visible;
    }

    /// <summary>Update one known presentation state.</summary>
    /// <param name="indicator">Stable presentation identifier.</param>
    /// <param name="visible">Requested visibility.</param>
    /// <returns>True when the identifier is supported.</returns>
    public bool SetIndicator(HomeIndicator indicator, bool visible)
    {
        if (!System.Enum.IsDefined(typeof(HomeIndicator), indicator))
            return false;

        indicators[indicator] = visible;

        return true;
    }

}

/// <summary>Mutually exclusive reward multiplier presentations.</summary>
public enum HomeDropState
{
    /// <summary>No promotion is displayed.</summary>
    None,

    /// <summary>Display the double drop artwork.</summary>
    Double,

    /// <summary>Display the triple drop artwork.</summary>
    Triple
}

/// <summary>Stable identities for independently controlled lobby elements.</summary>
public enum HomeIndicator
{
    /// <summary>Controls the FavorityNewTag presentation.</summary>
    FavorityNewTag = 1860273340,

    /// <summary>Controls the _ItemToolTips presentation.</summary>
    ItemToolTips = 176465348,

    /// <summary>Controls the TaskButton presentation.</summary>
    TaskButton = 1957348258,

    /// <summary>Controls the TaskEffectTips presentation.</summary>
    TaskEffectTips = 2020446927,

    /// <summary>Controls the TaskNewTag presentation.</summary>
    TaskNewTag = 522313836,

    /// <summary>Controls the SevendaysLogin presentation.</summary>
    SevendaysLogin = 1063216840,

    /// <summary>Controls the SevendaysTask presentation.</summary>
    SevendaysTask = 971503852,

    /// <summary>Controls the NewTag presentation.</summary>
    NewTag = 1567562792,

    /// <summary>Controls the FourteendaysTask presentation.</summary>
    FourteendaysTask = 235633573,

    /// <summary>Controls the NewTag presentation.</summary>
    NewTag0620301 = 495359385,

    /// <summary>Controls the DirectPurchaseButton presentation.</summary>
    DirectPurchaseButton = 1973166360,

    /// <summary>Controls the DirectPurchaseNewTag presentation.</summary>
    DirectPurchaseNewTag = 411054284,

    /// <summary>Controls the DirectPurchaseButton1 presentation.</summary>
    DirectPurchaseButton1 = 1839379618,

    /// <summary>Controls the DirectPurchase1NewTag presentation.</summary>
    DirectPurchase1NewTag = 45613098,

    /// <summary>Controls the CNBattlePassButton presentation.</summary>
    CNBattlePassButton = 712737229,

    /// <summary>Controls the CNBattlePassNewTag presentation.</summary>
    CNBattlePassNewTag = 1037881971,

    /// <summary>Controls the TimeLimitGiftButton presentation.</summary>
    TimeLimitGiftButton = 1291593008,

    /// <summary>Controls the TimeLimitGiftNewTag presentation.</summary>
    TimeLimitGiftNewTag = 1673184687,

    /// <summary>Controls the UpInProButton presentation.</summary>
    UpInProButton = 1204630184,

    /// <summary>Controls the UpInProNewTag presentation.</summary>
    UpInProNewTag = 422014507,

    /// <summary>Controls the ComeBackButton presentation.</summary>
    ComeBackButton = 1355960788,

    /// <summary>Controls the ComeBackNewTag presentation.</summary>
    ComeBackNewTag = 1223798100,

    /// <summary>Controls the PowerfulBait presentation.</summary>
    PowerfulBait = 1949776396,

    /// <summary>Controls the KingSetter presentation.</summary>
    KingSetter = 1780626494,

    /// <summary>Controls the ZhanShuNode presentation.</summary>
    ZhanShuNode = 381772465,

    /// <summary>Controls the TacticStudyNewTag presentation.</summary>
    TacticStudyNewTag = 1762738302,

    /// <summary>Controls the FateBattleNode presentation.</summary>
    FateBattleNode = 1810936888,

    /// <summary>Controls the FateBattleNewTag presentation.</summary>
    FateBattleNewTag = 326593717,

    /// <summary>Controls the YeJiuNode presentation.</summary>
    YeJiuNode = 123949700,

    /// <summary>Controls the PlayerTheme presentation.</summary>
    PlayerTheme = 1602871456,

    /// <summary>Controls the BtnTurnCard presentation.</summary>
    BtnTurnCard = 240680371,

    /// <summary>Controls the ImgNewTagTurnCard presentation.</summary>
    ImgNewTagTurnCard = 567831773,

    /// <summary>Controls the AliPayPackage presentation.</summary>
    AliPayPackage = 1411722651,

    /// <summary>Controls the AliPayPackageNewTag presentation.</summary>
    AliPayPackageNewTag = 124084413,

    /// <summary>Controls the QuestionNode presentation.</summary>
    QuestionNode = 1144685045,

    /// <summary>Controls the QuestionNewTag presentation.</summary>
    QuestionNewTag = 153413932,

    /// <summary>Controls the BtnPreOrder presentation.</summary>
    BtnPreOrder = 1076014581,

    /// <summary>Controls the ImgNewTagPreOrder presentation.</summary>
    ImgNewTagPreOrder = 713899548,

    /// <summary>Controls the BtnMatchGuess presentation.</summary>
    BtnMatchGuess = 1942530504,

    /// <summary>Controls the ImgNewTagMatchGuess presentation.</summary>
    ImgNewTagMatchGuess = 1935608623,

    /// <summary>Controls the BtnTopBattle presentation.</summary>
    BtnTopBattle = 773741012,

    /// <summary>Controls the ImgNewTagTopBattle presentation.</summary>
    ImgNewTagTopBattle = 1551483325,

    /// <summary>Controls the BtnServeGame presentation.</summary>
    BtnServeGame = 1185793795,

    /// <summary>Controls the ImgNewTagServeGame presentation.</summary>
    ImgNewTagServeGame = 233079366,

    /// <summary>Controls the BtnNewYear presentation.</summary>
    BtnNewYear = 185462211,

    /// <summary>Controls the ImgNewTagNewYear presentation.</summary>
    ImgNewTagNewYear = 1087283545,

    /// <summary>Controls the BtnChainGift presentation.</summary>
    BtnChainGift = 1061319292,

    /// <summary>Controls the ImgNewTagChainGift presentation.</summary>
    ImgNewTagChainGift = 1971095828,

    /// <summary>Controls the BtnSPTopic presentation.</summary>
    BtnSPTopic = 676408620,

    /// <summary>Controls the ImgNewTagSPTopic presentation.</summary>
    ImgNewTagSPTopic = 310973989,

    /// <summary>Controls the BtnAnniversary presentation.</summary>
    BtnAnniversary = 930081879,

    /// <summary>Controls the ImgNewTagAnniversary presentation.</summary>
    ImgNewTagAnniversary = 1641502494,

    /// <summary>Controls the BtnFreeMatch presentation.</summary>
    BtnFreeMatch = 262234992,

    /// <summary>Controls the ImgNewTagFreeMatch presentation.</summary>
    ImgNewTagFreeMatch = 1951098233,

    /// <summary>Controls the BtnRunningGame presentation.</summary>
    BtnRunningGame = 634463733,

    /// <summary>Controls the ImgNewTagRunningGame presentation.</summary>
    ImgNewTagRunningGame = 593650221,

    /// <summary>Controls the Btn1_5AnniversaryActive presentation.</summary>
    Btn15AnniversaryActive = 523752062,

    /// <summary>Controls the ImgNewTag1_5Anniversary presentation.</summary>
    ImgNewTag15Anniversary = 976271029,

    /// <summary>Controls the BtnLuckyDraw presentation.</summary>
    BtnLuckyDraw = 29275244,

    /// <summary>Controls the ImgNewTagLuckyDraw presentation.</summary>
    ImgNewTagLuckyDraw = 911114711,

    /// <summary>Controls the BtnWuYeCanteen presentation.</summary>
    BtnWuYeCanteen = 1923238890,

    /// <summary>Controls the ImgNewTagWuYeCanteen presentation.</summary>
    ImgNewTagWuYeCanteen = 160241079,

    /// <summary>Controls the BtnAceDuel presentation.</summary>
    BtnAceDuel = 1397766797,

    /// <summary>Controls the ImgNewTagAceDuel presentation.</summary>
    ImgNewTagAceDuel = 921504423,

    /// <summary>Controls the BtnCampTraining presentation.</summary>
    BtnCampTraining = 556518230,

    /// <summary>Controls the ImgNewTagCampTraining presentation.</summary>
    ImgNewTagCampTraining = 599303332,

    /// <summary>Controls the BtnSpecialOfferPackage presentation.</summary>
    BtnSpecialOfferPackage = 2063302110,

    /// <summary>Controls the ImgNewTagSpecialOfferPackage presentation.</summary>
    ImgNewTagSpecialOfferPackage = 866306026,

    /// <summary>Controls the BtnBarbecue presentation.</summary>
    BtnBarbecue = 2040027825,

    /// <summary>Controls the ImgNewTagBarbecue presentation.</summary>
    ImgNewTagBarbecue = 1193964017,

    /// <summary>Controls the BtnCombinationGift presentation.</summary>
    BtnCombinationGift = 1216062238,

    /// <summary>Controls the ImgNewTagCombinationGift presentation.</summary>
    ImgNewTagCombinationGift = 15326038,

    /// <summary>Controls the BtnSelectSelfGift presentation.</summary>
    BtnSelectSelfGift = 444194025,

    /// <summary>Controls the ImgNewTagSelectSelfGift presentation.</summary>
    ImgNewTagSelectSelfGift = 771140503,

    /// <summary>Controls the BtnSelectSelfGacha presentation.</summary>
    BtnSelectSelfGacha = 414049910,

    /// <summary>Controls the ImgNewTagSelectSelfGacha presentation.</summary>
    ImgNewTagSelectSelfGacha = 454022659,

    /// <summary>Controls the BtnSkillResonance presentation.</summary>
    BtnSkillResonance = 1225401357,

    /// <summary>Controls the ImgNewTagSkillResonance presentation.</summary>
    ImgNewTagSkillResonance = 1626702223,

    /// <summary>Controls the BtnNationalTournament presentation.</summary>
    BtnNationalTournament = 778676641,

    /// <summary>Controls the ImgNewTagNationalTournament presentation.</summary>
    ImgNewTagNationalTournament = 1143529667,

    /// <summary>Controls the AdsNode presentation.</summary>
    AdsNode = 1001429026,

    /// <summary>Controls the _PaiHangNewTag presentation.</summary>
    PaiHangNewTag = 855911354,

    /// <summary>Controls the NewTagBag presentation.</summary>
    NewTagBag = 2067430049,

    /// <summary>Controls the TuJianNewTag presentation.</summary>
    TuJianNewTag = 833598051,

    /// <summary>Controls the HonorNewTag presentation.</summary>
    HonorNewTag = 230743909,

    /// <summary>Controls the JuqingNewTag presentation.</summary>
    JuqingNewTag = 340380090,

    /// <summary>Controls the SaishiNewTag presentation.</summary>
    SaishiNewTag = 1727165129,

    /// <summary>Controls the HesujidiNewTag presentation.</summary>
    HesujidiNewTag = 1317670684,

    /// <summary>Controls the ZhaomuNewTag presentation.</summary>
    ZhaomuNewTag = 1612802959,

    /// <summary>Controls the DuiyuanNewTag presentation.</summary>
    DuiyuanNewTag = 1254032041,

    /// <summary>Controls the DuiwuNewTag presentation.</summary>
    DuiwuNewTag = 1683079727,

    /// <summary>Controls the SheTuanNewTag presentation.</summary>
    SheTuanNewTag = 1892449151,

    /// <summary>Controls the HuodongNewTag presentation.</summary>
    HuodongNewTag = 1669826348,
}
