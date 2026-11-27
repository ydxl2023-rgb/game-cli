using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oathx.Core;
using Oathx.Entity;
using UnityEngine;
using UnityEditor;
using Spine.Unity;

/// <summary>Exercises inn rules with isolated persistence and exposes real runtime evidence.</summary>
public static class MiniwowInnProbe
{
    /// <summary>Typed user checkpoint created by the coordinating acceptance task.</summary>
    private const string checkpoint = "Library/OpeningQA/facility-user-checkpoint.json";

    /// <summary>Checks every authored Spine layer and its actual texture and material references.</summary>
    /// <returns>Resolved render dependencies for room, innkeeper, counter and dining hero.</returns>
    public static string VerifyVisualAssets()
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Widgets/Inn/UIInn.prefab");
        var graphics = asset.GetComponentsInChildren<SkeletonGraphic>(true);
        Assert(graphics.Length == 4, "four required room and hero layers");
        foreach (var graphic in graphics)
        {
            Assert(graphic.material != null && graphic.material.shader.name == "Spine/SkeletonGraphic",
                "graphic material: " + graphic.name);
            Assert(graphic.skeletonDataAsset.GetSkeletonData(false) != null, "skeleton: " + graphic.name);
            Assert(graphic.skeletonDataAsset.atlasAssets[0].PrimaryMaterial.mainTexture != null,
                "atlas texture: " + graphic.name);
        }

        return JsonConvert.SerializeObject(graphics.Select(graphic => new {
            name = graphic.name,
            material = graphic.material.name,
            texture = graphic.skeletonDataAsset.atlasAssets[0].PrimaryMaterial.mainTexture.name,
            animation = graphic.startingAnimation
        }));
    }

    /// <summary>Inspects the actual opened room geometry, renderer state and textures.</summary>
    /// <returns>Runtime Spine layout evidence.</returns>
    public static string InspectVisuals()
    {
        var widget = UnityEngine.Object.FindObjectOfType<UIInn>();

        return JsonConvert.SerializeObject(widget.GetComponentsInChildren<SkeletonGraphic>(true).Select(graphic => {
            var rect = (RectTransform)graphic.transform;
            float[] vertices = null;
            graphic.Skeleton.GetBounds(out float x, out float y, out float width, out float height, ref vertices);

            return new {
                graphic.name, active = graphic.gameObject.activeInHierarchy,
                scale = rect.localScale.ToString(), position = rect.anchoredPosition.ToString(),
                rect = rect.rect.ToString(), pivot = rect.pivot.ToString(),
                skeletonBounds = new[] { x, y, width, height },
                texture = graphic.mainTexture.name, graphic.canvasRenderer.cull,
                material = graphic.material.name, color = graphic.color.ToString()
            };
        }));
    }

    /// <summary>Exercises transaction rejection, duplicate requests, mutual exclusion and reconnect.</summary>
    /// <returns>The accepted independent rule checks.</returns>
    public static string VerifyRules()
    {
        string key = StorageKey.For<HomeSystem, HomeEntity>(entity => entity.village);
        bool existed = PlayerPrefs.HasKey(key);
        string saved = PlayerPrefs.GetString(key);
        try
        {
            var tasks = new TaskCollectionEntity();
            tasks.OpenChapter(Config.Excel.taskSettingConfig[1].InitialChapterID);
            tasks.GetTask(60101).Restore(1, TaskStatus.Claimed);
            var inn = new InnEntity();
            var setting = Config.Excel.villageSettingConfig[1];

            var data = new HomeVillageSave {
                heroId = setting.HeroID,
                healthPercent = 100,
                heroGold = 1000,
                recruitmentAcknowledged = true,
                innEnergyInitialized = true,
                energyPercent = 0,
                recovery = HeroRecoveryPhase.Complete
            };

            data.items[setting.GoldID] = 279;
            var village = RestoreFixture(key, data);
            Assert(!village.ExecuteInn(inn, InnAction.Repair, tasks), "repair rejects insufficient gold");
            Assert(village.Count(setting.GoldID) == 279, "rejected repair keeps wallet");
            data.items[setting.GoldID] = 1000;
            village = RestoreFixture(key, data);
            Assert(village.ExecuteInn(inn, InnAction.Repair, tasks), "repair commits");
            Assert(village.Count(setting.GoldID) == 720, "repair consumes original 280");
            Assert(!village.ExecuteInn(inn, InnAction.Repair, tasks), "duplicate repair rejected");
            Assert(!village.ExecuteInn(inn, InnAction.Craft, tasks), "missing material rejected");
            data = village.Snapshot();
            data.items[77011] = 2;
            village = RestoreFixture(key, data);
            Assert(village.ExecuteInn(inn, InnAction.Craft, tasks), "food recipe commits");
            Assert(village.Count(77011) == 1 && village.Count(77111) == 1, "fish becomes food atomically");
            data = village.Snapshot();
            data.recovery = HeroRecoveryPhase.Healing;
            village = RestoreFixture(key, data);
            Assert(!village.ExecuteInn(inn, InnAction.Rest, tasks), "hospital and inn exclude each other");
            data.recovery = HeroRecoveryPhase.Complete;
            village = RestoreFixture(key, data);
            Assert(village.ExecuteInn(inn, InnAction.Rest, tasks), "meal starts");
            Assert(!village.ExecuteInn(inn, InnAction.Rest, tasks), "duplicate meal rejected");
            Assert(village.Count(77111) == 0 && village.hero.gold == 983, "meal charges once");
            Assert(!village.ExecuteInn(inn, InnAction.Complete, tasks), "future deadline cannot complete");
            data = village.Snapshot();
            data.innRestEndTicks = DateTime.UtcNow.AddSeconds(-1).Ticks;
            village = RestoreFixture(key, data);
            Assert(village.ExecuteInn(inn, InnAction.Complete, tasks), "elapsed meal completes after restore");
            Assert(!village.ExecuteInn(inn, InnAction.Complete, tasks), "completion is idempotent");
            var restored = new HomeVillageEntity(0);
            Assert(restored.Snapshot().energyPercent == 100 && restored.Snapshot().innMeals == 1,
                "energy and use observation persist");
            Assert(restored.hero.gold == 983 && restored.Snapshot().innRevenue == 17,
                "reconnect never charges again");
            Assert(!restored.ExecuteInn(inn, InnAction.Rest, tasks), "full energy rejects new meal");

            return JsonConvert.SerializeObject(new {
                repair = true, production = true, insufficientResources = true, duplicateRequests = true,
                hospitalExclusion = true, deadline = true, restore = true, fullEnergy = true
            });
        }
        finally
        {
            if (existed)
                PlayerPrefs.SetString(key, saved);
            else
                PlayerPrefs.DeleteKey(key);

            PlayerPrefs.Save();
        }
    }

    /// <summary>Prepares a replay at inn repair without altering the retained user checkpoint.</summary>
    public static void PrepareReplay()
    {
        var original = JObject.Parse(File.ReadAllText(checkpoint));
        string tasksKey = StorageKey.For<TaskSystem, TaskCollectionEntity>(entity => entity.chapterId);
        string villageKey = StorageKey.For<HomeSystem, HomeEntity>(entity => entity.village);
        var tasks = JsonConvert.DeserializeObject<TaskCollectionSave>((string)original[tasksKey]);
        var chapter = Config.Excel.taskChapterConfig[tasks.chapterId];
        int target = chapter.TaskIDs.IndexOf(60107);
        foreach (var receipt in tasks.tasks)
        {
            int index = chapter.TaskIDs.IndexOf(receipt.id);
            if (index >= 0 && index < target)
            {
                receipt.status = TaskStatus.Claimed;
                receipt.progress = Config.Excel.taskConfig[receipt.id].FinishValue;
            }
            else if (index >= target)
            {
                receipt.status = TaskStatus.Active;
                receipt.progress = 0;
            }
        }

        var data = JsonConvert.DeserializeObject<HomeVillageSave>((string)original[villageKey]);
        data.buildings.Remove(60017);
        data.items[77011] = 0;
        data.items[77111] = 0;
        data.innCrafts = 0;
        data.innMeals = 0;
        data.innRestEndTicks = 0;
        data.innRevenue = 0;
        data.energyPercent = 0;
        data.innEnergyInitialized = true;
        data.recovery = HeroRecoveryPhase.Complete;
        data.heroId = Config.Excel.villageSettingConfig[1].HeroID;
        data.heroGold = Config.Excel.villageSettingConfig[1].HeroGold;
        foreach (int id in chapter.TaskIDs.Take(target))
            data.receipts.Add(id);

        PlayerPrefs.SetString(tasksKey, JsonConvert.SerializeObject(tasks));
        PlayerPrefs.SetString(villageKey, JsonConvert.SerializeObject(data));
        PlayerPrefs.Save();
    }

    /// <summary>Returns current durable inn state and current task selection.</summary>
    /// <returns>Detached runtime evidence.</returns>
    public static string Snapshot()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var task = tasks.GetTrackedTask();

        return JsonConvert.SerializeObject(new {
            village = home.village.Snapshot(),
            task = task?.configuration.ID,
            status = task?.status.ToString()
        });
    }

    /// <summary>Requests a standalone inn reload through the production coordinator.</summary>
    public static void Reload()
    {
        XPluginUtility.FireEvent(new SystemActive { id = 10007 });
    }

    /// <summary>Requests independent inn release while preserving its durable meal.</summary>
    public static void Unload()
    {
        XPluginUtility.FireEvent(new SystemDetive { id = 10007 });
    }

    /// <summary>Restores a detached fixture into temporary persistence.</summary>
    /// <param name="key">Original product persistence key.</param>
    /// <param name="data">Isolated candidate.</param>
    /// <returns>A detached entity loaded through production restore.</returns>
    private static HomeVillageEntity RestoreFixture(string key, HomeVillageSave data)
    {
        PlayerPrefs.SetString(key, JsonConvert.SerializeObject(data));

        return new HomeVillageEntity(0);
    }

    /// <summary>Rejects a failed acceptance assertion with its exact rule name.</summary>
    /// <param name="condition">Observed rule result.</param>
    /// <param name="name">Human-readable acceptance case.</param>
    private static void Assert(bool condition, string name)
    {
        if (!condition)
            throw new InvalidOperationException("INN_ACCEPTANCE_FAILED: " + name);
    }
}
