using Oathx.Widget;

/// <summary>Presentation bindings for ShopRechargeItem.</summary>
public partial class VMShopRechargeItem : VM
{
    /// <summary>Bound price presentation value.</summary>
    [Binding(typeof(SyncText))]
    string price;

    /// <summary>Bound title presentation value.</summary>
    [Binding(typeof(SyncText))]
    string title;

    /// <summary>Bound bonus presentation value.</summary>
    [Binding(typeof(SyncText))]
    string bonus;

    /// <summary>Bound icon presentation value.</summary>
    [Binding(typeof(SyncImage))]
    string icon;

    /// <summary>Bound firstPay presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool firstPay;

    /// <summary>Bound firstPayText presentation value.</summary>
    [Binding(typeof(SyncText))]
    string firstPayText;
}
