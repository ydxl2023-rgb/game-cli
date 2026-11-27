using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Imports the selected original hospital model and binds village presentation natively.</summary>
[InitializeOnLoad]
public static class MiniwowVillageSetup
{
    /// <summary>Registers the edit-mode authoring request.</summary>
    static MiniwowVillageSetup()
    {
        EditorApplication.update += OnUpdate;
    }

    /// <summary>Consumes one explicit asset authoring request outside Play Mode.</summary>
    private static void OnUpdate()
    {
        const string request = "../tools/opening/village.request";
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || !File.Exists(request))
            return;

        File.Delete(request);
        try
        {
            Install();
            File.WriteAllText("../tools/opening/village-install.json", "{\"ok\":true}");
        }
        catch (Exception exception)
        {
            File.WriteAllText("../tools/opening/village-install.json", new JObject {
                ["ok"] = false,
                ["error"] = exception.ToString()
            }.ToString());
            Debug.LogException(exception);
        }
    }

    /// <summary>Authors only the new model and serialized references without rebuilding Home.</summary>
    private static void Install()
    {
        const string source = "../tools/main-scene/hospital-built";
        const string destination = "Assets/Data/Scenes/Home/Opening/hospital-built";
        var data = JObject.Parse(File.ReadAllText(source + "/scene.json"));
        MiniwowMainSceneSetup.ImportAssets(data, source, destination, out var meshes, out var materials);

        var scenes = JObject.Parse(File.ReadAllText("Assets/Data/Jsons/Excels/SceneConfig.bytes"));
        var scene = EditorSceneManager.OpenScene((string)scenes["data"]["10002"]["Path"]);
        var nodes = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
        var home = scene.GetRootGameObjects().Select(root => root.GetComponent<HomeScene>()).Single(item => item);
        var hospital = nodes.Single(item => item.name == "Hospital_60018");
        var garen = nodes.Single(item => item.name == "TownVisitor").GetComponent<ActorShape>();
        var model = hospital.Find("Model");
        MiniwowAuctionSetup.InstallModel(data, model, meshes, materials);
        var built = model.Find("priest").gameObject;
        built.SetActive(false);
        var visual = garen.transform.Find("OriginalHeroVisual");
        visual.rotation = Quaternion.Euler(30f, -135f, 0f);
        visual.localScale = Vector3.one * 1.65f;
        var opening = new SerializedObject(home.GetComponent<HomeOpeningScene>());
        opening.FindProperty("hospitalBuilt").objectReferenceValue = built;
        opening.FindProperty("garen").objectReferenceValue = garen;
        opening.ApplyModifiedPropertiesWithoutUndo();

        const string portraitPath = "Assets/Data/Widgets/Common/Icons/GarenPortrait.png";
        File.Copy("../extracted-resources/fairygui/CharacterIcon/Hero/hero_20025.png", portraitPath, true);
        AssetDatabase.DeleteAsset("Assets/Data/Widgets/Common/Icons/Conversation_hero_20025.png");
        AssetDatabase.ImportAsset(portraitPath);
        var importer = (TextureImporter)AssetImporter.GetAtPath(portraitPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.SaveAndReimport();
        const string widgetPath = "Assets/Data/Widgets/Home/UIHomeOpening.prefab";
        var widget = PrefabUtility.LoadPrefabContents(widgetPath);
        try
        {
            var binding = new SerializedObject(widget.GetComponent<UIHomeOpening>());
            binding.FindProperty("garen").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(portraitPath);
            binding.FindProperty("continueButton").objectReferenceValue = widget
                .GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(item => item.name == "ContinueClick");
            binding.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(widget, widgetPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(widget);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }
}
