using System;
using System.IO;
using Bulk_Editor.Configuration;
using Serilog;
using Serilog.Core;

using Bulk_Editor.Services.Abstractions;

namespace Bulk_Editor.Services
 {
     /// <summary>
     /// Logging service that integrates with LoggingSettings and uses the global Serilog logger
     /// </summary>
     public class LoggingService : ILoggingService
     {
         private readonly LoggingSettings _settings;
 
         public LoggingService(LoggingSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            ConfigureFileLogging();
        }

        private void ConfigureFileLogging()
        {
            // Ensure file logging is properly configured if enabled
            if (_settings.EnableFileLogging)
            {
                var logPath = GetLogFilePath();
                EnsureLogDirectoryExists(logPath);
                
                // The Serilog configuration in Program.cs should handle the main logging
                // This just ensures the directory structure exists
                System.Diagnostics.Debug.WriteLine($"File logging enabled. Log path: {logPath}");
            }
        }

        private bool IsLoggerConfiguredForFiles()
        {
            // Simple check - if log directory exists and has recent files, assume it's configured
            try
            {
                var logPath = GetLogFilePath();
                var logDir = Path.GetDirectoryName(logPath);
                return Directory.Exists(logDir) && Directory.GetFiles(logDir, "*.log").Length > 0;
            }
            catch
            {
                return false;
            }
        }

        private string GetOutputTemplate()
        {
            return _settings.LogFormat.ToLowerInvariant() switch
            {
                "json" => "{\"@t\":\"{Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ}\",\"@l\":\"{Level:u3}\",\"@m\":\"{Message:j}\"{NewLine}",
                _ => "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            };
        }

        private string GetLogFilePath()
        {
            if (Path.IsPathRooted(_settings.LogFilePath))
            {
                return _settings.LogFilePath;
            }

            // Relative path - make it relative to application directory
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings.LogFilePath);
        }

        private static void EnsureLogDirectoryExists(string logPath)
        {
            var directory = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public void LogInformation(string message, params object[] args)
        {
            Log.Information(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            Log.Warning(message, args);
        }

        public void LogWarning(Exception ex, string message, params object[] args)
        {
            Log.Warning(ex, message, args);
        }

        public void LogError(string message, params object[] args)
        {
            Log.Error(message, args);
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
            Log.Error(exception, message, args);
        }

        public void LogDebug(string message, params object[] args)
        {
            if (_settings.EnableDebugMode)
            {
                Log.Debug(message, args);
            }
        }

        public void LogUserAction(string action, string description)
        {
            if (_settings.LogUserActions)
            {
                Log.Information("User Action: {Action} {Details}", action, description);
            }
        }

        public void LogPerformanceMetric(string metric, double value, string unit = "")
        {
            if (_settings.LogPerformanceMetrics)
            {
                Log.Information("Performance: {Metric} = {Value} {Unit}", metric, value, unit);
            }
        }

        public void LogProcessingStep(string step, string details = "")
        {
            Log.Information("Processing: {Step} - {Details}", step, details);
        }

        public void LogFileOperation(string operation, string filePath, string details)
        {
            Log.Information("File {Operation}: {FilePath} - {Result}", operation, filePath, details);
        }

        public void LogApiCall(string endpoint, string method, TimeSpan duration, string result = "")
        {
            Log.Information("API {Method} {Endpoint} completed in {Duration}ms - {Result}",
                method, endpoint, duration.TotalMilliseconds, result);
        }

    }
}