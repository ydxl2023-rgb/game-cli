using Oathx.Widget;

/// <summary>Presentation bindings for ShopGiftItem.</summary>
public partial class VMShopGiftItem : VM
{
    /// <summary>Bound price presentation value.</summary>
    [Binding(typeof(SyncText))]
    string price;

    /// <summary>Bound title presentation value.</summary>
    [Binding(typeof(SyncText))]
    string title;

    /// <summary>Bound limit presentation value.</summary>
    [Binding(typeof(SyncText))]
    string limit;

    /// <summary>Bound icon presentation value.</summary>
    [Binding(typeof(SyncImage))]
    string icon;

    /// <summary>Bound count presentation value.</summary>
    [Binding(typeof(SyncText))]
    string count;

    /// <summary>Bound soldOut presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool soldOut;

    /// <summary>Bound soldOutText presentation value.</summary>
    [Binding(typeof(SyncText))]
    string soldOutText;
}
