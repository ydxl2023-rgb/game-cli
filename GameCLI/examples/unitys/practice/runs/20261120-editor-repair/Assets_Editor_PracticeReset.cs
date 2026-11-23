using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Runs the explicitly requested, identity-checked practice reset once.</summary>
[InitializeOnLoad]
public static class PracticeReset
{
    /// <summary>Schedule the guarded reset after script reload.</summary>
    static PracticeReset()
    {
        EditorApplication.update += Execute;
    }

    /// <summary>Reset business assets while retaining packages and project identity.</summary>
    private static void Execute()
    {
        string request = Path.GetFullPath(Path.Combine(Application.dataPath, "../Library/practice-reset.request"));
        if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        string identity = File.ReadAllText(Path.Combine(Application.dataPath, "../ProjectSettings/GameCLIProject.json"));
        if (!identity.Contains("p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484") || !identity.Contains("44f4f783-645c-41e7-80fe-aead8477c978"))
            throw new System.InvalidOperationException("Practice identity mismatch.");

        File.Delete(request);
        PracticeSetup.Build();
        foreach (string path in new[] { "Assets/Scripts/Systems", "Assets/Scripts/Config", "Assets/Data/Widgets" })
        {
            if (AssetDatabase.IsValidFolder(path) && !AssetDatabase.DeleteAsset(path))
                throw new System.InvalidOperationException("Failed to remove " + path);
        }
        AssetDatabase.SaveAssets();
        File.WriteAllText(Path.Combine(Application.dataPath, "../Library/practice-reset.complete"), System.DateTime.UtcNow.ToString("O"));
        Debug.Log("Practice reset complete; compile and runtime acceptance pending.");
    }
}
