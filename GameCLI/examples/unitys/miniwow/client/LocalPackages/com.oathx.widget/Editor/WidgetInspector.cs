using System;
using System.Collections.Generic;
using System.Linq;
using Oathx.Widget;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Oathx.Widget.Editor
{
    [CustomEditor(typeof(IWidget), true)]
    public class WidgetInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (target is IWidget widget)
            {
                var setting = widget.setting;
                if (setting != null)
                {
                    if (WidgetHelper.DrawFoldout(setting.GetType().Name))
                    {
                        EditorGUILayout.LabelField("Prefab Path", setting.path);
                        EditorGUILayout.EnumFlagsField("Layer", setting.layer);
                        EditorGUILayout.LabelField("Asynchronous loading", setting.async.ToString());
                        EditorGUILayout.LabelField("Destroy on closing", setting.destroy.ToString());
                        EditorGUILayout.LabelField("Cache", setting.cache.ToString());
                        EditorGUILayout.LabelField("Canvas render order", setting.order.ToString());
                        EditorGUILayout.LabelField("Display after activation", setting.show.ToString());

                        DateTime dt = new DateTime(setting.createAt);
                        EditorGUILayout.LabelField("CreateAt", dt.ToLongTimeString());

                        EditorGUILayout.LabelField("LoadTime", setting.loadTime.ToString() + "ms");
                    }
                }
            }
        }
    }
}