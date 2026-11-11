using UnityEngine;

namespace Oathx.Avatar
{
    /// <summary>
    /// Provides optional avatar visibility optimization hooks.
    /// </summary>
    public class XAvatarRenderer : MonoBehaviour
    {
        /// <summary>
        /// Whether invisible avatars should be deactivated by renderer visibility callbacks.
        /// </summary>
        [SerializeField]
        private bool enableVisibilityOptimization = false;

        /// <summary>
        /// Handles the renderer becoming invisible to all cameras.
        /// </summary>
        private void OnBecameInvisible()
        {
            if (!enableVisibilityOptimization)
                return;

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Handles the renderer becoming visible to a camera.
        /// </summary>
        private void OnBecameVisible()
        {
            if (!enableVisibilityOptimization)
                return;

            gameObject.SetActive(true);
        }
    }
}
