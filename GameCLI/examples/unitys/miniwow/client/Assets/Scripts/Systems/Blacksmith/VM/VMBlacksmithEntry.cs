using Oathx.Widget;

/// <summary>Bindings for the forge presentation.</summary>
public partial class VMBlacksmithEntry : VM
{
    /// <summary>Recipe or equipment identity carried by this row.</summary>
    public long identity
    { get; set; }

    /// <summary>Whether this row represents a saved shelf instance.</summary>
    public bool shelf
    { get; set; }

    /// <summary>Displays or submits title.</summary>
    [Binding(typeof(SyncText))]
    string title;

    /// <summary>Displays or submits detail.</summary>
    [Binding(typeof(SyncText))]
    string detail;

    /// <summary>Displays or submits actionText.</summary>
    [Binding(typeof(SyncText))]
    string actionText;

    /// <summary>Displays or submits icon.</summary>
    [Binding(typeof(SyncSprite))]
    string icon;

    /// <summary>Displays or submits selectClick.</summary>
    [Binding]
    ButtonClick selectClick;

}
