using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using UnityEngine;

namespace Oathx.Widget.Editor
{
    public enum WidgetMode
    {
        Inject,
        SourceGenerate
    }

    [WidgetFilePath("ProjectSettings/WidgetSettings.asset")]
    public class WidgetSettings : WidgetScriptableSingleton<WidgetSettings>
    {
        public string[] injectAssemblyNames;
        public bool outputDebugLog;
        public WidgetMode widgetMode;
        public AssemblyDefinitionAsset[] assemblyDefinitionAssets;
     
        public WidgetSettings()
        {
            injectAssemblyNames = new string[] { "Assembly-CSharp", "Oathx.Widget" };
            outputDebugLog = true;
            widgetMode = WidgetMode.SourceGenerate;
            assemblyDefinitionAssets = new AssemblyDefinitionAsset[] { };
        }
    }
}; 
