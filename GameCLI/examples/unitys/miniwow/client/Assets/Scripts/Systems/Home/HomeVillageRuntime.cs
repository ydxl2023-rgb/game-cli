using Oathx.Core;
using Oathx.Entity;
using UnityEngine;

/// <summary>Reports a committed village result or changing recovery presentation.</summary>
public sealed class HomeVillageChanged : IEvent { }

/// <summary>Refreshes the active recovery countdown without replaying business observations.</summary>
public sealed class HomeVillageTreatmentProgress : IEvent { }

/// <summary>Reports arrival at the hospital interaction point.</summary>
public sealed class HomeHospitalArrived : IEvent { }

/// <summary>Executes hero navigation and recovery against the original Home navigation data.</summary>
public sealed class HomeVillageRuntime
{
    /// <summary>Scene references owned by HomeRuntime.</summary>
    private readonly HomeScene scene;

    /// <summary>Whether this scene instance installed the persisted travel intent.</summary>
    private bool traveling;

    /// <summary>Current requested player route to the hospital.</summary>
    private Vector3[] hospitalRoute;

    /// <summary>Next corner in the active hospital route.</summary>
    private int hospitalCorner;

    /// <summary>Creates the scene adapter; business state remains in its entity.</summary>
    /// <param name="context">Current Home scene.</param>
    public HomeVillageRuntime(HomeScene context)
    {
        scene = context;
    }

    /// <summary>Runs navigation and time-based treatment independently of Shape visibility.</summary>
    /// <param name="elapsed">Simulation interval.</param>
    public void Tick(float elapsed)
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var village = home.village;
        scene.ShowVillage(home.opening, village);
        MoveToHospital(home.FindActor(scene.GetPlayerId()), elapsed);
        if (village.recovery == HeroRecoveryPhase.Traveling)
        {
            var actor = home.FindActor(village.configuration.ActorID);
            if (!traveling)
            {
                var target = new Vector3(village.configuration.HospitalX,
                    village.configuration.HospitalY, village.configuration.HospitalZ);
                var corners = scene.CalculateActorRoute(actor.position, target);
                if (corners.Length == 0)
                    throw new System.InvalidOperationException("The configured hospital route is unreachable.");

                if (!actor.FollowRoute(corners))
                    throw new System.InvalidOperationException("The hospital visitor rejected its navigation path.");

                traveling = true;
            }

            if (actor.phase == ActorPhase.Waiting)
                XPluginUtility.FireEvent(new HospitalPatientArrived());
        }

    }

    /// <summary>Requests a real player route on the recovered navigation mesh.</summary>
    public void NavigateHospital()
    {
        hospitalRoute = scene.CalculateHospitalRoute();
        hospitalCorner = 0;
    }

    /// <summary>Cancels only the player interaction route when manual input takes over.</summary>
    public void CancelNavigation()
    {
        hospitalRoute = null;
    }

    /// <summary>Replays absolute committed business observations after a transaction or restore.</summary>
    /// <param name="village">Authoritative village state.</param>
    public static void ReportProgress(HomeVillageEntity village)
    {
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        foreach (var step in Config.Excel.villageTaskConfig.Values)
        {
            var task = Config.Excel.taskConfig[step.ID];

            var observation = new TaskProgressReported {
                condition = task.TaskConditionType,
                param1 = task.Param1,
                value = village.Progress((HomeVillageAction)step.Action)
            };

            XPluginUtility.FireEvent(observation);
            if ((HomeVillageAction)step.Action == HomeVillageAction.Recruit
                && observation.value > 0 && !village.awaitingRecruitmentConfirmation
                && tasks.GetTask(step.ID).status == TaskStatus.Completed)
                XPluginUtility.PostEvent(new TaskClaimRequested { taskId = step.ID });
        }
    }

    /// <summary>Advances the player before notifying the interaction owner.</summary>
    /// <param name="actor">Authoritative playable actor.</param>
    /// <param name="elapsed">Simulation interval.</param>
    private void MoveToHospital(ActorEntity actor, float elapsed)
    {
        if (hospitalRoute == null)
            return;

        while (hospitalCorner < hospitalRoute.Length)
        {
            Vector3 offset = hospitalRoute[hospitalCorner] - actor.position;
            offset.y = 0f;
            if (offset.magnitude < 0.15f)
            {
                hospitalCorner++;

                continue;
            }

            float step = actor.moveSpeed * elapsed;
            actor.SetMoveInput(offset.normalized * Mathf.Min(1f, offset.magnitude / Mathf.Max(step, 0.001f)));

            return;
        }

        bool arrived = hospitalRoute.Length > 0;
        hospitalRoute = null;
        actor.SetMoveInput(Vector3.zero);
        if (arrived)
            XPluginUtility.PostEvent(new HomeHospitalArrived());
    }

}
