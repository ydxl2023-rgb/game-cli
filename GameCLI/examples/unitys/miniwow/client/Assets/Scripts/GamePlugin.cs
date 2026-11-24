using Oathx.Core;
using Oathx.Widget;
using UnityEngine;

/// <summary>Initializes only the local UI infrastructure for the clean project.</summary>
public sealed class GamePlugin : XPlugin
{
    /// <summary>Initializes the UI resource loader without installing business systems.</summary>
    /// <returns>True when infrastructure is ready.</returns>
    public override bool Install()
    {
        UISystem.GetSingleton().Startup(new ResourceConfigure { loader = new GameUILoader(), cacher = new UIDefaultCacher() });

        return true;
    }

    /// <summary>Finishes the local entry and leaves an empty application.</summary>
    /// <returns>True when startup completes.</returns>
    public override bool Startup()
    {
        XApp.GetSingleton().HideLaunch();
        Debug.Log("MINIWOW_CLEAN_ENTRY_READY");

        return true;
    }

    /// <summary>Releases the UI infrastructure.</summary>
    public override void Shutdown()
    {
        UISystem.GetSingleton().Shutdown();
    }

    /// <summary>Releases plugin registrations; this baseline registers none.</summary>
    public override void Uninstall()
    {
    }
}

/// <summary>Loads future widgets through the existing framework resource layer.</summary>
public sealed class GameUILoader : UILoader
{
    /// <summary>Loads a resource by its compile-time type.</summary>
    /// <param name="path">Resource path.</param>
    /// <param name="done">Completion callback.</param>
    /// <returns>The loading coroutine.</returns>
    public Coroutine LoadAsync<T>(string path, System.Action<Object> done) where T : Object
    {
        return Oathx.Bundle.Assets.XRes.LoadAsync<T>(path, done);
    }

    /// <summary>Loads a resource by its runtime type.</summary>
    /// <param name="path">Resource path.</param>
    /// <param name="type">Resource type.</param>
    /// <param name="done">Completion callback.</param>
    /// <returns>The loading coroutine.</returns>
    public Coroutine LoadAsync(string path, System.Type type, System.Action<Object> done)
    {
        return Oathx.Bundle.Assets.XRes.LoadAsync(path, type, done);
    }
}
