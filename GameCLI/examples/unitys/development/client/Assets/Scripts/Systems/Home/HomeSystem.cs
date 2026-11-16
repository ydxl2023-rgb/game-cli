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
        vm.Label063001 = CommonText.Get(Excel.HOME_TEXT_0_6_3_0_0_1);
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
