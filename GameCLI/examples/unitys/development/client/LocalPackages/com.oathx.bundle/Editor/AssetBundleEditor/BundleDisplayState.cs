namespace Oathx.Bundle.Editor
{
    /// <summary>
    /// Describes how a Bundle is currently represented in the analysis window.
    /// </summary>
    internal enum BundleDisplayState
    {
        Unloaded,
        Loading,
        Active,
        Cached,
        Resident
    }

    /// <summary>
    /// Resolves Bundle runtime fields into an Editor-only display state.
    /// </summary>
    internal static class BundleDisplayStateResolver
    {
        /// <summary>
        /// Gets the fixed display group used before the selected column sort.
        /// </summary>
        internal static int GetSortPriority(BundleDisplayState state)
        {
            if (state == BundleDisplayState.Loading)
                return 1;

            if (state == BundleDisplayState.Unloaded)
                return 2;

            return 0;
        }

        /// <summary>
        /// Resolves the display state without changing Bundle runtime behavior.
        /// </summary>
        /// <param name="isLoading">Whether the Bundle is currently loading.</param>
        /// <param name="isDone">Whether the Bundle load completed successfully.</param>
        /// <param name="hasBundle">Whether the Unity Bundle object exists.</param>
        /// <param name="refCount">The current active request count.</param>
        /// <param name="autoReleaseTime">The configured automatic release delay.</param>
        /// <returns>The state shown by the Bundle analysis window.</returns>
        internal static BundleDisplayState Resolve(
            bool isLoading,
            bool isDone,
            bool hasBundle,
            int refCount,
            float autoReleaseTime)
        {
            if (isLoading)
                return BundleDisplayState.Loading;

            if (!isDone || !hasBundle)
                return BundleDisplayState.Unloaded;

            if (refCount > 0)
                return BundleDisplayState.Active;

            return autoReleaseTime < 0
                ? BundleDisplayState.Resident
                : BundleDisplayState.Cached;
        }
    }
}
