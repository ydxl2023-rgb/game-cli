using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Spine.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Imports selected original Spine data through Unity asset APIs.</summary>
[InitializeOnLoad]
public static class MiniwowTownSetup
{
    /// <summary>Explicit request consumed only outside play mode.</summary>
    private const string requestPath = "../tools/town/setup.request";

    /// <summary>Registers the editor request processor.</summary>
    static MiniwowTownSetup()
    {
        EditorApplication.update += Poll;
    }

    /// <summary>Processes a single explicit setup request.</summary>
    private static void Poll()
    {
        if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(requestPath))
            return;

        string action = File.ReadAllText(requestPath).Trim();
        File.Delete(requestPath);

        try
        {
            if (action == "app")
                EditorSceneManager.OpenScene(JsonConvert.DeserializeObject<Config.SceneConfig>(File.ReadAllText("Assets/Data/Jsons/Excels/SceneConfig.bytes")).data[Config.Excel.SCENE_APP].Path);
            else if (action == "original-navigation")
                MiniwowOriginalNavigation.Import();
            else if (action == "player")
                MiniwowPlayerSetup.Install();
            else if (action == "login")
                MiniwowLoginSetup.Bind();
            else if (action == "home-runtime")
                MigrateHomeRuntime();
            else if (action == "merge-home-entity")
                AssetDatabase.DeleteAsset("Assets/Scripts/Systems/Home/TownEntity.cs");
            else if (action == "legacy-text")
                MiniwowLoginSetup.EnableLegacyText();
            else if (action == "config-source")
                File.WriteAllText("../tools/login/config-source.json", MiniwowSessionProbe.ConfigureSource());
            else if (action == "install")
                Install();
            else
                Inspect();
        }
        catch (Exception exception)
        {
            File.WriteAllText("../tools/town/setup-error.txt", exception.ToString());
            Debug.LogException(exception);
        }
    }

    /// <summary>Moves scene execution into Home while preserving serialized script identities.</summary>
    private static void MigrateHomeRuntime()
    {
        const string source = "Assets/Scripts/Systems/Town/";
        const string destination = "Assets/Scripts/Systems/Home/";
        string[] files = { "TownSystem.cs", "TownEntity.cs", "TownScene.cs", "TownNavigation.cs" };
        string[] targets = { "HomeRuntime.cs", "TownEntity.cs", "Common/TownScene.cs", "Common/TownNavigation.cs" };
        for (int index = 0; index < files.Length; index++)
        {
            string error = AssetDatabase.MoveAsset(source + files[index], destination + targets[index]);
            if (!string.IsNullOrEmpty(error))
                throw new InvalidOperationException(error);
        }

        AssetDatabase.SaveAssets();
    }

    /// <summary>Imports skeletons and records authored town anchors without guessing coordinates.</summary>
    private static void Inspect()
    {
        string[] names = { "hero_20025", "npc_forgingtable_2", "npc_hotel" };
        var skeletons = names.Select(name => {
            SkeletonDataAsset asset = ImportSkeleton(name);
            var data = asset.GetSkeletonData(false);

            return new {
                name,
                version = data.Version,
                width = data.Width,
                height = data.Height,
                animations = data.Animations.Select(animation => animation.Name).ToArray()
            };
        }).ToArray();

        Scene prior = SceneManager.GetActiveScene();
        Scene scene = SceneManager.GetSceneByPath("Assets/Scenes/Home.unity");
        bool opened = !scene.isLoaded;
        if (opened)
            scene = EditorSceneManager.OpenScene("Assets/Scenes/Home.unity", OpenSceneMode.Additive);

        var anchors = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Where(item => item.name.Contains("NPC_") || item.name.Contains("Point") || item.name.Contains("WeaponStore") || item.name == "Hotel")
            .Select(item => new {
                name = item.name,
                position = new[] { item.position.x, item.position.y, item.position.z },
                rotation = new[] { item.eulerAngles.x, item.eulerAngles.y, item.eulerAngles.z },
                scale = new[] { item.lossyScale.x, item.lossyScale.y, item.lossyScale.z }
            }).ToArray();

        File.WriteAllText("../tools/town/inspection.json", JsonConvert.SerializeObject(new { skeletons, anchors }, Formatting.Indented));

        if (opened)
            EditorSceneManager.CloseScene(scene, true);

        EditorSceneManager.SetActiveScene(prior);
        AssetDatabase.SaveAssets();
    }

    /// <summary>Installs explicit scene bindings and original skeletons into the existing town.</summary>
    private static void Install()
    {
        Scene prior = SceneManager.GetActiveScene();
        Scene scene = SceneManager.GetSceneByPath("Assets/Scenes/Home.unity");
        bool opened = !scene.isLoaded;
        if (opened)
            scene = EditorSceneManager.OpenScene("Assets/Scenes/Home.unity", OpenSceneMode.Additive);

        EditorSceneManager.SetActiveScene(scene);
        GameObject previous = scene.GetRootGameObjects().SingleOrDefault(root => root.name == "TownRuntime");
        if (previous)
            UnityEngine.Object.DestroyImmediate(previous);

        GameObject recovered = scene.GetRootGameObjects().Single(root => root.name == "RecoveredBuildingScene");
        Transform[] transforms = recovered.GetComponentsInChildren<Transform>(true);
        Camera camera = recovered.GetComponentInChildren<Camera>(true);
        Transform hotel = transforms.Single(item => item.name == "NPC_hotel");
        Transform forge = transforms.Single(item => item.name == "NPC_forgingtable_2");

        InstallResident(hotel, "npc_hotel");
        InstallResident(forge, "npc_forgingtable_2");

        var rootObject = new GameObject("TownRuntime");
        HomeScene town = rootObject.AddComponent<HomeScene>();
        var visitorObject = new GameObject("TownVisitor");
        visitorObject.transform.SetParent(rootObject.transform, false);
        var visual = new GameObject("OriginalHeroVisual");
        visual.transform.SetParent(visitorObject.transform, false);
        visual.transform.rotation = hotel.rotation;
        visual.transform.localScale = Vector3.one * 2f;
        SkeletonAnimation animation = SkeletonAnimation.AddToGameObject(visual, ImportSkeleton("hero_20025"));
        animation.loop = true;
        animation.AnimationName = ActorAnimationNames.idle;

        ActorShape visitor = visitorObject.AddComponent<ActorShape>();
        var visitorHit = visitorObject.AddComponent<CapsuleCollider>();
        visitorHit.center = new Vector3(0f, 1.1f, 0f);
        visitorHit.height = 2.6f;
        visitorHit.radius = 0.9f;
        visitorHit.isTrigger = true;

        // Author a short street route; no claim of recovered navigation or service availability.
        Transform origin = transforms.Single(item => item.name == "FirstHeroPoint");
        Vector3 destination = hotel.position + new Vector3(-2.5f, 0.28f, 2.5f);
        Vector3[] positions = { origin.position, new Vector3(-26f, origin.position.y, 0f), destination };
        var route = new Transform[positions.Length];
        for (int index = 0; index < positions.Length; index++)
        {
            route[index] = new GameObject("VisitorWaypoint" + index).transform;
            route[index].SetParent(rootObject.transform, false);
            route[index].position = positions[index];
        }

        visitorObject.transform.position = origin.position;

        Vector3 focus = (origin.position + hotel.position) * 0.5f + Vector3.up * 2f;
        Vector3 forward = camera.transform.forward;
        camera.transform.parent.position = focus - forward * (focus.y / forward.y);

        ConfigureShape(visitor, animation, camera, ActorAnimationNames.walk);
        ActorShape hotelShape = hotel.GetComponent<ActorShape>() ?? hotel.gameObject.AddComponent<ActorShape>();
        ActorShape forgeShape = forge.GetComponent<ActorShape>() ?? forge.gameObject.AddComponent<ActorShape>();
        ConfigureShape(hotelShape, hotel.GetComponent<SkeletonAnimation>(), camera, "");
        ConfigureShape(forgeShape, forge.GetComponent<SkeletonAnimation>(), camera, "");

        var entrance = new GameObject("HotelInteraction");
        entrance.transform.SetParent(rootObject.transform, false);
        entrance.transform.position = hotel.position + Vector3.up;
        var buildingHit = entrance.AddComponent<BoxCollider>();
        buildingHit.size = new Vector3(3f, 3f, 3f);
        buildingHit.isTrigger = true;

        var building = Newtonsoft.Json.Linq.JArray.Parse(File.ReadAllText("../tools/main-scene/building-config.json"))
            .Single(item => ((string)item["npcModel"]).StartsWith("NPC_hotel/", StringComparison.OrdinalIgnoreCase));
        var sceneData = new SerializedObject(town);
        sceneData.FindProperty("sceneCamera").objectReferenceValue = camera;
        sceneData.FindProperty("cameraControls").objectReferenceValue = camera.GetComponent<HomeSceneCamera>();
        sceneData.FindProperty("visitorId").intValue = 1;
        SerializedProperty actors = sceneData.FindProperty("actors");
        actors.arraySize = 3;
        ConfigureActor(actors.GetArrayElementAtIndex(0), 1, 20025, (int)building["id"], visitor, route);
        ConfigureActor(actors.GetArrayElementAtIndex(1), 2, (int)building["id"], 0, hotelShape, new[] { hotel });
        ConfigureActor(actors.GetArrayElementAtIndex(2), 3, 60002, 0, forgeShape, new[] { forge });
        sceneData.FindProperty("buildingId").intValue = (int)building["id"];
        sceneData.FindProperty("buildingCollider").objectReferenceValue = buildingHit;
        sceneData.FindProperty("visitorCollider").objectReferenceValue = visitorHit;

        sceneData.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        File.WriteAllText("../tools/town/install-result.json", JsonConvert.SerializeObject(new {
            scene = scene.path,
            buildingId = (int)building["id"],
            npcId = 1,
            skeletons = new[] { "npc_hotel", "npc_forgingtable_2", "hero_20025" },
            route = positions.Select(position => new[] { position.x, position.y, position.z }).ToArray()
        }, Formatting.Indented));

        if (opened)
            EditorSceneManager.CloseScene(scene, true);

        EditorSceneManager.SetActiveScene(prior);
    }

    /// <summary>Writes native presentation references through Unity serialization.</summary>
    private static void ConfigureShape(ActorShape shape, SkeletonAnimation animation, Camera camera, string walk)
    {
        var data = new SerializedObject(shape);
        data.FindProperty("animationView").objectReferenceValue = animation;
        data.FindProperty("sceneCamera").objectReferenceValue = camera;
        data.FindProperty("walkAnimation").stringValue = walk;
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Writes one authored actor definition and its explicit view reference.</summary>
    private static void ConfigureActor(SerializedProperty actor, int id, int config, int destination, ActorShape shape, Transform[] route)
    {
        actor.FindPropertyRelative("actorId").intValue = id;
        actor.FindPropertyRelative("configId").intValue = config;
        actor.FindPropertyRelative("destinationId").intValue = destination;
        actor.FindPropertyRelative("moveSpeed").floatValue = 3.2f;
        actor.FindPropertyRelative("shape").objectReferenceValue = shape;
        SerializedProperty points = actor.FindPropertyRelative("route");
        points.arraySize = route.Length;
        for (int index = 0; index < route.Length; index++)
            points.GetArrayElementAtIndex(index).objectReferenceValue = route[index];
    }

    /// <summary>Restores a resident at its original authored transform.</summary>
    /// <param name="anchor">Recovered original NPC transform.</param>
    /// <param name="resource">Selected extracted skeleton directory.</param>
    private static void InstallResident(Transform anchor, string resource)
    {
        SkeletonDataAsset data = ImportSkeleton(resource);
        SkeletonAnimation animation = anchor.GetComponent<SkeletonAnimation>();
        if (!animation)
            animation = SkeletonAnimation.AddToGameObject(anchor.gameObject, data);

        animation.skeletonDataAsset = data;
        animation.Initialize(true);

        animation.loop = true;
        animation.AnimationName = ActorAnimationNames.idle;
        EditorUtility.SetDirty(animation);
    }

    /// <summary>Creates native atlas, material and skeleton assets without changing recovered input.</summary>
    /// <param name="name">Recovered resource directory name.</param>
    /// <returns>The imported skeleton data asset.</returns>
    public static SkeletonDataAsset ImportSkeleton(string name)
    {
        string directory = "Assets/Data/Scenes/Town/Spine/" + name;
        string texturePath = directory + "/" + name + ".png";
        string pageName = File.ReadLines(directory + "/" + name + ".atlas.txt").First(line => !string.IsNullOrWhiteSpace(line)).Trim();
        string pagePath = directory + "/" + pageName;
        Texture2D existingTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (existingTexture && existingTexture.name != Path.GetFileNameWithoutExtension(pageName))
        {
            string temporary = directory + "/RenamingTexture.png";
            string error = AssetDatabase.MoveAsset(texturePath, temporary);
            if (!string.IsNullOrEmpty(error))
                throw new InvalidOperationException(error);

            error = AssetDatabase.MoveAsset(temporary, pagePath);
            if (!string.IsNullOrEmpty(error))
                throw new InvalidOperationException(error);
        }

        texturePath = pagePath;
        var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
        importer.textureType = TextureImporterType.Default;
        importer.alphaIsTransparency = false;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.wrapMode = TextureWrapMode.Clamp;

        importer.SaveAndReimport();

        Material material = AssetDatabase.LoadAssetAtPath<Material>(directory + "/Skeleton.mat");
        if (!material)
        {
            material = new Material(Shader.Find("Spine/Skeleton"));
            AssetDatabase.CreateAsset(material, directory + "/Skeleton.mat");
        }

        material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        EditorUtility.SetDirty(material);

        SpineAtlasAsset atlas = AssetDatabase.LoadAssetAtPath<SpineAtlasAsset>(directory + "/Atlas.asset");
        if (!atlas)
        {
            atlas = SpineAtlasAsset.CreateRuntimeInstance(AssetDatabase.LoadAssetAtPath<TextAsset>(directory + "/" + name + ".atlas.txt"), new[] { material }, true);
            AssetDatabase.CreateAsset(atlas, directory + "/Atlas.asset");
        }

        atlas.Clear();

        SkeletonDataAsset skeleton = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(directory + "/Skeleton.asset");
        if (!skeleton)
        {
            skeleton = SkeletonDataAsset.CreateRuntimeInstance(AssetDatabase.LoadAssetAtPath<TextAsset>(directory + "/" + name + ".skel.bytes"), atlas, true);
            AssetDatabase.CreateAsset(skeleton, directory + "/Skeleton.asset");
        }

        return skeleton;
    }
}
