using Oathx.Widget;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Displays the ShopGiftItem view through serialized Metadata.</summary>
[RequireComponent(typeof(FancyGridScrollViewCell), typeof(Metadata))]
public class UIShopGiftItem : UIWidget<VMShopGiftItem>, IPointerClickHandler
{
    /// <summary>Owns the source entrance and click feedback states.</summary>
    [SerializeField]
    private Animator entrance;

    /// <summary>Replay entrance when a recycled cell receives new content.</summary>
    /// <param name="value">The new list item presentation.</param>
    /// <returns>True when Metadata binding succeeded.</returns>
    public override bool Ref(VM value)
    {
        bool bound = base.Ref(value);
        if (bound && entrance)
            entrance.Play("Open", 0, 0f);

        return bound;
    }

    /// <summary>Play the original brief scale feedback on a real pointer click.</summary>
    /// <param name="eventData">Pointer input forwarded by the event system.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (entrance && eventData.button == PointerEventData.InputButton.Left)
            entrance.Play("Click", 0, 0f);
    }
}
