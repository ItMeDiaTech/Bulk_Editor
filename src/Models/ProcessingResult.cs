using System;
using System.Collections.Generic;

namespace Bulk_Editor.Models
{
    /// <summary>
    /// Represents the result of a processing operation with success/failure status and data
    /// </summary>
    public class ProcessingResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
        public List<string> Warnings { get; set; } = [];
        public Dictionary<string, object> Metadata { get; set; } = [];

        /// <summary>
        /// Creates a successful result
        /// </summary>
        public static ProcessingResult<T> CreateSuccess(T data, List<string> warnings = null)
        {
            return new ProcessingResult<T>
            {
                Success = true,
                Data = data,
                Warnings = warnings ?? []
            };
        }

        /// <summary>
        /// Creates a failed result
        /// </summary>
        public static ProcessingResult<T> CreateFailure(string errorMessage, Exception exception = null)
        {
            return new ProcessingResult<T>
            {
                Success = false,
                ErrorMessage = errorMessage,
                Exception = exception
            };
        }

        /// <summary>
        /// Creates a failed result from an exception
        /// </summary>
        public static ProcessingResult<T> CreateFailure(Exception exception)
        {
            return new ProcessingResult<T>
            {
                Success = false,
                ErrorMessage = exception.Message,
                Exception = exception
            };
        }
    }

    /// <summary>
    /// Non-generic processing result for operations that don't return data
    /// </summary>
    public class ProcessingResult : ProcessingResult<object>
    {
        /// <summary>
        /// Creates a successful result without data
        /// </summary>
        public static ProcessingResult CreateSuccess(List<string> warnings = null)
        {
            return new ProcessingResult
            {
                Success = true,
                Warnings = warnings ?? []
            };
        }

        /// <summary>
        /// Creates a failed result
        /// </summary>
        public static new ProcessingResult CreateFailure(string errorMessage, Exception exception = null)
        {
            return new ProcessingResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                Exception = exception
            };
        }

        /// <summary>
        /// Creates a failed result from an exception
        /// </summary>
        public static new ProcessingResult CreateFailure(Exception exception)
        {
            return new ProcessingResult
            {
                Success = false,
                ErrorMessage = exception.Message,
                Exception = exception
            };
        }
    }

    /// <summary>
    /// File processing statistics
    /// </summary>
    public class FileProcessingStats
    {
        public int TotalFiles { get; set; }
        public int ProcessedFiles { get; set; }
        public int FailedFiles { get; set; }
        public int SkippedFiles { get; set; }
        public TimeSpan TotalProcessingTime { get; set; }
        public long TotalBytesProcessed { get; set; }
        public List<string> ProcessedFileNames { get; set; } = [];
        public List<string> FailedFileNames { get; set; } = [];
        public List<string> SkippedFileNames { get; set; } = [];

        /// <summary>
        /// Success rate as a percentage
        /// </summary>
        public double SuccessRate => TotalFiles > 0 ? (double)ProcessedFiles / TotalFiles * 100 : 0;

        /// <summary>
        /// Average processing time per file
        /// </summary>
        public TimeSpan AverageProcessingTime => ProcessedFiles > 0
            ? TimeSpan.FromMilliseconds(TotalProcessingTime.TotalMilliseconds / ProcessedFiles)
            : TimeSpan.Zero;
    }
}