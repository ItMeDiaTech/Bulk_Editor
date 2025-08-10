using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bulk_Editor.Configuration
{
    /// <summary>
    /// Application configuration settings
    /// </summary>
    public class AppSettings
    {
        public ApiSettings ApiSettings { get; set; } = new();
        public RetrySettings RetrySettings { get; set; } = new();
        public ApplicationSettings ApplicationSettings { get; set; } = new();

        // Keep existing settings for backward compatibility
        public ProcessingSettings Processing { get; set; } = new();
        public UiSettings UI { get; set; } = new();
        public LoggingSettings Logging { get; set; } = new();

        /// <summary>
        /// Default configuration file path
        /// </summary>
        private static readonly string ConfigPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "appsettings.json"
        );

        /// <summary>
        /// Load configuration from file or create default
        /// </summary>
        public static async Task<AppSettings> LoadAsync()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = await File.ReadAllTextAsync(ConfigPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true
                    });

                    if (settings != null)
                    {
                        settings.ValidateSettings();
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading configuration: {ex.Message}");
            }

            // Return default configuration and save it
            var defaultSettings = new AppSettings();
            defaultSettings.ValidateSettings();
            await defaultSettings.SaveAsync();
            return defaultSettings;
        }

        /// <summary>
        /// Validate configuration settings and apply defaults
        /// </summary>
        private void ValidateSettings()
        {
            // Ensure API settings are valid
            if (string.IsNullOrWhiteSpace(ApiSettings.PowerAutomateFlowUrl))
            {
                ApiSettings.PowerAutomateFlowUrl = "https://prod-00.eastus.logic.azure.com:443/workflows/...";
            }

            // Ensure retry settings are reasonable
            if (RetrySettings.MaxRetryAttempts < 1 || RetrySettings.MaxRetryAttempts > 10)
            {
                RetrySettings.MaxRetryAttempts = 3;
            }

            if (RetrySettings.BaseDelayMs < 100 || RetrySettings.BaseDelayMs > 10000)
            {
                RetrySettings.BaseDelayMs = 1000;
            }

            if (RetrySettings.MaxDelayMs < RetrySettings.BaseDelayMs)
            {
                RetrySettings.MaxDelayMs = RetrySettings.BaseDelayMs * 8;
            }
        }

        /// <summary>
        /// Save configuration to file
        /// </summary>
        public async Task SaveAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(ConfigPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving configuration: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// API-related settings
    /// </summary>
    public class ApiSettings
    {
        public string PowerAutomateFlowUrl { get; set; } = "https://prod-00.eastus.logic.azure.com:443/workflows/...";
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxConcurrentRequests { get; set; } = 5;

        // Keep existing properties for backward compatibility
        public string BaseUrl { get; set; } = "https://prod-00.eastus.logic.azure.com:443/workflows/";
        public int RetryCount { get; set; } = 3;
        public string UserAgent { get; set; } = "Bulk-Editor/2.1";
        public bool ValidateSsl { get; set; } = true;
    }

    /// <summary>
    /// Retry policy settings
    /// </summary>
    public class RetrySettings
    {
        public int MaxRetryAttempts { get; set; } = 3;
        public int BaseDelayMs { get; set; } = 1000;
        public int MaxDelayMs { get; set; } = 8000;
        public bool UseExponentialBackoff { get; set; } = true;
    }

    /// <summary>
    /// General application settings
    /// </summary>
    public class ApplicationSettings
    {
        public int MaxFileBatchSize { get; set; } = 100;
        public bool EnableDetailedLogging { get; set; } = true;
        public bool AutoBackupEnabled { get; set; } = true;
    }

    /// <summary>
    /// Document processing settings
    /// </summary>
    public class ProcessingSettings
    {
        public long MaxFileSizeBytes { get; set; } = 50 * 1024 * 1024; // 50MB
        public int MaxConcurrentFiles { get; set; } = 5;
        public bool CreateBackups { get; set; } = true;
        public string BackupFolderName { get; set; } = "Backup";
        public bool ValidateDocuments { get; set; } = true;
        public string[] AllowedExtensions { get; set; } = { ".docx" };
    }

    /// <summary>
    /// UI-related settings
    /// </summary>
    public class UiSettings
    {
        public bool RememberWindowSize { get; set; } = true;
        public bool ShowProgressDetails { get; set; } = true;
        public bool AutoSelectFirstFile { get; set; } = true;
        public int ChangelogRefreshIntervalMs { get; set; } = 1000;
        public string Theme { get; set; } = "Light";
    }

    /// <summary>
    /// Logging configuration
    /// </summary>
    public class LoggingSettings
    {
        public string LogLevel { get; set; } = "Information";
        public bool EnableFileLogging { get; set; } = true;
        public bool EnableConsoleLogging { get; set; } = true;
        public string LogFilePath { get; set; } = "logs/bulk-editor.log";
        public int MaxLogFileSizeMB { get; set; } = 10;
        public int MaxLogFiles { get; set; } = 5;
    }
}