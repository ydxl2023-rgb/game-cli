using Config;
using Oathx.Widget;

/// <summary>Displays the ShopRecharge view through serialized Metadata.</summary>
[Config("Assets/Data/Widgets/Shop/ShopRecharge/ShopRecharge.prefab", WidgetLayer.topmost, cacheWidget: false)]
public class UIShopRecharge : UIWidget<VMShopRecharge>
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
        if (!entity || !vm)
            return false;

        var items = new VList<VMShopRechargeItem>();
        foreach (var product in entity.GetProducts())
            items.Add(new VMShopRechargeItem {
                productId = product.id,
                Title = UIShop.ProductTitle(product),
                Price = CommonText.Get(Excel.SHOP_PRICE, UIShop.Money(product.price)),
                Item = new VMItemSlot {
                    Icon = product.icon,
                    IconVisible = true,
                    Text = string.Empty
                },
                Bonus = string.Empty,
                FirstPay = false,
                FirstPayText = CommonText.Get(Excel.SHOP_FIRST_PAY),
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
