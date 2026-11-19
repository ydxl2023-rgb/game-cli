using System;
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

/// <summary>Serialized visual variant and mount owned by an entry.</summary>
[System.Serializable]
public class ActivityBadgeSlot
{
    /// <summary>The decoration represented by this slot.</summary>
    [SerializeField]
    private ActivityBadge kind;

    /// <summary>Explicit placement preserving the authored layout.</summary>
    [SerializeField]
    private RectTransform anchor;

    /// <summary>Reusable visual and animation asset.</summary>
    [SerializeField]
    private GameObject prefab;

    /// <summary>The live child widget, absent when the slot is unused.</summary>
    private UIActivityBadge instance;

    /// <summary>Reconcile one slot without rebinding an existing widget.</summary>
    /// <param name="requested">Requested independent decorations.</param>
    /// <returns>True when the configured slot was reconciled.</returns>
    public bool Apply(ActivityBadge requested)
    {
        if ((requested & kind) == 0)
        {
            Release();

            return true;
        }

        if (!anchor || !prefab || !prefab.GetComponent<UIActivityBadge>() || (kind != ActivityBadge.New && kind != ActivityBadge.Advertisement))
            return false;

        anchor.gameObject.SetActive(true);

        if (!instance)
        {
            instance = UnityEngine.Object.Instantiate(prefab, anchor, false).GetComponent<UIActivityBadge>();
            if (!instance.Ref(new VMActivityBadge { Visible = true }))
            {
                Release();

                return false;
            }

            instance.gameObject.SetActive(true);
        }

        return true;
    }

    /// <summary>Unbind and remove the optional visual immediately from the layout.</summary>
    public void Release()
    {
        if (instance)
        {
            if (instance.vm)
                instance.Unref(instance.vm);

            instance.gameObject.SetActive(false);
            UnityEngine.Object.Destroy(instance.gameObject);
            instance = null;
        }
    }
}

/// <summary>Independent entry decorations that can be combined.</summary>
[Flags]
public enum ActivityBadge
{
    /// <summary>No attached decoration.</summary>
    None = 0,

    /// <summary>New content is available.</summary>
    New = 1,

    /// <summary>An advertisement entry is available.</summary>
    Advertisement = 2
}

/// <summary>Adapts VM decoration state to optional child-widget lifetimes.</summary>
[Binding(typeof(UIActivityEntry))]
public class SyncActivityBadges : ISyncAction
{
    /// <summary>Apply the current decoration flags to one entry.</summary>
    /// <param name="propertyName">Bound VM property name.</param>
    /// <param name="value">Decoration flags.</param>
    /// <param name="property">Compiled binding contract.</param>
    /// <param name="component">Serialized entry component.</param>
    public void Sync(string propertyName, object value, Property property, Component component)
    {
        if (component is UIActivityEntry entry && value is Boxed<ActivityBadge> badges)
        {
            if (!entry.ShowBadges(badges.value))
                throw new InvalidOperationException("The entry decoration references or flags are invalid.");
        }
        else
        {
            throw new InvalidOperationException("Activity badge binding requires an entry and boxed decoration flags.");
        }
    }

    /// <summary>Release child widgets when the VM binding ends.</summary>
    /// <param name="property">Compiled binding contract.</param>
    public void UnSync(Property property)
    {
        foreach (var component in property.components)
        {
            if (component is UIActivityEntry entry)
                entry.ReleaseBadges();
        }
    }
}
