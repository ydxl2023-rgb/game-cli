using System;
using System.Linq;
using Newtonsoft.Json;
using Oathx.Core;
using Oathx.Entity;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AI;

/// <summary>Exercises player data, live input routing and presentation independently.</summary>
public static class MiniwowPlayerProbe
{
    /// <summary>Captured player position before a live movement interval.</summary>
    private static Vector3 before;

    /// <summary>Simulation time at the beginning of a measured drag.</summary>
    private static float measurementStart;

    /// <summary>Captured camera position before a live movement interval.</summary>
    private static Vector3 cameraBefore;

    /// <summary>Captured position for verifying explicit cancellation.</summary>
    private static Vector3 stopped;

    /// <summary>Reports live player and camera state without changing it.</summary>
    /// <returns>Serialized observations.</returns>
    public static string Snapshot()
    {
        ActorEntity actor = Player();
        ActorShape shape = Shape();
        SkeletonAnimation animation = shape.GetComponentInChildren<SkeletonAnimation>(true);
        Camera sceneCamera = UnityEngine.Object.FindObjectsOfType<Camera>().Single(item => item.GetComponent<HomeSceneCamera>());

        return JsonConvert.SerializeObject(new {
            id = actor.actorId,
            config = actor.configId,
            position = new[] { actor.position.x, actor.position.y, actor.position.z },
            phase = actor.phase.ToString(),
            animation = animation.AnimationState.GetCurrent(0)?.Animation.Name,
            trackTime = animation.AnimationState.GetCurrent(0)?.TrackTime,
            shapeActive = shape.gameObject.activeSelf,
            camera = new[] { sceneCamera.transform.position.x, sceneCamera.transform.position.y, sceneCamera.transform.position.z },
            navigationVertices = NavMesh.CalculateTriangulation().vertices.Length
        });
    }

    /// <summary>Begins a drag through the same scene methods called by mouse and touch.</summary>
    /// <param name="hideShape">Whether to hide the player presentation during movement.</param>
    /// <returns>Initial observation.</returns>
    public static string Begin(bool hideShape)
    {
        HomeScene scene = UnityEngine.Object.FindObjectOfType<HomeScene>();
        before = Player().position;
        measurementStart = Time.time;
        cameraBefore = UnityEngine.Object.FindObjectsOfType<Camera>().Single(item => item.GetComponent<HomeSceneCamera>()).transform.position;
        var point = new Vector2(Screen.width * 0.32f, Screen.height * 0.36f);
        Require(scene.BeginPointer(point, -1), "Gesture accepted outside UI");
        scene.MovePointer(point + new Vector2(0f, -80f * Screen.width / 1080f));
        if (hideShape)
            Shape().gameObject.SetActive(false);

        return Snapshot();
    }

    /// <summary>Drives live navigation for entrance acceptance without teleporting the actor.</summary>
    /// <param name="x">World horizontal intent.</param>
    /// <param name="z">World depth intent.</param>
    /// <returns>Whether the active runtime accepted input.</returns>
    public static bool MoveWorld(float x, float z)
    {
        var runtime = UnityEngine.Object.FindObjectOfType<HomeRuntime>();

        return runtime.SetPlayerInput(new Vector3(x, 0f, z));
    }

    /// <summary>Reports actual horizontal travel speed before ending a live drag.</summary>
    /// <returns>Measured movement and configured maximum.</returns>
    public static string MeasureSpeed()
    {
        var actor = Player();
        float elapsed = Time.time - measurementStart;
        float distance = Vector3.ProjectOnPlane(actor.position - before, Vector3.up).magnitude;

        return JsonConvert.SerializeObject(new {
            elapsed = elapsed,
            distance = distance,
            unitsPerSecond = distance / elapsed,
            configured = actor.moveSpeed
        });
    }

    /// <summary>Checks sustained movement and camera following, then cancels the gesture.</summary>
    /// <returns>Observations before stopping.</returns>
    public static string Stop()
    {
        ActorEntity actor = Player();
        Camera camera = UnityEngine.Object.FindObjectsOfType<Camera>().Single(item => item.GetComponent<HomeSceneCamera>());
        Require(Vector3.Distance(before, actor.position) > 0.1f, "Entity moved");
        Require(Vector3.Distance(cameraBefore, camera.transform.position) > 0.1f, "Camera followed logical player");
        string result = Snapshot();
        UnityEngine.Object.FindObjectOfType<HomeScene>().CancelPointer();
        Shape().gameObject.SetActive(true);
        Require(Shape().Bind(actor), "Shape restored without resetting player");
        stopped = actor.position;

        return result;
    }

    /// <summary>Verifies no retained input remains after releasing the pointer.</summary>
    /// <returns>Whether cancellation stopped both movement and animation.</returns>
    public static bool VerifyStopped()
    {
        Require(Player().position == stopped && Player().phase == ActorPhase.Idle, "No drift after cancellation");
        Require(Shape().GetComponentInChildren<SkeletonAnimation>().AnimationState.GetCurrent(0).Animation.Name == ActorAnimationNames.idle, "Idle animation restored");

        return true;
    }

    /// <summary>Checks gated movement and navigation boundaries without waiting for a frame.</summary>
    /// <returns>Whether all boundary and lifecycle assertions passed.</returns>
    public static bool VerifyRules()
    {
        MiniwowTownProbe.VerifyEntity();
        var config = Config.Excel.propActorById(Config.Excel.ACTOR_TOWN_PLAYER);
        var spawn = new ActorSpawn {
            actorId = 101,
            configId = config.ID,
            moveSpeed = config.MoveSpeed,
            route = new[] { Player().position },
            movement = new HomeNavigation(CreateAgent(Player().position, config.AgentTypeId, config.AgentHeight))
        };
        var actor = new ActorEntity();
        var other = new ActorEntity();
        Require(actor.Initialize(spawn) && other.Initialize(spawn), "Detached manual actors");
        Vector3 origin = actor.position;
        Require(!actor.SetMoveInput(new Vector3(float.NaN, 0f, 0f)), "Invalid input rejected");
        actor.SetMoveInput(Vector3.right * 100f);
        actor.Update(float.NaN);
        actor.Update(-1f);
        Require(actor.position == origin, "Invalid time ignored");
        actor.Update(0.1f);
        Require(Vector3.Distance(origin, actor.position) <= config.MoveSpeed * 0.1f + 0.01f, "Analog speed bounded");
        Require(other.position == origin && other.phase == ActorPhase.Idle, "Independent actor data");
        actor.SetMoveInput(Vector3.zero);
        Vector3 halt = actor.position;
        actor.Update(1f);
        Require(actor.position == halt && actor.phase == ActorPhase.Idle, "Explicit input release");
        var navigation = spawn.movement;
        bool edge = false;
        for (int index = 0; index < 8; index++)
        {
            Vector3 direction = Quaternion.Euler(0f, index * 45f, 0f) * Vector3.forward;
            Vector3 target = origin + direction * 500f;
            if (!NavMesh.Raycast(origin, target, out NavMeshHit hit, new NavMeshQueryFilter { agentTypeID = config.AgentTypeId, areaMask = NavMesh.AllAreas }))
                continue;

            Require(navigation.TryMove(origin, target, out Vector3 result), "Boundary adapter resolves");
            Require(NavMesh.SamplePosition(result, out NavMeshHit projected, 0.05f, new NavMeshQueryFilter { agentTypeID = config.AgentTypeId, areaMask = NavMesh.AllAreas }), "Boundary result remains on original navigation");
            edge = true;
        }

        Require(edge, "Navigation boundary was exercised");
        CleanupAgents();
        actor.Destroy();
        Require(!actor.SetMoveInput(Vector3.right), "Released actor rejects input");
        other.Destroy();
        HomeRuntime town = MiniwowTownProbe.GetRuntime();
        town.SetInputEnabled(false);
        Require(!town.SetPlayerInput(Vector3.right), "System rejects hidden-page movement");
        town.SetInputEnabled(true);

        return true;
    }

    /// <summary>Verifies fixed presentation and captured dragging across blocking UI.</summary>
    /// <returns>Whether UI hover preserves capture while release still stops movement.</returns>
    public static bool VerifyPointerCapture()
    {
        HomeScene scene = UnityEngine.Object.FindObjectOfType<HomeScene>();
        ActorJoystick joystick = UnityEngine.Object.FindObjectOfType<ActorJoystick>();
        RectTransform stick = (RectTransform)joystick.transform.Find("Stick");
        RectTransform knob = (RectTransform)stick.Find("Knob");
        Vector3 fixedPosition = stick.position;
        var origin = new Vector2(Screen.width * 0.32f, Screen.height * 0.36f);
        var graphic = UnityEngine.Object.FindObjectOfType<UIHome>().GetComponentsInChildren<UnityEngine.UI.Graphic>()
            .First(item => item.raycastTarget && item.isActiveAndEnabled && item.rectTransform.rect.width > 0f);
        Vector2 blocked = RectTransformUtility.WorldToScreenPoint(null, graphic.rectTransform.TransformPoint(graphic.rectTransform.rect.center));
        Require(!scene.BeginPointer(blocked, -1), "UI cannot begin a scene gesture");
        Require(stick.gameObject.activeInHierarchy, "Fixed joystick visible at rest");
        Require(scene.BeginPointer(origin, -1), "Scene captures mouse");
        scene.MovePointer(blocked);
        Require(!scene.BeginPointer(origin, 21), "Crossing UI retains original pointer ownership");
        Require(knob.anchoredPosition.sqrMagnitude > 0f && stick.position == fixedPosition, "Knob moves but base remains fixed over UI");
        scene.EndPointer(blocked, -1);
        Require(Player().phase == ActorPhase.Idle && knob.anchoredPosition == Vector2.zero, "Release over UI clears input");
        Require(stick.gameObject.activeInHierarchy && stick.position == fixedPosition, "Release preserves fixed resting control");

        return true;
    }

    /// <summary>Checks input release through focus, pause and pointer ownership paths.</summary>
    /// <returns>Whether cancellation leaves no retained movement.</returns>
    public static bool VerifyCancellation()
    {
        HomeScene scene = UnityEngine.Object.FindObjectOfType<HomeScene>();
        var point = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Require(scene.BeginPointer(point, 12), "Touch gesture acquired");
        Require(!scene.BeginPointer(point, 13), "Second pointer cannot replace owner");
        scene.MovePointer(point + Vector2.down * 40f);
        Require(!scene.EndPointer(point, 13), "Wrong pointer cannot release owner");
        scene.SendMessage("OnApplicationFocus", false);
        Vector3 origin = Player().position;
        Player().Update(0.1f);
        Require(Player().position == origin && Player().phase == ActorPhase.Idle, "Focus loss clears input");
        Require(scene.BeginPointer(point, 14), "Pointer can reacquire after cancellation");
        scene.MovePointer(point + Vector2.down * 40f);
        scene.SendMessage("OnApplicationPause", true);
        origin = Player().position;
        Player().Update(0.1f);
        Require(Player().position == origin && Player().phase == ActorPhase.Idle, "Pause clears input");

        return true;
    }

    /// <summary>Builds a temporary inclined navigation fixture to verify original horizontal speed.</summary>
    /// <returns>Whether a detached player can climb without exceeding its speed budget.</returns>
    public static bool VerifySlope()
    {
        var mesh = new Mesh();
        mesh.vertices = new[] { new Vector3(-10f, -5f, -10f), new Vector3(-10f, 5f, 10f), new Vector3(10f, 5f, 10f), new Vector3(10f, -5f, -10f) };
        mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        var offset = new Vector3(1000f, 1000f, 1000f);
        var sources = new System.Collections.Generic.List<NavMeshBuildSource> {
            new NavMeshBuildSource {
                shape = NavMeshBuildSourceShape.Mesh,
                sourceObject = mesh,
                transform = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one),
                area = 0
            }
        };
        NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(0);
        settings.agentSlope = 45f;
        settings.overrideVoxelSize = true;
        settings.voxelSize = 0.05f;
        NavMeshData data = NavMeshBuilder.BuildNavMeshData(settings, sources, new Bounds(offset, Vector3.one * 40f), Vector3.zero, Quaternion.identity);
        NavMeshDataInstance registration = NavMesh.AddNavMeshData(data);
        var actor = new ActorEntity();
        try
        {
            Require(NavMesh.SamplePosition(offset, out NavMeshHit start, 1f, NavMesh.AllAreas), "Slope fixture exists");
            var spawn = new ActorSpawn {
                actorId = 102,
                configId = Config.Excel.ACTOR_TOWN_PLAYER,
                moveSpeed = 1f,
                route = new[] { start.position },
                movement = new HomeNavigation(CreateAgent(start.position, settings.agentTypeID, settings.agentHeight))
            };
            Require(actor.Initialize(spawn) && actor.SetMoveInput(Vector3.forward), "Slope actor initialized");
            for (int index = 0; index < 10; index++)
            {
                Vector3 origin = actor.position;
                actor.Update(0.1f);
                Require(Vector3.ProjectOnPlane(actor.position - origin, Vector3.up).magnitude <= 0.101f, "Horizontal slope speed budget");
            }

            Require(actor.position.y > start.position.y + 0.1f && actor.position.z > start.position.z + 0.3f, "Actor climbed the slope");
        }
        finally
        {
            actor.Destroy();
            CleanupAgents();
            registration.Remove();
            UnityEngine.Object.DestroyImmediate(data);
            UnityEngine.Object.DestroyImmediate(mesh);
        }

        return true;
    }

    /// <summary>Renders the live scene and active canvases when desktop capture is stale.</summary>
    /// <returns>The native render output path; this is not a desktop screenshot.</returns>
    public static string RenderPreview()
    {
        Camera sceneCamera = UnityEngine.Object.FindObjectsOfType<Camera>().Single(item => item.GetComponent<HomeSceneCamera>());
        Canvas[] canvases = UnityEngine.Object.FindObjectsOfType<Canvas>().Where(item => item.isRootCanvas).ToArray();
        var modes = canvases.Select(item => item.renderMode).ToArray();
        var cameras = canvases.Select(item => item.worldCamera).ToArray();
        var planes = canvases.Select(item => item.planeDistance).ToArray();
        var target = new RenderTexture(Screen.width, Screen.height, 24);
        var uiObject = new GameObject("PlayerPreviewCamera");
        Camera uiCamera = uiObject.AddComponent<Camera>();
        uiCamera.clearFlags = CameraClearFlags.Depth;
        uiCamera.cullingMask = -1;
        uiCamera.targetTexture = target;
        RenderTexture previous = RenderTexture.active;
        RenderTexture priorTarget = sceneCamera.targetTexture;
        Texture2D image = null;
        try
        {
            sceneCamera.targetTexture = target;
            sceneCamera.Render();
            foreach (Canvas canvas in canvases)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = uiCamera;
                canvas.planeDistance = 1f;
            }

            Canvas.ForceUpdateCanvases();
            uiCamera.Render();
            RenderTexture.active = target;
            image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0f, 0f, target.width, target.height), 0, 0);
            image.Apply();
            System.IO.Directory.CreateDirectory("Library/Player");
            System.IO.File.WriteAllBytes("Library/Player/native-preview.png", image.EncodeToPNG());
        }
        finally
        {
            for (int index = 0; index < canvases.Length; index++)
            {
                canvases[index].renderMode = modes[index];
                canvases[index].worldCamera = cameras[index];
                canvases[index].planeDistance = planes[index];
            }

            sceneCamera.targetTexture = priorTarget;
            RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(uiObject);
            UnityEngine.Object.DestroyImmediate(target);
            if (image)
                UnityEngine.Object.DestroyImmediate(image);

            Canvas.ForceUpdateCanvases();
        }

        return "Library/Player/native-preview.png";
    }

    /// <summary>Creates a temporary original-style navigation proxy for detached logic tests.</summary>
    /// <param name="position">Initial surface position.</param>
    /// <param name="type">Navigation type matching the fixture.</param>
    /// <param name="height">Original agent height.</param>
    /// <returns>The active test proxy.</returns>
    private static NavMeshAgent CreateAgent(Vector3 position, int type, float height)
    {
        var root = new GameObject("PlayerProbeNavigation");
        root.SetActive(false);
        root.transform.position = position;
        NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
        agent.agentTypeID = type;
        agent.height = height;
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        root.SetActive(true);
        Require(agent.Warp(position), "Temporary proxy bound");

        return agent;
    }

    /// <summary>Releases verification-only agents without touching the player's proxy.</summary>
    private static void CleanupAgents()
    {
        foreach (NavMeshAgent agent in UnityEngine.Object.FindObjectsOfType<NavMeshAgent>().Where(item => item.name == "PlayerProbeNavigation"))
            UnityEngine.Object.DestroyImmediate(agent.gameObject);
    }

    /// <summary>Queries the authoritative playable instance.</summary>
    /// <returns>The live player entity.</returns>
    private static ActorEntity Player()
    {
        XEntityManager.GetSingleton().TryGetEntity(nameof(HomeEntity), out var entity);

        return ((HomeEntity)entity).FindActor(4);
    }

    /// <summary>Finds the authored player view for editor-only verification.</summary>
    /// <returns>The live or temporarily hidden player Shape.</returns>
    private static ActorShape Shape()
    {
        return Resources.FindObjectsOfTypeAll<ActorShape>().Single(item => item.name == "TownPlayer" && item.gameObject.scene.isLoaded);
    }

    /// <summary>Fails verification with a concrete invariant name.</summary>
    /// <param name="value">Observed result.</param>
    /// <param name="message">Expected invariant.</param>
    private static void Require(bool value, string message)
    {
        if (!value)
            throw new InvalidOperationException(message);
    }
}
