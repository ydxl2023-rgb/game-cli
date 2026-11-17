using Config;
using System.Collections.Generic;
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

        UnsubscribeAll();

        Subscribe<FunctionOpenChanged>(OnFunctionChanged);
        Subscribe<FunctionInputsUpdated>(OnInputsUpdated);

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

        var functions = XEntityManager.GetSingleton().QueryEntity<FunctionManagerEntity>();
        if (!functions)
            return false;

        entity.playerLevel = functions.level;

        vm.PlayerName = entity.playerName ?? string.Empty;
        vm.PlayerLevel = entity.playerLevel.ToString(CultureInfo.InvariantCulture);
        vm.CurrencyOneAmount = entity.currencyOneAmount.ToString(CultureInfo.InvariantCulture);
        vm.CurrencyTwoAmount = entity.currencyTwoAmount.ToString(CultureInfo.InvariantCulture);
        vm.CurrencyThreeAmount = entity.currencyThreeAmount.ToString(CultureInfo.InvariantCulture);
        vm.ChatMessage = entity.chatMessage ?? string.Empty;

        if (vm.AcitiveList == null || vm.AcitiveList.Count == 0)
        {
            var entries = new List<VMActivityEntry>();
            foreach (int id in widget.GetActivityIds())
            {
                int systemId = id;
                entries.Add(new VMActivityEntry {
                    systemId = systemId,
                    OpenClick = (sender) => OpenActivity(systemId),
                    Visible = functions.IsOpen(systemId)
                });
            }

            vm.AcitiveList = entries;
        }
        else
        {
            foreach (VMActivityEntry entry in vm.AcitiveList)
                entry.Visible = functions.IsOpen(entry.systemId);
        }

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

    /// <summary>Request an available activity through the system coordinator.</summary>
    /// <param name="id">FunctionSystem identifier.</param>
    /// <returns>True when an available, implemented target was requested.</returns>
    public bool OpenActivity(int id)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<FunctionManagerEntity>();
        if (!entity)
            return false;

        if (!IsActive() || !entity.IsOpen(id)
            || !Excel.functionSystemConfig.TryGetValue(id, out var row))
            return false;

        var type = FunctionManagerAccess.ResolveType(row);
        if (type == null)
            return false;

        return XPluginUtility.PostEvent(new SystemActive { type = type });
    }

    /// <summary>Refresh entries after a configured availability transition.</summary>
    /// <param name="data">Changed function.</param>
    private void OnFunctionChanged(FunctionOpenChanged data)
    {
        Refresh();
    }

    /// <summary>Refresh only after the availability service has applied the input message.</summary>
    /// <param name="data">Completed input update notification.</param>
    private void OnInputsUpdated(FunctionInputsUpdated data)
    {
        Refresh();
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
