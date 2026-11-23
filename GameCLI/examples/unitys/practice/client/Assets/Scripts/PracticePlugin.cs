using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;

/// <summary>Initializes the local ten-system practice integration.</summary>
public class PracticePlugin : XPlugin, IUIEventSender
{
    /// <summary>Initialize UI infrastructure.</summary>
    /// <returns>True when initialization completes.</returns>
    public override bool Install()
    {
        UISystem.GetSingleton().Startup(new ResourceConfigure { loader = new PracticeUILoader(), cacher = new UIDefaultCacher(), eventSenders = new System.Collections.Generic.List<IUIEventSender> { this } });

        UISystem.GetSingleton().RegisterSyncActions(new System.Type[] {
            typeof(SyncFancyGridScrollView),
            typeof(SyncFancyGridClick)
        });



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
        LoadConfiguration();
        var types = new System.Collections.Generic.List<System.Type>();
        foreach (var type in GetType().Assembly.GetTypes())
        {
            if (typeof(XEntity).IsAssignableFrom(type) && !type.IsAbstract && type.GetCustomAttributes(typeof(Initialize), true).Length > 0)
                types.Add(type);
        }
        XEntityManager.GetSingleton().Startup(types);
        foreach (var type in new[] { typeof(MasterSystem), typeof(FunctionManagerSystem), typeof(RedDotSystem), typeof(LoginSystem) })
        {
            var module = Load(type);
            if (module != null && !module.IsActive())
                module.Active();
        }
        UnityEngine.Debug.Log("Practice ten-system offline integration ready.");

        return true;
    }

    /// <summary>Release UI infrastructure.</summary>
    public override void Shutdown()
    {
        UnloadAll();

        UISystem.GetSingleton().Shutdown();
        XEntityManager.GetSingleton().Shutdown();
    }

    /// <summary>Forward widget activation into the plugin event dispatcher.</summary>
    /// <param name="widget">Activated widget.</param>
    public void OnWidgetOpened(IWidget widget)
    {
        DispatchEvent(new WidgetOpenedEvent { data = new WidgetOpened { widget = widget } });
    }

    /// <summary>Forward widget closing into the plugin event dispatcher.</summary>
    /// <param name="widget">Closing widget.</param>
    public void OnWidgetClosing(IWidget widget)
    {
        DispatchEvent(new WidgetClosingEvent { data = new WidgetClosing { widget = widget } });
    }

    /// <summary>Load exported configuration before creating business entities.</summary>
    private void LoadConfiguration()
    {
        foreach (var asset in UnityEngine.Resources.LoadAll<UnityEngine.TextAsset>("PracticeConfig"))
        {
            var type = GetType().Assembly.GetType("Config." + asset.name);
            if (type == null)
                continue;

            var data = (Config.IConfigData)Newtonsoft.Json.JsonConvert.DeserializeObject(asset.text, type);
            var field = typeof(Config.Excel).GetField(data.GetName());
            if (field != null)
                field.SetValue(null, data.GetData());
        }
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


/// <summary>Application wrapper for a widget activation.</summary>
public class WidgetOpenedEvent : IEvent
{
    /// <summary>Widget event data.</summary>
    public WidgetOpened data
    { get; set; }
}

/// <summary>Application wrapper for a widget closing.</summary>
public class WidgetClosingEvent : IEvent
{
    /// <summary>Widget event data.</summary>
    public WidgetClosing data
    { get; set; }
}
