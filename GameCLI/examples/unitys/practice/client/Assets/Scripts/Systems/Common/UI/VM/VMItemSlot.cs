using UnityEngine;
using Oathx.Widget;

/// <summary>Host-owned presentation data for a reusable item slot.</summary>
public partial class VMItemSlot : VM
{
    /// <summary>Background asset path supplied by the host.</summary>
    [Binding]
    string back;

    /// <summary>Icon asset path resolved by the shared widget loader.</summary>
    [Binding(typeof(SyncSprite))]
    string icon;

    /// <summary>Formatted quantity or label supplied by the host.</summary>
    [Binding]
    string text;

    /// <summary>Whether the slot is locked.</summary>
    [Binding]
    bool locked;

    /// <summary>Normalized overlay fill.</summary>
    [Binding]
    float amount;

    /// <summary>Whether the slot is selected.</summary>
    [Binding]
    bool select;

    /// <summary>Whether the slot shows its hint arrow.</summary>
    [Binding]
    bool arrow;

    /// <summary>Host-defined border color.</summary>
    [Binding]
    Color edge;

    /// <summary>Whether the host shows the icon.</summary>
    [Binding(typeof(SyncVisible))]
    bool iconVisible;

    /// <summary>Product-defined category, independent of equipment enums.</summary>
    public int type
    { get; set; }
}
