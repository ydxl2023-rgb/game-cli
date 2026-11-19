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

    /// <summary>Play the original brief scale feedback on a real pointer click.</summary>
    /// <param name="eventData">Pointer input forwarded by the event system.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (entrance && eventData.button == PointerEventData.InputButton.Left)
            entrance.Play("Click", 0, 0f);
    }

    /// <summary>Release nested subscriptions when the product card is destroyed.</summary>
    private void OnDestroy()
    {
        if (vm)
            Unref(vm);
    }
}
