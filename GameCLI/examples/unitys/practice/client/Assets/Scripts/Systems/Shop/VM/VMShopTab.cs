using Oathx.Widget;

/// <summary>Presentation bindings for ShopTab.</summary>
public partial class VMShopTab : VM
{
    /// <summary>Bound title presentation value.</summary>
    [Binding(typeof(SyncText))]
    string title;

    /// <summary>Bound selected presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool selected;

    /// <summary>Bound unselected presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool unselected;
}
