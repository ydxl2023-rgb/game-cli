using Oathx.Widget;
using UnityEngine;

/// <summary>Reusable function entry driven only by its VM.</summary>
[RequireComponent(typeof(Metadata))]
public class UIActivityEntry : UIWidget<VMActivityEntry>
{
    /// <summary>Stable navigation or function configuration identifier.</summary>
    [SerializeField]
    private int id;

    /// <summary>Optional visual variants with explicit mount references.</summary>
    [SerializeField]
    private ActivityBadgeSlot[] badgeSlots;

    /// <summary>Requested decorations retained while the entry is hidden.</summary>
    private ActivityBadge requested;

    /// <summary>Whether this entry owns an active binding.</summary>
    private bool bound;

    /// <summary>Bind one entry without accumulating duplicate listeners.</summary>
    /// <param name="refVM">The entry data.</param>
    /// <returns>True when the entry was bound.</returns>
    public override bool Ref(VM refVM)
    {
        if (bound || vm || !(refVM is VMActivityEntry))
            return false;

        MetadataSyncFactory.GetSingleton().Register(nameof(SyncActivityBadges), new SyncActivityBadges());

        bound = base.Ref(refVM);

        return bound;
    }

    /// <summary>Release decorations before the base binding is removed.</summary>
    /// <param name="refVM">The bound entry data.</param>
    public override void Unref(VM refVM)
    {
        if (!bound)
            return;

        requested = ActivityBadge.None;
        ReleaseBadges();
        base.Unref(refVM);

        bound = false;
    }

    /// <summary>Read the authored navigation identifier.</summary>
    /// <returns>The associated entry identifier.</returns>
    public int GetSystemId()
    {
        return id;
    }

    /// <summary>Apply independent decoration flags without business lookups.</summary>
    /// <param name="value">Requested decoration types.</param>
    /// <returns>True when each configured slot was updated.</returns>
    public bool ShowBadges(ActivityBadge value)
    {
        if ((value & ~(ActivityBadge.New | ActivityBadge.Advertisement)) != 0)
            return false;

        requested = value;
        if (!isActiveAndEnabled)
            return true;

        if (badgeSlots == null)
            return false;

        bool complete = true;
        foreach (var slot in badgeSlots)
            complete &= slot != null && slot.Apply(value);

        return complete;
    }

    /// <summary>Release all live decorations while retaining requested state.</summary>
    public void ReleaseBadges()
    {
        if (badgeSlots != null)
        {
            foreach (var slot in badgeSlots)
                slot?.Release();
        }
    }

    /// <summary>Release nested metadata bindings when the page is destroyed.</summary>
    private void OnDestroy()
    {
        if (bound)
            Unref(vm);
    }

    /// <summary>Restore decorations when an entry becomes visible again.</summary>
    private void OnEnable()
    {
        if (badgeSlots != null)
            ShowBadges(requested);
    }

    /// <summary>Release presentation when the entry or its page is hidden.</summary>
    private void OnDisable()
    {
        ReleaseBadges();
    }
}

