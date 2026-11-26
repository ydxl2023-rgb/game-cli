using Oathx.Widget;

/// <summary>Bindings for the hospital presentation.</summary>
public partial class VMHomeHospital : VM
{
    /// <summary>Presentation binding for craftClick.</summary>
    [Binding]
    ButtonClick craftClick;

    /// <summary>Presentation binding for closeClick.</summary>
    [Binding]
    ButtonClick closeClick;

    /// <summary>Presentation binding for claimClick.</summary>
    [Binding]
    ButtonClick claimClick;

    /// <summary>Presentation binding for title.</summary>
    [Binding(typeof(SyncText))]
    string title;

    /// <summary>Presentation binding for productName.</summary>
    [Binding(typeof(SyncText))]
    string productName;

    /// <summary>Presentation binding for productStock.</summary>
    [Binding(typeof(SyncText))]
    string productStock;

    /// <summary>Presentation binding for materialStock.</summary>
    [Binding(typeof(SyncText))]
    string materialStock;

    /// <summary>Presentation binding for detail.</summary>
    [Binding(typeof(SyncText))]
    string detail;

    /// <summary>Presentation binding for gold.</summary>
    [Binding(typeof(SyncText))]
    string gold;

    /// <summary>Presentation binding for revenue.</summary>
    [Binding(typeof(SyncText))]
    string revenue;

    /// <summary>Presentation binding for taskText.</summary>
    [Binding(typeof(SyncText))]
    string taskText;

    /// <summary>Presentation binding for treatment.</summary>
    [Binding(typeof(SyncText))]
    string treatment;

    /// <summary>Presentation binding for feedback.</summary>
    [Binding(typeof(SyncText))]
    string feedback;

    /// <summary>Presentation binding for claimVisible.</summary>
    [Binding(typeof(SyncVisible))]
    bool claimVisible;

    /// <summary>Presentation binding for productIcon.</summary>
    [Binding(typeof(SyncSprite))]
    string productIcon;

    /// <summary>Presentation binding for materialIcon.</summary>
    [Binding(typeof(SyncSprite))]
    string materialIcon;

    /// <summary>Presentation binding for gem.</summary>
    [Binding(typeof(SyncText))]
    string gem;

    /// <summary>Presentation binding for priceBonus.</summary>
    [Binding(typeof(SyncText))]
    string priceBonus;

    /// <summary>Presentation binding for timeBonus.</summary>
    [Binding(typeof(SyncText))]
    string timeBonus;

    /// <summary>Presentation binding for capacityBonus.</summary>
    [Binding(typeof(SyncText))]
    string capacityBonus;

    /// <summary>Presentation binding for productsVisible.</summary>
    [Binding(typeof(SyncVisible))]
    bool productsVisible;

    /// <summary>Presentation binding for emptyVisible.</summary>
    [Binding(typeof(SyncVisible))]
    bool emptyVisible;

}
