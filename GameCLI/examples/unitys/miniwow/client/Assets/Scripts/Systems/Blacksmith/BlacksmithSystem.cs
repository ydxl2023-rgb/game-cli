using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using System.Linq;

/// <summary>Requests the forge after its configured system becomes active.</summary>
public sealed class BlacksmithOpenRequested : IEvent
{
    /// <summary>Optional owner of an asynchronous guide opening.</summary>
    public TaskPresentationAction guide
    { get; set; }
}

/// <summary>Coordinates forge pages and delegates business rules to its entity.</summary>
public sealed class BlacksmithSystem : RuntimeSystem
{
    /// <summary>Request owning the framework's current page load.</summary>
    private BlacksmithOpenRequested openingRequest;

    /// <summary>Installs this system's event handlers.</summary>
    /// <returns>Whether installation succeeded.</returns>
    public override bool OnActive()
    {
        Subscribe<BlacksmithOpenRequested>(OnOpen);
        Subscribe<ReqBlacksmithClose>(OnClose);
        Subscribe<ReqBlacksmithSelect>(OnSelect);
        Subscribe<ReqBlacksmithQuantity>(OnQuantity);
        Subscribe<ReqBlacksmithAction>(OnAction);
        Subscribe<ReqBlacksmithCancel>(OnCancel);
        Subscribe<HomeVillageChanged>(OnVillageChanged);
        Subscribe<TaskChanged>(OnTaskChanged);
        Subscribe<HomeSessionClosing>(OnSessionClosing);
        Subscribe<TaskPresentationCancelled>(OnGuideCancelled);

        return true;
    }

    /// <summary>Releases owned pages and subscriptions.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        CloseBlacksmith();
    }

    /// <summary>Checks original access rules before opening the page.</summary>
    /// <param name="message">Manual or guide-owned request.</param>
    private void OnOpen(BlacksmithOpenRequested message)
    {
        OpenBlacksmith(message);
    }

    /// <summary>Opens one page through the framework's asynchronous loader.</summary>
    /// <param name="request">Owner of this opening.</param>
    /// <returns>Whether the request was accepted.</returns>
    private bool OpenBlacksmith(BlacksmithOpenRequested request)
    {
        if (!IsActive())
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<BlacksmithEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var ui = UISystem.GetSingleton();
        if (!entity.IsAvailable(tasks))
        {
            if (ui.TryGetWidget<UIHome>(out var home))
                home.ShowFeedback(Config.Excel.TEXT_BLACKSMITH_LOCKED);

            XPluginUtility.FireEvent(new HomeFacilityOpened {
                action = request.guide,
                buildingId = 60002,
                succeeded = false
            });

            return false;
        }

        ReportProgress();
        if (request.guide != null && request.guide.status != TaskActionStatus.Running)
            return false;

        if (ui.TryGetWidget<UIBlacksmith>(out _))
        {
            RefreshBlacksmith();
            XPluginUtility.FireEvent(new HomeFacilityOpened {
                action = request.guide,
                buildingId = 60002,
                succeeded = true
            });

            return true;
        }

        if (openingRequest != null)
        {
            if (ReferenceEquals(openingRequest.guide, request.guide))
                return true;

            var previous = openingRequest.guide;
            CloseBlacksmith();
            if (previous != null)
                previous.Cancel();
        }

        entity.Cancel();
        openingRequest = request;
        XPluginUtility.FireEvent(new HomeFacilityPresentationChanged {
            systemId = 10008,
            visible = true
        });
        ui.OpenWidget<UIBlacksmith>(OnLoaded);

        return true;

        /// <summary>Retains request identity until the framework completes.</summary>
        /// <param name="widget">Loaded page.</param>
        void OnLoaded(UIBlacksmith widget)
        {
            OnBlacksmithOpened(widget, request);
        }
    }

    /// <summary>Completes only the page load owned by this request.</summary>
    /// <param name="widget">Loaded page.</param>
    /// <param name="request">Original owner.</param>
    private void OnBlacksmithOpened(UIBlacksmith widget, BlacksmithOpenRequested request)
    {
        if (!ReferenceEquals(openingRequest, request))
            return;

        openingRequest = null;
        if (widget)
            RefreshBlacksmith();
        else
            XPluginUtility.FireEvent(new HomeFacilityPresentationChanged {
                systemId = 10008,
                visible = false
            });

        XPluginUtility.FireEvent(new HomeFacilityOpened {
            action = request.guide,
            buildingId = 60002,
            succeeded = widget
        });
    }

    /// <summary>Closes a canceled guide-owned pending page.</summary>
    /// <param name="message">Canceled presentation.</param>
    private void OnGuideCancelled(TaskPresentationCancelled message)
    {
        if (openingRequest != null && ReferenceEquals(openingRequest.guide, message.action))
            CloseBlacksmith();
    }

    /// <summary>Handles the page back intention.</summary>
    /// <param name="message">Back intention.</param>
    private void OnClose(ReqBlacksmithClose message)
    {
        CloseBlacksmith();
    }

    /// <summary>Releases presentation before Home ownership ends.</summary>
    /// <param name="message">Closing session.</param>
    private void OnSessionClosing(HomeSessionClosing message)
    {
        CloseBlacksmith();
    }

    /// <summary>Selects a recipe or shelf identity and opens explicit confirmation.</summary>
    /// <param name="message">Selected row.</param>
    private void OnSelect(ReqBlacksmithSelect message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<BlacksmithEntity>();
        entity.Cancel();
        if (message.shelf)
            entity.SelectEquipment(message.identity);
        else
            entity.SelectRecipe(checked((int)message.identity));

        OpenConfirmation();
    }

    /// <summary>Reuses an open confirmation instead of creating duplicate noncached pages.</summary>
    /// <returns>Whether confirmation was requested.</returns>
    private bool OpenConfirmation()
    {
        if (!IsActive())
            return false;

        var ui = UISystem.GetSingleton();
        if (ui.TryGetWidget<UIBlacksmithConfirm>(out var widget))
            OnConfirmationOpened(widget);
        else
            ui.OpenWidget<UIBlacksmithConfirm>(OnConfirmationOpened);

        return true;
    }

    /// <summary>Projects the current pending transaction.</summary>
    /// <param name="widget">Loaded confirmation.</param>
    private void OnConfirmationOpened(UIBlacksmithConfirm widget)
    {
        if (widget)
            RefreshBlacksmith();
    }

    /// <summary>Changes pending quantity without spending.</summary>
    /// <param name="message">Quantity step.</param>
    private void OnQuantity(ReqBlacksmithQuantity message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<BlacksmithEntity>();
        entity.ChangeQuantity(message.delta);
        RefreshBlacksmith();
    }

    /// <summary>Commits one validated transaction and refreshes from its durable result.</summary>
    /// <param name="message">Explicit operation intention.</param>
    private void OnAction(ReqBlacksmithAction message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<BlacksmithEntity>();
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        int result = home.village.ExecuteBlacksmith(entity, message.action, tasks);
        var ui = UISystem.GetSingleton();
        if (result == 0)
        {
            CloseConfirmation();
            XPluginUtility.FireEvent(new HomeVillageChanged());
            int feedback = message.action == BlacksmithAction.Forge ? Config.Excel.TEXT_BLACKSMITH_SUCCESS
                : message.action == BlacksmithAction.Sell ? Config.Excel.TEXT_BLACKSMITH_SOLD
                : message.action == BlacksmithAction.Repair ? Config.Excel.TEXT_BLACKSMITH_REPAIRED : 0;
            if (feedback != 0 && ui.TryGetWidget<UIBlacksmith>(out var page))
            {
                page.ShowFeedback(feedback);
                if (message.action == BlacksmithAction.Forge)
                    page.PlayProduction();
            }
        }
        else if (ui.TryGetWidget<UIBlacksmithConfirm>(out var confirmation))
            confirmation.ShowFeedback(result);
        else if (ui.TryGetWidget<UIBlacksmith>(out var page))
            page.ShowFeedback(result);
    }

    /// <summary>Cancels the transaction selection.</summary>
    /// <param name="message">Cancel intention.</param>
    private void OnCancel(ReqBlacksmithCancel message)
    {
        CloseConfirmation();
    }

    /// <summary>Refreshes pages after committed economy changes.</summary>
    /// <param name="message">Committed change.</param>
    private void OnVillageChanged(HomeVillageChanged message)
    {
        ReportProgress();
        RefreshBlacksmith();
    }

    /// <summary>Replays committed observations when a new task is dispatched.</summary>
    /// <param name="message">Updated task collection.</param>
    private void OnTaskChanged(TaskChanged message)
    {
        ReportProgress();
    }

    /// <summary>Reports absolute totals without coupling progress to task identities.</summary>
    private void ReportProgress()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var saved = home.village.Snapshot().blacksmith;
        long count = saved.forged.Values.Sum();
        XPluginUtility.FireEvent(new TaskProgressReported {
            condition = Config.ETaskConditionType.BuildEquipmentTimes,
            value = count,
            itemCounts = saved.forged
        });
        XPluginUtility.FireEvent(new TaskProgressReported {
            condition = Config.ETaskConditionType.ForgeAnyEquipment,
            value = count
        });
        XPluginUtility.FireEvent(new TaskProgressReported {
            condition = Config.ETaskConditionType.TradeEquipmentTimes,
            value = saved.sold.Values.Sum(),
            itemCounts = saved.sold
        });
    }

    /// <summary>Projects entities through Widget APIs.</summary>
    private void RefreshBlacksmith()
    {
        var ui = UISystem.GetSingleton();
        var entity = XEntityManager.GetSingleton().QueryEntity<BlacksmithEntity>();
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (ui.TryGetWidget<UIBlacksmith>(out var page))
            page.Invalidate(entity, home.village);

        if ((entity.selectedRecipe != 0 || entity.selectedEquipment != 0)
            && ui.TryGetWidget<UIBlacksmithConfirm>(out var confirmation))
            confirmation.Invalidate(entity, home.village);
    }

    /// <summary>Releases confirmation and its pending selection.</summary>
    private void CloseConfirmation()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<BlacksmithEntity>();
        entity.Cancel();

        System.Type[] widgets = new System.Type[] {
            typeof(UIBlacksmithConfirm)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
    }

    /// <summary>Releases all presentation and scene-input ownership.</summary>
    private void CloseBlacksmith()
    {
        openingRequest = null;
        var entity = XEntityManager.GetSingleton().QueryEntity<BlacksmithEntity>();
        entity.Cancel();

        System.Type[] widgets = new System.Type[] {
            typeof(UIBlacksmithConfirm),
            typeof(UIBlacksmith)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
        XPluginUtility.FireEvent(new HomeFacilityPresentationChanged {
            systemId = 10008,
            visible = false
        });
    }
}

