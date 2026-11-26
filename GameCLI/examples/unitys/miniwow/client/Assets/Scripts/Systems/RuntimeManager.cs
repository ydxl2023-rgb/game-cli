using System;
using System.Collections;
using System.Collections.Generic;
using Oathx.Core;
using UnityEngine;

/// <summary>Owns one live state-machine runtime per concrete type, independently of system registration.</summary>
public sealed class RuntimeManager : XSimpleSingleton<RuntimeManager>
{
    /// <summary>Live instances available to their owning systems.</summary>
    private readonly Dictionary<Type, GameRuntime> runtimes = new Dictionary<Type, GameRuntime>();

    /// <summary>Creates a persistent runtime component or returns its existing live instance.</summary>
    /// <typeparam name="T">The state-machine runtime type.</typeparam>
    /// <returns>The managed runtime whose startup was accepted.</returns>
    public T CreateRuntime<T>() where T : GameRuntime
    {
        if (runtimes.TryGetValue(typeof(T), out var existing))
            return (T)existing;

        var host = new GameObject(typeof(T).Name);
        UnityEngine.Object.DontDestroyOnLoad(host);

        T runtime = host.AddComponent<T>();
        runtimes.Add(typeof(T), runtime);
        try
        {
            if (!runtime.Startup())
                throw new InvalidOperationException($"Runtime {typeof(T).Name} refused startup.");

            return runtime;
        }
        catch
        {
            DestroyRuntime<T>();

            throw;
        }
    }

    /// <summary>Queries an existing instance without creating or activating it.</summary>
    /// <typeparam name="T">The state-machine runtime type.</typeparam>
    /// <returns>The registered instance, or null when it is absent or unloading.</returns>
    public T QueryRuntime<T>() where T : GameRuntime
    {
        return runtimes.TryGetValue(typeof(T), out var runtime) ? (T)runtime : null;
    }

    /// <summary>Removes the live registration and releases its asynchronous resources before destruction.</summary>
    /// <typeparam name="T">The state-machine runtime type.</typeparam>
    public void DestroyRuntime<T>() where T : GameRuntime
    {
        if (!runtimes.TryGetValue(typeof(T), out var runtime))
            return;

        runtimes.Remove(typeof(T));
        ReleaseRuntime(runtime);
    }

    /// <summary>Releases all remaining runtimes before the plugin releases entity services.</summary>
    public void Shutdown()
    {
        foreach (GameRuntime runtime in runtimes.Values)
            ReleaseRuntime(runtime);

        runtimes.Clear();
    }

    /// <summary>Clears registrations left by a previous play session when domain reload is disabled.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRegistry()
    {
        GetSingleton().runtimes.Clear();
    }

    /// <summary>Stops interactions immediately and waits for scene cleanup.</summary>
    /// <param name="runtime">The unregistered component to release.</param>
    private static void ReleaseRuntime(GameRuntime runtime)
    {
        runtime.Shutdown();

        XCoroutine.Run(DestroyWhenStopped(runtime));
    }

    /// <summary>Keeps the state machine alive until its scene operations finish.</summary>
    /// <param name="runtime">The retiring Unity component.</param>
    /// <returns>The deferred Unity destruction sequence.</returns>
    private static IEnumerator DestroyWhenStopped(GameRuntime runtime)
    {
        while (runtime && !runtime.IsStopped())
            yield return null;

        if (runtime)
            UnityEngine.Object.Destroy(runtime.gameObject);
    }
}
