using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>Verifies manager ownership and failed-startup cleanup without adding a production runtime.</summary>
public static class MiniwowRuntimeProbe
{
    /// <summary>Observes live registration and any retiring Home components.</summary>
    /// <returns>The current manager and Unity component identities.</returns>
    public static string Snapshot()
    {
        var manager = RuntimeManager.GetSingleton();
        var runtime = manager.QueryRuntime<HomeRuntime>();

        return JsonConvert.SerializeObject(new {
            registered = runtime ? runtime.GetInstanceID() : 0,
            instances = UnityEngine.Object.FindObjectsOfType<HomeRuntime>().Select(item => new {
                id = item.GetInstanceID(),
                state = item.GetCurrentState().state.ToString(),
                scene = item.gameObject.scene.name
            }).ToArray()
        });
    }

    /// <summary>Checks that repeated creation preserves the existing ready state machine.</summary>
    /// <returns>Whether the registry owns exactly one persistent Home runtime.</returns>
    public static bool VerifyRegistry()
    {
        var manager = RuntimeManager.GetSingleton();
        var current = manager.QueryRuntime<HomeRuntime>();
        var repeated = manager.CreateRuntime<HomeRuntime>();
        if (!current || current != repeated || !current.IsReady())
            throw new InvalidOperationException("Repeated creation replaced or restarted Home.");

        if (UnityEngine.Object.FindObjectsOfType<HomeRuntime>().Length != 1
            || current.gameObject.scene.name != "DontDestroyOnLoad")
            throw new InvalidOperationException("Home runtime ownership is not persistent and unique.");

        return true;
    }

    /// <summary>Exercises invalid scene configuration before any scene is acquired.</summary>
    /// <returns>Whether startup throws and removes the failed live registration.</returns>
    public static bool VerifyFailedStartup()
    {
        var manager = RuntimeManager.GetSingleton();
        if (manager.QueryRuntime<HomeRuntime>())
            throw new InvalidOperationException("Run failed-startup verification from Login.");

        var configuration = Config.Excel.propSceneById(Config.Excel.SCENE_HOME);
        var property = typeof(Config.Scene).GetProperty(nameof(Config.Scene.Path));
        string original = configuration.Path;
        bool rejected = false;
        try
        {
            property.SetValue(configuration, string.Empty);
            try
            {
                manager.CreateRuntime<HomeRuntime>();
            }
            catch (InvalidOperationException)
            {
                rejected = true;
            }
        }
        finally
        {
            property.SetValue(configuration, original);
        }

        if (!rejected || manager.QueryRuntime<HomeRuntime>())
            throw new InvalidOperationException("Failed startup retained a live registration.");

        return true;
    }
}
