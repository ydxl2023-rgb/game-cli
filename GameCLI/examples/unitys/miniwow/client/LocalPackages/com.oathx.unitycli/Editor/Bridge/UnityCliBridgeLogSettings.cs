using System.Threading;
using UnityEditor;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Toggle for verbose request/response logging on the bridge.
    ///
    /// Backed by <see cref="EditorPrefs"/> on the main thread and mirrored into a volatile
    /// int so the named-pipe listener thread (background) can read it without touching
    /// EditorPrefs (which is main-thread only).
    /// </summary>
    public static class UnityCliBridgeLogSettings
    {
        /// <summary>
        /// EditorPrefs key that stores the verbose bridge logging flag.
        /// </summary>
        const string EditorPrefsKey = "UnityCLI.Bridge.VerboseLog";

        // -1 = uninitialised, 0 = false, 1 = true. Volatile so background readers always
        // see the latest value written by the Editor UI thread.
        static int _cached = -1;

        /// <summary>
        /// Gets or sets whether bridge request and response summaries are logged.
        /// </summary>
        public static bool VerboseLogging
        {
            get
            {
                var local = Volatile.Read(ref _cached);
                if (local < 0)
                {
                    Prime();
                    local = Volatile.Read(ref _cached);
                }
                return local == 1;
            }
            set
            {
                Volatile.Write(ref _cached, value ? 1 : 0);
                try { EditorPrefs.SetBool(EditorPrefsKey, value); }
                catch { }
            }
        }

        /// <summary>
        /// Loads the cached setting when the editor assembly is loaded.
        /// </summary>
        [InitializeOnLoadMethod]
        static void Prime()
        {
            try
            {
                Volatile.Write(ref _cached, EditorPrefs.GetBool(EditorPrefsKey, true) ? 1 : 0);
            }
            catch
            {
                Volatile.Write(ref _cached, 1);
            }
        }
    }
}
