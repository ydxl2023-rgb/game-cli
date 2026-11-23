using System;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;


namespace Oathx.Bundle.Assets
{
    /// <summary>
    /// hotfix downloader
    /// </summary>
    public class XHotfixDownloader
    {
        protected WebClient webCient;
        protected Action<int> progressCallback;
        protected Action<bool> completeCallback;

        /// <summary>
        /// download progress
        /// </summary>
        public int Progress
        { get; internal set; }

        /// <summary>
        /// download finish
        /// </summary>
        public bool Finish
        { get; internal set; }

        /// <summary>
        /// downloading flag
        /// </summary>
        public bool Downloading
        { get; internal set; }

        /// <summary>
        /// exception error
        /// </summary>
        public Exception ExceptionError
        { get; internal set; }

        public int notifyDelta = 5;
        public int lastProgress = 0;

        /// <summary>
        /// construct with progress and complete callback
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="complete"></param>
        public XHotfixDownloader(Action<int> progress, Action<bool> complete = null)
        {
            progressCallback = progress;
            completeCallback = complete;

            Init();
        }

        /// <summary>
        /// construct without callback
        /// </summary>
        public XHotfixDownloader()
        {
            progressCallback = null;
            completeCallback = null;

            Init();
        }

        /// <summary>
        /// initialize
        /// </summary>
        protected void Init()
        {
            webCient = new WebClient();
            webCient.DownloadProgressChanged += WebClientDownloadProgressChanged;
            webCient.DownloadFileCompleted += WebClientDownloadCompleted;

            ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;

            Progress = 0;
            Finish = false;
            Downloading = false;
        }

        /// <summary>
        /// web client download progress changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void WebClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;

            if (progressCallback != null && (Progress - lastProgress) >= notifyDelta)
            {
                progressCallback(e.ProgressPercentage);
                lastProgress = Progress;
            }
        }

        /// <summary>
        /// web client download completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void WebClientDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            ExceptionError = e.Error;
            Finish = ExceptionError == null;
            if (completeCallback != null)
                completeCallback(Finish);

            Downloading = false;
        }

        /// <summary>
        /// remote certificate validation callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        protected bool RemoteCertificateValidationCallback(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;

            // If there are errors in the certificate chain, look at each error to determine the cause.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;

                        bool chainIsValid = chain.Build((X509Certificate2)certificate);
                        if (!chainIsValid)
                        {
                            isOk = false;
                        }
                    }
                }
            }

            return isOk;
        }

        /// <summary>
        /// download file from url to path
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        public void Download(string url, string path)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"[XHotfixDownloader.Download] {url} {path}");
#endif
            ExceptionError = null;
            Progress = 0;
            lastProgress = 0;
            Downloading = true;
            Finish = false;

            try
            {
                webCient.DownloadFileAsync(new Uri(url), path);
            }
            catch (Exception exception)
            {
                ExceptionError = exception;
                Downloading = false;
            }
        }
    }
}
