using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oathx.Entity;
using Oathx.Widget;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

/// <summary>Exercises real hospital UI while preserving typed user save values.</summary>
public static class MiniwowHospitalProbe
{
    /// <summary>Typed checkpoint outside production assets.</summary>
    private const string checkpoint = "Library/OpeningQA/hospital-user-checkpoint.json";

    /// <summary>Stores the original integer opening phase and string transaction records.</summary>
    public static void SaveCheckpoint()
    {
        string opening = StorageKey.For<HomeSystem, HomeEntity>(entity => entity.opening);
        string village = StorageKey.For<HomeSystem, HomeEntity>(entity => entity.village);
        string tasks = StorageKey.For<TaskSystem, TaskCollectionEntity>(entity => entity.chapterId);

        var data = new JObject {
            [opening] = PlayerPrefs.HasKey(opening) ? new JValue(PlayerPrefs.GetInt(opening)) : JValue.CreateNull(),
            [village] = PlayerPrefs.HasKey(village) ? new JValue(PlayerPrefs.GetString(village)) : JValue.CreateNull(),
            [tasks] = PlayerPrefs.HasKey(tasks) ? new JValue(PlayerPrefs.GetString(tasks)) : JValue.CreateNull()
        };

        Directory.CreateDirectory(Path.GetDirectoryName(checkpoint));
        File.WriteAllText(checkpoint, data.ToString());
    }

    /// <summary>Restores exact values and original key absence after testing.</summary>
    public static void RestoreCheckpoint()
    {
        var data = JObject.Parse(File.ReadAllText(checkpoint));
        foreach (var property in data.Properties())
        {
            if (property.Value.Type == JTokenType.Null)
                PlayerPrefs.DeleteKey(property.Name);
            else if (property.Value.Type == JTokenType.Integer)
                PlayerPrefs.SetInt(property.Name, (int)property.Value);
            else
                PlayerPrefs.SetString(property.Name, (string)property.Value);
        }

        PlayerPrefs.Save();
    }

    /// <summary>Creates an isolated post-recruitment replay for the four hospital-related tasks.</summary>
    public static void PrepareReplay()
    {
        var data = JObject.Parse(File.ReadAllText(checkpoint));
        string tasksKey = StorageKey.For<TaskSystem, TaskCollectionEntity>(entity => entity.chapterId);
        var tasks = JsonConvert.DeserializeObject<TaskCollectionSave>((string)data[tasksKey]);
        foreach (var receipt in tasks.tasks)
        {
            if (receipt.id >= 60101 && receipt.id <= 60104)
            {
                receipt.progress = 0;
                receipt.status = TaskStatus.Active;
            }
        }

        var setting = Config.Excel.villageSettingConfig[1];

        var village = new HomeVillageSave {
            heroId = setting.HeroID,
            healthPercent = setting.InitialHealthPercent,
            heroGold = setting.HeroGold,
            recruitmentAcknowledged = true
        };

        village.items.Add(setting.GoldID, 19000);
        PlayerPrefs.SetString(tasksKey, JsonConvert.SerializeObject(tasks));
        PlayerPrefs.SetString(StorageKey.For<HomeSystem, HomeEntity>(entity => entity.village),
            JsonConvert.SerializeObject(village));
        PlayerPrefs.SetInt(StorageKey.For<HomeSystem, HomeEntity>(entity => entity.opening),
            (int)HomeOpeningPhase.AwaitRecruit);
        PlayerPrefs.Save();
    }

    /// <summary>Clicks a real active widget button through Unity's event.</summary>
    /// <param name="name">Unique authored button name.</param>
    /// <returns>Whether an interactable button was clicked.</returns>
    public static bool Click(string name)
    {
        var button = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .Single(item => item.name == name);
        if (!button.interactable)
            return false;

        var rect = (RectTransform)button.transform;
        var canvas = button.GetComponentInParent<Canvas>();
        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 point = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));

        var pointer = new PointerEventData(EventSystem.current) {
            position = point,
            button = PointerEventData.InputButton.Left
        };

        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, hits);
        if (hits.Count == 0
            || ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) != button.gameObject)
            throw new InvalidOperationException("Hospital button is obscured: " + name);

        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);

        return true;
    }

    /// <summary>Checks each restored resident's serialized range and shared effect without moving actors.</summary>
    /// <returns>Per-entrance observations.</returns>
    public static string VerifyNpcSelections()
    {
        var scene = UnityEngine.Object.FindObjectOfType<HomeScene>();
        var binding = new UnityEditor.SerializedObject(scene);
        var rows = binding.FindProperty("npcSelections");
        var field = typeof(HomeScene).GetField("npcSelections",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var selections = (HomeNpcSelection[])field.GetValue(scene);
        var result = new List<object>();
        for (int index = 0; index < rows.arraySize; index++)
        {
            var row = rows.GetArrayElementAtIndex(index);
            var entrance = (Transform)row.FindPropertyRelative("entrance").objectReferenceValue;
            var occupant = (GameObject)row.FindPropertyRelative("occupant").objectReferenceValue;
            var effect = (GameObject)row.FindPropertyRelative("effect").objectReferenceValue;
            Vector3 center = entrance.TransformPoint(row.FindPropertyRelative("center").vector3Value);
            if (!UnityEngine.AI.NavMesh.SamplePosition(center, out var ground, 3f, UnityEngine.AI.NavMesh.AllAreas))
                throw new InvalidOperationException("NPC entrance has no ground: " + occupant.name);

            center = ground.position;
            selections[index].Refresh(center, 0.5f, 1.09f, true);
            bool near = effect.activeInHierarchy == occupant.activeInHierarchy;
            selections[index].Refresh(center + Vector3.right * 100f, 0.5f, 1.09f, true);
            bool far = !effect.activeInHierarchy;
            selections[index].Refresh(center, 0.5f, 1.09f, false);
            bool gated = !effect.activeInHierarchy;
            if (!near || !far || !gated)
                throw new InvalidOperationException("NPC selection mismatch: " + occupant.name);

            result.Add(new { npc = occupant.name, near, far, gated });
        }

        return JsonConvert.SerializeObject(result);
    }

    /// <summary>Clicks the authored hospital collider through the normal scene pointer path.</summary>
    /// <returns>Whether the actual scene accepted the click.</returns>
    public static bool ClickHospitalScene()
    {
        var scene = UnityEngine.Object.FindObjectOfType<HomeScene>();
        var binding = new UnityEditor.SerializedObject(scene);
        var collider = (Collider)binding.FindProperty("hospitalCollider").objectReferenceValue;
        var camera = (Camera)binding.FindProperty("sceneCamera").objectReferenceValue;
        Vector2 point = camera.WorldToScreenPoint(collider.bounds.center);

        return scene.BeginPointer(point, -1) && scene.EndPointer(point, -1);
    }

    /// <summary>Observes entrance visibility and original world selection without changing state.</summary>
    /// <returns>Current near/far presentation.</returns>
    public static string EntranceSnapshot()
    {
        var scene = UnityEngine.Object.FindObjectOfType<HomeScene>();
        var binding = new UnityEditor.SerializedObject(scene);
        var selection = (GameObject)binding.FindProperty("hospitalSelection").objectReferenceValue;
        var entry = UnityEngine.Object.FindObjectOfType<UIHomeHospitalEntry>();
        var nodes = entry.GetComponentsInChildren<Transform>(true);

        return JsonConvert.SerializeObject(new {
            nearby = scene.IsPlayerAtHospital(),
            selection = selection.activeInHierarchy,
            button = nodes.Single(item => item.name == "EnterClick").gameObject.activeInHierarchy,
            merchantNearby = scene.IsNpcEntryVisible(60019),
            merchantButton = nodes.Single(item => item.name == "MerchantClick").gameObject.activeInHierarchy,
            forgeNearby = scene.IsNpcEntryVisible(60002),
            forgeButton = nodes.Single(item => item.name == "ForgeClick").gameObject.activeInHierarchy,
            hotelNearby = scene.IsNpcEntryVisible(60017),
            hotelButton = nodes.Single(item => item.name == "HotelClick").gameObject.activeInHierarchy,
            name = nodes.Single(item => item.name == "BuildingName").GetComponent<Text>().text
        });
    }

    /// <summary>Previews patient framing without changing treatment or saved progression.</summary>
    /// <param name="visible">Whether to show the authored patient for visual inspection.</param>
    public static void PreviewPatient(bool visible)
    {
        var widget = UnityEngine.Object.FindObjectOfType<UIHomeHospital>();
        var patient = widget.GetComponentsInChildren<Transform>(true).Single(item => item.name == "Patient");
        patient.gameObject.SetActive(visible);
    }

    /// <summary>Reads entity data and actual active UI texts for acceptance.</summary>
    /// <returns>Current state serialized as JSON.</returns>
    public static string Snapshot()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var tasks = XEntityManager.GetSingleton().QueryEntity<TaskCollectionEntity>();
        var ui = UISystem.GetSingleton();
        var texts = UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .Where(item => item.GetComponentInParent<UIHomeHospital>())
            .ToDictionary(item => item.name, item => item.text);
        var data = new {
            phase = home.opening.phase.ToString(),
            task = tasks.GetTrackedTask().configuration.ID,
            taskStatus = tasks.GetTrackedTask().status.ToString(),
            village = home.village.Snapshot(),
            hospitalOpen = ui.TryGetWidget<UIHomeHospital>(out _),
            entryOpen = ui.TryGetWidget<UIHomeHospitalEntry>(out _),
            texts = texts
        };

        return JsonConvert.SerializeObject(data);
    }
}
