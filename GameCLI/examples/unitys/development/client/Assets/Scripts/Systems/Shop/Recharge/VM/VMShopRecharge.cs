using Oathx.Widget;

/// <summary>Presentation bindings for ShopRecharge.</summary>
public partial class VMShopRecharge : VM
{
    /// <summary>Bound products presentation value.</summary>
    [Binding(typeof(SyncFancyGridScrollView))]
    VList<VMShopRechargeItem> products;

    /// <summary>Bound productClick presentation value.</summary>
    [Binding(typeof(SyncFancyGridClick))]
    FancyGridClick productClick;

    /// <summary>Bound productTemplate presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool productTemplate;
}
