using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Oathx.Core
{
    /// <summary>
    /// Controls the Loading and Update panels in Assets/Data/Widgets/Launch/Launch.
    /// Available before the game plugin and UISystem are initialized.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class XLauncher : UIBehaviour
    {
        /// <summary>
        /// Root of the regular loading panel.
        /// </summary>
        [SerializeField]
        private GameObject loading;

        /// <summary>
        /// Root of the resource download panel.
        /// </summary>
        [SerializeField]
        private GameObject update;

        /// <summary>
        /// Status message displayed during regular loading.
        /// </summary>
        [SerializeField]
        private Text loadingText;

        /// <summary>
        /// Progress percentage displayed in the loading panel.
        /// </summary>
        [SerializeField]
        private Text loadingPercent;

        /// <summary>
        /// Additional tip displayed in the loading panel.
        /// </summary>
        [SerializeField]
        private Text tip;

        /// <summary>
        /// Status message displayed during resource downloads.
        /// </summary>
        [SerializeField]
        private Text downloadText;

        /// <summary>
        /// Progress percentage displayed in the download panel.
        /// </summary>
        [SerializeField]
        private Text downloadPercent;

        /// <summary>
        /// RawImage rectangle stretched to represent download progress.
        /// </summary>
        [SerializeField]
        private RectTransform fill;

        /// <summary>
        /// Download progress indicator positioned along the handle area.
        /// </summary>
        [SerializeField]
        private RectTransform handle;

        /// <summary>Optional progress fill for the portrait loading panel.</summary>
        [SerializeField]
        private RectTransform loadingFill;

        /// <summary>Optional progress indicator for the portrait loading panel.</summary>
        [SerializeField]
        private RectTransform loadingHandle;

        /// <summary>
        /// Whether progress updates target the download panel.
        /// </summary>
        private bool downloading;

        /// <summary>
        /// Show the view without resetting its current progress.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide the view while preserving its current state.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Keep the launch background visible without loading or download overlays.
        /// </summary>
        public void ShowBackground()
        {
            loading.SetActive(false);
            update.SetActive(false);

            Show();
        }

        /// <summary>
        /// Display the regular loading panel.
        /// </summary>
        /// <param name="message">Initial status message; progress is reset to zero.</param>
        public void ShowLoading(string message = "Loading...")
        {
            downloading = false;

            loading.SetActive(true);
            update.SetActive(false);

            UpdateProgress(0f, message);
        }

        /// <summary>
        /// Display the resource download panel.
        /// </summary>
        /// <param name="message">Initial download message; progress is reset to zero.</param>
        public void ShowUpdate(string message = "Downloading resources...")
        {
            downloading = true;
            
            loading.SetActive(false);
            update.SetActive(true);

            UpdateProgress(0f, message);
        }

        /// <summary>
        /// Set the additional loading tip.
        /// </summary>
        /// <param name="text">Tip text to display.</param>
        public void SetTip(string text)
        {
            tip.text = text;
        }

        /// <summary>
        /// Update the active panel. The prefab uses a RawImage fill, not an Image or Slider.
        /// </summary>
        /// <param name="progress">Normalized progress, clamped to zero through one; NaN becomes zero.</param>
        /// <param name="text">Status message for the active panel.</param>
        /// <returns>True when the active panel has been updated.</returns>
        public bool UpdateProgress(float progress, string text)
        {
            progress = float.IsNaN(progress) ? 0f : Mathf.Clamp01(progress);
            int percent = Mathf.RoundToInt(progress * 100f);
            if (!downloading)
            {
                // Current templates combine the message and percentage in DownloadTips.
                // Retain the separate label when an older prefab still supplies one.
                loadingText.text = loadingPercent ? text : $"{text} ({percent}%)";
                if (loadingPercent) loadingPercent.text = $"({percent}%)";

                if (loadingFill && loadingHandle)
                {
                    ApplyProgress(loadingFill, loadingHandle, progress);
                }

                return true;
            }

            downloadText.text = downloadPercent ? text : $"{text} {percent}%";
            if (downloadPercent) downloadPercent.text = $"{percent}%";
            ApplyProgress(fill, handle, progress);

            return true;
        }

        /// <summary>Applies horizontal progress without changing vertical layout.</summary>
        /// <param name="progressFill">The authored fill rectangle.</param>
        /// <param name="progressHandle">The authored progress marker.</param>
        /// <param name="progress">Normalized progress supplied by the caller.</param>
        private static void ApplyProgress(RectTransform progressFill, RectTransform progressHandle, float progress)
        {
            progressFill.anchorMin = new Vector2(0f, progressFill.anchorMin.y);
            progressFill.anchorMax = new Vector2(progress, progressFill.anchorMax.y);
            progressFill.offsetMin = new Vector2(0f, progressFill.offsetMin.y);
            progressFill.offsetMax = new Vector2(0f, progressFill.offsetMax.y);
            progressHandle.anchorMin = new Vector2(progress, progressHandle.anchorMin.y);
            progressHandle.anchorMax = new Vector2(progress, progressHandle.anchorMax.y);
            progressHandle.anchoredPosition = new Vector2(0f, progressHandle.anchoredPosition.y);
        }
    }
}
