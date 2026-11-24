using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Samples real Animator controllers in isolation and creates an optional playable review scene.</summary>
    internal static class UnityCliMotionPreviewService
    {
        internal static PrefabData Execute(UnityCliBridgeRequest request)
        {
            JObject spec = JObject.Parse(File.ReadAllText(request.jsonFile));
            string directory = Path.GetFullPath((string)spec["outputDirectory"]);
            string scenePath = (string)spec["reviewScene"];
            int width = (int)spec["width"];
            int height = (int)spec["height"];
            int frames = (int)spec["frames"];
            float step = (float)spec["step"];
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(request.prefabPath);
            if (prefab == null || width < 64 || height < 64 || width > 4096 || height > 4096
                || frames < 2 || frames > 120 || step <= 0 || step > 2)
            {
                throw new InvalidOperationException("Invalid motion preview input.");
            }
            if (!string.IsNullOrEmpty(scenePath) && (!scenePath.StartsWith("Assets/", StringComparison.Ordinal)
                || scenePath.Contains("..") || !scenePath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase) || File.Exists(scenePath)))
            {
                throw new InvalidOperationException("Review scene must be a new Assets/*.unity file.");
            }
            if (request.dryRun)
            {
                return new PrefabData { action = request.action, valid = true, dryRun = true, message = "Motion preview input valid." };
            }
            Directory.CreateDirectory(directory);
            Scene scene = EditorSceneManager.NewPreviewScene();
            JArray evidence = new JArray();
            try
            {
                for (int i = 0; i < frames; i++)
                {
                    GameObject root = UnityEngine.Object.Instantiate(prefab);
                    SceneManager.MoveGameObjectToScene(root, scene);
                    ApplyActivePaths(root, spec);
                    // Restore from the same initial state for reproducible independent samples.
                    Animator[] animators = root.GetComponentsInChildren<Animator>(false);
                    foreach (Animator animator in animators)
                    {
                        animator.Rebind();
                        animator.Update(0);
                        animator.Update(i * step);
                    }
                    // Preview metadata is not acceptance evidence; verification belongs to prefab verify.
                    evidence.Add(new JObject
                    {
                        ["time"] = i * step,
                        ["animators"] = animators.Length
                    });
                    UnityCliSnapshotVisuals.Render(root, scene, new JObject
                    {
                        ["width"] = width, ["height"] = height,
                        ["output"] = Path.Combine(directory, "frame-" + i.ToString("D3") + ".png")
                    });
                    // Render creates a temporary host and camera in the same isolated scene.
                    foreach (GameObject item in scene.GetRootGameObjects())
                    {
                        UnityEngine.Object.DestroyImmediate(item);
                    }
                }
                File.WriteAllText(Path.Combine(directory, "samples.json"), evidence.ToString());
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
            if (!string.IsNullOrEmpty(scenePath))
            {
                CreateReviewScene(prefab, scenePath, width, height, spec);
            }
            return new PrefabData
            {
                action = request.action, valid = true, prefabPath = request.prefabPath,
                scenePath = scenePath, message = "Preview frames generated only; use prefab verify for acceptance. Source prefab and user scenes unchanged."
            };
        }

        private static void ApplyActivePaths(GameObject instance, JObject spec)
        {
            // Review setup changes only isolated instances, never the source prefab.
            foreach (JProperty property in ((JObject)spec["activePaths"] ?? new JObject()).Properties())
            {
                Transform target = property.Name.Length == 0 ? instance.transform : instance.transform.Find(property.Name);
                if (target == null || property.Value.Type != JTokenType.Boolean)
                {
                    throw new InvalidOperationException("Invalid review activation path: " + property.Name);
                }
                target.gameObject.SetActive((bool)property.Value);
            }
        }

        private static void CreateReviewScene(GameObject prefab, string path, int width, int height, JObject spec)
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene review = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                GameObject canvasObject = new GameObject("MotionPreviewCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                SceneManager.MoveGameObjectToScene(canvasObject, review);
                canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(width, height);
                scaler.matchWidthOrHeight = 0.5f;
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, review);
                instance.transform.SetParent(canvasObject.transform, false);
                ApplyActivePaths(instance, spec);
                GameObject events = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                SceneManager.MoveGameObjectToScene(events, review);
                if (!EditorSceneManager.SaveScene(review, path))
                {
                    throw new InvalidOperationException("Could not save review scene.");
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(review, true);
                SceneManager.SetActiveScene(previous);
            }
        }
    }
}
