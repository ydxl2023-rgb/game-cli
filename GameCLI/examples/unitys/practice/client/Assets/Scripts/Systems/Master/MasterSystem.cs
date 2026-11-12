using Oathx.Core;
using Oathx.Entity;

/// <summary>Requests activation through the product coordinator.</summary>
public class SystemActive : IEvent
{
    /// <summary>System type requested by the caller.</summary>
    public System.Type type
    { get; set; }
}

/// <summary>Requests deactivation through the product coordinator.</summary>
public class SystemDetive : IEvent
{
    /// <summary>System type requested by the caller.</summary>
    public System.Type type
    { get; set; }
}

/// <summary>Reports successful local test login without server authentication.</summary>
public class LoginSucceed : IEvent
{
    /// <summary>Positive local player identifier.</summary>
    public int pid
    { get; set; }
}

/// <summary>Coordinates local system events without introducing game systems.</summary>
public class MasterSystem : RuntimeSystem
{
    /// <summary>Subscribe to local lifecycle requests.</summary>
    /// <returns>True after all handlers have been registered.</returns>
    public override bool OnActive()
    {
        Subscribe<SystemActive>(OnSystemActive);
        Subscribe<SystemDetive>(OnSystemDetive);
        Subscribe<LoginSucceed>(OnLoginSucceed);

        return true;
    }

    /// <summary>Release local subscriptions when the coordinator stops.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();
    }

    /// <summary>Request a valid system activation after event dispatch completes.</summary>
    /// <param name="data">Requested system type.</param>
    private void OnSystemActive(SystemActive data)
    {
        if (data != null && IsSystemType(data.type))
        {
            LoadSystem(data.type);
        }
    }

    /// <summary>Request a valid system deactivation after event dispatch completes.</summary>
    /// <param name="data">Requested system type.</param>
    private void OnSystemDetive(SystemDetive data)
    {
        if (data != null && IsSystemType(data.type))
        {
            UnloadSystem(data.type);
        }
    }

    /// <summary>Record successful local login without creating a Home system.</summary>
    /// <param name="data">Local login result.</param>
    private void OnLoginSucceed(LoginSucceed data)
    {
        if (data != null && data.pid > 0)
        {
            var entity = XEntityManager.GetSingleton().QueryEntity<MasterEntity>();
            if (entity)
            {
                entity.pid = data.pid;
            }
        }
    }

    /// <summary>Validate a concrete runtime system type.</summary>
    /// <param name="type">Requested type.</param>
    /// <returns>True for supported systems other than this coordinator.</returns>
    private bool IsSystemType(System.Type type)
    {
        return IsActive() && type != null && type != typeof(MasterSystem)
            && !type.IsAbstract && !type.ContainsGenericParameters
            && typeof(RuntimeSystem).IsAssignableFrom(type)
            && type.GetConstructor(System.Type.EmptyTypes) != null;
    }
}
