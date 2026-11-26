using Oathx.Widget;

/// <summary>Bindings for the hospital presentation.</summary>
public partial class VMHomeHospitalEntry : VM
{
    /// <summary>Localized name of the configured building.</summary>
    [Binding(typeof(SyncText))]
    string buildingName;

    /// <summary>Presentation binding for enterClick.</summary>
    [Binding]
    ButtonClick enterClick;

    /// <summary>Requests the repair interaction.</summary>
    [Binding]
    ButtonClick repairClick;

    /// <summary>Shows the original constructed-building entrance.</summary>
    [Binding(typeof(SyncVisible))]
    bool builtVisible;

    /// <summary>Presentation binding for cost.</summary>
    [Binding(typeof(SyncText))]
    string cost;

    /// <summary>Presentation binding for feedback.</summary>
    [Binding(typeof(SyncText))]
    string feedback;

    /// <summary>Presentation binding for entryVisible.</summary>
    [Binding(typeof(SyncVisible))]
    bool entryVisible;

    /// <summary>Presentation binding for repairVisible.</summary>
    [Binding(typeof(SyncVisible))]
    bool repairVisible;

    /// <summary>Requests entry to the restored merchant.</summary>
    [Binding]
    ButtonClick merchantClick;

    /// <summary>Requests entry to the restored blacksmith.</summary>
    [Binding]
    ButtonClick forgeClick;

    /// <summary>Requests entry to the restored innkeeper.</summary>
    [Binding]
    ButtonClick hotelClick;

}
