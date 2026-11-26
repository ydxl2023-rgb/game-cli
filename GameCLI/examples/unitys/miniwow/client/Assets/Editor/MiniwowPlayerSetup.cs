using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Spine.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Installs the playable actor using native scene, navigation and prefab APIs.</summary>
public static class MiniwowPlayerSetup
{
    /// <summary>Installs the configured model, original navigation and imported joystick.</summary>
    public static void Install()
    {
        var table = JsonConvert.DeserializeObject<Config.ActorConfig>(File.ReadAllText("Assets/Data/Jsons/Excels/ActorConfig.bytes"));
        Config.Actor config = table.data[Config.Excel.ACTOR_TOWN_PLAYER];
        if (!ActorConfiguration.Valid(config))
            throw new InvalidOperationException("Invalid player configuration.");

        var scenes = JsonConvert.DeserializeObject<Config.SceneConfig>(File.ReadAllText("Assets/Data/Jsons/Excels/SceneConfig.bytes"));
        string path = scenes.data[Config.Excel.SCENE_HOME].Path;
        Scene prior = SceneManager.GetActiveScene();
        Scene scene = SceneManager.GetSceneByPath(path);
        bool opened = !scene.isLoaded;
        if (opened)
            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

        EditorSceneManager.SetActiveScene(scene);
        GameObject recovered = scene.GetRootGameObjects().Single(item => item.name == "RecoveredBuildingScene");
        HomeScene town = scene.GetRootGameObjects().Select(item => item.GetComponent<HomeScene>()).Single(item => item);
        Camera camera = recovered.GetComponentInChildren<Camera>(true);
        var spawn = new Vector3(config.SpawnX, config.SpawnY, config.SpawnZ);
        NavMeshData data = MiniwowOriginalNavigation.Import();
        NavMeshDataInstance registration = NavMesh.AddNavMeshData(data);
        var filter = new NavMeshQueryFilter {
            agentTypeID = config.AgentTypeId,
            areaMask = NavMesh.AllAreas
        };
        bool validSpawn = NavMesh.SamplePosition(spawn, out NavMeshHit spawnHit, 2f, filter);
        int vertices = NavMesh.CalculateTriangulation().vertices.Length;
        registration.Remove();
        if (!validSpawn || vertices == 0)
            throw new InvalidOperationException("Player spawn is outside the original navigation surface.");

        Transform oldNavigation = town.transform.Find("PlayerNavigation");
        if (oldNavigation)
            UnityEngine.Object.DestroyImmediate(oldNavigation.gameObject);

        var navigationObject = new GameObject("PlayerNavigation");
        navigationObject.SetActive(false);
        navigationObject.transform.SetParent(town.transform, false);
        navigationObject.transform.position = spawnHit.position;
        NavMeshAgent agent = navigationObject.AddComponent<NavMeshAgent>();
        agent.agentTypeID = config.AgentTypeId;
        agent.radius = config.AgentRadius;
        agent.height = config.AgentHeight;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.enabled = false;
        navigationObject.SetActive(true);

        Transform previous = town.transform.Find("TownPlayer");
        if (previous)
            UnityEngine.Object.DestroyImmediate(previous.gameObject);

        var playerObject = new GameObject("TownPlayer");
        playerObject.transform.SetParent(town.transform, false);
        playerObject.transform.position = spawnHit.position;
        var visual = new GameObject("OriginalPlayerVisual");
        visual.transform.SetParent(playerObject.transform, false);
        visual.transform.rotation = Quaternion.Euler(30f, -135f, 0f);
        visual.transform.localScale = Vector3.one * config.ModelScale;
        SkeletonDataAsset skeleton = MiniwowTownSetup.ImportSkeleton(config.Model);
        var skeletonData = skeleton.GetSkeletonData(false);
        if (skeletonData.FindAnimation(ActorAnimationNames.idle) == null || skeletonData.FindAnimation(ActorAnimationNames.run) == null)
            throw new InvalidOperationException("Required player animations are missing.");

        SkeletonAnimation animation = SkeletonAnimation.AddToGameObject(visual, skeleton);
        animation.loop = true;
        animation.AnimationName = ActorAnimationNames.idle;
        ActorShape shape = playerObject.AddComponent<ActorShape>();
        var shapeData = new SerializedObject(shape);
        shapeData.FindProperty("animationView").objectReferenceValue = animation;
        shapeData.FindProperty("sceneCamera").objectReferenceValue = camera;
        shapeData.FindProperty("walkAnimation").stringValue = ActorAnimationNames.run;
        shapeData.ApplyModifiedPropertiesWithoutUndo();

        const string prefabPath = "Assets/Data/Widgets/Actor/UIActorJoystick.prefab";
        GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
        ActorJoystick joystick = prefab.GetComponent<ActorJoystick>() ?? prefab.AddComponent<ActorJoystick>();
        RectTransform surface = prefab.GetComponent<RectTransform>();
        RectTransform stick = (RectTransform)prefab.transform.Find("Stick");
        RectTransform knob = (RectTransform)stick.Find("Knob");
        stick.anchorMin = stick.anchorMax = new Vector2(0.5f, 0f);
        stick.pivot = new Vector2(0.5f, 0.5f);
        stick.localScale = Vector3.one * 0.8f;
        // Original MainPage pivot (359,1219) in 720x1560, scaled by width and anchored to the bottom.
        stick.anchoredPosition = new Vector2(-1.5f, 511.5f);
        foreach (RectTransform child in stick)
        {
            child.anchorMin = child.anchorMax = new Vector2(0.5f, 0.5f);
            child.pivot = new Vector2(0.5f, 0.5f);
            child.anchoredPosition = Vector2.zero;
        }

        // Preserve the original base's authored (21,21) offset and 0.8 scale at a 1.5 UI conversion.
        ((RectTransform)stick.Find("Base")).anchoredPosition = new Vector2(-0.6f, 0.6f);
        RectTransform direction = (RectTransform)prefab.transform.Find("Direction");
        direction.anchorMin = direction.anchorMax = new Vector2(0.5f, 0.5f);
        direction.pivot = new Vector2(0.5f, 0f);
        direction.anchoredPosition = Vector2.zero;
        direction.gameObject.SetActive(false);

        foreach (Graphic graphic in prefab.GetComponentsInChildren<Graphic>(true))
            graphic.raycastTarget = false;

        var joystickData = new SerializedObject(joystick);
        joystickData.FindProperty("surface").objectReferenceValue = surface;
        joystickData.FindProperty("stick").objectReferenceValue = stick;
        joystickData.FindProperty("knob").objectReferenceValue = knob;
        joystickData.FindProperty("directionMarker").objectReferenceValue = direction;
        joystickData.ApplyModifiedPropertiesWithoutUndo();
        stick.gameObject.SetActive(false);
        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefab);

        Transform oldCanvas = town.transform.Find("PlayerInputCanvas");
        if (oldCanvas)
            UnityEngine.Object.DestroyImmediate(oldCanvas.gameObject);

        var canvasObject = new GameObject("PlayerInputCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(town.transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0f;
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath), canvasObject.transform);

        var sceneData = new SerializedObject(town);
        sceneData.FindProperty("playerId").intValue = 4;
        sceneData.FindProperty("joystick").objectReferenceValue = instance.GetComponent<ActorJoystick>();
        sceneData.FindProperty("navigationData").objectReferenceValue = data;
        sceneData.FindProperty("playerNavigation").objectReferenceValue = agent;
        SerializedProperty actors = sceneData.FindProperty("actors");
        actors.arraySize = 4;
        SerializedProperty actor = actors.GetArrayElementAtIndex(3);
        actor.FindPropertyRelative("actorId").intValue = 4;
        actor.FindPropertyRelative("configId").intValue = config.ID;
        actor.FindPropertyRelative("destinationId").intValue = 0;
        actor.FindPropertyRelative("moveSpeed").floatValue = config.MoveSpeed;
        actor.FindPropertyRelative("shape").objectReferenceValue = shape;
        SerializedProperty route = actor.FindPropertyRelative("route");
        route.arraySize = 1;
        route.GetArrayElementAtIndex(0).objectReferenceValue = playerObject.transform;
        sceneData.ApplyModifiedPropertiesWithoutUndo();

        Vector3 focus = spawnHit.position + Vector3.up * 3f;
        Vector3 forward = camera.transform.forward;
        camera.transform.parent.position = focus - forward * (focus.y / forward.y);
        camera.GetComponent<HomeSceneCamera>().enabled = false;
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        File.WriteAllText("../tools/player/install-result.json", JsonConvert.SerializeObject(new {
            scene = scene.path,
            actorId = 4,
            configId = config.ID,
            model = config.Model,
            animations = skeletonData.Animations.Select(item => item.Name).ToArray(),
            navigationSource = "Original NavMesh-mesh, 30 unmodified tiles",
            navigationVertices = vertices,
            spawn = new[] { spawnHit.position.x, spawnHit.position.y, spawnHit.position.z }
        }, Formatting.Indented));

        if (opened)
            EditorSceneManager.CloseScene(scene, true);

        EditorSceneManager.SetActiveScene(prior);
    }
}
