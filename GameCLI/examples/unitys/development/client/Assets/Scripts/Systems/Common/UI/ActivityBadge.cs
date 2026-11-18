using System;
using Oathx.Widget;
using UnityEngine;

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

/// <summary>Serialized visual variant and mount owned by an entry.</summary>
[Serializable]
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
