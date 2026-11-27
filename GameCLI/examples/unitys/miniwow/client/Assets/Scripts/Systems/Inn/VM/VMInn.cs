using Oathx.Widget;

/// <summary>Bindings for the independent inn page.</summary>
public partial class VMInn : VM
{
    /// <summary>Presentation binding for closeClick.</summary>
    [Binding]
    ButtonClick closeClick;

    /// <summary>Presentation binding for repairClick.</summary>
    [Binding]
    ButtonClick repairClick;

    /// <summary>Presentation binding for craftClick.</summary>
    [Binding]
    ButtonClick craftClick;

    /// <summary>Presentation binding for restClick.</summary>
    [Binding]
    ButtonClick restClick;

    /// <summary>Presentation binding for title.</summary>
    [Binding(typeof(SyncText))]
    string title;

    /// <summary>Presentation binding for gold.</summary>
    [Binding(typeof(SyncText))]
    string gold;

    /// <summary>Presentation binding for gem.</summary>
    [Binding(typeof(SyncText))]
    string gem;

    /// <summary>Presentation binding for revenue.</summary>
    [Binding(typeof(SyncText))]
    string revenue;

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

    /// <summary>Presentation binding for feedback.</summary>
    [Binding(typeof(SyncText))]
    string feedback;

    /// <summary>Presentation binding for energy.</summary>
    [Binding(typeof(SyncText))]
    string energy;

    /// <summary>Presentation binding for repairCost.</summary>
    [Binding(typeof(SyncText))]
    string repairCost;

    /// <summary>Presentation binding for priceBonus.</summary>
    [Binding(typeof(SyncText))]
    string priceBonus;

    /// <summary>Presentation binding for timeBonus.</summary>
    [Binding(typeof(SyncText))]
    string timeBonus;

    /// <summary>Presentation binding for capacityBonus.</summary>
    [Binding(typeof(SyncText))]
    string capacityBonus;

    /// <summary>Presentation binding for productIcon.</summary>
    [Binding(typeof(SyncSprite))]
    string productIcon;

    /// <summary>Presentation binding for materialIcon.</summary>
    [Binding(typeof(SyncSprite))]
    string materialIcon;

    /// <summary>Presentation binding for repairVisible.</summary>
    [Binding(typeof(SyncVisible))]
    bool repairVisible;

    /// <summary>Presentation binding for businessVisible.</summary>
    [Binding(typeof(SyncVisible))]
    bool businessVisible;

    /// <summary>Presentation binding for productsVisible.</summary>
    [Binding(typeof(SyncVisible))]
    bool productsVisible;

    /// <summary>Presentation binding for emptyVisible.</summary>
    [Binding(typeof(SyncVisible))]
    bool emptyVisible;

    /// <summary>Presentation binding for meal availability.</summary>
    [Binding(typeof(SyncVisible))]
    bool restVisible;

    /// <summary>Presentation binding for the dining hero.</summary>
    [Binding(typeof(SyncVisible))]
    bool patient;

    /// <summary>Presentation binding for the relevant tracked task.</summary>
    [Binding(typeof(SyncText))]
    string taskText;

    /// <summary>Presentation binding for an earned task reward.</summary>
    [Binding(typeof(SyncVisible))]
    bool claimVisible;

    /// <summary>Claims an already completed task.</summary>
    [Binding]
    ButtonClick claimClick;
}
