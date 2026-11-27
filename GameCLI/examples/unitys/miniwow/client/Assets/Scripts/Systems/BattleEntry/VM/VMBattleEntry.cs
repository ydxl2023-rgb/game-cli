using Oathx.Widget;

/// <summary>Bindings for the original map and first-stage list.</summary>
public partial class VMBattleEntry : VM
{
    /// <summary>Map entrance click.</summary>
    [Binding]
    ButtonClick stageClick;

    /// <summary>First available stage selection.</summary>
    [Binding]
    ButtonClick selectClick;

    /// <summary>Gather intention.</summary>
    [Binding]
    ButtonClick gatherClick;

    /// <summary>Page closure.</summary>
    [Binding]
    ButtonClick closeClick;

    /// <summary>Original scene title.</summary>
    [Binding(typeof(SyncText))]
    string title;

    /// <summary>Original title repeated on the stage panel.</summary>
    [Binding(typeof(SyncText))]
    string detailTitle;

    /// <summary>Original first-stage name.</summary>
    [Binding(typeof(SyncText))]
    string stageName;

    /// <summary>Original area description.</summary>
    [Binding(typeof(SyncText))]
    string description;

    /// <summary>Original recommended power.</summary>
    [Binding(typeof(SyncText))]
    string power;

    /// <summary>Actual entry outcome.</summary>
    [Binding(typeof(SyncText))]
    string feedback;

    /// <summary>Original map layer.</summary>
    [Binding(typeof(SyncVisible))]
    bool mapVisible;

    /// <summary>Original stage detail layer.</summary>
    [Binding(typeof(SyncVisible))]
    bool stageVisible;
}
