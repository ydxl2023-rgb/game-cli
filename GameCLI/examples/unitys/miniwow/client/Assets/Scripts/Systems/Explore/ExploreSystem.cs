using System.Collections;
using Oathx.Core;
using Oathx.Entity;
using UnityEngine;

/// <summary>Reports actual arrival of the excursion's world actor.</summary>
public sealed class ExploreWorldArrived : IEvent
{
    /// <summary>Excursion whose actor reached its assigned route endpoint.</summary>
    public int journeyId
    { get; set; }
}

/// <summary>Schedules introductory exploration independently from Home presentation.</summary>
public sealed class ExploreSystem : RuntimeSystem
{
    /// <summary>Owned clock for the currently attached Home session.</summary>
    private Coroutine clock;

    /// <summary>Installs event handlers and requests the current Home session.</summary>
    /// <returns>Whether installation succeeded.</returns>
    public override bool OnActive()
    {
        Subscribe<HomeReady>(OnHomeReady);
        Subscribe<HomeSessionAvailable>(OnSessionAvailable);
        Subscribe<HomeSessionClosing>(OnSessionClosing);
        Subscribe<ExploreWorldArrived>(OnWorldArrived);
        Subscribe<TaskClaimed>(OnTaskClaimed);
        Subscribe<HomeVillageChanged>(OnVillageChanged);

        return InstallExplore();
    }

    /// <summary>Releases subscriptions and the owned clock.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        UninstallExplore();
    }

    /// <summary>Attaches to an existing session without loading another gameplay system.</summary>
    /// <returns>Whether the attachment request was sent.</returns>
    private bool InstallExplore()
    {
        XPluginUtility.FireEvent(new HomeSessionRequested());

        return true;
    }

    /// <summary>Stops scheduling without discarding committed journey intent.</summary>
    private void UninstallExplore()
    {
        if (clock != null)
            XCoroutine.StopCoroutine(clock);

        clock = null;
    }

    /// <summary>Starts scheduling after the world has loaded.</summary>
    /// <param name="message">World readiness event.</param>
    private void OnHomeReady(HomeReady message)
    {
        StartExplore();
    }

    /// <summary>Attaches after this system is reinstalled into an existing session.</summary>
    /// <param name="message">Current session response.</param>
    private void OnSessionAvailable(HomeSessionAvailable message)
    {
        StartExplore();
    }

    /// <summary>Stops using the Home aggregate before it is released.</summary>
    /// <param name="message">Closing session event.</param>
    private void OnSessionClosing(HomeSessionClosing message)
    {
        UninstallExplore();
    }

    /// <summary>Replaces the owned session clock and replays committed observations.</summary>
    private void StartExplore()
    {
        UninstallExplore();
        ReportProgress();
        BeginExplore();
        clock = XCoroutine.Run(UpdateExplore());
    }

    /// <summary>Checks newly available excursions only after a reward receipt changes.</summary>
    /// <param name="message">Task reward outcome.</param>
    private void OnTaskClaimed(TaskClaimed message)
    {
        if (message.result == TaskClaimResult.Claimed)
            BeginExplore();
    }

    /// <summary>Rechecks availability after treatment, meals, or another excursion completes.</summary>
    /// <param name="message">Committed village change.</param>
    private void OnVillageChanged(HomeVillageChanged message)
    {
        BeginExplore();
    }

    /// <summary>Issues the first eligible configured excursion after a business change.</summary>
    private void BeginExplore()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (home.village.exploration != ExplorePhase.None && home.village.exploration != ExplorePhase.Completed)
            return;

        var explore = XEntityManager.GetSingleton().QueryEntity<ExploreEntity>();
        foreach (var definition in Config.Excel.exploreJourneyConfig.Values)
        {
            if (!home.village.BeginExplore(explore, definition))
                continue;

            XPluginUtility.FireEvent(new HomeVillageChanged());

            return;
        }
    }

    /// <summary>Accepts a world arrival through the entity transaction boundary.</summary>
    /// <param name="message">Actual route completion.</param>
    private void OnWorldArrived(ExploreWorldArrived message)
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var explore = XEntityManager.GetSingleton().QueryEntity<ExploreEntity>();
        var definition = Config.Excel.exploreJourneyConfig[message.journeyId];
        if (home.village.ArriveExplore(explore, definition))
            XPluginUtility.FireEvent(new HomeVillageChanged());
    }

    /// <summary>Advances configuration-driven excursions without depending on the visible page.</summary>
    /// <returns>The owned session clock.</returns>
    private IEnumerator UpdateExplore()
    {
        var explore = XEntityManager.GetSingleton().QueryEntity<ExploreEntity>();
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var interval = new WaitForSeconds(0.1f);
        while (true)
        {
            bool changed = false;
            if (home.village.exploration == ExplorePhase.Traveling
                || home.village.exploration == ExplorePhase.Executing)
            {
                var definition = Config.Excel.exploreJourneyConfig[home.village.journeyId];
                changed |= home.village.AdvanceExplore(explore, definition);
            }

            if (changed)
            {
                ReportProgress();
                XPluginUtility.FireEvent(new HomeVillageChanged());
            }

            yield return interval;
        }
    }

    /// <summary>Reports absolute event counts without targeting a particular task ID.</summary>
    private void ReportProgress()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var saved = home.village.Snapshot();
        foreach (var count in saved.explore.counts)
        {
            var observation = new TaskProgressReported {
                condition = Config.ETaskConditionType.AdventureEventTimes,
                param1 = count.Key,
                value = count.Value
            };

            XPluginUtility.FireEvent(observation);
        }
    }
}
