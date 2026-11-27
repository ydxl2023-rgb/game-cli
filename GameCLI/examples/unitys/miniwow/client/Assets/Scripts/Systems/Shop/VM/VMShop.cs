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

    /// <summary>Opens the configured material tier choices.</summary>
    [Binding]
    ButtonClick tierClick;

    /// <summary>Selects the available material tier.</summary>
    [Binding]
    ButtonClick tierOptionClick;

    /// <summary>Returns to the material tab.</summary>
    [Binding]
    ButtonClick materialClick;

    /// <summary>Explains the locked equipment tab.</summary>
    [Binding]
    ButtonClick equipmentClick;

    /// <summary>Requests the trading report entry.</summary>
    [Binding]
    ButtonClick logClick;

    /// <summary>Shows the original tier selector popup.</summary>
    [Binding(typeof(SyncVisible))]
    bool tierVisible;

}
