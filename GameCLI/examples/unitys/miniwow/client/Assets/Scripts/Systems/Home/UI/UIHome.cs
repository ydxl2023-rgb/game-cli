using System;
using System.Collections.Generic;
using System.Linq;
using Oathx.Core;
using Oathx.Widget;
using UnityEngine;

/// <summary>Requests the existing map preview.</summary>
public sealed class ReqHomeMap : UIRequestEvent { }

/// <summary>Requests a return to the camp.</summary>
public sealed class ReqHomeCamp : UIRequestEvent { }

/// <summary>Requests a configured MainPage entry.</summary>
public sealed class ReqHomeEntry : UIRequestEvent
{
    /// <summary>Stable product entry identifier.</summary>
    public int entryId
    { get; set; }
}

/// <summary>Requests a non-list Home shortcut.</summary>
public sealed class ReqHomeShortcut : UIRequestEvent
{
    /// <summary>Shortcut route key.</summary>
    public string route
    { get; set; }
}

/// <summary>Announces an allowed destination intention, without claiming it opened.</summary>
public sealed class HomeEntryRequested : IEvent
{
    /// <summary>Configured entry identifier; zero for a fixed shortcut.</summary>
    public int entryId
    { get; set; }

    /// <summary>Product destination key.</summary>
    public string route
    { get; set; }
}

/// <summary>Projects Home snapshots into the original MainPage-inspired portrait UI.</summary>
[Config("Assets/Data/Widgets/Home/UIHome.prefab", WidgetLayer.center, cacheWidget: false)]
public sealed class UIHome : UIWidget<VMHome>
{
    /// <summary>Ordered configuration IDs authored with this prefab; prevents silent art/intent mismatches.</summary>
    [SerializeField]
    private string entryOrder;

    /// <summary>Binds fixed intentions exactly once.</summary>
    protected override bool Bind()
    {
        vm.MapClick = Send<ReqHomeMap>;
        vm.ReturnClick = Send<ReqHomeCamp>;
        vm.CloseFeedbackClick = button => {
            vm.FeedbackVisible = false;

            return true;
        };
        vm.SettingsClick = button => Send(new ReqHomeShortcut { route = "Settings" });
        vm.TaskClick = button => Send(new ReqHomeShortcut { route = "Task" });
        vm.ChatClick = button => Send(new ReqHomeShortcut { route = "Chat" });

        return true;
    }

    /// <summary>Resets transient page presentation on activation.</summary>
    public override bool Active()
    {
        if (!base.Active())
            return false;

        ShowMap(false);
        vm.FeedbackVisible = false;

        return true;
    }

    /// <summary>Updates existing bindings without rebinding callbacks or list items.</summary>
    public void Invalidate(HomeSnapshot snapshot)
    {
        if (snapshot == null || snapshot.entries == null || entryOrder != string.Join(",", snapshot.entries.Select(entry => entry.id)))
            throw new InvalidOperationException("Home configuration IDs differ from the authored prefab; regenerate and bind UIHome.");

        if (vm.Entries == null)
        {
            var entries = new List<VMHomeEntry>();
            foreach (var entry in snapshot.entries)
                entries.Add(new VMHomeEntry { entryId = entry.id });
            vm.Entries = new VList<VMHomeEntry>(entries);
        }

        int activityIndex = 0;
        int functionIndex = 0;

        for (int index = 0; index < snapshot.entries.Length; index++)
        {
            var entry = snapshot.entries[index];
            var presentation = vm.Entries[index];
            if (presentation.entryId != entry.id)
                throw new InvalidOperationException("Home entry order changed; regenerate its prefab.");

            presentation.Visible = entry.visible;
            presentation.Locked = !entry.unlocked;

            if (entry.group == nameof(HomeEntryGroup.Activity))
            {
                presentation.Position = new Vector2((activityIndex % 3) * 132, -(activityIndex / 3) * 142);
                if (entry.visible)
                    activityIndex++;
            }
            else if (entry.group == nameof(HomeEntryGroup.Function))
            {
                presentation.Position = new Vector2(0, -functionIndex * 112);
                if (entry.visible)
                    functionIndex++;
            }
        }

        vm.PlayerName = snapshot.profile.playerName;
        vm.GuildTitle = snapshot.profile.guildTitle;
        vm.Power = FormatAmount(snapshot.profile.power);
        vm.Gold = FormatAmount(snapshot.profile.gold);
        vm.Gem = FormatAmount(snapshot.profile.gem);
        vm.Energy = snapshot.profile.energy + "/" + snapshot.profile.energyLimit;
        vm.TaskProgress = snapshot.profile.taskProgress;
        vm.Chat = snapshot.profile.chat;
    }

    /// <summary>Formats the original compact ten-thousand unit convention.</summary>
    private static string FormatAmount(long amount)
    {
        return amount >= 10000 ? (amount / 10000d).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "万" : amount.ToString();
    }

    /// <summary>Switches between the camp and existing map preview.</summary>
    public bool ShowMap(bool visible)
    {
        if (!vm)
            return false;

        vm.CampVisible = !visible;
        vm.MapVisible = visible;
        vm.FeedbackVisible = false;

        return true;
    }

    /// <summary>Shows explicit feedback without pretending to open an unimplemented system.</summary>
    public void ShowFeedback(string text)
    {
        vm.FeedbackText = text;
        vm.FeedbackVisible = true;
    }
}
