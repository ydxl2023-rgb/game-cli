using Oathx.Widget;

/// <summary>Presentation data for an attached decoration.</summary>
public partial class VMActivityBadge : VM
{
    /// <summary>Whether the decoration visual is shown.</summary>
    [Binding(typeof(SyncVisible))]
    bool visible;
}
