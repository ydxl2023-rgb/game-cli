using Oathx.Widget;

/// <summary>Presentation bindings for VMShopPurchase.</summary>
public partial class VMShopPurchase : VM
{
    /// <summary>Displays or submits name.</summary>
    [Binding(typeof(SyncText))]
    string name;

    /// <summary>Displays or submits description.</summary>
    [Binding(typeof(SyncText))]
    string description;

    /// <summary>Displays or submits icon.</summary>
    [Binding(typeof(SyncSprite))]
    string icon;

    /// <summary>Displays or submits quantity.</summary>
    [Binding(typeof(SyncText))]
    string quantity;

    /// <summary>Displays or submits total.</summary>
    [Binding(typeof(SyncText))]
    string total;

    /// <summary>Displays or submits stock.</summary>
    [Binding(typeof(SyncText))]
    string stock;

    /// <summary>Displays or submits owned.</summary>
    [Binding(typeof(SyncText))]
    string owned;

    /// <summary>Displays or submits feedback.</summary>
    [Binding(typeof(SyncText))]
    string feedback;

    /// <summary>Displays or submits minusClick.</summary>
    [Binding]
    ButtonClick minusClick;

    /// <summary>Displays or submits plusClick.</summary>
    [Binding]
    ButtonClick plusClick;

    /// <summary>Displays or submits confirmClick.</summary>
    [Binding]
    ButtonClick confirmClick;

    /// <summary>Displays or submits cancelClick.</summary>
    [Binding]
    ButtonClick cancelClick;

}
