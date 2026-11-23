using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Imports verified material bytes as one controlled AssetDatabase batch.</summary>
[InitializeOnLoad]
public static class PracticeMaterialImport
{
    /// <summary>Connect the edit-mode import request processor.</summary>
    static PracticeMaterialImport()
    {
        EditorApplication.update += Process;
    }

    /// <summary>Process a locally staged material directory when the editor is idle.</summary>
    private static void Process()
    {
        string request = "Library/practice-material-import.request";
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(request))
            return;

        string source = Path.GetFullPath(File.ReadAllText(request).Trim());
        File.Delete(request);

        try
        {
            string project = Path.GetFullPath(".") + Path.DirectorySeparatorChar;
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
                {
                    string relative = file.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    string target = Path.GetFullPath(Path.Combine(project, relative));
                    if (!target.StartsWith(project, StringComparison.OrdinalIgnoreCase) || !relative.Replace('\\', '/').StartsWith("Assets/", StringComparison.Ordinal))
                        throw new InvalidOperationException("Material path is outside Assets: " + relative);

                    Directory.CreateDirectory(Path.GetDirectoryName(target));
                    File.Copy(file, target, true);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.SaveAssets();
            File.WriteAllText("Library/practice-material-import.result", "success");
        }
        catch (Exception exception)
        {
            File.WriteAllText("Library/practice-material-import.result", exception.ToString());
            Debug.LogException(exception);
        }
    }
}
