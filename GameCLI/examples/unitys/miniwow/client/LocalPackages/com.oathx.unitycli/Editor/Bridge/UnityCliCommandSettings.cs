using System;
using System.Collections.Generic;
using UnityEditor;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Thread-safe enable/disable flags for bridge routes.
    ///
    /// Backed by <see cref="EditorPrefs"/>; changes from the Editor (main thread) are mirrored
    /// into an in-memory cache that <see cref="UnityCliServer"/> can query from the named-pipe
    /// listener thread (where touching <c>EditorPrefs</c> would be unsafe).
    ///
    /// Keys are command paths exactly as registered in <see cref="UnityCliCommandRegistry"/>
    /// (e.g. <c>/ping</c>). Default for unknown commands is <c>true</c> (enabled).
    /// </summary>
    public static class UnityCliCommandSettings
    {
        /// <summary>
        /// Prefix for per-route EditorPrefs keys.
        /// </summary>
        const string EditorPrefsPrefix = "UnityCLI.Command.";
        /// <summary>
        /// Suffix for per-route EditorPrefs keys.
        /// </summary>
        const string EditorPrefsSuffix = ".Enabled";

        /// <summary>
        /// In-memory route enablement cache read by the bridge listener thread.
        /// </summary>
        static readonly Dictionary<string, bool> Cache =
            new Dictionary<string, bool>(StringComparer.Ordinal);
        /// <summary>
        /// Protects access to the command enablement cache.
        /// </summary>
        static readonly object SyncRoot = new object();

        /// <summary>
        /// Returns whether a bridge route is currently enabled.
        /// </summary>
        /// <param name="commandPath">The route path, including the leading slash.</param>
        /// <returns>True when the route is enabled or has no cached setting.</returns>
        public static bool IsEnabled(string commandPath)
        {
            if (string.IsNullOrEmpty(commandPath)) return true;
            lock (SyncRoot)
            {
                if (Cache.TryGetValue(commandPath, out var cached))
                {
                    return cached;
                }
            }
            return true;
        }

        /// <summary>
        /// Updates a bridge route enablement flag.
        /// </summary>
        /// <param name="commandPath">The route path, including the leading slash.</param>
        /// <param name="enabled">True to enable the route; false to disable it.</param>
        public static void SetEnabled(string commandPath, bool enabled)
        {
            if (string.IsNullOrEmpty(commandPath)) return;

            lock (SyncRoot)
            {
                Cache[commandPath] = enabled;
            }

            try
            {
                EditorPrefs.SetBool(KeyFor(commandPath), enabled);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Copies the current route enablement cache for UI display.
        /// </summary>
        /// <returns>A snapshot of route paths and their enabled state.</returns>
        public static IReadOnlyDictionary<string, bool> Snapshot()
        {
            lock (SyncRoot)
            {
                return new Dictionary<string, bool>(Cache, StringComparer.Ordinal);
            }
        }

        /// <summary>
        /// Reloads route enablement from EditorPrefs into the thread-safe cache.
        /// </summary>
        /// <param name="commandPaths">The registered route paths to load.</param>
        public static void Reload(IEnumerable<string> commandPaths)
        {
            if (commandPaths == null) return;

            lock (SyncRoot)
            {
                Cache.Clear();
                foreach (var path in commandPaths)
                {
                    if (string.IsNullOrEmpty(path)) continue;
                    bool value;
                    try { value = EditorPrefs.GetBool(KeyFor(path), true); }
                    catch { value = true; }
                    Cache[path] = value;
                }
            }
        }

        /// <summary>
        /// Builds the EditorPrefs key for a route path.
        /// </summary>
        /// <param name="commandPath">The route path, with or without a leading slash.</param>
        /// <returns>The EditorPrefs key for the route.</returns>
        static string KeyFor(string commandPath)
        {
            var trimmed = commandPath.StartsWith("/", StringComparison.Ordinal)
                ? commandPath.Substring(1)
                : commandPath;
            return EditorPrefsPrefix + trimmed + EditorPrefsSuffix;
        }

        /// <summary>
        /// Initializes the command enablement cache when the editor assembly loads.
        /// </summary>
        [InitializeOnLoadMethod]
        static void PrimeCacheOnLoad()
        {
            try
            {
                Reload(UnityCliCommandRegistry.RegisteredPaths());
            }
            catch
            {
            }
        }
    }
}
