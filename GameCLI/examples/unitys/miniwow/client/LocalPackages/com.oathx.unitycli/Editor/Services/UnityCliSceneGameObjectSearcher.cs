using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Searches loaded scenes and serializes matching GameObjects for CLI inspection.
    /// </summary>
    internal static class UnityCliSceneGameObjectSearcher
    {
        /// <summary>
        /// Default number of objects returned when the caller omits a limit.
        /// </summary>
        const int DefaultCount = 50;
        /// <summary>
        /// Hard cap for returned objects to keep pipe responses bounded.
        /// </summary>
        const int MaxCount = 500;
        /// <summary>
        /// Maximum serialized inspector properties per component.
        /// </summary>
        const int MaxInspectorPropertiesPerComponent = 120;
        /// <summary>
        /// Maximum serialized string length for an inspector value.
        /// </summary>
        const int MaxInspectorStringLength = 500;

        /// <summary>
        /// Searches every loaded scene for objects whose name or hierarchy path matches the query.
        /// </summary>
        /// <param name="search">The plain text or wildcard search pattern.</param>
        /// <param name="count">The maximum number of matches to return.</param>
        /// <returns>The serialized search result.</returns>
        public static SearchData Search(string search, int count)
        {
            search = search ?? string.Empty;
            count = count <= 0 ? DefaultCount : Math.Min(count, MaxCount);

            var matched = new List<SearchGameObject>();
            var totalMatched = 0;
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
                    Visit(roots[rootIndex], scene, search, count, matched, ref totalMatched);
                }
            }

            return new SearchData
            {
                totalMatched = totalMatched,
                returnedCount = matched.Count,
                search = search,
                respondedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                objects = matched.ToArray()
            };
        }

        /// <summary>
        /// Recursively visits a hierarchy branch and collects matching objects.
        /// </summary>
        /// <param name="gameObject">The GameObject currently being inspected.</param>
        /// <param name="scene">The scene that owns the current object.</param>
        /// <param name="search">The plain text or wildcard search pattern.</param>
        /// <param name="count">The maximum number of serialized results to keep.</param>
        /// <param name="matched">The collected result list.</param>
        /// <param name="totalMatched">The total number of matches found, including omitted overflow.</param>
        static void Visit(GameObject gameObject, Scene scene, string search, int count, List<SearchGameObject> matched, ref int totalMatched)
        {
            if (MatchesGameObject(gameObject, search))
            {
                totalMatched++;
                if (matched.Count < count)
                {
                    matched.Add(CreateResult(gameObject, scene));
                }
            }

            var transform = gameObject.transform;
            for (var index = 0; index < transform.childCount; index++)
            {
                Visit(transform.GetChild(index).gameObject, scene, search, count, matched, ref totalMatched);
            }
        }

        /// <summary>
        /// Checks whether a GameObject matches the current search query.
        /// </summary>
        /// <param name="gameObject">The GameObject to test.</param>
        /// <param name="search">The plain text or wildcard search pattern.</param>
        /// <returns>True when the object name or hierarchy path matches.</returns>
        static bool MatchesGameObject(GameObject gameObject, string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return true;
            }

            return MatchesText(gameObject.name, search) || MatchesText(GetHierarchyPath(gameObject), search);
        }

        /// <summary>
        /// Creates the serializable search result for a matched GameObject.
        /// </summary>
        /// <param name="gameObject">The matched GameObject.</param>
        /// <param name="scene">The scene that owns the object.</param>
        /// <returns>The serialized GameObject details.</returns>
        static SearchGameObject CreateResult(GameObject gameObject, Scene scene)
        {
            return new SearchGameObject
            {
                name = gameObject.name,
                path = GetHierarchyPath(gameObject),
                sceneName = scene.name,
                scenePath = scene.path,
                instanceId = gameObject.GetInstanceID(),
                tag = gameObject.tag,
                layer = gameObject.layer,
                activeSelf = gameObject.activeSelf,
                activeInHierarchy = gameObject.activeInHierarchy,
                components = GetComponents(gameObject)
            };
        }

        /// <summary>
        /// Serializes the components attached to a GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject whose components should be inspected.</param>
        /// <returns>The serialized component list.</returns>
        static SearchComponent[] GetComponents(GameObject gameObject)
        {
            var components = gameObject.GetComponents<Component>();
            var results = new SearchComponent[components.Length];
            for (var index = 0; index < components.Length; index++)
            {
                var component = components[index];
                if (component == null)
                {
                    results[index] = new SearchComponent
                    {
                        type = "MissingScript",
                        fullName = "MissingScript",
                        assembly = string.Empty,
                        instanceId = 0,
                        properties = new InspectorProperty[0]
                    };
                    continue;
                }

                var type = component.GetType();
                var result = new SearchComponent
                {
                    type = type.Name,
                    fullName = type.FullName,
                    assembly = type.Assembly.GetName().Name,
                    instanceId = component.GetInstanceID(),
                    enabled = false,
                    hasEnabled = false,
                    properties = GetInspectorProperties(component)
                };

                if (component is Transform transform)
                {
                    result.transform = GetTransformDetails(transform);
                }

                if (component is Behaviour behaviour)
                {
                    result.enabled = behaviour.enabled;
                    result.hasEnabled = true;
                }
                else if (component is Renderer renderer)
                {
                    result.enabled = renderer.enabled;
                    result.hasEnabled = true;
                }
                else if (component is Collider collider)
                {
                    result.enabled = collider.enabled;
                    result.hasEnabled = true;
                }

                results[index] = result;
            }

            return results;
        }

        /// <summary>
        /// Serializes transform values in both local and world space.
        /// </summary>
        /// <param name="transform">The Transform component to serialize.</param>
        /// <returns>The serialized transform data.</returns>
        static SearchTransform GetTransformDetails(Transform transform)
        {
            return new SearchTransform
            {
                localPosition = SerializedVector3.From(transform.localPosition),
                localRotation = SerializedQuaternion.From(transform.localRotation),
                localEulerAngles = SerializedVector3.From(transform.localEulerAngles),
                localScale = SerializedVector3.From(transform.localScale),
                position = SerializedVector3.From(transform.position),
                rotation = SerializedQuaternion.From(transform.rotation),
                eulerAngles = SerializedVector3.From(transform.eulerAngles),
                lossyScale = SerializedVector3.From(transform.lossyScale)
            };
        }

        /// <summary>
        /// Reads editable inspector properties from a component.
        /// </summary>
        /// <param name="component">The component to inspect.</param>
        /// <returns>The serialized inspector properties.</returns>
        static InspectorProperty[] GetInspectorProperties(Component component)
        {
            var properties = new List<InspectorProperty>();
            try
            {
                var serializedObject = new SerializedObject(component);
                var iterator = serializedObject.GetIterator();
                var enterChildren = true;
                while (iterator.NextVisible(enterChildren) && properties.Count < MaxInspectorPropertiesPerComponent)
                {
                    enterChildren = false;
                    if (string.Equals(iterator.propertyPath, "m_Script", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (!iterator.editable)
                    {
                        continue;
                    }

                    var property = iterator.Copy();
                    properties.Add(new InspectorProperty
                    {
                        path = property.propertyPath,
                        displayName = property.displayName,
                        type = property.propertyType.ToString(),
                        value = ReadInspectorValue(property),
                        editable = property.editable
                    });
                }
            }
            catch (Exception exception)
            {
                properties.Add(new InspectorProperty
                {
                    path = string.Empty,
                    displayName = "Inspector Read Error",
                    type = "Error",
                    value = exception.Message,
                    editable = false
                });
            }

            return properties.ToArray();
        }

        /// <summary>
        /// Converts a SerializedProperty value into a concise string representation.
        /// </summary>
        /// <param name="property">The serialized property to read.</param>
        /// <returns>A CLI-friendly property value string.</returns>
        public static string ReadInspectorValue(SerializedProperty property)
        {
            try
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        return property.intValue.ToString(CultureInfo.InvariantCulture);
                    case SerializedPropertyType.Boolean:
                        return property.boolValue ? "true" : "false";
                    case SerializedPropertyType.Float:
                        return property.floatValue.ToString("G9", CultureInfo.InvariantCulture);
                    case SerializedPropertyType.String:
                        return Truncate(property.stringValue);
                    case SerializedPropertyType.Color:
                        return property.colorValue.ToString();
                    case SerializedPropertyType.ObjectReference:
                        return FormatObjectReference(property.objectReferenceValue);
                    case SerializedPropertyType.LayerMask:
                        return property.intValue.ToString(CultureInfo.InvariantCulture);
                    case SerializedPropertyType.Enum:
                        return property.enumDisplayNames != null
                            && property.enumValueIndex >= 0
                            && property.enumValueIndex < property.enumDisplayNames.Length
                            ? property.enumDisplayNames[property.enumValueIndex]
                            : property.enumValueIndex.ToString(CultureInfo.InvariantCulture);
                    case SerializedPropertyType.Vector2:
                        return property.vector2Value.ToString();
                    case SerializedPropertyType.Vector3:
                        return property.vector3Value.ToString();
                    case SerializedPropertyType.Vector4:
                        return property.vector4Value.ToString();
                    case SerializedPropertyType.Rect:
                        return property.rectValue.ToString();
                    case SerializedPropertyType.ArraySize:
                        return property.intValue.ToString(CultureInfo.InvariantCulture);
                    case SerializedPropertyType.Character:
                        return ((char)property.intValue).ToString();
                    case SerializedPropertyType.AnimationCurve:
                        return "AnimationCurve(keys: " + (property.animationCurveValue != null ? property.animationCurveValue.length : 0).ToString(CultureInfo.InvariantCulture) + ")";
                    case SerializedPropertyType.Bounds:
                        return property.boundsValue.ToString();
                    case SerializedPropertyType.Quaternion:
                        return property.quaternionValue.ToString();
                    case SerializedPropertyType.Vector2Int:
                        return property.vector2IntValue.ToString();
                    case SerializedPropertyType.Vector3Int:
                        return property.vector3IntValue.ToString();
                    case SerializedPropertyType.RectInt:
                        return property.rectIntValue.ToString();
                    case SerializedPropertyType.BoundsInt:
                        return property.boundsIntValue.ToString();
                    case SerializedPropertyType.ManagedReference:
                        return property.managedReferenceFullTypename;
                    case SerializedPropertyType.Generic:
                        return property.isArray ? "Array(size: " + property.arraySize.ToString(CultureInfo.InvariantCulture) + ")" : string.Empty;
                    default:
                        return property.propertyType.ToString();
                }
            }
            catch (Exception exception)
            {
                return "Unable to read value: " + exception.Message;
            }
        }

        /// <summary>
        /// Formats a Unity object reference for search output.
        /// </summary>
        /// <param name="value">The referenced Unity object.</param>
        /// <returns>The object name, type, and instance id, or "null".</returns>
        static string FormatObjectReference(UnityEngine.Object value)
        {
            if (value == null)
            {
                return "null";
            }

            return value.name + " (" + value.GetType().Name + ", " + value.GetInstanceID().ToString(CultureInfo.InvariantCulture) + ")";
        }

        /// <summary>
        /// Truncates long inspector strings to keep search responses readable.
        /// </summary>
        /// <param name="value">The raw inspector string.</param>
        /// <returns>The original or truncated string.</returns>
        static string Truncate(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= MaxInspectorStringLength)
            {
                return value ?? string.Empty;
            }

            return value.Substring(0, MaxInspectorStringLength) + "...";
        }

        /// <summary>
        /// Builds a slash-separated hierarchy path for a GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject whose path should be built.</param>
        /// <returns>The hierarchy path from scene root to object.</returns>
        public static string GetHierarchyPath(GameObject gameObject)
        {
            var names = new List<string>();
            var current = gameObject.transform;
            while (current != null)
            {
                names.Add(current.name);
                current = current.parent;
            }

            names.Reverse();
            return string.Join("/", names.ToArray());
        }

        /// <summary>
        /// Matches text using either case-insensitive contains or wildcard matching.
        /// </summary>
        /// <param name="value">The text to inspect.</param>
        /// <param name="search">The search pattern.</param>
        /// <returns>True when the text matches the search pattern.</returns>
        static bool MatchesText(string value, string search)
        {
            value = value ?? string.Empty;
            search = search ?? string.Empty;
            if (search.IndexOf('*') < 0 && search.IndexOf('?') < 0)
            {
                return value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return WildcardMatch(value, search);
        }

        /// <summary>
        /// Performs case-insensitive wildcard matching with * and ? tokens.
        /// </summary>
        /// <param name="value">The text to inspect.</param>
        /// <param name="pattern">The wildcard pattern.</param>
        /// <returns>True when the value matches the wildcard pattern.</returns>
        static bool WildcardMatch(string value, string pattern)
        {
            value = value ?? string.Empty;
            pattern = pattern ?? string.Empty;

            var valueIndex = 0;
            var patternIndex = 0;
            var starIndex = -1;
            var matchIndex = 0;

            while (valueIndex < value.Length)
            {
                if (patternIndex < pattern.Length
                    && (pattern[patternIndex] == '?'
                        || char.ToUpperInvariant(pattern[patternIndex]) == char.ToUpperInvariant(value[valueIndex])))
                {
                    patternIndex++;
                    valueIndex++;
                    continue;
                }

                if (patternIndex < pattern.Length && pattern[patternIndex] == '*')
                {
                    starIndex = patternIndex++;
                    matchIndex = valueIndex;
                    continue;
                }

                if (starIndex != -1)
                {
                    patternIndex = starIndex + 1;
                    valueIndex = ++matchIndex;
                    continue;
                }

                return false;
            }

            while (patternIndex < pattern.Length && pattern[patternIndex] == '*')
            {
                patternIndex++;
            }

            return patternIndex == pattern.Length;
        }
    }
}
