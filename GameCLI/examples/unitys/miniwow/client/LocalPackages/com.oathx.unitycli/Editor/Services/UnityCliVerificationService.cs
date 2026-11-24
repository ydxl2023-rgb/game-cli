using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Evaluates declarative obligations against actual Unity assets without project-specific node rules.</summary>
    internal static class UnityCliVerificationService
    {
        internal static JObject Execute(UnityCliBridgeRequest request)
        {
            string text = File.ReadAllText(request.jsonFile);
            JObject contract = JObject.Parse(text);
            JArray checks = contract["checks"] as JArray;
            JObject nodes = contract["nodes"] as JObject;
            if ((string)contract["schemaVersion"] != "ui-verification-contract/1" || checks == null || checks.Count == 0 || nodes == null
                || checks.Select(c => (string)c["id"]).Distinct().Count() != checks.Count)
            {
                throw new InvalidOperationException("Invalid, empty or duplicate verification contract.");
            }
            AssetDatabase.Refresh();
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(request.prefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException("Target prefab not found.");
            }
            JArray results = new JArray();
            Scene scene = EditorSceneManager.NewPreviewScene();
            try
            {
                foreach (JObject check in checks)
                {
                    JObject result = new JObject { ["id"] = check["id"], ["status"] = "failed" };
                    GameObject instance = UnityEngine.Object.Instantiate(prefab);
                    SceneManager.MoveGameObjectToScene(instance, scene);
                    try
                    {
                        Evaluate(instance, nodes, check, result);
                    }
                    catch (Exception error)
                    {
                        result["reason"] = error.Message;
                    }
                    finally
                    {
                        UnityEngine.Object.DestroyImmediate(instance);
                    }
                    results.Add(result);
                }
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
            JArray dependencies = new JArray();
            foreach (string path in AssetDatabase.GetDependencies(request.prefabPath, true).OrderBy(p => p, StringComparer.Ordinal))
            {
                foreach (string file in new[] { path, path + ".meta" })
                {
                    if (File.Exists(file))
                    {
                        string physical = file;
                        if (path.StartsWith("Packages/", StringComparison.Ordinal))
                        {
                            var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(path);
                            if (package != null)
                            {
                                physical = Path.Combine(package.resolvedPath, file.Substring(package.assetPath.Length + 1));
                            }
                        }
                        string project = Directory.GetParent(Application.dataPath).FullName.Replace('\\', '/') + "/";
                        string absolute = Path.GetFullPath(physical).Replace('\\', '/');
                        string storage = absolute.StartsWith(project, StringComparison.OrdinalIgnoreCase) ? absolute.Substring(project.Length) : absolute;
                        dependencies.Add(new JObject { ["path"] = file, ["storagePath"] = storage, ["sha256"] = Hash(File.ReadAllBytes(physical)) });
                    }
                }
            }
            return new JObject
            {
                ["schemaVersion"] = "ui-verification-evidence/1", ["checkerVersion"] = "unity-verifier/1",
                ["unityVersion"] = Application.unityVersion, ["contractSha256"] = Hash(Encoding.UTF8.GetBytes(text)),
                ["prefabPath"] = request.prefabPath, ["mode"] = "isolated-editor-animator-evaluation",
                ["passed"] = results.All(r => (string)r["status"] == "passed"),
                ["checks"] = results, ["dependencies"] = dependencies
            };
        }

        private static void Evaluate(GameObject root, JObject nodes, JObject check, JObject result)
        {
            string path = (string)nodes[(string)check["nodeId"]];
            if (path == null)
            {
                throw new InvalidOperationException("Unknown node ID.");
            }
            Transform target = Resolve(root.transform, path);
            if (target == null)
            {
                throw new InvalidOperationException("Missing target node: " + path);
            }
            string kind = (string)check["kind"];
            if (kind == "unsupported")
            {
                result["status"] = "unverified";
                result["reason"] = check["reason"] ?? "Unsupported source feature.";
                return;
            }
            if (kind == "node")
            {
                if ((check["active"]?.Type != null && check["active"].Type != JTokenType.Null && target.gameObject.activeSelf != (bool)check["active"])
                    || (check["siblingIndex"]?.Type != null && check["siblingIndex"].Type != JTokenType.Null && target.GetSiblingIndex() != (int)check["siblingIndex"]))
                {
                    throw new InvalidOperationException("Node activation or sibling order differs.");
                }
                result["status"] = "passed";
                return;
            }
            if (kind == "animator.controller" || kind == "animation.clip")
            {
                Animator animator = target.GetComponent<Animator>();
                if (animator == null || animator.runtimeAnimatorController == null)
                {
                    throw new InvalidOperationException("Missing Animator or controller: " + path);
                }
                result["actual"] = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
                if (kind == "animation.clip")
                {
                    AnimationClip[] clips = animator.runtimeAnimatorController.animationClips.Where(c => c.name == (string)check["clip"]).Distinct().ToArray();
                    if (clips.Length != 1 || Math.Abs(clips[0].length - (double)check["duration"]) > 0.001
                        || clips[0].isLooping != (bool)check["loop"])
                    {
                        throw new InvalidOperationException("Source clip, duration or loop differs.");
                    }
                }
                result["status"] = "passed";
                return;
            }
            if (kind == "animation.sample")
            {
                string animatorPath = (string)nodes[(string)check["animatorNodeId"]];
                Transform owner = animatorPath == null ? null : Resolve(root.transform, animatorPath);
                Animator animator = owner == null ? null : owner.GetComponent<Animator>();
                if (animator == null || animator.runtimeAnimatorController == null)
                {
                    throw new InvalidOperationException("Missing animation owner/controller.");
                }
                // Activation is an explicit test setup, not an edit to source assets.
                for (Transform current = target; current != null; current = current.parent)
                {
                    current.gameObject.SetActive(true);
                }
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.Rebind();
                animator.Update(0);
                if (check["state"] != null)
                {
                    int state = Animator.StringToHash((string)check["state"]);
                    if (!animator.HasState(0, state))
                    {
                        throw new InvalidOperationException("Requested state does not exist.");
                    }
                    animator.Play(state, 0, 0);
                    animator.Update(0);
                }
                float time = (float)check["timeSeconds"];
                if (float.IsNaN(time) || float.IsInfinity(time) || time < 0 || time > 60)
                {
                    throw new InvalidOperationException("Sample time must be 0..60 seconds.");
                }
                // Small steps exercise state transitions rather than jumping directly to a Clip pose.
                while (time > 0)
                {
                    float delta = Mathf.Min(time, 1f / 60f);
                    animator.Update(delta);
                    time -= delta;
                }
            }
            else if (kind != "component" && kind != "property")
            {
                result["status"] = "unverified";
                result["reason"] = "Unsupported check kind: " + kind;
                return;
            }
            string typeName = (string)check["component"];
            Type type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(typeName, false)).FirstOrDefault(t => t != null);
            if (type == null || !typeof(Component).IsAssignableFrom(type))
            {
                result["status"] = "unverified";
                result["reason"] = "Unsupported component type: " + typeName;
                return;
            }
            Component component = target.GetComponent(type);
            if (component == null)
            {
                throw new InvalidOperationException("Missing component: " + typeName);
            }
            if (kind == "component")
            {
                result["status"] = "passed";
                return;
            }
            SerializedObject serialized = new SerializedObject(component);
            serialized.Update();
            SerializedProperty property = serialized.FindProperty((string)check["property"]);
            string propertyPath = (string)check["property"];
            bool angle = component is Transform && new[] { "localEulerAngles.x", "localEulerAngles.y", "localEulerAngles.z" }.Contains(propertyPath);
            if (property == null && !angle)
            {
                throw new InvalidOperationException("Missing serialized property.");
            }
            double actual;
            if (angle)
            {
                actual = ((Transform)component).localEulerAngles[propertyPath.EndsWith(".x") ? 0 : propertyPath.EndsWith(".y") ? 1 : 2];
            }
            else if (property.propertyType == SerializedPropertyType.Float)
            {
                actual = property.floatValue;
            }
            else if (property.propertyType == SerializedPropertyType.Integer || property.propertyType == SerializedPropertyType.Enum)
            {
                actual = property.intValue;
            }
            else if (property.propertyType == SerializedPropertyType.Boolean)
            {
                actual = property.boolValue ? 1 : 0;
            }
            else
            {
                result["status"] = "unverified";
                result["reason"] = "Only numeric and boolean leaf properties are supported in version 1.";
                return;
            }
            double expected = (double)check["expected"];
            double tolerance = (double?)check["tolerance"] ?? 0.0001;
            if (double.IsNaN(tolerance) || double.IsInfinity(tolerance) || tolerance < 0 || double.IsNaN(expected) || double.IsInfinity(expected))
            {
                throw new InvalidOperationException("Invalid expectation or tolerance.");
            }
            result["actual"] = actual;
            result["expected"] = expected;
            result["tolerance"] = tolerance;
            double error = angle ? Math.Abs(Mathf.DeltaAngle((float)actual, (float)expected)) : Math.Abs(actual - expected);
            result["status"] = error <= tolerance ? "passed" : "failed";
        }

        private static Transform Resolve(Transform root, string path)
        {
            Transform current = root;
            foreach (string name in path.Split('/').Where(p => p.Length > 0))
            {
                Transform[] matches = current.Cast<Transform>().Where(t => t.name == name).ToArray();
                if (matches.Length != 1)
                {
                    return null;
                }
                current = matches[0];
            }
            return current;
        }

        private static string Hash(byte[] bytes)
        {
            using (SHA256 hash = SHA256.Create())
            {
                return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
