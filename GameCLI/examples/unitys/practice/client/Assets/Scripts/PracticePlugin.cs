using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;

/// <summary>Integrates the five-module local practice trial.</summary>
public class PracticePlugin : XPlugin
{
    /// <summary>Initialize UI infrastructure.</summary>
    /// <returns>True when initialization completes.</returns>
    public override bool Install()
    {
        UISystem.GetSingleton().Startup(new ResourceConfigure { loader = new PracticeUILoader(), cacher = new UIDefaultCacher() });

        XEntityManager.GetSingleton().CreateEntity<MasterEntity>();
        XEntityManager.GetSingleton().CreateEntity<LoginEntity>();
        XEntityManager.GetSingleton().CreateEntity<NoticeEntity>();

        return true;
    }

    /// <summary>Release plugin registration resources.</summary>
    public override void Uninstall()
    {
    }

    /// <summary>Start the coordinator and request the local login module.</summary>
    /// <returns>True when the practice entry is ready.</returns>
    public override bool Startup()
    {
        Load<MasterSystem>();

        XPluginUtility.PostEvent(new SystemActive { type = typeof(LoginSystem) });
        UnityEngine.Debug.Log("Practice trial ready: local template flow, no network authentication.");

        return true;
    }

    /// <summary>Release UI infrastructure.</summary>
    public override void Shutdown()
    {
        UnloadAll();

        UISystem.GetSingleton().Shutdown();
        XEntityManager.GetSingleton().Shutdown();
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

