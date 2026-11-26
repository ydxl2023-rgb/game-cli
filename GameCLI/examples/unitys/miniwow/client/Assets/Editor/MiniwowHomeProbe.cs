using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Oathx.Core;
using Oathx.Widget;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Editor-only observations and raycast interactions for the Home delivery.</summary>
public static class MiniwowHomeProbe
{
    /// <summary>Verifies feedback is read from generated language data rather than the Home snapshot.</summary>
    /// <returns>The three displayed localized messages.</returns>
    public static string VerifyFeedbackLanguage()
    {
        UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget);
        var results = new[] { HomeEntryResult.Locked, HomeEntryResult.Invalid, HomeEntryResult.Allowed };
        var ids = new[] { Config.Excel.TEXT_HOME_LOCKED, Config.Excel.TEXT_HOME_INVALID, Config.Excel.TEXT_HOME_PENDING };
        var messages = new List<string>();
        for (int index = 0; index < results.Length; index++)
        {
            var entry = Config.Excel.propCommonTextById(ids[index]);
            int languageId = int.Parse(entry.Context);
            string expected = Config.Excel.propLanguageById(languageId);
            widget.ShowFeedback(results[index]);
            if (widget.vm.FeedbackText != expected || !widget.vm.FeedbackVisible)
                throw new InvalidOperationException("Home feedback did not resolve its language entry.");

            messages.Add(widget.vm.FeedbackText);
        }

        var pending = Config.Excel.propCommonTextById(Config.Excel.TEXT_HOME_PENDING);
        int pendingId = int.Parse(pending.Context);
        var language = (IDictionary<int, string>)Config.Excel.languageConfig;
        string original = language[pendingId];
        try
        {
            language[pendingId] = "Language lookup probe";
            widget.ShowFeedback(Config.Excel.TEXT_HOME_PENDING);
            if (widget.vm.FeedbackText != language[pendingId])
                throw new InvalidOperationException("Home feedback retained a separate text copy.");
        }
        finally
        {
            language[pendingId] = original;
            widget.ShowFeedback(Config.Excel.TEXT_HOME_PENDING);
            widget.vm.FeedbackVisible = false;
        }

        return JsonConvert.SerializeObject(messages);
    }

    /// <summary>Reports actual runtime state without changing it.</summary>
    /// <returns>A serialized runtime observation.</returns>
    public static string Snapshot()
    {
        UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget);
        XLauncher[] launchers = UnityEngine.Object.FindObjectsOfType<XLauncher>(true);

        return JsonConvert.SerializeObject(new {
            resolution = new[] { Screen.width, Screen.height },
            activeScene = SceneManager.GetActiveScene().path,
            homeSceneLoaded = SceneManager.GetSceneByPath("Assets/Scenes/Home.unity").isLoaded,
            homeInstances = UnityEngine.Object.FindObjectsOfType<UIHome>(true).Length,
            homeActive = widget && widget.gameObject.activeInHierarchy,
            campVisible = widget && widget.vm && widget.vm.CampVisible,
            mapVisible = widget && widget.vm && widget.vm.MapVisible,
            launchVisible = launchers.Any(value => value.gameObject.activeInHierarchy),
            animationAlpha = widget ? widget.GetComponent<CanvasGroup>().alpha : -1,
            animationTime = widget ? widget.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime : -1
        });
    }

    /// <summary>Clicks the actual raycast target at the center of an authored button.</summary>
    /// <param name="name">The exact active button name.</param>
    /// <returns>The raycast target and resulting observation.</returns>
    public static string Click(string name)
    {
        if (!UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            throw new InvalidOperationException("Home is not open.");

        Button button = widget.GetComponentsInChildren<Button>().Single(value => value.name == name);
        RectTransform rect = (RectTransform)button.transform;
        Canvas canvas = button.GetComponentInParent<Canvas>();
        Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 point = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
        PointerEventData data = new PointerEventData(EventSystem.current) {
            position = point,
            button = PointerEventData.InputButton.Left
        };
        List<RaycastResult> hits = new List<RaycastResult>();

        EventSystem.current.RaycastAll(data, hits);
        if (hits.Count == 0 || ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) != button.gameObject)
            throw new InvalidOperationException("The intended button is not the first clickable raycast target.");

        ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerClickHandler);

        return JsonConvert.SerializeObject(new {
            target = hits[0].gameObject.name,
            point = new[] { point.x, point.y },
            state = JsonConvert.DeserializeObject(Snapshot())
        });
    }

    /// <summary>Clicks an authored entry through the real graphic raycast and pointer handlers.</summary>
    public static string ClickEntry(int id)
    {
        UIHome widget = UnityEngine.Object.FindObjectOfType<UIHome>();
        var entry = widget.GetComponentsInChildren<UIHomeEntry>(true).Single(item => item.vm.entryId == id);
        Button button = entry.GetComponentInChildren<Button>();
        RectTransform rect = button.GetComponent<RectTransform>();
        Vector2 point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
        PointerEventData data = new PointerEventData(EventSystem.current) {
            position = point,
            button = PointerEventData.InputButton.Left
        };
        var hits = new List<RaycastResult>();

        EventSystem.current.RaycastAll(data, hits);
        if (hits.Count == 0 || ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) != button.gameObject)
            throw new InvalidOperationException("Entry is obscured or cannot receive clicks: " + id);

        ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerClickHandler);

        return Presentation();
    }

    /// <summary>Reports bound entry state and missing asset references.</summary>
    public static string Presentation()
    {
        UIHome widget = UnityEngine.Object.FindObjectOfType<UIHome>();

        return JsonConvert.SerializeObject(new {
            feedback = widget.vm.FeedbackText,
            feedbackVisible = widget.vm.FeedbackVisible,
            gold = widget.vm.Gold,
            energy = widget.vm.Energy,
            entries = widget.GetComponentsInChildren<UIHomeEntry>(true).Select(item => new {
                id = item.vm.entryId, visible = item.vm.Visible, locked = item.vm.Locked,
                position = new[] { item.vm.Position.x, item.vm.Position.y }
            }).ToArray(),
            missingFonts = widget.GetComponentsInChildren<Text>(true).Count(text => !text.font),
            state = JsonConvert.DeserializeObject(Snapshot())
        });
    }

    /// <summary>Refreshes repeatedly to exercise the once-only list binding contract.</summary>
    public static bool RefreshRepeatedly()
    {
        HomeSystem system = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().Query<HomeSystem>();
        for (int index = 0; index < 20; index++)
            if (!system.Refresh())
                return false;

        return true;
    }

    /// <summary>Updates local availability through the entity and refreshes the active page.</summary>
    public static bool SetEntryAvailability(int id, bool visible, bool unlocked)
    {
        if (!Oathx.Entity.XEntityManager.GetSingleton().QueryEntity<HomeEntity>().UpdateEntryAvailability(id, visible, unlocked))
            return false;

        return XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().Query<HomeSystem>().Refresh();
    }

    /// <summary>Checks snapshot isolation and rejected resource transactions.</summary>
    public static string CheckEntityOwnership()
    {
        var entity = Oathx.Entity.XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var original = entity.Snapshot();
        var copy = entity.Snapshot();
        copy.entries[0].visible = !copy.entries[0].visible;
        copy.profile.gold = -1;

        bool isolated = entity.Snapshot().entries[0].visible == original.entries[0].visible && entity.Snapshot().profile.gold == original.profile.gold;
        bool rejected = !entity.UpdateResources(-1, original.profile.gem, original.profile.energy, original.profile.energyLimit);

        return JsonConvert.SerializeObject(new {
            isolated,
            rejected,
            gold = entity.Snapshot().profile.gold
        });
    }

    /// <summary>Reports actual runtime listener counts to catch accidental callback rebinding.</summary>
    public static string ListenerCounts()
    {
        var widget = UnityEngine.Object.FindObjectOfType<UIHome>();
        var callsField = typeof(UnityEngine.Events.UnityEventBase).GetField("m_Calls", BindingFlags.NonPublic | BindingFlags.Instance);

        return JsonConvert.SerializeObject(widget.GetComponentsInChildren<UIHomeEntry>(true).Select(entry => {
            var button = entry.GetComponentInChildren<Button>(true);
            var calls = callsField.GetValue(button.onClick);
            var runtime = (System.Collections.ICollection)calls.GetType().GetField("m_RuntimeCalls", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(calls);

            return new {
                id = entry.vm.entryId,
                listeners = runtime.Count
            };
        }).ToArray());
    }

    /// <summary>Exercises the real module deactivation path.</summary>
    /// <returns>The immediate state after the request.</returns>
    public static string CloseHome()
    {
        XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().Query<HomeSystem>().Detive();

        return Snapshot();
    }

    /// <summary>Exercises the real module activation path.</summary>
    /// <returns>Whether activation was accepted.</returns>
    public static bool OpenHome()
    {
        return XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().Query<HomeSystem>().Active();
    }

    /// <summary>Exercises several lifecycle requests within a single frame.</summary>
    /// <returns>True when the final activation is accepted.</returns>
    public static bool RapidRestart()
    {
        HomeSystem system = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().Query<HomeSystem>();

        system.Detive();
        system.Active();
        system.Detive();

        return system.Active();
    }

    /// <summary>Observes presentation camera panning through its actual operation.</summary>
    /// <param name="x">Horizontal screen displacement.</param>
    /// <param name="y">Vertical screen displacement.</param>
    /// <returns>Camera handle positions and the operation result.</returns>
    public static string PanScene(float x, float y)
    {
        HomeSceneCamera controls = UnityEngine.Object.FindObjectOfType<HomeSceneCamera>();
        Camera camera = controls.GetComponent<Camera>();
        Vector3 before = camera.transform.parent.position;
        Vector2 from = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        bool accepted = controls.Pan(from, from + new Vector2(x, y));
        Vector3 after = camera.transform.parent.position;

        return JsonConvert.SerializeObject(new {
            accepted,
            before = new[] { before.x, before.y, before.z },
            after = new[] { after.x, after.y, after.z },
            displacement = Vector3.Distance(before, after)
        });
    }

    /// <summary>Selects an actual fixed-size Game View for aspect-ratio checks.</summary>
    /// <param name="width">Physical render width.</param>
    /// <param name="height">Physical render height.</param>
    public static void SetResolution(int width, int height)
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
        object size = Activator.CreateInstance(sizeType, new object[] { Enum.ToObject(kindType, 1), width, height, "Miniwow Home QA" });
        groupType.GetMethod("AddCustomSize").Invoke(group, new[] { size });

        Type viewType = assembly.GetType("UnityEditor.GameView");
        EditorWindow view = EditorWindow.GetWindow(viewType);
        viewType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(view, count);
        view.Repaint();
        view.Focus();

        Application.runInBackground = true;
    }
}
