using System;
using System.IO;
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
            chkCheckForUpdates.Checked = _settings.ApplicationSettings.CheckForUpdatesOnStartup;
            chkRememberWindowPosition.Checked = _settings.ApplicationSettings.RememberWindowPosition;
            chkConfirmBeforeProcessing.Checked = _settings.ApplicationSettings.ConfirmBeforeProcessing;
            chkShowProcessingPreview.Checked = _settings.ApplicationSettings.ShowProcessingPreview;
            chkAutoSaveSettings.Checked = _settings.ApplicationSettings.AutoSaveSettings;
            numMaxBatchSize.Value = _settings.ApplicationSettings.MaxFileBatchSize;
            numRecentFiles.Value = _settings.ApplicationSettings.RecentFilesCount;

            // Processing Settings
            chkCreateBackups.Checked = _settings.Processing.CreateBackups;
            chkValidateDocuments.Checked = _settings.Processing.ValidateDocuments;
            chkSkipCorrupted.Checked = _settings.Processing.SkipCorruptedFiles;
            chkPreserveAttributes.Checked = _settings.Processing.PreserveFileAttributes;
            chkCreateProcessingReport.Checked = _settings.Processing.CreateProcessingReport;
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
            chkEnableSounds.Checked = _settings.UI.EnableSounds;
            chkShowWelcomeScreen.Checked = _settings.UI.ShowWelcomeScreen;
            cmbTheme.Text = _settings.UI.Theme;
            cmbLanguage.Text = _settings.UI.Language;

            // Logging Settings
            chkEnableFileLogging.Checked = _settings.Logging.EnableFileLogging;
            chkEnableConsoleLogging.Checked = _settings.Logging.EnableConsoleLogging;
            chkLogUserActions.Checked = _settings.Logging.LogUserActions;
            chkLogPerformance.Checked = _settings.Logging.LogPerformanceMetrics;
            chkEnableDebugMode.Checked = _settings.Logging.EnableDebugMode;
            chkIncludeStackTrace.Checked = _settings.Logging.IncludeStackTrace;
            chkCompressOldLogs.Checked = _settings.Logging.CompressOldLogs;
            cmbLogLevel.Text = _settings.Logging.LogLevel;
            numMaxLogSize.Value = _settings.Logging.MaxLogFileSizeMB;
            numMaxLogFiles.Value = _settings.Logging.MaxLogFiles;

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
            chkCheckForUpdates.CheckedChanged += (s, e) => _hasChanges = true;
            chkRememberWindowPosition.CheckedChanged += (s, e) => _hasChanges = true;
            chkConfirmBeforeProcessing.CheckedChanged += (s, e) => _hasChanges = true;
            chkShowProcessingPreview.CheckedChanged += (s, e) => _hasChanges = true;
            chkAutoSaveSettings.CheckedChanged += (s, e) => _hasChanges = true;
            numMaxBatchSize.ValueChanged += (s, e) => _hasChanges = true;
            numRecentFiles.ValueChanged += (s, e) => _hasChanges = true;

            // Track changes for Processing Settings
            chkCreateBackups.CheckedChanged += (s, e) => _hasChanges = true;
            chkValidateDocuments.CheckedChanged += (s, e) => _hasChanges = true;
            chkSkipCorrupted.CheckedChanged += (s, e) => _hasChanges = true;
            chkPreserveAttributes.CheckedChanged += (s, e) => _hasChanges = true;
            chkCreateProcessingReport.CheckedChanged += (s, e) => _hasChanges = true;
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
            chkEnableSounds.CheckedChanged += (s, e) => _hasChanges = true;
            chkShowWelcomeScreen.CheckedChanged += (s, e) => _hasChanges = true;
            cmbTheme.SelectedIndexChanged += (s, e) => _hasChanges = true;
            cmbLanguage.SelectedIndexChanged += (s, e) => _hasChanges = true;

            // Track changes for Logging Settings
            chkEnableFileLogging.CheckedChanged += (s, e) => _hasChanges = true;
            chkEnableConsoleLogging.CheckedChanged += (s, e) => _hasChanges = true;
            chkLogUserActions.CheckedChanged += (s, e) => _hasChanges = true;
            chkLogPerformance.CheckedChanged += (s, e) => _hasChanges = true;
            chkEnableDebugMode.CheckedChanged += (s, e) => _hasChanges = true;
            chkIncludeStackTrace.CheckedChanged += (s, e) => _hasChanges = true;
            chkCompressOldLogs.CheckedChanged += (s, e) => _hasChanges = true;
            cmbLogLevel.SelectedIndexChanged += (s, e) => _hasChanges = true;
            numMaxLogSize.ValueChanged += (s, e) => _hasChanges = true;
            numMaxLogFiles.ValueChanged += (s, e) => _hasChanges = true;
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
                chkCheckForUpdates.Checked = defaultAppSettings.ApplicationSettings.CheckForUpdatesOnStartup;
                chkRememberWindowPosition.Checked = defaultAppSettings.ApplicationSettings.RememberWindowPosition;
                chkConfirmBeforeProcessing.Checked = defaultAppSettings.ApplicationSettings.ConfirmBeforeProcessing;
                chkShowProcessingPreview.Checked = defaultAppSettings.ApplicationSettings.ShowProcessingPreview;
                chkAutoSaveSettings.Checked = defaultAppSettings.ApplicationSettings.AutoSaveSettings;
                numMaxBatchSize.Value = defaultAppSettings.ApplicationSettings.MaxFileBatchSize;
                numRecentFiles.Value = defaultAppSettings.ApplicationSettings.RecentFilesCount;

                // Reset Processing Settings
                chkCreateBackups.Checked = defaultAppSettings.Processing.CreateBackups;
                chkValidateDocuments.Checked = defaultAppSettings.Processing.ValidateDocuments;
                chkSkipCorrupted.Checked = defaultAppSettings.Processing.SkipCorruptedFiles;
                chkPreserveAttributes.Checked = defaultAppSettings.Processing.PreserveFileAttributes;
                chkCreateProcessingReport.Checked = defaultAppSettings.Processing.CreateProcessingReport;
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
                chkEnableSounds.Checked = defaultAppSettings.UI.EnableSounds;
                chkShowWelcomeScreen.Checked = defaultAppSettings.UI.ShowWelcomeScreen;
                cmbTheme.Text = defaultAppSettings.UI.Theme;
                cmbLanguage.Text = defaultAppSettings.UI.Language;

                // Reset Logging Settings
                chkEnableFileLogging.Checked = defaultAppSettings.Logging.EnableFileLogging;
                chkEnableConsoleLogging.Checked = defaultAppSettings.Logging.EnableConsoleLogging;
                chkLogUserActions.Checked = defaultAppSettings.Logging.LogUserActions;
                chkLogPerformance.Checked = defaultAppSettings.Logging.LogPerformanceMetrics;
                chkEnableDebugMode.Checked = defaultAppSettings.Logging.EnableDebugMode;
                chkIncludeStackTrace.Checked = defaultAppSettings.Logging.IncludeStackTrace;
                chkCompressOldLogs.Checked = defaultAppSettings.Logging.CompressOldLogs;
                cmbLogLevel.Text = defaultAppSettings.Logging.LogLevel;
                numMaxLogSize.Value = defaultAppSettings.Logging.MaxLogFileSizeMB;
                numMaxLogFiles.Value = defaultAppSettings.Logging.MaxLogFiles;

                _hasChanges = true;
                UpdateDependentControls();
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate base storage path
                if (chkUseCentralizedStorage.Checked && !string.IsNullOrWhiteSpace(txtBaseStoragePath.Text))
                {
                    try
                    {
                        if (!Directory.Exists(txtBaseStoragePath.Text))
                        {
                            Directory.CreateDirectory(txtBaseStoragePath.Text);
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
                _settings.ApplicationSettings.CheckForUpdatesOnStartup = chkCheckForUpdates.Checked;
                _settings.ApplicationSettings.RememberWindowPosition = chkRememberWindowPosition.Checked;
                _settings.ApplicationSettings.ConfirmBeforeProcessing = chkConfirmBeforeProcessing.Checked;
                _settings.ApplicationSettings.ShowProcessingPreview = chkShowProcessingPreview.Checked;
                _settings.ApplicationSettings.AutoSaveSettings = chkAutoSaveSettings.Checked;
                _settings.ApplicationSettings.MaxFileBatchSize = (int)numMaxBatchSize.Value;
                _settings.ApplicationSettings.RecentFilesCount = (int)numRecentFiles.Value;

                // Save Processing Settings
                _settings.Processing.CreateBackups = chkCreateBackups.Checked;
                _settings.Processing.ValidateDocuments = chkValidateDocuments.Checked;
                _settings.Processing.SkipCorruptedFiles = chkSkipCorrupted.Checked;
                _settings.Processing.PreserveFileAttributes = chkPreserveAttributes.Checked;
                _settings.Processing.CreateProcessingReport = chkCreateProcessingReport.Checked;
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
                _settings.UI.EnableSounds = chkEnableSounds.Checked;
                _settings.UI.ShowWelcomeScreen = chkShowWelcomeScreen.Checked;
                _settings.UI.Theme = cmbTheme.Text;
                _settings.UI.Language = cmbLanguage.Text;

                // Save Logging Settings
                _settings.Logging.EnableFileLogging = chkEnableFileLogging.Checked;
                _settings.Logging.EnableConsoleLogging = chkEnableConsoleLogging.Checked;
                _settings.Logging.LogUserActions = chkLogUserActions.Checked;
                _settings.Logging.LogPerformanceMetrics = chkLogPerformance.Checked;
                _settings.Logging.EnableDebugMode = chkEnableDebugMode.Checked;
                _settings.Logging.IncludeStackTrace = chkIncludeStackTrace.Checked;
                _settings.Logging.CompressOldLogs = chkCompressOldLogs.Checked;
                _settings.Logging.LogLevel = cmbLogLevel.Text;
                _settings.Logging.MaxLogFileSizeMB = (int)numMaxLogSize.Value;
                _settings.Logging.MaxLogFiles = (int)numMaxLogFiles.Value;

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
    }
}