using Oathx.Widget;

/// <summary>Displays a configured product or reward milestone using the skin's Metadata.</summary>
public class UIShopProductCell : UIWidget<VMShopProductCell>
{
    /// <summary>Release the card bindings when its page is discarded.</summary>
    private void OnDestroy()
    {
        if (vm)
            Unref(vm);
    }
}
