using Oathx.Widget;

/// <summary>Presentation bindings for ShopGiftItem.</summary>
public partial class VMShopGiftItem : VM
{
    /// <summary>Stable catalog identifier for selection.</summary>
    public int productId
    { get; set; }

    /// <summary>Bound price presentation value.</summary>
    [Binding(typeof(SyncText))]
    string price;

    /// <summary>Bound title presentation value.</summary>
    [Binding(typeof(SyncText))]
    string title;

    /// <summary>Bound limit presentation value.</summary>
    [Binding(typeof(SyncText))]
    string limit;

    /// <summary>Shared item presentation hosted by this product card.</summary>
    [Binding(typeof(SyncMetadata))]
    VMItemSlot item;

    /// <summary>Bound soldOut presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool soldOut;

    /// <summary>Bound soldOutText presentation value.</summary>
    [Binding(typeof(SyncText))]
    string soldOutText;
}
