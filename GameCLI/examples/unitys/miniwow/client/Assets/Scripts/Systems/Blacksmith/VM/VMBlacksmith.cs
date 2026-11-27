using Oathx.Widget;

/// <summary>Bindings for the forge presentation.</summary>
public partial class VMBlacksmith : VM
{
    /// <summary>Displays or submits entries.</summary>
    [Binding(typeof(SyncScrollRect))]
    VList<VMBlacksmithEntry> entries;

    /// <summary>Displays or submits gold.</summary>
    [Binding(typeof(SyncText))]
    string gold;

    /// <summary>Displays or submits feedback.</summary>
    [Binding(typeof(SyncText))]
    string feedback;

    /// <summary>Displays or submits repairText.</summary>
    [Binding(typeof(SyncText))]
    string repairText;

    /// <summary>Displays or submits heroText.</summary>
    [Binding(typeof(SyncText))]
    string heroText;

    /// <summary>Displays or submits equipmentText.</summary>
    [Binding(typeof(SyncText))]
    string equipmentText;

    /// <summary>Displays or submits revenue.</summary>
    [Binding(typeof(SyncText))]
    string revenue;

    /// <summary>Displays or submits repairVisible.</summary>
    [Binding(typeof(SyncVisible))]
    bool repairVisible;

    /// <summary>Displays or submits catalogVisible.</summary>
    [Binding(typeof(SyncVisible))]
    bool catalogVisible;

    /// <summary>Displays or submits heroVisible.</summary>
    [Binding(typeof(SyncVisible))]
    bool heroVisible;

    /// <summary>Displays or submits closeClick.</summary>
    [Binding]
    ButtonClick closeClick;

    /// <summary>Displays or submits repairClick.</summary>
    [Binding]
    ButtonClick repairClick;

    /// <summary>Displays or submits recipesClick.</summary>
    [Binding]
    ButtonClick recipesClick;

    /// <summary>Displays or submits shelfClick.</summary>
    [Binding]
    ButtonClick shelfClick;

    /// <summary>Displays or submits heroClick.</summary>
    [Binding]
    ButtonClick heroClick;

    /// <summary>Displays or submits collectClick.</summary>
    [Binding]
    ButtonClick collectClick;

}
