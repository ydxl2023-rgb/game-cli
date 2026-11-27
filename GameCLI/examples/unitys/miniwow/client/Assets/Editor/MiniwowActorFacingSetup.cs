using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Restores the original BuildingHero orientation through native scene authoring.</summary>
[InitializeOnLoad]
public static class MiniwowActorFacingSetup
{
    /// <summary>Explicit edit-mode request, separate from runtime behavior.</summary>
    private const string request = "Library/OpeningQA/actor-facing.request";

    /// <summary>Registers the explicit scene repair request processor.</summary>
    static MiniwowActorFacingSetup()
    {
        EditorApplication.update += OnUpdate;
    }

    /// <summary>Repairs only the hero visual orientation while the editor is idle.</summary>
    private static void OnUpdate()
    {
        if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode
            || EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        File.Delete(request);
        var configuration = JObject.Parse(File.ReadAllText("Assets/Data/Jsons/Excels/SceneConfig.bytes"));
        var scene = EditorSceneManager.OpenScene((string)configuration["data"]["10002"]["Path"]);
        var actor = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<ActorShape>(true))
            .Single(shape => shape.name == "TownVisitor");
        var fields = new SerializedObject(actor);
        var animation = (Component)fields.FindProperty("animationView").objectReferenceValue;
        animation.transform.rotation = Quaternion.Euler(30f, -135f, 0f);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorSceneManager.OpenScene((string)configuration["data"]["10001"]["Path"]);
        File.WriteAllText("Library/OpeningQA/actor-facing-setup.json", "{\"restoredOriginalHeroRotation\":true}");
    }
}
