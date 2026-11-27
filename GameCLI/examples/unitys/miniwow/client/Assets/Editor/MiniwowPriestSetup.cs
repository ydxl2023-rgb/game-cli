using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Restores the missing world priest at the original scene anchor.</summary>
[InitializeOnLoad]
public static class MiniwowPriestSetup
{
    /// <summary>Registers the explicit edit-mode authoring request.</summary>
    static MiniwowPriestSetup()
    {
        EditorApplication.update += OnUpdate;
    }

    /// <summary>Consumes an explicit request without changing live progression.</summary>
    private static void OnUpdate()
    {
        const string request = "Library/OpeningQA/priest.request";
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || !File.Exists(request))
            return;

        File.Delete(request);
        try
        {
            Install();
            File.WriteAllText("Library/OpeningQA/priest-install.json", "{\"ok\":true}");
        }
        catch (Exception exception)
        {
            File.WriteAllText("Library/OpeningQA/priest-install.json", exception.ToString());
            Debug.LogException(exception);
        }
    }

    /// <summary>Creates world-scale Spine assets while retaining the existing shared source data.</summary>
    /// <returns>The world priest skeleton.</returns>
    private static SkeletonDataAsset ImportSkeleton()
    {
        const string source = "Assets/Data/Widgets/Hospital/Resources/Priest/";
        const string target = "Assets/Data/Scenes/Town/Spine/npc_priest_2";
        if (!AssetDatabase.IsValidFolder(target))
            AssetDatabase.CreateFolder("Assets/Data/Scenes/Town/Spine", "npc_priest_2");

        var material = AssetDatabase.LoadAssetAtPath<Material>(target + "/Skeleton.mat");
        if (!material)
        {
            material = new Material(Shader.Find("Spine/Skeleton"));
            AssetDatabase.CreateAsset(material, target + "/Skeleton.mat");
        }

        material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(source + "npc_priest.png");
        EditorUtility.SetDirty(material);

        var atlas = AssetDatabase.LoadAssetAtPath<SpineAtlasAsset>(target + "/Atlas.asset");
        if (!atlas)
        {
            atlas = SpineAtlasAsset.CreateRuntimeInstance(
                AssetDatabase.LoadAssetAtPath<TextAsset>(source + "npc_priest.atlas.txt"), new[] { material }, true);
            AssetDatabase.CreateAsset(atlas, target + "/Atlas.asset");
        }

        var skeleton = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(target + "/Skeleton.asset");
        if (!skeleton)
        {
            skeleton = SkeletonDataAsset.CreateRuntimeInstance(
                AssetDatabase.LoadAssetAtPath<TextAsset>(source + "npc_priest_2.skel.bytes"), atlas, true);
            AssetDatabase.CreateAsset(skeleton, target + "/Skeleton.asset");
        }

        skeleton.scale = 0.01f;
        EditorUtility.SetDirty(skeleton);

        return skeleton;
    }

    /// <summary>Adds the resident to the existing Home actor collection without replacing any actors.</summary>
    private static void Install()
    {
        var skeleton = ImportSkeleton();
        var scenes = JObject.Parse(File.ReadAllText("Assets/Data/Jsons/Excels/SceneConfig.bytes"));
        var scene = EditorSceneManager.OpenScene((string)scenes["data"]["10002"]["Path"]);
        var nodes = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
        var home = nodes.Select(node => node.GetComponent<HomeScene>()).Single(item => item);
        var anchor = nodes.Single(node => node.name == "NPC_priest_2");
        var animation = anchor.GetComponent<SkeletonAnimation>();
        if (!animation)
            animation = SkeletonAnimation.AddToGameObject(anchor.gameObject, skeleton);

        animation.skeletonDataAsset = skeleton;
        animation.Initialize(true);
        animation.loop = true;
        animation.AnimationName = ActorAnimationNames.idle;

        var shape = anchor.GetComponent<ActorShape>();
        if (!shape)
            shape = anchor.gameObject.AddComponent<ActorShape>();

        var binding = new SerializedObject(home);
        var view = new SerializedObject(shape);
        view.FindProperty("animationView").objectReferenceValue = animation;
        view.FindProperty("sceneCamera").objectReferenceValue =
            binding.FindProperty("sceneCamera").objectReferenceValue;
        view.FindProperty("walkAnimation").stringValue = "";
        view.ApplyModifiedPropertiesWithoutUndo();

        var actors = binding.FindProperty("actors");
        int index = 0;
        while (index < actors.arraySize
            && actors.GetArrayElementAtIndex(index).FindPropertyRelative("actorId").intValue != 5)
            index++;

        if (index == actors.arraySize)
            actors.arraySize++;

        var actor = actors.GetArrayElementAtIndex(index);
        actor.FindPropertyRelative("actorId").intValue = 5;
        actor.FindPropertyRelative("configId").intValue = 60018;
        actor.FindPropertyRelative("destinationId").intValue = 0;
        actor.FindPropertyRelative("moveSpeed").floatValue = 3.2f;
        actor.FindPropertyRelative("shape").objectReferenceValue = shape;
        var route = actor.FindPropertyRelative("route");
        route.arraySize = 1;
        route.GetArrayElementAtIndex(0).objectReferenceValue = anchor;
        binding.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene((string)scenes["data"]["10001"]["Path"]);
    }
}
