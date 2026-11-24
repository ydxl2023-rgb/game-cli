using System;
using System.IO;
using System.Linq;
using UnityEditor;

/// <summary>Generates IDE project files using the installed Unity IDE integration.</summary>
[InitializeOnLoad]
public static class MiniwowProjectGeneration
{
    /// <summary>Explicit one-shot request, stored in Unity temporary data.</summary>
    private const string requestPath = "Library/miniwow-generate-project.request";

    /// <summary>Registers an edit-mode request handler.</summary>
    static MiniwowProjectGeneration()
    {
        EditorApplication.update += Poll;
    }

    /// <summary>Consumes explicit requests after compilation and package refresh finish.</summary>
    private static void Poll()
    {
        if (!File.Exists(requestPath) || EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        File.Delete(requestPath);

        Generate();
    }

    /// <summary>Regenerates the solution without changing the selected external editor.</summary>
    [MenuItem("GameCLI/Project/Generate C# Project")]
    public static void Generate()
    {
        var type = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType("Microsoft.Unity.VisualStudio.Editor.LegacyStyleProjectGeneration", false))
            .FirstOrDefault(candidate => candidate != null);
        if (type == null)
            throw new InvalidOperationException("Install com.unity.ide.visualstudio before generating the C# project.");

        var generator = Activator.CreateInstance(type);
        type.GetMethod("Sync").Invoke(generator, null);

        UnityEngine.Debug.Log("MINIWOW_CSHARP_PROJECT_GENERATED");
    }
}
