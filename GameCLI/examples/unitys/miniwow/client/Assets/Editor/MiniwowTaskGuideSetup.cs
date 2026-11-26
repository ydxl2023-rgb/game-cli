using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Installs configured guide target bindings through native prefab APIs.</summary>
[InitializeOnLoad]
public static class MiniwowTaskGuideSetup
{
    /// <summary>Explicit authoring request location.</summary>
    private const string directory = "../tools/task-guide/";

    /// <summary>Registers edit-mode request processing.</summary>
    static MiniwowTaskGuideSetup()
    {
        EditorApplication.update += OnUpdate;
    }

    /// <summary>Applies only an explicit authoring request outside play mode.</summary>
    private static void OnUpdate()
    {
        string request = directory + "bind.request";
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating
            || EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        File.Delete(request);
        try
        {
            BindTargets();
            File.WriteAllText(directory + "bind-result.json", "{\"ok\":true}");
        }
        catch (System.Exception exception)
        {
            File.WriteAllText(directory + "bind-result.json", exception.ToString());
            Debug.LogException(exception);
        }
    }

    /// <summary>Binds product identities without putting their paths in runtime task code.</summary>
    private static void BindTargets()
    {
        var marker = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Widgets/Task/UITaskGuideFinger.prefab");
        var bindings = JArray.Parse(File.ReadAllText(directory + "targets.json"));
        foreach (var group in bindings.GroupBy(row => (string)row["prefab"]))
        {
            var root = PrefabUtility.LoadPrefabContents(group.Key);
            try
            {
                foreach (var row in group)
                {
                    var button = root.transform.Find((string)row["button"]).GetComponent<Button>();
                    var target = button.GetComponent<TaskGuideTarget>();
                    if (!target)
                        target = button.gameObject.AddComponent<TaskGuideTarget>();

                    var existing = button.transform.Find(marker.name);
                    var finger = existing ? existing.gameObject :
                        (GameObject)PrefabUtility.InstantiatePrefab(marker, button.transform);
                    var rect = (RectTransform)finger.transform;
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    rect.localScale = Vector3.one;
                    var icon = (RectTransform)rect.GetChild(0);
                    icon.anchorMin = new Vector2(0.5f, 0.5f);
                    icon.anchorMax = icon.anchorMin;
                    icon.pivot = new Vector2(0f, 1f);
                    icon.anchoredPosition = Vector2.zero;
                    icon.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    icon.sizeDelta = new Vector2(100f, 100f);
                    foreach (var graphic in finger.GetComponentsInChildren<Graphic>(true))
                        graphic.raycastTarget = false;

                    finger.SetActive(false);
                    var serialized = new SerializedObject(target);
                    serialized.FindProperty("targetKey").stringValue = (string)row["key"];
                    serialized.FindProperty("finger").objectReferenceValue = finger;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }

                PrefabUtility.SaveAsPrefabAsset(root, group.Key);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        AssetDatabase.SaveAssets();
    }
}
