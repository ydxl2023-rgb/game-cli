#if UNITY_EDITOR
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Oathx.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Instantiates the CLI-authored Launch asset into the existing practice entry scene.</summary>
[InitializeOnLoad]
public static class PracticeTrialIntegration
{
    /// <summary>Reconnect the queued scene operation across domain reloads.</summary>
    static PracticeTrialIntegration()
    {
        EditorApplication.playModeStateChanged += OnStateChanged;
    }

    /// <summary>Schedule scene integration after the CLI's Play-only call has returned.</summary>
    /// <returns>Whether the edit-mode integration request was queued.</returns>
    public static bool QueueLaunchIntegration()
    {
        SessionState.SetBool("GameCLI.Practice.AttachLaunch", true);
        EditorApplication.isPlaying = false;

        return true;
    }

    /// <summary>Apply the requested scene-only edit after leaving Play mode.</summary>
    /// <param name="state">Current Editor lifecycle state.</param>
    private static void OnStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool("GameCLI.Practice.AttachLaunch", false))
        {
            SessionState.SetBool("GameCLI.Practice.AttachLaunch", false);

            IntegrateLaunch();
        }
    }

    /// <summary>Replace only the baseline launch display without regenerating business prefabs.</summary>
    private static void IntegrateLaunch()
    {
        var identity = JObject.Parse(File.ReadAllText("ProjectSettings/GameCLIProject.json"));
        if ((string)identity["projectId"] != "p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484"
            || (string)identity["unityProjectId"] != "44f4f783-645c-41e7-80fe-aead8477c978")
            throw new InvalidOperationException("Expected the bound practice trial project.");

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/App.unity")
            throw new InvalidOperationException("Expected the practice App scene.");

        PlayerSettings.productName = "GameCLIPracticeTrial";

        foreach (var canvas in UnityEngine.Object.FindObjectsOfType<Canvas>(true))
        {
            if (canvas.isRootCanvas)
            {
                var scaler = canvas.GetComponent<CanvasScaler>();
                if (!scaler)
                    scaler = canvas.gameObject.AddComponent<CanvasScaler>();

                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
        }

        var app = UnityEngine.Object.FindObjectOfType<XApp>(true);
        if (!app)
            throw new InvalidOperationException("The practice XApp is missing.");

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Widgets/Launch/Launch.prefab");
        if (!prefab || !prefab.GetComponent<XLauncher>())
            throw new InvalidOperationException("The CLI-authored Launch prefab is not ready.");

        var serialized = new SerializedObject(app);
        var reference = serialized.FindProperty("launcher");
        var previous = reference.objectReferenceValue as XLauncher;
        if (!previous)
            throw new InvalidOperationException("The baseline launcher reference is missing.");

        if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(previous.gameObject) == "Assets/Data/Widgets/Launch/Launch.prefab")
        {
            Debug.Log("Practice Launch already integrated.");
        }
        else
        {
            var canvas = previous.GetComponent<Canvas>();
            if (!canvas)
                throw new InvalidOperationException("Expected the baseline Launch canvas.");

            foreach (Transform child in previous.transform)
                child.gameObject.SetActive(false);

            previous.enabled = false;
            canvas.sortingOrder = -100;
            var instance = PrefabUtility.InstantiatePrefab(prefab, previous.transform) as GameObject;
            instance.name = "TemplateLaunch";
            instance.SetActive(true);
            reference.objectReferenceValue = instance.GetComponent<XLauncher>();
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Practice Launch integrated from its CLI-authored prefab.");
        }
    }
}
#endif
