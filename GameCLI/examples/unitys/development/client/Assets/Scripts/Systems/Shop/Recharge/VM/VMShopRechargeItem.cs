using Oathx.Widget;

/// <summary>Presentation bindings for ShopRechargeItem.</summary>
public partial class VMShopRechargeItem : VM
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

    /// <summary>Bound bonus presentation value.</summary>
    [Binding(typeof(SyncText))]
    string bonus;

    /// <summary>Shared item presentation hosted by this product card.</summary>
    [Binding(typeof(SyncMetadata))]
    VMItemSlot item;

    /// <summary>Bound firstPay presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool firstPay;

    /// <summary>Bound firstPayText presentation value.</summary>
    [Binding(typeof(SyncText))]
    string firstPayText;
}
