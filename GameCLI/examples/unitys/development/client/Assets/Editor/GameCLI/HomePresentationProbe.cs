#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Editor-only runtime evidence for the Home presentation repair.</summary>
public static class HomePresentationProbe
{
    /// <summary>Inspect text render state and screen bounds without changing presentation.</summary>
    /// <returns>The active labels and their rendering dependencies.</returns>
    public static string ReadTextRendering()
    {
        if (!UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            return "Home is not open";

        return JsonConvert.SerializeObject(widget.GetComponentsInChildren<Text>(true)
            .Where(text => text.gameObject.activeInHierarchy && !string.IsNullOrEmpty(text.text))
            .Select(text => {
                var corners = new Vector3[4];
                text.rectTransform.GetWorldCorners(corners);
                var canvas = text.canvas;
                var material = text.materialForRendering;

                return new {
                    text.name, text.text,
                    channels = canvas.additionalShaderChannels.ToString(),
                    keywords = material.shaderKeywords,
                    clip = material.HasProperty("_ClipRect") ? material.GetVector("_ClipRect").ToString() : "none",
                    corners = corners.Select(corner => {
                        var point = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corner);

                        return new[] { point.x, point.y };
                    }),
                    vertices = text.cachedTextGenerator.vertexCount,
                    cull = text.canvasRenderer.cull
                };
            }));
    }

    /// <summary>Remove temporary blank placeholders through the Unity prefab API.</summary>
    /// <returns>The number of normalized dynamic labels.</returns>
    public static int NormalizePrefabText()
    {
        const string path = "Assets/Data/Widgets/Home/Home.prefab";
        var root = UnityEditor.PrefabUtility.LoadPrefabContents(path);
        try
        {
            var texts = root.GetComponentsInChildren<Text>(true).Where(text => text.text == " ").ToArray();
            if (texts.Length != 32)
                throw new InvalidOperationException("Unexpected Home placeholder count.");

            foreach (var text in texts)
                text.text = string.Empty;

            UnityEditor.PrefabUtility.SaveAsPrefabAsset(root, path);

            return texts.Length;
        }
        finally
        {
            UnityEditor.PrefabUtility.UnloadPrefabContents(root);
        }
    }

    /// <summary>Read the actual widget, fonts and visible promotion state.</summary>
    /// <returns>Runtime evidence serialized for UnityCLI.</returns>
    public static string Read()
    {
        if (!UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            return "Home is not open";

        var texts = widget.GetComponentsInChildren<Text>(true);
        var images = widget.GetComponentsInChildren<Image>(true);
        var animators = widget.GetComponentsInChildren<Animator>(true);

        return JsonConvert.SerializeObject(new {
            texts = texts.Where(t => t.gameObject.activeInHierarchy).Select(t => new { t.name, t.text, font = t.font ? t.font.name : null, shader = t.material.shader.name }),
            indicators = images.Where(i => i.gameObject.activeInHierarchy && (i.name.Contains("NewTag") || i.name.Contains("DoubleReward"))).Select(i => new { i.name, sprite = i.sprite ? i.sprite.name : null, size = new[] { i.rectTransform.sizeDelta.x, i.rectTransform.sizeDelta.y } }),
            activities = widget.GetComponentsInChildren<Button>(true).Where(b => b.transform.parent.name == "AcitiveList").Select(b => new { b.name, active = b.gameObject.activeInHierarchy, position = new[] { ((RectTransform)b.transform).anchoredPosition.x, ((RectTransform)b.transform).anchoredPosition.y } }),
            animators = animators.Where(a => a.gameObject.activeInHierarchy).Select(a => new { a.name, controller = a.runtimeAnimatorController ? a.runtimeAnimatorController.name : null, time = a.GetCurrentAnimatorStateInfo(0).normalizedTime }),
            invalidFonts = texts.Count(t => !t.font || t.font.name == "LegacyRuntime")
        });
    }

    /// <summary>Drive a real pointer click after checking the current raycast target.</summary>
    /// <param name="name">Exact serialized button name.</param>
    /// <returns>The clicked target or a failure reason.</returns>
    public static string Click(string name)
    {
        var buttons = UnityEngine.Object.FindObjectsOfType<Button>();
        var matches = buttons.Where(b => b.name == name && b.gameObject.activeInHierarchy).ToArray();
        if (matches.Length != 1 || !EventSystem.current)
            return "Button is missing or ambiguous";

        var button = matches[0];
        var rect = (RectTransform)button.transform;
        var canvas = button.GetComponentInParent<Canvas>();
        var position = RectTransformUtility.WorldToScreenPoint(canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, rect.TransformPoint(rect.rect.center));
        var pointer = new PointerEventData(EventSystem.current) { position = position, button = PointerEventData.InputButton.Left };
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, hits);
        if (hits.Count == 0 || ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) != button.gameObject)
            return "Button is blocked by another raycast target";

        ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerClickHandler);

        return "Clicked " + name;
    }

    /// <summary>Exercise the same entity-to-view refresh used by the product.</summary>
    /// <param name="state">None, Double or Triple.</param>
    /// <returns>True when the open widget accepted the state.</returns>
    public static bool SetDrop(string state)
    {
        if (!Enum.TryParse(state, out HomeDropState parsed) || !Enum.IsDefined(typeof(HomeDropState), parsed))
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (!entity)
            return false;

        if (!UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            return false;

        entity.dropState = parsed;

        return widget.RefreshPresentation(entity);
    }

    /// <summary>Toggle one independent visibility key for runtime verification.</summary>
    /// <param name="name">The exact indicator enum name.</param>
    /// <param name="visible">Whether to show the target.</param>
    /// <returns>True when the serialized view was refreshed.</returns>
    public static bool SetIndicator(string name, bool visible)
    {
        if (!Enum.TryParse(name, out HomeIndicator indicator) || !Enum.IsDefined(typeof(HomeIndicator), indicator))
            return false;

        var entity = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        if (!entity)
            return false;

        if (!UISystem.GetSingleton().TryGetWidget<UIHome>(out var widget))
            return false;

        entity.indicators[indicator] = visible;

        return widget.RefreshPresentation(entity);
    }

    /// <summary>Close the test page through its real coordinator.</summary>
    public static void Close()
    {
        XPluginUtility.FireEvent(new SystemDetive { type = typeof(HomeSystem) });
    }

    /// <summary>Reopen the test page through its real coordinator.</summary>
    public static void Open()
    {
        XPluginUtility.FireEvent(new SystemActive { type = typeof(HomeSystem) });
    }
}
#endif
