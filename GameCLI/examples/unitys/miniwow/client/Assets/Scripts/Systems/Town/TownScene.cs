using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>Owns serialized scene references and routes pointer intent to TownSystem.</summary>
public sealed class TownScene : MonoBehaviour
{
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

    /// <summary>The system that explicitly initialized this scene instance.</summary>
    private TownSystem owner;

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

    /// <summary>Validates scene bindings and initializes the scene visitor.</summary>
    /// <param name="system">The explicit scene lifecycle owner.</param>
    /// <param name="entity">The initialized logical actor collection.</param>
    /// <returns>Whether this scene can accept interaction.</returns>
    public bool Initialize(TownSystem system, TownEntity entity)
    {
        if (system == null || !entity || !sceneCamera || !cameraControls || !buildingCollider || !visitorCollider || buildingId <= 0 || !entity.FindActor(visitorId) || actors == null)
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
        if (!player || !ActorConfiguration.Valid(playerConfig) || !joystick || !joystick.Initialize(playerConfig.JoystickRadius, playerConfig.JoystickSourceWidth) || !sceneCamera.transform.parent)
        {
            Release();

            return false;
        }

        cameraOffset = sceneCamera.transform.parent.position - player.position;
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
                spawn.movement = new TownNavigation(playerNavigation);
            }

            definitions[index] = spawn;
        }

        return definitions;
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

        pressed = inputEnabled && owner != null && owner.IsReady() && sceneCamera.pixelRect.Contains(position) && !IsBlocked(position);
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
        bool hitBuilding = buildingCollider.Raycast(ray, out RaycastHit buildingHit, sceneCamera.farClipPlane);
        bool hitVisitor = visitorCollider.Raycast(ray, out RaycastHit visitorHit, sceneCamera.farClipPlane);
        if (hitVisitor && (!hitBuilding || visitorHit.distance < buildingHit.distance))
            return owner.ReturnNpc(visitorId);

        if (hitBuilding)
            return owner.VisitBuilding(buildingId);

        return false;
    }

    /// <summary>Stops both camera and interaction gestures behind full-page presentation.</summary>
    /// <param name="value">Whether scene gestures are enabled.</param>
    public void SetInputEnabled(bool value)
    {
        CancelPointer();
        inputEnabled = value;

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
        handle.position = Vector3.Lerp(handle.position, player.position + cameraOffset, 1f - Mathf.Exp(-playerConfig.FollowRate * Time.deltaTime));
        joystick.Follow(player.position, sceneCamera);
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
    public bool Bind(TownEntity entity)
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
