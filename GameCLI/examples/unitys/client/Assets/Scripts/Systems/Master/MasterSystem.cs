using Oathx.Core;
using Oathx.Entity;

/// <summary>
/// Shared login notification retained for existing local subscribers.
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
    /// <summary>
    /// Load local player data and request Home startup.
    /// </summary>
    /// <returns>True when local data is ready and startup has been requested.</returns>
    public override bool OnActive()
    {
        return Load();
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
    /// <returns>True when local data is ready and startup has been requested.</returns>
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

        return Launch();
    }

    /// <summary>
    /// Unload Home, including during deactivation.
    /// </summary>
    /// <returns>True after requesting Home removal from the dispatcher.</returns>
    public bool Unload()
    {
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
    /// Unload login and request Home startup.
    /// </summary>
    /// <returns>True when the transition has been requested.</returns>
    private bool Launch()
    {
        if (!IsActive())
            return false;

        UnloadSystem(typeof(LoginSystem), () => {
            if (IsActive())
            {
                LoadSystem(typeof(HomeSystem));
            }
        });

        return true;
    }
}
