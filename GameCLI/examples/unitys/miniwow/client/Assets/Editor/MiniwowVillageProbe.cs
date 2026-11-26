using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Oathx.Entity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Oathx.Core;

/// <summary>Observes real village transactions without simulating completion events.</summary>
public static class MiniwowVillageProbe
{
    /// <summary>Preserves the exact pre-test task, opening and village saves.</summary>
    public static void SaveCheckpoint()
    {
        string[] keys = {
            StorageKey.For<HomeSystem, HomeEntity>(entity => entity.opening),
            StorageKey.For<HomeSystem, HomeEntity>(entity => entity.village),
            StorageKey.For<TaskSystem, TaskCollectionEntity>(entity => entity.chapterId)
        };

        var saved = new JObject();
        foreach (string key in keys)
            saved[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null;

        File.WriteAllText("Library/OpeningQA/village-checkpoint.json", saved.ToString());
    }

    /// <summary>Restores the pre-test save before stopping the editor play session.</summary>
    public static void RestoreCheckpoint()
    {
        var saved = JObject.Parse(File.ReadAllText("Library/OpeningQA/village-checkpoint.json"));
        foreach (var property in saved.Properties())
        {
            if (property.Value.Type == JTokenType.Null)
                PlayerPrefs.DeleteKey(property.Name);
            else
                PlayerPrefs.SetString(property.Name, (string)property.Value);
        }

        PlayerPrefs.Save();
    }

    /// <summary>Prepares an isolated replay of only these five tasks while retaining the user's checkpoint.</summary>
    public static void PrepareReplay()
    {
        var saved = JObject.Parse(File.ReadAllText("Library/OpeningQA/village-checkpoint.json"));
        string key = StorageKey.For<TaskSystem, TaskCollectionEntity>(entity => entity.chapterId);
        var tasks = JsonConvert.DeserializeObject<TaskCollectionSave>((string)saved[key]);
        foreach (var receipt in tasks.tasks)
        {
            if (Config.Excel.villageTaskConfig.ContainsKey(receipt.id))
            {
                receipt.progress = 0;
                receipt.status = TaskStatus.Active;
            }
        }

        PlayerPrefs.SetString(key, JsonConvert.SerializeObject(tasks));
        PlayerPrefs.DeleteKey(StorageKey.For<HomeSystem, HomeEntity>(entity => entity.village));
        PlayerPrefs.Save();
    }

    /// <summary>Checks rejected transactions, durable reward receipts and actor lifecycle boundaries.</summary>
    /// <returns>JSON assertions from isolated data fixtures with the live save restored afterward.</returns>
    public static string ValidateRules()
    {
        string key = StorageKey.For<HomeSystem, HomeEntity>(entity => entity.village);
        string original = PlayerPrefs.GetString(key);
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var checks = new JObject();
        try
        {
            var data = new HomeVillageSave();
            data.items.Add(70001, 0);
            PlayerPrefs.SetString(key, JsonConvert.SerializeObject(data));
            var fixture = new HomeVillageEntity(0);
            string before = JsonConvert.SerializeObject(fixture.Snapshot());
            checks["insufficientRepair"] = !fixture.Execute(60101, tasks)
                && before == JsonConvert.SerializeObject(fixture.Snapshot());
            checks["insufficientPurchase"] = !fixture.Execute(60102, tasks)
                && before == JsonConvert.SerializeObject(fixture.Snapshot());
            fixture.Destroy();
            data.buildings.Add(60018, 1);
            PlayerPrefs.SetString(key, JsonConvert.SerializeObject(data));
            fixture = new HomeVillageEntity(0);
            before = JsonConvert.SerializeObject(fixture.Snapshot());
            checks["missingMaterial"] = !fixture.Execute(60103, tasks)
                && before == JsonConvert.SerializeObject(fixture.Snapshot());
            var hero = fixture.hero;
            var buildings = fixture.buildings;
            fixture.Grant(Config.Excel.taskConfig[60102]);
            string granted = JsonConvert.SerializeObject(fixture.Snapshot());
            fixture.Grant(Config.Excel.taskConfig[60102]);
            checks["duplicateRewards"] = granted == JsonConvert.SerializeObject(fixture.Snapshot());
            checks["stableEntityIdentity"] = ReferenceEquals(hero, fixture.hero)
                && ReferenceEquals(buildings, fixture.buildings);
            fixture.Destroy();
            fixture = new HomeVillageEntity(0);
            fixture.Grant(Config.Excel.taskConfig[60102]);
            checks["rewardReceiptRecovery"] = granted == JsonConvert.SerializeObject(fixture.Snapshot());
            var detached = fixture.Snapshot();
            detached.items[70001] = 999999;
            checks["snapshotIsolation"] = fixture.Count(70001) == 10;
            checks["validProduction"] = fixture.Execute(60103, tasks)
                && fixture.Count(77001) == 49 && fixture.Count(77101) == 1;
            fixture.Destroy();

            var actor = new ActorEntity();

            var spawn = new ActorSpawn {
                actorId = 9001,
                configId = 125,
                moveSpeed = 1,
                route = new[] { Vector3.zero }
            };

            actor.Initialize(spawn);
            checks["emptyRouteRejected"] = !actor.FollowRoute(System.Array.Empty<Vector3>());
            checks["invalidRouteRejected"] = !actor.FollowRoute(new[] { new Vector3(float.NaN, 0, 0) });
            actor.FollowRoute(new[] { Vector3.zero, Vector3.right });
            actor.Update(1);
            checks["logicalArrival"] = actor.phase == ActorPhase.Waiting && actor.arrivals == 1;
            actor.Destroy();
            checks["destroyedActorRejected"] = !actor.FollowRoute(new[] { Vector3.right, Vector3.one });
            actor.Update(2);
            checks["destroyedActorRemainsStopped"] = actor.phase == ActorPhase.Stopped
                && actor.position == Vector3.right;
        }
        finally
        {
            PlayerPrefs.SetString(key, original);
            PlayerPrefs.Save();
        }

        if (checks.Properties().Any(property => !(bool)property.Value))
            throw new System.InvalidOperationException(checks.ToString());

        return checks.ToString();
    }

    /// <summary>Replays claims through the real task event API to check reward idempotency.</summary>
    /// <returns>Whether the live economic save remained unchanged.</returns>
    public static bool RepeatClaims()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        string before = JsonConvert.SerializeObject(home.village.Snapshot());
        foreach (int id in Config.Excel.villageTaskConfig.Keys)
            XPluginUtility.FireEvent(new TaskClaimRequested { taskId = id });

        return before == JsonConvert.SerializeObject(home.village.Snapshot());
    }

    /// <summary>Captures authoritative state and actual authored presentation.</summary>
    /// <returns>JSON runtime evidence.</returns>
    public static string Snapshot()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var village = home.village;
        var actor = home.FindActor(village.configuration.ActorID);
        var scene = SceneManager.GetSceneByPath(Config.Excel.propSceneById(Config.Excel.SCENE_HOME).Path);
        var nodes = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true));
        var overlay = Object.FindObjectOfType<UIHomeOpening>();

        return JsonConvert.SerializeObject(new {
            task = tasks.GetTrackedTask().configuration.ID,
            tasks = Config.Excel.villageTaskConfig.Keys.Select(id => new {
                id,
                status = tasks.GetTask(id).status.ToString(),
                progress = tasks.GetTask(id).progress
            }),
            save = village.Snapshot(),
            remaining = village.remainingSeconds,
            built = nodes.Single(item => item.name == "Hospital_60018")
                .Find("Model/priest").gameObject.activeInHierarchy,
            actor = new {
                id = actor.actorId,
                config = actor.configId,
                phase = actor.phase.ToString(),
                position = new[] { actor.position.x, actor.position.y, actor.position.z }
            },
            texts = overlay.GetComponentsInChildren<Text>().Select(item => new { item.name, item.text }),
            buttons = overlay.GetComponentsInChildren<Button>().Select(item => item.name)
        });
    }
}
