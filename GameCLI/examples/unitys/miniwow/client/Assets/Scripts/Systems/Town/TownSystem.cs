using System.Collections;
using Oathx.Core;
using Oathx.Entity;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Announces a change in the town scene's readiness.</summary>
public sealed class TownReadyChanged : IEvent
{
    /// <summary>Whether the scene and its authored bindings are ready.</summary>
    public bool ready
    { get; set; }
}

/// <summary>Owns the town scene lifecycle and coordinates scene interaction.</summary>
public sealed class TownSystem : RuntimeSystem
{
    /// <summary>Serializes ownership transfer between old and newly loaded module instances.</summary>
    private static bool sceneOperationInProgress;

    /// <summary>Latest requested lifecycle state.</summary>
    private bool requestedActive;

    /// <summary>Prevents overlapping load and unload operations.</summary>
    private bool transitioning;

    /// <summary>Invalidates initialization after a newer lifecycle request.</summary>
    private int generation;

    /// <summary>Authored scene bindings, valid only while the scene is owned.</summary>
    private TownScene context;

    /// <summary>Whether initialization completed for the current lifecycle.</summary>
    private bool ready;

    /// <summary>Whether this lifecycle owns the current TownEntity actor collection.</summary>
    private bool ownsActors;

    /// <summary>Requests asynchronous activation of the town.</summary>
    /// <returns>Whether activation was accepted.</returns>
    public override bool OnActive()
    {
        if (!IsActive())
            return false;

        if (requestedActive)
            return true;

        requestedActive = true;
        generation++;

        RequestTransition();

        return true;
    }

    /// <summary>Stops scene input and motion before requesting scene unloading.</summary>
    public override void OnDetive()
    {
        requestedActive = false;
        generation++;

        UnsubscribeAll();

        if (context)
            context.Release();

        ReleaseActors();

        SetReady(false);
        RequestTransition();
    }

    /// <summary>Reports readiness without exposing scene references.</summary>
    /// <returns>Whether the active town can accept interaction.</returns>
    public bool IsReady()
    {
        return requestedActive && ready && context;
    }

    /// <summary>Accepts a building interaction without changing building ownership or level.</summary>
    /// <param name="buildingId">Stable building instance identifier.</param>
    /// <returns>Whether the authored NPC route was started.</returns>
    public bool VisitBuilding(int buildingId)
    {
        if (!IsReady())
            return false;

        var entity = GetTownEntity();
        if (!entity)
            return false;

        var actor = entity.FindActor(context.GetVisitorId());
        if (!actor)
            return false;

        return actor.Visit(buildingId);
    }

    /// <summary>Returns the visitor along the same authored route.</summary>
    /// <param name="npcId">Stable scene visitor identifier.</param>
    /// <returns>Whether return movement was accepted.</returns>
    public bool ReturnNpc(int npcId)
    {
        if (!IsReady())
            return false;

        var entity = GetTownEntity();
        if (!entity)
            return false;

        var actor = entity.FindActor(npcId);
        if (!actor)
            return false;

        return actor.Return();
    }

    /// <summary>Routes player intent to its authoritative entity and honors presentation gates.</summary>
    /// <param name="direction">World-space analog intent; zero always releases movement.</param>
    /// <returns>Whether the actor accepted the input.</returns>
    public bool SetPlayerInput(Vector3 direction)
    {
        if (!context || (!IsReady() && direction != Vector3.zero) || (!context.AcceptsInput() && direction != Vector3.zero))
            return false;

        var entity = GetTownEntity();
        var actor = entity ? entity.FindActor(context.GetPlayerId()) : null;

        return actor && actor.SetMoveInput(direction);
    }

    /// <summary>Enables scene gestures only while the camp presentation is visible.</summary>
    /// <param name="enabled">Whether the Home presentation permits scene input.</param>
    public void SetInputEnabled(bool enabled)
    {
        if (context)
            context.SetInputEnabled(enabled && IsReady());
    }

    /// <summary>Resets the cross-instance gate even when domain reload is disabled.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSceneGate()
    {
        sceneOperationInProgress = false;
    }

    /// <summary>Starts the single scene transition worker.</summary>
    private void RequestTransition()
    {
        if (transitioning)
            return;

        transitioning = true;

        XCoroutine.Run(Reconcile());
    }

    /// <summary>Converges scene ownership to the latest requested state.</summary>
    /// <returns>The serialized scene transition.</returns>
    private IEnumerator Reconcile()
    {
        while (sceneOperationInProgress)
            yield return null;

        sceneOperationInProgress = true;

        try
        {
            if (Config.Excel.sceneConfig == null)
            {
                Debug.LogError("MINIWOW_SCENE_CONFIG_NOT_LOADED");

                yield break;
            }

            var homeConfig = Config.Excel.propSceneById(Config.Excel.SCENE_HOME);
            var appConfig = Config.Excel.propSceneById(Config.Excel.SCENE_APP);
            if (!homeConfig || !appConfig || string.IsNullOrWhiteSpace(homeConfig.Path) || string.IsNullOrWhiteSpace(appConfig.Path))
            {
                Debug.LogError("MINIWOW_SCENE_CONFIG_INVALID");

                yield break;
            }

            while (true)
            {
                int ticket = generation;
                Scene scene = SceneManager.GetSceneByPath(homeConfig.Path);
                if (!requestedActive)
                {
                    if (scene.isLoaded)
                    {
                        Scene app = SceneManager.GetSceneByPath(appConfig.Path);
                        if (app.isLoaded)
                            SceneManager.SetActiveScene(app);

                        yield return SceneManager.UnloadSceneAsync(scene);
                    }

                    context = null;
                    if (ticket != generation)
                        continue;

                    yield break;
                }

                if (!scene.isLoaded)
                {
                    yield return SceneManager.LoadSceneAsync(homeConfig.Path, LoadSceneMode.Additive);
                    scene = SceneManager.GetSceneByPath(homeConfig.Path);
                }

                if (ticket != generation)
                    continue;

                if (!scene.isLoaded)
                {
                    Debug.LogError("MINIWOW_TOWN_SCENE_LOAD_FAILED");

                    yield break;
                }

                // Discover only the explicit scene root contract; all internal references are serialized.
                context = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (!root.TryGetComponent<TownScene>(out var candidate))
                        continue;

                    if (context)
                        throw new System.InvalidOperationException("Multiple TownScene roots are not supported.");

                    context = candidate;
                }

                var entity = GetTownEntity();
                if (!entity)
                    entity = XEntityManager.GetSingleton().CreateEntity<TownEntity>();

                if (!context || !entity.Initialize(context.CreateActorDefinitions()))
                {
                    Debug.LogError("MINIWOW_TOWN_BINDINGS_INVALID");

                    yield break;
                }

                ownsActors = true;
                if (!context.Initialize(this, entity))
                {
                    context.Release();
                    ReleaseActors();
                    Debug.LogError("MINIWOW_TOWN_ACTOR_BINDINGS_INVALID");

                    yield break;
                }

                SceneManager.SetActiveScene(scene);
                SetReady(true);
                Debug.Log("MINIWOW_TOWN_READY");

                yield break;
            }
        }
        finally
        {
            sceneOperationInProgress = false;
            transitioning = false;
        }
    }

    /// <summary>Publishes readiness only when its value changes.</summary>
    /// <param name="value">The new readiness state.</param>
    private void SetReady(bool value)
    {
        if (ready == value)
            return;

        ready = value;

        XPluginUtility.PostEvent(new TownReadyChanged { ready = value });
    }

    /// <summary>Queries the framework-owned collection without retaining a duplicate state reference.</summary>
    /// <returns>The town entity, or null before its first creation.</returns>
    private static TownEntity GetTownEntity()
    {
        return XEntityManager.GetSingleton().TryGetEntity(nameof(TownEntity), out var entity) ? entity as TownEntity : null;
    }

    /// <summary>Clears only the actor collection owned by this lifecycle.</summary>
    private void ReleaseActors()
    {
        if (!ownsActors)
            return;

        var entity = GetTownEntity();
        if (entity)
            entity.Clear();

        ownsActors = false;
    }
}
