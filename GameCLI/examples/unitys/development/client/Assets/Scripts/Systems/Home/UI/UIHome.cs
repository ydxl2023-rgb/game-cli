using System.Collections.Generic;
using System.Globalization;
using Oathx.Widget;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Displays the lobby and its independent promotion states.</summary>
[Config("Assets/Data/Widgets/Home/Home.prefab", WidgetLayer.center, cacheWidget: false)]
public class UIHome : UIWidget<VMHome>
{
    /// <summary>Activity list bound through VMHome.</summary>
    [SerializeField]
    private UIActivityEntry[] activities;

    /// <summary>The single image shared by both drop promotions.</summary>
    [SerializeField]
    private Image dropImage;

    /// <summary>Original double drop artwork.</summary>
    [SerializeField]
    private Sprite doubleDrop;

    /// <summary>Original triple drop artwork.</summary>
    [SerializeField]
    private Sprite tripleDrop;

    /// <summary>Bind lobby entry actions once per page view model.</summary>
    protected override bool Bind()
    {
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

        var entries = new List<VMActivityEntry>();
        foreach (int id in GetActivityIds())
        {
            entries.Add(new VMActivityEntry {
                systemId = id,
                OpenClick = (sender) => Send(new ReqHomeOpenActivity { systemId = id }),
                Interactable = true
            });
        }

        vm.AcitiveList = entries;

        return true;
    }

    /// <summary>Release the nested currency bar before the page binding ends.</summary>
    /// <param name="refVM">The page presentation being released.</param>
    public override void Unref(VM refVM)
    {
        if (vm)
        {
            vm.Profile = null;
            vm.CurrencyBar = null;
            vm.ProfileShortcuts = null;
            base.Unref(refVM);
        }
    }

    /// <summary>Read identifiers in the same order as the authored SyncGroup children.</summary>
    /// <returns>A detached ordered identifier list.</returns>
    public int[] GetActivityIds()
    {
        var ids = new int[activities.Length];
        for (int index = 0; index < activities.Length; index++)
            ids[index] = activities[index].GetSystemId();

        return ids;
    }

    /// <summary>Read authored promotion artwork and its native display size.</summary>
    /// <param name="triple">Whether to select the triple-drop artwork.</param>
    /// <param name="sprite">The selected authored sprite.</param>
    /// <param name="size">Native size in the target canvas units.</param>
    /// <returns>True when the authored presentation is available.</returns>
    public bool GetDropPresentation(bool triple, out Sprite sprite, out Vector2 size)
    {
        sprite = triple ? tripleDrop : doubleDrop;
        size = Vector2.zero;

        if (!dropImage || !sprite)
            return false;

        float referencePixels = dropImage.canvas ? dropImage.canvas.referencePixelsPerUnit : 100f;
        size = sprite.rect.size * referencePixels / sprite.pixelsPerUnit;

        return true;
    }

    /// <summary>Refresh lobby presentation and activity availability from current entities.</summary>
    public bool Invalidate(HomeEntity entity, FunctionManagerEntity functions)
    {
        if (!entity || !functions || !vm || !vm.Profile || !vm.ProfileShortcuts || !vm.CurrencyBar)
            return false;

        if (vm.CurrencyBar.Entries == null || vm.CurrencyBar.Entries.Count != 3 || vm.AcitiveList == null)
            return false;

        vm.Profile.PlayerName = entity.playerName ?? string.Empty;
        vm.Profile.PlayerLevel = entity.playerLevel.ToString(CultureInfo.InvariantCulture);
        vm.CurrencyBar.Entries[0].Amount = entity.currencyOneAmount.ToString(CultureInfo.InvariantCulture);
        vm.CurrencyBar.Entries[1].Amount = entity.currencyTwoAmount.ToString(CultureInfo.InvariantCulture);
        vm.CurrencyBar.Entries[2].Amount = entity.currencyThreeAmount.ToString(CultureInfo.InvariantCulture);
        vm.ChatMessage = entity.chatMessage ?? string.Empty;

        foreach (VMActivityEntry entry in vm.AcitiveList)
        {
            entry.Visible = functions.IsOpen(entry.systemId);
            entry.Badges = entity.GetBadges(entry.systemId);
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

        if (!GetDropPresentation(entity.dropState == HomeDropState.Triple, out var dropSprite, out var dropSize))
            return false;

        vm.DropSprite = dropSprite;
        vm.DropSize = dropSize;
        vm.DropVisible = entity.dropState == HomeDropState.Double || entity.dropState == HomeDropState.Triple;

        return true;
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
            OpenClick = (sender) => Send(new ReqHomeOpenDestination { entry = details }),
            AddClick = (sender) => Send(new ReqHomeOpenDestination { entry = acquire })
        };
    }

    /// <summary>Create one navigation VM while keeping business decisions outside the widget.</summary>
    /// <param name="entry">The requested local navigation destination.</param>
    /// <returns>A VM with the common entry interaction contract.</returns>
    private VMActivityEntry CreateEntry(HomeEntry entry)
    {
        return new VMActivityEntry {
            systemId = (int)entry,
            OpenClick = (sender) => Send(new ReqHomeOpenDestination { entry = entry }),
            Visible = true,
            Interactable = true
        };
    }
}

/// <summary>The player selected a lobby destination.</summary>
public class ReqHomeOpenDestination : UIRequestEvent
{
    /// <summary>Destination selected by the player.</summary>
    public HomeEntry entry
    { get; set; }
}

/// <summary>The player selected a configured lobby activity.</summary>
public class ReqHomeOpenActivity : UIRequestEvent
{
    /// <summary>Selected activity configuration identifier.</summary>
    public int systemId
    { get; set; }
}
