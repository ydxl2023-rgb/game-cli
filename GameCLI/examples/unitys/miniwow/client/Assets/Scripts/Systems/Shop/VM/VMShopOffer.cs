using Oathx.Widget;

/// <summary>Presentation bindings for VMShopOffer.</summary>
public partial class VMShopOffer : VM
{
    /// <summary>Stable original offer identity.</summary>
    public int offerId
    { get; set; }

    /// <summary>Whether this card represents the item requested by the tracked purchase task.</summary>
    public bool isGuideTarget
    { get; set; }

    /// <summary>Displays or submits name.</summary>
    [Binding(typeof(SyncText))]
    string name;

    /// <summary>Displays or submits icon.</summary>
    [Binding(typeof(SyncSprite))]
    string icon;

    /// <summary>Displays or submits price.</summary>
    [Binding(typeof(SyncText))]
    string price;

    /// <summary>Displays or submits stock.</summary>
    [Binding(typeof(SyncText))]
    string stock;

    /// <summary>Displays or submits owned.</summary>
    [Binding(typeof(SyncText))]
    string owned;

    /// <summary>Displays or submits selectClick.</summary>
    [Binding]
    ButtonClick selectClick;

}
