using Oathx.Widget;
using UnityEngine;

/// <summary>Displays the ShopGroup view through serialized Metadata.</summary>
[RequireComponent(typeof(FancyGridScrollViewCell), typeof(Metadata))]
public class UIShopGroup : UIWidget<VMShopGroup>
{
    /// <summary>Native icon for both selected and unselected states.</summary>
    [SerializeField]
    private UnityEngine.UI.Image sectionIcon;

    /// <summary>Refresh the icon when the list recycles a section.</summary>
    public override bool Ref(VM value)
    {
        bool bound = base.Ref(value);
        if (bound && sectionIcon && value is VMShopGroup group)
            sectionIcon.sprite = group.icon;

        return bound;
    }
}
