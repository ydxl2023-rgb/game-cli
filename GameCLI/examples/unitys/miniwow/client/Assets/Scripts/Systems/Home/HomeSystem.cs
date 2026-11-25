using System.Collections;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Bundle.Assets;
using Oathx.Widget;
using UnityEngine;

/// <summary>Owns the Home page and waits for TownSystem before handing over from Launch.</summary>
public sealed class HomeSystem : RuntimeSystem
{
    /// <summary>Invalidates asynchronous callbacks when this system is deactivated.</summary>
    private int generation;

    /// <summary>Prevents overlapping scene transitions.</summary>
    private bool transitioning;

    /// <summary>The latest requested lifecycle state, independent of an in-flight scene operation.</summary>
    private bool requestedActive;

    /// <summary>Starts the scene transition and registers the implemented intents.</summary>
    /// <returns>True when the transition has been requested.</returns>
    public override bool OnActive()
    {
        if (!IsActive())
            return false;

        if (requestedActive)
            return true;

        UnsubscribeAll();
        Subscribe<ReqHomeMap>(request => ShowMap(true));
        Subscribe<ReqHomeCamp>(request => ShowMap(false));
        Subscribe<ReqHomeEntry>(HandleEntry);
        Subscribe<ReqHomeShortcut>(HandleShortcut);
        Subscribe<TownReadyChanged>(HandleTownReady);

        if (!XEntityManager.GetSingleton().TryGetEntity(nameof(HomeEntity), out _))
            XEntityManager.GetSingleton().CreateEntity<HomeEntity>();

        generation++;
        requestedActive = true;

        RequestTransition();

        return true;
    }

    /// <summary>Invalidates pending work and closes only the owned Home page.</summary>
    public override void OnDetive()
    {
        generation++;
        requestedActive = false;

        UnsubscribeAll();

        System.Type[] widgets = new System.Type[] {
            typeof(UIHome)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);

        XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().GetTown()?.SetInputEnabled(false);

        RequestTransition();
    }

    /// <summary>Switches between the camp and the implemented map preview.</summary>
    /// <param name="visible">Whether to show the map.</param>
    /// <returns>True when a live Home widget handled the request.</returns>
    public bool ShowMap(bool visible)
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            return false;

        bool changed = widget.ShowMap(visible);
        if (changed)
            XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().GetTown()?.SetInputEnabled(!visible);

        return changed;
    }

    /// <summary>Refreshes the active page from a detached entity snapshot.</summary>
    /// <returns>Whether a live initialized page was refreshed.</returns>
    public bool Refresh()
    {
        if (!requestedActive || !UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            return false;

        if (!XEntityManager.GetSingleton().TryGetEntity(nameof(HomeEntity), out var found) || !(found is HomeEntity entity) || !entity.IsInitialized())
            return false;

        widget.Invalidate(entity.Snapshot());

        return true;
    }

    /// <summary>Rechecks current entry access before dispatching its destination.</summary>
    /// <param name="request">The configured entry intention.</param>
    private void HandleEntry(ReqHomeEntry request)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (!entity)
            return;

        HomeEntryResult result = entity.RequestEntry(request.entryId, out string route);
        if (result == HomeEntryResult.Allowed)
        {
            XPluginUtility.PostEvent(new HomeEntryRequested {
                entryId = request.entryId,
                route = route
            });
            if (route == "MapPreview")
            {
                ShowMap(true);

                return;
            }
        }

        string key = result == HomeEntryResult.Locked ? "home.locked" :
            result == HomeEntryResult.Invalid ? "home.invalid" : "home.pending";
        if (UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            widget.ShowFeedback(entity.Snapshot().texts[key]);
    }

    /// <summary>Routes fixed shortcuts through the same explicit destination contract.</summary>
    /// <param name="request">The fixed shortcut intention.</param>
    private void HandleShortcut(ReqHomeShortcut request)
    {
        if (request.route != "Settings" && request.route != "Task" && request.route != "Chat")
            return;

        XPluginUtility.PostEvent(new HomeEntryRequested {
            entryId = 0,
            route = request.route
        });
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (!entity)
            return;

        if (UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            widget.ShowFeedback(entity.Snapshot().texts["home.pending"]);
    }

    /// <summary>Invalidates the page when town ownership changes.</summary>
    /// <param name="message">The town readiness notification.</param>
    private void HandleTownReady(TownReadyChanged message)
    {
        if (!requestedActive)
            return;

        var town = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().GetTown();
        if (town == null || !town.IsReady())
        {
            generation++;

            System.Type[] widgets = new System.Type[] {
                typeof(UIHome)
            };

            UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
        }

        RequestTransition();
    }

    /// <summary>Serializes asynchronous Home configuration and page opening.</summary>
    private void RequestTransition()
    {
        if (transitioning)
            return;

        transitioning = true;

        XCoroutine.Run(Reconcile());
    }

    /// <summary>Converges the Home page to the latest lifecycle request.</summary>
    /// <returns>The serialized transition sequence.</returns>
    private IEnumerator Reconcile()
    {
        try
        {
            while (true)
            {
                int requestGeneration = generation;
                if (!requestedActive)
                    yield break;

                var town = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().GetTown();
                float townDeadline = Time.realtimeSinceStartup + 30f;
                while (requestGeneration == generation && (town == null || !town.IsReady()) && Time.realtimeSinceStartup < townDeadline)
                {
                    yield return null;
                    town = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().GetTown();
                }

                if (requestGeneration != generation)
                    continue;

                if (town == null || !town.IsReady())
                {
                    Debug.LogError("MINIWOW_HOME_TOWN_NOT_READY");

                    yield break;
                }

                var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
                if (!entity)
                    yield break;

                if (!entity.IsInitialized())
                {
                    TextAsset configuration = null;

                    yield return XRes.LoadAsync<TextAsset>("Assets/Data/Config/Home.json", asset => configuration = asset as TextAsset);

                    if (requestGeneration != generation)
                        continue;
                    if (!configuration)
                    {
                        Debug.LogError("MINIWOW_HOME_CONFIG_LOAD_FAILED");

                        yield break;
                    }

                    entity.Initialize(configuration.text);
                }

                bool completed = false;
                bool ready = false;

                if (UISystem.GetSingleton().TryGetWidget<UIHome>(out var existing))
                {
                    ready = existing.ShowMap(false);

                    completed = true;
                }
                else
                {
                    UISystem.GetSingleton().OpenWidget<UIHome>(widget => {
                        completed = true;

                        if (widget)
                        {
                            if (requestGeneration == generation && requestedActive && town.IsReady())
                                ready = widget.ShowMap(false);
                        }
                    });
                }

                float deadline = Time.realtimeSinceStartup + 20f;
                while (!completed && requestGeneration == generation && Time.realtimeSinceStartup < deadline)
                    yield return null;

                if (requestGeneration != generation)
                    continue;

                if (!ready)
                {
                    System.Type[] widgets = new System.Type[] {
                        typeof(UIHome)
                    };

                    UISystem.GetSingleton().CloseWidget(widgets, immediate: true);

                    Debug.LogError("MINIWOW_HOME_WIDGET_LOAD_FAILED");

                    yield break;
                }

                if (!Refresh())
                {
                    Debug.LogError("MINIWOW_HOME_REFRESH_FAILED");

                    yield break;
                }

                XApp.GetSingleton().HideLaunch();
                town.SetInputEnabled(true);
                Debug.Log("MINIWOW_HOME_READY");

                yield break;
            }
        }
        finally
        {
            transitioning = false;
        }
    }
}
