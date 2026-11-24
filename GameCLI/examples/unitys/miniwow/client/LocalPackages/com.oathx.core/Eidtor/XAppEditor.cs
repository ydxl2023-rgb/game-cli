using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

namespace Oathx.Core.Editor
{
    [CustomEditor(typeof(XApp))]
    public class XAppEditor : UnityEditor.Editor
    {
        const string AppConfDefaultPath = "Assets/Resources";

        /// <summary>
        /// The SerializedProperty for the XAppConfigure reference
        /// </summary>
        private SerializedProperty appConf;
        /// <summary>
        /// The Editor for the XAppConfigure ScriptableObject
        /// </summary>
        private UnityEditor.Editor editor;

        /// <summary>
        /// OnEnable is called when the script is loaded or a value is changed in the Inspector
        /// </summary>
        private void OnEnable()
        {
            appConf = serializedObject.FindProperty("conf");
            if (!appConf.objectReferenceValue)
            {
                serializedObject.Update();

                var target = Resources.Load<XAppConfigure>(typeof(XAppConfigure).Name);
                if (!target)
                {
                    target = CreateInstance<XAppConfigure>();
                    AssetDatabase.CreateAsset(target, 
                        System.IO.Path.Combine(AppConfDefaultPath, typeof(XAppConfigure).Name + ".asset"));
                }

                appConf.objectReferenceValue = target;
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// OnDisable is called when the script is disabled or destroyed
        /// </summary>
        private void OnDisable()
        {
            // build 期间场景会被临时打开/关闭，此时 target 与 serializedObject 可能已被销毁，
            // 直接访问会抛 "SerializedObject target has been destroyed" 并被打包流程计入错误。
            if (target == null || appConf == null || appConf.objectReferenceValue == null)
                return;

            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// OnInspectorGUI is called to draw the Inspector GUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            InspectorAppConfigureGUI();
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draw the Inspector GUI for the XAppConfigure ScriptableObject
        /// </summary>
        private void InspectorAppConfigureGUI()
        {
            if (!editor && appConf != null)
                editor = CreateEditor(appConf.objectReferenceValue);

            if (editor)
                editor.OnInspectorGUI();
        }
    }
}
