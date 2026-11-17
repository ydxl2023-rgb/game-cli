using Oathx.Core;

/// <summary>Stable local destinations exposed by the basic lobby.</summary>
public enum HomeEntry
{
    /// <summary>The Profile entry.</summary>
    Profile,

    /// <summary>The Avatar entry.</summary>
    Avatar,

    /// <summary>The Mail entry.</summary>
    Mail,

    /// <summary>The Settings entry.</summary>
    Settings,

    /// <summary>The Notice entry.</summary>
    Notice,

    /// <summary>The Friends entry.</summary>
    Friends,

    /// <summary>The Affinity entry.</summary>
    Affinity,

    /// <summary>The CurrencyOne entry.</summary>
    CurrencyOne,

    /// <summary>The AddCurrencyOne entry.</summary>
    AddCurrencyOne,

    /// <summary>The CurrencyTwo entry.</summary>
    CurrencyTwo,

    /// <summary>The AddCurrencyTwo entry.</summary>
    AddCurrencyTwo,

    /// <summary>The CurrencyThree entry.</summary>
    CurrencyThree,

    /// <summary>The AddCurrencyThree entry.</summary>
    AddCurrencyThree,

    /// <summary>The TaskButton entry.</summary>
    TaskButton,

    /// <summary>The SevendaysLogin entry.</summary>
    SevendaysLogin,

    /// <summary>The SevendaysTask entry.</summary>
    SevendaysTask,

    /// <summary>The FourteendaysTask entry.</summary>
    FourteendaysTask,

    /// <summary>The DirectPurchaseButton entry.</summary>
    DirectPurchaseButton,

    /// <summary>The DirectPurchaseButton1 entry.</summary>
    DirectPurchaseButton1,

    /// <summary>The CNBattlePassButton entry.</summary>
    CNBattlePassButton,

    /// <summary>The TimeLimitGiftButton entry.</summary>
    TimeLimitGiftButton,

    /// <summary>The UpInProButton entry.</summary>
    UpInProButton,

    /// <summary>The ComeBackButton entry.</summary>
    ComeBackButton,

    /// <summary>The BtnPowerfulBait entry.</summary>
    BtnPowerfulBait,

    /// <summary>The BtnKingSetter entry.</summary>
    BtnKingSetter,

    /// <summary>The ZhanshuButton entry.</summary>
    ZhanshuButton,

    /// <summary>The FateBattleButton entry.</summary>
    FateBattleButton,

    /// <summary>The FateBattleDlcBtn entry.</summary>
    FateBattleDlcBtn,

    /// <summary>The AppearBtn entry.</summary>
    AppearBtn,

    /// <summary>The BtnPlayerTheme entry.</summary>
    BtnPlayerTheme,

    /// <summary>The BtnTurnCard entry.</summary>
    BtnTurnCard,

    /// <summary>The AliPayPackageBtn entry.</summary>
    AliPayPackageBtn,

    /// <summary>The QuestionBtn entry.</summary>
    QuestionBtn,

    /// <summary>The BtnPreOrder entry.</summary>
    BtnPreOrder,

    /// <summary>The BtnMatchGuess entry.</summary>
    BtnMatchGuess,

    /// <summary>The BtnTopBattle entry.</summary>
    BtnTopBattle,

    /// <summary>The BtnServeGame entry.</summary>
    BtnServeGame,

    /// <summary>The BtnNewYear entry.</summary>
    BtnNewYear,

    /// <summary>The BtnChainGift entry.</summary>
    BtnChainGift,

    /// <summary>The BtnSPTopic entry.</summary>
    BtnSPTopic,

    /// <summary>The BtnAnniversary entry.</summary>
    BtnAnniversary,

    /// <summary>The BtnFreeMatch entry.</summary>
    BtnFreeMatch,

    /// <summary>The BtnRunningGame entry.</summary>
    BtnRunningGame,

    /// <summary>The Btn15AnniversaryActive entry.</summary>
    Btn15AnniversaryActive,

    /// <summary>The BtnLuckyDraw entry.</summary>
    BtnLuckyDraw,

    /// <summary>The BtnWuYeCanteen entry.</summary>
    BtnWuYeCanteen,

    /// <summary>The BtnAceDuel entry.</summary>
    BtnAceDuel,

    /// <summary>The BtnCampTraining entry.</summary>
    BtnCampTraining,

    /// <summary>The BtnSpecialOfferPackage entry.</summary>
    BtnSpecialOfferPackage,

    /// <summary>The BtnBarbecue entry.</summary>
    BtnBarbecue,

    /// <summary>The BtnCombinationGift entry.</summary>
    BtnCombinationGift,

    /// <summary>The BtnSelectSelfGift entry.</summary>
    BtnSelectSelfGift,

    /// <summary>The BtnSelectSelfGacha entry.</summary>
    BtnSelectSelfGacha,

    /// <summary>The BtnSkillResonance entry.</summary>
    BtnSkillResonance,

    /// <summary>The BtnNationalTournament entry.</summary>
    BtnNationalTournament,

    /// <summary>The Shop entry.</summary>
    Shop,

    /// <summary>The Benefits entry.</summary>
    Benefits,

    /// <summary>The Rankings entry.</summary>
    Rankings,

    /// <summary>The Chat entry.</summary>
    Chat,

    /// <summary>The ChatChannel entry.</summary>
    ChatChannel,

    /// <summary>The Backpack entry.</summary>
    Backpack,

    /// <summary>The Collection entry.</summary>
    Collection,

    /// <summary>The Honor entry.</summary>
    Honor,

    /// <summary>The Story entry.</summary>
    Story,

    /// <summary>The Matches entry.</summary>
    Matches,

    /// <summary>The Training entry.</summary>
    Training,

    /// <summary>The Recruit entry.</summary>
    Recruit,

    /// <summary>The Players entry.</summary>
    Players,

    /// <summary>The Team entry.</summary>
    Team,

    /// <summary>The Club entry.</summary>
    Club,

    /// <summary>The Activities entry.</summary>
    Activities,
}

/// <summary>Requests navigation; the product coordinator owns the destination system.</summary>
public class HomeEntryRequested : IEvent
{
    /// <summary>Entry requested by a real lobby button.</summary>
    public HomeEntry entry
    { get; set; }
}

/// <summary>Offers optional presentation keys to a product-specific renderer.</summary>
public class HomePresentationRequested : IEvent
{
    /// <summary>Character asset key; empty means no character presentation.</summary>
    public string characterKey
    { get; set; }

    /// <summary>Season asset key; empty keeps the static reference background.</summary>
    public string seasonKey
    { get; set; }
}
