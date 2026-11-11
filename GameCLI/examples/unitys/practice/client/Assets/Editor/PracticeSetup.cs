using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using Oathx.Core;
using Oathx.Widget;

/// <summary>Creates the minimal practice entry using Unity asset APIs.</summary>
public static class PracticeSetup
{
    /// <summary>Build and save the clean entry scene and configuration.</summary>
    public static void Build()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Main Camera", typeof(Camera)).GetComponent<Camera>();
        camera.tag = "MainCamera";
        var ui = new GameObject("UISystem", typeof(RectTransform), typeof(UISystem));

        foreach (string layer in System.Enum.GetNames(typeof(WidgetLayer)))
        {
            var root = new GameObject(layer, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(ui.transform, false);
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        }
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        var launch = new GameObject("Launch", typeof(RectTransform), typeof(Canvas), typeof(XLauncher));
        launch.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var launcher = launch.GetComponent<XLauncher>();
        var serialized = new SerializedObject(launcher);

        foreach (string field in new[] { "loading", "update" })
        {
            var panel = new GameObject(field, typeof(RectTransform));
            panel.transform.SetParent(launch.transform, false);
            serialized.FindProperty(field).objectReferenceValue = panel;
        }

        foreach (string field in new[] { "loadingText", "loadingPercent", "tip", "downloadText", "downloadPercent" })
        {
            var label = new GameObject(field, typeof(RectTransform), typeof(Text));
            label.transform.SetParent(launch.transform, false);
            serialized.FindProperty(field).objectReferenceValue = label.GetComponent<Text>();
        }

        foreach (string field in new[] { "fill", "handle" })
        {
            var rect = new GameObject(field, typeof(RectTransform));
            rect.transform.SetParent(launch.transform, false);
            serialized.FindProperty(field).objectReferenceValue = rect.transform;
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();


        var app = new GameObject("XApp", typeof(XApp)).GetComponent<XApp>();
        var appData = new SerializedObject(app);
        appData.FindProperty("launcher").objectReferenceValue = launcher;
        appData.FindProperty("mainCamera").objectReferenceValue = camera;
        appData.ApplyModifiedPropertiesWithoutUndo();

        var config = AssetDatabase.LoadAssetAtPath<XAppConfigure>("Assets/Resources/XAppConfigure.asset");
        bool create = !config;
        if (create)
            config = ScriptableObject.CreateInstance<XAppConfigure>();

        config.runtime = AppRuntime.Mono;
        config.bundleSource = AppBundleSource.Local;
        config.hotfix = false;
        config.syncEntityArchiveToRemote = false;
        config.plugins = new[] { "PracticePlugin" };
        if (create)
            AssetDatabase.CreateAsset(config, "Assets/Resources/XAppConfigure.asset");
        else
            EditorUtility.SetDirty(config);


        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), "Assets/Scenes/App.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/App.unity", true) };
        AssetDatabase.SaveAssets();
    }
}

