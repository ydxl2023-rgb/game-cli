using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>Reads compiled Widget contracts and serialized references without invoking user code.</summary>
    internal static class UnityCliWidgetBindingEvidence
    {
        /// <summary>Export static binding evidence from the same disposable prefab as the layout export.</summary>
        /// <param name="root">Loaded prefab root.</param>
        /// <param name="ids">Exported hierarchy identities.</param>
        /// <param name="prefabPath">Asset path used by Widget Config declarations.</param>
        /// <returns>Versioned evidence, including unsupported cases.</returns>
        public static JObject Export(GameObject root, Dictionary<Transform, string> ids, string prefabPath)
        {
            var widgets = new JArray();
            var issues = new JArray();
            foreach (var pair in ids)
            {
                var components = pair.Key.GetComponents<Component>();
                var metadata = components.Where(c => c && c.GetType().FullName == "Oathx.Widget.Metadata").ToArray();
                var owners = components.Where(c => c && WidgetVM(c.GetType()) != null).Select(c => c.GetType()).ToArray();
                // UISystem adds the page Widget at runtime; absence on the prefab is valid.
                if (owners.Length == 0 && pair.Key == root.transform)
                {
                    owners = TypeCache.GetTypesDerivedFrom<MonoBehaviour>()
                        .Where(t => !t.IsAbstract && !t.ContainsGenericParameters && WidgetVM(t) != null
                            && string.Equals(Read(Attribute(t, "Oathx.Widget.ConfigAttribute"), "path") as string, prefabPath, StringComparison.Ordinal))
                        .ToArray();
                }
                if (metadata.Length == 0 && owners.Length == 0)
                {
                    continue;
                }

                if (metadata.Length != 1 || owners.Length != 1)
                {
                    issues.Add(new JObject { ["nodeId"] = pair.Value, ["code"] = "widget-metadata-count", ["metadataCount"] = metadata.Length, ["widgetCount"] = owners.Length });
                    continue;
                }

                var vm = WidgetVM(owners[0]);
                var declared = new JArray();
                var fields = new JArray();
                var vmFields = vm.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                    .Where(f => Attribute(f, "Oathx.Widget.BindingAttribute") != null).OrderBy(f => f.MetadataToken).ToArray();
                if (vm.BaseType != null && vm.BaseType.FullName != "Oathx.Widget.VM")
                {
                    issues.Add(new JObject { ["nodeId"] = pair.Value, ["code"] = "inherited-vm-order-unverified", ["severity"] = "incomplete" });
                }

                foreach (var field in vmFields)
                {
                    string syncName = Read(Attribute(field, "Oathx.Widget.BindingAttribute"), "name") as string;
                    if (string.IsNullOrEmpty(syncName))
                    {
                        syncName = field.FieldType.Name == "ButtonClick" ? "SyncClick" : "SyncVisible";
                    }

                    var sync = Resolve(syncName);
                    var expectedComponent = Resolve(Read(Attribute(sync, "Oathx.Widget.BindingAttribute"), "name") as string);
                    string property = char.ToUpperInvariant(field.Name[0]) + field.Name.Substring(1);
                    declared.Add(new JObject {
                        ["name"] = field.Name, ["valueType"] = field.FieldType.FullName, ["syncType"] = sync == null ? syncName : sync.Name,
                        ["syncValid"] = sync != null && sync.GetInterfaces().Any(t => t.FullName == "Oathx.Widget.ISyncAction"),
                        ["componentType"] = expectedComponent == null ? null : expectedComponent.FullName,
                        ["propertyPresent"] = vm.GetProperty(property, BindingFlags.Public | BindingFlags.Instance)?.GetSetMethod() != null
                    });
                }

                var serializedFields = Read(metadata[0], "metadatas") as IEnumerable;
                if (serializedFields != null)
                {
                    foreach (var field in serializedFields)
                    {
                        if (field == null)
                        {
                            fields.Add(JValue.CreateNull());
                            continue;
                        }
                        string syncName = Read(field, "type") as string;
                        var sync = Resolve(syncName);
                        var expectedComponent = Resolve(Read(Attribute(sync, "Oathx.Widget.BindingAttribute"), "name") as string);
                        var targets = (Read(field, "targets") as IEnumerable)?.Cast<object>().ToArray() ?? new object[0];
                        var references = (Read(field, "components") as IEnumerable)?.Cast<object>().ToArray() ?? new object[0];
                        var bindings = new JArray();
                        for (int index = 0; index < Math.Max(targets.Length, references.Length); index++)
                        {
                            var target = index < targets.Length ? targets[index] as GameObject : null;
                            var component = index < references.Length ? references[index] as Component : null;
                            string targetId = null, componentId = null;
                            if (target)
                            {
                                ids.TryGetValue(target.transform, out targetId);
                            }
                            if (component)
                            {
                                ids.TryGetValue(component.transform, out componentId);
                            }
                            bindings.Add(new JObject {
                                ["nodeId"] = targetId, ["componentNodeId"] = componentId,
                                ["componentType"] = component ? component.GetType().FullName : null,
                                ["componentIndex"] = component ? Array.IndexOf(component.GetComponents<Component>(), component) : -1,
                                ["assignable"] = component && expectedComponent != null && expectedComponent.IsInstanceOfType(component)
                            });
                        }
                        fields.Add(new JObject { ["name"] = Read(field, "name") as string, ["syncType"] = syncName,
                            ["targetCount"] = targets.Length, ["componentCount"] = references.Length, ["targets"] = bindings });
                    }
                }
                string className = Read(metadata[0], "className") as string;
                string guid = Read(metadata[0], "classGuid") as string;
                var script = string.IsNullOrEmpty(guid) ? null : AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
                widgets.Add(new JObject {
                    ["nodeId"] = pair.Value, ["uiType"] = owners[0].FullName, ["vmType"] = vm.FullName,
                    ["metadataVmType"] = className, ["metadataTypeMatches"] = Resolve(className) == vm,
                    ["scriptMatches"] = script && script.GetClass() == vm,
                    ["declaredFields"] = declared, ["fields"] = fields
                });
            }

            return new JObject { ["schemaVersion"] = "oathx-widget-bindings/1", ["mode"] = "static-compiled-contract", ["widgets"] = widgets, ["issues"] = issues };
        }

        /// <summary>Find the generic VM inherited by a Widget implementation.</summary>
        /// <param name="type">Component type.</param>
        /// <returns>The VM type or null.</returns>
        private static Type WidgetVM(Type type)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition().FullName == "Oathx.Widget.UIWidget`1")
                {
                    return current.GetGenericArguments()[0];
                }
            }

            return null;
        }

        /// <summary>Read a framework attribute without referencing its optional assembly.</summary>
        /// <param name="member">Annotated member.</param>
        /// <param name="name">Full attribute type name.</param>
        /// <returns>The attribute or null.</returns>
        private static object Attribute(MemberInfo member, string name)
        {
            return member?.GetCustomAttributes(false).FirstOrDefault(a => a.GetType().FullName == name);
        }

        /// <summary>Read a public serialized framework field.</summary>
        /// <param name="value">Framework object.</param>
        /// <param name="name">Field name.</param>
        /// <returns>The stored value or null.</returns>
        private static object Read(object value, string name)
        {
            return value?.GetType().GetField(name)?.GetValue(value) ?? value?.GetType().GetProperty(name)?.GetValue(value);
        }

        /// <summary>Resolve only an unambiguous loaded type.</summary>
        /// <param name="name">Full or short type name.</param>
        /// <returns>The unique type or null.</returns>
        private static Type Resolve(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            // Match the Widget generator's built-in component namespaces before short-name fallback.
            foreach (string candidate in new[] { name, "Oathx.Widget." + name, "UnityEngine.UI." + name, "UnityEngine." + name })
            {
                var exact = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(candidate, false)).Where(t => t != null).Distinct().ToArray();
                if (exact.Length == 1)
                {
                    return exact[0];
                }
                if (exact.Length > 1)
                {
                    return null;
                }
            }

            var matches = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException error)
                {
                    types = error.Types;
                }
                matches.AddRange(types.Where(t => t != null && (t.FullName == name || t.Name == name)));
            }

            return matches.Distinct().Count() == 1 ? matches[0] : null;
        }
    }
}
