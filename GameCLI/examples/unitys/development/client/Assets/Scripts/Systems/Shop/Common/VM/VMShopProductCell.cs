using Oathx.Widget;
using UnityEngine;

/// <summary>Presentation bindings for a configured shop product or milestone.</summary>
public partial class VMShopProductCell : VM
{
    /// <summary>Title.</summary>
    [Binding(typeof(SyncText))]
    string title;

    /// <summary>Price.</summary>
    [Binding(typeof(SyncText))]
    string price;

    /// <summary>Quantity.</summary>
    [Binding(typeof(SyncText))]
    string quantity;

    /// <summary>Limit.</summary>
    [Binding(typeof(SyncText))]
    string limit;

    /// <summary>Discount.</summary>
    [Binding(typeof(SyncText))]
    string discount;

    /// <summary>Condition.</summary>
    [Binding(typeof(SyncText))]
    string condition;

    /// <summary>Configured product artwork.</summary>
    [Binding(typeof(SyncSprite))]
    Sprite icon;

    /// <summary>Icon Visible.</summary>
    [Binding(typeof(SyncVisible))]
    bool iconVisible;

    /// <summary>Price currency artwork.</summary>
    [Binding(typeof(SyncSprite))]
    Sprite currency;

    /// <summary>Currency Visible.</summary>
    [Binding(typeof(SyncVisible))]
    bool currencyVisible;

    /// <summary>Premium reward artwork.</summary>
    [Binding(typeof(SyncSprite))]
    Sprite premium;

    /// <summary>Premium Visible.</summary>
    [Binding(typeof(SyncVisible))]
    bool premiumVisible;

    /// <summary>Show the purchased or claimed overlay.</summary>
    [Binding(typeof(SyncVisible))]
    bool purchased;

    /// <summary>Show the unlock requirement.</summary>
    [Binding(typeof(SyncVisible))]
    bool locked;

    /// <summary>Discount Tag.</summary>
    [Binding(typeof(SyncVisible))]
    bool discountTag;

    /// <summary>Select this product for preview.</summary>
    [Binding(typeof(SyncClick))]
    ButtonClick select;
}
