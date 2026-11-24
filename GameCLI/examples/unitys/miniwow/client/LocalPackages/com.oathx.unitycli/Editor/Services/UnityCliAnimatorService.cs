using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace Oathx.UnityCLI.Editor
{
    internal static class UnityCliAnimatorService
    {
        public static AnimatorData Execute(UnityCliBridgeRequest request)
        {
            if (request == null) throw new InvalidOperationException("Empty request.");

            var action = Normalize(request.action);
            var operation = Normalize(request.animatorOperation);
            var data = new AnimatorData
            {
                action = action,
                operation = operation,
                controllerPath = NormalizeAssetPath(request.animatorControllerPath),
                clipPath = NormalizeAssetPath(request.animationClipPath),
                objectPath = request.objectPath ?? string.Empty,
                layer = string.IsNullOrWhiteSpace(request.animatorLayer) ? "Base Layer" : request.animatorLayer.Trim(),
                stateName = request.animatorStateName ?? string.Empty,
                parameterName = request.animatorParameterName ?? string.Empty,
                parameterType = request.animatorParameterType ?? string.Empty,
                fromState = request.animatorFromState ?? string.Empty,
                toState = request.animatorToState ?? string.Empty,
                dryRun = request.dryRun,
                respondedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            };

            switch (action)
            {
                case "controller":
                    ExecuteController(request, data);
                    break;
                case "parameter":
                    ExecuteParameter(request, data);
                    break;
                case "clip":
                    ExecuteClip(request, data);
                    break;
                case "state":
                    ExecuteState(request, data);
                    break;
                case "transition":
                    ExecuteTransition(request, data);
                    break;
                case "bind":
                    ExecuteBind(request, data);
                    break;
                case "inspect":
                    ExecuteInspect(request, data);
                    break;
                default:
                    throw new InvalidOperationException("animator action must be controller, parameter, clip, state, transition, bind, or inspect.");
            }

            return data;
        }

        static void ExecuteController(UnityCliBridgeRequest request, AnimatorData data)
        {
            if (data.operation == "import-json")
            {
                UnityCliControllerJson.Import(request, data);
                return;
            }
            if (data.operation != "create") throw new InvalidOperationException("animator controller supports operation create.");
            RequireControllerPath(data.controllerPath);

            data.details = new[] { "create controller " + data.controllerPath };
            data.message = data.dryRun ? "animator controller dry-run passed" : "animator controller created";
            if (data.dryRun) return;

            EnsureParentFolder(data.controllerPath);
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(data.controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(data.controllerPath);
            }

            data.assetModified = true;
            FillControllerSummary(controller, data);
            AssetDatabase.SaveAssets();
        }

        static void ExecuteParameter(UnityCliBridgeRequest request, AnimatorData data)
        {
            if (data.operation != "set" && data.operation != "add" && data.operation != "remove")
            {
                throw new InvalidOperationException("animator parameter supports add, set, or remove.");
            }

            var controller = LoadController(data.controllerPath);
            var parameterName = RequireName(request.animatorParameterName, "--parameter");
            data.parameterName = parameterName;
            data.parameterType = string.IsNullOrWhiteSpace(request.animatorParameterType) || request.animatorParameterType == "all"
                ? "Float"
                : request.animatorParameterType.Trim();

            data.details = new[] { data.operation + " parameter " + parameterName };
            data.message = data.dryRun ? "animator parameter dry-run passed" : "animator parameter updated";
            if (!data.dryRun)
            {
                var existing = controller.parameters.FirstOrDefault(p => p.name == parameterName);
                if (existing != null) controller.RemoveParameter(existing);

                if (data.operation != "remove")
                {
                    var type = ParseParameterType(data.parameterType);
                    controller.AddParameter(parameterName, type);
                    ApplyParameterDefault(controller, parameterName, type, request.animatorDefaultValue);
                }

                EditorUtility.SetDirty(controller);
                data.assetModified = true;
                AssetDatabase.SaveAssets();
            }

            FillControllerSummary(controller, data);
        }

        static void ExecuteClip(UnityCliBridgeRequest request, AnimatorData data)
        {
            if (data.operation == "import-json" || data.operation == "export-json")
            {
                ExecuteClipJson(request, data);
                return;
            }
            if (data.operation == "create")
            {
                RequireClipPath(data.clipPath);
                data.details = new[] { "create clip " + data.clipPath };
                data.message = data.dryRun ? "animation clip dry-run passed" : "animation clip created";
                if (data.dryRun) return;

                EnsureParentFolder(data.clipPath);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(data.clipPath);
                if (clip == null)
                {
                    clip = new AnimationClip();
                    AssetDatabase.CreateAsset(clip, data.clipPath);
                }

                if (request.animatorLoopSet) SetClipLoop(clip, request.animatorLoop);
                EditorUtility.SetDirty(clip);
                data.assetModified = true;
                FillClipSummary(clip, data);
                AssetDatabase.SaveAssets();
                return;
            }

            if (data.operation == "set-curve")
            {
                var clip = LoadClip(data.clipPath);
                var bindingPath = request.objectPath ?? string.Empty;
                var componentType = ResolveComponentType(request.component);
                if (string.IsNullOrWhiteSpace(request.property)) throw new InvalidOperationException("--property is required.");
                var curve = ParseCurve(request.animatorKeys);

                data.details = new[]
                {
                    "set curve " + bindingPath + " " + componentType.Name + "." + request.property,
                    "keys " + request.animatorKeys
                };
                data.message = data.dryRun ? "animation curve dry-run passed" : "animation curve updated";
                if (!data.dryRun)
                {
                    var binding = EditorCurveBinding.FloatCurve(bindingPath, componentType, request.property);
                    AnimationUtility.SetEditorCurve(clip, binding, curve);
                    if (request.animatorLoopSet) SetClipLoop(clip, request.animatorLoop);
                    EditorUtility.SetDirty(clip);
                    data.assetModified = true;
                    AssetDatabase.SaveAssets();
                }

                FillClipSummary(clip, data);
                return;
            }

            if (data.operation == "sample")
            {
                var clip = LoadClip(data.clipPath);
                var target = FindSceneObject(request.objectPath);
                if (target == null) throw new InvalidOperationException("Scene object not found: " + request.objectPath);
                var sampleTime = request.animatorSampleTimeSet
                    ? Mathf.Clamp(request.animatorSampleTime, 0f, Mathf.Max(0f, clip.length))
                    : 0f;

                data.details = new[]
                {
                    "sample clip " + data.clipPath + " on " + request.objectPath,
                    "time " + sampleTime.ToString("0.###", CultureInfo.InvariantCulture)
                };
                data.message = data.dryRun ? "animation clip sample dry-run passed" : "animation clip sampled";
                if (!data.dryRun)
                {
                    clip.SampleAnimation(target, sampleTime);
                    EditorUtility.SetDirty(target);
                    data.sceneModified = true;
                }

                FillClipSummary(clip, data);
                return;
            }

            throw new InvalidOperationException("animator clip supports create, set-curve, or sample.");
        }

        // JSON transport preserves the full numeric curve data instead of lossy CLI strings.
        private static void ExecuteClipJson(UnityCliBridgeRequest request, AnimatorData data)
        {
            RequireClipPath(data.clipPath);
            if (data.operation == "export-json")
            {
                data.details = new[] { ExportClipJson(LoadClip(data.clipPath)).ToString(Newtonsoft.Json.Formatting.None) };
                data.message = "animation clip full curve export";
                return;
            }
            JObject input = JObject.Parse(File.ReadAllText(request.jsonFile));
            if ((string)input["schemaVersion"] != "animation-clip/1" || !(input["curves"] is JArray curves)
                || input["loop"]?.Type != JTokenType.Boolean)
            {
                throw new InvalidOperationException("Expected animation-clip/1 with curves and loop.");
            }
            float rate = ReadCurveNumber(input, "frameRate");
            if (rate <= 0)
            {
                throw new InvalidOperationException("frameRate must be positive.");
            }
            AnimationClip candidate = new AnimationClip { frameRate = rate };
            try
            {
                HashSet<string> seen = new HashSet<string>();
                foreach (JObject item in curves)
                {
                    string path = (string)item["path"];
                    string property = (string)item["property"];
                    Type type = ResolveComponentType((string)item["component"]);
                    if (path == null || string.IsNullOrWhiteSpace(property) || !seen.Add(path + "|" + type.FullName + "|" + property))
                    {
                        throw new InvalidOperationException("Missing or duplicate curve binding.");
                    }
                    JArray keys = item["keys"] as JArray;
                    if (keys == null || keys.Count == 0)
                    {
                        throw new InvalidOperationException("Curve requires keys.");
                    }
                    List<Keyframe> frames = new List<Keyframe>();
                    float previous = -1;
                    foreach (JObject key in keys)
                    {
                        float time = ReadCurveNumber(key, "time");
                        if (time < 0 || time <= previous)
                        {
                            throw new InvalidOperationException("Key times must be nonnegative and strictly increasing.");
                        }
                        previous = time;
                        Keyframe frame = new Keyframe(time, ReadCurveNumber(key, "value"),
                            ReadCurveNumber(key, "inSlope", true), ReadCurveNumber(key, "outSlope", true),
                            ReadCurveNumber(key, "inWeight"), ReadCurveNumber(key, "outWeight"));
                        int weighted = (int)key["weightedMode"];
                        if (weighted < 0 || weighted > 3)
                        {
                            throw new InvalidOperationException("Invalid weightedMode.");
                        }
                        frame.weightedMode = (WeightedMode)weighted;
                        // Preserve Unity's serialized tangent bitfield without recomputing slopes.
#pragma warning disable 0618
                        frame.tangentMode = (int)key["tangentMode"];
#pragma warning restore 0618
                        frames.Add(frame);
                    }
                    AnimationCurve curve = new AnimationCurve(frames.ToArray());
                    curve.preWrapMode = ReadWrapMode(item, "preWrapMode");
                    curve.postWrapMode = ReadWrapMode(item, "postWrapMode");
                    AnimationUtility.SetEditorCurve(candidate, EditorCurveBinding.FloatCurve(path, type, property), curve);
                }
                SetClipLoop(candidate, (bool)input["loop"]);
                if (!data.dryRun)
                {
                    EnsureParentFolder(data.clipPath);
                    AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(data.clipPath);
                    if (existing == null)
                    {
                        existing = new AnimationClip();
                        AssetDatabase.CreateAsset(existing, data.clipPath);
                    }
                    // Validate all curves first, then replace the clip as a single operation.
                    EditorUtility.CopySerialized(candidate, existing);
                    existing.name = Path.GetFileNameWithoutExtension(data.clipPath);
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                    data.assetModified = true;
                    data.details = new[] { ExportClipJson(existing).ToString(Newtonsoft.Json.Formatting.None) };
                }
                else
                {
                    data.details = new[] { ExportClipJson(candidate).ToString(Newtonsoft.Json.Formatting.None) };
                }
                data.message = data.dryRun ? "animation JSON validated" : "animation JSON imported";
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(candidate);
            }
        }

        private static float ReadCurveNumber(JObject item, string key, bool allowInfinity = false)
        {
            JToken value = item[key];
            if (value == null)
            {
                throw new InvalidOperationException("Missing curve field: " + key);
            }
            float number = float.Parse(value.ToString(), CultureInfo.InvariantCulture);
            if (float.IsNaN(number) || (!allowInfinity && float.IsInfinity(number)))
            {
                throw new InvalidOperationException("Invalid curve number: " + key);
            }
            return number;
        }

        private static WrapMode ReadWrapMode(JObject item, string key)
        {
            int value = (int)item[key];
            if (!Enum.IsDefined(typeof(WrapMode), value))
            {
                throw new InvalidOperationException("Invalid wrap mode: " + key);
            }
            return (WrapMode)value;
        }

        private static JObject ExportClipJson(AnimationClip clip)
        {
            JArray curves = new JArray();
            foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
            {
                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                JArray keys = new JArray();
                foreach (Keyframe key in curve.keys)
                {
#pragma warning disable 0618
                    keys.Add(new JObject
                    {
                        ["time"] = key.time, ["value"] = key.value,
                        ["inSlope"] = key.inTangent, ["outSlope"] = key.outTangent,
                        ["inWeight"] = key.inWeight, ["outWeight"] = key.outWeight,
                        ["weightedMode"] = (int)key.weightedMode, ["tangentMode"] = key.tangentMode
                    });
#pragma warning restore 0618
                }
                curves.Add(new JObject
                {
                    ["path"] = binding.path, ["component"] = binding.type.FullName,
                    ["property"] = binding.propertyName, ["keys"] = keys,
                    ["preWrapMode"] = (int)curve.preWrapMode, ["postWrapMode"] = (int)curve.postWrapMode
                });
            }
            return new JObject
            {
                ["schemaVersion"] = "animation-clip/1", ["frameRate"] = clip.frameRate,
                ["loop"] = AnimationUtility.GetAnimationClipSettings(clip).loopTime, ["curves"] = curves
            };
        }

        static void ExecuteState(UnityCliBridgeRequest request, AnimatorData data)
        {
            if (data.operation != "add") throw new InvalidOperationException("animator state supports operation add.");
            var controller = LoadController(data.controllerPath);
            var stateName = RequireName(request.animatorStateName, "--state");
            var layer = FindLayer(controller, data.layer);
            var state = FindState(layer.stateMachine, stateName);
            var clip = string.IsNullOrWhiteSpace(data.clipPath) ? null : LoadClip(data.clipPath);

            data.details = new[] { "add state " + stateName + " on layer " + layer.name };
            data.message = data.dryRun ? "animator state dry-run passed" : "animator state added";
            if (!data.dryRun)
            {
                if (state == null) state = layer.stateMachine.AddState(stateName);
                if (clip != null) state.motion = clip;
                layer.stateMachine.defaultState = state;
                EditorUtility.SetDirty(layer.stateMachine);
                EditorUtility.SetDirty(controller);
                data.assetModified = true;
                AssetDatabase.SaveAssets();
            }

            FillControllerSummary(controller, data);
        }

        static void ExecuteTransition(UnityCliBridgeRequest request, AnimatorData data)
        {
            if (data.operation != "add") throw new InvalidOperationException("animator transition supports operation add.");
            var controller = LoadController(data.controllerPath);
            var layer = FindLayer(controller, data.layer);
            var from = FindState(layer.stateMachine, RequireName(request.animatorFromState, "--from"));
            var to = FindState(layer.stateMachine, RequireName(request.animatorToState, "--to"));
            if (from == null) throw new InvalidOperationException("From state not found: " + request.animatorFromState);
            if (to == null) throw new InvalidOperationException("To state not found: " + request.animatorToState);

            data.details = new[] { "add transition " + from.name + " -> " + to.name };
            data.message = data.dryRun ? "animator transition dry-run passed" : "animator transition added";
            if (!data.dryRun)
            {
                var transition = from.AddTransition(to);
                transition.hasExitTime = false;
                transition.duration = request.animatorDurationSet ? Mathf.Max(0f, request.animatorDuration) : 0.1f;
                if (!string.IsNullOrWhiteSpace(request.animatorCondition))
                {
                    AddCondition(transition, request.animatorCondition);
                }

                EditorUtility.SetDirty(controller);
                data.assetModified = true;
                AssetDatabase.SaveAssets();
            }

            FillControllerSummary(controller, data);
        }

        static void ExecuteBind(UnityCliBridgeRequest request, AnimatorData data)
        {
            var controller = LoadController(data.controllerPath);
            var target = FindSceneObject(request.objectPath);
            if (target == null) throw new InvalidOperationException("Scene object not found: " + request.objectPath);

            data.details = new[] { "bind " + request.objectPath + " to " + data.controllerPath };
            data.message = data.dryRun ? "animator bind dry-run passed" : "animator bound";
            if (!data.dryRun)
            {
                var animator = target.GetComponent<Animator>();
                if (animator == null) animator = target.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                EditorUtility.SetDirty(target);
                data.sceneModified = true;
            }

            FillControllerSummary(controller, data);
        }

        static void ExecuteInspect(UnityCliBridgeRequest request, AnimatorData data)
        {
            if (!string.IsNullOrWhiteSpace(data.controllerPath))
            {
                var controller = LoadController(data.controllerPath);
                FillControllerSummary(controller, data);
                data.details = BuildControllerDetails(controller).ToArray();
                data.message = "animator controller inspected";
                return;
            }

            if (!string.IsNullOrWhiteSpace(data.clipPath))
            {
                var clip = LoadClip(data.clipPath);
                FillClipSummary(clip, data);
                data.details = BuildClipDetails(clip).ToArray();
                data.message = "animation clip inspected";
                return;
            }

            throw new InvalidOperationException("inspect requires --controller or --clip.");
        }

        static void FillControllerSummary(AnimatorController controller, AnimatorData data)
        {
            data.parameterCount = controller.parameters.Length;
            data.layerCount = controller.layers.Length;
            data.stateCount = controller.layers.Sum(layer => layer.stateMachine.states.Length);
            data.transitionCount = controller.layers.Sum(layer => layer.stateMachine.anyStateTransitions.Length + layer.stateMachine.states.Sum(state => state.state.transitions.Length));
        }

        static void FillClipSummary(AnimationClip clip, AnimatorData data)
        {
            data.curveCount = AnimationUtility.GetCurveBindings(clip).Length;
        }

        static IEnumerable<string> BuildControllerDetails(AnimatorController controller)
        {
            yield return "parameters:";
            foreach (var parameter in controller.parameters)
            {
                yield return "  " + parameter.name + " " + parameter.type;
            }

            yield return "layers:";
            foreach (var layer in controller.layers)
            {
                yield return "  " + layer.name;
                yield return "    default " + (layer.stateMachine.defaultState == null ? "null" : layer.stateMachine.defaultState.name);
                foreach (AnimatorStateTransition transition in layer.stateMachine.anyStateTransitions)
                {
                    yield return "    any-state -> " + (transition.destinationState == null ? "null" : transition.destinationState.name)
                        + " duration=" + transition.duration + " conditions=" + transition.conditions.Length;
                }
                foreach (var childState in layer.stateMachine.states)
                {
                    var state = childState.state;
                    yield return "    state " + state.name + " motion=" + (state.motion == null ? "null" : state.motion.name);
                    foreach (var transition in state.transitions)
                    {
                        yield return "      transition -> " + (transition.destinationState == null ? "null" : transition.destinationState.name)
                                     + " duration=" + transition.duration
                                     + " conditions=" + transition.conditions.Length;
                    }
                }
            }
        }

        static IEnumerable<string> BuildClipDetails(AnimationClip clip)
        {
            yield return "length: " + clip.length.ToString(CultureInfo.InvariantCulture);
            yield return "curves:";
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                yield return "  " + binding.path + " " + binding.type.Name + "." + binding.propertyName + " keys=" + (curve == null ? 0 : curve.length);
            }
        }

        static void ApplyParameterDefault(AnimatorController controller, string name, AnimatorControllerParameterType type, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            var parameters = controller.parameters;
            for (var index = 0; index < parameters.Length; index++)
            {
                if (parameters[index].name != name) continue;
                var parameter = parameters[index];
                switch (type)
                {
                    case AnimatorControllerParameterType.Float:
                        parameter.defaultFloat = ParseFloat(value, "--default");
                        break;
                    case AnimatorControllerParameterType.Int:
                        parameter.defaultInt = ParseInt(value, "--default");
                        break;
                    case AnimatorControllerParameterType.Bool:
                        parameter.defaultBool = ParseBool(value, "--default");
                        break;
                }

                parameters[index] = parameter;
                controller.parameters = parameters;
                return;
            }
        }

        static AnimatorControllerParameterType ParseParameterType(string type)
        {
            switch (Normalize(type))
            {
                case "float": return AnimatorControllerParameterType.Float;
                case "int":
                case "integer": return AnimatorControllerParameterType.Int;
                case "bool":
                case "boolean": return AnimatorControllerParameterType.Bool;
                case "trigger": return AnimatorControllerParameterType.Trigger;
                default: throw new InvalidOperationException("--type must be Float, Int, Bool, or Trigger.");
            }
        }

        static void AddCondition(AnimatorStateTransition transition, string text)
        {
            var parts = text.Split(new[] { ' ', ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) throw new InvalidOperationException("--condition requires Param Mode [threshold].");
            var parameter = parts[0];
            var mode = ParseConditionMode(parts[1]);
            var threshold = parts.Length >= 3 ? ParseFloat(parts[2], "--condition threshold") : 0f;
            transition.AddCondition(mode, threshold, parameter);
        }

        static AnimatorConditionMode ParseConditionMode(string mode)
        {
            switch (Normalize(mode))
            {
                case "if": return AnimatorConditionMode.If;
                case "ifnot":
                case "not": return AnimatorConditionMode.IfNot;
                case "greater":
                case ">": return AnimatorConditionMode.Greater;
                case "less":
                case "<": return AnimatorConditionMode.Less;
                case "equals":
                case "equal":
                case "==": return AnimatorConditionMode.Equals;
                case "notequal":
                case "!=": return AnimatorConditionMode.NotEqual;
                default: throw new InvalidOperationException("Unsupported condition mode: " + mode);
            }
        }

        static AnimationCurve ParseCurve(string keys)
        {
            const int TimeValuePartCount = 2;
            const int TangentPartCount = 4;
            if (string.IsNullOrWhiteSpace(keys)) throw new InvalidOperationException("--keys is required.");
            var frames = new List<Keyframe>();
            foreach (var part in keys.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var pair = part.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (pair.Length != TimeValuePartCount && pair.Length != TangentPartCount)
                {
                    throw new InvalidOperationException("Invalid keyframe: " + part);
                }

                var time = ParseFloat(pair[0], "key time");
                var value = ParseFloat(pair[1], "key value");
                frames.Add(pair.Length == TangentPartCount
                    ? new Keyframe(time, value, ParseFloat(pair[2], "key in slope"), ParseFloat(pair[3], "key out slope"))
                    : new Keyframe(time, value));
            }

            if (frames.Count == 0) throw new InvalidOperationException("--keys contains no keyframes.");
            return new AnimationCurve(frames.OrderBy(frame => frame.time).ToArray());
        }

        static void SetClipLoop(AnimationClip clip, bool loop)
        {
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }

        static AnimatorController LoadController(string path)
        {
            RequireControllerPath(path);
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null) throw new InvalidOperationException("AnimatorController not found: " + path);
            return controller;
        }

        static AnimationClip LoadClip(string path)
        {
            RequireClipPath(path);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) throw new InvalidOperationException("AnimationClip not found: " + path);
            return clip;
        }

        static AnimatorControllerLayer FindLayer(AnimatorController controller, string layerName)
        {
            var layer = controller.layers.FirstOrDefault(item => item.name == layerName);
            if (layer == null) throw new InvalidOperationException("Animator layer not found: " + layerName);
            return layer;
        }

        static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
        {
            foreach (var childState in stateMachine.states)
            {
                if (childState.state.name == stateName) return childState.state;
            }

            return null;
        }

        static Type ResolveComponentType(string component)
        {
            if (string.IsNullOrWhiteSpace(component)) throw new InvalidOperationException("--component is required.");
            var name = component.Trim();
            var direct = Type.GetType(name);
            if (direct != null) return direct;

            var unityEngine = Type.GetType("UnityEngine." + name + ", UnityEngine");
            if (unityEngine != null) return unityEngine;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var match = assembly.GetTypes().FirstOrDefault(type => type.Name == name || type.FullName == name);
                if (match != null) return match;
            }

            throw new InvalidOperationException("Component type not found: " + component);
        }

        static GameObject FindSceneObject(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name == path) return root;
                var found = root.transform.Find(path);
                if (found != null) return found.gameObject;
                if (path.StartsWith(root.name + "/", StringComparison.Ordinal))
                {
                    found = root.transform.Find(path.Substring(root.name.Length + 1));
                    if (found != null) return found.gameObject;
                }
            }

            return GameObject.Find(path);
        }

        static string RequireName(string value, string option)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(option + " is required.");
            return value.Trim();
        }

        static void RequireControllerPath(string path)
        {
            RequireAssetPath(path, "--controller");
            if (!path.EndsWith(".controller", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("--controller must end with .controller.");
            }
        }

        static void RequireClipPath(string path)
        {
            RequireAssetPath(path, "--clip");
            if (!path.EndsWith(".anim", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("--clip must end with .anim.");
            }
        }

        static void RequireAssetPath(string path, string option)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new InvalidOperationException(option + " is required.");
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) && !path.StartsWith("Packages/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(option + " must be under Assets/ or Packages/.");
            }
        }

        static void EnsureParentFolder(string assetPath)
        {
            var folder = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder)) return;
            var parts = folder.Split('/');
            var current = parts[0];
            for (var index = 1; index < parts.Length; index++)
            {
                var next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }

                current = next;
            }
        }

        static string NormalizeAssetPath(string path)
        {
            return string.IsNullOrWhiteSpace(path) ? string.Empty : path.Trim().Replace('\\', '/');
        }

        static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
        }

        static float ParseFloat(string value, string name)
        {
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            {
                throw new InvalidOperationException(name + " requires a number.");
            }

            return result;
        }

        static int ParseInt(string value, string name)
        {
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            {
                throw new InvalidOperationException(name + " requires an integer.");
            }

            return result;
        }

        static bool ParseBool(string value, string name)
        {
            if (bool.TryParse(value, out var result)) return result;
            if (value == "1") return true;
            if (value == "0") return false;
            throw new InvalidOperationException(name + " requires true or false.");
        }
    }
}
