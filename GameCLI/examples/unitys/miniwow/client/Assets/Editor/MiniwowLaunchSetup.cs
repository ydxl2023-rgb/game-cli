using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Oathx.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Installs the generated launch prefab into the verified product scene.</summary>
[InitializeOnLoad]
public static class MiniwowLaunchSetup
{
    /// <summary>Explicit request file written by the project installation tool.</summary>
    private const string RequestPath = "../tools/launch/install.request";

    /// <summary>Registers a request-driven editor installation callback.</summary>
    static MiniwowLaunchSetup()
    {
        EditorApplication.update += Poll;
    }

    /// <summary>Installs only when the user-authorized request exists.</summary>
    private static void Poll()
    {
        if (!File.Exists(RequestPath) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        File.Delete(RequestPath);
        try
        {
            Install();
            File.WriteAllText("../tools/launch/install-result.json", "{\"ok\":true,\"resolution\":[1080,1920]}");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            File.WriteAllText("../tools/launch/install-result.json", JsonUtility.ToJson(new Failure { message = exception.Message }));
        }
    }

    /// <summary>Updates the scene and player settings without rewriting prefab assets.</summary>
    private static void Install()
    {
        string identity = File.ReadAllText("ProjectSettings/GameCLIProject.json");
        if (!identity.Contains("p-fac9bb57-0a72-4db3-9f48-7d42e95f37a3") || !identity.Contains("83c29c9f-21c1-4597-9691-6707ca146fa7"))
        {
            throw new InvalidOperationException("Unexpected Unity project identity.");
        }

        UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/App.unity")
        {
            throw new InvalidOperationException("Open the product App scene before installation.");
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Widgets/Launch/Launch.prefab");
        if (!prefab || !prefab.GetComponent<XLauncher>())
        {
            throw new InvalidOperationException("The CLI-generated launch component is missing.");
        }

        XApp app = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<XApp>(true)).Single();
        foreach (GameObject root in scene.GetRootGameObjects().Where(root => root.name == "Launch" || root.name == "LaunchCanvas"))
        {
            UnityEngine.Object.DestroyImmediate(root);
        }

        GameObject canvasObject = new GameObject("LaunchCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -10;
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvasObject.transform);
        SerializedObject serialized = new SerializedObject(app);
        serialized.FindProperty("launcher").objectReferenceValue = instance.GetComponent<XLauncher>();
        serialized.FindProperty("startupResolution").vector2IntValue = new Vector2Int(1080, 1920);
        serialized.ApplyModifiedPropertiesWithoutUndo();

        foreach (CanvasScaler scaler in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<CanvasScaler>(true)))
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        PlayerSettings.defaultScreenWidth = 1080;
        PlayerSettings.defaultScreenHeight = 1920;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;
        SetGameView();
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    /// <summary>Selects a portrait fixed-resolution Game View for reproducible previews.</summary>
    private static void SetGameView()
    {
        Assembly assembly = typeof(Editor).Assembly;
        Type sizesType = assembly.GetType("UnityEditor.GameViewSizes");
        Type singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        object sizes = singleton.GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        object group = sizesType.GetMethod("GetGroup").Invoke(sizes, new object[] { 0 });
        Type groupType = group.GetType();
        int count = (int)groupType.GetMethod("GetTotalCount").Invoke(group, null);
        Type sizeType = assembly.GetType("UnityEditor.GameViewSize");
        Type kindType = assembly.GetType("UnityEditor.GameViewSizeType");
        object size = Activator.CreateInstance(sizeType, new object[] { Enum.ToObject(kindType, 1), 1080, 1920, "Miniwow Portrait" });
        groupType.GetMethod("AddCustomSize").Invoke(group, new[] { size });
        Type viewType = assembly.GetType("UnityEditor.GameView");
        EditorWindow view = EditorWindow.GetWindow(viewType);
        viewType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(view, count);
        view.Repaint();
    }

    /// <summary>Serializes an installation failure without swallowing diagnostics.</summary>
    [Serializable]
    private sealed class Failure
    {
        /// <summary>Installation failure description.</summary>
        public string message;
    }
}
