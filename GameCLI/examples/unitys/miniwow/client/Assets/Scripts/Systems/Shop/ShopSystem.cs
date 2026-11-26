using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;

/// <summary>Requests opening the trading page after system activation.</summary>
public sealed class ShopOpenRequested : IEvent { }

/// <summary>Coordinates trading UI and atomic inventory changes through events.</summary>
public sealed class ShopSystem : RuntimeSystem
{
    /// <summary>Installs only this system's event subscriptions.</summary>
    /// <returns>Whether subscriptions were installed.</returns>
    public override bool OnActive()
    {
        Subscribe<ShopOpenRequested>(OnOpen);
        Subscribe<ReqShopClose>(OnClose);
        Subscribe<ReqShopSelect>(OnSelect);
        Subscribe<ReqShopQuantity>(OnQuantity);
        Subscribe<ReqShopConfirm>(OnConfirm);
        Subscribe<ReqShopCancel>(OnCancel);
        Subscribe<ReqShopClaim>(OnClaim);
        Subscribe<HomeVillageChanged>(OnVillageChanged);
        Subscribe<TaskChanged>(OnTaskChanged);
        Subscribe<HomeSessionClosing>(OnSessionClosing);

        return true;
    }

    /// <summary>Cancels selection and releases all owned UI and scene input ownership.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();
        CloseShop();
    }

    /// <summary>Opens trading only after the original prerequisite task is claimed.</summary>
    /// <param name="message">The accepted activation intention.</param>
    private void OnOpen(ShopOpenRequested message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        if (!entity.IsAvailable(tasks))
        {
            var ui = UISystem.GetSingleton();
            if (ui.TryGetWidget<UIHome>(out var home))
                home.ShowFeedback(Config.Excel.TEXT_TRADE_LOCKED);

            return;
        }

        entity.Cancel();
        XPluginUtility.FireEvent(new HomeFacilityPresentationChanged { systemId = 10005, visible = true });
        UISystem.GetSingleton().OpenWidget<UIShop>(OnShopOpened);
    }

    /// <summary>Projects live state after asynchronous UI creation.</summary>
    /// <param name="widget">The successfully opened page.</param>
    private void OnShopOpened(UIShop widget)
    {
        if (widget)
            RefreshShop();
        else
            XPluginUtility.FireEvent(new HomeFacilityPresentationChanged { systemId = 10005, visible = false });
    }

    /// <summary>Closes the trading page.</summary>
    /// <param name="message">The back intention.</param>
    private void OnClose(ReqShopClose message)
    {
        CloseShop();
    }

    /// <summary>Releases the page before Home's scene and shared inventory disappear.</summary>
    /// <param name="message">The closing session.</param>
    private void OnSessionClosing(HomeSessionClosing message)
    {
        CloseShop();
    }

    /// <summary>Selects an offer and opens the quantity confirmation.</summary>
    /// <param name="message">The original offer identity.</param>
    private void OnSelect(ReqShopSelect message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        entity.Select(message.offerId);
        UISystem.GetSingleton().OpenWidget<UIShopPurchase>(OnPurchaseOpened);
    }

    /// <summary>Initializes the confirmation popup.</summary>
    /// <param name="widget">The opened confirmation.</param>
    private void OnPurchaseOpened(UIShopPurchase widget)
    {
        if (widget)
            RefreshShop();
    }

    /// <summary>Changes only the pending quantity.</summary>
    /// <param name="message">The requested quantity step.</param>
    private void OnQuantity(ReqShopQuantity message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        entity.ChangeQuantity(message.delta, home.village);
        RefreshShop();
    }

    /// <summary>Commits the selected purchase and reports actual progress.</summary>
    /// <param name="message">The explicit purchase confirmation.</param>
    private void OnConfirm(ReqShopConfirm message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var result = entity.Confirm(home.village, tasks);
        var ui = UISystem.GetSingleton();
        if (result == ShopPurchaseResult.Success)
        {
            ClosePurchase();
            XPluginUtility.FireEvent(new HomeVillageChanged());
            if (ui.TryGetWidget<UIShop>(out var shop))
                shop.ShowPurchased();
        }
        else if (ui.TryGetWidget<UIShopPurchase>(out var purchase))
            purchase.ShowResult(result);
    }

    /// <summary>Cancels confirmation without modifying the wallet.</summary>
    /// <param name="message">The cancel intention.</param>
    private void OnCancel(ReqShopCancel message)
    {
        ClosePurchase();
    }

    /// <summary>Claims a completed task without executing another purchase.</summary>
    /// <param name="message">The reward intention.</param>
    private void OnClaim(ReqShopClaim message)
    {
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var task = tasks.GetTrackedTask();
        if (task.status == TaskStatus.Completed)
            XPluginUtility.FireEvent(new TaskClaimRequested { taskId = task.configuration.ID });
    }

    /// <summary>Refreshes the catalog after inventory changes.</summary>
    /// <param name="message">Committed inventory.</param>
    private void OnVillageChanged(HomeVillageChanged message)
    {
        RefreshShop();
    }

    /// <summary>Refreshes the task footer after progress or rewards.</summary>
    /// <param name="message">Committed task state.</param>
    private void OnTaskChanged(TaskChanged message)
    {
        RefreshShop();
    }

    /// <summary>Updates pages from entities without accessing their view models.</summary>
    private void RefreshShop()
    {
        var ui = UISystem.GetSingleton();
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        if (ui.TryGetWidget<UIShop>(out var shop))
            shop.Invalidate(entity, home.village, tasks.GetTrackedTask());

        if (ui.TryGetWidget<UIShopPurchase>(out var purchase) && entity.selectedId != 0)
            purchase.Invalidate(entity, home.village);
    }

    /// <summary>Releases the pending selection and its popup.</summary>
    private void ClosePurchase()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        entity.Cancel();
        System.Type[] widgets = new System.Type[] { typeof(UIShopPurchase) };
        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
    }

    /// <summary>Releases the complete trading presentation.</summary>
    private void CloseShop()
    {
        ClosePurchase();
        System.Type[] widgets = new System.Type[] { typeof(UIShop) };
        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
        XPluginUtility.FireEvent(new HomeFacilityPresentationChanged { systemId = 10005, visible = false });
    }
}
