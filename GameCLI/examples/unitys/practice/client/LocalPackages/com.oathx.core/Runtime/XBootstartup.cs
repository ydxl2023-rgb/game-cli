using UnityEngine;
using UnityEngine.UI;

namespace Oathx.Core
{
    /// <summary>
    /// Launch screen singleton controlled by the application startup flow.
    /// </summary>
    public class XBootstartup : XMonoSingleton<XBootstartup>
    {
        /// <summary>
        /// The version text.
        /// </summary>
        [SerializeField]
        private Text version;

        /// <summary>
        /// The progress bar.
        /// </summary>
        [SerializeField]
        private Image progressBar;

        /// <summary>
        /// The progress image.
        /// </summary>
        [SerializeField]
        private Image progressImg;

        /// <summary>
        /// The message text.
        /// </summary>
        [SerializeField]
        private Text messageText;

        /// <summary>
        /// Awake this instance.
        /// </summary>
        private void Awake()
        {
            instance = this;
        }

        /// <summary>
        /// Sets the version.
        /// </summary>
        /// <param name="versionCode"></param>
        public void SetVersion(string versionCode)
        {
            if (version)
                version.text = versionCode;
        }

        /// <summary>
        /// Updates the progress.
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="text"></param>
        public void UpdateProgress(float progress, string text)
        {
            if (progressBar)
                progressBar.fillAmount = progress;

            if (messageText)
                messageText.text = text;
        }

        /// <summary>
        /// Shows or hides the progress UI.
        /// </summary>
        /// <param name="show"></param>
        public void ShowProgress(bool show)
        {
            if (progressImg)
            {
                progressImg.gameObject.SetActive(show);
            }
            else if (progressBar && progressBar.transform.parent)
            {
                progressBar.transform.parent.gameObject.SetActive(show);
            }

            if (progressBar)
                progressBar.gameObject.SetActive(show);

            if (messageText)
                messageText.gameObject.SetActive(show);
        }

        /// <summary>
        /// Shows or hides the bootstartup UI.
        /// </summary>
        /// <param name="show"></param>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hides the bootstartup UI.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
