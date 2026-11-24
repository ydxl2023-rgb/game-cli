using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Backend for <c>POST /components</c>: add or remove a Component on a scene GameObject
    /// resolved by instanceId or hierarchy path.
    ///
    /// Type resolution mirrors <c>ReflectionCallInvoker</c>:
    ///   1. <c>Type.GetType(name)</c> handles assembly-qualified names ("Foo, MyAsm").
    ///   2. <c>assembly.GetType(name)</c> across loaded assemblies handles full names.
    ///   3. Short-name fallback restricted to <c>UnityEngine.Component</c> subclasses, so a
    ///      bare "Rigidbody" matches <c>UnityEngine.Rigidbody</c> without sweeping in random
    ///      types of the same name.
    ///
    /// Both Add and Remove go through <c>Undo.*</c> so the change shows up in the Editor's
    /// undo stack just like a manual Inspector action, and the owning scene is explicitly
    /// marked dirty so a subsequent <c>save</c> persists it.
    /// </summary>
    internal static class UnityCliComponentsService
    {
        /// <summary>
        /// Executes the requested component add or remove operation.
        /// </summary>
        /// <param name="request">The bridge request containing target object and component type.</param>
        /// <returns>The component operation result data.</returns>
        public static ComponentsData Execute(UnityCliBridgeRequest request)
        {
            if (request == null)
            {
                throw new InvalidOperationException("Empty request.");
            }

            var action = (request.action ?? string.Empty).Trim().ToLowerInvariant();
            if (action != "add" && action != "remove")
            {
                throw new InvalidOperationException(
                    "components requires --action add|remove (got: \"" + (request.action ?? string.Empty) + "\").");
            }

            var requested = (request.component ?? string.Empty).Trim();
            if (requested.Length == 0)
            {
                throw new InvalidOperationException("Missing --component <type> argument.");
            }

            if (!string.IsNullOrWhiteSpace(request.assetPath))
            {
                return ExecutePrefabAsset(request, action, requested);
            }

            var gameObject = ResolveGameObject(request);
            var responseAt = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);

            if (action == "add")
            {
                return DoAdd(gameObject, requested, responseAt, string.Empty);
            }

            return DoRemove(gameObject, requested, responseAt, string.Empty);
        }

        /// <summary>
        /// Executes a component add or remove operation against a prefab asset.
        /// </summary>
        /// <param name="request">The bridge request containing prefab target and component type.</param>
        /// <param name="action">The normalized action.</param>
        /// <param name="requested">The requested component type name.</param>
        /// <returns>The component operation result data.</returns>
        static ComponentsData ExecutePrefabAsset(UnityCliBridgeRequest request, string action, string requested)
        {
            var assetPath = NormalizeAssetPath(request.assetPath);
            if (string.IsNullOrWhiteSpace(assetPath) || !assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("components --asset currently supports prefab assets only: " + request.assetPath);
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(assetPath) == null)
            {
                throw new InvalidOperationException("Prefab asset was not found: " + assetPath);
            }

            GameObject prefabRoot = null;
            try
            {
                prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
                var gameObject = ResolveGameObjectInRoot(prefabRoot, request.objectPath);
                var responseAt = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
                var data = action == "add"
                    ? DoAdd(gameObject, requested, responseAt, assetPath)
                    : DoRemove(gameObject, requested, responseAt, assetPath);

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
                AssetDatabase.SaveAssets();
                return data;
            }
            finally
            {
                if (prefabRoot != null)
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
        }

        // ---- add ---------------------------------------------------------------------

        /// <summary>
        /// Adds the requested component type to the target GameObject.
        /// </summary>
        /// <param name="gameObject">The target scene GameObject.</param>
        /// <param name="requested">The component type name requested by the CLI.</param>
        /// <param name="respondedAtUtc">The timestamp to include in the response.</param>
        /// <returns>The result data for the added component.</returns>
        static ComponentsData DoAdd(GameObject gameObject, string requested, string respondedAtUtc, string assetPath)
        {
            var componentType = ResolveComponentType(requested);
            if (componentType == null)
            {
                throw new InvalidOperationException(
                    "Component type was not found in any loaded assembly: \"" + requested + "\". " +
                    "Try the full name (e.g. UnityEngine.Rigidbody) or assembly-qualified (\"My.Type, My.Asm\").");
            }

            if (!typeof(Component).IsAssignableFrom(componentType))
            {
                throw new InvalidOperationException(
                    "Type \"" + componentType.FullName + "\" is not a UnityEngine.Component and cannot be added.");
            }

            if (componentType.IsAbstract)
            {
                throw new InvalidOperationException(
                    "Type \"" + componentType.FullName + "\" is abstract and cannot be added directly.");
            }

            // Transform is special: every GameObject already has exactly one and Unity refuses
            // to add another. Surface that explicitly so the user gets a useful error.
            if (componentType == typeof(Transform))
            {
                throw new InvalidOperationException("Transform cannot be added because every GameObject already has one.");
            }

            // Some components (e.g. Camera, Rigidbody) can stack as duplicates; Unity allows it
            // but it's almost never intentional from a CLI. We still go through with it because
            // some MonoBehaviours legitimately allow duplicates, and refusing here would block
            // those. Just record whether one already existed in the response.
            var componentBefore = gameObject.GetComponent(componentType);

            Component added;
            try
            {
                added = string.IsNullOrEmpty(assetPath)
                    ? Undo.AddComponent(gameObject, componentType)
                    : gameObject.AddComponent(componentType);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "AddComponent threw: " + exception.Message +
                    (componentBefore != null
                        ? " (one " + componentType.Name + " already exists on this GameObject)"
                        : string.Empty));
            }

            if (added == null)
            {
                // Unity returns null when DisallowMultipleComponent forbids the add, or when a
                // RequireComponent dependency couldn't be auto-attached.
                throw new InvalidOperationException(
                    "AddComponent returned null for \"" + componentType.FullName + "\" on " +
                    UnityCliSceneGameObjectSearcher.GetHierarchyPath(gameObject) +
                    ". Check [DisallowMultipleComponent] / [RequireComponent] constraints.");
            }

            EditorUtility.SetDirty(gameObject);
            if (gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }

            return new ComponentsData
            {
                action = "add",
                assetPath = assetPath ?? string.Empty,
                instanceId = gameObject.GetInstanceID(),
                objectPath = UnityCliSceneGameObjectSearcher.GetHierarchyPath(gameObject),
                requestedComponent = requested,
                resolvedComponent = componentType.FullName ?? componentType.Name,
                componentInstanceId = added.GetInstanceID(),
                added = true,
                removed = false,
                respondedAtUtc = respondedAtUtc
            };
        }

        // ---- remove ------------------------------------------------------------------

        /// <summary>
        /// Removes the requested component type from the target GameObject.
        /// </summary>
        /// <param name="gameObject">The target scene GameObject.</param>
        /// <param name="requested">The component type name requested by the CLI.</param>
        /// <param name="respondedAtUtc">The timestamp to include in the response.</param>
        /// <returns>The result data for the removed component.</returns>
        static ComponentsData DoRemove(GameObject gameObject, string requested, string respondedAtUtc, string assetPath)
        {
            string componentName = requested;
            int componentIndex = 0;
            int selectorStart = requested.LastIndexOf('[');
            if (selectorStart >= 0)
            {
                if (!requested.EndsWith("]", StringComparison.Ordinal)
                    || !int.TryParse(requested.Substring(selectorStart + 1, requested.Length - selectorStart - 2),
                        NumberStyles.None, CultureInfo.InvariantCulture, out componentIndex))
                {
                    throw new InvalidOperationException("Component selectors require a nonnegative zero-based index, for example Metadata[1].");
                }

                componentName = requested.Substring(0, selectorStart).Trim();
            }

            var existing = gameObject.GetComponents<Component>()
                .Where(c => c != null && MatchesComponent(c.GetType(), componentName))
                .ToList();

            if (existing.Count == 0)
            {
                throw new InvalidOperationException(
                    "No component matching \"" + requested + "\" found on " +
                    UnityCliSceneGameObjectSearcher.GetHierarchyPath(gameObject) + ".");
            }

            // Multiple distinct types matched the same short name; refuse the ambiguous request.
            var distinctTypes = existing.Select(c => c.GetType()).Distinct().ToList();
            if (distinctTypes.Count > 1)
            {
                var typeList = string.Join(", ", distinctTypes.Select(t => t.FullName));
                throw new InvalidOperationException(
                    "Ambiguous component name \"" + requested + "\" matches multiple types on this GameObject: " +
                    typeList + ". Re-run with the full name.");
            }

            var componentType = distinctTypes[0];

            if (componentType == typeof(Transform))
            {
                throw new InvalidOperationException("Transform cannot be removed from a GameObject.");
            }

            if (componentIndex >= existing.Count)
            {
                throw new InvalidOperationException("Component index " + componentIndex
                    + " is outside the " + existing.Count + " matching components.");
            }

            // An explicit index preserves the valid component when only a duplicate is unwanted.
            // Requests without an index retain the original first-component behavior.
            var target = existing[componentIndex];
            var targetInstanceId = target.GetInstanceID();

            try
            {
                if (string.IsNullOrEmpty(assetPath))
                {
                    Undo.DestroyObjectImmediate(target);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(target, true);
                }
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "RemoveComponent (Undo.DestroyObjectImmediate) threw: " + exception.Message);
            }

            EditorUtility.SetDirty(gameObject);
            if (gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }

            return new ComponentsData
            {
                action = "remove",
                assetPath = assetPath ?? string.Empty,
                instanceId = gameObject.GetInstanceID(),
                objectPath = UnityCliSceneGameObjectSearcher.GetHierarchyPath(gameObject),
                requestedComponent = requested,
                resolvedComponent = componentType.FullName ?? componentType.Name,
                componentInstanceId = targetInstanceId,
                added = false,
                removed = true,
                respondedAtUtc = respondedAtUtc
            };
        }

        // ---- helpers -----------------------------------------------------------------

        /// <summary>
        /// Resolves the target GameObject from instance id or hierarchy path.
        /// </summary>
        /// <param name="request">The bridge request containing the target selector.</param>
        /// <returns>The resolved scene GameObject.</returns>
        static GameObject ResolveGameObject(UnityCliBridgeRequest request)
        {
            if (request.instanceId != 0)
            {
                var target = EditorUtility.InstanceIDToObject(request.instanceId);
                if (target is GameObject go)
                {
                    return go;
                }

                if (target is Component existingComponent)
                {
                    return existingComponent.gameObject;
                }

                throw new InvalidOperationException(
                    "GameObject was not found for instance id: " +
                    request.instanceId.ToString(CultureInfo.InvariantCulture));
            }

            if (string.IsNullOrWhiteSpace(request.objectPath))
            {
                throw new InvalidOperationException("Missing target. Provide --id or --path.");
            }

            var found = FindGameObjectByPath(request.objectPath);
            if (found == null)
            {
                throw new InvalidOperationException("GameObject was not found at path: " + request.objectPath);
            }
            return found;
        }

        /// <summary>
        /// Resolves a GameObject inside a loaded prefab root.
        /// </summary>
        /// <param name="root">The loaded prefab root.</param>
        /// <param name="path">The requested root-relative or full hierarchy path.</param>
        /// <returns>The matching prefab GameObject.</returns>
        static GameObject ResolveGameObjectInRoot(GameObject root, string path)
        {
            if (root == null)
            {
                throw new InvalidOperationException("Prefab root was not loaded.");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidOperationException("components with --asset requires --path <prefab hierarchy path>.");
            }

            var found = FindInHierarchy(root, path);
            if (found == null)
            {
                throw new InvalidOperationException("GameObject was not found in prefab " + root.name + ": " + path);
            }

            return found;
        }

        /// <summary>
        /// Searches all loaded scenes for a GameObject with the exact hierarchy path.
        /// </summary>
        /// <param name="path">The hierarchy path to find.</param>
        /// <returns>The matching GameObject, or null when no match exists.</returns>
        static GameObject FindGameObjectByPath(string path)
        {
            for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                var scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded) continue;

                foreach (var root in scene.GetRootGameObjects())
                {
                    var hit = FindInHierarchy(root, path);
                    if (hit != null) return hit;
                }
            }
            return null;
        }

        /// <summary>
        /// Recursively searches a root GameObject hierarchy by full path.
        /// </summary>
        /// <param name="gameObject">The current GameObject to inspect.</param>
        /// <param name="path">The hierarchy path to match.</param>
        /// <returns>The matching GameObject, or null when this branch has no match.</returns>
        static GameObject FindInHierarchy(GameObject gameObject, string path)
        {
            if (MatchesHierarchyPath(gameObject, path))
            {
                return gameObject;
            }

            var transform = gameObject.transform;
            for (var index = 0; index < transform.childCount; index++)
            {
                var hit = FindInHierarchy(transform.GetChild(index).gameObject, path);
                if (hit != null) return hit;
            }
            return null;
        }

        /// <summary>
        /// Checks whether a GameObject matches a full, root-relative, or local hierarchy path.
        /// </summary>
        /// <param name="gameObject">The GameObject to compare.</param>
        /// <param name="path">The requested hierarchy path.</param>
        /// <returns>True when the path resolves to the GameObject.</returns>
        static bool MatchesHierarchyPath(GameObject gameObject, string path)
        {
            if (gameObject == null || string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var normalizedPath = NormalizeHierarchyPath(path);
            var fullPath = NormalizeHierarchyPath(UnityCliSceneGameObjectSearcher.GetHierarchyPath(gameObject));
            if (string.Equals(fullPath, normalizedPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return string.Equals(gameObject.name, normalizedPath, StringComparison.OrdinalIgnoreCase)
                || fullPath.EndsWith("/" + normalizedPath, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Normalizes Unity asset paths to forward slash form.
        /// </summary>
        /// <param name="path">The raw asset path.</param>
        /// <returns>The normalized path.</returns>
        static string NormalizeAssetPath(string path)
        {
            return (path ?? string.Empty).Trim().Replace('\\', '/');
        }

        /// <summary>
        /// Normalizes hierarchy paths to forward slash form without outer separators.
        /// </summary>
        /// <param name="path">The raw hierarchy path.</param>
        /// <returns>The normalized hierarchy path.</returns>
        static string NormalizeHierarchyPath(string path)
        {
            return (path ?? string.Empty).Trim().Replace('\\', '/').Trim('/');
        }

        /// <summary>
        /// Resolves a Component type from full, assembly-qualified, or unique short name.
        /// </summary>
        /// <param name="typeName">The requested component type name.</param>
        /// <returns>The resolved component type, or null when no match exists.</returns>
        static Type ResolveComponentType(string typeName)
        {
            // Step 1 + 2: exact / full name across loaded assemblies.
            var direct = Type.GetType(typeName, false, false);
            if (direct != null && typeof(Component).IsAssignableFrom(direct))
            {
                return direct;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic) continue;

                Type found = null;
                try { found = assembly.GetType(typeName, false, false); }
                catch { continue; }

                if (found != null && typeof(Component).IsAssignableFrom(found))
                {
                    return found;
                }
            }

            // Step 3: short-name fallback, restricted to Component subclasses.
            var matches = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic) continue;

                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray(); }
                catch { continue; }

                foreach (var type in types)
                {
                    if (type == null) continue;
                    if (!typeof(Component).IsAssignableFrom(type)) continue;
                    if (type.IsAbstract) continue;

                    if (string.Equals(type.Name, typeName, StringComparison.OrdinalIgnoreCase))
                    {
                        matches.Add(type);
                    }
                }
            }

            if (matches.Count == 1) return matches[0];
            if (matches.Count > 1)
            {
                var list = string.Join(", ", matches.Select(t => t.FullName).Take(8));
                throw new InvalidOperationException(
                    "Component short name \"" + typeName + "\" is ambiguous; matches: " + list +
                    (matches.Count > 8 ? ", ..." : string.Empty) +
                    ". Use the full name to disambiguate.");
            }

            return null;
        }

        /// <summary>
        /// Checks whether a component type matches the requested type string.
        /// </summary>
        /// <param name="type">The component type to compare.</param>
        /// <param name="requested">The requested short, full, or assembly-qualified type name.</param>
        /// <returns>True when the type matches the request.</returns>
        static bool MatchesComponent(Type type, string requested)
        {
            if (type == null) return false;
            if (string.Equals(type.Name, requested, StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(type.FullName, requested, StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(type.AssemblyQualifiedName, requested, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
