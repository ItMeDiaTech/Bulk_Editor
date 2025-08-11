using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bulk_Editor.Configuration;

namespace Bulk_Editor.Services
{
    /// <summary>
    /// Manages changelog storage, organization, and retrieval
    /// </summary>
    public class ChangelogManager
    {
        private readonly ChangelogSettings _settings;

        public ChangelogManager(ChangelogSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Initialize the changelog directory structure
        /// </summary>
        public async Task InitializeStorageAsync()
        {
            try
            {
                // Create base directory
                Directory.CreateDirectory(_settings.BaseStoragePath);

                // Create changelog directories
                if (_settings.SeparateIndividualAndCombined)
                {
                    Directory.CreateDirectory(_settings.GetIndividualChangelogsPath());
                    Directory.CreateDirectory(_settings.GetCombinedChangelogsPath());
                }
                else
                {
                    Directory.CreateDirectory(Path.Combine(_settings.BaseStoragePath, "Changelogs"));
                }

                // Create backups directory if centralized
                if (_settings.CentralizeBackups)
                {
                    Directory.CreateDirectory(_settings.GetBackupsPath());
                }

                // Create settings directory
                Directory.CreateDirectory(Path.Combine(_settings.BaseStoragePath, "Settings"));

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing changelog storage: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get the path where an individual changelog should be saved
        /// </summary>
        public string GetIndividualChangelogPath(string documentName)
        {
            if (!_settings.UseCentralizedStorage)
            {
                // Legacy behavior - return empty to use document folder
                return string.Empty;
            }

            var basePath = _settings.GetIndividualChangelogsPath();
            var datePath = _settings.GetDateBasedPath(basePath);

            // Ensure directory exists
            Directory.CreateDirectory(datePath);

            var fileName = $"{Path.GetFileNameWithoutExtension(documentName)}_Changelog_{DateTime.Now:MMddyyyy}.txt";
            return Path.Combine(datePath, fileName);
        }

        /// <summary>
        /// Get the path where a combined changelog should be saved
        /// </summary>
        public string GetCombinedChangelogPath()
        {
            if (!_settings.UseCentralizedStorage)
            {
                // Legacy behavior - return empty to use document folder
                return string.Empty;
            }

            var basePath = _settings.GetCombinedChangelogsPath();
            var datePath = _settings.GetDateBasedPath(basePath);

            // Ensure directory exists
            Directory.CreateDirectory(datePath);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"BulkEditor_Changelog_{timestamp}.txt";
            return Path.Combine(datePath, fileName);
        }

        /// <summary>
        /// Find the most recent changelog file (centralized or legacy)
        /// </summary>
        public string FindLatestChangelog(string documentFolderPath = null)
        {
            var allChangelogFiles = new List<string>();

            // First check centralized storage if enabled
            if (_settings.UseCentralizedStorage)
            {
                allChangelogFiles.AddRange(FindCentralizedChangelogs());
            }

            // Also check legacy locations if document folder is provided
            if (!string.IsNullOrEmpty(documentFolderPath))
            {
                allChangelogFiles.AddRange(FindLegacyChangelogs(documentFolderPath));
            }

            if (allChangelogFiles.Count > 0)
            {
                // Sort by file creation time to get the most recent
                allChangelogFiles.Sort((x, y) => File.GetCreationTime(y).CompareTo(File.GetCreationTime(x)));
                return allChangelogFiles[0];
            }

            // Fallback to application directory if nothing found
            string appChangelogPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "BulkEditor_Changelog.txt");

            return File.Exists(appChangelogPath) ? appChangelogPath : string.Empty;
        }

        /// <summary>
        /// Find changelogs in centralized storage
        /// </summary>
        private List<string> FindCentralizedChangelogs()
        {
            var changelogs = new List<string>();

            try
            {
                var changelogsBasePath = Path.Combine(_settings.BaseStoragePath, "Changelogs");
                if (!Directory.Exists(changelogsBasePath))
                    return changelogs;

                // Search in all subdirectories
                var searchPattern = "*_Changelog_*.txt";
                changelogs.AddRange(Directory.GetFiles(changelogsBasePath, searchPattern, SearchOption.AllDirectories));

                // Also search for combined changelogs
                var combinedPattern = "BulkEditor_Changelog_*.txt";
                changelogs.AddRange(Directory.GetFiles(changelogsBasePath, combinedPattern, SearchOption.AllDirectories));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching centralized changelogs: {ex.Message}");
            }

            return changelogs;
        }

        /// <summary>
        /// Find changelogs in legacy document folder locations
        /// </summary>
        private List<string> FindLegacyChangelogs(string documentFolderPath)
        {
            var changelogs = new List<string>();

            try
            {
                bool isFolder = Directory.Exists(documentFolderPath);
                string basePath = isFolder ? documentFolderPath : Path.GetDirectoryName(documentFolderPath);

                if (!Directory.Exists(basePath))
                    return changelogs;

                // Look for both old format and new format changelog files
                var bulkEditorFiles = Directory.GetFiles(basePath, "BulkEditor_Changelog_*.txt");
                changelogs.AddRange(bulkEditorFiles);

                // Look for single document changelog files
                var singleDocFiles = Directory.GetFiles(basePath, "*_Changelog_*.txt")
                    .Where(f => !Path.GetFileName(f).StartsWith("BulkEditor_"));
                changelogs.AddRange(singleDocFiles);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching legacy changelogs: {ex.Message}");
            }

            return changelogs;
        }

        /// <summary>
        /// Get the main changelogs folder path for UI access
        /// </summary>
        public string GetMainChangelogsFolder()
        {
            if (!_settings.UseCentralizedStorage)
            {
                // Return application directory as fallback
                return Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Changelogs");
            }

            return Path.Combine(_settings.BaseStoragePath, "Changelogs");
        }

        /// <summary>
        /// Perform automatic cleanup of old changelogs
        /// </summary>
        public async Task CleanupOldChangelogsAsync()
        {
            if (_settings.AutoCleanupDays <= 0 || !_settings.UseCentralizedStorage)
                return;

            try
            {
                var cutoffDate = DateTime.Now.AddDays(-_settings.AutoCleanupDays);
                var changelogsPath = Path.Combine(_settings.BaseStoragePath, "Changelogs");

                if (!Directory.Exists(changelogsPath))
                    return;

                var oldFiles = Directory.GetFiles(changelogsPath, "*.txt", SearchOption.AllDirectories)
                    .Where(f => File.GetCreationTime(f) < cutoffDate)
                    .ToList();

                foreach (var file in oldFiles)
                {
                    try
                    {
                        File.Delete(file);
                        System.Diagnostics.Debug.WriteLine($"Cleaned up old changelog: {file}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to delete old changelog {file}: {ex.Message}");
                    }
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during changelog cleanup: {ex.Message}");
            }
        }
    }
}