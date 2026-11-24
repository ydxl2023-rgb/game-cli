using System;
using System.IO;
using System.Net;

namespace Oathx.Bundle.Assets
{
    /// <summary>
    /// Tracks bounded download attempts and shared retry calculations.
    /// </summary>
    internal sealed class XHotfixRetryPolicy
    {
        /// <summary>
        /// Initializes a retry policy with a fixed attempt limit.
        /// </summary>
        /// <param name="maxAttempts">The maximum number of download attempts.</param>
        public XHotfixRetryPolicy(int maxAttempts)
        {
            if (maxAttempts < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxAttempts),
                    "Maximum attempts must be at least one.");
            }

            MaxAttempts = maxAttempts;
        }

        /// <summary>
        /// Gets the maximum number of attempts.
        /// </summary>
        public int MaxAttempts
        { get; }

        /// <summary>
        /// Gets the number of attempts that have started.
        /// </summary>
        public int AttemptCount
        { get; private set; }

        /// <summary>
        /// Gets whether an attempt completed successfully.
        /// </summary>
        public bool Succeeded
        { get; private set; }

        /// <summary>
        /// Gets whether every allowed attempt failed.
        /// </summary>
        public bool IsExhausted
        { get { return !Succeeded && AttemptCount >= MaxAttempts; } }

        /// <summary>
        /// Starts the next attempt when the policy still permits one.
        /// </summary>
        /// <param name="attempt">The one-based attempt number.</param>
        /// <returns>True when a new attempt was started.</returns>
        public bool TryBeginAttempt(out int attempt)
        {
            if (Succeeded || AttemptCount >= MaxAttempts)
            {
                attempt = AttemptCount;
                return false;
            }

            AttemptCount++;
            attempt = AttemptCount;
            return true;
        }

        /// <summary>
        /// Marks the current Entry as successfully downloaded and verified.
        /// </summary>
        public void MarkSucceeded()
        {
            if (AttemptCount < 1)
            {
                throw new InvalidOperationException(
                    "An attempt must start before success can be recorded.");
            }

            Succeeded = true;
        }

        /// <summary>
        /// Calculates the exponential delay after the current failed attempt.
        /// </summary>
        /// <param name="initialDelaySeconds">The first retry delay in seconds.</param>
        /// <returns>The delay for the next retry.</returns>
        public float GetRetryDelaySeconds(float initialDelaySeconds)
        {
            if (initialDelaySeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(initialDelaySeconds),
                    "Retry delay cannot be negative.");
            }

            if (AttemptCount < 1)
            {
                throw new InvalidOperationException(
                    "An attempt must start before a retry delay is calculated.");
            }

            int exponent = Math.Min(AttemptCount - 1, 10);
            return initialDelaySeconds * (1 << exponent);
        }

        /// <summary>
        /// Determines whether an exception represents a retryable download failure.
        /// </summary>
        /// <param name="exception">The downloader exception, if one was provided.</param>
        /// <returns>True when another attempt may recover.</returns>
        public static bool IsRetryable(Exception exception)
        {
            if (exception == null)
            {
                return true;
            }

            if (exception is UriFormatException ||
                exception is ArgumentException ||
                exception is UnauthorizedAccessException ||
                exception is DirectoryNotFoundException ||
                exception is PathTooLongException ||
                exception is NotSupportedException)
            {
                return false;
            }

            if (exception is WebException webException)
            {
                if (webException.InnerException != null &&
                    !IsRetryable(webException.InnerException))
                {
                    return false;
                }

                if (webException.Response is HttpWebResponse response)
                {
                    int statusCode = (int)response.StatusCode;
                    return statusCode == 408 ||
                           statusCode == 429 ||
                           statusCode >= 500;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculates bounded overall progress for the current attempt.
        /// </summary>
        /// <param name="completedBytes">Bytes from checksum-verified entries.</param>
        /// <param name="currentEntryBytes">The size of the Entry being attempted.</param>
        /// <param name="currentPercent">The current attempt percentage.</param>
        /// <param name="totalBytes">The total bytes across all Entries.</param>
        /// <returns>A progress value clamped between zero and one.</returns>
        public static float CalculateProgress(
            long completedBytes,
            long currentEntryBytes,
            int currentPercent,
            long totalBytes)
        {
            if (totalBytes <= 0)
            {
                return 1f;
            }

            long safeCompletedBytes = Math.Max(0L, completedBytes);
            long safeCurrentEntryBytes = Math.Max(0L, currentEntryBytes);
            int safePercent = Math.Max(0, Math.Min(100, currentPercent));
            double currentBytes = safeCurrentEntryBytes * (safePercent / 100.0);
            double progress = (safeCompletedBytes + currentBytes) / totalBytes;

            if (progress <= 0.0)
            {
                return 0f;
            }

            if (progress >= 1.0)
            {
                return 1f;
            }

            return (float)progress;
        }
    }
}
