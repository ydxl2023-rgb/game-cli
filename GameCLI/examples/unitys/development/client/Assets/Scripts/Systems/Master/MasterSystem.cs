using Oathx.Core;
using Oathx.Entity;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Requests system loading and activation through MasterSystem.
/// </summary>
public class SystemActive : IEvent
{
    /// <summary>
    /// Runtime system type to activate.
    /// </summary>
    public System.Type type { get; set; }

    /// <summary>
    /// Optional action after the system activation step completes.
    /// </summary>
    public System.Action complete { get; set; }
}

/// <summary>
/// Requests system deactivation and unloading through MasterSystem.
/// </summary>
public class SystemDetive : IEvent
{
    /// <summary>
    /// Runtime system type to unload.
    /// </summary>
    public System.Type type { get; set; }

    /// <summary>
    /// Optional action after the system unloading step completes.
    /// </summary>
    public System.Action complete { get; set; }
}

/// <summary>
/// Local login success notification that permits game startup.
/// </summary>
public class LoginSucceed : IEvent
{
    /// <summary>
    /// Player identifier supplied by the local caller.
    /// </summary>
    public int pid 
    { get; set; }
}

/// <summary>
/// Local login failure notification.
/// </summary>
public class LoginFailure : IEvent
{
    /// <summary>
    /// Player identifier supplied by the local caller.
    /// </summary>
    public int code
    { get; set; }
}

/// <summary>
/// Coordinates local player persistence and business system startup.
/// </summary>
public class MasterSystem : RuntimeSystem
{
    /// <summary>Pending coordinator requests, canceled when their system is unloaded.</summary>
    private readonly Dictionary<System.Type, SystemActive> pending = new Dictionary<System.Type, SystemActive>();

    /// <summary>
    /// Subscribe to local login success without starting Home.
    /// </summary>
    /// <returns>True after registering the local login handler.</returns>
    public override bool OnActive()
    {
        Subscribe<LoginSucceed>(OnLoginSucceed);
        Subscribe<SystemActive>(OnSystemActive);
        Subscribe<SystemDetive>(OnSystemDetive);
        Subscribe<HomeEntryRequested>(OnHomeEntryRequested);
        Subscribe<FunctionOpenChanged>(OnFunctionChanged);

        return true;
    }

    /// <summary>
    /// Release local event subscriptions and unload Home.
    /// </summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        Unload();
    }

    /// <summary>
    /// Restore local data or persist initial values before starting business systems.
    /// </summary>
    /// <returns>True when local player data is ready.</returns>
    public bool Load()
    {
        if (!IsActive())
            return false;

        if (!XEntityManager.GetSingleton().TryGetEntity(nameof(MasterEntity), out var entity)
            || !(entity is MasterEntity master))
            return false;

        bool ready = master.ExistsSerializeFile()
            ? master.Deserialize()
            : !string.IsNullOrEmpty(master.Serialize());
        if (!ready)
            return false;

        return true;
    }

    /// <summary>
    /// Unload Home, including during deactivation.
    /// </summary>
    /// <returns>True after requesting Home removal from the dispatcher.</returns>
    public bool Unload()
    {
        pending.Clear();

        dispatcher.Unload(typeof(SettingsSystem));
        dispatcher.Unload(typeof(MailSystem));
        dispatcher.Unload(typeof(ShopSystem));
        dispatcher.Unload(typeof(NoticeSystem));
        dispatcher.Unload(typeof(FightSystem));
        dispatcher.Unload(typeof(HomeSystem));

        return true;
    }

    /// <summary>
    /// Persist the current local player data without contacting a server.
    /// </summary>
    /// <returns>True when the local archive has been written.</returns>
    public bool Save()
    {
        if (!IsActive())
            return false;

        if (!XEntityManager.GetSingleton().TryGetEntity(nameof(MasterEntity), out var entity)
            || !(entity is MasterEntity master))
            return false;

        return !string.IsNullOrEmpty(master.Serialize());
    }

    /// <summary>
    /// Load and activate a requested business system.
    /// </summary>
    /// <param name="data">Target system and optional completion action.</param>
    private void OnSystemActive(SystemActive data)
    {
        if (IsActive() && IsSystemType(data.type) && FunctionManagerAccess.CanActivate(data.type))
        {
            pending[data.type] = data;

            XCoroutine.Run(ActivateSystem(data));
        }
    }

    /// <summary>Activate the system requested by a Home navigation entry.</summary>
    /// <param name="data">Home navigation request.</param>
    private void OnHomeEntryRequested(HomeEntryRequested data)
    {
        if (IsActive() && data.entry == HomeEntry.Shop)
            OnSystemActive(new SystemActive { type = typeof(ShopSystem) });

        if (IsActive() && data.entry == HomeEntry.Mail)
            OnSystemActive(new SystemActive { type = typeof(MailSystem) });

        if (IsActive() && data.entry == HomeEntry.Settings)
            OnSystemActive(new SystemActive { type = typeof(SettingsSystem) });

        if (IsActive() && data.entry == HomeEntry.Notice)
            OnSystemActive(new SystemActive { type = typeof(NoticeSystem) });
    }

    /// <summary>
    /// Wait until event dispatch has finished before changing the system registry.
    /// </summary>
    /// <param name="data">Target system and optional completion action.</param>
    /// <returns>The deferred activation sequence.</returns>
    private IEnumerator ActivateSystem(SystemActive data)
    {
        yield return null;

        if (pending.TryGetValue(data.type, out var current) && ReferenceEquals(current, data))
        {
            pending.Remove(data.type);

            if (IsActive() && FunctionManagerAccess.CanActivate(data.type))
                LoadSystem(data.type, data.complete);
        }
    }

    /// <summary>
    /// Unload a requested business system and its dependent pages.
    /// </summary>
    /// <param name="data">Target system and optional completion action.</param>
    private void OnSystemDetive(SystemDetive data)
    {
        if (IsActive() && IsSystemType(data.type))
        {
            pending.Remove(data.type);

            if (data.type == typeof(LoginSystem))
                dispatcher.Unload(typeof(NoticeSystem));

            UnloadSystem(data.type, data.complete);
        }
    }

    /// <summary>Close a business system when its configured gate becomes unavailable.</summary>
    /// <param name="data">Calculated availability transition.</param>
    private void OnFunctionChanged(FunctionOpenChanged data)
    {
        if (!data.opened && Config.Excel.functionSystemConfig.TryGetValue(data.id, out var row))
        {
            var type = FunctionManagerAccess.ResolveType(row);
            if (IsSystemType(type) && type != typeof(FunctionManagerSystem) && type != typeof(RedDotSystem))
                XCoroutine.Run(CloseUnavailable(type));
        }
    }

    /// <summary>Defer registry changes and reject stale close messages.</summary>
    /// <param name="type">Business system to close.</param>
    /// <returns>The deferred close sequence.</returns>
    private IEnumerator CloseUnavailable(System.Type type)
    {
        yield return null;

        if (IsActive() && !FunctionManagerAccess.CanActivate(type))
            UnloadSystem(type, null);
    }

    /// <summary>
    /// Reject invalid targets and requests to replace the lifecycle coordinator.
    /// </summary>
    /// <param name="type">Requested system type.</param>
    /// <returns>True for a concrete business runtime system.</returns>
    private bool IsSystemType(System.Type type)
    {
        return type != null && type != typeof(MasterSystem) && !type.IsAbstract
            && typeof(RuntimeSystem).IsAssignableFrom(type);
    }

    /// <summary>
    /// Prepare local player data and start Home after local login succeeds.
    /// </summary>
    /// <param name="data">Local login success notification.</param>
    private void OnLoginSucceed(LoginSucceed data)
    {
        if (Load())
        {
            Launch();
        }
    }

    /// <summary>
    /// Unload login and request Home startup.
    /// </summary>
    /// <returns>True when the transition has been requested.</returns>
    private bool Launch()
    {
        if (!IsActive())
            return false;

        dispatcher.Unload(typeof(NoticeSystem));

        UnloadSystem(typeof(LoginSystem), () => {
            if (IsActive())
            {
                LoadSystem(typeof(HomeSystem));
            }
        });

        return true;
    }
}
