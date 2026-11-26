using Oathx.Widget;
using UnityEngine.UI;

/// <summary>Shows quantity, full price and explicit confirmation before a purchase.</summary>
[Config("Assets/Data/Widgets/Shop/UIShopPurchase.prefab", WidgetLayer.topmost, cacheWidget: false)]
public sealed class UIShopPurchase : UIWidget<VMShopPurchase>
{
    /// <summary>Displays the selected offer and current inventory.</summary>
    /// <param name="shop">Pending purchase data.</param>
    /// <param name="inventory">Committed inventory.</param>
    public void Invalidate(ShopEntity shop, HomeVillageEntity inventory)
    {
        var offer = Config.Excel.shopGoodsConfig[shop.selectedId];
        vm.Name = CommonText.Get(offer.NameTextID);
        vm.Description = CommonText.Get(offer.DescriptionTextID);
        vm.Icon = offer.IconPath;
        vm.Quantity = shop.quantity.ToString();
        vm.Total = string.Format(CommonText.Get(Config.Excel.TEXT_TRADE_TOTAL), (long)offer.Price * shop.quantity);
        vm.Stock = string.Format(CommonText.Get(Config.Excel.TEXT_TRADE_REMAINING),
            offer.Limit - inventory.Purchased(offer));
        vm.Owned = string.Format(CommonText.Get(Config.Excel.TEXT_TRADE_OWNED), inventory.Count(offer.ItemID));
    }

    /// <summary>Explains an actual rejected transaction.</summary>
    /// <param name="result">Business validation result.</param>
    public void ShowResult(ShopPurchaseResult result)
    {
        int text;
        switch (result)
        {
            case ShopPurchaseResult.Locked: text = Config.Excel.TEXT_TRADE_LOCKED; break;
            case ShopPurchaseResult.NoGold: text = Config.Excel.TEXT_TRADE_NO_GOLD; break;
            case ShopPurchaseResult.SoldOut: text = Config.Excel.TEXT_TRADE_SOLD_OUT; break;
            case ShopPurchaseResult.NoSelection: text = Config.Excel.TEXT_TRADE_NO_SELECTION; break;
            default: text = Config.Excel.TEXT_TRADE_INVALID_QUANTITY; break;
        }

        vm.Feedback = CommonText.Get(text);
    }

    /// <summary>Binds confirmation and quantity intentions.</summary>
    /// <returns>Whether the popup is ready.</returns>
    protected override bool Bind()
    {
        vm.MinusClick = OnMinusClick;
        vm.PlusClick = OnPlusClick;
        vm.ConfirmClick = Send<ReqShopConfirm>;
        vm.CancelClick = Send<ReqShopCancel>;

        return true;
    }

    /// <summary>Requests one fewer pack.</summary>
    /// <param name="button">The clicked decrement control.</param>
    /// <returns>Whether the intention was sent.</returns>
    private bool OnMinusClick(Button button)
    {
        return Send(new ReqShopQuantity { delta = -1 });
    }

    /// <summary>Requests one additional pack.</summary>
    /// <param name="button">The clicked increment control.</param>
    /// <returns>Whether the intention was sent.</returns>
    private bool OnPlusClick(Button button)
    {
        return Send(new ReqShopQuantity { delta = 1 });
    }
}
