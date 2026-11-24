using Config;
using Oathx.Widget;

/// <summary>Displays the ShopGift view through serialized Metadata.</summary>
[Config("Assets/Data/Widgets/Shop/ShopGift/ShopGift.prefab", WidgetLayer.topmost, cacheWidget: false)]
public class UIShopGift : UIWidget<VMShopGift>, IShopCatalog
{
    /// <summary>Bind player intents when the widget receives its view model.</summary>
    protected override bool Bind()
    {
        vm.ProductTemplate = false;
        vm.ProductClick = (sender, index) => RequestProduct(index);

        return true;
    }

    /// <summary>Render the current mall data.</summary>
    public bool Invalidate(ShopEntity entity)
    {
        if (!entity || (!vm && !Ref(new VMShopGift())))
            return false;

        if (entity.category != ShopCategory.Gift && !entity.SelectCategory(ShopCategory.Gift))
            return false;

        var items = new VList<VMShopGiftItem>();
        foreach (var product in entity.GetProducts())
            items.Add(new VMShopGiftItem {
                productId = product.id,
                Title = UIShop.ProductTitle(product),
                Price = CommonText.Get(Excel.SHOP_PRICE, UIShop.Money(product.price)),
                Item = new VMItemSlot {
                    Icon = product.icon,
                    IconVisible = true,
                    Text = CommonText.Get(Excel.SHOP_COUNT, product.reward)
                },
                Limit = CommonText.Get(Excel.SHOP_LIMIT, product.remaining),
                SoldOut = product.remaining == 0,
                SoldOutText = CommonText.Get(Excel.SHOP_SOLD_OUT),
            });

        vm.Products = items;

        return true;
    }

    /// <summary>Send the identifier of the displayed product.</summary>
    private bool RequestProduct(int index)
    {
        if (vm.Products == null || index < 0 || index >= vm.Products.Count)
            return false;

        return Send(new ReqShopSelectProduct { productId = vm.Products[index].productId });
    }
}
