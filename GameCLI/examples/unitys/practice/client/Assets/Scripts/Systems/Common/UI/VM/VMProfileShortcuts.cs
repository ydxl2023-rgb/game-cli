using Oathx.Widget;

/// <summary>Host-owned presentation for the reusable shortcut group.</summary>
public partial class VMProfileShortcuts : VM
{
    /// <summary>Presentation and interaction for the mail entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry mailEntry;

    /// <summary>Presentation and interaction for the settings entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry settingsEntry;

    /// <summary>Presentation and interaction for the notice entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry noticeEntry;

    /// <summary>Presentation and interaction for the friends entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry friendsEntry;

    /// <summary>Presentation and interaction for the affinity entry.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry affinityEntry;
}
