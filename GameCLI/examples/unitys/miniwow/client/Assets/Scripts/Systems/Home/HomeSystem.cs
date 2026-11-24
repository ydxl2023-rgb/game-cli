using System.Collections;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Bundle.Assets;
using Oathx.Widget;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Loads the camp scene and hands presentation ownership over from Launch.</summary>
public sealed class HomeSystem : XEventModule
{
    /// <summary>Scene included in the product build settings.</summary>
    private const string scenePath = "Assets/Scenes/Home.unity";

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

        if (!XEntityManager.GetSingleton().TryGetEntity(nameof(HomeEntity), out _))
            XEntityManager.GetSingleton().CreateEntity<HomeEntity>();

        generation++;
        requestedActive = true;

        RequestTransition();

        return true;
    }

    /// <summary>Invalidates pending work, closes the page and unloads the owned scene.</summary>
    public override void OnDetive()
    {
        generation++;
        requestedActive = false;

        UnsubscribeAll();

        System.Type[] widgets = new System.Type[] {
            typeof(UIHome)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);

        RequestTransition();
    }

    /// <summary>Switches between the camp and the implemented map preview.</summary>
    /// <param name="visible">Whether to show the map.</param>
    /// <returns>True when a live Home widget handled the request.</returns>
    public bool ShowMap(bool visible)
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            return false;

        return widget.ShowMap(visible);
    }

    /// <summary>Refreshes the active page from a detached entity snapshot.</summary>
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
    private void HandleEntry(ReqHomeEntry request)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
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
    private void HandleShortcut(ReqHomeShortcut request)
    {
        if (request.route != "Settings" && request.route != "Task" && request.route != "Chat")
            return;

        XPluginUtility.PostEvent(new HomeEntryRequested {
            entryId = 0,
            route = request.route
        });
        if (UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            widget.ShowFeedback(XEntityManager.GetSingleton().QueryEntity<HomeEntity>().Snapshot().texts["home.pending"]);
    }

    /// <summary>Serializes scene loading and unloading instead of overlapping asynchronous operations.</summary>
    private void RequestTransition()
    {
        if (transitioning)
            return;

        transitioning = true;

        XCoroutine.Run(Reconcile());
    }

    /// <summary>Converges scene and page state to the latest lifecycle request.</summary>
    /// <returns>The serialized transition sequence.</returns>
    private IEnumerator Reconcile()
    {
        try
        {
            while (true)
            {
                int requestGeneration = generation;
                Scene scene = SceneManager.GetSceneByPath(scenePath);

                if (!requestedActive)
                {
                    if (scene.isLoaded)
                        yield return SceneManager.UnloadSceneAsync(scene);

                    if (requestGeneration != generation)
                        continue;

                    yield break;
                }

                if (!scene.isLoaded)
                {
                    yield return SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
                    scene = SceneManager.GetSceneByPath(scenePath);
                }

                if (requestGeneration != generation)
                    continue;

                if (!scene.isLoaded)
                {
                    Debug.LogError("MINIWOW_HOME_SCENE_LOAD_FAILED");

                    yield break;
                }

                if (!XEntityManager.GetSingleton().QueryEntity<HomeEntity>().IsInitialized())
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

                    XEntityManager.GetSingleton().QueryEntity<HomeEntity>().Initialize(configuration.text);
                }

                SceneManager.SetActiveScene(scene);

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

                        if (requestGeneration == generation && requestedActive && widget)
                            ready = widget.ShowMap(false);
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
