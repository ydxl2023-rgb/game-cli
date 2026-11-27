using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oathx.Entity;
using Oathx.Core;
using Oathx.Widget;
using UnityEngine;

/// <summary>Exercises forge transactions with an explicitly isolated test save.</summary>
public static class MiniwowBlacksmithProbe
{
    /// <summary>Immutable user checkpoint for this acceptance run.</summary>
    private const string checkpoint = "Library/OpeningQA/blacksmith-user-checkpoint.json";

    /// <summary>Captures user data once before any fixture mutation.</summary>
    public static void SaveCheckpoint()
    {
        if (File.Exists(checkpoint))
            throw new InvalidOperationException("Forge checkpoint already exists; preserve it until restoration.");

        MiniwowHospitalProbe.SaveCheckpoint();
        File.Copy("Library/OpeningQA/hospital-user-checkpoint.json", checkpoint);
    }

    /// <summary>Restores every typed preference from the immutable checkpoint.</summary>
    public static void RestoreCheckpoint()
    {
        File.Copy(checkpoint, "Library/OpeningQA/hospital-user-checkpoint.json", true);
        MiniwowHospitalProbe.RestoreCheckpoint();
    }

    /// <summary>Creates an unlocked first-level forge fixture for the next login.</summary>
    public static void PrepareReplay()
    {
        var saved = JObject.Parse(File.ReadAllText(checkpoint));
        string taskKey = StorageKey.For<TaskSystem, TaskCollectionEntity>(entity => entity.chapterId);
        var tasks = JObject.Parse((string)saved[taskKey]);
        var entries = (JArray)tasks["tasks"];
        foreach (var entry in entries.Where(row => (int)row["id"] == 60329).ToArray())
            entry.Remove();

        entries.Add(new JObject {
            ["id"] = 60329,
            ["progress"] = Config.Excel.taskConfig[60329].FinishValue,
            ["status"] = (int)TaskStatus.Claimed
        });
        PlayerPrefs.SetString(taskKey, tasks.ToString());
        string villageKey = StorageKey.For<HomeSystem, HomeEntity>(entity => entity.village);
        var village = JsonConvert.DeserializeObject<HomeVillageSave>((string)saved[villageKey]);
        village.blacksmith = new BlacksmithSave();
        village.buildings.Remove(60002);
        village.items[70001] = 10000;
        village.items[76001] = 240;
        village.items[76002] = 100;
        village.heroId = 125;
        village.heroGold = 1000;
        PlayerPrefs.SetString(villageKey, JsonConvert.SerializeObject(village));
        PlayerPrefs.Save();
    }

    /// <summary>Reports committed state and actual open widget counts.</summary>
    /// <returns>Serialized acceptance evidence.</returns>
    public static string Snapshot()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var forge = XEntityManager.GetSingleton().QueryEntity<BlacksmithEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var widgets = UnityEngine.Object.FindObjectsOfType<UIBlacksmith>(true);

        return JsonConvert.SerializeObject(new {
            available = forge.IsAvailable(tasks),
            selectedRecipe = forge.selectedRecipe,
            quantity = forge.quantity,
            pages = widgets.Length,
            village = home.village.Snapshot(),
            texts = widgets.SelectMany(widget => widget.GetComponentsInChildren<UnityEngine.UI.Text>())
                .Select(text => new { text.name, text.text }).ToArray()
        });
    }

    /// <summary>Clicks the actual visible row through the existing pointer raycast verifier.</summary>
    /// <param name="identity">Recipe or instance identity.</param>
    /// <param name="shelf">Whether to select a saved instance.</param>
    /// <returns>Whether the real button accepted the pointer.</returns>
    public static bool ClickEntry(long identity, bool shelf)
    {
        var row = UnityEngine.Object.FindObjectsOfType<UIBlacksmithEntry>().Single(item => {
            var view = (VMBlacksmithEntry)item.GetVM();

            return view.identity == identity && view.shelf == shelf;
        });
        var button = row.GetComponentInChildren<UnityEngine.UI.Button>();
        string name = button.name;
        button.name = "ForgeAcceptanceSelection";
        try
        {
            return MiniwowHospitalProbe.Click(button.name);
        }
        finally
        {
            button.name = name;
        }
    }

    /// <summary>Requests duplicate production confirmation through the normal event boundary.</summary>
    public static void RepeatConfirm()
    {
        XPluginUtility.FireEvent(new ReqBlacksmithAction { action = BlacksmithAction.Forge });
    }

    /// <summary>Unloads the forge through its configured lifecycle coordinator.</summary>
    public static void Unload()
    {
        XPluginUtility.FireEvent(new SystemDetive { id = 10008 });
    }

    /// <summary>Checks restored transactions and configuration-matched task observations.</summary>
    /// <returns>Persistence and task evidence.</returns>
    public static string VerifyPersistence()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var restored = new HomeVillageEntity(0);
        try
        {
            string expected = JsonConvert.SerializeObject(home.village.Snapshot());
            if (expected != JsonConvert.SerializeObject(restored.Snapshot()))
                throw new InvalidOperationException("Persisted forge transaction differs from live state.");
        }
        finally
        {
            restored.Destroy();
        }

        XPluginUtility.FireEvent(new TaskDispatchRequested { taskId = 10011 });
        XPluginUtility.FireEvent(new TaskDispatchRequested { taskId = 10023 });
        XPluginUtility.FireEvent(new BlacksmithOpenRequested());
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        if (tasks.GetTask(10011).status != TaskStatus.Completed)
            throw new InvalidOperationException("Configured forge task did not receive saved production evidence.");

        if (tasks.GetTask(10023).progress != 0)
            throw new InvalidOperationException("Unrelated weapon recipe was counted toward a filtered task.");

        foreach (int id in new[] { 60434, 60435 })
        {
            var task = new TaskEntity(Config.Excel.taskConfig[id]);

            var evidence = new TaskProgressReported {
                condition = Config.ETaskConditionType.TradeEquipmentTimes,
                itemCounts = home.village.Snapshot().blacksmith.sold
            };

            task.Apply(evidence);
            if (task.progress != 0)
                throw new InvalidOperationException("An unrelated sale advanced filtered task " + id);

            task.Destroy();
        }

        var aggregate = new TaskEntity(Config.Excel.taskConfig[10075]);

        var production = new TaskProgressReported {
            condition = Config.ETaskConditionType.BuildEquipmentTimes,
            itemCounts = new System.Collections.Generic.Dictionary<int, long> {
                [210101] = 2,
                [210102] = 1,
                [110001] = 100
            }
        };

        aggregate.Apply(production);
        bool repeated = aggregate.Apply(production);
        if (aggregate.progress != 3 || repeated)
            throw new InvalidOperationException("List aggregation or repeated replay is incorrect.");

        aggregate.Destroy();

        return "Transaction restored; recipe-list task completed; unrelated recipe task remains zero.";
    }

    /// <summary>Tests failed transactions against an unchanged durable snapshot.</summary>
    /// <returns>Individual rule assertions.</returns>
    public static string VerifyRules()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var forge = XEntityManager.GetSingleton().QueryEntity<BlacksmithEntity>();
        var saved = home.village.Snapshot();
        saved.buildings[60002] = 1;
        saved.items[76001] = 0;
        forge.SelectRecipe(110001);
        string before = JsonConvert.SerializeObject(saved);
        var execute = typeof(BlacksmithEntity).GetMethod("Execute",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        int insufficient = (int)execute.Invoke(forge, new object[] { BlacksmithAction.Forge, saved, tasks });
        if (insufficient != Config.Excel.TEXT_BLACKSMITH_NO_MATERIAL
            || before != JsonConvert.SerializeObject(saved))
            throw new InvalidOperationException("Insufficient materials changed the candidate.");

        for (int i = 0; i < 30; i++)
            forge.ChangeQuantity(1);

        if (forge.quantity != Config.Excel.blacksmithSettingConfig[60002].MaxQuantity)
            throw new InvalidOperationException("Quantity exceeded the original limit.");

        forge.Cancel();
        int empty = (int)execute.Invoke(forge, new object[] { BlacksmithAction.Forge, saved, tasks });
        if (empty != Config.Excel.TEXT_BLACKSMITH_NO_SELECTION)
            throw new InvalidOperationException("Empty confirmation was accepted.");

        saved.blacksmith.shelf.Add(new BlacksmithEquipment { id = 9001, recipeId = 110002 });
        forge.SelectEquipment(9001);
        before = JsonConvert.SerializeObject(saved);
        int incompatible = (int)execute.Invoke(forge, new object[] { BlacksmithAction.Sell, saved, tasks });
        if (incompatible != Config.Excel.TEXT_BLACKSMITH_INCOMPATIBLE
            || before != JsonConvert.SerializeObject(saved))
            throw new InvalidOperationException("Incompatible profession changed equipment or currency.");

        saved.blacksmith.shelf.Add(new BlacksmithEquipment { id = 9002, recipeId = 110001 });
        forge.SelectEquipment(9002);
        saved.heroGold = 0;
        before = JsonConvert.SerializeObject(saved);
        int poor = (int)execute.Invoke(forge, new object[] { BlacksmithAction.Sell, saved, tasks });
        if (poor != Config.Excel.TEXT_BLACKSMITH_NO_GOLD || before != JsonConvert.SerializeObject(saved))
            throw new InvalidOperationException("Insufficient hero currency changed equipment.");

        saved.heroId = 0;
        int absent = (int)execute.Invoke(forge, new object[] { BlacksmithAction.Sell, saved, tasks });
        if (absent != Config.Excel.TEXT_BLACKSMITH_NO_HERO)
            throw new InvalidOperationException("Equipment was sold without a recruited hero.");

        forge.Cancel();

        return "Materials unchanged; quantity capped; "
            + "empty confirmation, profession, hero funds and absent hero rejected.";
    }
}

