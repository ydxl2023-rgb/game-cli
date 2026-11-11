using System;
using UnityEditor;
using UnityEngine;

namespace Oathx.Widget.Editor
{
    public class WidgetRoslynAnalyzerLabeler : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                if (assetPath.EndsWith("WidgetGenerator.dll"))
                {
                    ConfigureRoslynAnalyzerLabel(assetPath);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static bool ConfigureRoslynAnalyzerLabel(string assetPath)
        {
            PluginImporter importer = AssetImporter.GetAtPath(assetPath) as PluginImporter;
            if (!importer)
            {
                Debug.LogError($"Could not get PluginImporter for: {assetPath}");
                return false;
            }

            if (importer.GetCompatibleWithEditor())
                return false;

            importer.SetCompatibleWithEditor(false);
            importer.SetCompatibleWithAnyPlatform(false);
            
            var builds = Enum.GetValues(typeof(BuildTarget));
            foreach (BuildTarget target in builds)
            {
                importer.SetCompatibleWithPlatform(target, false);
            }

            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset)
                AssetDatabase.SetLabels(asset, new[] { "RoslynAnalyzer" });
            
            importer.SaveAndReimport();

            return true;
        }
    }
}