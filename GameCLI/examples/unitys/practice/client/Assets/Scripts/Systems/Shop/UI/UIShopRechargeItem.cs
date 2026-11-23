using Oathx.Widget;
using UnityEngine;

/// <summary>Displays the ShopRechargeItem view through serialized Metadata.</summary>
[RequireComponent(typeof(FancyGridScrollViewCell), typeof(Metadata))]
public class UIShopRechargeItem : UIWidget<VMShopRechargeItem>
{
    /// <summary>Replays the source entrance when the list recycles this cell.</summary>
    [SerializeField]
    private Animator entrance;

    /// <summary>Bind the next product and restart its entrance.</summary>
    /// <param name="value">The new list item presentation.</param>
    /// <returns>True when Metadata binding succeeded.</returns>
    public override bool Ref(VM value)
    {
        bool bound = base.Ref(value);
        if (bound && entrance)
            entrance.Play("Open", 0, 0f);

        return bound;
    }

    /// <summary>Release the child slot while preserving the recycled product model.</summary>
    /// <param name="refVM">Previously bound product presentation.</param>
    public override void Unref(VM refVM)
    {
        var current = vm;
        var item = current ? current.Item : null;
        if (current && item)
            current.Item = null;

        base.Unref(refVM);

        // Restore data after unsubscribing so the same product can be bound again.
        if (current && item)
            current.Item = item;
    }

    /// <summary>Release nested subscriptions when the product card is destroyed.</summary>
    private void OnDestroy()
    {
        if (vm)
            Unref(vm);
    }
}
