using System.Collections;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using UnityEngine;

/// <summary>Reports that Home resources and its initialized main page are ready.</summary>
public sealed class HomeReady : IEvent { }

/// <summary>Owns the Home page and its scene runtime.</summary>
public sealed class HomeSystem : RuntimeSystem
{
    /// <summary>Owned initialization operation canceled when Home is deactivated.</summary>
    private Coroutine initialization;

    /// <summary>Starts the scene transition and registers the implemented intents.</summary>
    /// <returns>True when the transition has been requested.</returns>
    public override bool OnActive()
    {
        Subscribe<ReqHomeMap>(OnHomeMap);
        Subscribe<ReqHomeCamp>(OnHomeCamp);
        Subscribe<ReqHomeEntry>(OnHomeEntry);
        Subscribe<ReqHomeShortcut>(OnHomeShortcut);

        return InstallHome();
    }

    /// <summary>Invalidates pending work and closes only the owned Home page.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        UninstallHome();
    }

    /// <summary>Reports actual Home page initialization.</summary>
    /// <returns>Whether the active page is ready.</returns>
    public override bool IsReady()
    {
        var runtime = GetRuntime();

        return IsActive()
            && runtime
            && runtime.IsReady()
            && UISystem.GetSingleton().TryGetWidget<UIHome>(out _);
    }

    /// <summary>Returns the scene execution owned by this Home instance.</summary>
    /// <returns>The Home runtime, not a separately registered system.</returns>
    public HomeRuntime GetRuntime()
    {
        return RuntimeManager.GetSingleton().QueryRuntime<HomeRuntime>();
    }

    /// <summary>Switches between the camp and the implemented map preview.</summary>
    /// <param name="visible">Whether to show the map.</param>
    /// <returns>True when a live Home widget handled the request.</returns>
    public bool ShowMap(bool visible)
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            return false;

        var runtime = GetRuntime();
        bool changed = widget.ShowMap(visible);
        if (changed)
            runtime.SetInputEnabled(!visible);

        return changed;
    }

    /// <summary>Refreshes the active page from a detached entity snapshot.</summary>
    /// <returns>Whether a live initialized page was refreshed.</returns>
    public bool Refresh()
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (!entity.IsInitialized())
            return false;

        widget.Invalidate(entity.Snapshot());

        return true;
    }

    /// <summary>Handles the ReqHomeMap event.</summary>
    /// <param name="message">The event payload.</param>
    private void OnHomeMap(ReqHomeMap message)
    {
        ShowMap(true);
    }

    /// <summary>Handles the ReqHomeCamp event.</summary>
    /// <param name="message">The event payload.</param>
    private void OnHomeCamp(ReqHomeCamp message)
    {
        ShowMap(false);
    }

    /// <summary>Rechecks current entry access before dispatching its destination.</summary>
    /// <param name="request">The configured entry intention.</param>
    private void OnHomeEntry(ReqHomeEntry request)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();

        HomeEntryResult result = entity.RequestEntry(request.entryId, out HomeEntryRoute route);
        if (result == HomeEntryResult.Allowed)
        {
            XPluginUtility.PostEvent(new HomeEntryRequested {
                entryId = request.entryId,
                route = route
            });
            if (route == HomeEntryRoute.MapPreview)
            {
                ShowMap(true);

                return;
            }
        }

        if (UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            widget.ShowFeedback(result);
    }

    /// <summary>Forwards fixed shortcuts using their typed destination contract.</summary>
    /// <param name="request">The fixed shortcut intention.</param>
    private void OnHomeShortcut(ReqHomeShortcut request)
    {
        XPluginUtility.PostEvent(new HomeShortcutRequested {
            route = request.route
        });

        if (UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            widget.ShowFeedback(Config.Excel.TEXT_HOME_PENDING);
    }

    /// <summary>Starts and owns the Home initialization operation.</summary>
    /// <returns>True when asynchronous initialization has been started.</returns>
    private bool InstallHome()
    {
        initialization = XCoroutine.Run(OpenHome());

        return true;
    }

    /// <summary>Cancels initialization and releases the Home page and scene runtime.</summary>
    private void UninstallHome()
    {
        if (initialization != null)
        {
            XCoroutine.StopCoroutine(initialization);
            initialization = null;
        }

        System.Type[] widgets = new System.Type[] {
            typeof(UIHome)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);

        RuntimeManager.GetSingleton().DestroyRuntime<HomeRuntime>();
    }

    /// <summary>Creates the scene runtime and opens Home after its load state completes.</summary>
    /// <returns>The initialization operation owned by this system.</returns>
    private IEnumerator OpenHome()
    {
        HomeRuntime runtime;
        try
        {
            runtime = RuntimeManager.GetSingleton().CreateRuntime<HomeRuntime>();
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
            XPluginUtility.PostEvent(new ReturnToLogin { failed = true });

            yield break;
        }

        while (!runtime.IsReady())
            yield return null;

        UISystem.GetSingleton().OpenWidget<UIHome>(OnHomeOpened);
        initialization = null;
    }

    /// <summary>Initializes the opened page before allowing scene input.</summary>
    /// <param name="widget">The Home page supplied by the UI framework.</param>
    private void OnHomeOpened(UIHome widget)
    {
        if (!widget)
            throw new System.InvalidOperationException("Home page could not be loaded.");

        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        widget.Invalidate(entity.Snapshot());
        widget.ShowMap(false);

        var runtime = GetRuntime();
        runtime.SetInputEnabled(true);

        XPluginUtility.FireEvent(new HomeReady());
        
    }
}
