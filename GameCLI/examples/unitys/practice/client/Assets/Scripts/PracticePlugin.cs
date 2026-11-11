using Oathx.Core;
using Oathx.Widget;

/// <summary>Provides the clean practice application entry.</summary>
public class PracticePlugin : XPlugin
{
    /// <summary>Initialize UI infrastructure.</summary>
    /// <returns>True when initialization completes.</returns>
    public override bool Install()
    {
        UISystem.GetSingleton().Startup(new ResourceConfigure { loader = new PracticeUILoader(), cacher = new UIDefaultCacher() });

        return true;
    }

    /// <summary>Release plugin registration resources.</summary>
    public override void Uninstall()
    {
    }

    /// <summary>Finish local startup without installing business systems.</summary>
    /// <returns>True when the practice entry is ready.</returns>
    public override bool Startup()
    {
        XApp.GetSingleton().HideLaunch();
        UnityEngine.Debug.Log("Practice entry ready: no business systems installed.");

        return true;
    }

    /// <summary>Release UI infrastructure.</summary>
    public override void Shutdown()
    {
        UISystem.GetSingleton().Shutdown();
    }
}

/// <summary>Loads practice UI assets through the framework resource layer.</summary>
public class PracticeUILoader : UILoader
{
    /// <summary>Load a typed UI resource.</summary>
    /// <param name="path">Asset path.</param>
    /// <param name="done">Completion callback.</param>
    /// <returns>The resource loading coroutine.</returns>
    public UnityEngine.Coroutine LoadAsync<T>(string path, System.Action<UnityEngine.Object> done) where T : UnityEngine.Object
    {
        return Oathx.Bundle.Assets.XRes.LoadAsync<T>(path, done);
    }

    /// <summary>Load a UI resource with a runtime type.</summary>
    /// <param name="path">Asset path.</param>
    /// <param name="type">Resource type.</param>
    /// <param name="done">Completion callback.</param>
    /// <returns>The resource loading coroutine.</returns>
    public UnityEngine.Coroutine LoadAsync(string path, System.Type type, System.Action<UnityEngine.Object> done)
    {
        return Oathx.Bundle.Assets.XRes.LoadAsync(path, type, done);
    }
}

