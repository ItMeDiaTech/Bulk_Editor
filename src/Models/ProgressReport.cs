using System;

namespace Bulk_Editor.Models
{
    /// <summary>
    /// Enhanced progress report for detailed file processing operations
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
        /// Current processing step within a file (0-based)
        /// </summary>
        public int CurrentStep { get; set; }

        /// <summary>
        /// Total number of processing steps for current file
        /// </summary>
        public int TotalSteps { get; set; }

        /// <summary>
        /// Detailed step description
        /// </summary>
        public string StepDescription { get; set; } = string.Empty;

        /// <summary>
        /// Number of items processed in current step (e.g., hyperlinks)
        /// </summary>
        public int ItemsProcessed { get; set; }

        /// <summary>
        /// Total number of items to process in current step
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Processing phase (e.g., "Validation", "Processing", "Finalizing")
        /// </summary>
        public string ProcessingPhase { get; set; } = string.Empty;

        /// <summary>
        /// Create a file progress report with enhanced details
        /// </summary>
        public static ProgressReport CreateFileProgress(int current, int total, string fileName, string operation = "Processing")
        {
            return new ProgressReport
            {
                CurrentFile = current,
                TotalFiles = total,
                CurrentFileName = fileName,
                CurrentOperation = operation,
                PercentComplete = total > 0 ? current * 100 / total : 0,
                StatusMessage = $"Processing file {current} of {total}: {fileName}",
                ProcessingPhase = "File Processing"
            };
        }

        /// <summary>
        /// Create a detailed step progress report
        /// </summary>
        public static ProgressReport CreateStepProgress(int fileNum, int totalFiles, string fileName,
            int step, int totalSteps, string stepDescription, int itemsProcessed = 0, int totalItems = 0)
        {
            int stepPercent = totalSteps > 0 ? step * 100 / totalSteps : 0;

            // Calculate overall progress: combine completed files + current file's step progress
            int overallPercent;
            if (totalFiles > 0)
            {
                // Progress from completed files + progress within current file
                int completedFilesPercent = totalFiles > 0 ? (fileNum - 1) * 100 / totalFiles : 0;
                int currentFileStepPercent = totalFiles > 0 ? stepPercent / totalFiles : 0;
                overallPercent = completedFilesPercent + currentFileStepPercent;
            }
            else
            {
                overallPercent = stepPercent;
            }

            var statusMessage = $"File {fileNum}/{totalFiles}: {fileName} - Step {step}/{totalSteps}: {stepDescription}";
            if (totalItems > 0)
            {
                statusMessage += $" ({itemsProcessed}/{totalItems} items)";
            }

            return new ProgressReport
            {
                CurrentFile = fileNum,
                TotalFiles = totalFiles,
                CurrentFileName = fileName,
                CurrentStep = step,
                TotalSteps = totalSteps,
                StepDescription = stepDescription,
                ItemsProcessed = itemsProcessed,
                TotalItems = totalItems,
                PercentComplete = Math.Max(0, Math.Min(100, overallPercent)),
                StatusMessage = statusMessage,
                ProcessingPhase = "Processing"
            };
        }

        /// <summary>
        /// Create a phase progress report
        /// </summary>
        public static ProgressReport CreatePhaseProgress(string phase, string description, int percent = -1)
        {
            return new ProgressReport
            {
                ProcessingPhase = phase,
                StatusMessage = description,
                PercentComplete = percent >= 0 ? percent : 0
            };
        }

        /// <summary>
        /// Create an item processing progress report
        /// </summary>
        public static ProgressReport CreateItemProgress(int fileNum, int totalFiles, string fileName,
            string operation, int processed, int total)
        {
            int itemPercent = total > 0 ? processed * 100 / total : 0;

            return new ProgressReport
            {
                CurrentFile = fileNum,
                TotalFiles = totalFiles,
                CurrentFileName = fileName,
                CurrentOperation = operation,
                ItemsProcessed = processed,
                TotalItems = total,
                PercentComplete = itemPercent,
                StatusMessage = $"{operation}: {processed}/{total} items",
                ProcessingPhase = "Item Processing"
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
                StatusMessage = $"Retry {attempt}/{maxAttempts}: {error}",
                ProcessingPhase = "Retry"
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
                PercentComplete = percent >= 0 ? percent : 0,
                ProcessingPhase = "Status Update"
            };
        }

        /// <summary>
        /// Create a validation progress report
        /// </summary>
        public static ProgressReport CreateValidationProgress(int validated, int total, string currentFile = "")
        {
            int percent = total > 0 ? validated * 100 / total : 0;
            var message = string.IsNullOrEmpty(currentFile)
                ? $"Validating files: {validated}/{total}"
                : $"Validating: {currentFile} ({validated}/{total})";

            return new ProgressReport
            {
                ItemsProcessed = validated,
                TotalItems = total,
                PercentComplete = percent,
                StatusMessage = message,
                ProcessingPhase = "Validation",
                CurrentFileName = currentFile
            };
        }

        /// <summary>
        /// Create an API call progress report
        /// </summary>
        public static ProgressReport CreateApiProgress(string operation, int itemCount)
        {
            return new ProgressReport
            {
                CurrentOperation = operation,
                TotalItems = itemCount,
                StatusMessage = $"{operation} - Processing {itemCount} items via API",
                ProcessingPhase = "API Communication"
            };
        }
    }
}