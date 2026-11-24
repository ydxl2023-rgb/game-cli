using Oathx.Widget;

/// <summary>Presentation bindings for ShopPurchase.</summary>
public partial class VMShopPurchase : VM
{
    /// <summary>Bound closeClick presentation value.</summary>
    [Binding]
    ButtonClick closeClick;

    /// <summary>Bound confirmClick presentation value.</summary>
    [Binding]
    ButtonClick confirmClick;

    /// <summary>Bound confirmText presentation value.</summary>
    [Binding(typeof(SyncText))]
    string confirmText;

    /// <summary>Bound price presentation value.</summary>
    [Binding(typeof(SyncText))]
    string price;

    /// <summary>Bound title presentation value.</summary>
    [Binding(typeof(SyncText))]
    string title;

    /// <summary>Bound icon presentation value.</summary>
    [Binding(typeof(SyncImage))]
    string icon;

    /// <summary>Bound rewardTitle presentation value.</summary>
    [Binding(typeof(SyncText))]
    string rewardTitle;

    /// <summary>Bound rewardName presentation value.</summary>
    [Binding(typeof(SyncText))]
    string rewardName;

    /// <summary>Bound rewardCount presentation value.</summary>
    [Binding(typeof(SyncText))]
    string rewardCount;

    /// <summary>Bound rewardIcon presentation value.</summary>
    [Binding(typeof(SyncImage))]
    string rewardIcon;

    /// <summary>Bound alternatePurchase presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool alternatePurchase;

    /// <summary>Bound freeText presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool freeText;

    /// <summary>Bound randomTitle presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool randomTitle;

    /// <summary>Bound randomRewards presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool randomRewards;

    /// <summary>Bound discount presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool discount;

    /// <summary>Bound labels presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool labels;
}
