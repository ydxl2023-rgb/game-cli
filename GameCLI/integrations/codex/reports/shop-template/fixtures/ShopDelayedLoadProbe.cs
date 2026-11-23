using System;
using System.Collections.Generic;
using System.Reflection;
using Oathx.Widget;
using UnityEngine;
using UnityEditor;

/// <summary>Temporary controlled asynchronous cancellation regression evidence.</summary>
public static class ShopDelayedLoadProbe
{
    /// <summary>Delay resource completion and verify both UI loading overloads.</summary>
    /// <returns>Passed request identity checks.</returns>
    public static string Run()
    {
        var manager = UIResourceManager.GetSingleton();
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var loaderField = typeof(UIResourceManager).GetField("loader", flags);
        var cacheField = typeof(UIResourceManager).GetField("cacher", flags);
        var loader = loaderField.GetValue(manager);
        var cache = cacheField.GetValue(manager);
        var delayed = new DeferredLoader();
        var parent = new GameObject("ShopDelayedLoadEvidence", typeof(RectTransform));
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Widgets/Shop/ShopRecharge/ShopRecharge.prefab");
        var ui = UISystem.GetSingleton();
        var config = new ConfigAttribute("Assets/Data/Widgets/Shop/ShopRecharge/ShopRecharge.prefab", WidgetLayer.topmost, cacheWidget: false);
        var types = new Type[] { typeof(UIShopRecharge) };
        var passed = new List<string>();
        int stale = 0;
        int fresh = 0;
        try
        {
            loaderField.SetValue(manager, delayed);
            cacheField.SetValue(manager, new UIDefaultCacher());
            ui.LoadAsyncWidget<UIShopRecharge>("evidence/cancel", widget => stale++, config, parent.transform);
            ui.CloseWidget(types, immediate: true);
            delayed.Flush(prefab);
            Check(stale == 0 && parent.transform.childCount == 0, "cancel before completion");
            passed.Add("cancel before completion");

            ui.LoadAsyncWidget<UIShopRecharge>("evidence/reopen", widget => stale++, config, parent.transform);
            ui.CloseWidget(types, immediate: true);
            ui.LoadAsyncWidget<UIShopRecharge>("evidence/reopen", widget => fresh++, config, parent.transform);
            delayed.Flush(prefab);
            Check(stale == 0 && fresh == 1 && parent.transform.childCount == 1, "old completion cannot replace reopened request");
            passed.Add("generic close then reopen");
            UnityEngine.Object.DestroyImmediate(parent.transform.GetChild(0).gameObject);

            ui.LoadAsyncWidget(typeof(UIShopRecharge), "evidence/type", widget => stale++, config, parent.transform);
            ui.CloseWidget(types, immediate: true);
            ui.LoadAsyncWidget(typeof(UIShopRecharge), "evidence/type", widget => fresh++, config, parent.transform);
            delayed.Flush(prefab);
            Check(stale == 0 && fresh == 2 && parent.transform.childCount == 1, "type overload identity");
            passed.Add("type close then reopen");

            return Newtonsoft.Json.JsonConvert.SerializeObject(passed);
        }
        finally
        {
            ui.CloseWidget(types, immediate: true);
            loaderField.SetValue(manager, loader);
            cacheField.SetValue(manager, cache);
            UnityEngine.Object.DestroyImmediate(parent);
        }
    }

    private static void Check(bool value, string label)
    {
        if (!value)
            throw new InvalidOperationException(label);
    }

    private sealed class DeferredLoader : UILoader
    {
        private readonly List<Action<UnityEngine.Object>> pending = new List<Action<UnityEngine.Object>>();

        public Coroutine LoadAsync<T>(string path, Action<UnityEngine.Object> done) where T : UnityEngine.Object
        {
            pending.Add(done);

            return null;
        }

        public Coroutine LoadAsync(string path, Type type, Action<UnityEngine.Object> done)
        {
            pending.Add(done);

            return null;
        }

        public void Flush(UnityEngine.Object value)
        {
            var callbacks = pending.ToArray();
            pending.Clear();
            foreach (var callback in callbacks)
                callback(value);
        }
    }
}
