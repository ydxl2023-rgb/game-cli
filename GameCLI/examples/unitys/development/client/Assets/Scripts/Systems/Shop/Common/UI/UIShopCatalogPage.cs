using Oathx.Widget;
using UnityEngine;

/// <summary>Refresh the gift or recharge catalog and release its bindings with the page.</summary>
public sealed class UIShopCatalogPage : UIShopPage
{
    /// <summary>Catalog widget supplied by the page prefab.</summary>
    [SerializeField]
    private IWidget catalog;

    /// <summary>Present the entity using the catalog widget's own product rules.</summary>
    public override bool RefreshPage(ShopEntity entity)
    {
        return catalog && catalog is IShopCatalog view && view.Invalidate(entity);
    }

    /// <summary>Detach the active catalog model when its page leaves the cache.</summary>
    public override void ReleasePage()
    {
        if (catalog && catalog.GetVM() is VM model)
            catalog.Unref(model);
    }
}

/// <summary>Product catalog widgets that can present a shop entity.</summary>
public interface IShopCatalog
{
    /// <summary>Refresh the widget with current shop data.</summary>
    bool Invalidate(ShopEntity entity);
}
