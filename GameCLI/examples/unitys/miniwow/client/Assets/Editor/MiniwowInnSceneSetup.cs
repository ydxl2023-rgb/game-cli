using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Imports the recovered constructed inn and binds its persistent scene presentation.</summary>
[InitializeOnLoad]
public static class MiniwowInnSceneSetup
{
    /// <summary>Registers explicit edit-mode authoring requests.</summary>
    static MiniwowInnSceneSetup()
    {
        EditorApplication.update += OnUpdate;
    }

    /// <summary>Consumes a scene-only request without changing unrelated UI assets.</summary>
    private static void OnUpdate()
    {
        const string request = "../tools/inn-ui/scene.request";
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling
            || EditorApplication.isUpdating || !File.Exists(request))
            return;

        File.Delete(request);
        try
        {
            InstallScene();
            File.WriteAllText("../tools/inn-ui/scene-result.json", "{\"ok\":true}");
        }
        catch (Exception exception)
        {
            var result = new JObject {
                ["ok"] = false,
                ["error"] = exception.ToString()
            };

            File.WriteAllText("../tools/inn-ui/scene-result.json", result.ToString());
            Debug.LogException(exception);
        }
    }

    /// <summary>Installs only the source inn model under its existing authored anchor.</summary>
    private static void InstallScene()
    {
        const string source = "../tools/main-scene/hotel-built";
        const string destination = "Assets/Data/Scenes/Home/Opening/hotel-built";
        var data = JObject.Parse(File.ReadAllText(source + "/scene.json"));
        MiniwowMainSceneSetup.ImportAssets(data, source, destination, out var meshes, out var materials);

        var scenes = JObject.Parse(File.ReadAllText("Assets/Data/Jsons/Excels/SceneConfig.bytes"));
        var scene = EditorSceneManager.OpenScene((string)scenes["data"]["10002"]["Path"]);
        var nodes = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
        var home = scene.GetRootGameObjects().Select(root => root.GetComponent<HomeOpeningScene>())
            .Single(item => item);
        var model = nodes.Single(item => item.name == "Hotel_60017").Find("Model");
        MiniwowAuctionSetup.InstallModel(data, model, meshes, materials);

        var built = model.Find((string)data["nodes"][0]["name"]).gameObject;
        built.SetActive(false);
        var binding = new SerializedObject(home);
        binding.FindProperty("hotelBuilt").objectReferenceValue = built;
        binding.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }
}
