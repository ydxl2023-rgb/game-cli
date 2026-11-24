using System;
using System.Globalization;
using UnityEditor;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Controls Unity Editor play mode state for the CLI run command.
    /// </summary>
    internal static class UnityCliEditorRunController
    {
        /// <summary>
        /// Applies a play, pause, resume, or stop action to the Editor.
        /// </summary>
        /// <param name="action">The requested play mode action.</param>
        /// <returns>A snapshot of the resulting Editor play mode state.</returns>
        public static RunData Run(string action)
        {
            action = NormalizeAction(action);
            switch (action)
            {
                case "status":
                    break;
                case "play":
                    EditorApplication.isPaused = false;
                    if (!EditorApplication.isPlaying)
                    {
                        EditorApplication.isPlaying = true;
                    }
                    break;
                case "pause":
                    if (!EditorApplication.isPlaying)
                    {
                        throw new InvalidOperationException("Editor is not in Play Mode.");
                    }

                    EditorApplication.isPaused = true;
                    break;
                case "resume":
                    if (!EditorApplication.isPlaying)
                    {
                        throw new InvalidOperationException("Editor is not in Play Mode.");
                    }

                    EditorApplication.isPaused = false;
                    break;
                case "stop":
                    EditorApplication.isPaused = false;
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.isPlaying = false;
                    }
                    break;
                default:
                    throw new InvalidOperationException("Unsupported run action: " + action + ". Use status, play, pause, resume, or stop.");
            }

            return Snapshot(action);
        }

        /// <summary>
        /// Normalizes aliases and whitespace in a requested run action.
        /// </summary>
        /// <param name="action">The raw action string.</param>
        /// <returns>The canonical action name.</returns>
        static string NormalizeAction(string action)
        {
            if (string.Equals(action, "unpause", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "continue", StringComparison.OrdinalIgnoreCase))
            {
                return "resume";
            }

            return string.IsNullOrWhiteSpace(action)
                ? string.Empty
                : action.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Captures the current Editor play mode state for the response.
        /// </summary>
        /// <param name="action">The action that produced this state.</param>
        /// <returns>The serialized run state.</returns>
        static RunData Snapshot(string action)
        {
            return new RunData
            {
                action = action,
                isPlaying = EditorApplication.isPlaying,
                isPaused = EditorApplication.isPaused,
                isPlayingOrWillChangePlaymode = EditorApplication.isPlayingOrWillChangePlaymode,
                respondedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            };
        }
    }
}
