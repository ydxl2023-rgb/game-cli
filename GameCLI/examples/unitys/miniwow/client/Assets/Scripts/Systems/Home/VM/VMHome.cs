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

    /// <summary>Entry widgets in the exact authored prefab order.</summary>
    [Binding(typeof(SyncGroup))]
    VList<VMHomeEntry> entries;

    /// <summary>Local player name.</summary>
    [Binding(typeof(SyncText))]
    string playerName;

    /// <summary>Settlement title.</summary>
    [Binding(typeof(SyncText))]
    string guildTitle;

    /// <summary>Formatted combat power.</summary>
    [Binding(typeof(SyncText))]
    string power;

    /// <summary>Formatted gold balance.</summary>
    [Binding(typeof(SyncText))]
    string gold;

    /// <summary>Gem balance.</summary>
    [Binding(typeof(SyncText))]
    string gem;

    /// <summary>Energy and capacity.</summary>
    [Binding(typeof(SyncText))]
    string energy;

    /// <summary>Local task progress.</summary>
    [Binding(typeof(SyncText))]
    string taskProgress;

    /// <summary>Local village message.</summary>
    [Binding(typeof(SyncText))]
    string chat;

    /// <summary>Shows explicit destination feedback.</summary>
    [Binding(typeof(SyncVisible))]
    bool feedbackVisible;

    /// <summary>Feedback for unavailable or locked destinations.</summary>
    [Binding(typeof(SyncText))]
    string feedbackText;

    /// <summary>Dismisses destination feedback.</summary>
    [Binding]
    ButtonClick closeFeedbackClick;

    /// <summary>Requests settings.</summary>
    [Binding]
    ButtonClick settingsClick;

    /// <summary>Requests task details.</summary>
    [Binding]
    ButtonClick taskClick;

    /// <summary>Requests village chat.</summary>
    [Binding]
    ButtonClick chatClick;
}
