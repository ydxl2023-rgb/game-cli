using Oathx.Entity;
using Oathx.Core;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>Executes original recruitment walks without owning recruitment rules or task outcomes.</summary>
public sealed class HomeRecruitRuntime
{
    /// <summary>Scene-only presentation bindings.</summary>
    private readonly HomeRecruitScene scene;

    /// <summary>Actors whose entry walk was issued during this scene attachment.</summary>
    private readonly HashSet<int> routes = new HashSet<int>();

    /// <summary>Actual arrival notifications already emitted during this attachment.</summary>
    private readonly HashSet<int> arrivals = new HashSet<int>();

    /// <summary>Binds the authored presentation.</summary>
    /// <param name="context">Original actor and entrance mounts.</param>
    public HomeRecruitRuntime(HomeRecruitScene context)
    {
        scene = context;
    }

    /// <summary>Starts each configured entry walk once and projects visibility.</summary>
    public void Tick()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        int index = 0;
        foreach (var definition in Config.Excel.recruitHeroConfig.Values)
        {
            var step = Config.Excel.recruitStepConfig[definition.TaskID];
            bool owned = home.village.heroes.Contains(definition.ID);
            bool available = owned || tasks.IsClaimed(step.RequiredTaskID);
            scene.ShowActor(index++, available);
            var actor = home.FindActor(definition.ActorID);
            if (available && actor.phase == ActorPhase.Waiting && arrivals.Add(definition.ID))
                XPluginUtility.FireEvent(new RecruitActorArrived { heroId = definition.ID });

            if (!available || !routes.Add(definition.ActorID) || owned)
                continue;

            var target = new Vector3(definition.EndX, definition.EndY, definition.EndZ);
            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(actor.position, target, NavMesh.AllAreas, path)
                || path.status != NavMeshPathStatus.PathComplete || !actor.FollowRoute(path.corners))
                throw new System.InvalidOperationException("Recruit entry route is outside original navigation.");
        }
    }
}
