#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Oathx.Widget;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Read-only asset and temporary-instance checks for shared profile widgets.</summary>
public static class ProfilePrefabProbe
{
    /// <summary>Latest asynchronous profile verification result.</summary>
    private static string result;

    /// <summary>Read the asynchronous verification outcome.</summary>
    /// <returns>The latest verification result.</returns>
    public static string ReadResult()
    {
        return result;
    }

    /// <summary>Start verification with the configured asynchronous resource loader.</summary>
    /// <returns>The initial verification state.</returns>
    public static string Check()
    {
        result = "pending";
        Oathx.Bundle.Assets.XBundleCoroutine.Run(CheckRoutine());

        return result;
    }

    /// <summary>Verify authored defaults and explicit visibility before binding.</summary>
    /// <returns>Default and visibility behavior.</returns>
    public static string CheckDefaults()
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Widgets/Common/Micros/Profile/Profile.prefab");
        foreach (bool clear in new[] { false, true })
        {
            var instance = UnityEngine.Object.Instantiate(asset);
            try
            {
                var profile = instance.GetComponent<UIProfile>();
                var metadata = profile.GetComponent<Metadata>();
                var frame = (Image)metadata.metadatas[3].components[0];
                var authored = frame.sprite;
                var model = new VMProfile();
                if (clear)
                {
                    model.PortraitVisible = false;
                    model.FrameVisible = false;
                }

                if (!profile.Ref(model))
                    throw new InvalidOperationException("Default profile failed to bind.");

                if (clear ? frame.gameObject.activeSelf : frame.sprite != authored)
                    throw new InvalidOperationException("Default or visibility behavior changed.");

                // Destroy while bound to exercise the component-owned cleanup path.
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        return JsonConvert.SerializeObject(new { authoredDefaults = true, initialHidden = true, destroyWhileBound = true });
    }

    /// <summary>Verify that one root VM controls and releases the five movable shortcuts.</summary>
    /// <returns>Group binding, movement and release evidence.</returns>
    public static string CheckGroup()
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Widgets/Common/Micros/Profile/ProfileShortcuts.prefab");
        var instance = UnityEngine.Object.Instantiate(asset);
        try
        {
            int clicks = 0;
            var entries = new List<VMActivityEntry>();
            for (int index = 0; index < 5; index++)
            {
                entries.Add(new VMActivityEntry {
                    Visible = true,
                    Interactable = true,
                    OpenClick = (sender) => {
                        clicks++;

                        return true;
                    }
                });
            }

            var model = new VMProfileShortcuts {
                MailEntry = entries[0],
                SettingsEntry = entries[1],
                NoticeEntry = entries[2],
                FriendsEntry = entries[3],
                AffinityEntry = entries[4]
            };
            var group = instance.GetComponent<UIProfileShortcuts>();
            if (!group.Ref(model))
                throw new InvalidOperationException("Group failed to bind.");

            var children = instance.GetComponentsInChildren<UIActivityEntry>(true);
            if (children.Length != 5 || children.Any(child => !child.vm))
                throw new InvalidOperationException("Group children failed to bind.");

            var positions = children.Select(child => child.transform.position).ToArray();
            instance.transform.position += new Vector3(100, 20, 0);
            for (int index = 0; index < children.Length; index++)
            {
                if (Vector3.Distance(children[index].transform.position - positions[index], new Vector3(100, 20, 0)) > 0.01f)
                    throw new InvalidOperationException("Child did not move with its group.");

                children[index].GetComponent<Button>().onClick.Invoke();
            }

            group.Unref(model);
            foreach (var child in children)
                child.GetComponent<Button>().onClick.Invoke();

            if (clicks != 5 || group.Ref(model))
                throw new InvalidOperationException("Group release or rebind guard failed.");

            return JsonConvert.SerializeObject(new { entries = children.Length, clicks, movedTogether = true, released = true });
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(instance);
        }
    }

    /// <summary>Check the original Home layout and actual pointer targets after extraction.</summary>
    /// <returns>Hit targets and total intact binding fields.</returns>
    public static string ReadHome()
    {
        if (!UISystem.GetSingleton().TryGetWidget<UIHome>(out var home))
            throw new InvalidOperationException("Home is not open.");

        UnityEditor.Selection.activeGameObject = home.GetComponentInChildren<UIProfile>(true).gameObject;
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

        var entries = home.GetComponentsInChildren<UIActivityEntry>(true).Where(item => item.GetSystemId() <= 6).OrderBy(item => item.GetSystemId());
        var results = new List<object>();
        foreach (var entry in entries)
        {
            var rect = (RectTransform)entry.transform;
            var point = RectTransformUtility.WorldToScreenPoint(entry.GetComponentInParent<Canvas>().worldCamera, rect.TransformPoint(rect.rect.center));
            var pointer = new PointerEventData(EventSystem.current) { position = point };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            var target = hits.Count == 0 ? null : hits[0].gameObject.GetComponentInParent<Button>();
            if (!target || target != entry.GetComponent<Button>())
                throw new InvalidOperationException("Pointer blocked for " + entry.name + " at " + point + "; hits=" + string.Join(",", hits.Select(hit => hit.gameObject.name)));

            results.Add(new { id = entry.GetSystemId(), entry.name, target = target.name });
        }

        return JsonConvert.SerializeObject(new { entries = results, bindings = HomeEntryProbe.CheckInspectedBindings() });
    }
    /// <summary>Verify text, asset paths, entry callbacks and full unbinding.</summary>
    /// <returns>Evidence for the standalone profile contract.</returns>
    private static System.Collections.IEnumerator CheckRoutine()
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Widgets/Common/Micros/Profile/Profile.prefab");
        var instance = UnityEngine.Object.Instantiate(asset);
        try
        {
            var profile = instance.GetComponent<UIProfile>();
            var metadata = profile.GetComponent<Metadata>();
            var name = (Text)metadata.metadatas[0].components[0];
            var level = (Text)metadata.metadatas[1].components[0];
            var portrait = (Image)metadata.metadatas[2].components[0];
            var frame = (Image)metadata.metadatas[3].components[0];
            var first = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Data/Widgets/Common/Resources/ui_systemnew_home_17.png");
            var second = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Data/Widgets/Common/Resources/ui_systemnew_home_18.png");
            if (!first || !second)
                throw new InvalidOperationException("Missing profile test sprites.");

            int details = 0;
            int avatars = 0;
            var model = new VMProfile {
                PlayerName = "Profile Test",
                PlayerLevel = "42",
                Portrait = AssetDatabase.GetAssetPath(first),
                Frame = AssetDatabase.GetAssetPath(second),
                ProfileEntry = new VMActivityEntry {
                    Visible = true,
                    Interactable = true,
                    OpenClick = (sender) => {
                        details++;

                        return true;
                    }
                },
                AvatarEntry = new VMActivityEntry {
                    Visible = true,
                    Interactable = true,
                    OpenClick = (sender) => {
                        avatars++;

                        return true;
                    }
                }
            };
            if (!profile.Ref(model))
                throw new InvalidOperationException("Profile binding failed.");

            float deadline = Time.realtimeSinceStartup + 10f;
            while ((portrait.sprite != first || frame.sprite != second) && Time.realtimeSinceStartup < deadline)
                yield return null;

            if (name.text != "Profile Test" || level.text != "42"
                || portrait.sprite != first || frame.sprite != second)
                throw new InvalidOperationException("Initial profile data did not bind.");

            model.PlayerName = "Updated";
            model.PlayerLevel = "43";
            model.PortraitVisible = false;
            model.FrameVisible = false;
            if (name.text != "Updated" || level.text != "43" || portrait.gameObject.activeSelf || frame.gameObject.activeSelf)
                throw new InvalidOperationException("Text refresh or image hiding failed.");

            model.Portrait = AssetDatabase.GetAssetPath(second);
            model.PortraitVisible = true;
            model.Frame = AssetDatabase.GetAssetPath(first);
            model.FrameVisible = true;
            deadline = Time.realtimeSinceStartup + 10f;
            while ((portrait.sprite != second || frame.sprite != first) && Time.realtimeSinceStartup < deadline)
                yield return null;

            if (portrait.sprite != second || frame.sprite != first || !portrait.gameObject.activeSelf || !frame.gameObject.activeSelf)
                throw new InvalidOperationException("Sprite restoration failed.");

            var detailButton = metadata.metadatas[4].components[0].GetComponent<Button>();
            var avatarButton = metadata.metadatas[5].components[0].GetComponent<Button>();
            detailButton.onClick.Invoke();
            avatarButton.onClick.Invoke();
            profile.Unref(model);
            model.PlayerName = "Stale";
            model.Portrait = AssetDatabase.GetAssetPath(first);
            detailButton.onClick.Invoke();
            avatarButton.onClick.Invoke();
            if (details != 1 || avatars != 1 || name.text != "Updated" || portrait.sprite != second || profile.Ref(model))
                throw new InvalidOperationException("Profile subscriptions were not released.");

            result = JsonConvert.SerializeObject(new { text = true, paths = true, hideAndRestore = true, details, avatars, released = true });
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(instance);
        }
    }

}
#endif


