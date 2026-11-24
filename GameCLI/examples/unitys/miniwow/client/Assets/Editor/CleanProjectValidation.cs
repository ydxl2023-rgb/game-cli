using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Runs two clean-entry play cycles and records actual startup evidence.</summary>
[InitializeOnLoad]
public static class CleanProjectValidation
{
    /// <summary>Registers callbacks again after a play-mode domain reload.</summary>
    static CleanProjectValidation()
    {
        Application.logMessageReceived += Observe;
        EditorApplication.update += Check;
        EditorApplication.playModeStateChanged += Changed;
    }

    /// <summary>Starts the two-cycle runtime validation.</summary>
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/App.unity");
        SessionState.SetInt("miniwow.validation.cycle", 1);
        SessionState.SetBool("miniwow.validation.ready", false);
        SessionState.SetBool("miniwow.validation.error", false);
        SessionState.SetFloat("miniwow.validation.started", (float)EditorApplication.timeSinceStartup);
        EditorApplication.isPlaying = true;
    }

    /// <summary>Observes framework startup and runtime errors.</summary>
    /// <param name="message">Log message.</param>
    /// <param name="trace">Stack trace.</param>
    /// <param name="type">Severity.</param>
    private static void Observe(string message, string trace, LogType type)
    {
        if (SessionState.GetInt("miniwow.validation.cycle", 0) == 0) return;
        if (trace.Contains("UnityEditor.PackageManager") || message.StartsWith("[Package Manager Window]"))
        {
            File.AppendAllText("../tools/editor-network-diagnostics.txt", message + "\n");
            return;
        }
        if (message == "MINIWOW_CLEAN_ENTRY_READY") SessionState.SetBool("miniwow.validation.ready", true);
        if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
            SessionState.SetBool("miniwow.validation.error", true);
    }

    /// <summary>Checks startup after enough editor frames have executed.</summary>
    private static void Check()
    {
        int cycle = SessionState.GetInt("miniwow.validation.cycle", 0);
        if (cycle == 0) return;
        double elapsed = EditorApplication.timeSinceStartup - SessionState.GetFloat("miniwow.validation.started", 0);
        if (elapsed > 60)
        {
            File.WriteAllText("../tools/runtime-validation.txt", "FAIL: timeout");
            SessionState.SetInt("miniwow.validation.cycle", 0);
            EditorApplication.Exit(1);
            return;
        }
        if (!EditorApplication.isPlaying || elapsed < 10) return;
        bool ready = SessionState.GetBool("miniwow.validation.ready", false);
        bool failed = SessionState.GetBool("miniwow.validation.error", false);
        if (!ready || failed)
        {
            File.WriteAllText("../tools/runtime-validation.txt", "FAIL: ready=" + ready + " errors=" + failed);
            SessionState.SetInt("miniwow.validation.cycle", 0);
            EditorApplication.Exit(1);
            return;
        }
        EditorApplication.isPlaying = false;
    }

    /// <summary>Restarts once, then exits with a recorded success.</summary>
    /// <param name="state">Editor play transition.</param>
    private static void Changed(PlayModeStateChange state)
    {
        int cycle = SessionState.GetInt("miniwow.validation.cycle", 0);
        if (cycle != 0 && state == PlayModeStateChange.EnteredPlayMode)
        {
            SessionState.SetFloat("miniwow.validation.started", (float)EditorApplication.timeSinceStartup);
            return;
        }
        if (cycle == 0 || state != PlayModeStateChange.EnteredEditMode) return;
        if (cycle == 1)
        {
            SessionState.SetInt("miniwow.validation.cycle", 2);
            SessionState.SetBool("miniwow.validation.ready", false);
            SessionState.SetFloat("miniwow.validation.started", (float)EditorApplication.timeSinceStartup);
            EditorApplication.isPlaying = true;
        }
        else
        {
            File.WriteAllText("../tools/runtime-validation.txt", "PASS: two Play Mode cycles; local GamePlugin ready; no runtime errors.");
            SessionState.SetInt("miniwow.validation.cycle", 0);
            EditorApplication.Exit(0);
        }
    }
}
