using Oathx.Widget;

/// <summary>Bindings for the local portrait entry page.</summary>
public partial class VMLogin : VM
{
    /// <summary>Request entry into the local game.</summary>
    [Binding]
    ButtonClick enterClick;

    /// <summary>Show the enabled start control.</summary>
    [Binding(typeof(SyncVisible))]
    bool enterVisible;

    /// <summary>Show progress while the town is prepared.</summary>
    [Binding(typeof(SyncVisible))]
    bool loadingVisible;

    /// <summary>Show recoverable entry failure feedback.</summary>
    [Binding(typeof(SyncVisible))]
    bool failedVisible;
}
