using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Modifies serialized component properties on scene GameObjects.
    /// </summary>
    internal static class UnityCliSceneComponentModifier
    {
        /// <summary>
        /// Resolves the target component and applies one serialized property change.
        /// </summary>
        /// <param name="request">The bridge request containing target, component, property, and value.</param>
        /// <returns>The serialized modification result.</returns>
        public static ModifyData Modify(UnityCliBridgeRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.assetPath))
            {
                return ModifyPrefabAsset(request);
            }

            var gameObject = ResolveGameObject(request);
            return ModifyComponent(request, gameObject, null, string.Empty);
        }

        /// <summary>
        /// Loads a prefab asset, applies one serialized property change, and saves it back.
        /// </summary>
        /// <param name="request">The bridge request containing prefab target and property data.</param>
        /// <returns>The serialized modification result.</returns>
        static ModifyData ModifyPrefabAsset(UnityCliBridgeRequest request)
        {
            var assetPath = NormalizeAssetPath(request.assetPath);
            if (string.IsNullOrWhiteSpace(assetPath) || !assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("modify --asset currently supports prefab assets only: " + request.assetPath);
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
                var data = ModifyComponent(request, gameObject, prefabRoot, assetPath);
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

        /// <summary>
        /// Applies one serialized property change to a component on a resolved GameObject.
        /// </summary>
        /// <param name="request">The bridge request containing component, property, and value.</param>
        /// <param name="gameObject">The resolved target GameObject.</param>
        /// <param name="contextRoot">The prefab root used for object reference resolution, or null for scenes.</param>
        /// <param name="assetPath">The asset path being modified, or empty for scenes.</param>
        /// <returns>The serialized modification result.</returns>
        static ModifyData ModifyComponent(UnityCliBridgeRequest request, GameObject gameObject, GameObject contextRoot, string assetPath)
        {
            var component = ResolveComponent(gameObject, request.component);
            var serializedObject = new SerializedObject(component);
            var property = ResolveProperty(serializedObject, request.property);
            if (property == null)
            {
                throw new InvalidOperationException("Property was not found: " + request.property);
            }

            if (!property.editable)
            {
                throw new InvalidOperationException("Property is not editable: " + request.property);
            }

            var oldValue = UnityCliSceneGameObjectSearcher.ReadInspectorValue(property);
            SetPropertyValue(property, request.value, contextRoot);
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(component);
            if (gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }

            var refreshedObject = new SerializedObject(component);
            var refreshedProperty = ResolveProperty(refreshedObject, request.property);
            var newValue = refreshedProperty != null
                ? UnityCliSceneGameObjectSearcher.ReadInspectorValue(refreshedProperty)
                : request.value;

            return new ModifyData
            {
                assetPath = assetPath ?? string.Empty,
                instanceId = gameObject.GetInstanceID(),
                objectPath = UnityCliSceneGameObjectSearcher.GetHierarchyPath(gameObject),
                component = component.GetType().FullName,
                property = property.propertyPath,
                propertyType = property.propertyType.ToString(),
                oldValue = oldValue,
                newValue = newValue
            };
        }

        /// <summary>
        /// Resolves a SerializedProperty by exact path, normalized path, or display name.
        /// </summary>
        /// <param name="serializedObject">The serialized component object.</param>
        /// <param name="requestedProperty">The property path or display name requested by the CLI.</param>
        /// <returns>The resolved property, or null when no match exists.</returns>
        static SerializedProperty ResolveProperty(SerializedObject serializedObject, string requestedProperty)
        {
            if (string.IsNullOrWhiteSpace(requestedProperty))
            {
                return null;
            }

            var exact = serializedObject.FindProperty(requestedProperty);
            if (exact != null)
            {
                return exact;
            }

            var requested = NormalizePropertyName(requestedProperty);
            var iterator = serializedObject.GetIterator();
            var enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (string.Equals(iterator.propertyPath, "m_Script", StringComparison.Ordinal))
                {
                    continue;
                }

                if (string.Equals(NormalizePropertyName(iterator.propertyPath), requested, StringComparison.Ordinal)
                    || string.Equals(NormalizePropertyName(iterator.displayName), requested, StringComparison.Ordinal))
                {
                    return iterator.Copy();
                }
            }

            return null;
        }

        /// <summary>
        /// Normalizes property names for fuzzy path and display-name matching.
        /// </summary>
        /// <param name="value">The property path or display name to normalize.</param>
        /// <returns>A lowercase alphanumeric key.</returns>
        static string NormalizePropertyName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var normalized = value.Trim();
            if (normalized.StartsWith("m_", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring(2);
            }

            var builder = new System.Text.StringBuilder(normalized.Length);
            for (var index = 0; index < normalized.Length; index++)
            {
                var current = normalized[index];
                if (char.IsLetterOrDigit(current))
                {
                    builder.Append(char.ToLowerInvariant(current));
                }
            }

            return builder.ToString();
        }

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
                if (target is GameObject gameObject)
                {
                    return gameObject;
                }

                if (target is Component component)
                {
                    return component.gameObject;
                }

                throw new InvalidOperationException("GameObject was not found for instance id: " + request.instanceId.ToString(CultureInfo.InvariantCulture));
            }

            if (string.IsNullOrWhiteSpace(request.objectPath))
            {
                throw new InvalidOperationException("Missing target. Provide --id or --path.");
            }

            var result = FindGameObjectByPath(request.objectPath);
            if (result == null)
            {
                throw new InvalidOperationException("GameObject was not found at path: " + request.objectPath);
            }

            return result;
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
                throw new InvalidOperationException("modify with --asset requires --path <prefab hierarchy path>.");
            }

            var result = FindInHierarchy(root, path);
            if (result == null)
            {
                throw new InvalidOperationException("GameObject was not found in prefab " + root.name + ": " + path);
            }

            return result;
        }

        /// <summary>
        /// Resolves a component on the target GameObject by short or full type name.
        /// </summary>
        /// <param name="gameObject">The target scene GameObject.</param>
        /// <param name="componentName">The requested component type name.</param>
        /// <returns>The matching component instance.</returns>
        static Component ResolveComponent(GameObject gameObject, string componentName)
        {
            if (string.IsNullOrWhiteSpace(componentName))
            {
                throw new InvalidOperationException("Missing component type.");
            }

            var components = gameObject.GetComponents<Component>();
            for (var index = 0; index < components.Length; index++)
            {
                var component = components[index];
                if (component == null)
                {
                    continue;
                }

                var type = component.GetType();
                if (string.Equals(type.Name, componentName, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(type.FullName, componentName, StringComparison.OrdinalIgnoreCase))
                {
                    return component;
                }
            }

            throw new InvalidOperationException("Component was not found on " + UnityCliSceneGameObjectSearcher.GetHierarchyPath(gameObject) + ": " + componentName);
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
                if (!scene.isLoaded)
                {
                    continue;
                }

                var roots = scene.GetRootGameObjects();
                for (var rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                {
                    var result = FindInHierarchy(roots[rootIndex], path);
                    if (result != null)
                    {
                        return result;
                    }
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
                var result = FindInHierarchy(transform.GetChild(index).gameObject, path);
                if (result != null)
                {
                    return result;
                }
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
        /// Writes a string value into a supported SerializedProperty type.
        /// </summary>
        /// <param name="property">The property being modified.</param>
        /// <param name="rawValue">The raw CLI value string.</param>
        /// <param name="contextRoot">The prefab root used for object reference resolution, or null for scenes.</param>
        static void SetPropertyValue(SerializedProperty property, string rawValue, GameObject contextRoot)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.Character:
                    property.intValue = int.Parse(rawValue, CultureInfo.InvariantCulture);
                    return;
                case SerializedPropertyType.Boolean:
                    property.boolValue = ParseBool(rawValue);
                    return;
                case SerializedPropertyType.Float:
                    property.floatValue = float.Parse(rawValue, CultureInfo.InvariantCulture);
                    return;
                case SerializedPropertyType.String:
                    property.stringValue = rawValue;
                    return;
                case SerializedPropertyType.Enum:
                    SetEnumValue(property, rawValue);
                    return;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = ParseVector2(rawValue);
                    return;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = ParseVector3(rawValue);
                    return;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = ParseVector4(rawValue);
                    return;
                case SerializedPropertyType.Color:
                    property.colorValue = ParseColor(rawValue);
                    return;
                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = ParseQuaternion(rawValue);
                    return;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = ResolveObjectReference(rawValue, contextRoot, property);
                    return;
                case SerializedPropertyType.Vector2Int:
                    property.vector2IntValue = Vector2Int.RoundToInt(ParseVector2(rawValue));
                    return;
                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue = Vector3Int.RoundToInt(ParseVector3(rawValue));
                    return;
                default:
                    throw new InvalidOperationException("Unsupported property type for modification: " + property.propertyType);
            }
        }

        /// <summary>
        /// Resolves an ObjectReference value from null, instance id, asset path, or @GameObjectPath#ComponentType syntax.
        /// </summary>
        /// <param name="rawValue">The raw CLI value string.</param>
        /// <param name="contextRoot">The prefab root used for hierarchy lookups, or null for scenes.</param>
        /// <returns>The resolved Unity object reference.</returns>
        static UnityEngine.Object ResolveObjectReference(string rawValue, GameObject contextRoot, SerializedProperty property)
        {
            var value = (rawValue ?? string.Empty).Trim();
            if (value.Length == 0
                || string.Equals(value, "null", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "none", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var instanceId))
            {
                var instance = EditorUtility.InstanceIDToObject(instanceId);
                if (instance == null)
                {
                    throw new InvalidOperationException("Object reference instance id was not found: " + value);
                }

                return instance;
            }

            if (value.StartsWith("@", StringComparison.Ordinal))
            {
                return ResolveHierarchyReference(value.Substring(1), contextRoot);
            }

            var assetPath = NormalizeAssetPath(value);
            var asset = LoadAssetForObjectReference(assetPath, property);
            if (asset != null)
            {
                return asset;
            }

            throw new InvalidOperationException(
                "Object reference value was not found. Use null, an instance id, an asset path, or @GameObjectPath#ComponentType: " + rawValue);
        }

        static UnityEngine.Object LoadAssetForObjectReference(string assetPath, SerializedProperty property)
        {
            var propertyType = property == null ? string.Empty : property.type;
            if (propertyType.Contains("Sprite"))
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite != null) return sprite;
            }

            if (propertyType.Contains("Texture2D"))
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (texture != null) return texture;
            }

            if (propertyType.Contains("Material"))
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                if (material != null) return material;
            }

            if (propertyType.Contains("GameObject"))
            {
                var gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (gameObject != null) return gameObject;
            }

            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
        }

        /// <summary>
        /// Resolves @GameObjectPath#ComponentType syntax into a GameObject or Component reference.
        /// </summary>
        /// <param name="specifier">The path and optional component type specifier.</param>
        /// <param name="contextRoot">The prefab root used for hierarchy lookups, or null for scenes.</param>
        /// <returns>The resolved Unity object reference.</returns>
        static UnityEngine.Object ResolveHierarchyReference(string specifier, GameObject contextRoot)
        {
            var splitIndex = specifier.LastIndexOf('#');
            var objectPath = splitIndex >= 0 ? specifier.Substring(0, splitIndex) : specifier;
            var componentName = splitIndex >= 0 ? specifier.Substring(splitIndex + 1) : string.Empty;
            if (string.IsNullOrWhiteSpace(objectPath))
            {
                throw new InvalidOperationException("Object reference hierarchy path is empty: @" + specifier);
            }

            var gameObject = contextRoot != null
                ? ResolveGameObjectInRoot(contextRoot, objectPath)
                : FindGameObjectByPath(objectPath);
            if (gameObject == null)
            {
                throw new InvalidOperationException("Object reference hierarchy path was not found: " + objectPath);
            }

            if (string.IsNullOrWhiteSpace(componentName))
            {
                return gameObject;
            }

            return ResolveComponent(gameObject, componentName);
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
        /// Parses boolean CLI values accepted by the modify command.
        /// </summary>
        /// <param name="value">The raw boolean text.</param>
        /// <returns>The parsed boolean value.</returns>
        static bool ParseBool(string value)
        {
            if (string.Equals(value, "1", StringComparison.Ordinal)
                || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(value, "0", StringComparison.Ordinal)
                || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "no", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            throw new FormatException("Expected boolean value, got: " + value);
        }

        /// <summary>
        /// Sets an enum property by numeric index or display name.
        /// </summary>
        /// <param name="property">The enum SerializedProperty.</param>
        /// <param name="value">The requested enum index or display name.</param>
        static void SetEnumValue(SerializedProperty property, string value)
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var enumIndex))
            {
                property.enumValueIndex = enumIndex;
                return;
            }

            var names = property.enumDisplayNames;
            for (var index = 0; index < names.Length; index++)
            {
                if (string.Equals(names[index], value, StringComparison.OrdinalIgnoreCase))
                {
                    property.enumValueIndex = index;
                    return;
                }
            }

            throw new FormatException("Unknown enum value: " + value);
        }

        /// <summary>
        /// Parses a Vector2 value from comma- or space-separated numbers.
        /// </summary>
        /// <param name="value">The raw vector string.</param>
        /// <returns>The parsed Vector2.</returns>
        static Vector2 ParseVector2(string value)
        {
            var parts = SplitNumbers(value, 2);
            return new Vector2(parts[0], parts[1]);
        }

        /// <summary>
        /// Parses a Vector3 value from comma- or space-separated numbers.
        /// </summary>
        /// <param name="value">The raw vector string.</param>
        /// <returns>The parsed Vector3.</returns>
        static Vector3 ParseVector3(string value)
        {
            var parts = SplitNumbers(value, 3);
            return new Vector3(parts[0], parts[1], parts[2]);
        }

        /// <summary>
        /// Parses a Vector4 value from comma- or space-separated numbers.
        /// </summary>
        /// <param name="value">The raw vector string.</param>
        /// <returns>The parsed Vector4.</returns>
        static Vector4 ParseVector4(string value)
        {
            var parts = SplitNumbers(value, 4);
            return new Vector4(parts[0], parts[1], parts[2], parts[3]);
        }

        /// <summary>
        /// Parses a Quaternion value from four comma- or space-separated numbers.
        /// </summary>
        /// <param name="value">The raw quaternion string.</param>
        /// <returns>The parsed Quaternion.</returns>
        static Quaternion ParseQuaternion(string value)
        {
            var parts = SplitNumbers(value, 4);
            return new Quaternion(parts[0], parts[1], parts[2], parts[3]);
        }

        /// <summary>
        /// Parses an RGB or RGBA color from comma- or space-separated numbers.
        /// </summary>
        /// <param name="value">The raw color string.</param>
        /// <returns>The parsed Color.</returns>
        static Color ParseColor(string value)
        {
            var parts = SplitNumbers(value, 4, true);
            return new Color(parts[0], parts[1], parts[2], parts.Length > 3 ? parts[3] : 1f);
        }

        /// <summary>
        /// Splits and parses numeric tuple input.
        /// </summary>
        /// <param name="value">The raw tuple string.</param>
        /// <param name="expected">The expected number of values.</param>
        /// <param name="allowShort">Whether one missing optional value is allowed.</param>
        /// <returns>The parsed float values.</returns>
        static float[] SplitNumbers(string value, int expected, bool allowShort = false)
        {
            var cleaned = value.Replace("(", string.Empty).Replace(")", string.Empty);
            var tokens = cleaned.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if ((!allowShort && tokens.Length != expected) || (allowShort && (tokens.Length < expected - 1 || tokens.Length > expected)))
            {
                throw new FormatException("Expected " + expected.ToString(CultureInfo.InvariantCulture) + " numeric values, got: " + value);
            }

            var result = new float[tokens.Length];
            for (var index = 0; index < tokens.Length; index++)
            {
                result[index] = float.Parse(tokens[index], CultureInfo.InvariantCulture);
            }

            return result;
        }

    }
}
