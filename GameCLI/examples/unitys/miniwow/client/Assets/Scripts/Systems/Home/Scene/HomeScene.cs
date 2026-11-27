using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>Owns serialized scene references and routes pointer intent to HomeRuntime.</summary>
public sealed class HomeScene : MonoBehaviour
{
    /// <summary>Original recruitment presentation owned by this scene.</summary>
    [SerializeField]
    private HomeRecruitScene recruitment;

    /// <summary>Authored opening effects and objective marker.</summary>
    [SerializeField]
    private HomeOpeningScene openingScene;

    /// <summary>Original hospital interaction anchor.</summary>
    [SerializeField]
    private Transform hospitalAnchor;

    /// <summary>Original building tips point used by its NPC title.</summary>
    [SerializeField]
    private Transform hospitalTitleAnchor;

    /// <summary>Original hospital pointer collider.</summary>
    [SerializeField]
    private Collider hospitalCollider;

    /// <summary>Recovered hospital ground selection effect.</summary>
    [SerializeField]
    private GameObject hospitalSelection;

    /// <summary>Original entrance selections for the other restored residents.</summary>
    [SerializeField]
    private HomeNpcSelection[] npcSelections;

    /// <summary>Authored playable actor instance identity.</summary>
    [SerializeField]
    private int playerId;

    /// <summary>Fixed-position joystick presentation.</summary>
    [SerializeField]
    private ActorJoystick joystick;

    /// <summary>Original recovered town navigation data.</summary>
    [SerializeField]
    private NavMeshData navigationData;

    /// <summary>Native movement proxy kept separate from the player's Shape.</summary>
    [SerializeField]
    private NavMeshAgent playerNavigation;

    /// <summary>Owned registration removed only when the scene is destroyed.</summary>
    private NavMeshDataInstance navigationInstance;

    /// <summary>Player data read by camera presentation, never modified by it.</summary>
    private ActorEntity player;

    /// <summary>Validated configuration for input projection and camera smoothing.</summary>
    private Config.Actor playerConfig;

    /// <summary>Camera offset established once when binding the player.</summary>
    private Vector3 cameraOffset;

    /// <summary>Optional facility framing while an interior page owns the camera.</summary>
    private Vector3? facilityCameraPosition;

    /// <summary>Whether an accepted gesture owns the joystick.</summary>
    private bool gestureActive;

    /// <summary>Camera used for scene raycasts.</summary>
    [SerializeField]
    private Camera sceneCamera;

    /// <summary>Existing camera gesture component, disabled with scene input.</summary>
    [SerializeField]
    private HomeSceneCamera cameraControls;

    /// <summary>The visitor controlled by the initial town interaction.</summary>
    [SerializeField]
    private int visitorId;

    /// <summary>Explicit authoring bindings for logical actors and their views.</summary>
    [SerializeField]
    private TownActorBinding[] actors;

    /// <summary>Stable building instance identifier from the recovered building table.</summary>
    [SerializeField]
    private int buildingId;

    /// <summary>Authored building entrance hit target.</summary>
    [SerializeField]
    private Collider buildingCollider;

    /// <summary>Authored visitor hit target.</summary>
    [SerializeField]
    private Collider visitorCollider;

    /// <summary>Pointer movement threshold in pixels at the portrait design width.</summary>
    [SerializeField]
    private float clickThreshold = 12f;

    /// <summary>The runtime that explicitly initialized this scene instance.</summary>
    private HomeRuntime owner;

    /// <summary>Initial position of the accepted pointer gesture.</summary>
    private Vector2 pressPosition;

    /// <summary>Whether a pointer gesture remains eligible to click.</summary>
    private bool pressed;

    /// <summary>Whether the current Home presentation permits scene gestures.</summary>
    private bool inputEnabled;

    /// <summary>Identity of the only accepted touch or mouse gesture.</summary>
    private int pointerId;

    /// <summary>Reusable UI hit list, preventing clicks through modal pages.</summary>
    private readonly List<RaycastResult> uiHits = new List<RaycastResult>();

    /// <summary>Returns authored recruitment presentation to its runtime owner.</summary>
    /// <returns>Scene-local recruitment mounts and shapes.</returns>
    public HomeRecruitScene GetRecruitment()
    {
        return recruitment;
    }

    /// <summary>Frames a resident in the upper page viewport or resumes player following.</summary>
    /// <param name="buildingId">Resident identity, or zero to restore player following.</param>
    public void FocusFacility(int buildingId)
    {
        if (buildingId == 0)
        {
            facilityCameraPosition = null;

            return;
        }

        var resident = GetResident(buildingId);
        Vector3 focus = resident.GetNavigationPosition();
        float depth = sceneCamera.WorldToViewportPoint(focus).z;
        Vector3 framed = sceneCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.84f, depth));
        facilityCameraPosition = sceneCamera.transform.parent.position + focus - framed;
    }

    /// <summary>Validates scene bindings and initializes the scene visitor.</summary>
    /// <param name="system">The explicit scene lifecycle owner.</param>
    /// <param name="entity">The initialized logical actor collection.</param>
    /// <returns>Whether this scene can accept interaction.</returns>
    public bool Initialize(HomeRuntime system, HomeEntity entity)
    {
        if (system == null
            || !entity
            || !sceneCamera
            || !cameraControls
            || !buildingCollider
            || !visitorCollider
            || buildingId <= 0
            || !entity.FindActor(visitorId)
            || actors == null)
            return false;

        foreach (TownActorBinding binding in actors)
        {
            if (binding == null || !binding.Bind(entity))
            {
                Release();

                return false;
            }
        }

        player = entity.FindActor(playerId);
        playerConfig = Config.Excel.propActorById(Config.Excel.ACTOR_TOWN_PLAYER);
        if (!player
            || !ActorConfiguration.Valid(playerConfig)
            || !joystick
            || !joystick.Initialize(playerConfig.JoystickRadius, playerConfig.JoystickSourceWidth)
            || !sceneCamera.transform.parent)
        {
            Release();

            return false;
        }

        Vector3 focus = player.position + Vector3.up * 3f;
        Vector3 forward = sceneCamera.transform.forward;
        Vector3 handlePosition = focus - forward * (focus.y / forward.y);
        cameraOffset = handlePosition - player.position;
        sceneCamera.transform.parent.position = handlePosition;
        owner = system;
        SetInputEnabled(false);

        return true;
    }

    /// <summary>Copies authored spawn values without retaining scene references in entities.</summary>
    /// <returns>Detached definitions, or null for invalid bindings.</returns>
    public ActorSpawn[] CreateActorDefinitions()
    {
        if (actors == null)
            return null;

        if (!navigationInstance.valid)
        {
            if (!navigationData)
                return null;

            navigationInstance = NavMesh.AddNavMeshData(navigationData);
        }

        var config = Config.Excel.propActorById(Config.Excel.ACTOR_TOWN_PLAYER);
        if (!ActorConfiguration.Valid(config) || !playerNavigation)
            return null;

        var definitions = new ActorSpawn[actors.Length];
        var home = Oathx.Entity.XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var village = home.village;
        for (int index = 0; index < actors.Length; index++)
        {
            ActorSpawn spawn = actors[index]?.CreateDefinition();
            if (spawn != null && spawn.actorId == playerId)
            {
                var origin = new Vector3(config.SpawnX, config.SpawnY, config.SpawnZ);

                var filter = new NavMeshQueryFilter {
                    agentTypeID = config.AgentTypeId,
                    areaMask = NavMesh.AllAreas
                };

                if (!NavMesh.SamplePosition(origin, out NavMeshHit hit, 2f, filter))
                    return null;

                playerNavigation.updatePosition = false;
                playerNavigation.updateRotation = false;
                playerNavigation.updateUpAxis = false;
                playerNavigation.enabled = true;
                if (!playerNavigation.Warp(hit.position))
                    return null;

                spawn.configId = config.ID;
                spawn.moveSpeed = config.MoveSpeed;
                spawn.route = new[] { hit.position };
                spawn.movement = new HomeNavigation(playerNavigation);
            }

            if (spawn.actorId == village.configuration.ActorID)
            {
                spawn.configId = village.configuration.HeroID;
                if (village.recovery >= HeroRecoveryPhase.Healing)
                {
                    var target = new Vector3(village.configuration.HospitalX,
                        village.configuration.HospitalY, village.configuration.HospitalZ);
                    if (village.exploration == ExplorePhase.Traveling
                        || village.exploration == ExplorePhase.Executing
                        || village.exploration == ExplorePhase.Returning)
                    {
                        var journey = Config.Excel.exploreJourneyConfig[village.journeyId];
                        target = new Vector3(journey.ExitX, journey.ExitY, journey.ExitZ);
                    }

                    var filter = new NavMeshQueryFilter {
                        agentTypeID = config.AgentTypeId,
                        areaMask = NavMesh.AllAreas
                    };

                    if (!NavMesh.SamplePosition(target, out var hospital, 3f, filter))
                        throw new System.InvalidOperationException(
                            "Hero service or exploration spawn is outside original navigation.");

                    spawn.route = new[] { hospital.position };
                }
            }

            foreach (var recruit in Config.Excel.recruitHeroConfig.Values)
            {
                if (spawn.actorId == recruit.ActorID && village.heroes.Contains(recruit.ID))
                    spawn.route = new[] { new Vector3(recruit.EndX, recruit.EndY, recruit.EndZ) };
            }

            definitions[index] = spawn;
        }

        return definitions;
    }

    /// <summary>Calculates a complete route using the original navigation data.</summary>
    /// <param name="target">The recovered interaction point.</param>
    /// <returns>Native corners, or an empty array if unreachable.</returns>
    public Vector3[] CalculateRoute(Vector3 target)
    {
        var path = new NavMeshPath();
        if (!playerNavigation.CalculatePath(target, path) || path.status != NavMeshPathStatus.PathComplete)
            return System.Array.Empty<Vector3>();

        return path.corners;
    }

    /// <summary>Resolves an NPC route against the same original navigation used by the player.</summary>
    /// <param name="origin">Authoritative NPC position.</param>
    /// <param name="target">Configured hospital destination.</param>
    /// <returns>A complete path, or an empty array when the route is unavailable.</returns>
    public Vector3[] CalculateActorRoute(Vector3 origin, Vector3 target)
    {
        var filter = new NavMeshQueryFilter {
            agentTypeID = playerNavigation.agentTypeID,
            areaMask = playerNavigation.areaMask
        };

        var path = new NavMeshPath();
        if (!NavMesh.SamplePosition(origin, out var start, 3f, filter)
            || !NavMesh.SamplePosition(target, out var end, 3f, filter)
            || !NavMesh.CalculatePath(start.position, end.position, filter, path)
            || path.status != NavMeshPathStatus.PathComplete)
            return System.Array.Empty<Vector3>();

        return path.corners;
    }

    /// <summary>Projects committed village state and disables the superseded prototype visitor interaction.</summary>
    /// <param name="opening">Original rescue presentation phase.</param>
    /// <param name="village">Committed building and hero state.</param>
    public void ShowVillage(HomeOpening opening, HomeVillageEntity village)
    {
        openingScene.ShowVillage(opening, village);
        buildingCollider.enabled = false;
        visitorCollider.enabled = false;
    }

    /// <summary>Projects opening progress into serialized scene presentation.</summary>
    /// <param name="opening">The entity-owned rescue progress.</param>
    public void ShowOpening(HomeOpening opening)
    {
        openingScene.Invalidate(opening);
    }

    /// <summary>Displays the active water stream from the current player.</summary>
    /// <param name="active">Whether water is being applied.</param>
    public void ShowRescue(bool active)
    {
        openingScene.ShowRescue(active, player.position);
    }

    /// <summary>Displays the owned debris dust only while clearing is actually executing.</summary>
    /// <param name="active">Whether the player is clearing the hospital beam.</param>
    public void ShowClearing(bool active)
    {
        openingScene.ShowClearing(active);
    }

    /// <summary>Returns the authored visitor instance identity.</summary>
    /// <returns>The visitor identity.</returns>
    public int GetVisitorId()
    {
        return visitorId;
    }

    /// <summary>Releases input and view bindings without owning entity lifetime.</summary>
    public void Release()
    {
        SetInputEnabled(false);
        owner = null;
        player = null;
        if (playerNavigation)
            playerNavigation.enabled = false;
        if (actors != null)
        {
            foreach (TownActorBinding binding in actors)
                binding?.Unbind();
        }
    }

    /// <summary>Begins a pointer gesture only outside blocking UI.</summary>
    /// <param name="position">Screen position.</param>
    /// <param name="id">Mouse or touch identifier.</param>
    /// <returns>Whether the gesture was accepted.</returns>
    public bool BeginPointer(Vector2 position, int id)
    {
        if (gestureActive)
            return false;

        pressed = inputEnabled
            && owner != null
            && owner.IsReady()
            && sceneCamera.pixelRect.Contains(position)
            && !IsBlocked(position);
        pressPosition = position;
        pointerId = id;

        gestureActive = pressed;
        if (gestureActive)
            joystick.Begin(position);

        return pressed;
    }

    /// <summary>Cancels click eligibility after dragging beyond the threshold.</summary>
    /// <param name="position">Current screen position.</param>
    public void MovePointer(Vector2 position)
    {
        if (!gestureActive)
            return;

        if (!inputEnabled)
        {
            CancelPointer();

            return;
        }

        Vector2 intent = joystick.Move(position);
        Vector3 forward = Vector3.ProjectOnPlane(sceneCamera.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(sceneCamera.transform.right, Vector3.up).normalized;
        owner.SetPlayerInput(forward * intent.y + right * (intent.x * playerConfig.HorizontalScale));

        float threshold = clickThreshold * Screen.width / 1080f;
        if ((position - pressPosition).sqrMagnitude > threshold * threshold)
            pressed = false;
    }

    /// <summary>Ends a valid gesture and dispatches its authored scene target.</summary>
    /// <param name="position">Release screen position.</param>
    /// <param name="id">Mouse or touch identifier.</param>
    /// <returns>Whether an interaction was accepted by the system.</returns>
    public bool EndPointer(Vector2 position, int id)
    {
        if (id != pointerId)
            return false;

        MovePointer(position);
        bool accepted = pressed && inputEnabled && id == pointerId && owner != null && owner.IsReady();
        CancelPointer();
        if (!accepted || !sceneCamera.pixelRect.Contains(position) || IsBlocked(position))
            return false;

        Ray ray = sceneCamera.ScreenPointToRay(position);
        if (hospitalCollider.Raycast(ray, out _, sceneCamera.farClipPlane))
        {
            Oathx.Core.XPluginUtility.PostEvent(new ReqHomeHospitalVisit());

            return true;
        }

        bool hitBuilding = buildingCollider.Raycast(ray, out RaycastHit buildingHit, sceneCamera.farClipPlane);
        bool hitVisitor = visitorCollider.Raycast(ray, out RaycastHit visitorHit, sceneCamera.farClipPlane);
        if (hitVisitor && (!hitBuilding || visitorHit.distance < buildingHit.distance))
            return owner.ReturnNpc(visitorId);

        if (hitBuilding)
            return owner.VisitBuilding(buildingId);

        return false;
    }

    /// <summary>Routes the player to the entrance separately from the visiting hero's resting point.</summary>
    /// <returns>Navigation corners to the authored hospital entrance.</returns>
    public Vector3[] CalculateHospitalRoute()
    {
        return CalculateRoute(hospitalCollider.transform.position);
    }

    /// <summary>Routes to an authored resident using the original navigation surface.</summary>
    /// <param name="buildingId">Original resident building identity.</param>
    /// <returns>A complete route, or an empty array when no reachable entrance exists.</returns>
    public Vector3[] CalculateResidentRoute(int buildingId)
    {
        var resident = GetResident(buildingId);
        if (!NavMesh.SamplePosition(resident.GetNavigationPosition(), out var ground, 3f, NavMesh.AllAreas))
            return System.Array.Empty<Vector3>();

        return CalculateRoute(ground.position);
    }

    /// <summary>Tests the player's original agent radius against the original entrance collider.</summary>
    /// <returns>Whether the player overlaps the hospital interaction area.</returns>
    public bool IsPlayerAtHospital()
    {
        Vector3 closest = hospitalCollider.ClosestPoint(player.position);

        return (closest - player.position).sqrMagnitude <= playerConfig.AgentRadius * playerConfig.AgentRadius;
    }

    /// <summary>Shows the world-space selection only while this entrance is usable.</summary>
    /// <param name="visible">Whether to display the original selection effect.</param>
    public void ShowHospitalSelection(bool visible)
    {
        if (hospitalSelection.activeSelf != visible)
            hospitalSelection.SetActive(visible);
    }

    /// <summary>Projects the authored hospital bubble anchor.</summary>
    /// <returns>The screen position including camera depth.</returns>
    public Vector3 GetHospitalScreenPoint()
    {
        return sceneCamera.WorldToScreenPoint(hospitalAnchor.position);
    }

    /// <summary>Projects the original NPC title offset independently of the enter button.</summary>
    /// <returns>The screen position of the original title mount.</returns>
    public Vector3 GetHospitalTitleScreenPoint()
    {
        return sceneCamera.WorldToScreenPoint(hospitalTitleAnchor.position + Vector3.down * 0.5f);
    }

    /// <summary>Projects a restored resident's original title mount.</summary>
    /// <param name="index">Resident index in the authored scene bindings.</param>
    /// <returns>The screen position including camera depth.</returns>
    public Vector3 GetNpcTitleScreenPoint(int index)
    {
        return sceneCamera.WorldToScreenPoint(npcSelections[index].GetTitlePosition());
    }

    /// <summary>Applies scene interaction and resident visibility to a title.</summary>
    /// <param name="index">Resident index in the authored scene bindings.</param>
    /// <returns>Whether the resident title may be shown.</returns>
    public bool IsNpcTitleVisible(int index)
    {
        return inputEnabled && npcSelections[index].IsVisible();
    }

    /// <summary>Projects an original building entrance button independently of its title.</summary>
    /// <param name="buildingId">Original building configuration identity.</param>
    /// <returns>The screen position including camera depth.</returns>
    public Vector3 GetNpcEntryScreenPoint(int buildingId)
    {
        var resident = GetResident(buildingId);

        return sceneCamera.WorldToScreenPoint(resident.GetEntryPosition());
    }

    /// <summary>Shows entrances only while the visible resident is within interaction range.</summary>
    /// <param name="buildingId">Original building configuration identity.</param>
    /// <returns>Whether the entrance accepts a visit.</returns>
    public bool IsNpcEntryVisible(int buildingId)
    {
        var resident = GetResident(buildingId);

        return inputEnabled && resident.IsVisible()
            && resident.IsNearby(player.position, playerConfig.AgentRadius, playerConfig.AgentHeight);
    }

    /// <summary>Projects the logical player position for item flights.</summary>
    /// <returns>The screen position including camera depth.</returns>
    public Vector3 GetPlayerScreenPoint()
    {
        return sceneCamera.WorldToScreenPoint(player.position);
    }

    /// <summary>Stops both camera and interaction gestures behind full-page presentation.</summary>
    /// <param name="value">Whether scene gestures are enabled.</param>
    public void SetInputEnabled(bool value)
    {
        CancelPointer();
        inputEnabled = value;

        if (!value)
        {
            foreach (var selection in npcSelections)
                selection.Hide();
        }

        if (joystick)
            joystick.SetVisible(value);

        if (cameraControls)
            cameraControls.enabled = false;
    }

    /// <summary>Stops logical input and releases the captured pointer.</summary>
    public void CancelPointer()
    {
        pressed = false;
        gestureActive = false;
        if (owner != null)
            owner.SetPlayerInput(Vector3.zero);

        if (joystick)
            joystick.ResetInput();
    }

    /// <summary>Returns the authored playable actor identity.</summary>
    /// <returns>The player instance identifier.</returns>
    public int GetPlayerId()
    {
        return playerId;
    }

    /// <summary>Reports whether this scene accepts new movement intent.</summary>
    /// <returns>Whether interaction is enabled.</returns>
    public bool AcceptsInput()
    {
        return inputEnabled && isActiveAndEnabled;
    }

    /// <summary>Samples one pointer and rejects multitouch or canceled gestures.</summary>
    private void Update()
    {
        if (owner == null)
            return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (Input.touchCount > 1 || touch.phase == TouchPhase.Canceled)
                CancelPointer();
            else if (gestureActive && touch.fingerId != pointerId)
                CancelPointer();
            else if (touch.phase == TouchPhase.Began)
                BeginPointer(touch.position, touch.fingerId);
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                MovePointer(touch.position);
            else if (touch.phase == TouchPhase.Ended)
                EndPointer(touch.position, touch.fingerId);

            return;
        }

        if (gestureActive && pointerId >= 0)
            CancelPointer();

        if (Input.GetMouseButtonDown(0))
            BeginPointer(Input.mousePosition, -1);

        if (Input.GetMouseButton(0))
            MovePointer(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
            EndPointer(Input.mousePosition, -1);
    }

    /// <summary>Cancels pending gestures when focus leaves the application.</summary>
    /// <param name="focused">Whether the application has focus.</param>
    private void OnApplicationFocus(bool focused)
    {
        if (!focused)
            CancelPointer();
    }

    /// <summary>Tracks logical position even when the player's Shape is hidden.</summary>
    private void LateUpdate()
    {
        if (!player || player.phase == ActorPhase.Stopped || !sceneCamera || playerConfig == null)
            return;

        Transform handle = sceneCamera.transform.parent;
        handle.position = Vector3.Lerp(
            handle.position,
            facilityCameraPosition ?? player.position + cameraOffset,
            1f - Mathf.Exp(-playerConfig.FollowRate * Time.deltaTime));
        joystick.Follow(player.position, sceneCamera);
        foreach (var selection in npcSelections)
            selection.Refresh(player.position, playerConfig.AgentRadius, playerConfig.AgentHeight, inputEnabled);
    }

    /// <summary>Cancels captured movement when the application is suspended.</summary>
    /// <param name="paused">Whether the application is suspended.</param>
    private void OnApplicationPause(bool paused)
    {
        if (paused)
            CancelPointer();
    }

    /// <summary>Removes only this scene's owned navigation registration.</summary>
    private void OnDestroy()
    {
        if (navigationInstance.valid)
            navigationInstance.Remove();
    }

    /// <summary>Releases references if the scene is externally disabled.</summary>
    private void OnDisable()
    {
        Release();
    }

    /// <summary>Finds the authored resident by its stable building identity.</summary>
    /// <param name="buildingId">Original building configuration identity.</param>
    /// <returns>The required scene binding.</returns>
    private HomeNpcSelection GetResident(int buildingId)
    {
        foreach (var resident in npcSelections)
        {
            if (resident.Matches(buildingId))
                return resident;
        }

        throw new System.InvalidOperationException("Missing resident binding: " + buildingId);
    }

    /// <summary>Tests current graphic raycasts instead of relying on stale pointer hover state.</summary>
    /// <param name="position">Screen position to test.</param>
    /// <returns>Whether any active UI graphic blocks this pointer.</returns>
    private bool IsBlocked(Vector2 position)
    {
        if (!EventSystem.current)
            return false;

        uiHits.Clear();
        var pointer = new PointerEventData(EventSystem.current) { position = position };

        EventSystem.current.RaycastAll(pointer, uiHits);

        return uiHits.Count > 0;
    }
}

/// <summary>Converts scene authoring into detached actor data and binds its view.</summary>
[System.Serializable]
public sealed class TownActorBinding
{
    /// <summary>Stable actor instance identity.</summary>
    [SerializeField]
    private int actorId;

    /// <summary>Source configuration identity.</summary>
    [SerializeField]
    private int configId;

    /// <summary>Destination building identity, or zero for a resident.</summary>
    [SerializeField]
    private int destinationId;

    /// <summary>Movement speed in world units per second.</summary>
    [SerializeField]
    private float moveSpeed = 3.2f;

    /// <summary>Authoring anchors copied once at scene initialization.</summary>
    [SerializeField]
    private Transform[] route;

    /// <summary>The explicitly authored presentation component.</summary>
    [SerializeField]
    private ActorShape shape;

    /// <summary>Copies authoring coordinates into an independent definition.</summary>
    /// <returns>The definition, or null when an anchor is missing.</returns>
    public ActorSpawn CreateDefinition()
    {
        if (route == null || !shape)
            return null;

        var positions = new Vector3[route.Length];
        for (int index = 0; index < route.Length; index++)
        {
            if (!route[index])
                return null;

            positions[index] = route[index].position;
        }

        return new ActorSpawn {
            actorId = actorId,
            configId = configId,
            destinationId = destinationId,
            moveSpeed = moveSpeed,
            route = positions
        };
    }

    /// <summary>Binds the view to the collection's matching instance.</summary>
    /// <param name="entity">Owning town data.</param>
    /// <returns>Whether binding succeeded.</returns>
    public bool Bind(HomeEntity entity)
    {
        return shape && shape.Bind(entity.FindActor(actorId));
    }

    /// <summary>Clears only the presentation binding.</summary>
    public void Unbind()
    {
        if (shape)
            shape.Unbind();
    }



}
