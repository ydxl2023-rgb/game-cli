using Config;
using Oathx.Widget;

/// <summary>Displays the ShopPurchase view through serialized Metadata.</summary>
[Config("Assets/Data/Widgets/Shop/ShopPurchase/ShopPurchase.prefab", WidgetLayer.topmost, cacheWidget: false)]
public class UIShopPurchase : UIWidget<VMShopPurchase>
{
    /// <summary>Bind player intents when the widget receives its view model.</summary>
    protected override bool Bind()
    {
        vm.CloseClick = Send<ReqShopCancelPurchase>;
        vm.ConfirmClick = Send<ReqShopConfirmPurchase>;

        return true;
    }

    /// <summary>Render the current mall data.</summary>
    public bool Invalidate(ShopEntity entity)
    {
        if (!entity || !vm)
            return false;

        var product = entity.GetProducts().Find(item => item.id == entity.selectedProduct);
        if (product == null)
            return false;

        vm.ConfirmText = CommonText.Get(Excel.SHOP_CONFIRM);
        vm.Price = CommonText.Get(Excel.SHOP_PRICE, UIShop.Money(product.price));
        vm.Title = UIShop.ProductTitle(product);
        vm.Icon = product.icon;
        vm.RewardTitle = CommonText.Get(Excel.SHOP_REWARDS);
        vm.RewardName = CommonText.Get(Excel.SHOP_TOKEN_NAME);
        vm.RewardCount = CommonText.Get(Excel.SHOP_COUNT, product.reward);
        vm.RewardIcon = ShopDemoCatalog.tokenIcon;
        vm.AlternatePurchase = false;
        vm.FreeText = false;
        vm.RandomTitle = false;
        vm.RandomRewards = false;
        vm.Discount = false;
        vm.Labels = false;

        return true;
    }
}
