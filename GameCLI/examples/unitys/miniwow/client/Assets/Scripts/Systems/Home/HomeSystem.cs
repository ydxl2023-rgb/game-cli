using System.Collections;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using UnityEngine;

/// <summary>Reports that Home resources and its initialized main page are ready.</summary>
public sealed class HomeReady : IEvent { }

/// <summary>Owns the Home page and its scene runtime.</summary>
public sealed class HomeSystem : RuntimeSystem
{
    /// <summary>Owned initialization operation canceled when Home is deactivated.</summary>
    private Coroutine initialization;

    /// <summary>Starts the scene transition and registers the implemented intents.</summary>
    /// <returns>True when the transition has been requested.</returns>
    public override bool OnActive()
    {
        Subscribe<ReqHomeOpeningContinue>(OnOpeningContinue);
        Subscribe<ReqHomeOpeningRescue>(OnOpeningRescue);
        Subscribe<ReqHomeOpeningClaim>(OnOpeningClaim);
        Subscribe<HomeOpeningChanged>(OnOpeningChanged);
        Subscribe<HomeOpeningOperationProgress>(OnOpeningOperationProgress);
        Subscribe<TaskChanged>(OnTaskChanged);
        Subscribe<ReqHomeVillageAction>(OnVillageAction);
        Subscribe<HomeVillageChanged>(OnVillageChanged);
        Subscribe<HomeVillageTreatmentProgress>(OnVillageTreatmentProgress);
        Subscribe<ReqHomeHospitalVisit>(OnHospitalVisit);
        Subscribe<ReqHomeResidentVisit>(OnResidentVisit);
        Subscribe<HomeHospitalArrived>(OnHospitalArrived);
        Subscribe<ReqHomeHospitalRepair>(OnHospitalRepair);
        Subscribe<ReqHomeHospitalCraft>(OnHospitalCraft);
        Subscribe<ReqHomeHospitalClose>(OnHospitalClose);
        Subscribe<ReqHomeHospitalClaim>(OnHospitalClaim);
        Subscribe<ReqHomeMap>(OnHomeMap);
        Subscribe<ReqHomeCamp>(OnHomeCamp);
        Subscribe<ReqHomeEntry>(OnHomeEntry);
        Subscribe<ReqHomeShortcut>(OnHomeShortcut);

        return InstallHome();
    }

    /// <summary>Invalidates pending work and closes only the owned Home page.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();

        UninstallHome();
    }

    /// <summary>Reports actual Home page initialization.</summary>
    /// <returns>Whether the active page is ready.</returns>
    public override bool IsReady()
    {
        var runtime = GetRuntime();

        return IsActive()
            && runtime
            && runtime.IsReady()
            && UISystem.GetSingleton().TryGetWidget<UIHome>(out _);
    }

    /// <summary>Returns the scene execution owned by this Home instance.</summary>
    /// <returns>The Home runtime, not a separately registered system.</returns>
    public HomeRuntime GetRuntime()
    {
        return RuntimeManager.GetSingleton().QueryRuntime<HomeRuntime>();
    }

    /// <summary>Switches between the camp and the implemented map preview.</summary>
    /// <param name="visible">Whether to show the map.</param>
    /// <returns>True when a live Home widget handled the request.</returns>
    public bool ShowMap(bool visible)
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            return false;

        var runtime = GetRuntime();
        bool changed = widget.ShowMap(visible);
        if (changed)
            runtime.SetInputEnabled(!visible);

        return changed;
    }

    /// <summary>Refreshes the active page from a detached entity snapshot.</summary>
    /// <returns>Whether a live initialized page was refreshed.</returns>
    public bool Refresh()
    {
        if (!IsActive() || !UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (!entity.IsInitialized())
            return false;

        widget.Invalidate(entity.Snapshot());

        return true;
    }

    /// <summary>Handles the ReqHomeMap event.</summary>
    /// <param name="message">The event payload.</param>
    private void OnHomeMap(ReqHomeMap message)
    {
        ShowMap(true);
    }

    /// <summary>Handles the ReqHomeCamp event.</summary>
    /// <param name="message">The event payload.</param>
    private void OnHomeCamp(ReqHomeCamp message)
    {
        ShowMap(false);
    }

    /// <summary>Rechecks current entry access before dispatching its destination.</summary>
    /// <param name="request">The configured entry intention.</param>
    private void OnHomeEntry(ReqHomeEntry request)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();

        HomeEntryResult result = entity.RequestEntry(request.entryId, out HomeEntryRoute route);
        if (result == HomeEntryResult.Allowed)
        {
            XPluginUtility.PostEvent(new HomeEntryRequested {
                entryId = request.entryId,
                route = route
            });
            if (route == HomeEntryRoute.MapPreview)
            {
                ShowMap(true);

                return;
            }
        }

        if (UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            widget.ShowFeedback(result);
    }

    /// <summary>Forwards fixed shortcuts using their typed destination contract.</summary>
    /// <param name="request">The fixed shortcut intention.</param>
    private void OnHomeShortcut(ReqHomeShortcut request)
    {
        if (request.route == HomeShortcut.Task)
        {
            OnOpeningClaim(new ReqHomeOpeningClaim());

            return;
        }

        XPluginUtility.PostEvent(new HomeShortcutRequested {
            route = request.route
        });

        if (UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            widget.ShowFeedback(Config.Excel.TEXT_HOME_PENDING);
    }

    /// <summary>Starts and owns the Home initialization operation.</summary>
    /// <returns>True when asynchronous initialization has been started.</returns>
    private bool InstallHome()
    {
        initialization = XCoroutine.Run(OpenHome());

        return true;
    }

    /// <summary>Cancels initialization and releases the Home page and scene runtime.</summary>
    private void UninstallHome()
    {
        XPluginUtility.FireEvent(new TaskRewardsRegistered { receiver = null });
        if (initialization != null)
        {
            XCoroutine.StopCoroutine(initialization);
            initialization = null;
        }

        System.Type[] widgets = new System.Type[] {
            typeof(UIHome),
            typeof(UIHomeOpening),
            typeof(UIHomeHospitalEntry),
            typeof(UIHomeHospital)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);

        RuntimeManager.GetSingleton().DestroyRuntime<HomeRuntime>();
    }

    /// <summary>Creates the scene runtime and opens Home after its load state completes.</summary>
    /// <returns>The initialization operation owned by this system.</returns>
    private IEnumerator OpenHome()
    {
        HomeRuntime runtime;
        try
        {
            runtime = RuntimeManager.GetSingleton().CreateRuntime<HomeRuntime>();
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
            XPluginUtility.PostEvent(new ReturnToLogin { failed = true });

            yield break;
        }

        while (!runtime.IsReady())
            yield return null;

        UISystem.GetSingleton().OpenWidget<UIHome>(OnHomeOpened);
        initialization = null;
    }

    /// <summary>Initializes the opened page before allowing scene input.</summary>
    /// <param name="widget">The Home page supplied by the UI framework.</param>
    private void OnHomeOpened(UIHome widget)
    {
        if (!widget)
            throw new System.InvalidOperationException("Home page could not be loaded.");

        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        widget.Invalidate(entity.Snapshot());
        widget.ShowMap(false);

        var runtime = GetRuntime();
        runtime.SetInputEnabled(false);
        UISystem.GetSingleton().OpenWidget<UIHomeOpening>(OnOpeningOpened);

    }

    /// <summary>Opens the original intro after Home owns its scene and page.</summary>
    /// <param name="widget">The initialized opening overlay.</param>
    private void OnOpeningOpened(UIHomeOpening widget)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        entity.opening.Reconcile(tasks);
        XPluginUtility.FireEvent(new TaskRewardsRegistered { receiver = entity.village });
        HomeVillageRuntime.ReportProgress(entity.village);
        if (entity.opening.taskCompleted)
            HomeOpeningRuntime.ReportCompletion(entity.opening);

        UISystem.GetSingleton().OpenWidget<UIHomeHospitalEntry>(OnHospitalEntryOpened);
        RefreshOpening();
        XPluginUtility.FireEvent(new HomeReady());
    }

    /// <summary>Advances the current movie or dialogue boundary.</summary>
    /// <param name="message">The user confirmation.</param>
    private void OnOpeningContinue(ReqHomeOpeningContinue message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (entity.opening.Continue())
            RefreshOpening();
    }

    /// <summary>Reports unimplemented resident interfaces without executing transactions.</summary>
    /// <param name="message">The resident entrance intention.</param>
    private void OnResidentVisit(ReqHomeResidentVisit message)
    {
        var ui = UISystem.GetSingleton();
        if (ui.TryGetWidget<UIHome>(out var home))
            home.ShowFeedback(Config.Excel.TEXT_HOME_PENDING);
    }

    /// <summary>Requests the original auction movement and rescue.</summary>
    /// <param name="message">The rescue intention.</param>
    private void OnOpeningRescue(ReqHomeOpeningRescue message)
    {
        var runtime = GetRuntime();
        runtime.Rescue();
    }

    /// <summary>Claims only after the merchant dialogue has finished.</summary>
    /// <param name="message">The explicit claim intention.</param>
    private void OnOpeningClaim(ReqHomeOpeningClaim message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (entity.opening.phase == HomeOpeningPhase.AwaitRecruit)
        {
            OnVillageAction(new ReqHomeVillageAction());

            return;
        }

        if (entity.opening.fetchingWater || entity.opening.canOperate)
        {
            var runtime = GetRuntime();
            runtime.Rescue();

            return;
        }

        if (!entity.opening.canClaim)
            return;

        XPluginUtility.PostEvent(new TaskClaimRequested { taskId = entity.opening.rescue.TaskID });
    }

    /// <summary>Executes the displayed business operation or claims its already completed task.</summary>
    /// <param name="message">The village interaction intention.</param>
    private void OnVillageAction(ReqHomeVillageAction message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var village = entity.village;
        var task = tasks.GetTrackedTask();
        if (entity.opening.phase != HomeOpeningPhase.AwaitRecruit)
            return;

        if (village.awaitingRecruitmentConfirmation)
        {
            village.ConfirmRecruitment();
            HomeVillageRuntime.ReportProgress(village);
        }
        else if (task.status == TaskStatus.Completed)
        {
            XPluginUtility.FireEvent(new TaskClaimRequested { taskId = task.configuration.ID });
        }
        else if (Config.Excel.villageTaskConfig.TryGetValue(task.configuration.ID, out var step)
            && ((HomeVillageAction)step.Action == HomeVillageAction.Repair
                || (HomeVillageAction)step.Action == HomeVillageAction.Craft
                || (HomeVillageAction)step.Action == HomeVillageAction.Heal))
        {
            OnHospitalVisit(new ReqHomeHospitalVisit());

            return;
        }
        else if (village.Execute(task.configuration.ID, tasks))
        {
            HomeVillageRuntime.ReportProgress(village);
        }
        else
        {
            var ui = UISystem.GetSingleton();
            if (!Config.Excel.villageTaskConfig.ContainsKey(task.configuration.ID)
                && ui.TryGetWidget<UIHome>(out var home))
                home.ShowFeedback(Config.Excel.TEXT_HOME_PENDING);
            else if (ui.TryGetWidget<UIHomeOpening>(out var overlay))
                overlay.ShowVillageFailure();

            return;
        }

        RefreshVillage();
    }

    /// <summary>Projects recovery progress without canceling the current joystick gesture.</summary>
    /// <param name="message">The committed result or recovery clock observation.</param>
    private void OnVillageChanged(HomeVillageChanged message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        HomeVillageRuntime.ReportProgress(entity.village);
        RefreshVillage();
    }

    /// <summary>Refreshes treatment text without allocating snapshots or resetting input.</summary>
    /// <param name="message">The recovery clock observation.</param>
    private void OnVillageTreatmentProgress(HomeVillageTreatmentProgress message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (UISystem.GetSingleton().TryGetWidget<UIHomeOpening>(out var overlay))
            overlay.ShowTreatmentProgress(entity.village);

        if (UISystem.GetSingleton().TryGetWidget<UIHomeHospital>(out var hospital))
            hospital.ShowTreatment(entity.village);
    }

    /// <summary>Updates economic HUD values and the active village interaction.</summary>
    private void RefreshVillage()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var snapshot = entity.Snapshot();
        entity.UpdateResources(entity.village.Count(entity.village.configuration.GoldID), snapshot.profile.gem,
            snapshot.profile.energy, snapshot.profile.energyLimit);
        Refresh();
        if (UISystem.GetSingleton().TryGetWidget<UIHome>(out var home))
            home.InvalidateOpening(entity.opening, tasks.GetTrackedTask());

        if (entity.opening.phase == HomeOpeningPhase.AwaitRecruit
            && UISystem.GetSingleton().TryGetWidget<UIHomeOpening>(out var overlay))
            overlay.InvalidateVillage(entity.village, tasks.GetTrackedTask());

        var ui = UISystem.GetSingleton();
        if (ui.TryGetWidget<UIHomeHospitalEntry>(out var entry))
            entry.Invalidate(entity.village, tasks);

        if (ui.TryGetWidget<UIHomeHospital>(out var hospital))
            hospital.Invalidate(entity.village, tasks.GetTrackedTask());
    }

    /// <summary>Updates transient countdown text without resetting scene input.</summary>
    /// <param name="message">The remaining operation duration.</param>
    private void OnOpeningOperationProgress(HomeOpeningOperationProgress message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var ui = UISystem.GetSingleton();
        if (ui.TryGetWidget<UIHomeOpening>(out var overlay))
            overlay.ShowOperationProgress(entity.opening, message.active, message.remaining);
    }

    /// <summary>Refreshes after real scene progress.</summary>
    /// <param name="message">The opening observation.</param>
    private void OnOpeningChanged(HomeOpeningChanged message)
    {
        RefreshOpening();
    }

    /// <summary>Refreshes after the task system commits progress or a claim.</summary>
    /// <param name="message">The committed task change.</param>
    private void OnTaskChanged(TaskChanged message)
    {
        if (UISystem.GetSingleton().TryGetWidget<UIHomeOpening>(out _))
            RefreshOpening();
    }

    /// <summary>Projects entity progress to Home without accessing either view model.</summary>
    private void RefreshOpening()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var opening = entity.opening;
        opening.Reconcile(tasks);
        var task = tasks.GetTrackedTask();
        var ui = UISystem.GetSingleton();
        if (ui.TryGetWidget<UIHomeOpening>(out var overlay))
            overlay.Invalidate(opening, task);

        if (ui.TryGetWidget<UIHome>(out var home))
            home.InvalidateOpening(opening, task);

        var runtime = GetRuntime();
        bool villagePanel = opening.phase == HomeOpeningPhase.AwaitRecruit
            && Config.Excel.villageTaskConfig.TryGetValue(task.configuration.ID, out var step)
            && ((HomeVillageAction)step.Action == HomeVillageAction.Recruit
                || (HomeVillageAction)step.Action == HomeVillageAction.Purchase);
        villagePanel |= ui.TryGetWidget<UIHomeHospital>(out _);
        runtime.SetInputEnabled(!villagePanel && (opening.fetchingWater || opening.canOperate
            || opening.phase == HomeOpeningPhase.AwaitRecruit));
        RefreshVillage();
    }

    /// <summary>Connects the hospital entry to its scene and current feature state.</summary>
    /// <param name="widget">Newly initialized entry.</param>
    private void OnHospitalEntryOpened(UIHomeHospitalEntry widget)
    {
        if (widget)
        {
            var runtime = GetRuntime();
            runtime.BindHospitalEntry(widget);
            RefreshVillage();
        }
    }

    /// <summary>Starts navigation only when the hospital feature is available.</summary>
    /// <param name="message">Hospital interaction intention.</param>
    private void OnHospitalVisit(ReqHomeHospitalVisit message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        if (!entity.village.buildings.IsAvailable(entity.village.configuration.BuildingID, tasks))
            return;

        var runtime = GetRuntime();
        runtime.NavigateHospital();
    }

    /// <summary>Chooses construction or the shop after actual player arrival.</summary>
    /// <param name="message">Navigation completion.</param>
    private void OnHospitalArrived(HomeHospitalArrived message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var village = entity.village;
        int id = village.configuration.BuildingID;
        var ui = UISystem.GetSingleton();
        if (village.buildings.Level(id) > 0)
        {
            var runtime = GetRuntime();
            runtime.SetInputEnabled(false);
            ui.OpenWidget<UIHomeHospital>(OnHospitalOpened);

            return;
        }

        if (ui.TryGetWidget<UIHomeHospitalEntry>(out var entry))
        {
            foreach (var cost in village.buildings.GetRepairCosts(id))
            {
                if (village.Count(cost.ItemID) < cost.Amount)
                {
                    entry.ShowMissingGold();

                    return;
                }
            }

            entry.PlayRepair(id);
        }
    }

    /// <summary>Commits construction after presentation while rechecking current state.</summary>
    /// <param name="message">Completed cost-flight intention.</param>
    private void OnHospitalRepair(ReqHomeHospitalRepair message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        if (entity.village.Execute(HomeVillageAction.Repair, tasks))
            OnVillageChanged(new HomeVillageChanged());
    }

    /// <summary>Initializes the production page from actual village data.</summary>
    /// <param name="widget">Initialized hospital page.</param>
    private void OnHospitalOpened(UIHomeHospital widget)
    {
        if (widget)
        {
            var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
            var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
            widget.Invalidate(entity.village, tasks.GetTrackedTask());
        }
    }

    /// <summary>Commits a recipe before playing the original success animation.</summary>
    /// <param name="message">Material click intention.</param>
    private void OnHospitalCraft(ReqHomeHospitalCraft message)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        if (!UISystem.GetSingleton().TryGetWidget<UIHomeHospital>(out var widget))
            return;

        if (!entity.village.Execute(HomeVillageAction.Craft, tasks))
        {
            widget.ShowMissingMaterial();

            return;
        }

        OnVillageChanged(new HomeVillageChanged());
        widget.PlayProduction(entity.village.configuration.BuildingID);
    }

    /// <summary>Closes only the hospital presentation and restores current Home input.</summary>
    /// <param name="message">Return intention.</param>
    private void OnHospitalClose(ReqHomeHospitalClose message)
    {
        System.Type[] widgets = new System.Type[] { typeof(UIHomeHospital) };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
        RefreshOpening();
    }

    /// <summary>Claims an already completed task without triggering a business operation.</summary>
    /// <param name="message">Reward intention.</param>
    private void OnHospitalClaim(ReqHomeHospitalClaim message)
    {
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var task = tasks.GetTrackedTask();
        if (task.status == TaskStatus.Completed)
            XPluginUtility.FireEvent(new TaskClaimRequested { taskId = task.configuration.ID });
    }

}
