using Oathx.Widget;

/// <summary>Reward-specific presentation around a reusable item slot.</summary>
public partial class VMRewardEntry : VM
{
    /// <summary>Reusable item presentation owned by the host.</summary>
    [Binding(typeof(SyncMetadata))]
    VMItemSlot item;

    /// <summary>Whether the host has recorded a claim.</summary>
    [Binding(typeof(SyncVisible))]
    bool claimed;
}
