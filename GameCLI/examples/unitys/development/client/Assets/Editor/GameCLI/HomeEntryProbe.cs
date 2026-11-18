#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Read-only prefab and live VM evidence for reusable Home entries.</summary>
public static class HomeEntryProbe
{
    /// <summary>Start the local login-success transition through the real coordinator.</summary>
    public static void EnterGame()
    {
        XPluginUtility.FireEvent(new LoginSucceed());
    }

    /// <summary>Select the live page or an entry to exercise the actual Metadata Inspector.</summary>
    /// <param name="id">Entry ID, or minus one for the page.</param>
    /// <returns>True when the Inspector target was selected.</returns>
    public static bool Inspect(int id)
    {
        if (!UISystem.GetSingleton().TryGetWidget<UIHome>(out var home))
            return false;

        var entry = home.GetComponentsInChildren<UIActivityEntry>(true).FirstOrDefault(item => item.GetSystemId() == id);
        UnityEditor.Selection.activeGameObject = id < 0 ? home.gameObject : entry ? entry.gameObject : null;
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

        return UnityEditor.Selection.activeGameObject;
    }

    /// <summary>Check live bindings after repeated Inspector rendering.</summary>
    /// <returns>The number of unchanged binding fields.</returns>
    public static int CheckInspectedBindings()
    {
        if (!UISystem.GetSingleton().TryGetWidget<UIHome>(out var home))
            throw new InvalidOperationException("Home is not open.");

        var source = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Widgets/Home/Home.prefab");
        var expected = source.GetComponentsInChildren<Metadata>(true);
        int count = 0;
        foreach (var metadata in expected)
        {
            string path = UnityEditor.AnimationUtility.CalculateTransformPath(metadata.transform, source.transform);
            var target = (path.Length == 0 ? home.transform : home.transform.Find(path)).GetComponent<Metadata>();
            if (target.metadatas.Count != metadata.metadatas.Count)
                throw new InvalidOperationException("Inspector changed the binding count.");

            for (int index = 0; index < metadata.metadatas.Count; index++)
            {
                var field = target.metadatas[index];
                if (field.name != metadata.metadatas[index].name || field.type != metadata.metadatas[index].type
                    || field.components.Any(component => !component))
                    throw new InvalidOperationException("Inspector changed a binding: " + path + "/" + field.name);

                count++;
            }
        }

        return count;
    }

    /// <summary>Verify entry metadata and optional badge references without modifying assets.</summary>
    /// <returns>Counts for the checked prefab contract.</returns>
    public static string CheckPrefab()
    {
        var root = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Widgets/Home/Home.prefab");
        var entries = root.GetComponentsInChildren<UIActivityEntry>(true);
        if (entries.Length != 64 || entries.Select(entry => entry.GetSystemId()).Distinct().Count() != 64)
            throw new InvalidOperationException("Expected 64 unique entry identities.");

        string[] names = { "openClick", "visible", "interactable", "badges" };
        string[] types = { "SyncClick", "SyncVisible", "SyncInteractable", "SyncActivityBadges" };
        foreach (var entry in entries)
        {
            var metadata = entry.GetComponent<Metadata>();
            if (entry.GetComponents<Metadata>().Length != 1 || metadata.metadatas.Count != 4)
                throw new InvalidOperationException(entry.name + " has an invalid binding count.");

            for (int index = 0; index < names.Length; index++)
            {
                var field = metadata.metadatas[index];
                if (field.name != names[index] || field.type != types[index]
                    || field.components.Count == 0 || field.components.Any(component => !component))
                    throw new InvalidOperationException(entry.name + " has an invalid " + names[index] + " binding.");
            }

            var serialized = new UnityEditor.SerializedObject(entry);
            var slots = serialized.FindProperty("badgeSlots");
            if (slots.arraySize != 2)
                throw new InvalidOperationException(entry.name + " needs two badge slots.");

            for (int index = 0; index < slots.arraySize; index++)
            {
                var slot = slots.GetArrayElementAtIndex(index);
                var prefab = slot.FindPropertyRelative("prefab").objectReferenceValue as GameObject;
                var anchor = slot.FindPropertyRelative("anchor").objectReferenceValue as RectTransform;
                if (!anchor || !prefab || !prefab.GetComponent<UIActivityBadge>()
                    || !anchor.IsChildOf(entry.transform)
                    || slot.FindPropertyRelative("kind").intValue != index + 1)
                    throw new InvalidOperationException(entry.name + " has a missing badge dependency.");

                var badgeMetadata = prefab.GetComponent<Metadata>();
                if (!badgeMetadata || badgeMetadata.metadatas.Count != 1
                    || badgeMetadata.metadatas[0].name != "visible" || badgeMetadata.metadatas[0].type != "SyncVisible"
                    || badgeMetadata.metadatas[0].components.Count != 1
                    || badgeMetadata.metadatas[0].components[0] != prefab.transform)
                    throw new InvalidOperationException(entry.name + " has invalid badge metadata.");

                if (prefab.GetComponentsInChildren<Text>(true).Any(text => !text.font || text.text.Contains("\uFFFD")))
                    throw new InvalidOperationException(entry.name + " has missing or corrupted badge typography.");

                if (prefab.GetComponentsInChildren<Graphic>(true).Any(graphic => graphic.raycastTarget))
                    throw new InvalidOperationException(entry.name + " has a raycast-blocking badge.");
            }
        }

        return JsonConvert.SerializeObject(new { entries = entries.Length, slots = entries.Length * 2 });
    }

    /// <summary>Read active entry bindings and decoration instances.</summary>
    /// <returns>Live entries including hidden items and their requested flags.</returns>
    public static string Read()
    {
        if (!UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            throw new InvalidOperationException("Home is not open.");

        return JsonConvert.SerializeObject(widget.GetComponentsInChildren<UIActivityEntry>(true).Select(entry => new {
            id = entry.GetSystemId(),
            entry.name,
            visible = entry.gameObject.activeInHierarchy,
            bound = (bool)typeof(UIActivityEntry).GetField("bound", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(entry),
            requested = entry.vm ? (int)entry.vm.Badges : -1,
            badges = entry.GetComponentsInChildren<UIActivityBadge>(true)
                .Where(badge => badge.GetComponentInParent<UIActivityEntry>() == entry)
                .Select(badge => new { badge.name, active = badge.gameObject.activeInHierarchy,
                    raycast = badge.GetComponentsInChildren<Graphic>(true).Any(graphic => graphic.raycastTarget),
                    images = badge.GetComponentsInChildren<Image>(true).Count(image => image.sprite),
                    animated = badge.GetComponentsInChildren<Animator>(true).Any(animator => animator.runtimeAnimatorController) })
        }));
    }

    /// <summary>Drive an existing entry VM without rebinding its callbacks.</summary>
    /// <param name="id">Navigation or function identity.</param>
    /// <param name="visible">Requested visibility.</param>
    /// <param name="interactable">Requested click availability.</param>
    /// <returns>True when the authored entry exists.</returns>
    public static bool SetPresentation(int id, bool visible, bool interactable)
    {
        if (!UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            return false;

        var entry = widget.GetComponentsInChildren<UIActivityEntry>(true).FirstOrDefault(item => item.GetSystemId() == id);
        if (!entry || !entry.vm)
            return false;

        entry.vm.Visible = visible;
        entry.vm.Interactable = interactable;

        return true;
    }
}
#endif
