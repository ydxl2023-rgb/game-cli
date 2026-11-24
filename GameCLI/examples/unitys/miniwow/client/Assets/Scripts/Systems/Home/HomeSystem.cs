using System.Collections;
using Oathx.Core;
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
