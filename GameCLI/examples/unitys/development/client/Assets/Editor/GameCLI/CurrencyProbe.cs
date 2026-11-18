#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oathx.Widget;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Exercise currency presentation independently from the lobby.</summary>
public static class CurrencyProbe
{
    /// <summary>Validate an isolated instance and release all test objects.</summary>
    /// <returns>Checks completed against actual widgets and buttons.</returns>
    public static string Check()
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Widgets/Currency/CurrencyBar.prefab");
        var instance = UnityEngine.Object.Instantiate(asset);
        try
        {
            var bar = instance.GetComponent<UICurrencyBar>();
            var entries = instance.GetComponentsInChildren<UICurrencyEntry>(true);
            int opened = 0;
            int added = 0;
            var models = new List<VMCurrencyEntry>();
            for (int index = 0; index < 3; index++)
            {
                models.Add(new VMCurrencyEntry {
                    Amount = "123",
                    Visible = true,
                    CanAdd = true,
                    Interactable = true,
                    OpenClick = (sender) => { opened++; return true; },
                    AddClick = (sender) => { added++; return true; }
                });
            }

            var model = new VMCurrencyBar { Entries = models };
            if (!bar.Ref(model) || entries.Length != 3)
                throw new InvalidOperationException("Currency group failed to bind.");

            for (int index = 0; index < entries.Length; index++)
            {
                var metadata = entries[index].GetComponent<Metadata>();
                var text = (Text)metadata.metadatas[0].components[0];
                var open = (Button)metadata.metadatas[5].components[0];
                var add = (Button)metadata.metadatas[6].components[0];
                models[index].Amount = "456";
                if (text.text != "456" || !text.font)
                    throw new InvalidOperationException("Amount/font binding failed.");

                open.onClick.Invoke();
                add.onClick.Invoke();
                models[index].CanAdd = false;
                if (add.gameObject.activeSelf)
                    throw new InvalidOperationException("Add visibility failed.");

                models[index].CanAdd = true;
                models[index].Interactable = false;
                if (open.interactable || add.interactable)
                    throw new InvalidOperationException("Interaction state failed.");

                models[index].Interactable = true;
                models[index].Visible = false;
                if (entries[index].gameObject.activeSelf)
                    throw new InvalidOperationException("Entry visibility failed.");

                models[index].Visible = true;
            }

            if (opened != 3 || added != 3)
                throw new InvalidOperationException("Click routing failed.");

            bar.Unref(model);
            for (int index = 0; index < entries.Length; index++)
            {
                models[index].Amount = "stale";
                var metadata = entries[index].GetComponent<Metadata>();
                if (((Text)metadata.metadatas[0].components[0]).text != "456")
                    throw new InvalidOperationException("Stale VM subscription remains.");

                ((Button)metadata.metadatas[5].components[0]).onClick.Invoke();
                ((Button)metadata.metadatas[6].components[0]).onClick.Invoke();
            }

            if (opened != 3 || added != 3)
                throw new InvalidOperationException("Stale click subscriptions remain.");

            return JsonConvert.SerializeObject(new { entries = 3, opened, added, release = true });
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(instance);
        }
    }

    /// <summary>Inspect the real lobby instance and its nested prefab.</summary>
    /// <returns>Live amounts, binding count and nested prefab origin.</returns>
    public static string ReadHome()
    {
        if (!UISystem.GetSingleton().TryGetWidget<UIHome>(out var home))
            throw new InvalidOperationException("Home is not open.");

        var bar = home.GetComponentInChildren<UICurrencyBar>(true);
        var entries = bar.GetComponentsInChildren<UICurrencyEntry>(true);
        if (entries.Length != 3 || entries.Any(entry => !entry.vm))
            throw new InvalidOperationException("Home currency entries are not bound.");

        return JsonConvert.SerializeObject(new {
            amounts = entries.Select(entry => entry.vm.Amount).ToArray(),
            metadata = HomeEntryProbe.CheckInspectedBindings(),
            prefab = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(bar.gameObject)
        });
    }
}
#endif

