using Oathx.Widget;
using UnityEngine;

/// <summary>Presentation bindings for the basic lobby.</summary>
public partial class VMHome : VM
{
    /// <summary>Authored activity widgets in stable hierarchy order.</summary>
    [Binding(typeof(SyncGroup))]
    VList<VMActivityEntry> acitiveList;

    /// <summary>Complete reusable player profile presentation.</summary>
    [Binding(typeof(SyncMetadata))]
    VMProfile profile;

    /// <summary>Reusable group of five profile shortcuts.</summary>
    [Binding(typeof(SyncMetadata))]
    VMProfileShortcuts profileShortcuts;

    /// <summary>Reusable currency presentation owned by this page.</summary>
    [Binding(typeof(SyncMetadata))]
    VMCurrencyBar currencyBar;

    /// <summary>Request the Shop entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry shopEntry;

    /// <summary>Request the Benefits entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry benefitsEntry;

    /// <summary>Request the Rankings entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry rankingsEntry;

    /// <summary>Request the Chat entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry chatEntry;

    /// <summary>Request the ChatChannel entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry chatChannelEntry;

    /// <summary>Display ChatMessage.</summary>
    [Binding(typeof(SyncText))]
    string chatMessage;

    /// <summary>Request the Backpack entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry backpackEntry;

    /// <summary>Request the Collection entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry collectionEntry;

    /// <summary>Request the Honor entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry honorEntry;

    /// <summary>Request the Story entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry storyEntry;

    /// <summary>Request the Matches entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry matchesEntry;

    /// <summary>Request the Training entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry trainingEntry;

    /// <summary>Request the Recruit entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry recruitEntry;

    /// <summary>Request the Players entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry playersEntry;

    /// <summary>Request the Team entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry teamEntry;

    /// <summary>Request the Club entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry clubEntry;

    /// <summary>Request the Activities entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry activitiesEntry;

    /// <summary>Authored artwork for the current drop promotion.</summary>
    [Binding(typeof(SyncSprite))]
    Sprite dropSprite;

    /// <summary>Native promotion dimensions in canvas units.</summary>
    [Binding(typeof(SyncSizeDelta))]
    Vector2 dropSize;

    /// <summary>Whether a drop promotion is active.</summary>
    [Binding(typeof(SyncVisible))]
    bool dropVisible;
}
