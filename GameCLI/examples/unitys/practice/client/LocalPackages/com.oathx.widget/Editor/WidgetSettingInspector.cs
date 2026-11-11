using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using UnityEditorInternal;
using System.Linq;

namespace Oathx.Widget.Editor
{
    public class WidgetScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance;

        /// <summary>
        /// 
        /// </summary>
        public static T Instance
        {
            get
            {
                if (!_instance)
                {
                    LoadOrCreate();
                }

                return _instance;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static T LoadOrCreate()
        {
            string filePath = GetFilePath();
            if (!string.IsNullOrEmpty(filePath))
            {
                var arr = InternalEditorUtility.LoadSerializedFileAndForget(filePath);
                _instance = arr.Length > 0 ? arr[0] as T : _instance ?? CreateInstance<T>();
            }
            else
            {
                Debug.LogError($"save location of {nameof(ScriptableSingleton<T>)} is invalid");
            }

            return _instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saveAsText"></param>
        public static void Save(bool saveAsText = true)
        {
            if (!_instance)
            {
                Debug.LogError("Cannot save ScriptableSingleton: no instance!");
                return;
            }

            string filePath = GetFilePath();
            if (!string.IsNullOrEmpty(filePath))
            {
                string directoryName = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                UnityEngine.Object[] obj = new T[1] { _instance };
                InternalEditorUtility.SaveToSerializedFileAndForget(obj, filePath, saveAsText);
            }
        }
        protected static string GetFilePath()
        {
            return typeof(T).GetCustomAttributes(inherit: true)
                .Where(v => v is WidgetFilePathAttribute)
                .Cast<WidgetFilePathAttribute>()
                .FirstOrDefault()
                ?.filepath;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class WidgetFilePathAttribute : Attribute
    {
        internal string filepath;

        public WidgetFilePathAttribute(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Invalid relative path (it is empty)");
            }
            if (path[0] == '/')
            {
                path = path.Substring(1);
            }
            filepath = path;
        }
    }

    public class WidgetSettingInspector : EditorWindow
    {
        private SerializedObject _serializedObject;
        private SerializedProperty _injectAssemblyNames;
        private SerializedProperty _outputDebugLog;
        private SerializedProperty _widgetMode;
        private SerializedProperty _assemblyDefinitionAssets;

        [MenuItem("Widget/Setting")]
        static void OpenEditor()
        {
            var window = GetWindow<WidgetSettingInspector>();
            window.titleContent = new GUIContent("Widget Setting");
            window.Show();
        }

        private void OnEnable()
        {
            if (_serializedObject == null)
                InitGUI();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnGUI()
        {
            if (_serializedObject.targetObject)
            {
                _serializedObject.Update();

                EditorGUI.BeginChangeCheck();
                LayoutPropertyFields();

                if (EditorGUI.EndChangeCheck())
                {
                    _serializedObject.ApplyModifiedProperties();

                    // save rpc configure asset
                    WidgetSettings.Save();
                }
            }
        }

        private void InitGUI()
        {
            var setting = WidgetSettings.LoadOrCreate();
            if (setting.injectAssemblyNames == null)
            {
                setting.injectAssemblyNames = new string[] {
                    "Assembly-CSharp", "Oathx.Widget"
                };
            }

            _serializedObject?.Dispose();

            _serializedObject = new SerializedObject(setting);
            _injectAssemblyNames = _serializedObject.FindProperty("injectAssemblyNames");
            _outputDebugLog = _serializedObject.FindProperty("outputDebugLog");
            _assemblyDefinitionAssets = _serializedObject.FindProperty("assemblyDefinitionAssets");
            _widgetMode = _serializedObject.FindProperty("widgetMode");
        }

        /// <summary>
        /// 
        /// </summary>
        private void LayoutPropertyFields()
        {
            EditorGUILayout.PropertyField(_widgetMode);
            EditorGUILayout.PropertyField(_injectAssemblyNames);
            EditorGUILayout.PropertyField(_assemblyDefinitionAssets);
            EditorGUILayout.PropertyField(_outputDebugLog);
        }
    }
}
