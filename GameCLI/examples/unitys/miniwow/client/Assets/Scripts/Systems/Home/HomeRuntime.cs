using System.Collections;
using Oathx.Bundle.Assets;
using Oathx.Core;
using Oathx.Entity;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Owns the town scene lifecycle and coordinates scene interaction.</summary>
public sealed class HomeRuntime : GameRuntime
{
    /// <summary>Scene execution for the configured opening rescue.</summary>
    private HomeOpeningRuntime opening;

    /// <summary>Scene execution for persistent hero recovery.</summary>
    private HomeVillageRuntime village;

    /// <summary>Serializes ownership transfer between old and newly loaded module instances.</summary>
    private static bool sceneOperationInProgress;

    /// <summary>Prevents overlapping load and unload operations.</summary>
    private bool transitioning;

    /// <summary>Authored scene bindings, valid only while the scene is owned.</summary>
    private HomeScene context;

    /// <summary>Whether this lifecycle owns the current HomeEntity actor collection.</summary>
    private bool ownsActors;

    /// <summary>Configured scene path retained until asynchronous cleanup finishes.</summary>
    private string homeScenePath;

    /// <summary>Configured application scene restored before Home is unloaded.</summary>
    private string appScenePath;

    /// <summary>Requests asynchronous activation of the town.</summary>
    /// <returns>Whether activation was accepted.</returns>
    public override bool Startup()
    {
        if (!ActorConfiguration.Valid(Config.Excel.propActorById(Config.Excel.ACTOR_TOWN_PLAYER)))
            return false;

        var homeConfig = Config.Excel.propSceneById(Config.Excel.SCENE_HOME);
        var appConfig = Config.Excel.propSceneById(Config.Excel.SCENE_APP);
        homeScenePath = homeConfig.Path;
        appScenePath = appConfig.Path;
        if (string.IsNullOrWhiteSpace(homeScenePath) || string.IsNullOrWhiteSpace(appScenePath))
            throw new System.InvalidOperationException("Home scene configuration requires valid scene paths.");

        return base.Startup();
    }

    /// <summary>Accepts a building interaction without changing building ownership or level.</summary>
    /// <param name="buildingId">Stable building instance identifier.</param>
    /// <returns>Whether the authored NPC route was started.</returns>
    public bool VisitBuilding(int buildingId)
    {
        if (!IsReady())
            return false;

        var entity = GetHomeEntity();
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

        var entity = GetHomeEntity();
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
        if (!context
            || (!IsReady()
            && direction != Vector3.zero)
            || (!context.AcceptsInput()
            && direction != Vector3.zero))
            return false;

        var entity = GetHomeEntity();
        var actor = entity.FindActor(context.GetPlayerId());

        if (direction != Vector3.zero)
        {
            opening.CancelMovement();
            village.CancelNavigation();
        }

        return actor && actor.SetMoveInput(direction);
    }

    /// <summary>Starts hospital navigation using the existing player movement adapter.</summary>
    public void NavigateHospital()
    {
        village.NavigateHospital();
    }

    /// <summary>Connects the world entry to this runtime's authored scene references.</summary>
    /// <param name="widget">Initialized entry widget.</param>
    public void BindHospitalEntry(UIHomeHospitalEntry widget)
    {
        widget.BindScene(context);
    }

    /// <summary>Starts the requested auction interaction.</summary>
    public void Rescue()
    {
        opening.Rescue();
    }

    /// <summary>Enables scene gestures only while the camp presentation is visible.</summary>
    /// <param name="enabled">Whether the Home presentation permits scene input.</param>
    public void SetInputEnabled(bool enabled)
    {
        if (IsReady() && !enabled)
            village.CancelNavigation();

        if (context)
            context.SetInputEnabled(enabled && IsReady());
    }

    /// <summary>Starts scene initialization through the serialized scene worker.</summary>
    protected override void OnLoadEnter()
    {
        RequestTransition();
    }

    /// <summary>Reports that the authored scene and actors are available.</summary>
    protected override void OnReadyEnter()
    {
        opening = new HomeOpeningRuntime(context);
        village = new HomeVillageRuntime(context);
        Debug.Log("MINIWOW_TOWN_READY");
    }

    /// <summary>Advances opening execution while the scene is ready.</summary>
    /// <param name="deltaTime">Elapsed simulation seconds.</param>
    protected override void OnReadyUpdate(float deltaTime)
    {
        opening.Tick(deltaTime);
        village.Tick(deltaTime);
    }

    /// <summary>Stops gestures immediately before asynchronous scene release.</summary>
    protected override void OnUnloadEnter()
    {
        if (context)
            context.Release();

        ReleaseActors();
        RequestTransition();
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
            while (true)
            {
                Scene scene = SceneManager.GetSceneByPath(homeScenePath);
                if (GetCurrentState().state.Equals(RuntimeState.Unload))
                {
                    if (scene.isLoaded)
                    {
                        Scene app = SceneManager.GetSceneByPath(appScenePath);
                        if (app.isLoaded)
                            SceneManager.SetActiveScene(app);

                        yield return SceneManager.UnloadSceneAsync(scene);
                    }

                    context = null;
                    SetState(RuntimeState.Stopped);

                    yield break;
                }

                if (!scene.isLoaded)
                {
                    yield return SceneManager.LoadSceneAsync(homeScenePath, LoadSceneMode.Additive);
                    scene = SceneManager.GetSceneByPath(homeScenePath);
                }

                if (GetCurrentState().state.Equals(RuntimeState.Unload))
                    continue;

                if (!scene.isLoaded)
                {
                    Debug.LogError("MINIWOW_TOWN_SCENE_LOAD_FAILED");

                    yield break;
                }

                var entity = GetHomeEntity();
                if (!entity.IsInitialized())
                {
                    TextAsset configuration = null;

                    yield return XRes.LoadAsync<TextAsset>(
                        "Assets/Data/Config/Home.json",
                        asset => configuration = asset as TextAsset);

                    if (GetCurrentState().state.Equals(RuntimeState.Unload))
                        continue;

                    entity.Initialize(configuration.text);
                }

                // Discover only the explicit scene root contract; all internal references are serialized.
                context = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (!root.TryGetComponent<HomeScene>(out var candidate))
                        continue;

                    if (context)
                        throw new System.InvalidOperationException("Multiple HomeScene roots are not supported.");

                    context = candidate;
                }

                if (!context || !entity.InitializeActors(context.CreateActorDefinitions()))
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
                SetState(RuntimeState.Ready);

                yield break;
            }
        }
        finally
        {
            sceneOperationInProgress = false;
            transitioning = false;
        }
    }

    /// <summary>Queries the framework-owned collection without retaining a duplicate state reference.</summary>
    /// <returns>The collection installed before Home activation.</returns>
    private static HomeEntity GetHomeEntity()
    {
        return XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
    }

    /// <summary>Clears only the actor collection owned by this lifecycle.</summary>
    private void ReleaseActors()
    {
        if (!ownsActors)
            return;

        var entity = GetHomeEntity();
        entity.ClearActors();

        ownsActors = false;
    }
}
