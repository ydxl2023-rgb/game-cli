using System.Collections;
using Newtonsoft.Json;
using Oathx.Bundle.Assets;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using UnityEngine;

/// <summary>Initializes the product UI and its local Home system.</summary>
public sealed class GamePlugin : XPlugin
{
    /// <summary>Invalidates pending configuration loading when the plugin shuts down.</summary>
    private int startupGeneration;

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

        startupGeneration++;
        XCoroutine.Run(StartConfiguredSystems(startupGeneration));

        return true;
    }

    /// <summary>Releases the UI infrastructure.</summary>
    public override void Shutdown()
    {
        startupGeneration++;

        UnloadAll();

        UISystem.GetSingleton().Shutdown();
    }

    /// <summary>Releases plugin registrations; this baseline registers none.</summary>
    public override void Uninstall()
    {
    }

    /// <summary>Advances registered logical entities independently of scene rendering.</summary>
    /// <param name="elapsed">Elapsed application frame time.</param>
    public override void Update(float elapsed)
    {
        base.Update(elapsed);

        XEntityManager.GetSingleton().Update(elapsed);
    }

    /// <summary>Returns the town module when installed, including during lifecycle transitions.</summary>
    /// <returns>The installed town module, or null while it is absent.</returns>
    public TownSystem GetTown()
    {
        return modules.TryGetValue(nameof(TownSystem), out var module) ? module as TownSystem : null;
    }

    /// <summary>Loads generated scene configuration before starting dependent systems.</summary>
    /// <param name="generation">The owning startup request.</param>
    /// <returns>The asynchronous configuration initialization.</returns>
    private IEnumerator StartConfiguredSystems(int generation)
    {
        TextAsset asset = null;
        yield return XRes.LoadAsync<TextAsset>("Assets/Data/Jsons/Excels/SceneConfig.bytes", loaded => asset = loaded as TextAsset);
        if (generation != startupGeneration)
            yield break;

        Config.SceneConfig table = null;
        try
        {
            if (asset)
                table = JsonConvert.DeserializeObject<Config.SceneConfig>(asset.text);
        }
        catch (JsonException exception)
        {
            Debug.LogException(exception);
        }

        if (table == null || table.data == null || !table.data.TryGetValue(Config.Excel.SCENE_APP, out var app) || !table.data.TryGetValue(Config.Excel.SCENE_HOME, out var home) || !app || !home || string.IsNullOrWhiteSpace(app.Path) || string.IsNullOrWhiteSpace(home.Path) || app.Path == home.Path)
        {
            Debug.LogError("MINIWOW_SCENE_CONFIG_INVALID");

            yield break;
        }

        // Match the framework's generated read-only table registration contract.
        typeof(Config.Excel).GetField(nameof(Config.Excel.sceneConfig)).SetValue(null, table.data);

        Load<TownSystem>();
        Load<HomeSystem>();
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
