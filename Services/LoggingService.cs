using System;
using System.IO;
using Bulk_Editor.Configuration;
using Serilog;
using Serilog.Core;

namespace Bulk_Editor.Services
{
    /// <summary>
    /// Logging service that integrates with LoggingSettings and uses the global Serilog logger
    /// </summary>
    public class LoggingService : IDisposable
    {
        private readonly LoggingSettings _settings;
        private bool _disposed = false;
        private static LoggingService _instance;
        private static readonly object _lock = new object();

        // Use singleton pattern to ensure consistent logging
        public static LoggingService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            // Create with default settings if none provided
                            _instance = new LoggingService(new LoggingSettings());
                        }
                    }
                }
                return _instance;
            }
        }

        public static void Initialize(LoggingSettings settings)
        {
            lock (_lock)
            {
                _instance?.Dispose();
                _instance = new LoggingService(settings);
            }
        }

        private LoggingService(LoggingSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            ConfigureFileLogging();
        }

        private void ConfigureFileLogging()
        {
            // Only configure file logging if enabled and not already configured
            if (_settings.EnableFileLogging)
            {
                var logPath = GetLogFilePath();
                EnsureLogDirectoryExists(logPath);

                // Use the global logger but ensure our file sink is configured
                var additionalConfig = new LoggerConfiguration()
                    .WriteTo.Logger(Log.Logger) // Chain to existing logger
                    .WriteTo.File(
                        path: logPath,
                        outputTemplate: GetOutputTemplate(),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: _settings.MaxLogFiles,
                        fileSizeLimitBytes: _settings.MaxLogFileSizeMB * 1024 * 1024,
                        rollOnFileSizeLimit: true,
                        shared: true);

                // Only replace if we're not already using a properly configured logger
                if (!IsLoggerConfiguredForFiles())
                {
                    Log.Logger = additionalConfig.CreateLogger();
                }
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

        public void LogUserAction(string action, string details = "")
        {
            if (_settings.LogUserActions)
            {
                Log.Information("User Action: {Action} {Details}", action, details);
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

        public void LogFileOperation(string operation, string filePath, string result = "")
        {
            Log.Information("File {Operation}: {FilePath} - {Result}", operation, filePath, result);
        }

        public void LogApiCall(string endpoint, string method, TimeSpan duration, string result = "")
        {
            Log.Information("API {Method} {Endpoint} completed in {Duration}ms - {Result}",
                method, endpoint, duration.TotalMilliseconds, result);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // Don't dispose the global logger, just mark as disposed
                _disposed = true;
            }
        }
    }
}