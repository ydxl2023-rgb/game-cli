using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Installs the CLI-authored Home environment into a dedicated additive scene.</summary>
[InitializeOnLoad]
public static class MiniwowHomeSetup
{
    /// <summary>Explicit installation request supplied by the product tooling.</summary>
    private const string RequestPath = "../tools/home/install.request";

    /// <summary>Registers the guarded installer.</summary>
    static MiniwowHomeSetup()
    {
        EditorApplication.update += Poll;
    }

    /// <summary>Consumes a request only while the editor is idle outside Play Mode.</summary>
    private static void Poll()
    {
        if (!File.Exists(RequestPath) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        File.Delete(RequestPath);

        try
        {
            Install();
            File.WriteAllText("../tools/home/install-result.json", "{\"ok\":true}");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            File.WriteAllText("../tools/home/install-result.json", exception.ToString());
        }
    }

    /// <summary>Uses scene and asset APIs without constructing prefab or scene YAML.</summary>
    private static void Install()
    {
        string identity = File.ReadAllText("ProjectSettings/GameCLIProject.json");
        if (!identity.Contains("p-fac9bb57-0a72-4db3-9f48-7d42e95f37a3") || !identity.Contains("83c29c9f-21c1-4597-9691-6707ca146fa7"))
            throw new InvalidOperationException("Unexpected project identity.");

        Scene app = EditorSceneManager.GetActiveScene();
        if (app.path != "Assets/Scenes/App.unity")
            throw new InvalidOperationException("Open App.unity before installing Home.");

        foreach (Canvas canvas in app.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Canvas>(true)))
        {
            if (canvas.name == "LaunchCanvas")
                canvas.sortingOrder = 100;
        }

        foreach (CanvasScaler scaler in app.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<CanvasScaler>(true)))
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0;
        }

        EditorSceneManager.SaveScene(app);

        string path = "Assets/Scenes/Home.unity";
        if (!File.Exists(path))
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Widgets/Home/HomeEnvironment.prefab");
            if (!prefab)
                throw new InvalidOperationException("Generate the HomeEnvironment prefab first.");

            Scene home = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            EditorSceneManager.SetActiveScene(home);

            GameObject canvasObject = new GameObject("HomeEnvironmentCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -5;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0;

            PrefabUtility.InstantiatePrefab(prefab, canvasObject.transform);

            EditorSceneManager.SaveScene(home, path);
            EditorSceneManager.CloseScene(home, true);
            EditorSceneManager.SetActiveScene(app);
        }

        var scenes = EditorBuildSettings.scenes.ToList();
        foreach (string scenePath in new[] { "Assets/Scenes/App.unity", path })
        {
            int index = scenes.FindIndex(scene => scene.path == scenePath);
            if (index < 0)
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            else
                scenes[index].enabled = true;
        }

        EditorBuildSettings.scenes = scenes.ToArray();

        AssetDatabase.SaveAssets();
    }
}
