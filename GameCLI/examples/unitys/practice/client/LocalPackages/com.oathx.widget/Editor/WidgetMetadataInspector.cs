using System;
using System.Collections.Generic;
using System.Linq;
using Oathx.Widget;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Oathx.Widget.Editor
{
    
    [CustomEditor(typeof(Metadata))]
    public class WidgetMetadataInspector : UnityEditor.Editor
    {
        /// <summary>
        /// metadata class guid
        /// </summary>
        private SerializedProperty classGuid;

        /// <summary>
        /// binding infos
        /// </summary>
        private SerializedProperty metadatas;
        private SerializedProperty className;

        /// <summary>
        /// 
        /// </summary>
        public enum MetadataWord
        {
            metadatas,
            classGuid,
            className
        }
        
        /// <summary>
        /// sync actions types
        /// </summary>
        private static readonly Dictionary<string, int> syncTypes = new Dictionary<string, int>();
        private static readonly Dictionary<string, string> bindTypes = new Dictionary<string, string>();

        [InitializeOnLoadMethod]
        public static void WidgetStartup()
        {
            UnityEditor.Compilation.CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="compilerMessages"></param>
        public static void OnAssemblyCompilationFinished(string file, UnityEditor.Compilation.CompilerMessage[] compilerMessages)
        {
            var assemblyNames = WidgetSettings.Instance.injectAssemblyNames;
            foreach (var assemblyName in assemblyNames)
            {
                if (file.Contains(assemblyName))
                {
                    TryCollectSyncTypes();
                }
            }
        }

        /// <summary>
        /// Attempt to collect all synchronizers injected into the assembly
        /// </summary>
        [MenuItem("Widget/Force collect sync type")]
        public static void TryCollectSyncTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var assemblyNames = new List<string>();
            assemblyNames.AddRange(WidgetSettings.Instance.injectAssemblyNames);

            var assemblyDefinitionAssets = WidgetSettings.Instance.assemblyDefinitionAssets;
            if (assemblyDefinitionAssets != null)
            {
                foreach (var assemblyDefinitionAsset in assemblyDefinitionAssets)
                {
                    if (assemblyDefinitionAsset != null)
                        assemblyNames.Add(assemblyDefinitionAsset.name);
                }
            }

            foreach (var name in assemblyNames)
            {
                var assembly = assemblies.FirstOrDefault(v => v.GetName().Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                if (assembly != null)
                    CollectSyncTypes(assembly);
            }
        }

        /// <summary>
        /// Attempt to collect synchronizer injected into the assembly
        /// </summary>
        /// <param name="assembly"></param>
        public static void CollectSyncTypes(Assembly assembly)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (typeof(ISyncAction).IsAssignableFrom(type) && !type.Name.Equals(typeof(ISyncAction).Name))
                {
                    if (!syncTypes.ContainsKey(type.Name))
                        syncTypes.Add(type.Name, type.Name.GetHashCode());

                    var attr = type.GetCustomAttribute<BindingAttribute>();
                    if (attr != null)
                    {
                        if (!bindTypes.ContainsKey(type.Name))
                            bindTypes.Add(type.Name, attr.name);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static string GetBindTypeName(string typeName)
        {
            if (bindTypes.TryGetValue(typeName, out var result))
                return result;

            throw new NullReferenceException($"Can't find type {typeName} bind attribute");
        }

        /// <summary>
        /// Get type name
        /// </summary>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        public static string GetTypeName(int hashCode)
        {
            foreach(var sync in syncTypes)
            {
                if (sync.Value == hashCode)
                    return sync.Key;
            }

            return typeof(SyncVisible).Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static int GetTypeCode(string typeName)
        {
            foreach (var sync in syncTypes)
            {
                if (sync.Key == typeName)
                    return sync.Value;
            }

            return typeof(SyncVisible).Name.GetHashCode();
        }

        /// <summary>
        /// cache field info
        /// </summary>
        public void OnEnable()
        {
            metadatas = serializedObject.FindProperty(MetadataWord.metadatas.ToString());
            classGuid = serializedObject.FindProperty(MetadataWord.classGuid.ToString());
            className = serializedObject.FindProperty(MetadataWord.className.ToString());
        }

        /// <summary>
        /// update metadata info
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (syncTypes.Count <= 0 || bindTypes.Count <= 0)
                TryCollectSyncTypes();

            DrawMetadataFields();
        }

        /// <summary>
        /// draw inspector metadata
        /// </summary>
        /// <returns></returns>
        private bool DrawMetadataFields()
        {
            serializedObject.Update();

            bool missingScript = false;
            if (!TryGetMonoScript(out var monoScript))
                missingScript = true;

            var scriptObject = EditorGUILayout.ObjectField(typeof(VM).Name, monoScript, typeof(MonoScript), false);
            if (scriptObject != null)
            {
                monoScript = scriptObject as MonoScript;

                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(monoScript, out string guid, out long localId))
                    classGuid.stringValue = guid;

                var monoClass = monoScript.GetClass();
                if (monoClass != null)
                    className.stringValue = monoClass.FullName;

                SyncPropertyFields(monoClass);

                // draw property fields
                DrawPropertyFields();
            }
            else
            {
                if (!missingScript)
                    className.stringValue = classGuid.stringValue = string.Empty;
            }

            if (!monoScript)
            {
                EditorGUILayout.HelpBox($"{target.name} Metadata reference missing. ", MessageType.Error);
                return false;
            }

            serializedObject.ApplyModifiedProperties();
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="monoScript"></param>
        /// <returns></returns>
        private bool TryGetMonoScript(out MonoScript monoScript)
        {
            monoScript = null;
            if (string.IsNullOrEmpty(classGuid.stringValue))
                return true;

            var path = AssetDatabase.GUIDToAssetPath(classGuid.stringValue);
            if (string.IsNullOrEmpty(path))
                return false;

            monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (monoScript == null)
                return false;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public enum PropertyKeyWord
        {
            name,
            type,
            hash,
            components,
            targets,
            reset,
            undefined,
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool DrawPropertyFields()
        {
            if (WidgetHelper.DrawFoldout(typeof(MetadataField).Name))
            {
                for (int index = 0; index < metadatas.arraySize; index++)
                {
                    var element = metadatas.GetArrayElementAtIndex(index);
                    DrawPropertyField(element);
                }

                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("Reset"))
                {
                    int count = metadatas.arraySize - 1;
                    for (int index = count; index > 0; index--)
                    {
                        metadatas.DeleteArrayElementAtIndex(index);
                    }

                    metadatas.arraySize = 0;
                }
                GUI.backgroundColor = Color.white;
            }
            
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        private void DrawPropertyField(SerializedProperty element)
        {
            var propertyName = element.FindPropertyRelative(PropertyKeyWord.name.ToString());
            var propertyType = element.FindPropertyRelative(PropertyKeyWord.type.ToString());
            var propertyHash = element.FindPropertyRelative(PropertyKeyWord.hash.ToString());

            var components = element.FindPropertyRelative(PropertyKeyWord.components.ToString());
            var targets = element.FindPropertyRelative(PropertyKeyWord.targets.ToString());
            if (targets.arraySize <= 0)
            {
                targets.InsertArrayElementAtIndex(targets.arraySize);
                components.InsertArrayElementAtIndex(components.arraySize);
            }

            int emptyIndex = -1;
            for (int j = 0; j < targets.arraySize; j++)
            {
                var targetElement = targets.GetArrayElementAtIndex(j);
                if (!targetElement.objectReferenceValue)
                {
                    emptyIndex = j;
                    break;
                }
            }

            for (int j = 0; j < targets.arraySize; j++)
            {
                GUI.backgroundColor = j == emptyIndex ? Color.red : Color.white;

                var rect = EditorGUILayout.BeginHorizontal();
                var targetElement = targets.GetArrayElementAtIndex(j);

                string propertyTitle = propertyName.stringValue;

                if (EditorApplication.isPlaying)
                {
                    bool showTooltip = rect.Contains(Event.current.mousePosition);
                    if (showTooltip && Event.current.type == EventType.Repaint)
                    {
                        var value = GetPropertyValue(propertyName);
                        if (value != null)
                        {
                            GUI.color = Color.green;
                            propertyTitle = value.ToString();
                        }
                        else
                        {
                            propertyTitle = PropertyKeyWord.undefined.ToString();
                            GUI.color = Color.red;
                        }
                    }
                }

                targetElement.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField(j == 0 ? propertyTitle : " ",
                    targetElement.objectReferenceValue, typeof(GameObject), true);

                GUI.color = Color.white;

                List<string> keys = new List<string>(syncTypes.Keys);
                List<string> displayNames = new List<string>();
                for(int k=0; k<keys.Count; k++)
                {
                    displayNames.Add(keys[k].Substring(4));
                }
                List<int> optionValues = new List<int>(syncTypes.Values);
                
                if (!EditorApplication.isPlaying)
                {
                    propertyHash.intValue = EditorGUILayout.IntPopup(propertyHash.intValue, displayNames.ToArray(), optionValues.ToArray(), GUILayout.Width(100));
                }
                else
                {
                    EditorGUILayout.IntPopup(propertyHash.intValue, displayNames.ToArray(), optionValues.ToArray(), GUILayout.Width(100));
                }

                propertyType.stringValue = GetTypeName(propertyHash.intValue);

                if (targetElement.objectReferenceValue)
                {
                    try
                    {
                        var component = components.GetArrayElementAtIndex(j);
                        UpdateComponent(propertyType.stringValue, targetElement, component);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }

                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    targets.InsertArrayElementAtIndex(targets.arraySize);
                    components.InsertArrayElementAtIndex(components.arraySize);
                    break;
                }

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    targets.DeleteArrayElementAtIndex(j);
                    components.DeleteArrayElementAtIndex(j);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private object GetPropertyValue(SerializedProperty property)
        {
            var metadata = target as Metadata;
            if (!metadata)
                return null;

            if (property == null || string.IsNullOrEmpty(property.stringValue))
                return null;
            
            var widget = metadata.gameObject.GetComponent<IWidget>();
            if (!widget)
                return null;
                
            var vm = widget.GetVM();
            if (vm == null)
                return null;

            var propertyName = property.stringValue;
            var vmType = vm.GetType();
            var prop = FindVmProperty(vmType, propertyName);
            if (prop != null)
            {
                try
                {
                    return prop.GetValue(vm);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            var field = vmType.GetField(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null)
            {
                try
                {
                    return field.GetValue(vm);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return null;
        }

        private static PropertyInfo FindVmProperty(Type vmType, string metadataName)
        {
            if (vmType == null || string.IsNullOrEmpty(metadataName))
                return null;

            var propertyName = ToPropertyName(metadataName);
            return vmType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(v =>
                    string.Equals(v.Name, metadataName, StringComparison.Ordinal) ||
                    string.Equals(v.Name, propertyName, StringComparison.Ordinal) ||
                    string.Equals(v.Name, metadataName, StringComparison.OrdinalIgnoreCase));
        }

        private static string ToPropertyName(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                return fieldName;

            if (fieldName.Length == 1)
                return fieldName.ToUpperInvariant();

            return char.ToUpperInvariant(fieldName[0]) + fieldName.Substring(1);
        }

        /// <summary>
        /// update metadata field component
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="targetProperty"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        private void UpdateComponent(string typeName, SerializedProperty targetProperty, SerializedProperty component)
        {
            GameObject targetGo = (GameObject)targetProperty.objectReferenceValue;
            if (!targetGo)
                throw new NullReferenceException();

            var bindTypeName = GetBindTypeName(typeName);
            if (!string.IsNullOrEmpty(bindTypeName))
            {
                var objectReferenceValue = targetGo.GetComponent(bindTypeName);
                if (!objectReferenceValue)
                    throw new NullReferenceException($"The UI GameObject [{targetGo.name}] Missing bind component {bindTypeName}");
                    
                component.objectReferenceValue = objectReferenceValue;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="classType"></param>
        private void SyncPropertyFields(Type classType)
        {
            var metadata = target as Metadata;
            if (metadata)
            {
                var metadataIndex = 0;
                var metadataBuild = metadata.metadatas.Count == 0;

                //var classPropertys = classType.GetProperties();
                //for (int i = 0; i < classPropertys.Length; i++)
                //{
                //    var p = classPropertys[i];
                //    if (!p.HasAttribute<BindingAttribute>())
                //        continue;

                //    if (!metadataBuild)
                //    {
                //        var field = metadata.metadatas[metadataIndex];
                //        if (!field.name.Equals(p.Name, StringComparison.Ordinal) || metadataIndex != metadata.metadatas.Count)
                //        {
                //            metadataBuild = true;
                //            break;
                //        }
                //    }

                //    metadataIndex++;
                //}

                var classFields = classType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (classFields.Length <= 0) 
                {
                    Debug.LogWarning($"{classType.Name} undefined field");
                }

                for(int i=0;i<classFields.Length; i++)
                {
                    var p = classFields[i];
                    if (!p.HasAttribute<BindingAttribute>())
                        continue;

                    if (!metadataBuild)
                    {
                        if (metadataIndex >= metadata.metadatas.Count)
                        {
                            metadataBuild = true;
                            break;
                        }

                        var field = metadata.metadatas[metadataIndex];
                        if (!field.name.Equals(p.Name, StringComparison.Ordinal))
                        {
                            metadataBuild = true;
                            break;
                        }
                    }

                    metadataIndex++;
                }

                if (!metadataBuild && metadataIndex != metadata.metadatas.Count)
                    metadataBuild = true;

                if (metadataBuild)
                {
                    List<MetadataField> fields = new List<MetadataField>();

                    // update all propertys
                    for (int i = 0; i < classFields.Length; i++)
                    {
                        var p = classFields[i];
                        if (!p.HasAttribute<BindingAttribute>())
                            continue;

                        var field = GeneratePropertyField(p, metadata.metadatas);
                        fields.Add(field);
                    }

                    UpdatePropertyFields(fields);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        private MetadataField GeneratePropertyField(FieldInfo info, List<MetadataField> fields)
        {
            List<GameObject> targets = new List<GameObject>();
            List<Component> components = new List<Component>();
            string type = typeof(SyncVisible).Name;
            int sync = type.GetHashCode();

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];

                if (info.Name.Equals(field.name, StringComparison.Ordinal))
                {
                    type = field.type;
                    sync = field.hash;

                    if (info.HasAttribute<BindingAttribute>())
                    {
                        var bind = info.GetCustomAttribute<BindingAttribute>();
                        if (bind != null && !string.IsNullOrEmpty(bind.name))
                            sync = bind.name.GetHashCode();
                    }
                    
                    targets.AddRange(field.targets);
                    components.AddRange(field.components);
                }
            }

            return new MetadataField()
            {
                hash = sync,
                name = info.Name,
                type = type,
                targets = targets,
                components = components
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fields"></param>
        private void UpdatePropertyFields(List<MetadataField> fields)
        {
            metadatas.arraySize = fields.Count;
            for (int index = 0; index < fields.Count; index++)
            {
                var element = metadatas.GetArrayElementAtIndex(index);
                if (element != null)
                {
                    var propertyName = element.FindPropertyRelative(PropertyKeyWord.name.ToString());
                    propertyName.stringValue = fields[index].name;

                    var propertyType = element.FindPropertyRelative(PropertyKeyWord.type.ToString());
                    propertyType.stringValue = fields[index].type;

                    var propertySync = element.FindPropertyRelative(PropertyKeyWord.hash.ToString());
                    propertySync.intValue = fields[index].hash;

                    var propertyTargets = element.FindPropertyRelative(PropertyKeyWord.targets.ToString());
                    propertyTargets.arraySize = fields[index].targets.Count;
                    for (int j = 0; j < fields[index].targets.Count; j++)
                    {
                        var targetElement = propertyTargets.GetArrayElementAtIndex(j);
                        targetElement.objectReferenceValue = fields[index].targets[j];
                    }

                    var propertyComponents = element.FindPropertyRelative(PropertyKeyWord.components.ToString());
                    propertyComponents.arraySize = fields[index].components.Count;
                    for (int j = 0; j < fields[index].components.Count; j++)
                    {
                        var targetElement = propertyComponents.GetArrayElementAtIndex(j);
                        targetElement.objectReferenceValue = fields[index].components[j];
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class AttributeExtension
    {
        public static bool HasAttribute<T>(this PropertyInfo info) where T : Attribute
        {
            var attrs = info.GetCustomAttributes(true);
            return attrs != null && attrs.Any(a => a.GetType().Equals(typeof(T)));
        }

        public static bool HasAttribute<T>(this FieldInfo info) where T : Attribute
        {
            var attrs = info.GetCustomAttributes(true);
            return attrs != null && attrs.Any(a => a.GetType().Equals(typeof(T)));
        }
    }
}
