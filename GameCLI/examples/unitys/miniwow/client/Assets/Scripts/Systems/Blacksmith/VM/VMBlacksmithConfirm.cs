using Oathx.Widget;

/// <summary>Bindings for the forge presentation.</summary>
public partial class VMBlacksmithConfirm : VM
{
    /// <summary>Displays or submits title.</summary>
    [Binding(typeof(SyncText))]
    string title;

    /// <summary>Displays or submits cost.</summary>
    [Binding(typeof(SyncText))]
    string cost;

    /// <summary>Displays or submits quantity.</summary>
    [Binding(typeof(SyncText))]
    string quantity;

    /// <summary>Displays or submits feedback.</summary>
    [Binding(typeof(SyncText))]
    string feedback;

    /// <summary>Displays or submits confirmText.</summary>
    [Binding(typeof(SyncText))]
    string confirmText;

    /// <summary>Displays or submits icon.</summary>
    [Binding(typeof(SyncSprite))]
    string icon;

    /// <summary>Displays or submits quantityVisible.</summary>
    [Binding(typeof(SyncVisible))]
    bool quantityVisible;

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
