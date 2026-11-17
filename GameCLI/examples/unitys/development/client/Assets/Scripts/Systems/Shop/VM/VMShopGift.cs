using Oathx.Widget;

/// <summary>Presentation bindings for ShopGift.</summary>
public partial class VMShopGift : VM
{
    /// <summary>Bound products presentation value.</summary>
    [Binding(typeof(SyncFancyGridScrollView))]
    VList<VMShopGiftItem> products;

    /// <summary>Bound productClick presentation value.</summary>
    [Binding(typeof(SyncFancyGridClick))]
    FancyGridClick productClick;

    /// <summary>Bound productTemplate presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool productTemplate;
}
