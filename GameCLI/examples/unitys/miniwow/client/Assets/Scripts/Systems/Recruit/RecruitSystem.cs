using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;

/// <summary>Requests the available conversation at an original recruitment destination.</summary>
public sealed class RecruitOpenRequested : IEvent
{
    /// <summary>Original building identity.</summary>
    public int buildingId
    { get; set; }
}

/// <summary>Reports actual completion of a configured recruitment entrance walk.</summary>
public sealed class RecruitActorArrived : IEvent
{
    /// <summary>Hero whose scene actor reached the original conversation position.</summary>
    public int heroId
    { get; set; }
}

/// <summary>Coordinates configured conversations, whistle playback and recruitment receipts.</summary>
public sealed class RecruitSystem : RuntimeSystem
{
    /// <summary>Task presentation currently owning player navigation.</summary>
    private TaskPresentationAction navigation;

    /// <summary>Configured operation whose page load is still owned by this module.</summary>
    private Config.RecruitStep openingStep;

    /// <summary>Explicit interaction awaiting the hero's actual entry walk.</summary>
    private RecruitOpenRequested arrivalRequest;

    /// <summary>Installs named interaction handlers.</summary>
    /// <returns>Whether installation succeeded.</returns>
    public override bool OnActive()
    {
        Subscribe<HomeReady>(OnHomeReady);
        Subscribe<HomeSessionAvailable>(OnSessionAvailable);
        Subscribe<HomeSessionClosing>(OnSessionClosing);
        Subscribe<RecruitOpenRequested>(OnOpen);
        Subscribe<RecruitActorArrived>(OnActorArrived);
        Subscribe<ReqRecruitContinue>(OnContinue);
        Subscribe<ReqRecruitClose>(OnClose);
        Subscribe<ReqRecruitMovieFinished>(OnMovieFinished);
        Subscribe<TaskPresentationRequested>(OnPresentation);
        Subscribe<TaskPresentationCancelled>(OnPresentationCancelled);
        Subscribe<HomeResidentNavigationFinished>(OnNavigationFinished);
        Subscribe<HomeManualMovementStarted>(OnManualMovement);

        return InstallRecruit();
    }

    /// <summary>Releases presentation before subscriptions disappear.</summary>
    public override void OnDetive()
    {
        UninstallRecruit();
        UnsubscribeAll();
    }

    /// <summary>Attaches to an available Home session.</summary>
    /// <returns>Whether the attachment request was sent.</returns>
    private bool InstallRecruit()
    {
        XPluginUtility.FireEvent(new HomeSessionRequested());

        return true;
    }

    /// <summary>Closes owned presentation without erasing durable whistle intent.</summary>
    private void UninstallRecruit()
    {
        CloseRecruit();
        if (navigation != null)
        {
            var action = navigation;
            navigation = null;
            XPluginUtility.FireEvent(new HomeResidentNavigationCancelled { action = action });
            action.Complete(false);
        }

        navigation = null;
    }

    /// <summary>Restores receipts after Home becomes available.</summary>
    /// <param name="message">Scene readiness.</param>
    private void OnHomeReady(HomeReady message)
    {
        RestoreRecruit();
    }

    /// <summary>Restores receipts when reinstalled into a running session.</summary>
    /// <param name="message">Session response.</param>
    private void OnSessionAvailable(HomeSessionAvailable message)
    {
        RestoreRecruit();
    }

    /// <summary>Releases presentation before Home data is destroyed.</summary>
    /// <param name="message">Closing session.</param>
    private void OnSessionClosing(HomeSessionClosing message)
    {
        UninstallRecruit();
    }

    /// <summary>Replays committed observations and resumes paid movie presentation.</summary>
    private void RestoreRecruit()
    {
        ReportProgress();
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var saved = home.village.Snapshot();
        if (saved.recruit.pendingHorn != 0 && openingStep == null
            && !UISystem.GetSingleton().TryGetWidget<UIRecruit>(out _))
        {
            var recruit = XEntityManager.GetSingleton().QueryEntity<RecruitEntity>();
            recruit.Select(Config.Excel.recruitStepConfig[saved.recruit.pendingHorn]);
            OpenRecruit();
        }
    }

    /// <summary>Accepts only configured recruitment navigation.</summary>
    /// <param name="message">Task presentation request.</param>
    private void OnPresentation(TaskPresentationRequested message)
    {
        if (message.action.operation != TaskPresentation.Navigate)
            return;

        int buildingId = checked((int)message.action.argument.AsNumber());
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var recruit = XEntityManager.GetSingleton().QueryEntity<RecruitEntity>();
        var step = recruit.FindStep(buildingId, home.village.Snapshot(), tasks);
        if (step == null)
            return;

        message.accepted = true;
        navigation = message.action;
        XPluginUtility.FireEvent(new HomeResidentNavigationRequested {
            buildingId = buildingId,
            action = navigation
        });
    }

    /// <summary>Stops navigation when its task relinquishes ownership.</summary>
    /// <param name="message">Canceled presentation.</param>
    private void OnPresentationCancelled(TaskPresentationCancelled message)
    {
        if (!ReferenceEquals(navigation, message.action))
            return;

        var action = navigation;
        navigation = null;
        XPluginUtility.FireEvent(new HomeResidentNavigationCancelled { action = action });
        CloseRecruit();
    }

    /// <summary>Opens a conversation only after the player actually arrives.</summary>
    /// <param name="message">World route outcome.</param>
    private void OnNavigationFinished(HomeResidentNavigationFinished message)
    {
        if (navigation == null || !ReferenceEquals(navigation, message.action))
            return;

        if (!message.arrived)
        {
            CompleteNavigation(false);

            return;
        }

        OnOpen(new RecruitOpenRequested { buildingId = message.buildingId });
    }

    /// <summary>Cancels pending interaction when the player takes control before the hero arrives.</summary>
    /// <param name="message">Explicit manual movement.</param>
    private void OnManualMovement(HomeManualMovementStarted message)
    {
        if (navigation != null)
            navigation.Cancel();
    }

    /// <summary>Completes the guide only after its interaction is available or has failed.</summary>
    /// <param name="succeeded">Actual interaction readiness.</param>
    private void CompleteNavigation(bool succeeded)
    {
        if (navigation == null)
            return;

        var action = navigation;
        navigation = null;
        action.Complete(succeeded);
    }

    /// <summary>Selects the available configured operation for an explicit interaction.</summary>
    /// <param name="message">Resident selected by the player.</param>
    private void OnOpen(RecruitOpenRequested message)
    {
        if (openingStep != null || UISystem.GetSingleton().TryGetWidget<UIRecruit>(out _))
            return;

        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var recruit = XEntityManager.GetSingleton().QueryEntity<RecruitEntity>();
        var step = recruit.FindStep(message.buildingId, home.village.Snapshot(), tasks);
        if (step == null)
        {
            CompleteNavigation(false);

            return;
        }

        if ((RecruitStepKind)step.Kind == RecruitStepKind.Receive)
        {
            var definition = Config.Excel.recruitHeroConfig[step.HeroID];
            var actor = home.FindActor(definition.ActorID);
            if (actor.phase != ActorPhase.Waiting)
            {
                arrivalRequest = message;

                return;
            }
        }

        recruit.Select(step);
        OpenRecruit();
    }

    /// <summary>Resumes an explicitly requested conversation after its actor arrives.</summary>
    /// <param name="message">Actual original-position arrival.</param>
    private void OnActorArrived(RecruitActorArrived message)
    {
        if (arrivalRequest == null)
            return;

        var request = arrivalRequest;
        arrivalRequest = null;
        OnOpen(request);
    }

    /// <summary>Opens the original conversation presentation.</summary>
    private void OpenRecruit()
    {
        var recruit = XEntityManager.GetSingleton().QueryEntity<RecruitEntity>();
        var step = Config.Excel.recruitStepConfig[recruit.selectedStep];
        openingStep = step;
        XPluginUtility.FireEvent(new HomeFacilityPresentationChanged { systemId = 10010, visible = true });
        UISystem.GetSingleton().OpenWidget<UIRecruit>(OnOpened);

        /// <summary>Accepts only the resource completion still owned by this operation.</summary>
        /// <param name="widget">Loaded conversation widget.</param>
        void OnOpened(UIRecruit widget)
        {
            if (!ReferenceEquals(openingStep, step))
                return;

            openingStep = null;
            RefreshRecruit(widget);
        }
    }

    /// <summary>Projects the selected dialogue or paid movie.</summary>
    /// <param name="widget">Loaded conversation widget.</param>
    private void RefreshRecruit(UIRecruit widget)
    {
        if (!widget)
        {
            CloseRecruit();
            CompleteNavigation(false);

            return;
        }

        var recruit = XEntityManager.GetSingleton().QueryEntity<RecruitEntity>();
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (home.village.Snapshot().recruit.pendingHorn != 0)
            widget.PlayMovie();
        else
            widget.Invalidate(recruit.GetDialogue());

        CompleteNavigation(true);
    }

    /// <summary>Commits an interaction only after the final original line is acknowledged.</summary>
    /// <param name="message">Explicit conversation continuation.</param>
    private void OnContinue(ReqRecruitContinue message)
    {
        var recruit = XEntityManager.GetSingleton().QueryEntity<RecruitEntity>();
        if (!UISystem.GetSingleton().TryGetWidget<UIRecruit>(out var widget))
            return;

        if (recruit.NextDialogue())
        {
            widget.Invalidate(recruit.GetDialogue());

            return;
        }

        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var step = Config.Excel.recruitStepConfig[recruit.selectedStep];
        if (!home.village.ExecuteRecruit(recruit, step, tasks))
        {
            widget.ShowFailure();

            return;
        }

        XPluginUtility.FireEvent(new HomeVillageChanged());
        if ((RecruitStepKind)step.Kind == RecruitStepKind.Horn)
            widget.PlayMovie();
        else
        {
            CloseRecruit();
            ReportProgress();
        }
    }

    /// <summary>Completes the paid whistle after the original movie reaches its end.</summary>
    /// <param name="message">Actual video completion.</param>
    private void OnMovieFinished(ReqRecruitMovieFinished message)
    {
        var recruit = XEntityManager.GetSingleton().QueryEntity<RecruitEntity>();
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (!home.village.CompleteRecruitHorn(recruit))
            return;

        CloseRecruit();
        ReportProgress();
        XPluginUtility.FireEvent(new HomeVillageChanged());
    }

    /// <summary>Closes a conversation without completing its task.</summary>
    /// <param name="message">Explicit close intention.</param>
    private void OnClose(ReqRecruitClose message)
    {
        CloseRecruit();
    }

    /// <summary>Releases modal ownership and transient dialogue selection.</summary>
    private void CloseRecruit()
    {
        openingStep = null;
        arrivalRequest = null;

        System.Type[] widgets = new System.Type[] {
            typeof(UIRecruit)
        };

        UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
        var recruit = XEntityManager.GetSingleton().QueryEntity<RecruitEntity>();
        recruit.Cancel();
        XPluginUtility.FireEvent(new HomeFacilityPresentationChanged { systemId = 10010, visible = false });
    }

    /// <summary>Replays absolute receipts and original automatic recruitment reward claims.</summary>
    private void ReportProgress()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var saved = home.village.Snapshot();
        foreach (int id in saved.recruit.completed)
        {
            var definition = Config.Excel.taskConfig[id];

            var observation = new TaskProgressReported {
                taskId = id,
                condition = definition.TaskConditionType,
                param1 = definition.Param1,
                param2 = definition.Param2,
                param3 = definition.Param3,
                value = 1
            };

            XPluginUtility.FireEvent(observation);
            if (Config.Excel.recruitStepConfig[id].AutoClaim != 0
                && tasks.GetTask(id).status == TaskStatus.Completed)
                XPluginUtility.PostEvent(new TaskClaimRequested { taskId = id });
        }
    }
}
