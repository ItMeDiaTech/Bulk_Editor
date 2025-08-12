using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bulk_Editor.Configuration;

namespace Bulk_Editor
{
    public partial class SettingsForm : Form
    {
        private readonly AppSettings _settings;
        private bool _hasChanges = false;

        public SettingsForm(AppSettings settings)
        {
            InitializeComponent();
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            LoadSettings();
            SetupEventHandlers();
            
            // Auto-refresh logs when form opens
            _ = Task.Run(async () =>
            {
                await Task.Delay(300); // Delay to ensure form is fully loaded and logging is initialized
                if (InvokeRequired)
                {
                    Invoke(new Action(() => BtnRefreshLogs_Click(null, EventArgs.Empty)));
                }
                else
                {
                    BtnRefreshLogs_Click(null, EventArgs.Empty);
                }
            });
        }

        private void LoadSettings()
        {
            // Changelog Settings
            txtBaseStoragePath.Text = _settings.ChangelogSettings.BaseStoragePath;
            chkUseCentralizedStorage.Checked = _settings.ChangelogSettings.UseCentralizedStorage;
            chkOrganizeByDate.Checked = _settings.ChangelogSettings.OrganizeByDate;
            numAutoCleanupDays.Value = _settings.ChangelogSettings.AutoCleanupDays;
            chkSeparateIndividualAndCombined.Checked = _settings.ChangelogSettings.SeparateIndividualAndCombined;
            chkCentralizeBackups.Checked = _settings.ChangelogSettings.CentralizeBackups;

            // Application Settings
            chkRememberWindowPosition.Checked = _settings.ApplicationSettings.RememberWindowPosition;
            chkConfirmBeforeProcessing.Checked = _settings.ApplicationSettings.ConfirmBeforeProcessing;
            chkRemoveDocumentFilesOnExit.Checked = _settings.ApplicationSettings.RemoveDocumentFilesOnExit;
            numMaxBatchSize.Value = _settings.ApplicationSettings.MaxFileBatchSize;
            numRecentFiles.Value = _settings.ApplicationSettings.RecentFilesCount;

            // Processing Settings
            chkCreateBackups.Checked = _settings.Processing.CreateBackups;
            chkValidateDocuments.Checked = _settings.Processing.ValidateDocuments;
            chkSkipCorrupted.Checked = _settings.Processing.SkipCorruptedFiles;
            chkPreserveAttributes.Checked = _settings.Processing.PreserveFileAttributes;
            numMaxFileSize.Value = _settings.Processing.MaxFileSizeBytes / (1024 * 1024); // Convert bytes to MB
            numConcurrentFiles.Value = _settings.Processing.MaxConcurrentFiles;
            numProcessingTimeout.Value = _settings.Processing.ProcessingTimeoutMinutes * 60; // Convert minutes to seconds

            // Interface Settings
            chkRememberWindowSize.Checked = _settings.UI.RememberWindowSize;
            chkShowProgressDetails.Checked = _settings.UI.ShowProgressDetails;
            chkAutoSelectFirstFile.Checked = _settings.UI.AutoSelectFirstFile;
            chkShowToolTips.Checked = _settings.UI.ShowToolTips;
            chkConfirmOnExit.Checked = _settings.UI.ConfirmOnExit;
            chkShowStatusBar.Checked = _settings.UI.ShowStatusBar;
            cmbTheme.Text = _settings.UI.Theme;

            // Logging Settings
            cmbLogLevel.Text = _settings.Logging.LogLevel;
            chkEnableFileLogging.Checked = _settings.Logging.EnableFileLogging;
            chkLogUserActions.Checked = _settings.Logging.LogUserActions;
            chkLogPerformance.Checked = _settings.Logging.LogPerformanceMetrics;

            // Update dependent controls
            UpdateDependentControls();
        }

        private void SetupEventHandlers()
        {
            // Track changes for Changelog Settings
            txtBaseStoragePath.TextChanged += (s, e) => _hasChanges = true;
            chkUseCentralizedStorage.CheckedChanged += (s, e) => { _hasChanges = true; UpdateDependentControls(); };
            chkOrganizeByDate.CheckedChanged += (s, e) => _hasChanges = true;
            numAutoCleanupDays.ValueChanged += (s, e) => _hasChanges = true;
            chkSeparateIndividualAndCombined.CheckedChanged += (s, e) => _hasChanges = true;
            chkCentralizeBackups.CheckedChanged += (s, e) => _hasChanges = true;

            // Track changes for Application Settings
            chkRememberWindowPosition.CheckedChanged += (s, e) => _hasChanges = true;
            chkConfirmBeforeProcessing.CheckedChanged += (s, e) => _hasChanges = true;
            chkRemoveDocumentFilesOnExit.CheckedChanged += (s, e) => _hasChanges = true;
            numMaxBatchSize.ValueChanged += (s, e) => _hasChanges = true;
            numRecentFiles.ValueChanged += (s, e) => _hasChanges = true;

            // Track changes for Processing Settings
            chkCreateBackups.CheckedChanged += (s, e) => _hasChanges = true;
            chkValidateDocuments.CheckedChanged += (s, e) => _hasChanges = true;
            chkSkipCorrupted.CheckedChanged += (s, e) => _hasChanges = true;
            chkPreserveAttributes.CheckedChanged += (s, e) => _hasChanges = true;
            numMaxFileSize.ValueChanged += (s, e) => _hasChanges = true;
            numConcurrentFiles.ValueChanged += (s, e) => _hasChanges = true;
            numProcessingTimeout.ValueChanged += (s, e) => _hasChanges = true;

            // Track changes for Interface Settings
            chkRememberWindowSize.CheckedChanged += (s, e) => _hasChanges = true;
            chkShowProgressDetails.CheckedChanged += (s, e) => _hasChanges = true;
            chkAutoSelectFirstFile.CheckedChanged += (s, e) => _hasChanges = true;
            chkShowToolTips.CheckedChanged += (s, e) => _hasChanges = true;
            chkConfirmOnExit.CheckedChanged += (s, e) => _hasChanges = true;
            chkShowStatusBar.CheckedChanged += (s, e) => _hasChanges = true;
            cmbTheme.SelectedIndexChanged += (s, e) => _hasChanges = true;
            
            // Logging button event handlers
            btnRefreshLogs.Click += BtnRefreshLogs_Click;
            btnExportLogs.Click += BtnExportLogs_Click;
            btnClearOldLogs.Click += BtnClearOldLogs_Click;
            
            // Auto-refresh logs when the logging tab is selected
            if (this.Controls.Find("tabControl", true).FirstOrDefault() is TabControl tabControl)
            {
                tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            }
        }
        
        private async void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is TabControl tabControl && tabControl.SelectedTab?.Name == "tabLogging")
            {
                // Auto-refresh logs when logging tab is selected
                await Task.Delay(100); // Small delay to ensure tab is fully loaded
                BtnRefreshLogs_Click(null, EventArgs.Empty);
            }
        }

        private void UpdateDependentControls()
        {
            // Enable/disable controls based on centralized storage setting
            bool useCentralized = chkUseCentralizedStorage.Checked;

            txtBaseStoragePath.Enabled = useCentralized;
            btnBrowsePath.Enabled = useCentralized;
            chkOrganizeByDate.Enabled = useCentralized;
            chkSeparateIndividualAndCombined.Enabled = useCentralized;
            chkCentralizeBackups.Enabled = useCentralized;
            numAutoCleanupDays.Enabled = useCentralized;

            // Update labels color
            var enabledColor = System.Drawing.SystemColors.ControlText;
            var disabledColor = System.Drawing.SystemColors.ControlDark;

            lblBaseStoragePath.ForeColor = useCentralized ? enabledColor : disabledColor;
            lblAutoCleanupDays.ForeColor = useCentralized ? enabledColor : disabledColor;
        }

        private void BtnBrowsePath_Click(object sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Select base folder for changelog storage";
            folderDialog.SelectedPath = txtBaseStoragePath.Text;
            folderDialog.ShowNewFolderButton = true;

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                txtBaseStoragePath.Text = folderDialog.SelectedPath;
            }
        }

        private void BtnResetToDefaults_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "This will reset ALL settings to their default values. Are you sure?",
                "Reset to Defaults",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                var defaultAppSettings = new AppSettings();

                // Reset Changelog Settings
                txtBaseStoragePath.Text = defaultAppSettings.ChangelogSettings.BaseStoragePath;
                chkUseCentralizedStorage.Checked = defaultAppSettings.ChangelogSettings.UseCentralizedStorage;
                chkOrganizeByDate.Checked = defaultAppSettings.ChangelogSettings.OrganizeByDate;
                numAutoCleanupDays.Value = defaultAppSettings.ChangelogSettings.AutoCleanupDays;
                chkSeparateIndividualAndCombined.Checked = defaultAppSettings.ChangelogSettings.SeparateIndividualAndCombined;
                chkCentralizeBackups.Checked = defaultAppSettings.ChangelogSettings.CentralizeBackups;

                // Reset Application Settings
                chkRememberWindowPosition.Checked = defaultAppSettings.ApplicationSettings.RememberWindowPosition;
                chkConfirmBeforeProcessing.Checked = defaultAppSettings.ApplicationSettings.ConfirmBeforeProcessing;
                chkRemoveDocumentFilesOnExit.Checked = defaultAppSettings.ApplicationSettings.RemoveDocumentFilesOnExit;
                numMaxBatchSize.Value = defaultAppSettings.ApplicationSettings.MaxFileBatchSize;
                numRecentFiles.Value = defaultAppSettings.ApplicationSettings.RecentFilesCount;

                // Reset Processing Settings
                chkCreateBackups.Checked = defaultAppSettings.Processing.CreateBackups;
                chkValidateDocuments.Checked = defaultAppSettings.Processing.ValidateDocuments;
                chkSkipCorrupted.Checked = defaultAppSettings.Processing.SkipCorruptedFiles;
                chkPreserveAttributes.Checked = defaultAppSettings.Processing.PreserveFileAttributes;
                numMaxFileSize.Value = defaultAppSettings.Processing.MaxFileSizeBytes / (1024 * 1024);
                numConcurrentFiles.Value = defaultAppSettings.Processing.MaxConcurrentFiles;
                numProcessingTimeout.Value = defaultAppSettings.Processing.ProcessingTimeoutMinutes * 60;

                // Reset Interface Settings
                chkRememberWindowSize.Checked = defaultAppSettings.UI.RememberWindowSize;
                chkShowProgressDetails.Checked = defaultAppSettings.UI.ShowProgressDetails;
                chkAutoSelectFirstFile.Checked = defaultAppSettings.UI.AutoSelectFirstFile;
                chkShowToolTips.Checked = defaultAppSettings.UI.ShowToolTips;
                chkConfirmOnExit.Checked = defaultAppSettings.UI.ConfirmOnExit;
                chkShowStatusBar.Checked = defaultAppSettings.UI.ShowStatusBar;
                cmbTheme.Text = defaultAppSettings.UI.Theme;

                _hasChanges = true;
                UpdateDependentControls();
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            string originalStoragePath = _settings.ChangelogSettings.BaseStoragePath;
            string newStoragePath = txtBaseStoragePath.Text.Trim();

            try
            {
                // Handle storage path migration if changed
                if (chkUseCentralizedStorage.Checked &&
                    !string.IsNullOrWhiteSpace(newStoragePath) &&
                    !newStoragePath.Equals(originalStoragePath, StringComparison.OrdinalIgnoreCase))
                {
                    if (!await MigrateStorageDirectory(originalStoragePath, newStoragePath))
                    {
                        // Migration failed, revert the path in UI
                        txtBaseStoragePath.Text = originalStoragePath;
                        return;
                    }
                }
                else if (chkUseCentralizedStorage.Checked && !string.IsNullOrWhiteSpace(newStoragePath))
                {
                    // Just validate/create the path if no migration needed
                    try
                    {
                        if (!Directory.Exists(newStoragePath))
                        {
                            Directory.CreateDirectory(newStoragePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error creating storage path: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // Save Changelog Settings
                _settings.ChangelogSettings.BaseStoragePath = txtBaseStoragePath.Text.Trim();
                _settings.ChangelogSettings.UseCentralizedStorage = chkUseCentralizedStorage.Checked;
                _settings.ChangelogSettings.OrganizeByDate = chkOrganizeByDate.Checked;
                _settings.ChangelogSettings.AutoCleanupDays = (int)numAutoCleanupDays.Value;
                _settings.ChangelogSettings.SeparateIndividualAndCombined = chkSeparateIndividualAndCombined.Checked;
                _settings.ChangelogSettings.CentralizeBackups = chkCentralizeBackups.Checked;

                // Save Application Settings
                _settings.ApplicationSettings.RememberWindowPosition = chkRememberWindowPosition.Checked;
                _settings.ApplicationSettings.ConfirmBeforeProcessing = chkConfirmBeforeProcessing.Checked;
                _settings.ApplicationSettings.RemoveDocumentFilesOnExit = chkRemoveDocumentFilesOnExit.Checked;
                _settings.ApplicationSettings.MaxFileBatchSize = (int)numMaxBatchSize.Value;
                _settings.ApplicationSettings.RecentFilesCount = (int)numRecentFiles.Value;

                // Save Processing Settings
                _settings.Processing.CreateBackups = chkCreateBackups.Checked;
                _settings.Processing.ValidateDocuments = chkValidateDocuments.Checked;
                _settings.Processing.SkipCorruptedFiles = chkSkipCorrupted.Checked;
                _settings.Processing.PreserveFileAttributes = chkPreserveAttributes.Checked;
                _settings.Processing.MaxFileSizeBytes = (long)numMaxFileSize.Value * 1024 * 1024; // Convert MB to bytes
                _settings.Processing.MaxConcurrentFiles = (int)numConcurrentFiles.Value;
                _settings.Processing.ProcessingTimeoutMinutes = (int)numProcessingTimeout.Value / 60; // Convert seconds to minutes

                // Save Interface Settings
                _settings.UI.RememberWindowSize = chkRememberWindowSize.Checked;
                _settings.UI.ShowProgressDetails = chkShowProgressDetails.Checked;
                _settings.UI.AutoSelectFirstFile = chkAutoSelectFirstFile.Checked;
                _settings.UI.ShowToolTips = chkShowToolTips.Checked;
                _settings.UI.ConfirmOnExit = chkConfirmOnExit.Checked;
                _settings.UI.ShowStatusBar = chkShowStatusBar.Checked;
                _settings.UI.Theme = cmbTheme.Text;

                await _settings.SaveAsync();

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (_hasChanges)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Are you sure you want to close without saving?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    return;
                }
            }

            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BtnOpenCurrentFolder_Click(object sender, EventArgs e)
        {
            try
            {
                string pathToOpen = chkUseCentralizedStorage.Checked && !string.IsNullOrWhiteSpace(txtBaseStoragePath.Text)
                    ? txtBaseStoragePath.Text
                    : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (!Directory.Exists(pathToOpen))
                {
                    Directory.CreateDirectory(pathToOpen);
                }

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = pathToOpen,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening folder: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnImportSettings_Click(object sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            openFileDialog.Title = "Import Settings";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var importedSettings = await AppSettings.ImportSettingsAsync(openFileDialog.FileName);

                    var result = MessageBox.Show(
                        "This will replace all current settings with the imported ones. Are you sure?",
                        "Import Settings",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _settings.ChangelogSettings = importedSettings.ChangelogSettings;
                        _settings.ApplicationSettings = importedSettings.ApplicationSettings;
                        _settings.Processing = importedSettings.Processing;
                        _settings.UI = importedSettings.UI;
                        _settings.Logging = importedSettings.Logging;

                        LoadSettings();
                        _hasChanges = true;

                        MessageBox.Show("Settings imported successfully!", "Import Complete",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing settings: {ex.Message}", "Import Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnExportSettings_Click(object sender, EventArgs e)
        {
            using var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            saveFileDialog.Title = "Export Settings";
            saveFileDialog.FileName = $"BulkEditor_Settings_{DateTime.Now:yyyy-MM-dd}.json";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    await _settings.ExportSettingsAsync(saveFileDialog.FileName);
                    MessageBox.Show("Settings exported successfully!", "Export Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting settings: {ex.Message}", "Export Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Migrates the entire BulkEditor storage directory to a new location
        /// </summary>
        private async Task<bool> MigrateStorageDirectory(string originalPath, string newPath)
        {
            try
            {
                // Skip migration if original path doesn't exist or is the same as new path
                if (!Directory.Exists(originalPath) || originalPath.Equals(newPath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Confirm migration with user
                var result = MessageBox.Show(
                    $"This will move all BulkEditor data from:\n{originalPath}\n\nTo:\n{newPath}\n\nDo you want to continue?",
                    "Migrate Storage Location",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    return false;
                }

                // Create new directory if it doesn't exist
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }

                // Check if new directory is empty (to avoid conflicts)
                if (Directory.GetFileSystemEntries(newPath).Length > 0)
                {
                    MessageBox.Show(
                        $"The destination directory is not empty:\n{newPath}\n\nPlease choose an empty directory or manually merge the contents.",
                        "Migration Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return false;
                }

                // Get all subdirectories and files in the original location
                var directories = Directory.GetDirectories(originalPath, "*", SearchOption.AllDirectories);
                var files = Directory.GetFiles(originalPath, "*", SearchOption.AllDirectories);

                // Create a backup list of what we're moving (for rollback if needed)
                var movedItems = new List<(string source, string destination)>();

                try
                {
                    // Create all subdirectories in the new location
                    foreach (var dir in directories)
                    {
                        var relativePath = Path.GetRelativePath(originalPath, dir);
                        var newDir = Path.Combine(newPath, relativePath);
                        Directory.CreateDirectory(newDir);
                    }

                    // Move all files
                    foreach (var file in files)
                    {
                        var relativePath = Path.GetRelativePath(originalPath, file);
                        var newFile = Path.Combine(newPath, relativePath);

                        File.Move(file, newFile);
                        movedItems.Add((file, newFile));
                    }

                    // Remove the original empty directories (from deepest to shallowest)
                    var sortedDirs = directories.OrderByDescending(d => d.Length).ToArray();
                    foreach (var dir in sortedDirs)
                    {
                        if (Directory.Exists(dir) && !Directory.GetFileSystemEntries(dir).Any())
                        {
                            Directory.Delete(dir);
                        }
                    }

                    // Finally remove the original base directory if it's empty
                    if (Directory.Exists(originalPath) && !Directory.GetFileSystemEntries(originalPath).Any())
                    {
                        Directory.Delete(originalPath);
                    }

                    MessageBox.Show(
                        $"Successfully migrated BulkEditor data to:\n{newPath}",
                        "Migration Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return true;
                }
                catch (Exception ex)
                {
                    // Migration failed - attempt to roll back
                    MessageBox.Show(
                        $"Error during migration: {ex.Message}\n\nAttempting to restore files to original location...",
                        "Migration Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    await RollbackMigration(movedItems, originalPath);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error preparing migration: {ex.Message}",
                    "Migration Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Attempts to roll back a failed migration
        /// </summary>
        private async Task RollbackMigration(List<(string source, string destination)> movedItems, string originalPath)
        {

            await Task.Run(() =>
            {

                try
                {
                    // Ensure original directory exists
                    if (!Directory.Exists(originalPath))
                    {
                        Directory.CreateDirectory(originalPath);
                    }

                    // Move files back to original location
                    foreach (var (source, destination) in movedItems)
                    {
                        if (File.Exists(destination))
                        {
                            // Ensure source directory exists
                            var sourceDir = Path.GetDirectoryName(source);
                            if (!Directory.Exists(sourceDir))
                            {
                                Directory.CreateDirectory(sourceDir);
                            }

                            File.Move(destination, source);
                        }
                    }

                    MessageBox.Show(
                        "Files have been restored to their original location.",
                        "Rollback Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error during rollback: {ex.Message}\n\nSome files may be in an inconsistent state. Please check both directories manually.",
                        "Rollback Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            });
        }
        
        /// <summary>
        /// Refreshes the logging settings display after import
        /// </summary>
        private void RefreshLoggingSettingsDisplay()
        {
            cmbLogLevel.Text = _settings.Logging.LogLevel;
            chkEnableFileLogging.Checked = _settings.Logging.EnableFileLogging;
            chkLogUserActions.Checked = _settings.Logging.LogUserActions;
            chkLogPerformance.Checked = _settings.Logging.LogPerformanceMetrics;
        }

        /// <summary>
        /// Refreshes the log viewer with recent entries
        /// </summary>
        private async void BtnRefreshLogs_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnRefreshLogs.InvokeRequired)
                {
                    btnRefreshLogs.Invoke(new Action(() =>
                    {
                        btnRefreshLogs.Enabled = false;
                        btnRefreshLogs.Text = "⏳ Loading...";
                    }));
                }
                else
                {
                    btnRefreshLogs.Enabled = false;
                    btnRefreshLogs.Text = "⏳ Loading...";
                }

                // Use the singleton LoggingService instance
                var logger = Services.LoggingService.Instance;
                
                // Force some log entries to be created for testing
                logger.LogInformation("Settings dialog opened at {Timestamp}", DateTime.Now);
                logger.LogInformation("Log viewer refresh requested");
                
                // Only log user action if this is a user-initiated refresh
                if (sender != null)
                {
                    logger.LogInformation("User Action: Log Refresh - User requested log refresh from settings");
                }

                // Check log file path and directory
                var logViewerService = new Services.LogViewerService(_settings.Logging);
                string logPath = GetLogFilePath();
                string logDir = Path.GetDirectoryName(logPath);
                
                // Ensure log directory exists
                if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
                {
                    try
                    {
                        Directory.CreateDirectory(logDir);
                        logger.LogInformation("Created log directory: {LogDirectory}", logDir);
                    }
                    catch (Exception dirEx)
                    {
                        logger.LogError(dirEx, "Failed to create log directory: {LogDirectory}", logDir);
                    }
                }
                
                // Give logging system time to write the entries
                await Task.Delay(500);
                
                var logEntries = await logViewerService.GetRecentLogsAsync(200);
                
                // Add diagnostic information
                var diagnosticInfo = new List<string>();
                diagnosticInfo.Add($"[{DateTime.Now:HH:mm:ss}] === Log Viewer Diagnostic Info ===");
                diagnosticInfo.Add($"Log File Path: {logPath}");
                diagnosticInfo.Add($"Log Directory: {logDir}");
                diagnosticInfo.Add($"Directory Exists: {Directory.Exists(logDir)}");
                diagnosticInfo.Add($"File Exists: {File.Exists(logPath)}");
                diagnosticInfo.Add($"File Logging Enabled: {_settings.Logging.EnableFileLogging}");
                diagnosticInfo.Add($"Log Level: {_settings.Logging.LogLevel}");
                diagnosticInfo.Add($"Log Format: {_settings.Logging.LogFormat}");
                diagnosticInfo.Add($"Entries Found: {logEntries.Count}");
                
                if (File.Exists(logPath))
                {
                    try
                    {
                        var fileInfo = new FileInfo(logPath);
                        diagnosticInfo.Add($"Log File Size: {fileInfo.Length} bytes");
                        diagnosticInfo.Add($"Last Modified: {fileInfo.LastWriteTime}");
                    }
                    catch { }
                }
                
                diagnosticInfo.Add("=== End Diagnostic Info ===");
                diagnosticInfo.Add("");

                if (lstLogEntries.InvokeRequired)
                {
                    lstLogEntries.Invoke(new Action(() => UpdateLogDisplay(logEntries, diagnosticInfo)));
                }
                else
                {
                    UpdateLogDisplay(logEntries, diagnosticInfo);
                }

                if (lblLogInfo.InvokeRequired)
                {
                    lblLogInfo.Invoke(new Action(() =>
                    {
                        lblLogInfo.Text = $"Last refreshed: {DateTime.Now:HH:mm:ss} ({logEntries.Count} entries)";
                    }));
                }
                else
                {
                    lblLogInfo.Text = $"Last refreshed: {DateTime.Now:HH:mm:ss} ({logEntries.Count} entries)";
                }

                logger.LogDebug("Log viewer refreshed with {EntryCount} entries", logEntries.Count);
            }
            catch (Exception ex)
            {
                var logger = Services.LoggingService.Instance;
                logger.LogError(ex, "Error refreshing log viewer");
                
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        MessageBox.Show($"Error refreshing logs: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
                else
                {
                    MessageBox.Show($"Error refreshing logs: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                if (btnRefreshLogs.InvokeRequired)
                {
                    btnRefreshLogs.Invoke(new Action(() =>
                    {
                        btnRefreshLogs.Enabled = true;
                        btnRefreshLogs.Text = "🔄 Refresh";
                    }));
                }
                else
                {
                    btnRefreshLogs.Enabled = true;
                    btnRefreshLogs.Text = "🔄 Refresh";
                }
            }
        }
        
        private void UpdateLogDisplay(List<Services.LogEntry> logEntries, List<string> diagnosticInfo)
        {
            lstLogEntries.Items.Clear();
            
            // Add diagnostic info first
            foreach (var info in diagnosticInfo)
            {
                lstLogEntries.Items.Add(info);
            }
            
            if (logEntries.Count == 0)
            {
                lstLogEntries.Items.Add("No log entries found in the log file.");
                lstLogEntries.Items.Add("This could indicate:");
                lstLogEntries.Items.Add("- Log file is empty or doesn't contain parseable entries");
                lstLogEntries.Items.Add("- Log format may not match expected format");
                lstLogEntries.Items.Add("- Logging configuration issue");
            }
            else
            {
                lstLogEntries.Items.Add("=== Recent Log Entries ===");
                foreach (var entry in logEntries.OrderBy(x => x.Timestamp))
                {
                    lstLogEntries.Items.Add(entry.ToString());
                }
            }
            
            // Auto-scroll to show latest entries
            if (lstLogEntries.Items.Count > 0)
            {
                lstLogEntries.TopIndex = Math.Max(0, lstLogEntries.Items.Count - 1);
            }
        }
        
        private string GetLogFilePath()
        {
            if (Path.IsPathRooted(_settings.Logging.LogFilePath))
            {
                return _settings.Logging.LogFilePath;
            }
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings.Logging.LogFilePath);
        }
        
        /// <summary>
        /// Get the path where exported logs should be saved
        /// </summary>
        private string GetLogExportPath()
        {
            string baseStoragePath;
            
            // Use centralized storage path if configured, otherwise use application directory
            if (_settings.ChangelogSettings.UseCentralizedStorage && !string.IsNullOrWhiteSpace(_settings.ChangelogSettings.BaseStoragePath))
            {
                baseStoragePath = _settings.ChangelogSettings.BaseStoragePath;
            }
            else
            {
                baseStoragePath = AppDomain.CurrentDomain.BaseDirectory;
            }
            
            // Create logs subfolder within the storage path
            string logsFolder = Path.Combine(baseStoragePath, "Logs");
            
            // Generate filename with timestamp
            string filename = $"BulkEditor_Logs_{DateTime.Now:yyyy-MM-dd_HHmmss}.txt";
            
            return Path.Combine(logsFolder, filename);
        }
        
        /// <summary>
        /// Exports current logs to a file
        /// </summary>
        private async void BtnExportLogs_Click(object sender, EventArgs e)
        {
            try
            {
                btnExportLogs.Enabled = false;
                btnExportLogs.Text = "⏳ Exporting...";

                var logViewerService = new Services.LogViewerService(_settings.Logging);
                string exportPath = GetLogExportPath();
                
                // Ensure the logs directory exists
                string logDir = Path.GetDirectoryName(exportPath);
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                bool success = await logViewerService.ExportLogsAsync(exportPath);

                if (success)
                {
                    try
                    {
                        // Auto-open the log file in notepad without prompting
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "notepad.exe",
                            Arguments = $"\"{exportPath}\"",
                            UseShellExecute = true
                        });
                        
                        // Log the successful export
                        var logger = Services.LoggingService.Instance;
                        logger.LogUserAction("Log Export", $"Exported to: {exportPath}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Logs exported to: {exportPath}\n\nError opening file in Notepad: {ex.Message}",
                            "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Failed to export logs. Please check the configuration and try again.", "Export Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting logs: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnExportLogs.Enabled = true;
                btnExportLogs.Text = "📤 Export";
            }
        }

        /// <summary>
        /// Clears old log files to free up space
        /// </summary>
        private async void BtnClearOldLogs_Click(object sender, EventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "This will delete log files older than 7 days. Continue?",
                    "Clear Old Logs",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    btnClearOldLogs.Enabled = false;
                    btnClearOldLogs.Text = "⏳ Clearing...";

                    var logViewerService = new Services.LogViewerService(_settings.Logging);
                    int deletedCount = await logViewerService.ClearOldLogsAsync(7);

                    MessageBox.Show($"Cleared {deletedCount} old log files.", "Cleanup Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh the log viewer
                    BtnRefreshLogs_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing old logs: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnClearOldLogs.Enabled = true;
                btnClearOldLogs.Text = "🗑️ Clear Old";
            }
        }
    }
}