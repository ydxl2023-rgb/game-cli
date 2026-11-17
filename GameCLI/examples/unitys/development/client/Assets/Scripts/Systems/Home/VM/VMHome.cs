using Oathx.Widget;

/// <summary>Presentation bindings for the basic lobby.</summary>
public partial class VMHome : VM
{
    /// <summary>Request the Profile entry.</summary>
    [Binding]
    ButtonClick profileClick;

    /// <summary>Display PlayerName.</summary>
    [Binding(typeof(SyncText))]
    string playerName;

    /// <summary>Request the Avatar entry.</summary>
    [Binding]
    ButtonClick avatarClick;

    /// <summary>Display PlayerLevel.</summary>
    [Binding(typeof(SyncText))]
    string playerLevel;

    /// <summary>Request the Mail entry.</summary>
    [Binding]
    ButtonClick mailClick;

    /// <summary>Request the Settings entry.</summary>
    [Binding]
    ButtonClick settingsClick;

    /// <summary>Request the Notice entry.</summary>
    [Binding]
    ButtonClick noticeClick;

    /// <summary>Request the Friends entry.</summary>
    [Binding]
    ButtonClick friendsClick;

    /// <summary>Request the Affinity entry.</summary>
    [Binding]
    ButtonClick affinityClick;

    /// <summary>Request the CurrencyOne entry.</summary>
    [Binding]
    ButtonClick currencyOneClick;

    /// <summary>Display CurrencyOneAmount.</summary>
    [Binding(typeof(SyncText))]
    string currencyOneAmount;

    /// <summary>Request the AddCurrencyOne entry.</summary>
    [Binding]
    ButtonClick addCurrencyOneClick;

    /// <summary>Request the CurrencyTwo entry.</summary>
    [Binding]
    ButtonClick currencyTwoClick;

    /// <summary>Display CurrencyTwoAmount.</summary>
    [Binding(typeof(SyncText))]
    string currencyTwoAmount;

    /// <summary>Request the AddCurrencyTwo entry.</summary>
    [Binding]
    ButtonClick addCurrencyTwoClick;

    /// <summary>Request the CurrencyThree entry.</summary>
    [Binding]
    ButtonClick currencyThreeClick;

    /// <summary>Display CurrencyThreeAmount.</summary>
    [Binding(typeof(SyncText))]
    string currencyThreeAmount;

    /// <summary>Request the AddCurrencyThree entry.</summary>
    [Binding]
    ButtonClick addCurrencyThreeClick;

    /// <summary>Request the TaskButton entry.</summary>
    [Binding]
    ButtonClick taskButtonClick;

    /// <summary>Display Label062003.</summary>
    [Binding(typeof(SyncText))]
    string label062003;

    /// <summary>Request the SevendaysLogin entry.</summary>
    [Binding]
    ButtonClick sevendaysLoginClick;

    /// <summary>Request the SevendaysTask entry.</summary>
    [Binding]
    ButtonClick sevendaysTaskClick;

    /// <summary>Request the FourteendaysTask entry.</summary>
    [Binding]
    ButtonClick fourteendaysTaskClick;

    /// <summary>Display Label062032.</summary>
    [Binding(typeof(SyncText))]
    string label062032;

    /// <summary>Request the DirectPurchaseButton entry.</summary>
    [Binding]
    ButtonClick directPurchaseButtonClick;

    /// <summary>Display Label0620420.</summary>
    [Binding(typeof(SyncText))]
    string label0620420;

    /// <summary>Request the DirectPurchaseButton1 entry.</summary>
    [Binding]
    ButtonClick directPurchaseButton1Click;

    /// <summary>Display Label062052.</summary>
    [Binding(typeof(SyncText))]
    string label062052;

    /// <summary>Display Label0620530.</summary>
    [Binding(typeof(SyncText))]
    string label0620530;

    /// <summary>Request the CNBattlePassButton entry.</summary>
    [Binding]
    ButtonClick cNBattlePassButtonClick;

    /// <summary>Display Label062062.</summary>
    [Binding(typeof(SyncText))]
    string label062062;

    /// <summary>Display Label0620630.</summary>
    [Binding(typeof(SyncText))]
    string label0620630;

    /// <summary>Request the TimeLimitGiftButton entry.</summary>
    [Binding]
    ButtonClick timeLimitGiftButtonClick;

    /// <summary>Display Label0620720.</summary>
    [Binding(typeof(SyncText))]
    string label0620720;

    /// <summary>Request the UpInProButton entry.</summary>
    [Binding]
    ButtonClick upInProButtonClick;

    /// <summary>Request the ComeBackButton entry.</summary>
    [Binding]
    ButtonClick comeBackButtonClick;

    /// <summary>Display Label0620900.</summary>
    [Binding(typeof(SyncText))]
    string label0620900;

    /// <summary>Request the BtnPowerfulBait entry.</summary>
    [Binding]
    ButtonClick btnPowerfulBaitClick;

    /// <summary>Display Label06201003.</summary>
    [Binding(typeof(SyncText))]
    string label06201003;

    /// <summary>Request the BtnKingSetter entry.</summary>
    [Binding]
    ButtonClick btnKingSetterClick;

    /// <summary>Display Label06201102.</summary>
    [Binding(typeof(SyncText))]
    string label06201102;

    /// <summary>Request the ZhanshuButton entry.</summary>
    [Binding]
    ButtonClick zhanshuButtonClick;

    /// <summary>Display Label062012000.</summary>
    [Binding(typeof(SyncText))]
    string label062012000;

    /// <summary>Display Label062012010.</summary>
    [Binding(typeof(SyncText))]
    string label062012010;

    /// <summary>Display Label062013000.</summary>
    [Binding(typeof(SyncText))]
    string label062013000;

    /// <summary>Display Label0620130010.</summary>
    [Binding(typeof(SyncText))]
    string label0620130010;

    /// <summary>Display Label062013002.</summary>
    [Binding(typeof(SyncText))]
    string label062013002;

    /// <summary>Display Label0620130050.</summary>
    [Binding(typeof(SyncText))]
    string label0620130050;

    /// <summary>Request the FateBattleButton entry.</summary>
    [Binding]
    ButtonClick fateBattleButtonClick;

    /// <summary>Display Label062013010.</summary>
    [Binding(typeof(SyncText))]
    string label062013010;

    /// <summary>Request the FateBattleDlcBtn entry.</summary>
    [Binding]
    ButtonClick fateBattleDlcBtnClick;

    /// <summary>Request the AppearBtn entry.</summary>
    [Binding]
    ButtonClick appearBtnClick;

    /// <summary>Display Label062014020.</summary>
    [Binding(typeof(SyncText))]
    string label062014020;

    /// <summary>Request the BtnPlayerTheme entry.</summary>
    [Binding]
    ButtonClick btnPlayerThemeClick;

    /// <summary>Display Label06201502.</summary>
    [Binding(typeof(SyncText))]
    string label06201502;

    /// <summary>Request the BtnTurnCard entry.</summary>
    [Binding]
    ButtonClick btnTurnCardClick;

    /// <summary>Display Label06201600.</summary>
    [Binding(typeof(SyncText))]
    string label06201600;

    /// <summary>Request the AliPayPackageBtn entry.</summary>
    [Binding]
    ButtonClick aliPayPackageBtnClick;

    /// <summary>Request the QuestionBtn entry.</summary>
    [Binding]
    ButtonClick questionBtnClick;

    /// <summary>Request the BtnPreOrder entry.</summary>
    [Binding]
    ButtonClick btnPreOrderClick;

    /// <summary>Request the BtnMatchGuess entry.</summary>
    [Binding]
    ButtonClick btnMatchGuessClick;

    /// <summary>Request the BtnTopBattle entry.</summary>
    [Binding]
    ButtonClick btnTopBattleClick;

    /// <summary>Display Label06202100.</summary>
    [Binding(typeof(SyncText))]
    string label06202100;

    /// <summary>Request the BtnServeGame entry.</summary>
    [Binding]
    ButtonClick btnServeGameClick;

    /// <summary>Display Label06202200.</summary>
    [Binding(typeof(SyncText))]
    string label06202200;

    /// <summary>Request the BtnNewYear entry.</summary>
    [Binding]
    ButtonClick btnNewYearClick;

    /// <summary>Display Label06202300.</summary>
    [Binding(typeof(SyncText))]
    string label06202300;

    /// <summary>Request the BtnChainGift entry.</summary>
    [Binding]
    ButtonClick btnChainGiftClick;

    /// <summary>Display Label06202400.</summary>
    [Binding(typeof(SyncText))]
    string label06202400;

    /// <summary>Request the BtnSPTopic entry.</summary>
    [Binding]
    ButtonClick btnSPTopicClick;

    /// <summary>Display Label06202500.</summary>
    [Binding(typeof(SyncText))]
    string label06202500;

    /// <summary>Request the BtnAnniversary entry.</summary>
    [Binding]
    ButtonClick btnAnniversaryClick;

    /// <summary>Display Label06202600.</summary>
    [Binding(typeof(SyncText))]
    string label06202600;

    /// <summary>Request the BtnFreeMatch entry.</summary>
    [Binding]
    ButtonClick btnFreeMatchClick;

    /// <summary>Request the BtnRunningGame entry.</summary>
    [Binding]
    ButtonClick btnRunningGameClick;

    /// <summary>Display Label06202800.</summary>
    [Binding(typeof(SyncText))]
    string label06202800;

    /// <summary>Request the Btn15AnniversaryActive entry.</summary>
    [Binding]
    ButtonClick btn15AnniversaryActiveClick;

    /// <summary>Display Label06202900.</summary>
    [Binding(typeof(SyncText))]
    string label06202900;

    /// <summary>Request the BtnLuckyDraw entry.</summary>
    [Binding]
    ButtonClick btnLuckyDrawClick;

    /// <summary>Display Label06203000.</summary>
    [Binding(typeof(SyncText))]
    string label06203000;

    /// <summary>Request the BtnWuYeCanteen entry.</summary>
    [Binding]
    ButtonClick btnWuYeCanteenClick;

    /// <summary>Display Label06203100.</summary>
    [Binding(typeof(SyncText))]
    string label06203100;

    /// <summary>Request the BtnAceDuel entry.</summary>
    [Binding]
    ButtonClick btnAceDuelClick;

    /// <summary>Display Label06203200.</summary>
    [Binding(typeof(SyncText))]
    string label06203200;

    /// <summary>Request the BtnCampTraining entry.</summary>
    [Binding]
    ButtonClick btnCampTrainingClick;

    /// <summary>Display Label06203300.</summary>
    [Binding(typeof(SyncText))]
    string label06203300;

    /// <summary>Request the BtnSpecialOfferPackage entry.</summary>
    [Binding]
    ButtonClick btnSpecialOfferPackageClick;

    /// <summary>Display Label06203400.</summary>
    [Binding(typeof(SyncText))]
    string label06203400;

    /// <summary>Request the BtnBarbecue entry.</summary>
    [Binding]
    ButtonClick btnBarbecueClick;

    /// <summary>Display Label06203500.</summary>
    [Binding(typeof(SyncText))]
    string label06203500;

    /// <summary>Request the BtnCombinationGift entry.</summary>
    [Binding]
    ButtonClick btnCombinationGiftClick;

    /// <summary>Display Label06203600.</summary>
    [Binding(typeof(SyncText))]
    string label06203600;

    /// <summary>Request the BtnSelectSelfGift entry.</summary>
    [Binding]
    ButtonClick btnSelectSelfGiftClick;

    /// <summary>Display Label06203700.</summary>
    [Binding(typeof(SyncText))]
    string label06203700;

    /// <summary>Request the BtnSelectSelfGacha entry.</summary>
    [Binding]
    ButtonClick btnSelectSelfGachaClick;

    /// <summary>Display Label06203800.</summary>
    [Binding(typeof(SyncText))]
    string label06203800;

    /// <summary>Request the BtnSkillResonance entry.</summary>
    [Binding]
    ButtonClick btnSkillResonanceClick;

    /// <summary>Display Label06203900.</summary>
    [Binding(typeof(SyncText))]
    string label06203900;

    /// <summary>Request the BtnNationalTournament entry.</summary>
    [Binding]
    ButtonClick btnNationalTournamentClick;

    /// <summary>Display Label06204000.</summary>
    [Binding(typeof(SyncText))]
    string label06204000;

    /// <summary>Request the Shop entry.</summary>
    [Binding]
    ButtonClick shopClick;

    /// <summary>Display Label063001.</summary>
    [Binding(typeof(SyncText))]
    string label063001;

    /// <summary>Display Label063021.</summary>
    [Binding(typeof(SyncText))]
    string label063021;

    /// <summary>Request the Benefits entry.</summary>
    [Binding]
    ButtonClick benefitsClick;

    /// <summary>Display Label06310.</summary>
    [Binding(typeof(SyncText))]
    string label06310;

    /// <summary>Request the Rankings entry.</summary>
    [Binding]
    ButtonClick rankingsClick;

    /// <summary>Display Label06320.</summary>
    [Binding(typeof(SyncText))]
    string label06320;

    /// <summary>Request the Chat entry.</summary>
    [Binding]
    ButtonClick chatClick;

    /// <summary>Request the ChatChannel entry.</summary>
    [Binding]
    ButtonClick chatChannelClick;

    /// <summary>Display ChatMessage.</summary>
    [Binding(typeof(SyncText))]
    string chatMessage;

    /// <summary>Request the Backpack entry.</summary>
    [Binding]
    ButtonClick backpackClick;

    /// <summary>Display Label06410.</summary>
    [Binding(typeof(SyncText))]
    string label06410;

    /// <summary>Request the Collection entry.</summary>
    [Binding]
    ButtonClick collectionClick;

    /// <summary>Display Label06420.</summary>
    [Binding(typeof(SyncText))]
    string label06420;

    /// <summary>Request the Honor entry.</summary>
    [Binding]
    ButtonClick honorClick;

    /// <summary>Display Label06430.</summary>
    [Binding(typeof(SyncText))]
    string label06430;

    /// <summary>Request the Story entry.</summary>
    [Binding]
    ButtonClick storyClick;

    /// <summary>Display Label06444.</summary>
    [Binding(typeof(SyncText))]
    string label06444;

    /// <summary>Request the Matches entry.</summary>
    [Binding]
    ButtonClick matchesClick;

    /// <summary>Request the Training entry.</summary>
    [Binding]
    ButtonClick trainingClick;

    /// <summary>Request the Recruit entry.</summary>
    [Binding]
    ButtonClick recruitClick;

    /// <summary>Request the Players entry.</summary>
    [Binding]
    ButtonClick playersClick;

    /// <summary>Display Label06534.</summary>
    [Binding(typeof(SyncText))]
    string label06534;

    /// <summary>Request the Team entry.</summary>
    [Binding]
    ButtonClick teamClick;

    /// <summary>Display Label06544.</summary>
    [Binding(typeof(SyncText))]
    string label06544;

    /// <summary>Request the Club entry.</summary>
    [Binding]
    ButtonClick clubClick;

    /// <summary>Display Label06550.</summary>
    [Binding(typeof(SyncText))]
    string label06550;

    /// <summary>Request the Activities entry.</summary>
    [Binding]
    ButtonClick activitiesClick;

    /// <summary>Display Label06560.</summary>
    [Binding(typeof(SyncText))]
    string label06560;
}
