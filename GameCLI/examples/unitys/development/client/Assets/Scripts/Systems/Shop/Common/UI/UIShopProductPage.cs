using UnityEngine;

/// <summary>Present the configured product track selected by the current shop tab.</summary>
public sealed class UIShopProductPage : UIShopPage
{
    /// <summary>Product list supplied by this page layout.</summary>
    [SerializeField]
    private UIShopProductList products;

    /// <summary>Display the configured offers for this route.</summary>
    public override bool RefreshPage(ShopEntity entity)
    {
        return products && products.Invalidate(entity, id);
    }
}
