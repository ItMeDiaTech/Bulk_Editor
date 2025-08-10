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
        public ChangelogSettings ChangelogSettings { get; set; } = new();

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

        /// <summary>
        /// Export current settings to a file
        /// </summary>
        public async Task ExportSettingsAsync(string filePath)
        {
            try
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting settings: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Import settings from a file
        /// </summary>
        public static async Task<AppSettings> ImportSettingsAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("Settings file not found");
                }

                var json = await File.ReadAllTextAsync(filePath);
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

                throw new InvalidOperationException("Failed to deserialize settings");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error importing settings: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Reset all settings to defaults
        /// </summary>
        public void ResetToDefaults()
        {
            ApiSettings = new ApiSettings();
            RetrySettings = new RetrySettings();
            ApplicationSettings = new ApplicationSettings();
            ChangelogSettings = new ChangelogSettings();
            Processing = new ProcessingSettings();
            UI = new UiSettings();
            Logging = new LoggingSettings();
            ValidateSettings();
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
        public bool CheckForUpdatesOnStartup { get; set; } = true;
        public bool RememberWindowPosition { get; set; } = true;
        public bool ConfirmBeforeProcessing { get; set; } = true;
        public bool ShowProcessingPreview { get; set; } = true;
        public string DefaultFileFilter { get; set; } = "*.docx";
        public bool AutoSaveSettings { get; set; } = true;
        public int RecentFilesCount { get; set; } = 10;
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
        public bool SkipCorruptedFiles { get; set; } = true;
        public bool PreserveFileAttributes { get; set; } = true;
        public int ProcessingTimeoutMinutes { get; set; } = 30;
        public bool EnableFileComparison { get; set; } = false;
        public bool CreateProcessingReport { get; set; } = true;
        public string TempFolderPath { get; set; } = Path.GetTempPath();
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
        public bool ShowToolTips { get; set; } = true;
        public bool ConfirmOnExit { get; set; } = false;
        public bool MinimizeToTray { get; set; } = false;
        public bool ShowStatusBar { get; set; } = true;
        public bool EnableSounds { get; set; } = true;
        public string Language { get; set; } = "en-US";
        public int AutoSaveIntervalSeconds { get; set; } = 300;
        public bool ShowWelcomeScreen { get; set; } = true;
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
        public bool LogUserActions { get; set; } = false;
        public bool LogPerformanceMetrics { get; set; } = false;
        public bool EnableDebugMode { get; set; } = false;
        public bool IncludeStackTrace { get; set; } = true;
        public string LogFormat { get; set; } = "JSON";
        public bool CompressOldLogs { get; set; } = true;
    }

    /// <summary>
    /// Changelog storage and organization settings
    /// </summary>
    public class ChangelogSettings
    {
        /// <summary>
        /// Base directory for all changelog storage. Defaults to %APPDATA%\BulkEditor
        /// </summary>
        public string BaseStoragePath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BulkEditor");

        /// <summary>
        /// Whether to use centralized storage (true) or document folders (false)
        /// </summary>
        public bool UseCentralizedStorage { get; set; } = true;

        /// <summary>
        /// Whether to organize changelogs by date
        /// </summary>
        public bool OrganizeByDate { get; set; } = true;

        /// <summary>
        /// Number of days to keep changelogs before auto-cleanup (0 = disabled)
        /// </summary>
        public int AutoCleanupDays { get; set; } = 30;

        /// <summary>
        /// Whether to create separate folders for individual vs combined changelogs
        /// </summary>
        public bool SeparateIndividualAndCombined { get; set; } = true;

        /// <summary>
        /// Whether to create backups in the centralized location
        /// </summary>
        public bool CentralizeBackups { get; set; } = false;

        /// <summary>
        /// Get the individual changelogs folder path
        /// </summary>
        public string GetIndividualChangelogsPath()
        {
            var basePath = Path.Combine(BaseStoragePath, "Changelogs");
            return SeparateIndividualAndCombined
                ? Path.Combine(basePath, "Individual")
                : basePath;
        }

        /// <summary>
        /// Get the combined changelogs folder path
        /// </summary>
        public string GetCombinedChangelogsPath()
        {
            var basePath = Path.Combine(BaseStoragePath, "Changelogs");
            return SeparateIndividualAndCombined
                ? Path.Combine(basePath, "Combined")
                : basePath;
        }

        /// <summary>
        /// Get the dated subfolder path for organizing by date
        /// </summary>
        public string GetDateBasedPath(string baseFolder)
        {
            return OrganizeByDate
                ? Path.Combine(baseFolder, DateTime.Now.ToString("MM-dd-yyyy"))
                : baseFolder;
        }

        /// <summary>
        /// Get the centralized backups folder path
        /// </summary>
        public string GetBackupsPath()
        {
            return Path.Combine(BaseStoragePath, "Backups");
        }
    }
}