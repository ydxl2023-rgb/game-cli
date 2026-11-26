using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Checks configuration reuse without changing player persistence.</summary>
public static class MiniwowTaskGuideProbe
{
    /// <summary>Tests two task definitions, different targets and actual guide cancellation.</summary>
    /// <returns>The verified generic behavior.</returns>
    public static string Verify()
    {
        var table = JObject.Parse(System.IO.File.ReadAllText("Assets/Data/Jsons/Excels/TaskConfig.bytes"));
        var source = (JObject)table["data"]["60105"];
        var first = new TaskEntity(source.ToObject<Config.Task>());
        source["ID"] = 999001;
        source["FinishValue"] = 2;
        var second = new TaskEntity(source.ToObject<Config.Task>());
        source["ID"] = 999002;
        source["Param1"] = first.configuration.Param1 + 1;
        var unrelated = new TaskEntity(source.ToObject<Config.Task>());

        var report = new TaskProgressReported {
            condition = first.configuration.TaskConditionType,
            param1 = first.configuration.Param1,
            value = 1
        };

        if (!first.Apply(report) || !second.Apply(report) || unrelated.Apply(report)
            || first.status != TaskStatus.Completed || second.status != TaskStatus.Active
            || first.Apply(report) || second.Apply(report))
            throw new InvalidOperationException("Configured task matching failed.");

        var objects = new List<GameObject>();
        try
        {
            for (int index = 0; index < 2; index++)
            {
                string key = "ProbeTarget" + index;
                var root = new GameObject(key, typeof(RectTransform));
                objects.Add(root);
                root.SetActive(false);
                var button = root.AddComponent<Button>();
                var target = root.AddComponent<TaskGuideTarget>();
                var finger = new GameObject("Finger");
                finger.transform.SetParent(root.transform);
                var serialized = new SerializedObject(target);
                serialized.FindProperty("targetKey").stringValue = key;
                serialized.FindProperty("finger").objectReferenceValue = finger;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                root.SetActive(true);
                var guides = JObject.Parse(System.IO.File.ReadAllText(
                    "Assets/Data/Jsons/Excels/TaskGuideConfig.bytes"));
                var node = (JObject)guides["data"]["1018001"];
                node["ActionExpression"] = "GuideFinger(\"" + key + "\")";
                var definition = node.ToObject<Config.TaskGuide>();
                var runner = new TaskGuideRunner(new Dictionary<int, Config.TaskGuide> {
                    [definition.ID] = definition
                });

                runner.Start(definition.ID);
                runner.Tick(1f);
                if (!finger.activeSelf || runner.nodeId == 0)
                    throw new InvalidOperationException("Guide completed without a real click.");

                button.onClick.Invoke();
                runner.Tick(0f);
                if (finger.activeSelf || runner.nodeId != 0)
                    throw new InvalidOperationException("Guide did not consume the actual target click.");

                runner.Start(definition.ID);
                runner.Cancel();
                if (finger.activeSelf)
                    throw new InvalidOperationException("Canceled guide retained its finger.");

                runner.Start(definition.ID);
                root.SetActive(false);
                root.SetActive(true);
                runner.Tick(0f);
                if (runner.nodeId != 0 || finger.activeSelf)
                    throw new InvalidOperationException("Closed target retained a running guide.");

                runner.Clear();
            }
        }
        finally
        {
            foreach (var root in objects)
                UnityEngine.Object.DestroyImmediate(root);
        }

        return JsonConvert.SerializeObject(new {
            matching = true,
            differentTargets = true,
            waitsForClick = true,
            cancellation = true,
            pageClosure = true,
            saveUntouched = true
        });
    }
}
