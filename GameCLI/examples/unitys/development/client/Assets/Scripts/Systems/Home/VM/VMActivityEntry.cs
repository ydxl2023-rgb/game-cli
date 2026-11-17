using Oathx.Widget;

/// <summary>One configured activity entry; artwork remains in its item template.</summary>
public partial class VMActivityEntry : VM
{
    /// <summary>FunctionSystem identifier used for availability and navigation.</summary>
    public int systemId
    { get; set; }

    /// <summary>Request the configured system when the item is clicked.</summary>
    [Binding]
    ButtonClick openClick;

    /// <summary>Whether the configured activity is currently open.</summary>
    [Binding(typeof(SyncVisible))]
    bool visible;
}
