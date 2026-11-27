using System.Linq;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;

/// <summary>Requests the original exploration-to-stage entry page.</summary>
public sealed class BattleEntryOpenRequested : IEvent { }

/// <summary>Owns the unlocked first-stage entry without fabricating combat results.</summary>
public sealed class BattleEntrySystem : RuntimeSystem
{
    /// <summary>Opening intention retained only until its page resource completes.</summary>
    private BattleEntryOpenRequested openingRequest;

    /// <summary>Installs entry intentions.</summary>
    /// <returns>Whether installation succeeded.</returns>
    public override bool OnActive()
    {
        Subscribe<BattleEntryOpenRequested>(OnOpen);
        Subscribe<ReqBattleEntryClose>(OnClose);
        Subscribe<ReqBattleEntryStage>(OnStage);
        Subscribe<ReqBattleEntryGather>(OnGather);
        Subscribe<HomeSessionClosing>(OnSessionClosing);

        return true;
    }

    /// <summary>Releases owned presentation and session selection.</summary>
    public override void OnDetive()
    {
        CloseEntry();
        UnsubscribeAll();
    }

    /// <summary>Opens the map using original feature availability.</summary>
    /// <param name="message">Explicit exploration intention.</param>
    private void OnOpen(BattleEntryOpenRequested message)
    {
        if (openingRequest != null || UISystem.GetSingleton().TryGetWidget<UIBattleEntry>(out _))
            return;

        var entry = XEntityManager.GetSingleton().QueryEntity<BattleEntryEntity>();
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var definition = Config.Excel.battleEntryConfig.Values.First();
        int result = entry.Check(definition, tasks, home.village.heroes);
        if (result != 0)
        {
            if (UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
                widget.ShowFeedback(result);

            return;
        }

        openingRequest = message;
        XPluginUtility.FireEvent(new HomeFacilityPresentationChanged { systemId = 10011, visible = true });
        UISystem.GetSingleton().OpenWidget<UIBattleEntry>(OnOpened);

        /// <summary>Accepts the page only while its opening intention remains owned.</summary>
        /// <param name="widget">Loaded first-stage entry.</param>
        void OnOpened(UIBattleEntry widget)
        {
            if (!ReferenceEquals(openingRequest, message))
                return;

            openingRequest = null;
            RefreshEntry(widget);
        }
    }

    /// <summary>Initializes original map and stage labels.</summary>
    /// <param name="widget">Loaded entry page.</param>
    private void RefreshEntry(UIBattleEntry widget)
    {
        if (!widget)
        {
            CloseEntry();

            return;
        }

        widget.Invalidate(Config.Excel.battleEntryConfig.Values.First());
        widget.ShowStage(false);
    }

    /// <summary>Opens the original boss list after its map button is clicked.</summary>
    /// <param name="message">Map stage button intention.</param>
    private void OnStage(ReqBattleEntryStage message)
    {
        if (!UISystem.GetSingleton().TryGetWidget<UIBattleEntry>(out var widget))
            return;

        widget.ShowStage(true);
    }

    /// <summary>Records the available first-stage selection without rewarding a win.</summary>
    /// <param name="message">Explicit gather intention.</param>
    private void OnGather(ReqBattleEntryGather message)
    {
        var entry = XEntityManager.GetSingleton().QueryEntity<BattleEntryEntity>();
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var definition = Config.Excel.battleEntryConfig.Values.First();
        int result = entry.Select(definition, tasks, home.village.heroes);
        if (!UISystem.GetSingleton().TryGetWidget<UIBattleEntry>(out var widget))
            return;

        widget.ShowFeedback(result == 0 ? Config.Excel.TEXT_BATTLE_ENTRY_PREPARED : result);
    }

    /// <summary>Closes entry presentation.</summary>
    /// <param name="message">Explicit close intention.</param>
    private void OnClose(ReqBattleEntryClose message)
    {
        CloseEntry();
    }

    /// <summary>Releases session-owned entry before Home disappears.</summary>
    /// <param name="message">Session closing notification.</param>
    private void OnSessionClosing(HomeSessionClosing message)
    {
        CloseEntry();
    }

    /// <summary>Releases the page and its scene input claim.</summary>
    private void CloseEntry()
    {
        openingRequest = null;

        System.Type[] widgets = new System.Type[] {
            typeof(UIBattleEntry)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
        var entry = XEntityManager.GetSingleton().QueryEntity<BattleEntryEntity>();
        entry.Clear();
        XPluginUtility.FireEvent(new HomeFacilityPresentationChanged { systemId = 10011, visible = false });
    }
}
