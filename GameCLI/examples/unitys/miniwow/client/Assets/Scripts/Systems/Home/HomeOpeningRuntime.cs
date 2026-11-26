using Oathx.Core;
using Oathx.Entity;
using UnityEngine;

/// <summary>Executes the configured opening operations against the original navigation and entity position.</summary>
public sealed class HomeOpeningRuntime
{
    /// <summary>Scene references supplied by the owning Home runtime.</summary>
    private readonly HomeScene scene;

    /// <summary>Current native navigation corners, cleared by manual input or arrival.</summary>
    private Vector3[] route;

    /// <summary>The next unvisited path corner.</summary>
    private int corner;

    /// <summary>Elapsed uninterrupted operation time.</summary>
    private float operationElapsed;

    /// <summary>Whether the user has requested the current operation.</summary>
    private bool rescuing;

    /// <summary>Creates scene execution without retaining a system reference.</summary>
    /// <param name="context">The authored Home scene.</param>
    public HomeOpeningRuntime(HomeScene context)
    {
        scene = context;
    }

    /// <summary>Requests navigation before executing the configured operation.</summary>
    public void Rescue()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (!entity.opening.canOperate && !entity.opening.fetchingWater)
            return;

        var target = entity.opening.fetchingWater ? entity.opening.GetWaterPoint() : entity.opening.GetRescuePoint();
        route = scene.CalculateRoute(target);
        corner = 0;
        rescuing = route.Length > 0;
    }

    /// <summary>Manual movement cancels navigation without consuming carried water.</summary>
    public void CancelMovement()
    {
        if (operationElapsed > 0f)
            XPluginUtility.PostEvent(new HomeOpeningOperationProgress { active = false });

        route = null;
        rescuing = false;
        operationElapsed = 0f;
    }

    /// <summary>Advances native movement and reports completion only after actual rescue.</summary>
    /// <param name="elapsed">Elapsed simulation seconds.</param>
    public void Tick(float elapsed)
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var opening = entity.opening;
        var actor = entity.FindActor(scene.GetPlayerId());
        if (opening.CollectWater(actor.position))
        {
            CancelMovement();
            actor.SetMoveInput(Vector3.zero);
            XPluginUtility.PostEvent(new HomeOpeningChanged());
        }

        scene.ShowOpening(opening);
        bool operating = rescuing && opening.CanOperate(actor.position);
        scene.ShowRescue(operating && opening.carryingWater);
        scene.ShowClearing(operating && opening.phase == HomeOpeningPhase.ClearDebris);
        if (!rescuing)
            return;

        if (!opening.CanOperate(actor.position))
        {
            operationElapsed = 0f;
            Move(actor, elapsed);

            return;
        }

        actor.SetMoveInput(Vector3.zero);
        operationElapsed += elapsed;
        XPluginUtility.PostEvent(new HomeOpeningOperationProgress {
            active = true,
            remaining = Mathf.Max(0f, opening.rescue.DurationSeconds - operationElapsed)
        });
        if (operationElapsed < opening.rescue.DurationSeconds)
            return;

        rescuing = false;
        operationElapsed = 0f;
        if (opening.CompleteOperation(actor.position))
        {
            scene.ShowRescue(false);
            scene.ShowClearing(false);
            scene.ShowOpening(opening);
            if (opening.taskCompleted)
                ReportCompletion(opening);

            XPluginUtility.PostEvent(new HomeOpeningChanged());
        }
    }

    /// <summary>Replays the idempotent observation when a saved rescue preceded task persistence.</summary>
    /// <param name="opening">The saved opening state.</param>
    public static void ReportCompletion(HomeOpening opening)
    {
        XPluginUtility.FireEvent(new TaskProgressReported {
            taskId = opening.rescue.TaskID,
            condition = Config.ETaskConditionType.FinishTalk,
            value = 1
        });

        if (opening.rescue.AutoClaim != 0)
            XPluginUtility.PostEvent(new TaskClaimRequested { taskId = opening.rescue.TaskID });
    }

    /// <summary>Feeds the existing ActorEntity movement adapter, never moving its Shape directly.</summary>
    /// <param name="actor">The authoritative playable actor.</param>
    /// <param name="elapsed">Elapsed simulation seconds.</param>
    private void Move(ActorEntity actor, float elapsed)
    {
        while (corner < route.Length)
        {
            Vector3 offset = route[corner] - actor.position;
            offset.y = 0f;
            if (offset.magnitude < 0.15f)
            {
                corner++;

                continue;
            }

            float step = actor.moveSpeed * elapsed;
            actor.SetMoveInput(offset.normalized * Mathf.Min(1f, offset.magnitude / Mathf.Max(step, 0.001f)));

            return;
        }

        actor.SetMoveInput(Vector3.zero);
        rescuing = false;
    }
}

/// <summary>Reports transient operation time without changing durable task progress.</summary>
public sealed class HomeOpeningOperationProgress : IEvent
{
    /// <summary>Whether the countdown is still being displayed.</summary>
    public bool active
    { get; set; }

    /// <summary>Remaining seconds during the active operation.</summary>
    public float remaining
    { get; set; }
}
