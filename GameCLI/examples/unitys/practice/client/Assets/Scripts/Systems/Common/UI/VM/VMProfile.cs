using Oathx.Widget;

/// <summary>Complete host-owned player profile presentation.</summary>
public partial class VMProfile : VM
{
    /// <summary>Display name supplied by the host.</summary>
    [Binding(typeof(SyncText))]
    string playerName;

    /// <summary>Formatted level supplied by the host.</summary>
    [Binding(typeof(SyncText))]
    string playerLevel;

    /// <summary>Portrait asset path resolved by the Widget resource loader.</summary>
    [Binding(typeof(SyncSprite))]
    string portrait;

    /// <summary>Frame asset path resolved by the Widget resource loader.</summary>
    [Binding(typeof(SyncSprite))]
    string frame;

    /// <summary>Profile interaction and optional entry decorations.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry profileEntry;

    /// <summary>Avatar interaction and optional entry decorations.</summary>
    [Binding(typeof(SyncMetadata))]
    VMActivityEntry avatarEntry;

    /// <summary>Whether the portrait image is visible.</summary>
    [Binding(typeof(SyncVisible))]
    bool portraitVisible;

    /// <summary>Whether the frame image is visible.</summary>
    [Binding(typeof(SyncVisible))]
    bool frameVisible;
}
