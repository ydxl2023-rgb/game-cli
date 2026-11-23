using Config;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;

/// <summary>Coordinates the local mall reference without payment or network APIs.</summary>
public class ShopSystem : RuntimeSystem
{
    /// <summary>Open the mall and initialize its local demonstration catalog once.</summary>
    /// <returns>True when the page was requested.</returns>
    public override bool OnActive()
    {
        if (!IsActive() || !InitializeDemo())
            return false;

        UnsubscribeAll();

        Subscribe<ReqShopClose>(data => Close());
        Subscribe<ReqShopShowHelp>(data => ShowMessage(Excel.SHOP_HELP));
        Subscribe<ReqShopShiftSection>(data => MoveSection(data.offset));
        Subscribe<ReqShopSelectSection>(data => OpenPage(data.index, 0));
        Subscribe<ReqShopSelectTab>(data => SelectTab(data.index));
        Subscribe<ReqShopSelectProduct>(data => OpenProduct(data.productId));
        Subscribe<ReqShopCancelPurchase>(data => CancelPurchase());
        Subscribe<ReqShopConfirmPurchase>(data => ConfirmPurchase());

        UISystem.GetSingleton().OpenWidget<UIShop>((widget) => {
            if (widget)
            {
                OpenPage(0, 0);
                ShowMessage(Excel.SHOP_DEMO_NOTICE);
            }
        });

        return true;
    }

    /// <summary>Cancel the local selection and release all owned views.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        if (entity)
            entity.CancelPurchase();

        System.Type[] widgets = new System.Type[] {
            typeof(UIShopPurchase),
            typeof(UIShopGift),
            typeof(UIShopRecharge),
            typeof(UIShop)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
    }

    /// <summary>Refresh the mall and any currently open product list.</summary>
    /// <returns>True when the mall was available.</returns>
    public bool Refresh()
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UIShop>(out var widget))
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        if (!entity)
            return false;

        if (!widget.Invalidate(entity))
            return false;
        RefreshProducts(entity);

        return true;
    }

    /// <summary>Open a configured source section and secondary tab.</summary>
    public bool OpenPage(int sectionIndex, int pageIndex)
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UIShop>(out var widget))
            return false;

        var section = widget.GetSection(sectionIndex);
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        if (!entity || section == null || pageIndex < 0 || pageIndex >= section.pages.Length)
            return false;

        var page = section.pages[pageIndex];
        entity.SetPage(sectionIndex, pageIndex);
        widget.DisplayPage(page.prefab, section.pages.Length > 1, page.id);

        if (page.window == "RechargeShopView")
            return SelectCategory(ShopCategory.Recharge);

        if (page.window == "NormalGiftView")
            return SelectCategory(ShopCategory.Gift);

        System.Type[] widgets = new System.Type[] {
            typeof(UIShopPurchase),
            typeof(UIShopGift),
            typeof(UIShopRecharge)
        };
        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);

        return Refresh();
    }

    /// <summary>Switch between the supported local recharge and gift catalogs.</summary>
    /// <param name="category">Requested category.</param>
    /// <returns>True when the child page was requested.</returns>
    public bool SelectCategory(ShopCategory category)
    {
        if (!IsActive())
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        if (!entity)
            return false;

        if (!entity.SelectCategory(category))
            return false;

        System.Type[] widgets = new System.Type[] {
            typeof(UIShopPurchase),
            typeof(UIShopGift),
            typeof(UIShopRecharge)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
        Refresh();

        if (category == ShopCategory.Recharge)
        {
            UISystem.GetSingleton().OpenWidget<UIShopRecharge>((widget) => {
                if (widget)
                {
                    Refresh();
                }
            });
        }
        else
        {
            UISystem.GetSingleton().OpenWidget<UIShopGift>((widget) => {
                if (widget)
                {
                    Refresh();
                }
            });
        }

        return true;
    }

    /// <summary>Open the local confirmation for one product in the visible list.</summary>
    /// <param name="index">Index within the current category.</param>
    /// <returns>True when a valid product confirmation was requested.</returns>
    public bool SelectProduct(int index)
    {
        if (!IsActive())
            return false;

        if (!UISystem.GetSingleton().TryGetWidget<UIShop>(out var main) || !main.CanInteract())
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        if (!entity)
            return false;

        var products = entity.GetProducts();
        return index >= 0 && index < products.Count && OpenProduct(products[index].id);
    }

    /// <summary>Confirm the pending local transaction once and refresh the visible catalog.</summary>
    /// <returns>True only when the simulated transaction succeeded.</returns>
    public bool ConfirmPurchase()
    {
        if (!IsActive())
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        if (!entity)
            return false;

        long before = entity.demoTokens;
        var result = entity.ConfirmPurchase();

        CancelPurchase();
        Refresh();
        ShowMessage(result == ShopPurchaseResult.Succeeded ? Excel.SHOP_SUCCESS
            : result == ShopPurchaseResult.InsufficientBalance ? Excel.SHOP_INSUFFICIENT
            : result == ShopPurchaseResult.SoldOut ? Excel.SHOP_SOLD_OUT
            : result == ShopPurchaseResult.CapacityExceeded ? Excel.SHOP_CAPACITY : Excel.SHOP_NO_SELECTION,
            entity.demoTokens - before);

        return result == ShopPurchaseResult.Succeeded;
    }

    /// <summary>Close confirmation without applying a transaction.</summary>
    /// <returns>True when pending selection was cancelled.</returns>
    public bool CancelPurchase()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        if (!entity)
            return false;

        entity.CancelPurchase();

        System.Type[] widgets = new System.Type[] {
            typeof(UIShopPurchase)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);

        return true;
    }

    /// <summary>Ask the owning coordinator to deactivate this mall.</summary>
    /// <returns>True when a local deactivation event was emitted.</returns>
    public bool Close()
    {
        if (!IsActive())
            return false;

        return XPluginUtility.PostEvent(new SystemDetive { type = typeof(ShopSystem) });
    }

    /// <summary>Navigate to the adjacent primary section.</summary>
    private bool MoveSection(int offset)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();

        return entity && OpenPage(entity.sectionIndex + offset, 0);
    }

    /// <summary>Navigate within the current primary section.</summary>
    private bool SelectTab(int index)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();

        return entity && OpenPage(entity.sectionIndex, index);
    }

    /// <summary>Open the confirmation for the selected catalog identifier.</summary>
    private bool OpenProduct(int productId)
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UIShop>(out var main) || !main.CanInteract())
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        if (!entity || !entity.SelectProduct(productId))
            return false;

        UISystem.GetSingleton().OpenWidget<UIShopPurchase>((widget) => {
            if (widget)
            {
                widget.Invalidate(entity);
            }
        });

        return true;
    }

    /// <summary>Install explicit sample data only when no product catalog exists.</summary>
    /// <returns>True when a catalog is available.</returns>
    private bool InitializeDemo()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        if (!entity)
            return false;

        if (entity.productCount > 0)
            return true;

        return entity.InitializeDemo(ShopDemoCatalog.Create(), 100000);
    }

    /// <summary>Refresh the active catalog page from the mall entity.</summary>
    /// <param name="entity">Validated local mall entity.</param>
    private void RefreshProducts(ShopEntity entity)
    {
        if (entity.category == ShopCategory.Recharge && UISystem.GetSingleton().TryGetWidget<UIShopRecharge>(out var recharge))
            recharge.Invalidate(entity);

        if (entity.category == ShopCategory.Gift && UISystem.GetSingleton().TryGetWidget<UIShopGift>(out var gift))
            gift.Invalidate(entity);
    }

    /// <summary>Display a localized local-operation message.</summary>
    /// <param name="textId">CommonText identifier.</param>
    /// <param name="args">Localized format arguments.</param>
    /// <returns>True when the current mall message was changed.</returns>
    private bool ShowMessage(int textId, params object[] args)
    {
        if (!UISystem.GetSingleton().TryGetWidget<UIShop>(out var widget))
            return false;

        return widget.ShowMessage(textId, args);
    }
}
