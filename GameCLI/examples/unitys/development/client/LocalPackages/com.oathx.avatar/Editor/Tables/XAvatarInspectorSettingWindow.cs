using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Oathx.Avatar.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FilePathAttribute : Attribute
    {
        internal string filepath;

        public FilePathAttribute(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Invalid relative path (it is empty)");
            }

            if (path[0] == '/')
                path = path.Substring(1);
            
            filepath = path;
        }
    }

    public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance;

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

        public static T LoadOrCreate()
        {
            string filePath = GetFilePath();
            if (!string.IsNullOrEmpty(filePath))
            {
                var arr = InternalEditorUtility.LoadSerializedFileAndForget(filePath);
                if (arr.Length != 0)
                {
                    _instance = arr[0] as T;
                }
                else
                {
                    _instance = CreateInstance<T>();
                }
            }
            else
            {
                Debug.LogError($"save location of {nameof(ScriptableSingleton<T>)} is invalid");
            }

            return _instance;
        }

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

                InternalEditorUtility.SaveToSerializedFileAndForget(new T[1] { _instance }, filePath, saveAsText);
            }
        }
        public static string GetFilePath()
        {
            return typeof(T).GetCustomAttributes(inherit: true)
                .Where(v => v is FilePathAttribute)
                .Cast<FilePathAttribute>()
                .FirstOrDefault()
                ?.filepath;
        }
    }

    public class XAvatarInspectorSettingWindow : EditorWindow
    {
        [MenuItem("Avatar/Setting")]
        static void OpenEditor()
        {
            var window = GetWindow<XAvatarInspectorSettingWindow>();
            window.titleContent = new GUIContent("Avatar Setting");
            window.Show();
        }

        private SerializedObject _serializedObject;

        private SerializedProperty _complites;
        private SerializedProperty _boneNogPoints;

        private void OnEnable()
        {
            if (_serializedObject == null)
            {
                InitGUI();
            }
        }

        private void OnDisable()
        {
            XAvatarEditorSetting.Save();
        }

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
                    XAvatarEditorSetting.Save();
                }
            }
        }

        private void InitGUI()
        {
            var setting = XAvatarEditorSetting.LoadOrCreate();
            if (!setting)
                throw new NullReferenceException($"Can't load avatar config asset from path {XAvatarEditorSetting.GetFilePath()}");

            _serializedObject?.Dispose();
            _serializedObject = new SerializedObject(setting);

            _complites = _serializedObject.FindProperty("complites");
            _boneNogPoints = _serializedObject.FindProperty("boneNogPoints");
        }

        private void LayoutPropertyFields()
        {
            EditorGUILayout.PropertyField(_complites);
            EditorGUILayout.PropertyField(_boneNogPoints);
        }
    }
}
