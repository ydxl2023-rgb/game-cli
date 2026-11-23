using Oathx.Widget;

/// <summary>Presentation bindings for MailEntry.</summary>
public partial class VMMailEntry : VM
{
    /// <summary>Stable identifier used when the displayed snapshot is clicked.</summary>
    public int mailId
    { get; set; }

    /// <summary>Bound title presentation value.</summary>
    [Binding(typeof(SyncText))]
    string title;

    /// <summary>Bound sentAt presentation value.</summary>
    [Binding(typeof(SyncText))]
    string sentAt;

    /// <summary>Bound unselected presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool unselected;

    /// <summary>Bound received presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool received;

    /// <summary>Bound selected presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool selected;

    /// <summary>Bound unread presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool unread;

    /// <summary>Bound hasAttachment presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool hasAttachment;

    /// <summary>Bound claimed presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool claimed;

    /// <summary>Bound pending presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool pending;
}
