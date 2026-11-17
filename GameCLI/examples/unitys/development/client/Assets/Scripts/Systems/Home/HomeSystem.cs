using Config;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using System.Globalization;

/// <summary>Coordinates lobby data and emits local navigation requests.</summary>
public class HomeSystem : RuntimeSystem
{
    /// <summary>Request the lobby while this system is active.</summary>
    /// <returns>True when page loading was requested.</returns>
    public override bool OnActive()
    {
        if (!IsActive())
            return false;

        UISystem.GetSingleton().OpenWidget<UIHome>((widget) => {
            if (widget)
            {
                Bind(widget);
                Refresh();
            }
        });

        return true;
    }

    /// <summary>Release subscriptions and close the owned page.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        System.Type[] widgets = new System.Type[] {
            typeof(UIHome)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
    }

    /// <summary>Refresh the open lobby from the current entity.</summary>
    /// <returns>True when the visible page was refreshed.</returns>
    public bool Refresh()
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            return false;

        var vm = widget.vm;
        if (!vm)
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (!entity)
            return false;

        vm.PlayerName = entity.playerName ?? string.Empty;
        vm.PlayerLevel = entity.playerLevel.ToString(CultureInfo.InvariantCulture);
        vm.CurrencyOneAmount = entity.currencyOneAmount.ToString(CultureInfo.InvariantCulture);
        vm.CurrencyTwoAmount = entity.currencyTwoAmount.ToString(CultureInfo.InvariantCulture);
        vm.CurrencyThreeAmount = entity.currencyThreeAmount.ToString(CultureInfo.InvariantCulture);
        vm.Label062003 = entity.label062003 ?? string.Empty;
        vm.Label062032 = entity.label062032 ?? string.Empty;
        vm.Label0620420 = entity.label0620420 ?? string.Empty;
        vm.Label062052 = CommonText.Get(Excel.HOME_TEXT_0_6_2_0_5_2);
        vm.Label0620530 = entity.label0620530 ?? string.Empty;
        vm.Label062062 = CommonText.Get(Excel.HOME_TEXT_0_6_2_0_6_2);
        vm.Label0620630 = entity.label0620630 ?? string.Empty;
        vm.Label0620720 = entity.label0620720 ?? string.Empty;
        vm.Label0620900 = entity.label0620900 ?? string.Empty;
        vm.Label06201003 = entity.label06201003 ?? string.Empty;
        vm.Label06201102 = entity.label06201102 ?? string.Empty;
        vm.Label062012000 = CommonText.Get(Excel.HOME_TEXT_0_6_2_0_12_0_0_0);
        vm.Label062012010 = CommonText.Get(Excel.HOME_TEXT_0_6_2_0_12_0_1_0);
        vm.Label062013000 = CommonText.Get(Excel.HOME_TEXT_0_6_2_0_13_0_0_0);
        vm.Label0620130010 = entity.label0620130010 ?? string.Empty;
        vm.Label062013002 = entity.label062013002 ?? string.Empty;
        vm.Label0620130050 = CommonText.Get(Excel.HOME_TEXT_0_6_2_0_13_0_0_5_0);
        vm.Label062013010 = CommonText.Get(Excel.HOME_TEXT_0_6_2_0_13_0_1_0);
        vm.Label062014020 = CommonText.Get(Excel.HOME_TEXT_0_6_2_0_14_0_2_0);
        vm.Label06201502 = entity.label06201502 ?? string.Empty;
        vm.Label06201600 = entity.label06201600 ?? string.Empty;
        vm.Label06202100 = entity.label06202100 ?? string.Empty;
        vm.Label06202200 = entity.label06202200 ?? string.Empty;
        vm.Label06202300 = entity.label06202300 ?? string.Empty;
        vm.Label06202400 = entity.label06202400 ?? string.Empty;
        vm.Label06202500 = entity.label06202500 ?? string.Empty;
        vm.Label06202600 = entity.label06202600 ?? string.Empty;
        vm.Label06202800 = entity.label06202800 ?? string.Empty;
        vm.Label06202900 = entity.label06202900 ?? string.Empty;
        vm.Label06203000 = entity.label06203000 ?? string.Empty;
        vm.Label06203100 = entity.label06203100 ?? string.Empty;
        vm.Label06203200 = entity.label06203200 ?? string.Empty;
        vm.Label06203300 = entity.label06203300 ?? string.Empty;
        vm.Label06203400 = entity.label06203400 ?? string.Empty;
        vm.Label06203500 = entity.label06203500 ?? string.Empty;
        vm.Label06203600 = entity.label06203600 ?? string.Empty;
        vm.Label06203700 = entity.label06203700 ?? string.Empty;
        vm.Label06203800 = entity.label06203800 ?? string.Empty;
        vm.Label06203900 = entity.label06203900 ?? string.Empty;
        vm.Label06204000 = entity.label06204000 ?? string.Empty;
        vm.Label063001 = CommonText.Get(Excel.HOME_TEXT_0_6_3_0_0_1);
        vm.Label063021 = CommonText.Get(Excel.HOME_TEXT_0_6_3_0_2_1);
        vm.Label06310 = CommonText.Get(Excel.HOME_TEXT_0_6_3_1_0);
        vm.Label06320 = CommonText.Get(Excel.HOME_TEXT_0_6_3_2_0);
        vm.ChatMessage = entity.chatMessage ?? string.Empty;
        vm.Label06410 = CommonText.Get(Excel.HOME_TEXT_0_6_4_1_0);
        vm.Label06420 = CommonText.Get(Excel.HOME_TEXT_0_6_4_2_0);
        vm.Label06430 = CommonText.Get(Excel.HOME_TEXT_0_6_4_3_0);
        vm.Label06444 = CommonText.Get(Excel.HOME_TEXT_0_6_4_4_4);
        vm.Label06534 = CommonText.Get(Excel.HOME_TEXT_0_6_5_3_4);
        vm.Label06544 = CommonText.Get(Excel.HOME_TEXT_0_6_5_4_4);
        vm.Label06550 = CommonText.Get(Excel.HOME_TEXT_0_6_5_5_0);
        vm.Label06560 = CommonText.Get(Excel.HOME_TEXT_0_6_5_6_0);

        if (!widget.RefreshPresentation(entity))
            return false;

        XPluginUtility.FireEvent(new HomePresentationRequested() {
            characterKey = entity.characterKey,
            seasonKey = entity.seasonKey
        });

        return true;
    }

    /// <summary>Emit a navigation request without starting an unimplemented system.</summary>
    /// <param name="entry">The destination requested by the caller.</param>
    /// <returns>True when the local request was emitted.</returns>
    public bool OpenEntry(HomeEntry entry)
    {
        if (!IsActive() || !System.Enum.IsDefined(typeof(HomeEntry), entry))
            return false;

        XPluginUtility.FireEvent(new HomeEntryRequested() {
            entry = entry
        });

        return true;
    }

    /// <summary>Bind the loaded page to local navigation.</summary>
    /// <param name="widget">The newly opened lobby widget.</param>
    /// <returns>True when every declared button callback was assigned.</returns>
    private bool Bind(UIHome widget)
    {
        var vm = widget.vm;
        if (!vm)
            return false;

        vm.ProfileClick = (sender) => OpenEntry(HomeEntry.Profile);
        vm.AvatarClick = (sender) => OpenEntry(HomeEntry.Avatar);
        vm.MailClick = (sender) => OpenEntry(HomeEntry.Mail);
        vm.SettingsClick = (sender) => OpenEntry(HomeEntry.Settings);
        vm.NoticeClick = (sender) => OpenEntry(HomeEntry.Notice);
        vm.FriendsClick = (sender) => OpenEntry(HomeEntry.Friends);
        vm.AffinityClick = (sender) => OpenEntry(HomeEntry.Affinity);
        vm.CurrencyOneClick = (sender) => OpenEntry(HomeEntry.CurrencyOne);
        vm.AddCurrencyOneClick = (sender) => OpenEntry(HomeEntry.AddCurrencyOne);
        vm.CurrencyTwoClick = (sender) => OpenEntry(HomeEntry.CurrencyTwo);
        vm.AddCurrencyTwoClick = (sender) => OpenEntry(HomeEntry.AddCurrencyTwo);
        vm.CurrencyThreeClick = (sender) => OpenEntry(HomeEntry.CurrencyThree);
        vm.AddCurrencyThreeClick = (sender) => OpenEntry(HomeEntry.AddCurrencyThree);
        vm.TaskButtonClick = (sender) => OpenEntry(HomeEntry.TaskButton);
        vm.SevendaysLoginClick = (sender) => OpenEntry(HomeEntry.SevendaysLogin);
        vm.SevendaysTaskClick = (sender) => OpenEntry(HomeEntry.SevendaysTask);
        vm.FourteendaysTaskClick = (sender) => OpenEntry(HomeEntry.FourteendaysTask);
        vm.DirectPurchaseButtonClick = (sender) => OpenEntry(HomeEntry.DirectPurchaseButton);
        vm.DirectPurchaseButton1Click = (sender) => OpenEntry(HomeEntry.DirectPurchaseButton1);
        vm.CNBattlePassButtonClick = (sender) => OpenEntry(HomeEntry.CNBattlePassButton);
        vm.TimeLimitGiftButtonClick = (sender) => OpenEntry(HomeEntry.TimeLimitGiftButton);
        vm.UpInProButtonClick = (sender) => OpenEntry(HomeEntry.UpInProButton);
        vm.ComeBackButtonClick = (sender) => OpenEntry(HomeEntry.ComeBackButton);
        vm.BtnPowerfulBaitClick = (sender) => OpenEntry(HomeEntry.BtnPowerfulBait);
        vm.BtnKingSetterClick = (sender) => OpenEntry(HomeEntry.BtnKingSetter);
        vm.ZhanshuButtonClick = (sender) => OpenEntry(HomeEntry.ZhanshuButton);
        vm.FateBattleButtonClick = (sender) => OpenEntry(HomeEntry.FateBattleButton);
        vm.FateBattleDlcBtnClick = (sender) => OpenEntry(HomeEntry.FateBattleDlcBtn);
        vm.AppearBtnClick = (sender) => OpenEntry(HomeEntry.AppearBtn);
        vm.BtnPlayerThemeClick = (sender) => OpenEntry(HomeEntry.BtnPlayerTheme);
        vm.BtnTurnCardClick = (sender) => OpenEntry(HomeEntry.BtnTurnCard);
        vm.AliPayPackageBtnClick = (sender) => OpenEntry(HomeEntry.AliPayPackageBtn);
        vm.QuestionBtnClick = (sender) => OpenEntry(HomeEntry.QuestionBtn);
        vm.BtnPreOrderClick = (sender) => OpenEntry(HomeEntry.BtnPreOrder);
        vm.BtnMatchGuessClick = (sender) => OpenEntry(HomeEntry.BtnMatchGuess);
        vm.BtnTopBattleClick = (sender) => OpenEntry(HomeEntry.BtnTopBattle);
        vm.BtnServeGameClick = (sender) => OpenEntry(HomeEntry.BtnServeGame);
        vm.BtnNewYearClick = (sender) => OpenEntry(HomeEntry.BtnNewYear);
        vm.BtnChainGiftClick = (sender) => OpenEntry(HomeEntry.BtnChainGift);
        vm.BtnSPTopicClick = (sender) => OpenEntry(HomeEntry.BtnSPTopic);
        vm.BtnAnniversaryClick = (sender) => OpenEntry(HomeEntry.BtnAnniversary);
        vm.BtnFreeMatchClick = (sender) => OpenEntry(HomeEntry.BtnFreeMatch);
        vm.BtnRunningGameClick = (sender) => OpenEntry(HomeEntry.BtnRunningGame);
        vm.Btn15AnniversaryActiveClick = (sender) => OpenEntry(HomeEntry.Btn15AnniversaryActive);
        vm.BtnLuckyDrawClick = (sender) => OpenEntry(HomeEntry.BtnLuckyDraw);
        vm.BtnWuYeCanteenClick = (sender) => OpenEntry(HomeEntry.BtnWuYeCanteen);
        vm.BtnAceDuelClick = (sender) => OpenEntry(HomeEntry.BtnAceDuel);
        vm.BtnCampTrainingClick = (sender) => OpenEntry(HomeEntry.BtnCampTraining);
        vm.BtnSpecialOfferPackageClick = (sender) => OpenEntry(HomeEntry.BtnSpecialOfferPackage);
        vm.BtnBarbecueClick = (sender) => OpenEntry(HomeEntry.BtnBarbecue);
        vm.BtnCombinationGiftClick = (sender) => OpenEntry(HomeEntry.BtnCombinationGift);
        vm.BtnSelectSelfGiftClick = (sender) => OpenEntry(HomeEntry.BtnSelectSelfGift);
        vm.BtnSelectSelfGachaClick = (sender) => OpenEntry(HomeEntry.BtnSelectSelfGacha);
        vm.BtnSkillResonanceClick = (sender) => OpenEntry(HomeEntry.BtnSkillResonance);
        vm.BtnNationalTournamentClick = (sender) => OpenEntry(HomeEntry.BtnNationalTournament);
        vm.ShopClick = (sender) => OpenEntry(HomeEntry.Shop);
        vm.BenefitsClick = (sender) => OpenEntry(HomeEntry.Benefits);
        vm.RankingsClick = (sender) => OpenEntry(HomeEntry.Rankings);
        vm.ChatClick = (sender) => OpenEntry(HomeEntry.Chat);
        vm.ChatChannelClick = (sender) => OpenEntry(HomeEntry.ChatChannel);
        vm.BackpackClick = (sender) => OpenEntry(HomeEntry.Backpack);
        vm.CollectionClick = (sender) => OpenEntry(HomeEntry.Collection);
        vm.HonorClick = (sender) => OpenEntry(HomeEntry.Honor);
        vm.StoryClick = (sender) => OpenEntry(HomeEntry.Story);
        vm.MatchesClick = (sender) => OpenEntry(HomeEntry.Matches);
        vm.TrainingClick = (sender) => OpenEntry(HomeEntry.Training);
        vm.RecruitClick = (sender) => OpenEntry(HomeEntry.Recruit);
        vm.PlayersClick = (sender) => OpenEntry(HomeEntry.Players);
        vm.TeamClick = (sender) => OpenEntry(HomeEntry.Team);
        vm.ClubClick = (sender) => OpenEntry(HomeEntry.Club);
        vm.ActivitiesClick = (sender) => OpenEntry(HomeEntry.Activities);

        return true;
    }
}
