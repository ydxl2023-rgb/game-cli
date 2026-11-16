using System.Collections.Generic;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Temporary editor-only evidence collection for the Home template.</summary>
public static class HomeTemplateProbe
{
    /// <summary>Describe the actual dispatcher and pending load state.</summary>
    /// <returns>The diagnostic state.</returns>
    public static string Diagnose()
    {
        var plugin = XPluginManager.GetSingleton().QueryPlugin<GamePlugin>();
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var modules = (System.Collections.IDictionary)typeof(XEventDispatcher).GetField("modules", flags).GetValue(plugin);
        var rows = new List<object>();
        foreach (System.Collections.DictionaryEntry row in modules)
            rows.Add(new { type = row.Key, active = ((XEventModule)row.Value).IsActive() });

        var loading = typeof(UISystem).GetField("loadingMap", flags).GetValue(UISystem.GetSingleton());

        return Newtonsoft.Json.JsonConvert.SerializeObject(new { modules = rows, loading, playing = Application.isPlaying });
    }

    /// <summary>Seed isolated in-memory display data and request Home.</summary>
    /// <returns>The local request status.</returns>
    public static bool Open()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (!entity)
            return false;

        entity.playerName = "Home Test";
        entity.playerLevel = 12;
        entity.currencyOneAmount = 120;
        entity.currencyTwoAmount = 4567;
        entity.currencyThreeAmount = 89;
        entity.chatMessage = "Local preview";

        XPluginManager.GetSingleton().QueryPlugin<GamePlugin>().Load<HomeEvidenceObserver>();

        XPluginUtility.PostEvent(new SystemDetive() { type = typeof(LoginSystem) });
        XPluginUtility.PostEvent(new SystemActive() { type = typeof(HomeSystem) });

        return true;
    }

    /// <summary>Read real text controls and raycast each button at its center.</summary>
    /// <returns>A JSON evidence report.</returns>
    public static string Inspect()
    {
        if (!UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            return "missing";

        var rows = new List<object>();
        foreach (var button in widget.GetComponentsInChildren<Button>())
        {
            var rect = (RectTransform)button.transform;
            var canvas = button.GetComponentInParent<Canvas>().rootCanvas;
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var point = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = point }, hits);
            var hit = hits.Count > 0 ? ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) : null;
            rows.Add(new { button = button.name, hit = hit ? hit.name : null, interactable = button.IsInteractable() });
        }

        var texts = new List<object>();
        foreach (var text in widget.GetComponentsInChildren<Text>())
            texts.Add(new { node = text.name, value = text.text });

        return Newtonsoft.Json.JsonConvert.SerializeObject(new { buttons = rows, texts });
    }

    /// <summary>Close the page through its lifecycle coordinator.</summary>
    /// <returns>The local request status.</returns>
    public static bool Close()
    {
        return XPluginUtility.PostEvent(new SystemDetive() { type = typeof(HomeSystem) });
    }

    /// <summary>Send pointer clicks only to buttons hit by the UI raycaster.</summary>
    /// <returns>The observed local navigation requests.</returns>
    public static string ClickAll()
    {
        if (!UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            return "missing";

        HomeEvidenceObserver.entries.Clear();

        foreach (var button in widget.GetComponentsInChildren<Button>())
        {
            var rect = (RectTransform)button.transform;
            var canvas = button.GetComponentInParent<Canvas>().rootCanvas;
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var point = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
            var pointer = new PointerEventData(EventSystem.current) { position = point, button = PointerEventData.InputButton.Left };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            var hit = hits.Count > 0 ? ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) : null;
            if (hit == button.gameObject)
                ExecuteEvents.Execute(hit, pointer, ExecuteEvents.pointerClickHandler);
        }

        return Newtonsoft.Json.JsonConvert.SerializeObject(HomeEvidenceObserver.entries);
    }

    /// <summary>Request activation and deactivation in one dispatch cycle.</summary>
    /// <returns>The queued close status.</returns>
    public static bool Race()
    {
        XPluginUtility.PostEvent(new SystemActive() { type = typeof(HomeSystem) });

        return XPluginUtility.PostEvent(new SystemDetive() { type = typeof(HomeSystem) });
    }
}

/// <summary>Temporary receiver for pointer-driven navigation evidence.</summary>
public class HomeEvidenceObserver : RuntimeSystem
{
    /// <summary>Observed requests in dispatch order.</summary>
    public static readonly List<string> entries = new List<string>();

    /// <summary>Observe local Home navigation.</summary>
    /// <returns>True after installing the observer.</returns>
    public override bool OnActive()
    {
        Subscribe<HomeEntryRequested>((data) => entries.Add(data.entry.ToString()));

        return true;
    }

    /// <summary>Release the temporary observer.</summary>
    public override void OnDetive()
    {
        UnsubscribeAll();
    }
}
