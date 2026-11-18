#if UNITY_EDITOR
using System;
using System.Linq;
using Config;
using Newtonsoft.Json;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Editor-only availability, item lifecycle and coordinator regression evidence.</summary>
public static class HomeFunctionProbe
{
    /// <summary>Read actual live items and the displayed level.</summary>
    /// <returns>Serialized runtime state.</returns>
    public static string Read()
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<FunctionManagerEntity>();
        UISystem.GetSingleton().TryGetWidget<UIHome>(out var home);
        UISystem.GetSingleton().TryGetWidget<UIShop>(out var shop);
        var ids = home ? home.GetActivityIds() : new int[0];
        var widgets = home ? home.GetComponentsInChildren<UIActivityEntry>(true).Where(item => ids.Contains(item.GetSystemId())).ToArray() : new UIActivityEntry[0];

        return JsonConvert.SerializeObject(new {
            level = entity.level,
            displayLevel = home ? home.vm.Profile.PlayerLevel : null,
            items = widgets.Where(item => item.gameObject.activeInHierarchy && item.vm)
                .Select(item => new { id = item.vm.systemId, instance = item.GetInstanceID(), name = item.name }).ToArray(),
            activeTemplates = widgets.Count(item => item.gameObject.activeInHierarchy && !item.vm),
            shopOpen = shop != null
        });
    }

    /// <summary>Submit a product switch update through the real event queue.</summary>
    /// <param name="id">Function identifier.</param>
    /// <param name="enabled">Requested local switch.</param>
    /// <returns>Whether the queue accepted the input.</returns>
    public static bool Switch(int id, bool enabled)
    {
        return XPluginUtility.PostEvent(new FunctionSwitchChanged { id = id, enabled = enabled });
    }

    /// <summary>Submit a product level through the real event queue.</summary>
    /// <param name="level">New level.</param>
    /// <returns>Whether the queue accepted the input.</returns>
    public static bool Level(int level)
    {
        return XPluginUtility.PostEvent(new FunctionLevelChanged { level = level });
    }

    /// <summary>Set an in-memory level rule for a controlled runtime test.</summary>
    /// <param name="id">Function identifier.</param>
    /// <param name="minimum">Required level.</param>
    public static void RequireLevel(int id, int minimum)
    {
        Populate(JsonConvert.SerializeObject(new { Condtions = new[] { "Level:" + minimum } }), Excel.propFunctionSystemById(id));
    }

    /// <summary>Schedule a short UTC availability window in memory only.</summary>
    /// <param name="id">Function identifier.</param>
    /// <param name="startDelay">Seconds until opening.</param>
    /// <param name="stopDelay">Seconds until closing.</param>
    public static void Schedule(int id, int startDelay, int stopDelay)
    {
        var now = DateTimeOffset.UtcNow;
        Populate(JsonConvert.SerializeObject(new {
            StartTime = now.AddSeconds(startDelay).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
            StopTime = now.AddSeconds(stopDelay).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
        }), Excel.propFunctionSystemById(id));
    }

    /// <summary>Request the real Shop through Master, including its gate.</summary>
    /// <returns>Whether the request was queued.</returns>
    public static bool OpenShop()
    {
        return XPluginUtility.PostEvent(new SystemActive { type = typeof(ShopSystem) });
    }

    /// <summary>Verify pure boundary and invalid-input semantics without changing live rows.</summary>
    /// <returns>The number of passing checks.</returns>
    public static int CheckRules()
    {
        var entity = new FunctionManagerEntity();
        entity.UpdateLevel(5);
        var row = JsonConvert.DeserializeObject<Config.FunctionSystem>("{\"ID\":1,\"Condtions\":[\"Level:5\",\"Switch:1\"],\"StartTime\":\"2026-11-17T00:00:00Z\",\"StopTime\":\"2026-11-17T01:00:00Z\"}");
        var start = DateTimeOffset.Parse("2026-11-17T00:00:00Z");
        int passed = 0;
        Check(!entity.Evaluate(row, start.AddSeconds(-1)), ref passed);
        Check(entity.Evaluate(row, start), ref passed);
        Check(!entity.Evaluate(row, start.AddHours(1)), ref passed);
        Populate("{\"Condtions\":[\"Level:5\",\"Switch:0\"]}", row);
        Check(!entity.Evaluate(row, start), ref passed);
        Populate("{\"Condtions\":[\"Level:5\",\"Switch:1\"]}", row);
        entity.UpdateLevel(4);
        Check(!entity.Evaluate(row, start), ref passed);
        entity.UpdateLevel(5);
        Populate("{\"Condtions\":[\"Unknown:1\"]}", row);
        Check(!entity.Evaluate(row, start), ref passed);
        Populate("{\"Condtions\":[],\"StartTime\":\"invalid\"}", row);
        Check(!entity.Evaluate(row, start), ref passed);
        Populate("{\"StartTime\":\"\",\"StopTime\":\"\"}", row);
        Check(entity.Evaluate(row, start), ref passed);

        return passed;
    }

    /// <summary>Verify entity mutation guards and detached transition snapshots.</summary>
    /// <returns>The number of passing entity contract checks.</returns>
    public static int CheckEntity()
    {
        var entity = new FunctionManagerEntity();
        int passed = 0;
        Check(!entity.UpdateLevel(-1) && entity.level == 0, ref passed);
        Check(!entity.UpdateSwitch(int.MinValue, true), ref passed);
        Check(!entity.IsOpen(int.MinValue), ref passed);
        entity.Refresh();
        Check(entity.Refresh().Count == 0, ref passed);
        Check(entity.UpdateSwitch(12000, false) && !entity.IsOpen(12000), ref passed);
        var changes = entity.Refresh();
        Check(changes.TryGetValue(12000, out bool opened) && !opened, ref passed);
        changes[12000] = true;
        Check(entity.Refresh().Count == 0 && !entity.IsOpen(12000), ref passed);
        Check(entity.UpdateSwitch(12000, true) && entity.IsOpen(12000), ref passed);

        return passed;
    }

    /// <summary>Read runtime click listener counts after repeated list refreshes.</summary>
    /// <returns>One count per bound activity button.</returns>
    public static string Listeners()
    {
        UISystem.GetSingleton().TryGetWidget<UIHome>(out var home);
        var callsField = typeof(UnityEngine.Events.UnityEventBase).GetField("m_Calls", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var counts = new System.Collections.Generic.List<int>();
        foreach (var item in home.GetComponentsInChildren<UIActivityEntry>(true).Where(item => item.gameObject.activeInHierarchy && item.vm))
        {
            foreach (var button in item.GetComponent<Metadata>().metadatas[0].components.OfType<Button>())
            {
                var calls = callsField.GetValue(button.onClick);
                var field = calls.GetType().GetField("m_RuntimeCalls", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                counts.Add(((System.Collections.ICollection)field.GetValue(calls)).Count);
            }
        }

        return JsonConvert.SerializeObject(counts);
    }

    /// <summary>Validate the authored SyncGroup order and visibility ownership.</summary>
    /// <returns>The number of correctly bound authored items.</returns>
    public static int CheckGroup()
    {
        if (!UISystem.GetSingleton().TryGetWidget<UIHome>(out var home))
            throw new InvalidOperationException("Home is not open.");

        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var items = (UIActivityEntry[])typeof(UIHome).GetField("activities", flags).GetValue(home);
        var parent = items[0].transform.parent;
        if (items.Length != parent.childCount || home.vm.AcitiveList.Count != items.Length)
            throw new InvalidOperationException("SyncGroup count mismatch.");

        for (int index = 0; index < items.Length; index++)
        {
            var item = items[index];
            if (parent.GetChild(index) != item.transform || !item.vm
                || item.vm.systemId != item.GetSystemId())
                throw new InvalidOperationException("SyncGroup order mismatch.");
        }

        return items.Length;
    }

    /// <summary>Apply test properties without appending to existing configuration lists.</summary>
    /// <param name="json">In-memory test values.</param>
    /// <param name="target">Test configuration row.</param>
    private static void Populate(string json, object target)
    {
        JsonConvert.PopulateObject(json, target, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
    }

    /// <summary>Fail immediately on an incorrect availability result.</summary>
    /// <param name="condition">Expected assertion.</param>
    /// <param name="passed">Completed assertion counter.</param>
    private static void Check(bool condition, ref int passed)
    {
        if (!condition)
            throw new InvalidOperationException("Function availability regression at check " + passed);

        passed++;
    }
}
#endif

