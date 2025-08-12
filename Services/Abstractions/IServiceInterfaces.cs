using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bulk_Editor.Configuration;
using Bulk_Editor.Models;

namespace Bulk_Editor.Services.Abstractions
{
    public interface ILoggingService
    {
        void LogDebug(string message, params object[] args);
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogWarning(Exception ex, string message, params object[] args);
        void LogError(Exception ex, string message, params object[] args);
        void LogUserAction(string action, string description);
        void LogFileOperation(string operation, string filePath, string details);
        void LogProcessingStep(string step, string details = "");
        void LogApiCall(string endpoint, string method, TimeSpan duration, string result = "");
        void LogPerformanceMetric(string metric, double value, string unit = "");
    }

    public interface IProcessingService
    {
        Task<ProcessingResult> ProcessFilesAsync(string path, AppSettings settings, IProgress<ProgressReport> progress);
        Task<string> GenerateChangelogContentAsync(ProcessingResult result, AppSettings settings);
        Task FixSourceHyperlinksWithProgress(string? content, List<HyperlinkData> hyperlinks, WordDocumentProcessor processor, List<string> changes, Collection<string> updatedLinks, Collection<string> notFoundLinks, Collection<string> expiredLinks, Collection<string> errorLinks, Collection<string> updatedUrls, RetryPolicyService retryService, IProgress<ProgressReport> progress, CancellationToken cancellationToken);
        int AppendContentIDToHyperlinks(List<HyperlinkData> hyperlinks, Collection<string> updatedLinks, Dictionary<string, bool>? urlUpdatedTracker = null);
        void FixInternalHyperlink(string? content, List<HyperlinkData> hyperlinks, List<string> changes, Collection<string> internalLinks);
        void DetectTitleChanges(string? content, List<HyperlinkData> hyperlinks, Dictionary<string, ApiResult> apiResults, List<string> changes, Collection<string> titleChangesList);
        void UpdateTitles(string? content, List<HyperlinkData> hyperlinks, Dictionary<string, ApiResult> apiResults, List<string> changes, Collection<string> updatedLinks, Dictionary<string, bool> urlUpdatedTracker);
        void SkipProcessedHyperlinks(string? content, List<HyperlinkData> hyperlinks, List<string> changes);
        void ReplaceHyperlinks(string? content, List<HyperlinkData> hyperlinks, HyperlinkReplacementRules rules, List<string> changes, Collection<string> replacedHyperlinks);
    }
    
    public interface ISettingsService
    {
        AppSettings Settings { get; }
        Task LoadSettingsAsync();
        Task SaveSettingsAsync();
    }

    public interface IThemeService
    {
        void ApplyTheme(Form form);
        void UpdateCheckboxColors(Control container);
        Theme GetCurrentTheme();
    }

    public interface IValidationService
    {
        ProcessingResult ValidateFilePath(string filePath, ProcessingSettings settings);
        ProcessingResult ValidateDirectoryPath(string directoryPath);
        ProcessingResult ValidateUrl(string url);
        ProcessingResult ValidateContentId(string contentId);
        ProcessingResult ValidateHyperlinkReplacementRule(HyperlinkReplacementRule rule);
        ProcessingResult ValidateHyperlinkData(HyperlinkData hyperlink);
        ProcessingResult ValidateBackupDirectory(string basePath, string backupFolderName);
        string SanitizeFilePath(string filePath);
        bool IsSafePath(string basePath, string targetPath);
    }

    public interface ILogViewerService
    {
        Task<List<LogEntry>> GetRecentLogsAsync(int lineCount);
        Task<bool> ExportLogsAsync(string exportPath);
        Task<int> ClearOldLogsAsync(int daysToKeep);
        Task<string> GetChangelogForFileAsync(string changelogPath, string fileName);
    }

    public interface IWindowStateService
    {
        void RestoreWindowState(Form form);
        void SaveWindowState(Form form);
    }
}