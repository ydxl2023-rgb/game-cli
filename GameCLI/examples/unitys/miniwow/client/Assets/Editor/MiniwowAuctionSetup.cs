using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Installs the original auction rescue result without rebuilding Home.</summary>
[InitializeOnLoad]
public static class MiniwowAuctionSetup
{
    /// <summary>Explicit installation request owned by this feature.</summary>
    private const string requestPath = "../tools/opening/auction.request";

    /// <summary>Audited burned model export.</summary>
    private const string sourceDirectory = "../tools/main-scene/auction";

    /// <summary>Native dependencies for the burned auction variant.</summary>
    private const string assetDirectory = "Assets/Data/Scenes/Home/Opening/Auction";

    /// <summary>Registers the idle editor request handler.</summary>
    static MiniwowAuctionSetup()
    {
        EditorApplication.update += Poll;
    }

    /// <summary>Installs one audited model variant without replacing its scene anchor.</summary>
    /// <param name="data">The source node graph.</param>
    /// <param name="model">The original building Model anchor.</param>
    /// <param name="meshes">Native mesh dependencies.</param>
    /// <param name="materials">Native material dependencies.</param>
    public static void InstallModel(JObject data, Transform model,
        Dictionary<string, Mesh> meshes, Dictionary<string, Material> materials)
    {
        var existing = model.Find((string)data["nodes"][0]["name"]);
        if (existing)
            UnityEngine.Object.DestroyImmediate(existing.gameObject);

        var objects = new Dictionary<string, GameObject>();
        foreach (JObject node in data["nodes"])
            objects.Add((string)node["id"], new GameObject((string)node["name"]));

        foreach (JObject node in data["nodes"])
        {
            var target = objects[(string)node["id"]];
            string parent = (string)node["parent"];
            target.transform.SetParent(parent == "0" ? model : objects[parent].transform, false);
            target.transform.localPosition = ReadVector(node["position"]);
            target.transform.localScale = ReadVector(node["scale"]);
            var rotation = node["rotation"];
            target.transform.localRotation = new Quaternion(
                (float)rotation["x"], (float)rotation["y"], (float)rotation["z"], (float)rotation["w"]);
            target.SetActive((bool)node["active"]);
            if (node["mesh"] != null)
            {
                var filter = target.AddComponent<MeshFilter>();
                filter.sharedMesh = meshes[(string)node["mesh"]];
                var renderer = target.AddComponent<MeshRenderer>();
                renderer.sharedMaterials = node["materials"].Select(id => materials[(string)id]).ToArray();
                renderer.enabled = (bool)node["rendererEnabled"];
            }
        }

    }

    /// <summary>Processes an explicit request only outside play mode.</summary>
    private static void Poll()
    {
        if (!File.Exists(requestPath) || EditorApplication.isPlayingOrWillChangePlaymode
            || EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        File.Delete(requestPath);
        try
        {
            Install();
            File.WriteAllText("../tools/opening/auction-install.json", "{\"ok\":true}");
        }
        catch (Exception exception)
        {
            File.WriteAllText("../tools/opening/auction-install.json", new JObject {
                ["ok"] = false,
                ["error"] = exception.ToString()
            }.ToString());
            Debug.LogException(exception);
        }
    }

    /// <summary>Authors model variants, the merchant actor and explicit presentation references.</summary>
    private static void Install()
    {
        var data = JObject.Parse(File.ReadAllText(sourceDirectory + "/scene.json"));
        MiniwowMainSceneSetup.ImportAssets(data, sourceDirectory, assetDirectory, out var meshes, out var materials);

        var scenes = JObject.Parse(File.ReadAllText("Assets/Data/Jsons/Excels/SceneConfig.bytes"));
        var scene = EditorSceneManager.OpenScene((string)scenes["data"]["10002"]["Path"]);
        var nodes = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
        var home = scene.GetRootGameObjects().Select(root => root.GetComponent<HomeScene>()).Single(item => item);
        var model = nodes.Single(item => item.name == "AuctionHouse_60019").Find("Model");
        InstallModel(data, model, meshes, materials);

        var merchant = nodes.Single(item => item.name == "NPC_auction_2");
        var skeleton = MiniwowTownSetup.ImportSkeleton("npc_auction_2");
        var animation = merchant.GetComponent<SkeletonAnimation>();
        if (!animation)
            animation = SkeletonAnimation.AddToGameObject(merchant.gameObject, skeleton);

        animation.skeletonDataAsset = skeleton;
        animation.Initialize(true);
        animation.loop = true;
        animation.AnimationName = ActorAnimationNames.idle;

        var shape = merchant.GetComponent<ActorShape>();
        if (!shape)
            shape = merchant.gameObject.AddComponent<ActorShape>();

        var homeData = new SerializedObject(home);
        var shapeData = new SerializedObject(shape);
        shapeData.FindProperty("animationView").objectReferenceValue = animation;
        var camera = homeData.FindProperty("sceneCamera").objectReferenceValue;
        shapeData.FindProperty("sceneCamera").objectReferenceValue = camera;
        shapeData.FindProperty("walkAnimation").stringValue = "";
        shapeData.ApplyModifiedPropertiesWithoutUndo();

        var actors = homeData.FindProperty("actors");
        int index = 0;
        while (index < actors.arraySize
            && actors.GetArrayElementAtIndex(index).FindPropertyRelative("actorId").intValue != 5)
            index++;

        if (index == actors.arraySize)
            actors.arraySize++;

        var binding = actors.GetArrayElementAtIndex(index);
        binding.FindPropertyRelative("actorId").intValue = 5;
        binding.FindPropertyRelative("configId").intValue = 60019;
        binding.FindPropertyRelative("destinationId").intValue = 0;
        binding.FindPropertyRelative("moveSpeed").floatValue = 3.2f;
        binding.FindPropertyRelative("shape").objectReferenceValue = shape;
        var route = binding.FindPropertyRelative("route");
        route.arraySize = 1;
        route.GetArrayElementAtIndex(0).objectReferenceValue = merchant;
        homeData.ApplyModifiedPropertiesWithoutUndo();

        var opening = new SerializedObject(home.GetComponent<HomeOpeningScene>());
        var damaged = model.Find("auction_initial_1").gameObject;
        var restored = model.Find("auction_initial").gameObject;
        var guide = nodes.Single(item => item.name == "GuideObjs");
        opening.FindProperty("auctionDamaged").objectReferenceValue = damaged;
        opening.FindProperty("auctionRestored").objectReferenceValue = restored;
        opening.FindProperty("merchant").objectReferenceValue = shape;
        opening.FindProperty("wellAvailable").objectReferenceValue = guide.Find("Money1").gameObject;
        opening.FindProperty("wellCollected").objectReferenceValue = guide.Find("Money5").gameObject;
        opening.ApplyModifiedPropertiesWithoutUndo();
        damaged.SetActive(true);
        restored.SetActive(false);
        merchant.gameObject.SetActive(false);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    /// <summary>Retains the original transform coordinates.</summary>
    /// <param name="value">The exported source vector.</param>
    /// <returns>The original vector.</returns>
    private static Vector3 ReadVector(JToken value)
    {
        return new Vector3((float)value["x"], (float)value["y"], (float)value["z"]);
    }
}
