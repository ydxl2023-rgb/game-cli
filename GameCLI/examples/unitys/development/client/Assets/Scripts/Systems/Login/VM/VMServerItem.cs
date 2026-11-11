using Oathx.Widget;

/// <summary>Bound presentation data for a server selection row.</summary>
public partial class VMServerItem : VM
{
    /// <summary>Presentation value for serverName.</summary>
    [Binding(typeof(SyncText))]
    string serverName;

    /// <summary>Presentation value for serverStatus.</summary>
    [Binding(typeof(SyncImage))]
    string serverStatus;

    /// <summary>Presentation value for recommend.</summary>
    [Binding(typeof(SyncVisible))]
    bool recommend;

    /// <summary>Presentation value for playerHead.</summary>
    [Binding(typeof(SyncVisible))]
    bool playerHead;
}
