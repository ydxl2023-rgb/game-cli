using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Compiles a frozen window and explicit delivery mappings into the existing prefab builder.</summary>
    internal static class UnityCliContractBuildService
    {
        internal static JObject ReadContract(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }
            JObject document = ReadDocument(path);
            string schema = (string)document["schemaVersion"];
            if (schema != "ui-production-contract/1" && schema != "ui-task-input/1")
            {
                return null;
            }
            UnityCliContractNativeReferences.Expand(document);
            return document;
        }

        internal static PrefabData Execute(UnityCliBridgeRequest request, JObject contract)
        {
            Stopwatch elapsed = Stopwatch.StartNew();
            JObject report = new JObject
            {
                ["schemaVersion"] = "ui-build-report/1",
                ["passed"] = false,
                ["dryRun"] = request.dryRun,
                ["stage"] = "compile",
                ["previews"] = new JArray()
            };
            UnityCliBuildTransaction transaction = null;
            try
            {
                PrefabData result = ExecuteCore(request, contract, report, ref transaction);
                report["passed"] = true;
                report["stage"] = request.dryRun ? "validated" : "completed";
                report["nodeCount"] = result.nodeCount;
                report["componentCount"] = result.componentCount;
                report["elapsedMs"] = elapsed.ElapsedMilliseconds;
                WriteReport(request, report);
                if (!request.dryRun && !string.IsNullOrWhiteSpace(request.screenshotPath))
                {
                    JObject manifest = ReadDocument(request.assetManifestPath);
                    UnityCliBuildCache.Save(result.prefabPath, UnityCliBuildCache.Key(request, manifest), report);
                }
                transaction?.Commit();
                if (!string.IsNullOrWhiteSpace(request.screenshotPath))
                {
                    result.buildReportPath = System.IO.Path.Combine(request.screenshotPath, "build-report.json");
                }
                return result;
            }
            catch (Exception exception)
            {
                if (transaction != null)
                {
                    try
                    {
                        transaction.Dispose();
                        report["rollback"] = "restored";
                    }
                    catch (Exception rollbackError)
                    {
                        report["rollback"] = "failed: " + rollbackError.Message;
                    }
                    transaction = null;
                }
                report["error"] = exception.Message;
                report["elapsedMs"] = elapsed.ElapsedMilliseconds;
                WriteReport(request, report);
                throw;
            }
            finally
            {
                transaction?.Dispose();
            }
        }

        private static PrefabData ExecuteCore(UnityCliBridgeRequest request, JObject contract, JObject report, ref UnityCliBuildTransaction transaction)
        {
            JObject window = SelectWindow(contract, request.prefabPath);
            string output = Required(window, "prefabPath");
            if (!string.IsNullOrEmpty(request.prefabPath) && request.prefabPath != output)
            {
                throw new InvalidOperationException("Requested output differs from contract prefabPath.");
            }
            if (string.IsNullOrEmpty(request.assetManifestPath))
            {
                throw new InvalidOperationException("Contract build requires --asset-manifest <ui-contract-resources/1.json>; no resource is inferred.");
            }
            JObject manifest = ReadDocument(request.assetManifestPath);
            if ((string)manifest["schemaVersion"] != "ui-contract-resources/1")
            {
                throw new InvalidOperationException("Expected ui-contract-resources/1 manifest.");
            }
            // Bind the resource mapping to exact file bytes, including amendments and formatting.
            if ((string)manifest["contractFileSha256"] != FileHash(request.jsonFile))
            {
                throw new InvalidOperationException("Manifest contractFileSha256 mismatch.");
            }
            report["cacheHit"] = false;
            if (!request.dryRun && request.replaceExistingChildSet && request.replaceExistingChild && !string.IsNullOrWhiteSpace(request.screenshotPath))
            {
                JObject cached = UnityCliBuildCache.Read(output, UnityCliBuildCache.Key(request, manifest));
                if (cached != null)
                {
                    report.Merge(cached, new Newtonsoft.Json.Linq.JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
                    report["cacheHit"] = true;
                    return new PrefabData
                    {
                        action = "contract-build",
                        prefabPath = output,
                        valid = true,
                        nodeCount = (int)cached["nodeCount"],
                        componentCount = (int)cached["componentCount"],
                        prefabModified = false,
                        message = "Reused verified unchanged prefab and state previews; no assets modified."
                    };
                }
            }
            JObject snapshot = Compile(window, manifest);
            if (snapshot["declarativeAnimations"] != null)
            {
                report["declarativeAnimations"] = snapshot["declarativeAnimations"].DeepClone();
                report["runtimePlayback"] = "External client lifecycle binding required; static build does not implement playback.";
            }
            ValidatePreviewStates(window, snapshot);
            report["prefabPath"] = output;
            report["contractFileSha256"] = (string)manifest["contractFileSha256"];
            UnityCliBridgeRequest build = new UnityCliBridgeRequest
            {
                action = "restore",
                prefabPath = output,
                jsonFile = request.jsonFile,
                dryRun = true,
                replaceExistingChild = request.replaceExistingChild,
                replaceExistingChildSet = request.replaceExistingChildSet
            };
            // Both modes use the same compiler and isolated Unity validation before any write.
            report["stage"] = "preflight";
            PrefabData result = UnityCliPrefabRestoreService.Execute(build, snapshot);
            if (!request.dryRun)
            {
                transaction = new UnityCliBuildTransaction(output, snapshot, window, request.screenshotPath, manifest);
                build.dryRun = false;
                report["stage"] = "build";
                result = UnityCliPrefabRestoreService.Execute(build, snapshot);
                GameObject saved = AssetDatabase.LoadAssetAtPath<GameObject>(output);
                if (saved == null || saved.GetComponentsInChildren<Transform>(true).Length != result.nodeCount)
                {
                    throw new InvalidOperationException("Saved prefab hierarchy count differs from the compiled contract.");
                }
                report["stage"] = "readback";
                VerifySaved(saved, snapshot);
                report["prefabSha256"] = FileHash(output);
                if (!string.IsNullOrWhiteSpace(request.screenshotPath))
                {
                    report["stage"] = "preview";
                    report["previews"] = RenderStates(saved, snapshot, window, request.screenshotPath);
                }
            }
            if (!request.dryRun)
            {
                JArray resourceReadback = new JArray();
                foreach (JObject resource in ((JArray)manifest["resources"]).Concat(manifest["assets"] as JArray ?? new JArray()))
                {
                    string resourcePath = Required(resource, "path");
                    if (resourcePath.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
                    {
                        Material material = AssetDatabase.LoadAssetAtPath<Material>(resourcePath);
                        if (material != null)
                        {
                            // Rendering can trigger URP material validation; persist and verify before granting a receipt.
                            AssetDatabase.SaveAssetIfDirty(material);
                        }
                    }
                    VerifyAsset(resource);
                    resourceReadback.Add(new JObject { ["path"] = resourcePath, ["sha256"] = FileHash(resourcePath) });
                }
                report["resourceReadback"] = resourceReadback;
            }
            result.action = "contract-build";
            result.message = request.dryRun
                ? "Contract and resolved resources validated in an isolated scene; no assets modified."
                : "Contract assembled; saved hierarchy, transforms and declared component properties read back. Visual and business acceptance remain separate.";
            return result;
        }

        private static void WriteReport(UnityCliBridgeRequest request, JObject report)
        {
            if (string.IsNullOrWhiteSpace(request.screenshotPath))
            {
                return;
            }
            Directory.CreateDirectory(request.screenshotPath);
            File.WriteAllText(System.IO.Path.Combine(request.screenshotPath, "build-report.json"), report.ToString());
        }

        private static void ValidatePreviewStates(JObject window, JObject snapshot)
        {
            int width = (int?)window["referenceResolution"]?["width"] ?? 1920;
            int height = (int?)window["referenceResolution"]?["height"] ?? 1080;
            if (width < 64 || height < 64 || width > 4096 || height > 4096)
            {
                throw new InvalidOperationException("Preview resolution must be between 64 and 4096 pixels per axis.");
            }
            HashSet<string> ids = new HashSet<string>(((JArray)snapshot["prefab"]["nodes"]).Values<JObject>().Select(n => (string)n["id"]));
            HashSet<string> states = new HashSet<string>();
            foreach (JObject state in window["previewStates"] as JArray ?? new JArray())
            {
                string id = Required(state, "id");
                if (state["animationTime"] != null)
                {
                    float time = (float)state["animationTime"];
                    if (float.IsNaN(time) || float.IsInfinity(time) || time < 0 || time > 300)
                    {
                        throw new InvalidOperationException("Preview animationTime must be between 0 and 300 seconds.");
                    }
                }
                if (state["particleFramingVersion"] != null && (state["particleFramingVersion"].Type != JTokenType.Integer || (int)state["particleFramingVersion"] < 1 || (int)state["particleFramingVersion"] > 4))
                {
                    throw new InvalidOperationException("Particle framing version must be 1, 2, 3 or 4.");
                }
                if (state["frameParticles"] != null && state["frameParticles"].Type != JTokenType.Boolean)
                {
                    throw new InvalidOperationException("Preview frameParticles must be boolean.");
                }
                if (state["sampleDisabledAnimators"] != null && state["sampleDisabledAnimators"].Type != JTokenType.Boolean)
                {
                    throw new InvalidOperationException("Disabled Animator sampling must be boolean.");
                }
                if ((bool?)state["sampleDisabledAnimators"] == true && (state["animationTime"] == null || (string)state["fixtureKind"] != "native-motion-sample"))
                {
                    throw new InvalidOperationException("Disabled Animator sampling requires a labeled native motion diagnostic time.");
                }
                if (state["sampleDisabledParticleRenderers"] != null && state["sampleDisabledParticleRenderers"].Type != JTokenType.Boolean)
                {
                    throw new InvalidOperationException("Disabled particle renderer sampling must be boolean.");
                }
                if ((bool?)state["sampleDisabledParticleRenderers"] == true && (state["particleTime"] == null || (bool?)state["frameParticles"] != true
                    || (bool?)state["hideUiGraphics"] != true || ((string)state["fixtureKind"] != "diagnostic-particle-isolation" && (string)state["fixtureKind"] != "diagnostic-particle-isolation-emission")))
                {
                    throw new InvalidOperationException("Disabled particle renderer sampling requires a labeled isolated particle diagnostic.");
                }
                if (state["hideUiGraphics"] != null && state["hideUiGraphics"].Type != JTokenType.Boolean)
                {
                    throw new InvalidOperationException("Preview hideUiGraphics must be boolean.");
                }
                if ((bool?)state["hideUiGraphics"] == true && (state["particleTime"] == null || (bool?)state["frameParticles"] != true
                    || ((string)state["fixtureKind"] != "diagnostic-particle-isolation" && (string)state["fixtureKind"] != "diagnostic-particle-isolation-emission")))
                {
                    throw new InvalidOperationException("Graphic suppression requires a labeled isolated particle diagnostic sample.");
                }
                if (state["particleEmitCount"] != null && (state["particleEmitCount"].Type != JTokenType.Integer || (int)state["particleEmitCount"] < 0 || (int)state["particleEmitCount"] > 100 || state["particleTime"] == null))
                {
                    throw new InvalidOperationException("Explicit diagnostic particle emission requires a sample time and integer count between 0 and 100.");
                }
                if (state["previewBackground"] != null && ((string)state["previewBackground"] != "light" && (string)state["previewBackground"] != "dark"))
                {
                    throw new InvalidOperationException("Preview background must be light or dark.");
                }
                if (state["frameContent"] != null && state["frameContent"].Type != JTokenType.Boolean)
                {
                    throw new InvalidOperationException("Preview frameContent must be boolean.");
                }
                if ((bool?)state["frameContent"] == true && (bool?)state["frameParticles"] != true)
                {
                    throw new InvalidOperationException("Content framing requires particle framing.");
                }
                if ((bool?)state["frameParticles"] == true && state["particleTime"] == null)
                {
                    throw new InvalidOperationException("Particle framing requires an explicit sample time.");
                }
                if (state["particleTime"] != null)
                {
                    JToken token = state["particleTime"];
                    float time = token.Type == JTokenType.Float || token.Type == JTokenType.Integer ? (float)token : float.NaN;
                    if (float.IsNaN(time) || float.IsInfinity(time) || time < 0 || time > 30)
                    {
                        throw new InvalidOperationException("Preview particleTime must be numeric and within 0..30 seconds.");
                    }
                }
                if (id.Any(c => !(char.IsLetterOrDigit(c) || c == '-' || c == '_')) || !states.Add(id))
                {
                    throw new InvalidOperationException("Invalid or duplicate preview state: " + id);
                }
                foreach (JProperty active in ((JObject)state["activeNodes"]).Properties())
                {
                    if (!ids.Contains(active.Name) || active.Value.Type != JTokenType.Boolean)
                    {
                        throw new InvalidOperationException("Unknown preview node or invalid state value: " + active.Name);
                    }
                }
                if (state["scaleFactors"] != null && !(state["scaleFactors"] is JObject))
                {
                    throw new InvalidOperationException("Preview scaleFactors must be an object.");
                }
                foreach (JProperty factor in ((JObject)state["scaleFactors"] ?? new JObject()).Properties())
                {
                    if (!ids.Contains(factor.Name) || (factor.Value.Type != JTokenType.Float && factor.Value.Type != JTokenType.Integer))
                    {
                        throw new InvalidOperationException("Unknown scale feedback target or nonnumeric factor.");
                    }
                    float value = (float)factor.Value;
                    if (float.IsNaN(value) || float.IsInfinity(value) || value < 0.05f || value > 4f)
                    {
                        throw new InvalidOperationException("Preview scale feedback factor must be in 0.05..4.");
                    }
                }
            }
        }

        private static JArray RenderStates(GameObject saved, JObject snapshot, JObject window, string directory)
        {
            Directory.CreateDirectory(directory);
            JArray results = new JArray();
            JArray states = window["previewStates"] as JArray ?? new JArray();
            foreach (JObject state in states)
            {
                Scene scene = EditorSceneManager.NewPreviewScene();
                try
                {
                    // Render the saved prefab, never a second model-authored snapshot.
                    GameObject instance = UnityEngine.Object.Instantiate(saved);
                    SceneManager.MoveGameObjectToScene(instance, scene);
                    // Nested overlay canvases would resize their RectTransforms before
                    // the isolated camera host is attached. Keep source geometry intact.
                    foreach (Canvas canvas in instance.GetComponentsInChildren<Canvas>(true))
                    {
                        canvas.renderMode = RenderMode.WorldSpace;
                    }
                    Dictionary<Animator, bool> sourceAnimatorEnabled = new Dictionary<Animator, bool>();
                    foreach (Animator animator in instance.GetComponentsInChildren<Animator>(true))
                    {
                        // Capture source state before suppressing automatic playback in the isolated preview.
                        sourceAnimatorEnabled.Add(animator, animator.enabled);
                        animator.enabled = false;
                    }
                    Dictionary<string, GameObject> nodes = ResolveSavedNodes(instance, (JArray)snapshot["prefab"]["nodes"], (string)snapshot["prefab"]["rootNodeId"]);
                    Dictionary<string, bool> sourceParticleRendererEnabled = new Dictionary<string, bool>();
                    foreach (KeyValuePair<string, GameObject> pair in nodes)
                    {
                        ParticleSystemRenderer renderer = pair.Value.GetComponent<ParticleSystemRenderer>();
                        if (renderer != null)
                        {
                            // Diagnostic visibility changes only this disposable preview instance.
                            sourceParticleRendererEnabled.Add(pair.Key, renderer.enabled);
                            if ((bool?)state["sampleDisabledParticleRenderers"] == true)
                            {
                                renderer.enabled = true;
                            }
                        }
                    }
                    foreach (JProperty active in ((JObject)state["activeNodes"]).Properties())
                    {
                        nodes[active.Name].SetActive((bool)active.Value);
                    }
                    JArray animationSamples = new JArray();
                    if (state["animationTime"] != null)
                    {
                        float time = (float)state["animationTime"];
                        // Sample only isolated instances. Never advance or modify saved assets.
                        foreach (Animator animator in instance.GetComponentsInChildren<Animator>())
                        {
                            // Disabled source owners stay disabled; only labeled diagnostics sample their clip.
                            bool sourceEnabled = sourceAnimatorEnabled[animator];
                            string sourceNodeId = nodes.Single(pair => pair.Value == animator.gameObject).Key;
                            if (!sourceEnabled && (bool?)state["sampleDisabledAnimators"] != true)
                            {
                                animationSamples.Add(new JObject { ["node"] = animator.name, ["nodeId"] = sourceNodeId, ["sourceEnabled"] = false, ["clip"] = null, ["time"] = 0, ["reason"] = "Source Animator is disabled." });
                                continue;
                            }
                            AnimationClip[] clips = animator.runtimeAnimatorController?.animationClips;
                            UnityEditor.Animations.AnimatorController controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                            if (controller != null && controller.layers.Length == 1 && controller.layers[0].stateMachine.defaultState != null
                                && controller.layers[0].stateMachine.defaultState.motion == null)
                            {
                                animationSamples.Add(new JObject { ["node"] = animator.name, ["nodeId"] = sourceNodeId, ["sourceEnabled"] = sourceEnabled, ["clip"] = null, ["time"] = 0, ["reason"] = "Default state has no motion." });
                                continue;
                            }
                            AnimationClip clip = clips != null && clips.Length == 1 ? clips[0]
                                : controller != null && controller.layers.Length == 1 ? controller.layers[0].stateMachine.defaultState?.motion as AnimationClip : null;
                            if (clip == null)
                            {
                                throw new InvalidOperationException("Timed preview requires one clip or a single-layer controller with an explicit default clip.");
                            }
                            float sampleTime = clip.isLooping && clip.length > 0 ? time % clip.length : Mathf.Min(time, clip.length);
                            clip.SampleAnimation(animator.gameObject, sampleTime);
                            animationSamples.Add(new JObject { ["node"] = animator.name, ["nodeId"] = sourceNodeId, ["sourceEnabled"] = sourceEnabled, ["sampledDisabledSource"] = !sourceEnabled, ["clip"] = clip.name, ["time"] = sampleTime });
                        }
                    }
                    foreach (JProperty factor in ((JObject)state["scaleFactors"] ?? new JObject()).Properties())
                    {
                        // Feedback poses affect only this isolated instance, after native clip sampling.
                        Transform target = nodes[factor.Name].transform;
                        target.localScale *= (float)factor.Value;
                    }
                    string path = System.IO.Path.Combine(directory, Required(state, "id") + ".png");
                    JArray particleSamples = new JArray();
                    UnityCliSnapshotVisuals.Render(instance, scene, new JObject
                    {
                        ["width"] = (int?)window["referenceResolution"]?["width"] ?? 1920,
                        ["height"] = (int?)window["referenceResolution"]?["height"] ?? 1080,
                        ["output"] = path,
                        ["frameParticles"] = (bool?)state["frameParticles"] ?? false,
                        ["hideUiGraphics"] = (bool?)state["hideUiGraphics"] ?? false,
                        ["previewBackground"] = (string)state["previewBackground"] ?? "dark",
                        ["frameContent"] = (bool?)state["frameContent"] ?? false,
                        ["particleFramingVersion"] = (int?)state["particleFramingVersion"] ?? 1
                    }, () =>
                    {
                        if (state["particleTime"] != null)
                        {
                            float time = (float)state["particleTime"];
                            bool hasSubEmitters = nodes.Values.Select(node => node.GetComponent<ParticleSystem>()).Any(system =>
                                system != null && system.subEmitters.enabled && system.subEmitters.subEmittersCount > 0);
                            if (hasSubEmitters)
                            {
                                SampleSubEmitterHierarchy(nodes, time, (int?)state["particleEmitCount"] ?? 0, particleSamples);
                            }
                            else
                            {
                                foreach (KeyValuePair<string, GameObject> pair in nodes)
                                {
                                    ParticleSystem particles = pair.Value.GetComponent<ParticleSystem>();
                                    if (particles == null)
                                    {
                                        continue;
                                    }
                                    // Sampling never invokes source callbacks or changes the saved prefab.
                                    ParticleSystem.MainModule main = particles.main;
                                    if (main.stopAction != ParticleSystemStopAction.None || main.maxParticles > 10000)
                                    {
                                        throw new InvalidOperationException("Isolated particle preview requires bounded capacity and no stop action.");
                                    }
                                    particles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                                    particles.useAutoRandomSeed = false;
                                    particles.randomSeed = 1;
                                    int emitted = (int?)state["particleEmitCount"] ?? 0;
                                    if (emitted > 0)
                                    {
                                        particles.Emit(Mathf.Min(emitted, main.maxParticles));
                                    }
                                    particles.Simulate(time, false, emitted == 0, true);
                                    particleSamples.Add(new JObject
                                    {
                                        ["nodeId"] = pair.Key,
                                        ["name"] = pair.Value.name,
                                        ["time"] = time,
                                        ["seed"] = 1,
                                        ["activeInHierarchy"] = pair.Value.activeInHierarchy,
                                        ["particleCount"] = particles.particleCount,
                                        ["diagnosticEmitCount"] = emitted
                                    });
                                }
                            }
                        }
                    });
                    foreach (JObject sample in particleSamples.OfType<JObject>())
                    {
                        string nodeId = (string)sample["nodeId"];
                        if (sourceParticleRendererEnabled.TryGetValue(nodeId, out bool sourceEnabled))
                        {
                            sample["sourceRendererEnabled"] = sourceEnabled;
                            sample["sampledDisabledRenderer"] = !sourceEnabled && (bool?)state["sampleDisabledParticleRenderers"] == true;
                        }
                    }
                    JArray sampledLayout = new JArray();
                    if (state["animationTime"] != null)
                    {
                        foreach (KeyValuePair<string, GameObject> pair in nodes)
                        {
                            if (pair.Value.transform is RectTransform rect)
                            {
                                CanvasGroup group = pair.Value.GetComponent<CanvasGroup>();
                                sampledLayout.Add(new JObject
                                {
                                    ["nodeId"] = pair.Key,
                                    ["name"] = pair.Value.name,
                                    ["position"] = new JArray(rect.anchoredPosition.x, rect.anchoredPosition.y),
                                    ["scale"] = new JArray(rect.localScale.x, rect.localScale.y, rect.localScale.z),
                                    ["alpha"] = group != null ? (float?)group.alpha : null
                                });
                            }
                        }
                    }
                    JObject previewResult = new JObject { ["state"] = (string)state["id"], ["path"] = path, ["sha256"] = FileHash(path), ["metricsHash"] = UnityCliBuildCache.Hash(path + ".text-metrics.json"), ["uiMetricsPath"] = path + ".ui-metrics.json", ["uiMetricsHash"] = FileHash(path + ".ui-metrics.json"), ["animationSamples"] = animationSamples, ["particleSamples"] = particleSamples, ["sampledLayout"] = sampledLayout };
                    previewResult["uiGraphicsSuppressed"] = (bool?)state["hideUiGraphics"] ?? false;
                    if (File.Exists(path + ".particle-framing.json"))
                    {
                        previewResult["particleFramingMetricsPath"] = path + ".particle-framing.json";
                        previewResult["particleFramingMetricsHash"] = FileHash(path + ".particle-framing.json");
                    }
                    results.Add(previewResult);
                }
                finally
                {
                    EditorSceneManager.ClosePreviewScene(scene);
                }
            }
            return results;
        }

        private static JObject SelectWindow(JObject contract, string output)
        {
            if ((string)contract["schemaVersion"] == "ui-task-input/1")
            {
                if ((string)contract["type"] != "prefab-assembly")
                {
                    throw new InvalidOperationException("Contract build requires prefab-assembly task input.");
                }
                return contract["window"] as JObject ?? throw new InvalidOperationException("Missing window.");
            }
            JArray windows = contract["windows"] as JArray;
            JObject[] matches = windows?.Cast<JObject>().Where(w => string.IsNullOrEmpty(output) || (string)w["prefabPath"] == output).ToArray();
            if (matches == null || matches.Length != 1)
            {
                throw new InvalidOperationException("Select exactly one contract window using --prefab.");
            }
            return matches[0];
        }

        private static void SampleSubEmitterHierarchy(Dictionary<string, GameObject> nodes, float time, int emitted, JArray samples)
        {
            KeyValuePair<string, GameObject>[] owners = nodes.Where(pair => pair.Value.GetComponent<ParticleSystem>() != null).ToArray();
            HashSet<ParticleSystem> systems = new HashSet<ParticleSystem>(owners.Select(pair => pair.Value.GetComponent<ParticleSystem>()));
            foreach (ParticleSystem system in systems)
            {
                ParticleSystem.MainModule main = system.main;
                if (main.stopAction != ParticleSystemStopAction.None || main.maxParticles > 10000)
                {
                    throw new InvalidOperationException("Isolated particle preview requires bounded capacity and no stop action.");
                }
                ParticleSystem.SubEmittersModule subEmitters = system.subEmitters;
                for (int index = 0; subEmitters.enabled && index < subEmitters.subEmittersCount; index++)
                {
                    ParticleSystem target = subEmitters.GetSubEmitterSystem(index);
                    // Serialized empty slots are valid no-ops and retain their original array indexes.
                    // Non-null references still require an exact captured descendant before sampling.
                    if (target == null)
                    {
                        continue;
                    }
                    if (!systems.Contains(target) || !target.transform.IsChildOf(system.transform))
                    {
                        throw new InvalidOperationException("Sub-emitter sampling requires captured descendant particle systems.");
                    }
                }
            }
            // Configure every target before any parent can emit into it. Per-owner reset after a parent
            // simulation would erase child emissions and expose the child's uninitialized random seed.
            foreach (ParticleSystem system in systems)
            {
                system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                system.useAutoRandomSeed = false;
                system.randomSeed = 1;
            }
            if (emitted > 0)
            {
                foreach (ParticleSystem system in systems)
                {
                    system.Emit(Mathf.Min(emitted, system.main.maxParticles));
                }
            }
            foreach (KeyValuePair<string, GameObject> owner in owners)
            {
                ParticleSystem system = owner.Value.GetComponent<ParticleSystem>();
                bool hasAncestor = system.GetComponentsInParent<ParticleSystem>(true).Any(parent => parent != system && systems.Contains(parent));
                if (!hasAncestor)
                {
                    // Unity advances the complete hierarchy once, including its native sub-emitter dependencies.
                    system.Simulate(time, true, emitted == 0, true);
                }
            }
            foreach (KeyValuePair<string, GameObject> owner in owners)
            {
                ParticleSystem system = owner.Value.GetComponent<ParticleSystem>();
                samples.Add(new JObject
                {
                    ["nodeId"] = owner.Key,
                    ["name"] = owner.Value.name,
                    ["time"] = time,
                    ["seed"] = 1,
                    ["activeInHierarchy"] = owner.Value.activeInHierarchy,
                    ["particleCount"] = system.particleCount,
                    ["diagnosticEmitCount"] = emitted,
                    ["algorithm"] = "native-sub-emitter-hierarchy-fixed-seed-v1"
                });
            }
        }

        private static JObject Compile(JObject window, JObject manifest)
        {
            JArray nodes = (JArray)(window["nodes"] ?? throw new InvalidOperationException("Missing nodes.")).DeepClone();
            Dictionary<string, JObject> byId = nodes.Cast<JObject>().ToDictionary(n => Required(n, "id"));
            JObject root = byId[Required(window, "rootNodeId")];
            if ((string)root["name"] != Path.GetFileNameWithoutExtension(Required(window, "prefabPath")))
            {
                throw new InvalidOperationException("Root name must match prefab filename.");
            }
            JObject rootRect = root["rect"] as JObject;
            if (rootRect == null || !JToken.DeepEquals(rootRect["anchorMin"], new JArray(0, 0))
                || !JToken.DeepEquals(rootRect["anchorMax"], new JArray(1, 1)))
            {
                // Compare numerically because JSON distinguishes integer and floating point tokens.
                if (rootRect == null || !VectorMatches(rootRect["anchorMin"], Vector2.zero)
                    || !VectorMatches(rootRect["anchorMax"], Vector2.one))
                {
                    throw new InvalidOperationException("Contract root must use full-screen stretch anchors.");
                }
            }
            if (!VectorMatches(rootRect["pivot"], new Vector2(0.5f, 0.5f))
                || !VectorMatches(rootRect["sizeDelta"], Vector2.zero)
                || !VectorMatches(rootRect["anchoredPosition"], Vector2.zero))
            {
                throw new InvalidOperationException("Contract root requires centered pivot and zero sizeDelta/anchoredPosition.");
            }
            foreach (JObject node in nodes)
            {
                node["kind"] = "Node";
            }
            JArray resources = manifest["resources"] as JArray ?? throw new InvalidOperationException("Missing resources array.");
            Dictionary<string, JObject> mappings = resources.Cast<JObject>().ToDictionary(r => Required(r, "slotId"));
            JArray textures = new JArray();
            HashSet<string> bindings = new HashSet<string>();
            JArray slots = window["resourceSlots"] as JArray ?? throw new InvalidOperationException("Missing resourceSlots.");
            if (slots.Count != mappings.Count)
            {
                throw new InvalidOperationException("Resource mapping count must match contract slots exactly.");
            }
            foreach (JObject slot in slots)
            {
                string id = Required(slot, "slotId");
                if ((string)slot["kind"] != "font" && (string)slot["kind"] != "sprite" && (string)slot["kind"] != "material" && (string)slot["kind"] != "mesh" && (string)slot["kind"] != "particle-shape-texture")
                {
                    throw new InvalidOperationException("Unsupported resource kind: " + (string)slot["kind"]);
                }
                if (!mappings.TryGetValue(id, out JObject mapping))
                {
                    throw new InvalidOperationException("Missing resource slot: " + id);
                }
                string path = VerifyAsset(mapping);
                if (slot["visibleBounds"] is JObject visibleBounds)
                {
                    ValidateVisibleBounds(path, visibleBounds);
                }
                if (slot["sourceReference"] != null && (string)slot["sourceReference"]["sha256"] != (string)mapping["sha256"])
                {
                    throw new InvalidOperationException("Reuse resource hash differs: " + id);
                }
                foreach (JObject binding in (JArray)slot["bindings"])
                {
                    Bind(byId, bindings, binding, path);
                }
                if ((string)slot["kind"] == "sprite")
                {
                    if (slot["wrapMode"] != null && (slot["wrapMode"].Type != JTokenType.Integer || (int)slot["wrapMode"] < 0 || (int)slot["wrapMode"] > 1))
                    {
                        throw new InvalidOperationException("Sprite wrapMode must be integer 0 (Repeat) or 1 (Clamp).");
                    }
                    JObject size = slot["deliverySize"] as JObject ?? throw new InvalidOperationException("Sprite deliverySize required: " + id);
                    JObject border = slot["border"] as JObject;
                    textures.Add(new JObject
                    {
                        ["path"] = path,
                        ["deliverySha256"] = mapping["sha256"].DeepClone(),
                        ["deliveryWidth"] = size["width"].DeepClone(),
                        ["deliveryHeight"] = size["height"].DeepClone(),
                        ["settings"] = new JObject
                        {
                            ["spriteMode"] = 1,
                            ["wrapMode"] = (int?)slot["wrapMode"] ?? 1,
                            ["spritePixelsToUnits"] = (float?)slot["pixelsPerUnit"] ?? 100,
                            ["spriteBorder"] = new JObject
                            {
                                ["x"] = (int?)border?["left"] ?? 0,
                                ["y"] = (int?)border?["bottom"] ?? 0,
                                ["z"] = (int?)border?["right"] ?? 0,
                                ["w"] = (int?)border?["top"] ?? 0
                            }
                        }
                    });
                }
            }
            Dictionary<string, JObject> assets = (manifest["assets"] as JArray ?? new JArray()).Cast<JObject>().ToDictionary(a => Required(a, "path"));
            foreach (JObject asset in assets.Values)
            {
                VerifyAsset(asset);
            }
            foreach (JObject slot in slots.Cast<JObject>().Where(item => (string)item["kind"] == "material"))
            {
                string materialPath = (string)mappings[Required(slot, "slotId")]["path"];
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null || material.shader == null || material.shader.name != "Universal Render Pipeline/Particles/Unlit")
                {
                    throw new InvalidOperationException("Particle material must use the verified URP unlit particle shader.");
                }
                foreach (string dependency in AssetDatabase.GetDependencies(materialPath, true))
                {
                    if (dependency != materialPath && dependency.StartsWith("Assets/", StringComparison.Ordinal) && !assets.ContainsKey(dependency))
                    {
                        throw new InvalidOperationException("Unverified particle material dependency: " + dependency);
                    }
                }
                foreach (JObject binding in (JArray)slot["bindings"])
                {
                    JObject component = ((JArray)byId[Required(binding, "nodeId")]["components"]).Cast<JObject>()
                        .Single(item => (string)item["id"] == Required(binding, "componentId"));
                    if ((string)component["type"] != "UnityEngine.ParticleSystemRenderer"
                        || !System.Text.RegularExpressions.Regex.IsMatch(Required(binding, "property"), @"^m_Materials\.Array\.data\[\d+\]$"))
                    {
                        throw new InvalidOperationException("Particle material slots must bind explicit native renderer array entries.");
                    }
                }
            }
            foreach (JObject slot in slots.Cast<JObject>().Where(item => (string)item["kind"] == "mesh"))
            {
                foreach (JObject binding in (JArray)slot["bindings"])
                {
                    JObject component = ((JArray)byId[Required(binding, "nodeId")]["components"]).Cast<JObject>()
                        .Single(item => (string)item["id"] == Required(binding, "componentId"));
                    bool rendererMesh = (string)component["type"] == "UnityEngine.ParticleSystemRenderer"
                        && System.Text.RegularExpressions.Regex.IsMatch(Required(binding, "property"), @"^m_Mesh[1-3]?$");
                    bool emissionMesh = (string)component["type"] == "UnityEngine.ParticleSystem"
                        && Required(binding, "property") == "ShapeModule.m_Mesh";
                    if ((!rendererMesh && !emissionMesh)
                        || !long.TryParse((string)binding["localFileId"], out long localId) || localId == 0)
                    {
                        throw new InvalidOperationException("Mesh slots require explicit native renderer or emission-shape mesh properties and local IDs.");
                    }
                }
            }
            foreach (JObject slot in slots.Cast<JObject>().Where(item => (string)item["kind"] == "particle-shape-texture"))
            {
                string texturePath = (string)mappings[Required(slot, "slotId")]["path"];
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture == null || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(texture, out string textureGuid, out long textureId))
                {
                    throw new InvalidOperationException("Shape masks require an imported native Texture2D.");
                }
                foreach (JObject binding in (JArray)slot["bindings"])
                {
                    JObject component = ((JArray)byId[Required(binding, "nodeId")]["components"]).Cast<JObject>().Single(item => (string)item["id"] == Required(binding, "componentId"));
                    if ((string)component["type"] != "UnityEngine.ParticleSystem" || Required(binding, "property") != "ShapeModule.m_Texture"
                        || (string)binding["localFileId"] != textureId.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    {
                        throw new InvalidOperationException("Shape masks require exact ParticleSystem texture bindings and native local IDs.");
                    }
                }
            }
            JArray animations = window["animations"] as JArray ?? new JArray();
            JArray declarative = new JArray();
            foreach (JObject animation in animations)
            {
                JObject spec = animation["specification"] as JObject;
                string controller = (string)spec?["controllerPath"];
                if (controller == null && spec?["tracks"] is JArray tracks)
                {
                    JObject[] matches = assets.Values.Where(a => (string)a["animationId"] == (string)animation["id"]).ToArray();
                    if (matches.Length != 1)
                    {
                        throw new InvalidOperationException("Declarative animation requires one verified asset with animationId: " + (string)animation["id"]);
                    }
                    string clipPath = (string)matches[0]["path"];
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                    if (clip == null || Math.Abs(clip.frameRate - (float)spec["sampleRate"]) > 0.001f
                        || Math.Abs(clip.length - (float)spec["duration"]) > 0.001f
                        || AnimationUtility.GetAnimationClipSettings(clip).loopTime != (bool)spec["loop"])
                    {
                        throw new InvalidOperationException("Declarative clip duration/rate/loop mismatch: " + clipPath);
                    }
                    EditorCurveBinding[] curves = AnimationUtility.GetCurveBindings(clip);
                    if (curves.Length != tracks.Count || AnimationUtility.GetAnimationEvents(clip).Length != 0
                        || AnimationUtility.GetObjectReferenceCurveBindings(clip).Length != 0)
                    {
                        throw new InvalidOperationException("Unexpected animation curves, events or object references: " + clipPath);
                    }
                    HashSet<string> seenTracks = new HashSet<string>();
                    foreach (JObject track in tracks)
                    {
                        string id = Required(track, "targetNodeId");
                        string property = (string)(track["targetProperty"] ?? track["property"]);
                        if (!byId.ContainsKey(id) || !((JArray)animation["targetNodeIds"]).Values<string>().Contains(id))
                        {
                            throw new InvalidOperationException("Unknown declarative track target: " + id);
                        }
                        string component = (string)(track["targetComponentId"] ?? track["componentId"]);
                        string type = component == null ? "UnityEngine.RectTransform"
                            : (string)((JArray)byId[id]["components"]).Cast<JObject>().Single(c => (string)c["id"] == component)["type"];
                        List<string> pathParts = new List<string>();
                        JObject node = byId[id];
                        HashSet<string> ancestors = new HashSet<string>();
                        while ((string)node["parentId"] != null)
                        {
                            if (!ancestors.Add((string)node["id"]))
                            {
                                throw new InvalidOperationException("Cyclic animation target path.");
                            }
                            pathParts.Insert(0, (string)node["name"]);
                            node = byId[(string)node["parentId"]];
                        }
                        string targetPath = string.Join("/", pathParts);
                        string key = targetPath + ":" + type + ":" + property;
                        if (!seenTracks.Add(key) || !curves.Any(c => c.path == targetPath && c.type.FullName == type && c.propertyName == property))
                        {
                            throw new InvalidOperationException("Missing or duplicate declarative curve: " + key);
                        }
                    }
                    declarative.Add(new JObject { ["id"] = (string)animation["id"], ["clipPath"] = clipPath, ["trackCount"] = tracks.Count });
                    continue;
                }
                if (controller == null || !assets.ContainsKey(controller) || !(spec["bindings"] is JArray targets))
                {
                    throw new InvalidOperationException("Animation requires a verified controller and explicit bindings: " + (string)animation["id"]);
                }
                // Art creates animations; assembly only consumes existing verified controllers and clips.
                foreach (string dependency in AssetDatabase.GetDependencies(controller, true))
                {
                    if ((dependency.EndsWith(".anim", StringComparison.OrdinalIgnoreCase) || dependency.EndsWith(".controller", StringComparison.OrdinalIgnoreCase)) && !assets.ContainsKey(dependency))
                    {
                        throw new InvalidOperationException("Unverified animation dependency: " + dependency);
                    }
                }
                foreach (JObject binding in targets)
                {
                    Bind(byId, bindings, binding, controller);
                }
            }
            foreach (JObject effect in window["effects"] as JArray ?? new JArray())
            {
                string animationId = (string)effect["specification"]?["animationId"];
                if (animationId == null || !animations.Cast<JObject>().Any(a => (string)a["id"] == animationId))
                {
                    throw new InvalidOperationException("Unsupported effect specification: " + (string)effect["id"]);
                }
            }
            return new JObject
            {
                ["schemaVersion"] = "ui-template/1",
                ["declarativeAnimations"] = declarative.Count > 0 ? declarative : null,
                ["visualTextures"] = textures,
                ["prefab"] = new JObject { ["rootNodeId"] = window["rootNodeId"].DeepClone(), ["nodes"] = nodes }
            };
        }

        private static void Bind(Dictionary<string, JObject> nodes, HashSet<string> seen, JObject binding, string path)
        {
            string node = Required(binding, "nodeId");
            string component = Required(binding, "componentId");
            string property = Required(binding, "property");
            string key = node + ":" + component + ":" + property;
            if (!seen.Add(key) || !nodes.ContainsKey(node))
            {
                throw new InvalidOperationException("Duplicate or unknown binding: " + key);
            }
            JObject target = ((JArray)nodes[node]["components"]).Cast<JObject>().Single(c => (string)c["id"] == component);
            JObject properties = (JObject)target["properties"];
            if (properties[property]?.Type != JTokenType.Null)
            {
                throw new InvalidOperationException("Binding must resolve an explicit null: " + key);
            }
            properties[property] = new JObject { ["assetPath"] = path };
            if (binding["localFileId"] != null)
            {
                properties[property]["localFileId"] = binding["localFileId"].DeepClone();
            }
        }

        private static bool VectorMatches(JToken value, Vector2 actual)
        {
            return value is JArray array && array.Count == 2
                && Mathf.Abs((float)array[0] - actual.x) < 0.0001f
                && Mathf.Abs((float)array[1] - actual.y) < 0.0001f;
        }

        private static Dictionary<string, GameObject> ResolveSavedNodes(GameObject saved, JArray nodes, string rootId)
        {
            Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();
            objects.Add(rootId, saved);
            List<JObject> remaining = nodes.Cast<JObject>().Where(n => n["parentId"].Type != JTokenType.Null).ToList();
            while (remaining.Count > 0)
            {
                int prior = remaining.Count;
                foreach (JObject node in remaining.ToArray())
                {
                    if (!objects.TryGetValue((string)node["parentId"], out GameObject parent))
                    {
                        continue;
                    }
                    Transform child = parent.transform.GetChild((int)node["siblingIndex"]);
                    objects.Add((string)node["id"], child.gameObject);
                    remaining.Remove(node);
                }
                if (remaining.Count == prior)
                {
                    throw new InvalidOperationException("Saved hierarchy could not be resolved.");
                }
            }
            return objects;
        }

        private static void VerifySaved(GameObject saved, JObject snapshot)
        {
            JArray nodes = (JArray)snapshot["prefab"]["nodes"];
            Dictionary<string, GameObject> objects = ResolveSavedNodes(saved, nodes, (string)snapshot["prefab"]["rootNodeId"]);
            foreach (JObject node in nodes)
            {
                GameObject actual = objects[(string)node["id"]];
                if (actual.name != (string)node["name"] || actual.activeSelf != (bool)node["active"])
                {
                    throw new InvalidOperationException("Saved node mismatch: " + (string)node["id"]);
                }
                JArray scale = (JArray)node["localScale"];
                JArray rotation = (JArray)node["localRotation"];
                if (Vector3.Distance(actual.transform.localScale, new Vector3((float)scale[0], (float)scale[1], (float)scale[2])) > 0.0001f
                    || Quaternion.Angle(actual.transform.localRotation, new Quaternion((float)rotation[0], (float)rotation[1], (float)rotation[2], (float)rotation[3])) > 0.01f)
                {
                    throw new InvalidOperationException("Saved transform mismatch: " + actual.name);
                }
                RectTransform rect = actual.transform as RectTransform;
                JObject layout = node["rect"] as JObject;
                if (layout != null && (rect == null || !VectorMatches(layout["anchorMin"], rect.anchorMin)
                    || !VectorMatches(layout["anchorMax"], rect.anchorMax) || !VectorMatches(layout["pivot"], rect.pivot)
                    || !VectorMatches(layout["sizeDelta"], rect.sizeDelta) || !VectorMatches(layout["anchoredPosition"], rect.anchoredPosition)))
                {
                    throw new InvalidOperationException("Saved rect mismatch: " + actual.name);
                }
                Dictionary<string, int> indices = new Dictionary<string, int>();
                foreach (JObject component in (JArray)node["components"])
                {
                    string type = (string)component["type"];
                    int index = indices.TryGetValue(type, out int next) ? next : 0;
                    Component target = actual.GetComponents<Component>().Where(c => c != null && c.GetType().FullName == type).ElementAt(index);
                    indices[type] = index + 1;
                    UnityCliNativeParticleData.Verify(target, component["nativeSerialized"]);
                    SerializedObject serialized = new SerializedObject(target);
                    foreach (JProperty field in ((JObject)component["properties"]).Properties())
                    {
                        if (field.Name == "m_Name" || field.Name == "m_EditorHideFlags" || field.Name == "serializedVersion")
                        {
                            continue;
                        }
                        SerializedProperty property = serialized.FindProperty(field.Name);
                        bool matches = property != null && PropertyMatches(property, field.Value, objects);
                        if (!matches)
                        {
                            throw new InvalidOperationException("Saved property mismatch: " + actual.name + "/" + type + "/" + field.Name);
                        }
                    }
                }
            }
        }

        private static bool PropertyMatches(SerializedProperty property, JToken expected, Dictionary<string, GameObject> nodes)
        {
            if (property.isArray && expected is JArray array && array.Count == 0)
            {
                return property.arraySize == 0;
            }
            switch (property.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    return property.boolValue == (expected.Type == JTokenType.Boolean ? (bool)expected : (double)expected != 0);
                case SerializedPropertyType.Integer:
                    return property.longValue == (long)expected;
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.ArraySize:
                    return property.intValue == (int)expected;
                case SerializedPropertyType.Character:
                    return property.intValue == (expected.Type == JTokenType.String ? ((string)expected)[0] : (int)expected);
                case SerializedPropertyType.Float:
                    return Mathf.Abs(property.floatValue - (float)expected) < 0.0001f;
                case SerializedPropertyType.String:
                    return property.stringValue == (string)expected;
                case SerializedPropertyType.ObjectReference:
                    if (expected.Type == JTokenType.Null)
                    {
                        return property.objectReferenceValue == null;
                    }
                    if (expected["assetPath"] != null)
                    {
                        bool pathMatches = AssetDatabase.GetAssetPath(property.objectReferenceValue) == (string)expected["assetPath"];
                        if (expected["localFileId"] != null)
                        {
                            return pathMatches && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(property.objectReferenceValue, out string guid, out long localId)
                                && localId.ToString(System.Globalization.CultureInfo.InvariantCulture) == (string)expected["localFileId"];
                        }
                        return pathMatches;
                    }
                    GameObject node = nodes[(string)expected["nodeId"]];
                    return property.objectReferenceValue == node
                        || property.objectReferenceValue is Component reference && reference.gameObject == node;
                default:
                    return false;
            }
        }

        private static string VerifyAsset(JObject mapping)
        {
            string path = Required(mapping, "path");
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) || path.Contains("\\") || path.Split('/').Contains("..") || !File.Exists(path))
            {
                throw new InvalidOperationException("Missing or invalid asset path: " + path);
            }
            if (FileHash(path) != Required(mapping, "sha256"))
            {
                throw new InvalidOperationException("Asset hash mismatch: " + path);
            }
            if (mapping["metaSha256"] != null && FileHash(path + ".meta") != Required(mapping, "metaSha256"))
            {
                throw new InvalidOperationException("Asset importer metadata hash mismatch: " + path);
            }
            return path;
        }

        private static string FileHash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
            }
        }

        // Bounds use top-left image coordinates and a fixed alpha threshold, so faint
        // glow cannot silently change the measurable control silhouette between skins.
        private static void ValidateVisibleBounds(string path, JObject expected)
        {
            int threshold = (int?)expected["alphaThreshold"] ?? 128;
            int tolerance = (int?)expected["tolerance"] ?? 0;
            if (threshold < 1 || threshold > 255 || tolerance < 0 || tolerance > 8)
            {
                throw new InvalidOperationException("Invalid visibleBounds threshold or tolerance: " + path);
            }
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(path), false))
                {
                    throw new InvalidOperationException("Cannot decode visibleBounds image: " + path);
                }
                Color32[] pixels = texture.GetPixels32();
                int left = texture.width;
                int top = texture.height;
                int right = -1;
                int bottom = -1;
                int visibleCount = 0;
                int centerCount = 0;
                int centerVisible = 0;
                for (int y = 0; y < texture.height; y++)
                {
                    for (int x = 0; x < texture.width; x++)
                    {
                        bool center = x >= texture.width / 4 && x < texture.width * 3 / 4
                            && y >= texture.height / 4 && y < texture.height * 3 / 4;
                        if (center)
                        {
                            centerCount++;
                        }
                        if (pixels[y * texture.width + x].a < threshold)
                        {
                            continue;
                        }
                        visibleCount++;
                        if (center)
                        {
                            centerVisible++;
                        }
                        int imageY = texture.height - 1 - y;
                        left = Math.Min(left, x);
                        right = Math.Max(right, x);
                        top = Math.Min(top, imageY);
                        bottom = Math.Max(bottom, imageY);
                    }
                }
                int width = right - left + 1;
                int height = bottom - top + 1;
                float coverage = (float)visibleCount / pixels.Length;
                float centerCoverage = centerCount == 0 ? 0 : (float)centerVisible / centerCount;
                if (coverage < ((float?)expected["minCoverage"] ?? 0)
                    || centerCoverage < ((float?)expected["minCenterCoverage"] ?? 0))
                {
                    throw new InvalidOperationException($"Skin coverage mismatch: {path}; coverage {coverage:F3}, center {centerCoverage:F3}.");
                }
                if (right < 0 || Math.Abs(left - (int)expected["x"]) > tolerance
                    || Math.Abs(top - (int)expected["y"]) > tolerance
                    || Math.Abs(width - (int)expected["width"]) > tolerance
                    || Math.Abs(height - (int)expected["height"]) > tolerance)
                {
                    throw new InvalidOperationException($"Visible bounds mismatch: {path}; actual ({left},{top},{width},{height}), tolerance {tolerance}.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static JObject ReadDocument(string path)
        {
            if (new FileInfo(path).Length > 8 * 1024 * 1024)
            {
                throw new InvalidOperationException("Contract document exceeds 8 MiB.");
            }
            return JObject.Parse(File.ReadAllText(path));
        }

        private static string Required(JObject value, string key)
        {
            if (value[key]?.Type != JTokenType.String || string.IsNullOrWhiteSpace((string)value[key]))
            {
                throw new InvalidOperationException("Missing string field: " + key);
            }
            return (string)value[key];
        }
    }
}
