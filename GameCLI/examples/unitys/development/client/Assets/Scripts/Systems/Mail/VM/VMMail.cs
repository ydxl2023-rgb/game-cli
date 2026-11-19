using Oathx.Widget;

/// <summary>Presentation bindings for Mail.</summary>
public partial class VMMail : VM
{
    /// <summary>Bound closeClick presentation value.</summary>
    [Binding]
    ButtonClick closeClick;

    /// <summary>Bound claimClick presentation value.</summary>
    [Binding]
    ButtonClick claimClick;

    /// <summary>Bound deleteClick presentation value.</summary>
    [Binding]
    ButtonClick deleteClick;

    /// <summary>Bound claimAllClick presentation value.</summary>
    [Binding]
    ButtonClick claimAllClick;

    /// <summary>Bound deleteReadClick presentation value.</summary>
    [Binding]
    ButtonClick deleteReadClick;

    /// <summary>Bound messages presentation value.</summary>
    [Binding(typeof(SyncFancyGridScrollView))]
    VList<VMMailEntry> messages;

    /// <summary>Bound messageClick presentation value.</summary>
    [Binding(typeof(SyncFancyGridClick))]
    FancyGridClick messageClick;

    /// <summary>Bound messageTemplate presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool messageTemplate;

    /// <summary>Bound rewards presentation value.</summary>
    [Binding(typeof(SyncFancyGridScrollView))]
    VList<VMRewardEntry> rewards;

    /// <summary>Bound rewardTemplate presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool rewardTemplate;

    /// <summary>Bound title presentation value.</summary>
    [Binding(typeof(SyncText))]
    string title;

    /// <summary>Bound sentAt presentation value.</summary>
    [Binding(typeof(SyncText))]
    string sentAt;

    /// <summary>Bound body presentation value.</summary>
    [Binding(typeof(SyncText))]
    string body;

    /// <summary>Bound expiry presentation value.</summary>
    [Binding(typeof(SyncText))]
    string expiry;

    /// <summary>Bound total presentation value.</summary>
    [Binding(typeof(SyncText))]
    string total;

    /// <summary>Bound unread presentation value.</summary>
    [Binding(typeof(SyncText))]
    string unread;

    /// <summary>Bound detailVisible presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool detailVisible;

    /// <summary>Bound emptyVisible presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool emptyVisible;

    /// <summary>Bound claimVisible presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool claimVisible;

    /// <summary>Bound deleteVisible presentation value.</summary>
    [Binding(typeof(SyncVisible))]
    bool deleteVisible;

    /// <summary>Bound canClaimAll presentation value.</summary>
    [Binding(typeof(SyncInteractable))]
    bool canClaimAll;

    /// <summary>Bound canDeleteRead presentation value.</summary>
    [Binding(typeof(SyncInteractable))]
    bool canDeleteRead;

    /// <summary>Source envelope state selected by inbox availability.</summary>
    [Binding(typeof(SyncAnimator))]
    string envelopeState;
}
