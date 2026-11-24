using Oathx.Widget;
using UnityEngine;

/// <summary>Bindings shared by original MainPage entry appearances.</summary>
public partial class VMHomeEntry : VM
{
    /// <summary>Stable intention identifier, not a display binding.</summary>
    public int entryId
    { get; set; }

    /// <summary>Requests the entry destination.</summary>
    [Binding]
    ButtonClick openClick;

    /// <summary>Shows the entry artwork and hit area.</summary>
    [Binding(typeof(SyncVisible))]
    bool visible;

    /// <summary>Reflows the inner entry without recreating widgets.</summary>
    [Binding(typeof(SyncAnchoredPosition))]
    Vector2 position;

    /// <summary>Displays a lock overlay while preserving explanatory clicks.</summary>
    [Binding(typeof(SyncVisible))]
    bool locked;
}
