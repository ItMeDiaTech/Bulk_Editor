namespace Bulk_Editor.Configuration
{
    /// <summary>
    /// Root settings object
    /// </summary>
    public class AppSettings
    {
        public ApiSettings ApiSettings { get; set; } = new();
        public RetrySettings RetrySettings { get; set; } = new();
        public ApplicationSettings ApplicationSettings { get; set; } = new();
        public ChangelogSettings ChangelogSettings { get; set; } = new();
        public ProcessingSettings Processing { get; set; } = new();
        public UiSettings UI { get; set; } = new();
        public LoggingSettings Logging { get; set; } = new();
    }

    /// <summary>
    /// API-related settings
    /// </summary>
    public class ApiSettings
    {
        public string PowerAutomateFlowUrl { get; set; } = "https://prod-00.eastus.logic.azure.com:443/workflows/...";
        public string HyperlinkBaseUrl { get; set; } = "https://thesource.cvshealth.com/nuxeo/thesource/";
        public string HyperlinkViewPath { get; set; } = "!/view?docid=";
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxConcurrentRequests { get; set; } = 5;
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
        public bool RememberWindowPosition { get; set; } = true;
        public bool ConfirmBeforeProcessing { get; set; } = true;
        public string DefaultFileFilter { get; set; } = "*.docx";
        public bool RemoveDocumentFilesOnExit { get; set; } = true;
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
        public string TempFolderPath { get; set; } = "Temp";
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
        public int AutoSaveIntervalSeconds { get; set; } = 300;
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
        public string BaseStoragePath { get; set; } = "Data";
        public bool UseCentralizedStorage { get; set; } = true;
        public bool OrganizeByDate { get; set; } = true;
        public int AutoCleanupDays { get; set; } = 30;
        public bool SeparateIndividualAndCombined { get; set; } = true;
        public bool CentralizeBackups { get; set; } = true;
    }
}