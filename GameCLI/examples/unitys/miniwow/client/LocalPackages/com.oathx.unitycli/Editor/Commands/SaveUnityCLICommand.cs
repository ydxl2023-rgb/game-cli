using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// POST /save - equivalent of File &gt; Save (Ctrl+S) in the Editor.
    ///
    /// Default behaviour (no <c>saveScenePath</c>): walk every loaded scene, save the dirty
    /// ones that have an asset path, then call <c>AssetDatabase.SaveAssets()</c> to flush
    /// asset / prefab / material / scriptable-object changes.
    ///
    /// When <c>saveScenePath</c> is supplied: save only the loaded scene whose
    /// <c>scene.path</c> matches (case-insensitive), even if it isn't dirty, then still
    /// call <c>SaveAssets()</c>.
    ///
    /// Untitled scenes (<c>scene.path == ""</c>) are reported with <c>skipReason</c> instead
    /// of failing the whole call - the CLI is non-interactive and can't pick a path for
    /// them, so the user must do "Save As" once in the Editor.
    /// </summary>
    internal sealed class SaveUnityCLICommand : IUnityCLICommand
    {
        /// <summary>
        /// Gets the bridge route path.
        /// </summary>
        public string Path => "/save";

        /// <summary>
        /// Gets the bridge method required by this route.
        /// </summary>
        public string Method => "POST";

        /// <summary>
        /// Executes the route handler asynchronously.
        /// </summary>
        /// <param name="request">The bridge request payload.</param>
        /// <param name="context">Bridge context and editor-thread dispatcher.</param>
        /// <param name="cancellationToken">The token used to cancel command execution.</param>
        /// <returns>A task that completes with a serializable bridge response DTO.</returns>
        public async Task<object> ExecuteAsync(UnityCliBridgeRequest request, UnityCLICommandContext context, CancellationToken cancellationToken)
        {
            try
            {
                var data = await context.RunOnMainThreadAsync(() => DoSave(request));
                return SaveResult.Ok(data);
            }
            catch (Exception exception)
            {
                return PingResult.Fail("save_failed", exception.Message);
            }
        }

        /// <summary>
        /// Saves dirty loaded scenes, an explicitly requested scene, and pending asset changes.
        /// </summary>
        /// <param name="request">The bridge request payload.</param>
        /// <returns>The serialized save result.</returns>
        static SaveData DoSave(UnityCliBridgeRequest request)
        {
            var requestedPath = (request.saveScenePath ?? string.Empty).Trim();
            var entries = new List<SaveSceneEntry>();

            var sceneCount = SceneManager.sceneCount;
            for (var index = 0; index < sceneCount; index++)
            {
                var scene = SceneManager.GetSceneAt(index);
                if (!scene.IsValid())
                {
                    continue;
                }

                if (requestedPath.Length > 0 &&
                    !string.Equals(scene.path, requestedPath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var entry = new SaveSceneEntry
                {
                    name = scene.name,
                    path = scene.path,
                    wasDirty = scene.isDirty,
                    saved = false,
                    skipReason = null
                };

                if (string.IsNullOrEmpty(scene.path))
                {
                    entry.skipReason =
                        "scene has no asset path (untitled); use File > Save As in the Editor once to give it a path.";
                    entries.Add(entry);
                    continue;
                }

                // For batch saves we follow Unity's File > Save semantics - only touch dirty
                // scenes. When the caller named a specific scene we honour it regardless,
                // because they explicitly asked for that file to be (re)written.
                if (!scene.isDirty && requestedPath.Length == 0)
                {
                    entry.skipReason = "not dirty";
                    entries.Add(entry);
                    continue;
                }

                try
                {
                    entry.saved = EditorSceneManager.SaveScene(scene);
                    if (!entry.saved && string.IsNullOrEmpty(entry.skipReason))
                    {
                        entry.skipReason = "EditorSceneManager.SaveScene returned false";
                    }
                }
                catch (Exception exception)
                {
                    entry.saved = false;
                    entry.skipReason = exception.Message;
                }

                entries.Add(entry);
            }

            if (requestedPath.Length > 0 && entries.Count == 0)
            {
                throw new InvalidOperationException(
                    "No loaded scene matched the requested path: " + requestedPath +
                    ". Open the scene in the Editor first, or omit --scene to save all dirty scenes.");
            }

            // Flush asset DB regardless - modify edits to materials/prefabs/SOs land here.
            AssetDatabase.SaveAssets();

            return new SaveData
            {
                scenesSavedCount = entries.Count(entry => entry.saved),
                scenesSkippedCount = entries.Count(entry => !entry.saved),
                assetsSaved = true,
                respondedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                scenes = entries.ToArray()
            };
        }
    }
}
