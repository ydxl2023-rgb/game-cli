using Oathx.Widget;

/// <summary>Presentation bindings for VMShop.</summary>
public partial class VMShop : VM
{
    /// <summary>Displays or submits offers.</summary>
    [Binding(typeof(SyncScrollRect))]
    VList<VMShopOffer> offers;

    /// <summary>Displays or submits gold.</summary>
    [Binding(typeof(SyncText))]
    string gold;

    /// <summary>Displays or submits taskText.</summary>
    [Binding(typeof(SyncText))]
    string taskText;

    /// <summary>Displays or submits feedback.</summary>
    [Binding(typeof(SyncText))]
    string feedback;

    /// <summary>Displays or submits claimVisible.</summary>
    [Binding(typeof(SyncVisible))]
    bool claimVisible;

    /// <summary>Displays or submits closeClick.</summary>
    [Binding]
    ButtonClick closeClick;

    /// <summary>Displays or submits claimClick.</summary>
    [Binding]
    ButtonClick claimClick;

}
