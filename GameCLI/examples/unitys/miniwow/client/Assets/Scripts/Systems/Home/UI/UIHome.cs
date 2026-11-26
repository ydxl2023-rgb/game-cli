using System;
using System.Collections.Generic;
using System.Linq;
using Oathx.Core;
using Oathx.Widget;
using UnityEngine;
using UnityEngine.UI;

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
    /// <summary>Typed shortcut destination.</summary>
    public HomeShortcut route
    { get; set; }
}

/// <summary>Announces a fixed shortcut destination without converting it to a text key.</summary>
public sealed class HomeShortcutRequested : IEvent
{
    /// <summary>The requested shortcut destination.</summary>
    public HomeShortcut route
    { get; set; }
}

/// <summary>Announces an allowed destination intention, without claiming it opened.</summary>
public sealed class HomeEntryRequested : IEvent
{
    /// <summary>Configured entry identifier.</summary>
    public int entryId
    { get; set; }

    /// <summary>Typed product destination.</summary>
    public HomeEntryRoute route
    { get; set; }
}

/// <summary>Projects Home snapshots into the original MainPage-inspired portrait UI.</summary>
[Config("Assets/Data/Widgets/Home/UIHome.prefab", WidgetLayer.center, cacheWidget: false)]
public sealed class UIHome : UIWidget<VMHome>
{
    /// <summary>Normal HUD hidden during the opening rescue.</summary>
    [SerializeField]
    private GameObject[] openingHidden;

    /// <summary>Existing task control revealed after the priest introduction.</summary>
    [SerializeField]
    private GameObject taskControl;

    /// <summary>Ordered configuration IDs authored with this prefab; prevents silent art/intent mismatches.</summary>
    [SerializeField]
    private string entryOrder;

    /// <summary>Resets transient page presentation on activation.</summary>
    /// <returns>Whether the page became active.</returns>
    public override bool Active()
    {
        if (!base.Active())
            return false;

        ShowMap(false);
        vm.FeedbackVisible = false;

        return true;
    }

    /// <summary>Displays the authoritative selected task and opening HUD visibility.</summary>
    /// <param name="opening">The opening progress.</param>
    /// <param name="task">The currently tracked task.</param>
    public void InvalidateOpening(HomeOpening opening, TaskEntity task)
    {
        foreach (GameObject item in openingHidden)
            item.SetActive(opening.phase == HomeOpeningPhase.AwaitRecruit);

        taskControl.SetActive(opening.phase >= HomeOpeningPhase.FetchWater);
        vm.TaskProgress = string.Format(CommonText.Get(Config.Excel.TEXT_OPENING_TASK), task.configuration.ID,
            Config.Excel.propLanguageById(int.Parse(task.configuration.Description)),
            task.progress, task.configuration.FinishValue);
    }

    /// <summary>Updates existing bindings without rebinding callbacks or list items.</summary>
    /// <param name="snapshot">Detached Home state used for presentation.</param>
    public void Invalidate(HomeSnapshot snapshot)
    {
        if (snapshot == null
            || snapshot.entries == null
            || entryOrder != string.Join(",", snapshot.entries.Select(entry => entry.id)))
            throw new InvalidOperationException(
                "Home configuration IDs differ from the authored prefab; regenerate and bind UIHome.");

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

    /// <summary>Switches between the camp and existing map preview.</summary>
    /// <param name="visible">Whether to show the map instead of the camp.</param>
    /// <returns>Whether the bound page accepted the change.</returns>
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
    /// <param name="textId">Stable CommonText configuration identifier.</param>
    public void ShowFeedback(int textId)
    {
        vm.FeedbackText = CommonText.Get(textId);
        vm.FeedbackVisible = true;
    }

    /// <summary>Maps an entry result to a localized presentation message.</summary>
    /// <param name="result">The entry access result supplied by Home.</param>
    public void ShowFeedback(HomeEntryResult result)
    {
        switch (result)
        {
            case HomeEntryResult.Locked:
                ShowFeedback(Config.Excel.TEXT_HOME_LOCKED);
                break;
            case HomeEntryResult.Invalid:
                ShowFeedback(Config.Excel.TEXT_HOME_INVALID);
                break;
            default:
                ShowFeedback(Config.Excel.TEXT_HOME_PENDING);
                break;
        }
    }

    /// <summary>Binds fixed intentions exactly once.</summary>
    /// <returns>Whether fixed button intentions were bound.</returns>
    protected override bool Bind()
    {
        vm.MapClick = Send<ReqHomeMap>;
        vm.ReturnClick = Send<ReqHomeCamp>;
        vm.CloseFeedbackClick = button => {
            vm.FeedbackVisible = false;

            return true;
        };
        vm.SettingsClick = OnSettingsClick;
        vm.TaskClick = OnTaskClick;
        vm.ChatClick = OnChatClick;

        return true;
    }

    /// <summary>Submits the settings shortcut.</summary>
    /// <param name="button">The clicked settings button.</param>
    /// <returns>Whether the request was sent.</returns>
    private bool OnSettingsClick(Button button)
    {
        return Send(new ReqHomeShortcut { route = HomeShortcut.Settings });
    }

    /// <summary>Submits the task shortcut.</summary>
    /// <param name="button">The clicked task button.</param>
    /// <returns>Whether the request was sent.</returns>
    private bool OnTaskClick(Button button)
    {
        return Send(new ReqHomeShortcut { route = HomeShortcut.Task });
    }

    /// <summary>Submits the chat shortcut.</summary>
    /// <param name="button">The clicked chat button.</param>
    /// <returns>Whether the request was sent.</returns>
    private bool OnChatClick(Button button)
    {
        return Send(new ReqHomeShortcut { route = HomeShortcut.Chat });
    }

    /// <summary>Formats the original compact ten-thousand unit convention.</summary>
    /// <param name="amount">The resource amount to display.</param>
    /// <returns>The compact display amount.</returns>
    private static string FormatAmount(long amount)
    {
        return amount >= 10000
            ? (amount / 10000d).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "万"
            : amount.ToString();
    }

}
