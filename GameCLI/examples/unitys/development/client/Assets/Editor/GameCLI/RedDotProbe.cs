#if UNITY_EDITOR
using System.Linq;
using Config;
using Newtonsoft.Json;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using UnityEngine;

/// <summary>Editor-only lifecycle and event evidence for red dot extraction.</summary>
public static class RedDotProbe
{
    /// <summary>Exercise startup with and without the optional application event adapter.</summary>
    /// <param name="enabled">Whether to inject the current game plugin.</param>
    public static void ConfigureSender(bool enabled)
    {
        var configure = new ResourceConfigure { loader = new UIResourceLoader(), cacher = new UIDefaultCacher() };
        if (enabled)
            configure.eventSenders.Add(XPluginManager.GetSingleton().QueryPlugin<GamePlugin>());

        UISystem.GetSingleton().Startup(configure);
    }

    /// <summary>Observe notifications and submit three updates in one call and frame.</summary>
    /// <returns>The frame in which all inputs were submitted.</returns>
    public static int Burst()
    {
        var system = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().Query<RedDotSystem>();
        system.GetDispatcher().Load<RedDotObserver>();

        Signal(5);
        Signal(6);
        Signal(7);

        return Time.frameCount;
    }

    /// <summary>Read notification count and emission frame for coalescing evidence.</summary>
    /// <returns>The observed notification count, value and frame.</returns>
    public static string Notifications()
    {
        return JsonConvert.SerializeObject(new { count = RedDotObserver.notifications, value = RedDotObserver.value, frame = RedDotObserver.frame });
    }

    /// <summary>Release the temporary observer.</summary>
    public static void EndObserve()
    {
        var system = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().Query<RedDotSystem>();
        system.GetDispatcher().Unload<RedDotObserver>();
    }

    /// <summary>Read whether the service owns a framework widget-open subscription.</summary>
    /// <returns>One for an active subscription, otherwise zero.</returns>
    public static int ListenerCount()
    {
        try
        {
            var system = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().Query<RedDotSystem>();

            return system.HasEvent(new WidgetOpenedEvent()) ? 1 : 0;
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            return 0;
        }
    }

    /// <summary>Exercise the framework's repeated activation behavior.</summary>
    /// <returns>The number of callbacks after activating the same instance twice.</returns>
    public static int RepeatActivation()
    {
        var system = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().Query<RedDotSystem>();
        system.Active();
        system.Active();

        return ListenerCount();
    }

    /// <summary>Override only in-memory lifecycle flags for a configuration test.</summary>
    /// <param name="enabled">Whether widget lifecycle checks should run.</param>
    public static void SetRefreshFlags(bool enabled)
    {
        var row = Excel.propRedDotById(Excel.RED_DOT_HOME_MAILTIPS);
        int value = enabled ? 1 : 0;
        JsonConvert.PopulateObject(JsonConvert.SerializeObject(new { RefreshOnOpen = value, RefreshOnClose = value }), row);
    }

    /// <summary>Verify invalid configuration fails before any listeners are attached.</summary>
    /// <param name="fault">Cycle, missing parent, unknown condition or duplicate identifier.</param>
    /// <returns>The malformed input and concrete rejection reason.</returns>
    public static string RejectInvalid(string fault)
    {
        int id = Excel.RED_DOT_HOME_MAILTIPS;
        var original = Excel.redDotConfig[id];
        string backup = JsonConvert.SerializeObject(original);
        object change = fault == "cycle" ? new { ParentID = id } : (object)new { ParentID = int.MaxValue };
        if (fault == "unknown")
            change = new { RedDotCondition = 999 };
        if (fault == "duplicate")
            change = new { ID = Excel.RED_DOT_HOME, NumID = Excel.RED_DOT_HOME };

        JsonConvert.PopulateObject(JsonConvert.SerializeObject(change), original);
        var system = new RedDotSystem();
        try
        {
            system.Active();

            return "Unexpected validation success";
        }
        catch (System.InvalidOperationException exception)
        {
            return JsonConvert.SerializeObject(new { fault, change, error = exception.GetType().Name, exception.Message });
        }
        finally
        {
            system.Detive();
            JsonConvert.PopulateObject(backup, original);
        }
    }

    /// <summary>Publish a mail count through the business event contract.</summary>
    /// <param name="count">The local count.</param>
    /// <returns>Whether event dispatch accepted the message.</returns>
    public static bool Signal(int count)
    {
        return XPluginUtility.FireEvent(new RedDotSignalChanged { id = Excel.RED_DOT_HOME_MAILTIPS, count = count });
    }

    /// <summary>Read entity counts and the actual visual independently.</summary>
    /// <returns>Serialized runtime evidence.</returns>
    public static string Read()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<RedDotEntity>();
        if (!entity)
            return "Missing RedDotEntity";

        UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget);
        var targets = widget ? widget.GetComponentsInChildren<Transform>(true).Where(item => item.name == "MailTips").ToArray() : new Transform[0];

        return JsonConvert.SerializeObject(new {
            signals = entity.signals,
            counts = entity.counts,
            scopes = RedDotScope.Snapshot().Length,
            mailTargets = targets.Select(item => new { item.name, active = item.gameObject.activeSelf })
        });
    }

    /// <summary>Change input without notifying, to test lifecycle-triggered checks.</summary>
    /// <param name="count">Input for the next lifecycle refresh.</param>
    /// <returns>True when the input was changed.</returns>
    public static bool ChangeInput(int count)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<RedDotEntity>();
        if (!entity || count < 0)
            return false;

        entity.signals[Excel.RED_DOT_HOME_MAILTIPS] = count;

        return true;
    }

    /// <summary>Stop the service through the system coordinator.</summary>
    public static void Stop()
    {
        XPluginUtility.FireEvent(new SystemDetive { type = typeof(RedDotSystem) });
    }

    /// <summary>Reactivate the service while Home remains open.</summary>
    public static void Start()
    {
        XPluginUtility.FireEvent(new SystemActive { type = typeof(RedDotSystem) });
    }
}

/// <summary>Temporary event observer installed only by the editor probe.</summary>
public class RedDotObserver : XEventModule
{
    /// <summary>Observed mail notifications.</summary>
    public static int notifications
    { get; private set; }

    /// <summary>Last notified count.</summary>
    public static int value
    { get; private set; }

    /// <summary>Frame of the last notification.</summary>
    public static int frame
    { get; private set; }

    /// <summary>Begin an isolated observation interval.</summary>
    /// <returns>True after registering the observer.</returns>
    public override bool OnActive()
    {
        notifications = 0;
        Subscribe<RedDotChanged>(OnChanged);

        return true;
    }

    /// <summary>Remove the temporary subscription.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();
    }

    /// <summary>Record only the mail condition tested by the probe.</summary>
    /// <param name="data">Calculated notification change.</param>
    private void OnChanged(RedDotChanged data)
    {
        if (data.id == Excel.RED_DOT_HOME_MAILTIPS)
        {
            notifications++;
            value = data.count;
            frame = Time.frameCount;
        }
    }
}
#endif
