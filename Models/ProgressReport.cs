using System;

namespace Bulk_Editor.Models
{
    /// <summary>
    /// Progress report for file processing operations
    /// </summary>
    public class ProgressReport
    {
        /// <summary>
        /// Current file being processed (1-based)
        /// </summary>
        public int CurrentFile { get; set; }

        /// <summary>
        /// Total number of files to process
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// Name of the current file being processed
        /// </summary>
        public string CurrentFileName { get; set; } = string.Empty;

        /// <summary>
        /// Current operation being performed
        /// </summary>
        public string CurrentOperation { get; set; } = string.Empty;

        /// <summary>
        /// Overall progress percentage (0-100)
        /// </summary>
        public int PercentComplete { get; set; }

        /// <summary>
        /// Detailed status message for display
        /// </summary>
        public string StatusMessage { get; set; } = string.Empty;

        /// <summary>
        /// Whether this is a retry notification
        /// </summary>
        public bool IsRetryNotification { get; set; }

        /// <summary>
        /// Current retry attempt (if applicable)
        /// </summary>
        public int RetryAttempt { get; set; }

        /// <summary>
        /// Maximum retry attempts (if applicable)
        /// </summary>
        public int MaxRetryAttempts { get; set; }

        /// <summary>
        /// Create a file progress report
        /// </summary>
        public static ProgressReport CreateFileProgress(int current, int total, string fileName, string operation = "Processing")
        {
            return new ProgressReport
            {
                CurrentFile = current,
                TotalFiles = total,
                CurrentFileName = fileName,
                CurrentOperation = operation,
                PercentComplete = total > 0 ? (current * 100) / total : 0,
                StatusMessage = $"Processing file {current} of {total}: {fileName}"
            };
        }

        /// <summary>
        /// Create a retry notification report
        /// </summary>
        public static ProgressReport CreateRetryNotification(int attempt, int maxAttempts, string error)
        {
            return new ProgressReport
            {
                IsRetryNotification = true,
                RetryAttempt = attempt,
                MaxRetryAttempts = maxAttempts,
                StatusMessage = $"Retry {attempt}/{maxAttempts}: {error}"
            };
        }

        /// <summary>
        /// Create a general status report
        /// </summary>
        public static ProgressReport CreateStatus(string message, int percent = -1)
        {
            return new ProgressReport
            {
                StatusMessage = message,
                PercentComplete = percent >= 0 ? percent : 0
            };
        }
    }
}