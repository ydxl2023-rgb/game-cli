using Oathx.Widget;

/// <summary>Presentation bindings for ShopGroup.</summary>
public partial class VMShopGroup : VM
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

    /// <summary>Bound redPoint presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool redPoint;

    /// <summary>Bound selectedRedPoint presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool selectedRedPoint;
}
