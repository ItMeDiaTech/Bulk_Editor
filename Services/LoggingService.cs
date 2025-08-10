using System;
using System.IO;
using Serilog;
using Serilog.Core;
using Bulk_Editor.Configuration;

namespace Bulk_Editor.Services
{
    /// <summary>
    /// Logging service that integrates with LoggingSettings
    /// </summary>
    public class LoggingService : IDisposable
    {
        private Logger _logger;
        private readonly LoggingSettings _settings;
        private bool _disposed = false;

        public LoggingService(LoggingSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            InitializeLogger();
        }

        private void InitializeLogger()
        {
            var loggerConfig = new LoggerConfiguration();

            // Set log level
            var logLevel = _settings.LogLevel.ToLowerInvariant() switch
            {
                "verbose" => Serilog.Events.LogEventLevel.Verbose,
                "debug" => Serilog.Events.LogEventLevel.Debug,
                "information" => Serilog.Events.LogEventLevel.Information,
                "warning" => Serilog.Events.LogEventLevel.Warning,
                "error" => Serilog.Events.LogEventLevel.Error,
                "fatal" => Serilog.Events.LogEventLevel.Fatal,
                _ => Serilog.Events.LogEventLevel.Information
            };

            loggerConfig.MinimumLevel.Is(logLevel);

            // Configure console logging
            if (_settings.EnableConsoleLogging)
            {
                loggerConfig.WriteTo.Console();
            }

            // Configure file logging
            if (_settings.EnableFileLogging)
            {
                var logPath = GetLogFilePath();
                EnsureLogDirectoryExists(logPath);

                var outputTemplate = _settings.LogFormat.ToLowerInvariant() switch
                {
                    "json" => "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    _ => "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                };

                loggerConfig.WriteTo.File(
                    path: logPath,
                    outputTemplate: outputTemplate,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: _settings.MaxLogFiles,
                    fileSizeLimitBytes: _settings.MaxLogFileSizeMB * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    shared: true);
            }

            // Add enrichers for better context
            loggerConfig.Enrich.FromLogContext()
                     .Enrich.WithEnvironmentName()
                     .Enrich.WithProcessId()
                     .Enrich.WithThreadId();

            _logger = loggerConfig.CreateLogger();
            Log.Logger = _logger; // Set global logger
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

        private void EnsureLogDirectoryExists(string logPath)
        {
            var directory = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public void LogInformation(string message, params object[] args)
        {
            _logger?.Information(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger?.Warning(message, args);
        }

        public void LogError(string message, params object[] args)
        {
            _logger?.Error(message, args);
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
            _logger?.Error(exception, message, args);
        }

        public void LogDebug(string message, params object[] args)
        {
            if (_settings.EnableDebugMode)
            {
                _logger?.Debug(message, args);
            }
        }

        public void LogUserAction(string action, string details = "")
        {
            if (_settings.LogUserActions)
            {
                _logger?.Information("User Action: {Action} {Details}", action, details);
            }
        }

        public void LogPerformanceMetric(string metric, double value, string unit = "")
        {
            if (_settings.LogPerformanceMetrics)
            {
                _logger?.Information("Performance: {Metric} = {Value} {Unit}", metric, value, unit);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _logger?.Dispose();
                _disposed = true;
            }
        }
    }
}