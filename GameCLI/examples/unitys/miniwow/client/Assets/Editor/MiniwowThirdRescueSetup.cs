using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Installs the original cleared hospital stage and countdown UI references.</summary>
[InitializeOnLoad]
public static class MiniwowThirdRescueSetup
{
    /// <summary>Explicit installation request consumed outside play mode.</summary>
    private const string requestPath = "../tools/opening/third.request";

    /// <summary>Registers the idle request processor.</summary>
    static MiniwowThirdRescueSetup()
    {
        EditorApplication.update += Poll;
    }

    /// <summary>Processes the selected native asset installation.</summary>
    private static void Poll()
    {
        if (!File.Exists(requestPath) || EditorApplication.isPlayingOrWillChangePlaymode
            || EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        File.Delete(requestPath);
        try
        {
            Install();
            File.WriteAllText("../tools/opening/third-install.json", "{\"ok\":true}");
        }
        catch (Exception exception)
        {
            File.WriteAllText("../tools/opening/third-install.json", new JObject {
                ["ok"] = false,
                ["error"] = exception.ToString()
            }.ToString());
            Debug.LogException(exception);
        }
    }

    /// <summary>Adds the selected model and binds fixed scene and UI references.</summary>
    private static void Install()
    {
        const string source = "../tools/main-scene/hospital-cleared";
        const string destination = "Assets/Data/Scenes/Home/Opening/hospital-cleared";
        var data = JObject.Parse(File.ReadAllText(source + "/scene.json"));
        MiniwowMainSceneSetup.ImportAssets(data, source, destination, out var meshes, out var materials);

        var scenes = JObject.Parse(File.ReadAllText("Assets/Data/Jsons/Excels/SceneConfig.bytes"));
        var scene = EditorSceneManager.OpenScene((string)scenes["data"]["10002"]["Path"]);
        var nodes = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
        var home = scene.GetRootGameObjects().Select(root => root.GetComponent<HomeScene>()).Single(item => item);
        var hospital = nodes.Single(item => item.name == "Hospital_60018");
        var model = hospital.Find("Model");
        MiniwowAuctionSetup.InstallModel(data, model, meshes, materials);

        var opening = new SerializedObject(home.GetComponent<HomeOpeningScene>());
        var models = opening.FindProperty("hospitalModels");
        string[] names = { "priest_initial_1", "priest_initial_2", "priest_initial_3", "priest_initial" };
        models.arraySize = names.Length;
        for (int index = 0; index < names.Length; index++)
        {
            var variant = model.Find(names[index]).gameObject;
            models.GetArrayElementAtIndex(index).objectReferenceValue = variant;
            variant.SetActive(index == 0);
        }

        var targets = opening.FindProperty("rescueTargets");
        targets.arraySize = 5;
        targets.GetArrayElementAtIndex(3).objectReferenceValue = hospital.Find("PeopleBubble");
        targets.GetArrayElementAtIndex(4).objectReferenceValue = hospital.Find("PeopleBubble");
        opening.FindProperty("clearingSmoke").objectReferenceValue = CreateSmoke(home.transform,
            hospital.Find("PeopleBubble").position);
        opening.ApplyModifiedPropertiesWithoutUndo();

        const string widgetPath = "Assets/Data/Widgets/Home/UIHomeOpening.prefab";
        var widget = PrefabUtility.LoadPrefabContents(widgetPath);
        try
        {
            var binding = new SerializedObject(widget.GetComponent<UIHomeOpening>());
            var icon = widget.GetComponentsInChildren<Transform>(true).Single(item => item.name == "WaterIcon");
            binding.FindProperty("waterIcon").objectReferenceValue = icon.gameObject;
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

    /// <summary>Authors a local smoke adaptation with the original recovered smoke texture.</summary>
    /// <param name="parent">The scene-owned presentation root.</param>
    /// <param name="position">The original hospital effect anchor.</param>
    /// <returns>The authored particle system.</returns>
    private static ParticleSystem CreateSmoke(Transform parent, Vector3 position)
    {
        const string texturePath = "Assets/Data/Scenes/Home/Opening/hospital-cleared/smoke.png";
        File.Copy("../extracted-resources/assets/data/effects/textures/ef_tex_smoke_01.png", texturePath, true);
        AssetDatabase.ImportAsset(texturePath);
        const string materialPath = "Assets/Data/Scenes/Home/Opening/hospital-cleared/smoke.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (!material)
        {
            material = new Material(Shader.Find("Particles/Standard Unlit"));
            AssetDatabase.CreateAsset(material, materialPath);
        }

        material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        material.SetFloat("_Mode", 2);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.EnableKeyword("_ALPHABLEND_ON");
        material.renderQueue = 3000;
        var existing = parent.Find("ClearingSmoke");
        if (existing)
            UnityEngine.Object.DestroyImmediate(existing.gameObject);

        var smoke = new GameObject("ClearingSmoke");
        smoke.transform.SetParent(parent, false);
        smoke.transform.position = position;
        var particles = smoke.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = particles.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startLifetime = 0.8f;
        main.startSpeed = 0.5f;
        main.startSize = 2.5f;
        main.startColor = new Color(0.8f, 0.72f, 0.6f, 0.65f);
        var emission = particles.emission;
        emission.rateOverTime = 12f;
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(4f, 0.5f, 3f);
        particles.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
        EditorUtility.SetDirty(material);

        return particles;
    }
}
