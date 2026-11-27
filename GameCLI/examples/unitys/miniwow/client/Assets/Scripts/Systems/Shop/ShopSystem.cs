using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;

/// <summary>Requests opening the trading page after system activation.</summary>
public sealed class ShopOpenRequested : IEvent
{
    /// <summary>Optional task presentation owning this page request; absent for manual entrance clicks.</summary>
    public TaskPresentationAction guide
    { get; set; }
}

/// <summary>Coordinates trading UI and atomic inventory changes through events.</summary>
public sealed class ShopSystem : RuntimeSystem
{
    /// <summary>Current opening request retained only until the framework completes its page load.</summary>
    private ShopOpenRequested openingRequest;

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
        Subscribe<TaskPresentationCancelled>(OnGuideCancelled);

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
        OpenShop(message);
    }

    /// <summary>Opens only an active trading system with its configured prerequisite satisfied.</summary>
    /// <param name="request">Manual or guide-owned page intention.</param>
    /// <returns>Whether the page opening was requested.</returns>
    private bool OpenShop(ShopOpenRequested request)
    {
        if (!IsActive())
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        if (!entity.IsAvailable(tasks))
        {
            var ui = UISystem.GetSingleton();
            if (ui.TryGetWidget<UIHome>(out var home))
                home.ShowFeedback(Config.Excel.TEXT_TRADE_LOCKED);

            XPluginUtility.FireEvent(new HomeFacilityOpened {
                action = request.guide,
                buildingId = 60019,
                succeeded = false
            });

            return false;
        }

        ReportPurchaseProgress();
        if (request.guide != null && request.guide.status != TaskActionStatus.Running)
            return false;

        if (UISystem.GetSingleton().TryGetWidget<UIShop>(out _))
        {
            RefreshShop();
            XPluginUtility.FireEvent(new HomeFacilityOpened {
                action = request.guide,
                buildingId = 60019,
                succeeded = true
            });

            return true;
        }

        if (openingRequest != null)
        {
            if (ReferenceEquals(openingRequest.guide, request.guide))
                return true;

            var previous = openingRequest.guide;
            CloseShop();
            if (previous != null)
                previous.Cancel();
        }

        entity.Cancel();
        openingRequest = request;
        XPluginUtility.FireEvent(new HomeFacilityPresentationChanged { systemId = 10005, visible = true });
        UISystem.GetSingleton().OpenWidget<UIShop>(OnLoaded);

        return true;

        /// <summary>Retains the exact request through asynchronous resource completion.</summary>
        /// <param name="widget">Page supplied by the UI framework.</param>
        void OnLoaded(UIShop widget)
        {
            OnShopOpened(widget, request);
        }
    }

    /// <summary>Projects live state after asynchronous UI creation.</summary>
    /// <param name="widget">The successfully opened page.</param>
    /// <param name="request">The request that initiated this resource load.</param>
    private void OnShopOpened(UIShop widget, ShopOpenRequested request)
    {
        if (!ReferenceEquals(openingRequest, request))
            return;

        openingRequest = null;
        if (widget)
        {
            RefreshShop();
            XPluginUtility.FireEvent(new HomeFacilityOpened {
                action = request.guide,
                buildingId = 60019,
                succeeded = true
            });
        }
        else
        {
            XPluginUtility.FireEvent(new HomeFacilityPresentationChanged { systemId = 10005, visible = false });
            XPluginUtility.FireEvent(new HomeFacilityOpened {
                action = request.guide,
                buildingId = 60019,
                succeeded = false
            });
        }
    }

    /// <summary>Cancels only the in-flight page load owned by the canceled guide.</summary>
    /// <param name="message">Presentation canceled by TaskSystem or its scene owner.</param>
    private void OnGuideCancelled(TaskPresentationCancelled message)
    {
        if (openingRequest != null && ReferenceEquals(openingRequest.guide, message.action))
            CloseShop();
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
        OpenPurchase(message.offerId);
    }

    /// <summary>Opens an offer confirmation while its owning system remains active.</summary>
    /// <param name="offerId">Stable catalog identity selected by the user.</param>
    /// <returns>Whether the confirmation opening was requested.</returns>
    private bool OpenPurchase(int offerId)
    {
        if (!IsActive())
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        entity.Select(offerId);
        if (UISystem.GetSingleton().TryGetWidget<UIShopPurchase>(out var widget))
        {
            OnPurchaseOpened(widget);

            return true;
        }

        UISystem.GetSingleton().OpenWidget<UIShopPurchase>(OnPurchaseOpened);

        return true;
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
        var shop = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        if (shop.IsTaskRelevant(task) && task.status == TaskStatus.Completed)
            XPluginUtility.FireEvent(new TaskClaimRequested { taskId = task.configuration.ID });
    }

    /// <summary>Refreshes the catalog after inventory changes.</summary>
    /// <param name="message">Committed inventory.</param>
    private void OnVillageChanged(HomeVillageChanged message)
    {
        ReportPurchaseProgress();
        RefreshShop();
    }

    /// <summary>Refreshes the task footer after progress or rewards.</summary>
    /// <param name="message">Committed task state.</param>
    private void OnTaskChanged(TaskChanged message)
    {
        ReportPurchaseProgress();
        RefreshShop();
    }

    /// <summary>Replays committed item purchases without binding progress to particular task IDs.</summary>
    private void ReportPurchaseProgress()
    {
        var shop = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        foreach (var item in shop.GetPurchaseCounts(home.village))
        {
            var observation = new TaskProgressReported {
                condition = Config.ETaskConditionType.CumulativeItemPurchaseCount,
                param1 = item.Key,
                value = item.Value
            };

            XPluginUtility.FireEvent(observation);
        }
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

        System.Type[] widgets = new System.Type[] {
            typeof(UIShopPurchase)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
    }

    /// <summary>Releases the complete trading presentation.</summary>
    private void CloseShop()
    {
        openingRequest = null;
        var entity = XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        entity.Cancel();

        System.Type[] widgets = new System.Type[] {
            typeof(UIShopPurchase),
            typeof(UIShop)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
        XPluginUtility.FireEvent(new HomeFacilityPresentationChanged { systemId = 10005, visible = false });
    }
}
