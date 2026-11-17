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

    /// <summary>Current label062003 value supplied by the product.</summary>
    public string label062003
    { get; set; }

    /// <summary>Current label062032 value supplied by the product.</summary>
    public string label062032
    { get; set; }

    /// <summary>Current label0620420 value supplied by the product.</summary>
    public string label0620420
    { get; set; }

    /// <summary>Current label0620530 value supplied by the product.</summary>
    public string label0620530
    { get; set; }

    /// <summary>Current label0620630 value supplied by the product.</summary>
    public string label0620630
    { get; set; }

    /// <summary>Current label0620720 value supplied by the product.</summary>
    public string label0620720
    { get; set; }

    /// <summary>Current label0620900 value supplied by the product.</summary>
    public string label0620900
    { get; set; }

    /// <summary>Current label06201003 value supplied by the product.</summary>
    public string label06201003
    { get; set; }

    /// <summary>Current label06201102 value supplied by the product.</summary>
    public string label06201102
    { get; set; }

    /// <summary>Current label0620130010 value supplied by the product.</summary>
    public string label0620130010
    { get; set; }

    /// <summary>Current label062013002 value supplied by the product.</summary>
    public string label062013002
    { get; set; }

    /// <summary>Current label06201502 value supplied by the product.</summary>
    public string label06201502
    { get; set; }

    /// <summary>Current label06201600 value supplied by the product.</summary>
    public string label06201600
    { get; set; }

    /// <summary>Current label06202100 value supplied by the product.</summary>
    public string label06202100
    { get; set; }

    /// <summary>Current label06202200 value supplied by the product.</summary>
    public string label06202200
    { get; set; }

    /// <summary>Current label06202300 value supplied by the product.</summary>
    public string label06202300
    { get; set; }

    /// <summary>Current label06202400 value supplied by the product.</summary>
    public string label06202400
    { get; set; }

    /// <summary>Current label06202500 value supplied by the product.</summary>
    public string label06202500
    { get; set; }

    /// <summary>Current label06202600 value supplied by the product.</summary>
    public string label06202600
    { get; set; }

    /// <summary>Current label06202800 value supplied by the product.</summary>
    public string label06202800
    { get; set; }

    /// <summary>Current label06202900 value supplied by the product.</summary>
    public string label06202900
    { get; set; }

    /// <summary>Current label06203000 value supplied by the product.</summary>
    public string label06203000
    { get; set; }

    /// <summary>Current label06203100 value supplied by the product.</summary>
    public string label06203100
    { get; set; }

    /// <summary>Current label06203200 value supplied by the product.</summary>
    public string label06203200
    { get; set; }

    /// <summary>Current label06203300 value supplied by the product.</summary>
    public string label06203300
    { get; set; }

    /// <summary>Current label06203400 value supplied by the product.</summary>
    public string label06203400
    { get; set; }

    /// <summary>Current label06203500 value supplied by the product.</summary>
    public string label06203500
    { get; set; }

    /// <summary>Current label06203600 value supplied by the product.</summary>
    public string label06203600
    { get; set; }

    /// <summary>Current label06203700 value supplied by the product.</summary>
    public string label06203700
    { get; set; }

    /// <summary>Current label06203800 value supplied by the product.</summary>
    public string label06203800
    { get; set; }

    /// <summary>Current label06203900 value supplied by the product.</summary>
    public string label06203900
    { get; set; }

    /// <summary>Current label06204000 value supplied by the product.</summary>
    public string label06204000
    { get; set; }

    /// <summary>Local reference drop state; the product may replace it.</summary>
    public HomeDropState dropState
    { get; set; } = HomeDropState.Triple;

    /// <summary>Independent indicator states; missing keys are hidden.</summary>
    public Dictionary<HomeIndicator, bool> indicators
    { get; private set; } = new Dictionary<HomeIndicator, bool> {
        { HomeIndicator.MailTips, false },
        { HomeIndicator.SetTips, false },
        { HomeIndicator.AnnouncementTips, false },
        { HomeIndicator.FriendTips, false },
        { HomeIndicator.FavorTips, false },
        { HomeIndicator.FavorityNewTag, false },
        { HomeIndicator.ItemToolTips, true },
        { HomeIndicator.ManualTips, false },
        { HomeIndicator.TaskButton, true },
        { HomeIndicator.TaskTips, false },
        { HomeIndicator.TaskEffectTips, false },
        { HomeIndicator.TaskNewTag, false },
        { HomeIndicator.SevendaysLogin, true },
        { HomeIndicator.RedTips, true },
        { HomeIndicator.SevendaysTask, false },
        { HomeIndicator.RedTips0620200, true },
        { HomeIndicator.NewTag, false },
        { HomeIndicator.FourteendaysTask, true },
        { HomeIndicator.RedTips0620300, true },
        { HomeIndicator.NewTag0620301, false },
        { HomeIndicator.DirectPurchaseButton, false },
        { HomeIndicator.DirectPurchaseRedDot, false },
        { HomeIndicator.DirectPurchaseNewTag, false },
        { HomeIndicator.DirectPurchaseButton1, true },
        { HomeIndicator.DirectPurchase1RedDot, false },
        { HomeIndicator.DirectPurchase1NewTag, false },
        { HomeIndicator.CNBattlePassButton, false },
        { HomeIndicator.CNBattlePassRedDot, false },
        { HomeIndicator.CNBattlePassNewTag, false },
        { HomeIndicator.TimeLimitGiftButton, false },
        { HomeIndicator.TimeLimitGiftTips, false },
        { HomeIndicator.TimeLimitGiftNewTag, false },
        { HomeIndicator.UpInProButton, false },
        { HomeIndicator.UpInProTips, false },
        { HomeIndicator.UpInProNewTag, false },
        { HomeIndicator.ComeBackButton, true },
        { HomeIndicator.ComeBackRedDot, false },
        { HomeIndicator.ComeBackNewTag, false },
        { HomeIndicator.PowerfulBait, false },
        { HomeIndicator.RedDotBait, true },
        { HomeIndicator.KingSetter, false },
        { HomeIndicator.RedDotKingSetter, true },
        { HomeIndicator.ZhanShuNode, false },
        { HomeIndicator.TacticStudyNewTag, false },
        { HomeIndicator.FateBattleNode, false },
        { HomeIndicator.FateBattleNewTag, true },
        { HomeIndicator.YeJiuNode, false },
        { HomeIndicator.AppearRedDot, false },
        { HomeIndicator.PlayerTheme, false },
        { HomeIndicator.RedDotPlayerTheme, true },
        { HomeIndicator.BtnTurnCard, true },
        { HomeIndicator.ImgNewTagTurnCard, false },
        { HomeIndicator.ImgRedDotTurnCard, false },
        { HomeIndicator.AliPayPackage, false },
        { HomeIndicator.AliPayPackageNewTag, false },
        { HomeIndicator.QuestionNode, false },
        { HomeIndicator.QuestionNewTag, false },
        { HomeIndicator.BtnPreOrder, false },
        { HomeIndicator.ImgNewTagPreOrder, false },
        { HomeIndicator.ImgRedDotPreOrder, false },
        { HomeIndicator.BtnMatchGuess, false },
        { HomeIndicator.ImgNewTagMatchGuess, false },
        { HomeIndicator.ImgRedDotMatchGuess, false },
        { HomeIndicator.BtnTopBattle, false },
        { HomeIndicator.ImgNewTagTopBattle, false },
        { HomeIndicator.ImgRedDotTopBattle, false },
        { HomeIndicator.BtnServeGame, false },
        { HomeIndicator.ImgNewTagServeGame, false },
        { HomeIndicator.ImgRedDotServeGame, false },
        { HomeIndicator.BtnNewYear, true },
        { HomeIndicator.ImgNewTagNewYear, false },
        { HomeIndicator.ImgRedDotNewYear, false },
        { HomeIndicator.BtnChainGift, false },
        { HomeIndicator.ImgNewTagChainGift, false },
        { HomeIndicator.ImgRedDotChainGift, false },
        { HomeIndicator.BtnSPTopic, true },
        { HomeIndicator.ImgNewTagSPTopic, false },
        { HomeIndicator.ImgRedDotSPTopic, false },
        { HomeIndicator.BtnAnniversary, false },
        { HomeIndicator.ImgNewTagAnniversary, false },
        { HomeIndicator.ImgRedDotAnniversary, false },
        { HomeIndicator.BtnFreeMatch, false },
        { HomeIndicator.ImgNewTagFreeMatch, false },
        { HomeIndicator.ImgRedDotFreeMatch, false },
        { HomeIndicator.BtnRunningGame, false },
        { HomeIndicator.ImgNewTagRunningGame, false },
        { HomeIndicator.ImgRedDotRunningGame, false },
        { HomeIndicator.Btn15AnniversaryActive, false },
        { HomeIndicator.ImgNewTag15Anniversary, false },
        { HomeIndicator.ImgRedDot15Anniversary, false },
        { HomeIndicator.BtnLuckyDraw, false },
        { HomeIndicator.ImgNewTagLuckyDraw, false },
        { HomeIndicator.ImgRedDotLuckyDraw, false },
        { HomeIndicator.BtnWuYeCanteen, false },
        { HomeIndicator.ImgNewTagWuYeCanteen, false },
        { HomeIndicator.ImgRedDotWuYeCanteen, false },
        { HomeIndicator.BtnAceDuel, false },
        { HomeIndicator.ImgNewTagAceDuel, false },
        { HomeIndicator.ImgRedDotAceDuel, false },
        { HomeIndicator.BtnCampTraining, true },
        { HomeIndicator.ImgNewTagCampTraining, false },
        { HomeIndicator.ImgRedDotCampTraining, false },
        { HomeIndicator.BtnSpecialOfferPackage, false },
        { HomeIndicator.ImgNewTagSpecialOfferPackage, false },
        { HomeIndicator.ImgRedDotSpecialOfferPackage, false },
        { HomeIndicator.BtnBarbecue, false },
        { HomeIndicator.ImgNewTagBarbecue, false },
        { HomeIndicator.ImgRedDotBarbecue, false },
        { HomeIndicator.BtnCombinationGift, false },
        { HomeIndicator.ImgNewTagCombinationGift, false },
        { HomeIndicator.ImgRedDotCombinationGift, false },
        { HomeIndicator.BtnSelectSelfGift, false },
        { HomeIndicator.ImgNewTagSelectSelfGift, false },
        { HomeIndicator.ImgRedDotSelectSelfGift, false },
        { HomeIndicator.BtnSelectSelfGacha, false },
        { HomeIndicator.ImgNewTagSelectSelfGacha, false },
        { HomeIndicator.ImgRedDotSelectSelfGacha, false },
        { HomeIndicator.BtnSkillResonance, false },
        { HomeIndicator.ImgNewTagSkillResonance, false },
        { HomeIndicator.ImgRedDotSkillResonance, false },
        { HomeIndicator.BtnNationalTournament, false },
        { HomeIndicator.ImgNewTagNationalTournament, false },
        { HomeIndicator.ImgRedDotNationalTournament, false },
        { HomeIndicator.ShopTips, false },
        { HomeIndicator.AdsNode, true },
        { HomeIndicator.FuliTips, false },
        { HomeIndicator.PaiHangNewTag, false },
        { HomeIndicator.BeibaoTips, false },
        { HomeIndicator.NewTagBag, false },
        { HomeIndicator.GalleryTips, false },
        { HomeIndicator.TuJianNewTag, false },
        { HomeIndicator.HonorTips, false },
        { HomeIndicator.HonorNewTag, false },
        { HomeIndicator.JuqingNewTag, false },
        { HomeIndicator.ExpeditionTips, false },
        { HomeIndicator.SaishiNewTag, false },
        { HomeIndicator.HesujidiNewTag, false },
        { HomeIndicator.HesujidiTips, false },
        { HomeIndicator.ZhaomuTips, false },
        { HomeIndicator.ZhaomuNewTag, true },
        { HomeIndicator.DuiyuanNewTag, true },
        { HomeIndicator.DuiwuNewTag, false },
        { HomeIndicator.AssociationTips, false },
        { HomeIndicator.SheTuanNewTag, true },
        { HomeIndicator.DailyPlayTips, false },
        { HomeIndicator.HuodongNewTag, false },
    };
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
    /// <summary>Controls the MailTips presentation.</summary>
    MailTips = 1514372829,

    /// <summary>Controls the SetTips presentation.</summary>
    SetTips = 418538246,

    /// <summary>Controls the AnnouncementTips presentation.</summary>
    AnnouncementTips = 3199919,

    /// <summary>Controls the FriendTips presentation.</summary>
    FriendTips = 117418554,

    /// <summary>Controls the FavorTips presentation.</summary>
    FavorTips = 1199347660,

    /// <summary>Controls the FavorityNewTag presentation.</summary>
    FavorityNewTag = 1860273340,

    /// <summary>Controls the _ItemToolTips presentation.</summary>
    ItemToolTips = 176465348,

    /// <summary>Controls the manualTips presentation.</summary>
    ManualTips = 2087738905,

    /// <summary>Controls the TaskButton presentation.</summary>
    TaskButton = 1957348258,

    /// <summary>Controls the TaskTips presentation.</summary>
    TaskTips = 1782727633,

    /// <summary>Controls the TaskEffectTips presentation.</summary>
    TaskEffectTips = 2020446927,

    /// <summary>Controls the TaskNewTag presentation.</summary>
    TaskNewTag = 522313836,

    /// <summary>Controls the SevendaysLogin presentation.</summary>
    SevendaysLogin = 1063216840,

    /// <summary>Controls the RedTips presentation.</summary>
    RedTips = 2099569513,

    /// <summary>Controls the SevendaysTask presentation.</summary>
    SevendaysTask = 971503852,

    /// <summary>Controls the RedTips presentation.</summary>
    RedTips0620200 = 1194081923,

    /// <summary>Controls the NewTag presentation.</summary>
    NewTag = 1567562792,

    /// <summary>Controls the FourteendaysTask presentation.</summary>
    FourteendaysTask = 235633573,

    /// <summary>Controls the RedTips presentation.</summary>
    RedTips0620300 = 1228562200,

    /// <summary>Controls the NewTag presentation.</summary>
    NewTag0620301 = 495359385,

    /// <summary>Controls the DirectPurchaseButton presentation.</summary>
    DirectPurchaseButton = 1973166360,

    /// <summary>Controls the DirectPurchaseRedDot presentation.</summary>
    DirectPurchaseRedDot = 2028546540,

    /// <summary>Controls the DirectPurchaseNewTag presentation.</summary>
    DirectPurchaseNewTag = 411054284,

    /// <summary>Controls the DirectPurchaseButton1 presentation.</summary>
    DirectPurchaseButton1 = 1839379618,

    /// <summary>Controls the DirectPurchase1RedDot presentation.</summary>
    DirectPurchase1RedDot = 264176736,

    /// <summary>Controls the DirectPurchase1NewTag presentation.</summary>
    DirectPurchase1NewTag = 45613098,

    /// <summary>Controls the CNBattlePassButton presentation.</summary>
    CNBattlePassButton = 712737229,

    /// <summary>Controls the CNBattlePassRedDot presentation.</summary>
    CNBattlePassRedDot = 918252199,

    /// <summary>Controls the CNBattlePassNewTag presentation.</summary>
    CNBattlePassNewTag = 1037881971,

    /// <summary>Controls the TimeLimitGiftButton presentation.</summary>
    TimeLimitGiftButton = 1291593008,

    /// <summary>Controls the TimeLimitGiftTips presentation.</summary>
    TimeLimitGiftTips = 599903209,

    /// <summary>Controls the TimeLimitGiftNewTag presentation.</summary>
    TimeLimitGiftNewTag = 1673184687,

    /// <summary>Controls the UpInProButton presentation.</summary>
    UpInProButton = 1204630184,

    /// <summary>Controls the UpInProTips presentation.</summary>
    UpInProTips = 33199695,

    /// <summary>Controls the UpInProNewTag presentation.</summary>
    UpInProNewTag = 422014507,

    /// <summary>Controls the ComeBackButton presentation.</summary>
    ComeBackButton = 1355960788,

    /// <summary>Controls the ComeBackRedDot presentation.</summary>
    ComeBackRedDot = 1775596985,

    /// <summary>Controls the ComeBackNewTag presentation.</summary>
    ComeBackNewTag = 1223798100,

    /// <summary>Controls the PowerfulBait presentation.</summary>
    PowerfulBait = 1949776396,

    /// <summary>Controls the RedDotBait presentation.</summary>
    RedDotBait = 1483456732,

    /// <summary>Controls the KingSetter presentation.</summary>
    KingSetter = 1780626494,

    /// <summary>Controls the RedDotKingSetter presentation.</summary>
    RedDotKingSetter = 1213193502,

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

    /// <summary>Controls the AppearRedDot presentation.</summary>
    AppearRedDot = 328521371,

    /// <summary>Controls the PlayerTheme presentation.</summary>
    PlayerTheme = 1602871456,

    /// <summary>Controls the RedDotPlayerTheme presentation.</summary>
    RedDotPlayerTheme = 643335526,

    /// <summary>Controls the BtnTurnCard presentation.</summary>
    BtnTurnCard = 240680371,

    /// <summary>Controls the ImgNewTagTurnCard presentation.</summary>
    ImgNewTagTurnCard = 567831773,

    /// <summary>Controls the ImgRedDotTurnCard presentation.</summary>
    ImgRedDotTurnCard = 2039259506,

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

    /// <summary>Controls the ImgRedDotPreOrder presentation.</summary>
    ImgRedDotPreOrder = 692869669,

    /// <summary>Controls the BtnMatchGuess presentation.</summary>
    BtnMatchGuess = 1942530504,

    /// <summary>Controls the ImgNewTagMatchGuess presentation.</summary>
    ImgNewTagMatchGuess = 1935608623,

    /// <summary>Controls the ImgRedDotMatchGuess presentation.</summary>
    ImgRedDotMatchGuess = 178954592,

    /// <summary>Controls the BtnTopBattle presentation.</summary>
    BtnTopBattle = 773741012,

    /// <summary>Controls the ImgNewTagTopBattle presentation.</summary>
    ImgNewTagTopBattle = 1551483325,

    /// <summary>Controls the ImgRedDotTopBattle presentation.</summary>
    ImgRedDotTopBattle = 831366068,

    /// <summary>Controls the BtnServeGame presentation.</summary>
    BtnServeGame = 1185793795,

    /// <summary>Controls the ImgNewTagServeGame presentation.</summary>
    ImgNewTagServeGame = 233079366,

    /// <summary>Controls the ImgRedDotServeGame presentation.</summary>
    ImgRedDotServeGame = 1994941811,

    /// <summary>Controls the BtnNewYear presentation.</summary>
    BtnNewYear = 185462211,

    /// <summary>Controls the ImgNewTagNewYear presentation.</summary>
    ImgNewTagNewYear = 1087283545,

    /// <summary>Controls the ImgRedDotNewYear presentation.</summary>
    ImgRedDotNewYear = 946167488,

    /// <summary>Controls the BtnChainGift presentation.</summary>
    BtnChainGift = 1061319292,

    /// <summary>Controls the ImgNewTagChainGift presentation.</summary>
    ImgNewTagChainGift = 1971095828,

    /// <summary>Controls the ImgRedDotChainGift presentation.</summary>
    ImgRedDotChainGift = 1653909360,

    /// <summary>Controls the BtnSPTopic presentation.</summary>
    BtnSPTopic = 676408620,

    /// <summary>Controls the ImgNewTagSPTopic presentation.</summary>
    ImgNewTagSPTopic = 310973989,

    /// <summary>Controls the ImgRedDotSPTopic presentation.</summary>
    ImgRedDotSPTopic = 1660621660,

    /// <summary>Controls the BtnAnniversary presentation.</summary>
    BtnAnniversary = 930081879,

    /// <summary>Controls the ImgNewTagAnniversary presentation.</summary>
    ImgNewTagAnniversary = 1641502494,

    /// <summary>Controls the ImgRedDotAnniversary presentation.</summary>
    ImgRedDotAnniversary = 1694412007,

    /// <summary>Controls the BtnFreeMatch presentation.</summary>
    BtnFreeMatch = 262234992,

    /// <summary>Controls the ImgNewTagFreeMatch presentation.</summary>
    ImgNewTagFreeMatch = 1951098233,

    /// <summary>Controls the ImgRedDotFreeMatch presentation.</summary>
    ImgRedDotFreeMatch = 1646092040,

    /// <summary>Controls the BtnRunningGame presentation.</summary>
    BtnRunningGame = 634463733,

    /// <summary>Controls the ImgNewTagRunningGame presentation.</summary>
    ImgNewTagRunningGame = 593650221,

    /// <summary>Controls the ImgRedDotRunningGame presentation.</summary>
    ImgRedDotRunningGame = 340229412,

    /// <summary>Controls the Btn1_5AnniversaryActive presentation.</summary>
    Btn15AnniversaryActive = 523752062,

    /// <summary>Controls the ImgNewTag1_5Anniversary presentation.</summary>
    ImgNewTag15Anniversary = 976271029,

    /// <summary>Controls the ImgRedDot1_5Anniversary presentation.</summary>
    ImgRedDot15Anniversary = 1034379984,

    /// <summary>Controls the BtnLuckyDraw presentation.</summary>
    BtnLuckyDraw = 29275244,

    /// <summary>Controls the ImgNewTagLuckyDraw presentation.</summary>
    ImgNewTagLuckyDraw = 911114711,

    /// <summary>Controls the ImgRedDotLuckyDraw presentation.</summary>
    ImgRedDotLuckyDraw = 1552978579,

    /// <summary>Controls the BtnWuYeCanteen presentation.</summary>
    BtnWuYeCanteen = 1923238890,

    /// <summary>Controls the ImgNewTagWuYeCanteen presentation.</summary>
    ImgNewTagWuYeCanteen = 160241079,

    /// <summary>Controls the ImgRedDotWuYeCanteen presentation.</summary>
    ImgRedDotWuYeCanteen = 372794262,

    /// <summary>Controls the BtnAceDuel presentation.</summary>
    BtnAceDuel = 1397766797,

    /// <summary>Controls the ImgNewTagAceDuel presentation.</summary>
    ImgNewTagAceDuel = 921504423,

    /// <summary>Controls the ImgRedDotAceDuel presentation.</summary>
    ImgRedDotAceDuel = 1100961919,

    /// <summary>Controls the BtnCampTraining presentation.</summary>
    BtnCampTraining = 556518230,

    /// <summary>Controls the ImgNewTagCampTraining presentation.</summary>
    ImgNewTagCampTraining = 599303332,

    /// <summary>Controls the ImgRedDotCampTraining presentation.</summary>
    ImgRedDotCampTraining = 1919820457,

    /// <summary>Controls the BtnSpecialOfferPackage presentation.</summary>
    BtnSpecialOfferPackage = 2063302110,

    /// <summary>Controls the ImgNewTagSpecialOfferPackage presentation.</summary>
    ImgNewTagSpecialOfferPackage = 866306026,

    /// <summary>Controls the ImgRedDotSpecialOfferPackage presentation.</summary>
    ImgRedDotSpecialOfferPackage = 997624404,

    /// <summary>Controls the BtnBarbecue presentation.</summary>
    BtnBarbecue = 2040027825,

    /// <summary>Controls the ImgNewTagBarbecue presentation.</summary>
    ImgNewTagBarbecue = 1193964017,

    /// <summary>Controls the ImgRedDotBarbecue presentation.</summary>
    ImgRedDotBarbecue = 132605449,

    /// <summary>Controls the BtnCombinationGift presentation.</summary>
    BtnCombinationGift = 1216062238,

    /// <summary>Controls the ImgNewTagCombinationGift presentation.</summary>
    ImgNewTagCombinationGift = 15326038,

    /// <summary>Controls the ImgRedDotCombinationGift presentation.</summary>
    ImgRedDotCombinationGift = 1758652313,

    /// <summary>Controls the BtnSelectSelfGift presentation.</summary>
    BtnSelectSelfGift = 444194025,

    /// <summary>Controls the ImgNewTagSelectSelfGift presentation.</summary>
    ImgNewTagSelectSelfGift = 771140503,

    /// <summary>Controls the ImgRedDotSelectSelfGift presentation.</summary>
    ImgRedDotSelectSelfGift = 1300699865,

    /// <summary>Controls the BtnSelectSelfGacha presentation.</summary>
    BtnSelectSelfGacha = 414049910,

    /// <summary>Controls the ImgNewTagSelectSelfGacha presentation.</summary>
    ImgNewTagSelectSelfGacha = 454022659,

    /// <summary>Controls the ImgRedDotSelectSelfGacha presentation.</summary>
    ImgRedDotSelectSelfGacha = 43765714,

    /// <summary>Controls the BtnSkillResonance presentation.</summary>
    BtnSkillResonance = 1225401357,

    /// <summary>Controls the ImgNewTagSkillResonance presentation.</summary>
    ImgNewTagSkillResonance = 1626702223,

    /// <summary>Controls the ImgRedDotSkillResonance presentation.</summary>
    ImgRedDotSkillResonance = 1681821900,

    /// <summary>Controls the BtnNationalTournament presentation.</summary>
    BtnNationalTournament = 778676641,

    /// <summary>Controls the ImgNewTagNationalTournament presentation.</summary>
    ImgNewTagNationalTournament = 1143529667,

    /// <summary>Controls the ImgRedDotNationalTournament presentation.</summary>
    ImgRedDotNationalTournament = 149912668,

    /// <summary>Controls the ShopTips presentation.</summary>
    ShopTips = 128375330,

    /// <summary>Controls the AdsNode presentation.</summary>
    AdsNode = 1001429026,

    /// <summary>Controls the FuliTips presentation.</summary>
    FuliTips = 1995408800,

    /// <summary>Controls the _PaiHangNewTag presentation.</summary>
    PaiHangNewTag = 855911354,

    /// <summary>Controls the BeibaoTips presentation.</summary>
    BeibaoTips = 2016436025,

    /// <summary>Controls the NewTagBag presentation.</summary>
    NewTagBag = 2067430049,

    /// <summary>Controls the GalleryTips presentation.</summary>
    GalleryTips = 1375427408,

    /// <summary>Controls the TuJianNewTag presentation.</summary>
    TuJianNewTag = 833598051,

    /// <summary>Controls the _HonorTips presentation.</summary>
    HonorTips = 1344362679,

    /// <summary>Controls the HonorNewTag presentation.</summary>
    HonorNewTag = 230743909,

    /// <summary>Controls the JuqingNewTag presentation.</summary>
    JuqingNewTag = 340380090,

    /// <summary>Controls the ExpeditionTips presentation.</summary>
    ExpeditionTips = 615214434,

    /// <summary>Controls the SaishiNewTag presentation.</summary>
    SaishiNewTag = 1727165129,

    /// <summary>Controls the HesujidiNewTag presentation.</summary>
    HesujidiNewTag = 1317670684,

    /// <summary>Controls the HesujidiTips presentation.</summary>
    HesujidiTips = 2043321783,

    /// <summary>Controls the ZhaomuTips presentation.</summary>
    ZhaomuTips = 979659919,

    /// <summary>Controls the ZhaomuNewTag presentation.</summary>
    ZhaomuNewTag = 1612802959,

    /// <summary>Controls the DuiyuanNewTag presentation.</summary>
    DuiyuanNewTag = 1254032041,

    /// <summary>Controls the DuiwuNewTag presentation.</summary>
    DuiwuNewTag = 1683079727,

    /// <summary>Controls the _AssociationTips presentation.</summary>
    AssociationTips = 337575171,

    /// <summary>Controls the SheTuanNewTag presentation.</summary>
    SheTuanNewTag = 1892449151,

    /// <summary>Controls the DailyPlayTips presentation.</summary>
    DailyPlayTips = 244983610,

    /// <summary>Controls the HuodongNewTag presentation.</summary>
    HuodongNewTag = 1669826348,
}
