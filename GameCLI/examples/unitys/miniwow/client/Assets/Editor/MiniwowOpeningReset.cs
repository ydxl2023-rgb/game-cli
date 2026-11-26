using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>Resets opening progress after releasing the current editor play session.</summary>
[InitializeOnLoad]
public static class MiniwowOpeningReset
{
    /// <summary>Preserves the explicit reset request across the play-mode domain reload.</summary>
    private const string pendingKey = "MiniwowOpeningReset.Pending";

    /// <summary>Listens for completion of the editor's normal session teardown.</summary>
    static MiniwowOpeningReset()
    {
        EditorApplication.update += ResumePendingReset;
    }

    /// <summary>Stops the current session before clearing persisted progress and restarting App.</summary>
    [MenuItem("Miniwow/Opening/Reset Local Progress")]
    public static void Restart()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            SessionState.SetBool(pendingKey, true);
            EditorApplication.isPlaying = false;

            return;
        }

        OpenFreshSession();
    }

    /// <summary>Resumes an explicit reset even when domain reload replaced the original event subscription.</summary>
    private static void ResumePendingReset()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling
            || EditorApplication.isUpdating || !SessionState.GetBool(pendingKey, false))
            return;

        SessionState.EraseBool(pendingKey);
        OpenFreshSession();
    }

    /// <summary>Uses the configured App scene to rebuild entities through the normal startup path.</summary>
    private static void OpenFreshSession()
    {
        MiniwowOpeningProbe.ResetLocalSave();

        string json = File.ReadAllText("Assets/Data/Jsons/Excels/SceneConfig.bytes");
        var scenes = JsonConvert.DeserializeObject<Config.SceneConfig>(json);
        EditorSceneManager.OpenScene(scenes.data[Config.Excel.SCENE_APP].Path);
        EditorApplication.EnterPlaymode();
    }
}
