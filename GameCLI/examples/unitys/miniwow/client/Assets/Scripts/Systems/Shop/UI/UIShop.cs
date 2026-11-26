using Oathx.Widget;

/// <summary>Requests returning from trading to Home.</summary>
public sealed class ReqShopClose : UIRequestEvent { }

/// <summary>Requests confirmation of an original offer.</summary>
public sealed class ReqShopSelect : UIRequestEvent
{
    /// <summary>Stable original offer ID.</summary>
    public int offerId
    { get; set; }
}

/// <summary>Requests a change to the pending purchase quantity.</summary>
public sealed class ReqShopQuantity : UIRequestEvent
{
    /// <summary>Quantity step, without changing inventory.</summary>
    public int delta
    { get; set; }
}

/// <summary>Requests committing the pending purchase.</summary>
public sealed class ReqShopConfirm : UIRequestEvent { }

/// <summary>Discards the pending purchase.</summary>
public sealed class ReqShopCancel : UIRequestEvent { }

/// <summary>Requests the completed task reward.</summary>
public sealed class ReqShopClaim : UIRequestEvent { }

/// <summary>Displays the first-tier trading catalog using native scrolling Widget cells.</summary>
[Config("Assets/Data/Widgets/Shop/UIShop.prefab", WidgetLayer.dialog, cacheWidget: false)]
public sealed class UIShop : UIWidget<VMShop>
{
    /// <summary>Updates catalog quantities and the task footer from committed entities.</summary>
    /// <param name="shop">Catalog and pending-selection rules.</param>
    /// <param name="inventory">Shared inventory.</param>
    /// <param name="task">Currently tracked task.</param>
    public void Invalidate(ShopEntity shop, HomeVillageEntity inventory, TaskEntity task)
    {
        if (vm.Offers == null)
        {
            var offers = new VList<VMShopOffer>();
            foreach (int id in shop.GetOffers())
            {
                var item = new VMShopOffer { offerId = id };
                RefreshOffer(item, inventory);
                offers.Add(item);
            }

            vm.Offers = offers;
        }
        else
        {
            foreach (VMShopOffer item in vm.Offers)
                RefreshOffer(item, inventory);
        }

        vm.Gold = string.Format(CommonText.Get(Config.Excel.TEXT_TRADE_GOLD),
            inventory.Count(inventory.configuration.GoldID));
        vm.TaskText = string.Format(CommonText.Get(Config.Excel.TEXT_TRADE_TASK),
            Config.Excel.propLanguageById(int.Parse(task.configuration.Description)));
        vm.ClaimVisible = task.status == TaskStatus.Completed;
    }

    /// <summary>Displays a successfully committed purchase.</summary>
    public void ShowPurchased()
    {
        vm.Feedback = CommonText.Get(Config.Excel.TEXT_TRADE_SUCCESS);
    }

    /// <summary>Binds page-level intentions once.</summary>
    /// <returns>Whether bindings were installed.</returns>
    protected override bool Bind()
    {
        vm.CloseClick = Send<ReqShopClose>;
        vm.ClaimClick = Send<ReqShopClaim>;

        return true;
    }

    /// <summary>Updates an existing card without recreating its binding.</summary>
    /// <param name="item">The existing card view model.</param>
    /// <param name="inventory">Committed inventory and usage.</param>
    private void RefreshOffer(VMShopOffer item, HomeVillageEntity inventory)
    {
        var offer = Config.Excel.shopGoodsConfig[item.offerId];
        item.Name = CommonText.Get(offer.NameTextID);
        item.Icon = offer.IconPath;
        item.Price = offer.Price.ToString();
        item.Stock = string.Format(CommonText.Get(Config.Excel.TEXT_TRADE_REMAINING),
            offer.Limit - inventory.Purchased(offer));
        item.Owned = string.Format(CommonText.Get(Config.Excel.TEXT_TRADE_OWNED), inventory.Count(offer.ItemID));
    }
}
