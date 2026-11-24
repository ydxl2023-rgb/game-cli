using Oathx.Core;
using Oathx.Widget;
using UnityEngine;

/// <summary>Initializes the product UI and its local Home system.</summary>
public sealed class GamePlugin : XPlugin, UISender
{
    /// <summary>Initializes the UI resource loader without installing business systems.</summary>
    /// <returns>True when infrastructure is ready.</returns>
    public override bool Install()
    {
        UISystem.GetSingleton().Startup(new ResourceConfigure(this) { loader = new GameUILoader(), cacher = new UIDefaultCacher() });

        return true;
    }

    /// <summary>Starts Home; the system hides Launch only after the scene and widget are ready.</summary>
    /// <returns>True when startup completes.</returns>
    public override bool Startup()
    {
        XApp.GetSingleton().ShowLaunchBackground();
        Load<HomeSystem>();

        return true;
    }

    /// <summary>Releases the UI infrastructure.</summary>
    public override void Shutdown()
    {
        UnloadAll();
        UISystem.GetSingleton().Shutdown();
    }

    /// <summary>Releases plugin registrations; this baseline registers none.</summary>
    public override void Uninstall()
    {
    }

    /// <summary>Dispatches typed UI intent events through the existing plugin dispatcher.</summary>
    /// <param name="request">The request originating from a bound widget.</param>
    /// <returns>True when the request implements the application event contract.</returns>
    public bool Send(IUIRequest request)
    {
        if (!(request is IEvent message))
            return false;

        DispatchEvent(message);

        return true;
    }

    /// <summary>Queues UI intent through the existing application event loop.</summary>
    /// <param name="request">The request to enqueue.</param>
    /// <returns>True when the application accepts the event.</returns>
    public bool Post(IUIRequest request)
    {
        return request is IEvent message && XPluginUtility.PostEvent(message);
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
