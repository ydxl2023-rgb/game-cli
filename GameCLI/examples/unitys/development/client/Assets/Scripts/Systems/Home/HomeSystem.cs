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

        if (!vm.Profile || !vm.ProfileShortcuts || !vm.CurrencyBar || vm.CurrencyBar.Entries == null || vm.CurrencyBar.Entries.Count != 3)
            return false;

        entity.playerLevel = functions.level;

        vm.Profile.PlayerName = entity.playerName ?? string.Empty;
        vm.Profile.PlayerLevel = entity.playerLevel.ToString(CultureInfo.InvariantCulture);
        vm.CurrencyBar.Entries[0].Amount = entity.currencyOneAmount.ToString(CultureInfo.InvariantCulture);
        vm.CurrencyBar.Entries[1].Amount = entity.currencyTwoAmount.ToString(CultureInfo.InvariantCulture);
        vm.CurrencyBar.Entries[2].Amount = entity.currencyThreeAmount.ToString(CultureInfo.InvariantCulture);
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
                    Visible = functions.IsOpen(systemId),
                    Interactable = true,
                    Badges = entity.GetBadges(systemId)
                });
            }

            vm.AcitiveList = entries;
        }
        else
        {
            foreach (VMActivityEntry entry in vm.AcitiveList)
            {
                entry.Visible = functions.IsOpen(entry.systemId);
                entry.Badges = entity.GetBadges(entry.systemId);
            }
        }

        vm.Profile.ProfileEntry.Badges = entity.GetBadges((int)HomeEntry.Profile);
        vm.Profile.AvatarEntry.Badges = entity.GetBadges((int)HomeEntry.Avatar);
        vm.ProfileShortcuts.MailEntry.Badges = entity.GetBadges((int)HomeEntry.Mail);
        vm.ProfileShortcuts.SettingsEntry.Badges = entity.GetBadges((int)HomeEntry.Settings);
        vm.ProfileShortcuts.NoticeEntry.Badges = entity.GetBadges((int)HomeEntry.Notice);
        vm.ProfileShortcuts.FriendsEntry.Badges = entity.GetBadges((int)HomeEntry.Friends);
        vm.ProfileShortcuts.AffinityEntry.Badges = entity.GetBadges((int)HomeEntry.Affinity);
        vm.ShopEntry.Badges = entity.GetBadges((int)HomeEntry.Shop);
        vm.BenefitsEntry.Badges = entity.GetBadges((int)HomeEntry.Benefits);
        vm.RankingsEntry.Badges = entity.GetBadges((int)HomeEntry.Rankings);
        vm.ChatEntry.Badges = entity.GetBadges((int)HomeEntry.Chat);
        vm.ChatChannelEntry.Badges = entity.GetBadges((int)HomeEntry.ChatChannel);
        vm.BackpackEntry.Badges = entity.GetBadges((int)HomeEntry.Backpack);
        vm.CollectionEntry.Badges = entity.GetBadges((int)HomeEntry.Collection);
        vm.HonorEntry.Badges = entity.GetBadges((int)HomeEntry.Honor);
        vm.StoryEntry.Badges = entity.GetBadges((int)HomeEntry.Story);
        vm.MatchesEntry.Badges = entity.GetBadges((int)HomeEntry.Matches);
        vm.TrainingEntry.Badges = entity.GetBadges((int)HomeEntry.Training);
        vm.RecruitEntry.Badges = entity.GetBadges((int)HomeEntry.Recruit);
        vm.PlayersEntry.Badges = entity.GetBadges((int)HomeEntry.Players);
        vm.TeamEntry.Badges = entity.GetBadges((int)HomeEntry.Team);
        vm.ClubEntry.Badges = entity.GetBadges((int)HomeEntry.Club);
        vm.ActivitiesEntry.Badges = entity.GetBadges((int)HomeEntry.Activities);

        if (!widget.GetDropPresentation(entity.dropState == HomeDropState.Triple, out var dropSprite, out var dropSize))
            return false;

        vm.DropSprite = dropSprite;
        vm.DropSize = dropSize;
        vm.DropVisible = entity.dropState == HomeDropState.Double || entity.dropState == HomeDropState.Triple;

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

    /// <summary>Supply currency interactions without placing navigation inside the component.</summary>
    /// <param name="id">Product currency identifier.</param>
    /// <param name="details">Existing details destination.</param>
    /// <param name="acquire">Existing acquisition destination.</param>
    /// <returns>The host-owned entry presentation.</returns>
    private VMCurrencyEntry CreateCurrency(int id, HomeEntry details, HomeEntry acquire)
    {
        return new VMCurrencyEntry {
            currencyId = id,
            Visible = true,
            CanAdd = true,
            Interactable = true,
            OpenClick = (sender) => OpenEntry(details),
            AddClick = (sender) => OpenEntry(acquire)
        };
    }

    /// <summary>Create one navigation VM while keeping business decisions outside the widget.</summary>
    /// <param name="entry">The requested local navigation destination.</param>
    /// <returns>A VM with the common entry interaction contract.</returns>
    private VMActivityEntry CreateEntry(HomeEntry entry)
    {
        return new VMActivityEntry {
            systemId = (int)entry,
            OpenClick = (sender) => OpenEntry(entry),
            Visible = true,
            Interactable = true
        };
    }

    /// <summary>Bind the loaded page to local navigation.</summary>
    /// <param name="widget">The newly opened lobby widget.</param>
    /// <returns>True when every declared button callback was assigned.</returns>
    private bool Bind(UIHome widget)
    {
        var vm = widget.vm;
        if (!vm)
            return false;

        vm.CurrencyBar = new VMCurrencyBar {
            Entries = new List<VMCurrencyEntry> {
                CreateCurrency(1, HomeEntry.CurrencyOne, HomeEntry.AddCurrencyOne),
                CreateCurrency(2, HomeEntry.CurrencyTwo, HomeEntry.AddCurrencyTwo),
                CreateCurrency(3, HomeEntry.CurrencyThree, HomeEntry.AddCurrencyThree)
            }
        };

        vm.Profile = new VMProfile {
            ProfileEntry = CreateEntry(HomeEntry.Profile),
            AvatarEntry = CreateEntry(HomeEntry.Avatar)
        };

        vm.ProfileShortcuts = new VMProfileShortcuts {
            MailEntry = CreateEntry(HomeEntry.Mail),
            SettingsEntry = CreateEntry(HomeEntry.Settings),
            NoticeEntry = CreateEntry(HomeEntry.Notice),
            FriendsEntry = CreateEntry(HomeEntry.Friends),
            AffinityEntry = CreateEntry(HomeEntry.Affinity)
        };

        vm.ShopEntry = CreateEntry(HomeEntry.Shop);
        vm.BenefitsEntry = CreateEntry(HomeEntry.Benefits);
        vm.RankingsEntry = CreateEntry(HomeEntry.Rankings);
        vm.ChatEntry = CreateEntry(HomeEntry.Chat);
        vm.ChatChannelEntry = CreateEntry(HomeEntry.ChatChannel);
        vm.BackpackEntry = CreateEntry(HomeEntry.Backpack);
        vm.CollectionEntry = CreateEntry(HomeEntry.Collection);
        vm.HonorEntry = CreateEntry(HomeEntry.Honor);
        vm.StoryEntry = CreateEntry(HomeEntry.Story);
        vm.MatchesEntry = CreateEntry(HomeEntry.Matches);
        vm.TrainingEntry = CreateEntry(HomeEntry.Training);
        vm.RecruitEntry = CreateEntry(HomeEntry.Recruit);
        vm.PlayersEntry = CreateEntry(HomeEntry.Players);
        vm.TeamEntry = CreateEntry(HomeEntry.Team);
        vm.ClubEntry = CreateEntry(HomeEntry.Club);
        vm.ActivitiesEntry = CreateEntry(HomeEntry.Activities);

        return true;
    }
}
