using System;
using System.Linq;
using Newtonsoft.Json;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Editor-only observations and real pointer-path checks for Town acceptance.</summary>
public static class MiniwowTownProbe
{
    /// <summary>Observes lifecycle, movement and original skeleton state.</summary>
    /// <returns>Serialized runtime evidence.</returns>
    public static string Snapshot()
    {
        XEntityManager.GetSingleton().TryGetEntity(nameof(TownEntity), out var entity);
        ActorEntity npc = (entity as TownEntity)?.FindActor(1);
        var town = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().GetTown();
        UISystem.GetSingleton().TryGetWidget<UIHome>(out var home);
        SkeletonAnimation[] skeletons = UnityEngine.Object.FindObjectsOfType<SkeletonAnimation>();

        return JsonConvert.SerializeObject(new {
            ready = town != null && town.IsReady(),
            sceneLoaded = SceneManager.GetSceneByPath("Assets/Scenes/Home.unity").isLoaded,
            activeScene = SceneManager.GetActiveScene().path,
            contexts = UnityEngine.Object.FindObjectsOfType<TownScene>().Length,
            visitors = npc ? 1 : 0,
            actors = (entity as TownEntity)?.actorCount ?? 0,
            shapes = UnityEngine.Object.FindObjectsOfType<ActorShape>().Length,
            home = home && home.gameObject.activeInHierarchy,
            phase = npc ? npc.phase.ToString() : "Absent",
            arrivals = npc ? npc.arrivals : -1,
            position = npc ? new[] { npc.position.x, npc.position.y, npc.position.z } : null,
            skeletons = skeletons.Select(value => new {
                name = value.name,
                valid = value.valid,
                animation = value.AnimationState?.GetCurrent(0)?.Animation.Name,
                trackTime = value.AnimationState?.GetCurrent(0)?.TrackTime,
                vertices = value.GetComponent<MeshFilter>().sharedMesh.vertexCount,
                texture = value.GetComponent<MeshRenderer>().sharedMaterial.mainTexture.name
            }).ToArray()
        });
    }

    /// <summary>Validates entity movement without constructing any scene object.</summary>
    /// <returns>Whether detached data and lifecycle invariants hold.</returns>
    public static bool VerifyEntity()
    {
        var points = new[] { Vector3.zero, Vector3.right, Vector3.right * 3f };
        var spawn = new ActorSpawn {
            actorId = 10,
            configId = 20025,
            destinationId = 60017,
            moveSpeed = 1f,
            route = points
        };
        var actor = new ActorEntity();
        Require(actor.Initialize(spawn) && !actor.Initialize(spawn), "Initialization contract");
        points[2] = Vector3.one * 999f;
        Require(!actor.Visit(-1) && actor.Visit(60017) && !actor.Visit(60017), "Visit validation");
        actor.Update(float.NaN);
        actor.Update(-1f);
        Require(actor.position == Vector3.zero, "Invalid elapsed time");
        actor.Update(3f);
        Require(actor.position == Vector3.right * 3f && actor.phase == ActorPhase.Waiting && actor.arrivals == 1, "Detached route completion");
        actor.Update(100f);
        Require(actor.arrivals == 1 && actor.Return() && !actor.Return(), "Exactly once arrival");
        for (int index = 0; index < 30; index++)
            actor.Update(0.1f);
        actor.Update(0.001f);
        Require(actor.position == Vector3.zero && actor.phase == ActorPhase.Idle, "Small timestep return");
        actor.Destroy();
        actor.Update(1f);
        Require(!actor.Visit(60017) && actor.phase == ActorPhase.Stopped, "Released actor");
        var town = new TownEntity();
        Require(!town.Initialize(new[] { spawn, spawn }) && town.actorCount == 0, "Atomic duplicate rejection");
        town.Destroy();

        return true;
    }

    /// <summary>Starts a live test while hiding the visitor's complete presentation object.</summary>
    /// <returns>The starting logical position.</returns>
    public static string HideMovingShape()
    {
        XEntityManager.GetSingleton().TryGetEntity(nameof(TownEntity), out var entity);
        ActorEntity actor = ((TownEntity)entity).FindActor(1);
        Require(actor.Visit(60017), "Start hidden actor");
        ActorShape shape = UnityEngine.Object.FindObjectsOfType<ActorShape>().Single(item => item.name == "TownVisitor");
        shape.gameObject.SetActive(false);

        return Snapshot();
    }

    /// <summary>Rebinds and recreates presentation while preserving authoritative entity data.</summary>
    /// <returns>Whether hidden simulation and view replacement preserved data.</returns>
    public static bool VerifyShapeSeparation()
    {
        XEntityManager.GetSingleton().TryGetEntity(nameof(TownEntity), out var entity);
        ActorEntity actor = ((TownEntity)entity).FindActor(1);
        Require(actor.phase == ActorPhase.Waiting && actor.arrivals == 1, "Hidden actor completed simulation");
        ActorShape shape = Resources.FindObjectsOfTypeAll<ActorShape>().Single(item => item.name == "TownVisitor" && item.gameObject.scene.isLoaded);
        Vector3 position = actor.position;
        shape.Unbind();
        shape.transform.position = Vector3.zero;
        Require(actor.position == position, "View cannot write entity");
        shape.gameObject.SetActive(true);
        Require(shape.Bind(actor) && shape.transform.position == position, "Rebind presents existing state");
        ActorShape replacement = UnityEngine.Object.Instantiate(shape);
        Require(replacement.Bind(actor) && replacement.transform.position == position, "Replacement shape");
        UnityEngine.Object.DestroyImmediate(replacement.gameObject);
        Require(actor.position == position && actor.arrivals == 1 && actor.phase == ActorPhase.Waiting, "Shape destruction cannot destroy data");
        Require(((TownEntity)entity).actorCount == 3, "All NPCs have entities");

        return true;
    }

    /// <summary>Raises a deterministic acceptance failure.</summary>
    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    /// <summary>Checks per-shape state transitions without changing the town actor collection.</summary>
    /// <returns>Whether state entry, stable refresh, release and rebind behave correctly.</returns>
    public static bool VerifyActorMachine()
    {
        ActorShape original = UnityEngine.Object.FindObjectsOfType<ActorShape>().Single(item => item.name == "TownVisitor");
        ActorShape shape = UnityEngine.Object.Instantiate(original);
        var actor = new ActorEntity();
        var spawn = new ActorSpawn {
            actorId = 99,
            configId = 20025,
            destinationId = 60017,
            moveSpeed = 1f,
            route = new[] { Vector3.zero, Vector3.right }
        };
        try
        {
            Require(actor.Initialize(spawn) && shape.Bind(actor), "Bind state machine");
            Require(shape is XActionStateMachine && shape.GetCurrentState().state.Equals(ActorPhase.Idle), "Idle state");
            SkeletonAnimation animation = shape.GetComponentInChildren<SkeletonAnimation>();
            var idle = animation.AnimationState.GetCurrent(0);
            shape.Refresh();
            Require(ReferenceEquals(idle, animation.AnimationState.GetCurrent(0)), "Stable state does not restart animation");
            Require(actor.Visit(60017), "Visit accepted");
            shape.Refresh();
            Require(shape.GetCurrentState().state.Equals(ActorPhase.Moving) && animation.AnimationState.GetCurrent(0).Animation.Name == ActorAnimationNames.walk, "Moving state");
            actor.Update(1f);
            shape.Refresh();
            Require(shape.GetCurrentState().state.Equals(ActorPhase.Waiting), "Waiting state");
            Require(actor.Return(), "Return accepted");
            shape.Refresh();
            Require(shape.GetCurrentState().state.Equals(ActorPhase.Returning), "Returning state");
            shape.Unbind();
            Require(shape.GetCurrentState() == null && actor.phase == ActorPhase.Returning, "Unbind only clears presentation");
            Require(shape.Bind(actor) && shape.GetCurrentState().state.Equals(ActorPhase.Returning), "Rebind current state");
            actor.Destroy();
            shape.Refresh();
            Require(shape.GetCurrentState().state.Equals(ActorPhase.Stopped) && animation.AnimationState.GetCurrent(0) == null, "Stopped state");
        }
        finally
        {
            actor.Destroy();
            UnityEngine.Object.DestroyImmediate(shape.gameObject);
        }

        return true;
    }

    /// <summary>Uses the same pointer methods as touch and mouse input.</summary>
    /// <param name="target">Building or visitor hit target.</param>
    /// <param name="drag">Whether to exceed the drag rejection threshold.</param>
    /// <returns>The accepted result and actual projected screen point.</returns>
    public static string Gesture(string target, bool drag)
    {
        TownScene scene = UnityEngine.Object.FindObjectOfType<TownScene>();
        var data = new SerializedObject(scene);
        Camera camera = (Camera)data.FindProperty("sceneCamera").objectReferenceValue;
        Collider collider = (Collider)data.FindProperty(target == "building" ? "buildingCollider" : "visitorCollider").objectReferenceValue;
        Vector2 point = camera.WorldToScreenPoint(collider.bounds.center);
        bool began = scene.BeginPointer(point, -1);
        if (drag)
            scene.MovePointer(point + Vector2.right * 100f);

        bool accepted = scene.EndPointer(point, -1);

        return JsonConvert.SerializeObject(new {
            began,
            accepted,
            point = new[] { point.x, point.y },
            state = JsonConvert.DeserializeObject(Snapshot())
        });
    }

    /// <summary>Requests lifecycle changes through the actual framework module.</summary>
    /// <param name="active">Whether the town should be active.</param>
    public static void SetActive(bool active)
    {
        var town = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().GetTown();
        if (active)
            town.Active();
        else
            town.Detive();
    }

    /// <summary>Exercises conflicting lifecycle requests while scene operations are in flight.</summary>
    public static void RapidRestart()
    {
        var town = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().GetTown();

        town.Detive();
        town.Active();
        town.Detive();
        town.Active();
    }

    /// <summary>Replaces the module while its old scene unload is still in flight.</summary>
    public static void ReplaceModule()
    {
        XCoroutine.Run(ReplaceAfterRemoval());
    }

    /// <summary>Checks invalid IDs without creating fallback objects.</summary>
    /// <returns>Whether invalid interactions were rejected.</returns>
    public static bool RejectInvalidIds()
    {
        var town = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().GetTown();

        return !town.VisitBuilding(-1) && !town.ReturnNpc(-1);
    }

    /// <summary>Respects the dispatcher's deferred module removal contract.</summary>
    /// <returns>The asynchronous module replacement.</returns>
    private static System.Collections.IEnumerator ReplaceAfterRemoval()
    {
        var plugin = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>();

        plugin.Unload<TownSystem>();
        while (plugin.GetTown() != null)
            yield return null;

        plugin.Load<TownSystem>();
    }
}
