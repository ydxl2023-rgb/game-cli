using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Imports data-only UI state machines without source scripts or behaviours.</summary>
    internal static class UnityCliControllerJson
    {
        internal static void Import(UnityCliBridgeRequest request, AnimatorData data)
        {
            string path = data.controllerPath;
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) || !path.EndsWith(".controller", StringComparison.OrdinalIgnoreCase)
                || path.Split('/').Any(part => part == ".." || part == "."))
            {
                throw new InvalidOperationException("Expected an Assets controller path.");
            }
            JObject input = JObject.Parse(File.ReadAllText(request.jsonFile));
            if ((string)input["schemaVersion"] != "animation-controller/1" || !(input["layers"] is JArray layers) || layers.Count == 0)
            {
                throw new InvalidOperationException("Expected animation-controller/1 with layers.");
            }
            JArray parameters = input["parameters"] as JArray ?? new JArray();
            Dictionary<string, AnimatorControllerParameterType> parameterTypes = new Dictionary<string, AnimatorControllerParameterType>();
            foreach (JObject parameter in parameters)
            {
                string name = Required(parameter, "name");
                if (!Enum.TryParse((string)parameter["type"], out AnimatorControllerParameterType type) || !Enum.IsDefined(typeof(AnimatorControllerParameterType), type)
                    || parameterTypes.ContainsKey(name))
                {
                    throw new InvalidOperationException("Invalid or duplicate controller parameter: " + name);
                }
                parameterTypes.Add(name, type);
            }
            HashSet<string> layerNames = new HashSet<string>();
            foreach (JObject layer in layers)
            {
                if (!layerNames.Add(Required(layer, "name")) || !(layer["states"] is JArray states) || states.Count == 0)
                {
                    throw new InvalidOperationException("Invalid layer or empty states.");
                }
                Dictionary<string, JObject> statesById = states.Cast<JObject>().ToDictionary(state => Required(state, "id"));
                if (!statesById.ContainsKey(Required(layer, "defaultState")))
                {
                    throw new InvalidOperationException("Default state is missing.");
                }
                foreach (JObject state in states)
                {
                    Required(state, "name");
                    string motion = (string)state["clipPath"];
                    if (motion != null && AssetDatabase.LoadAssetAtPath<AnimationClip>(motion) == null)
                    {
                        throw new InvalidOperationException("Missing controller motion: " + motion);
                    }
                    ValidateTransitions(state["transitions"] as JArray, statesById, parameterTypes);
                }
                ValidateTransitions(layer["anyStateTransitions"] as JArray, statesById, parameterTypes);
            }
            // Existing controllers are immutable here; a different definition needs a new path.
            if (File.Exists(path) || AssetDatabase.LoadMainAssetAtPath(path) != null)
            {
                throw new InvalidOperationException("Controller import requires a new asset path.");
            }
            data.message = data.dryRun ? "controller JSON dry-run passed" : "controller JSON imported";
            if (data.dryRun)
            {
                return;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            AssetDatabase.Refresh();
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            try
            {
                controller.parameters = parameters.Cast<JObject>().Select(parameter => new AnimatorControllerParameter
                {
                    name = (string)parameter["name"],
                    type = parameterTypes[(string)parameter["name"]],
                    defaultFloat = (float?)parameter["defaultFloat"] ?? 0,
                    defaultInt = (int?)parameter["defaultInt"] ?? 0,
                    defaultBool = (bool?)parameter["defaultBool"] ?? false
                }).ToArray();
                List<AnimatorControllerLayer> result = new List<AnimatorControllerLayer>();
                foreach (JObject layer in layers)
                {
                    AnimatorStateMachine machine = new AnimatorStateMachine
                    {
                        name = (string)layer["name"]
                    };
                    AssetDatabase.AddObjectToAsset(machine, controller);
                    Dictionary<string, AnimatorState> states = new Dictionary<string, AnimatorState>();
                    foreach (JObject state in (JArray)layer["states"])
                    {
                        AnimatorState created = machine.AddState((string)state["name"]);
                        created.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>((string)state["clipPath"] ?? "");
                        created.speed = (float?)state["speed"] ?? 1;
                        created.writeDefaultValues = (bool?)state["writeDefaultValues"] ?? true;
                        created.tag = (string)state["tag"] ?? "";
                        created.cycleOffset = (float?)state["cycleOffset"] ?? 0;
                        states.Add((string)state["id"], created);
                    }
                    machine.defaultState = states[(string)layer["defaultState"]];
                    foreach (JObject state in (JArray)layer["states"])
                    {
                        foreach (JObject transition in state["transitions"] as JArray ?? new JArray())
                        {
                            ApplyTransition(states[(string)state["id"]].AddTransition(states[(string)transition["to"]]), transition);
                        }
                    }
                    foreach (JObject transition in layer["anyStateTransitions"] as JArray ?? new JArray())
                    {
                        ApplyTransition(machine.AddAnyStateTransition(states[(string)transition["to"]]), transition);
                    }
                    result.Add(new AnimatorControllerLayer
                    {
                        name = (string)layer["name"],
                        stateMachine = machine,
                        defaultWeight = (float?)layer["weight"] ?? 1,
                        blendingMode = (AnimatorLayerBlendingMode)((int?)layer["blendingMode"] ?? 0)
                    });
                }
                foreach (AnimatorControllerLayer old in controller.layers)
                {
                    UnityEngine.Object.DestroyImmediate(old.stateMachine, true);
                }
                controller.layers = result.ToArray();
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
                data.assetModified = true;
                data.parameterCount = controller.parameters.Length;
                data.layerCount = controller.layers.Length;
                data.stateCount = controller.layers.Sum(layer => layer.stateMachine.states.Length);
                data.transitionCount = controller.layers.Sum(layer => layer.stateMachine.anyStateTransitions.Length + layer.stateMachine.states.Sum(state => state.state.transitions.Length));
            }
            catch
            {
                AssetDatabase.DeleteAsset(path);
                throw;
            }
        }

        private static void ValidateTransitions(JArray transitions, Dictionary<string, JObject> states, Dictionary<string, AnimatorControllerParameterType> parameters)
        {
            foreach (JObject transition in transitions ?? new JArray())
            {
                if (!states.ContainsKey(Required(transition, "to")))
                {
                    throw new InvalidOperationException("Unknown transition destination.");
                }
                foreach (JObject condition in transition["conditions"] as JArray ?? new JArray())
                {
                    if (!parameters.ContainsKey(Required(condition, "parameter"))
                        || !Enum.IsDefined(typeof(AnimatorConditionMode), (int)condition["mode"]))
                    {
                        throw new InvalidOperationException("Invalid transition condition.");
                    }
                }
            }
        }

        private static void ApplyTransition(AnimatorStateTransition target, JObject source)
        {
            target.hasExitTime = (bool?)source["hasExitTime"] ?? false;
            target.exitTime = (float?)source["exitTime"] ?? 0;
            target.hasFixedDuration = (bool?)source["hasFixedDuration"] ?? true;
            target.duration = (float?)source["duration"] ?? 0;
            target.offset = (float?)source["offset"] ?? 0;
            target.canTransitionToSelf = (bool?)source["canTransitionToSelf"] ?? true;
            target.interruptionSource = (TransitionInterruptionSource)((int?)source["interruptionSource"] ?? 0);
            target.orderedInterruption = (bool?)source["orderedInterruption"] ?? true;
            foreach (JObject condition in source["conditions"] as JArray ?? new JArray())
            {
                target.AddCondition((AnimatorConditionMode)(int)condition["mode"], (float?)condition["threshold"] ?? 0, (string)condition["parameter"]);
            }
        }

        private static string Required(JObject value, string field)
        {
            string result = (string)value[field];
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new InvalidOperationException("Missing controller field: " + field);
            }
            return result;
        }
    }
}
