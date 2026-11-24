using Oathx.Widget;

/// <summary>Presentation bindings for the portrait camp and its map.</summary>
public partial class VMHome : VM
{
    /// <summary>Open the authored adventure map.</summary>
    [Binding]
    ButtonClick mapClick;

    /// <summary>Return from the map to the camp.</summary>
    [Binding]
    ButtonClick returnClick;

    /// <summary>Display the camp controls.</summary>
    [Binding(typeof(SyncVisible))]
    bool campVisible;

    /// <summary>Display the map controls.</summary>
    [Binding(typeof(SyncVisible))]
    bool mapVisible;
}
