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
        PointerEventData data = new PointerEventData(EventSystem.current) { position = point, button = PointerEventData.InputButton.Left };
        List<RaycastResult> hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, hits);
        if (hits.Count == 0 || ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) != button.gameObject)
            throw new InvalidOperationException("The intended button is not the first clickable raycast target.");

        ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerClickHandler);

        return JsonConvert.SerializeObject(new { target = hits[0].gameObject.name, point = new[] { point.x, point.y }, state = JsonConvert.DeserializeObject(Snapshot()) });
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

        return JsonConvert.SerializeObject(new { accepted, before = new[] { before.x, before.y, before.z }, after = new[] { after.x, after.y, after.z }, displacement = Vector3.Distance(before, after) });
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
