using Oathx.Widget;

/// <summary>Bound presentation data for a server selection row.</summary>
public partial class VMServerGroupItem : VM
{
    /// <summary>Presentation value for title.</summary>
    [Binding(typeof(SyncText))]
    string title;

    /// <summary>Presentation value for selected.</summary>
    [Binding(typeof(SyncVisible))]
    bool selected;

    /// <summary>Presentation value for unselected.</summary>
    [Binding(typeof(SyncVisible))]
    bool unselected;
}
