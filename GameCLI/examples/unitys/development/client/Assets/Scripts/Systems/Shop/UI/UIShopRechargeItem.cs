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
}
