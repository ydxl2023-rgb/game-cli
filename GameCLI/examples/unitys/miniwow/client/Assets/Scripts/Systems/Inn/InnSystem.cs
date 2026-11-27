using System.Collections;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using UnityEngine;

/// <summary>Owns inn presentation and meal scheduling independently of Home.</summary>
public sealed class InnSystem : RuntimeSystem
{
    /// <summary>Owned meal clock, stopped during system release.</summary>
    private Coroutine meals;

    /// <summary>Request whose page load is still owned by this system.</summary>
    private InnOpenRequested openingRequest;

    /// <summary>Subscribes inn intentions and installs the owned clock.</summary>
    /// <returns>Whether the system installed successfully.</returns>
    public override bool OnActive()
    {
        Subscribe<InnOpenRequested>(OnOpen);
        Subscribe<ReqInnClose>(OnClose);
        Subscribe<ReqInnRepair>(OnRepair);
        Subscribe<ReqInnCraft>(OnCraft);
        Subscribe<ReqInnRest>(OnRest);
        Subscribe<ReqInnClaim>(OnClaim);
        Subscribe<TaskChanged>(OnTaskChanged);
        Subscribe<HomeVillageChanged>(OnVillageChanged);
        Subscribe<HomeSessionClosing>(OnSessionClosing);
        Subscribe<HomeReady>(OnHomeReady);
        Subscribe<HomeSessionAvailable>(OnSessionAvailable);
        Subscribe<TaskPresentationCancelled>(OnGuideCancelled);

        return InstallInn();
    }

    /// <summary>Releases all owned presentation and scheduling.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        UninstallInn();
    }

    /// <summary>Attaches meal scheduling to the available Home aggregate.</summary>
    /// <param name="message">Completed Home initialization.</param>
    private void OnHomeReady(HomeReady message)
    {
        StartMeals();
    }

    /// <summary>Resumes durable meals when installed into an existing Home session.</summary>
    /// <param name="message">Available session response.</param>
    private void OnSessionAvailable(HomeSessionAvailable message)
    {
        StartMeals();
    }

    /// <summary>Owns a single coroutine for the current Home attachment.</summary>
    /// <returns>Whether the session attachment request was installed.</returns>
    private bool InstallInn()
    {
        XPluginUtility.FireEvent(new HomeSessionRequested());

        return true;
    }

    /// <summary>Starts the single clock for the current ready Home aggregate.</summary>
    private void StartMeals()
    {
        if (meals != null)
            XCoroutine.StopCoroutine(meals);

        meals = XCoroutine.Run(UpdateMeals());
        ReportProgress();
    }

    /// <summary>Releases session operations before Home destroys its aggregate.</summary>
    /// <param name="message">Closing Home session.</param>
    private void OnSessionClosing(HomeSessionClosing message)
    {
        UninstallInn();
    }

    /// <summary>Stops scheduling and releases the page.</summary>
    private void UninstallInn()
    {
        if (meals != null)
            XCoroutine.StopCoroutine(meals);

        meals = null;
        CloseInn();
    }

    /// <summary>Opens the original available feature, including its repair state.</summary>
    /// <param name="message">Accepted scene entrance.</param>
    private void OnOpen(InnOpenRequested message)
    {
        var inn = XEntityManager.GetSingleton().QueryEntity<InnEntity>();
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        if (!home.village.buildings.IsAvailable(inn.buildingId, tasks))
        {
            if (UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
                widget.ShowFeedback(Config.Excel.TEXT_INN_LOCKED);

            XPluginUtility.FireEvent(new HomeFacilityOpened {
                action = message.guide,
                buildingId = 60017,
                succeeded = false
            });

            return;
        }

        OpenInn(message);
    }

    /// <summary>Opens the page only while this module is active.</summary>
    /// <param name="request">Manual or guide-owned page intention.</param>
    /// <returns>Whether the resource request was accepted.</returns>
    private bool OpenInn(InnOpenRequested request)
    {
        if (!IsActive())
            return false;

        if (request.guide != null && request.guide.status != TaskActionStatus.Running)
            return false;

        if (UISystem.GetSingleton().TryGetWidget<UIInn>(out _))
        {
            RefreshInn();
            XPluginUtility.FireEvent(new HomeFacilityOpened {
                action = request.guide,
                buildingId = 60017,
                succeeded = true
            });

            return true;
        }

        if (openingRequest != null)
        {
            if (ReferenceEquals(openingRequest.guide, request.guide))
                return true;

            var previous = openingRequest.guide;
            CloseInn();
            if (previous != null)
                previous.Cancel();
        }

        openingRequest = request;
        XPluginUtility.FireEvent(new HomeFacilityPresentationChanged { systemId = 10007, visible = true });

        return UISystem.GetSingleton().OpenWidget<UIInn>(OnLoaded);

        /// <summary>Retains ownership through asynchronous resource completion.</summary>
        /// <param name="widget">Page supplied by the UI framework.</param>
        void OnLoaded(UIInn widget)
        {
            OnOpened(widget, request);
        }
    }

    /// <summary>Initializes the successfully loaded page.</summary>
    /// <param name="widget">Loaded inn widget.</param>
    /// <param name="request">The request that initiated this resource load.</param>
    private void OnOpened(UIInn widget, InnOpenRequested request)
    {
        if (!ReferenceEquals(openingRequest, request))
            return;

        openingRequest = null;
        if (widget)
        {
            RefreshInn();
            XPluginUtility.FireEvent(new HomeFacilityOpened {
                action = request.guide,
                buildingId = 60017,
                succeeded = true
            });
        }
        else
        {
            XPluginUtility.FireEvent(new HomeFacilityPresentationChanged { systemId = 10007, visible = false });
            XPluginUtility.FireEvent(new HomeFacilityOpened {
                action = request.guide,
                buildingId = 60017,
                succeeded = false
            });
        }
    }

    /// <summary>Releases only a page still loading for the canceled guide.</summary>
    /// <param name="message">Task presentation canceled before page readiness.</param>
    private void OnGuideCancelled(TaskPresentationCancelled message)
    {
        if (openingRequest != null && ReferenceEquals(openingRequest.guide, message.action))
            CloseInn();
    }

    /// <summary>Commits the first construction.</summary>
    /// <param name="message">Explicit repair intention.</param>
    private void OnRepair(ReqInnRepair message)
    {
        Execute(InnAction.Repair, Config.Excel.TEXT_INN_REPAIRED, Config.Excel.TEXT_INN_NO_GOLD);
    }

    /// <summary>Commits one food recipe.</summary>
    /// <param name="message">Material click intention.</param>
    private void OnCraft(ReqInnCraft message)
    {
        Execute(InnAction.Craft, Config.Excel.TEXT_INN_CRAFTED, Config.Excel.TEXT_INN_NO_MATERIAL);
    }

    /// <summary>Starts a paid meal without directly completing recovery.</summary>
    /// <param name="message">Owned hero meal intention.</param>
    private void OnRest(ReqInnRest message)
    {
        Execute(InnAction.Rest, Config.Excel.TEXT_INN_REST, Config.Excel.TEXT_INN_REST_UNAVAILABLE);
    }

    /// <summary>Commits through the shared atomic owner and refreshes presentation.</summary>
    /// <param name="action">Typed inn operation.</param>
    /// <param name="successText">Localized success response.</param>
    /// <param name="failureText">Localized unavailable response.</param>
    /// <returns>Whether the requested business transaction committed.</returns>
    private bool Execute(InnAction action, int successText, int failureText)
    {
        if (!UISystem.GetSingleton().TryGetWidget<UIInn>(out var widget))
            return false;

        var inn = XEntityManager.GetSingleton().QueryEntity<InnEntity>();
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        bool committed = home.village.ExecuteInn(inn, action, tasks);
        if (committed)
            XPluginUtility.FireEvent(new HomeVillageChanged());

        widget.ShowFeedback(committed ? successText : failureText);
        if (committed && action == InnAction.Craft)
            widget.PlayProduction();

        return committed;
    }

    /// <summary>Refreshes after any shared wallet or inventory transaction.</summary>
    /// <param name="message">Committed village change.</param>
    private void OnVillageChanged(HomeVillageChanged message)
    {
        ReportProgress();
        RefreshInn();
    }

    /// <summary>Reports absolute inn observations without coupling progression to task identities.</summary>
    private void ReportProgress()
    {
        var inn = XEntityManager.GetSingleton().QueryEntity<InnEntity>();
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var state = home.village.Snapshot();
        var service = home.village.buildings.GetService(inn.buildingId);

        XPluginUtility.FireEvent(new TaskProgressReported {
            condition = Config.ETaskConditionType.BuildingUpLevelTo,
            param1 = inn.buildingId,
            value = home.village.buildings.Level(inn.buildingId)
        });
        XPluginUtility.FireEvent(new TaskProgressReported {
            condition = Config.ETaskConditionType.CumulativeBuildItemTimes,
            param1 = service.productId,
            value = state.innCrafts
        });
        XPluginUtility.FireEvent(new TaskProgressReported {
            condition = Config.ETaskConditionType.UseCostItem,
            param1 = inn.buildingId,
            value = state.innMeals
        });
    }

    /// <summary>Claims only a completed inn-related task.</summary>
    /// <param name="message">Explicit reward intention.</param>
    private void OnClaim(ReqInnClaim message)
    {
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var task = tasks.GetTrackedTask();
        if (task != null && task.configuration.TaskBuildingType == Config.ETaskBuildingType.Hotel
            && task.status == TaskStatus.Completed)
            XPluginUtility.FireEvent(new TaskClaimRequested { taskId = task.configuration.ID });
    }

    /// <summary>Refreshes task visibility after progression.</summary>
    /// <param name="message">Committed task state.</param>
    private void OnTaskChanged(TaskChanged message)
    {
        RefreshInn();
    }

    /// <summary>Projects entities into the owned widget.</summary>
    /// <returns>Whether an open page was refreshed.</returns>
    private bool RefreshInn()
    {
        if (!UISystem.GetSingleton().TryGetWidget<UIInn>(out var widget))
            return false;

        var inn = XEntityManager.GetSingleton().QueryEntity<InnEntity>();
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        widget.Invalidate(inn, home, tasks.GetTrackedTask());

        return true;
    }

    /// <summary>Completes persisted meals and displays their remaining duration.</summary>
    /// <returns>The system-owned operation.</returns>
    private IEnumerator UpdateMeals()
    {
        var inn = XEntityManager.GetSingleton().QueryEntity<InnEntity>();
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        while (true)
        {
            var state = home.village.Snapshot();
            if (state.innRestEndTicks != 0 && home.village.ExecuteInn(inn, InnAction.Complete, tasks))
                XPluginUtility.FireEvent(new HomeVillageChanged());

            RefreshInn();

            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>Closes only the page while meals continue.</summary>
    /// <param name="message">Back intention.</param>
    private void OnClose(ReqInnClose message)
    {
        CloseInn();
    }

    /// <summary>Releases the page and its scene input ownership.</summary>
    private void CloseInn()
    {
        openingRequest = null;

        System.Type[] widgets = new System.Type[] {
            typeof(UIInn)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
        XPluginUtility.FireEvent(new HomeFacilityPresentationChanged { systemId = 10007, visible = false });
    }
}
