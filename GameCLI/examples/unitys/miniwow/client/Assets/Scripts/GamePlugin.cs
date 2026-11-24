using Oathx.Core;
using Oathx.Widget;
using UnityEngine;

/// <summary>Initializes the product UI and its local Home system.</summary>
public sealed class GamePlugin : XPlugin
{
    /// <summary>Initializes the UI resource loader without installing business systems.</summary>
    /// <returns>True when infrastructure is ready.</returns>
    public override bool Install()
    {
        MetadataSyncFactory.GetSingleton().Register(nameof(SyncAnchoredPosition), new SyncAnchoredPosition());

        var configure = new ResourceConfigure(new GameUISender(this)) {
            loader = new GameUILoader(),
            cacher = new UIDefaultCacher()
        };

        UISystem.GetSingleton().Startup(configure);

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

/// <summary>Adapts widget intentions to synchronous plugin events or the application event queue.</summary>
public sealed class GameUISender : UISender
{
    /// <summary>The dispatcher that owns this product's active systems.</summary>
    private readonly XEventDispatcher dispatcher;

    /// <summary>Requires an explicit dispatcher instead of looking up a global plugin.</summary>
    /// <param name="dispatcher">The product event dispatcher.</param>
    public GameUISender(XEventDispatcher dispatcher)
    {
        if (dispatcher == null)
            throw new System.ArgumentNullException(nameof(dispatcher));

        this.dispatcher = dispatcher;
    }

    /// <summary>Immediately dispatches valid application events to the configured product.</summary>
    /// <param name="request">The widget intention.</param>
    /// <returns>Whether the request implements the application event contract.</returns>
    public bool Send(IUIRequest request)
    {
        if (!(request is IEvent message))
            return false;

        dispatcher.DispatchEvent(message);

        return true;
    }

    /// <summary>Queues valid application events through the existing event loop.</summary>
    /// <param name="request">The widget intention.</param>
    /// <returns>Whether the application accepted the event.</returns>
    public bool Post(IUIRequest request)
    {
        return request is IEvent message && XPluginUtility.PostEvent(message);
    }
}
