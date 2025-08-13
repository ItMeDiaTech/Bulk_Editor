#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bulk_Editor.Configuration;
using Bulk_Editor.Models;
using Bulk_Editor.Services;
using Bulk_Editor.Services.Abstractions;
using Microsoft.Extensions.Configuration;
namespace Bulk_Editor
{
    public partial class MainForm : Form
    {
        [GeneratedRegex(@"\s*\((\d{5,6})\)\s*$")]
        private static partial Regex ContentIdPatternRegex();

        [GeneratedRegex(@"[ ]{2,}")]
        private static partial Regex MultipleSpacesPatternRegex();

        private readonly ILoggingService _loggingService;
        private readonly ISettingsService _settingsService;
        private readonly IThemeService _themeService;
        private readonly IProcessingService _processingService;
        private readonly ILogViewerService _logViewerService;
        private readonly ChangelogManager _changelogManager;
        private readonly IValidationService _validationService;
        private readonly WindowStateService _windowStateService;

        private HyperlinkReplacementRules? _hyperlinkReplacementRules;
        private WordDocumentProcessor? _processor;
        private AppSettings _appSettings;

        // Progress reporting and cancellation
        private CancellationTokenSource? _cancellationTokenSource;
        private IProgress<ProgressReport>? _progressReporter;

        // Button state management
        private string _originalButtonText = default!;
        private Color _originalButtonColor;

        public MainForm(
            ILoggingService loggingService,
            ISettingsService settingsService,
            IThemeService themeService,
            IProcessingService processingService,
            ILogViewerService logViewerService,
            ChangelogManager changelogManager,
            IValidationService validationService,
            WindowStateService windowStateService)
        {
            _loggingService = loggingService;
            _settingsService = settingsService;
            _themeService = themeService;
            _processingService = processingService;
            _logViewerService = logViewerService;
            _changelogManager = changelogManager;
            _validationService = validationService;
            _windowStateService = windowStateService;

            _appSettings = _settingsService.Settings;

            InitializeComponent();
            LoadEmbeddedResources();
            SetupProgressReporting();
            SetupCheckboxDependencies();
            SetupFileListHandlers();
            LoadConfigurationAsync();
        }

        private bool _isInitialized = false;

        protected override void SetVisibleCore(bool value)
        {
            // Don't show the form until initialization is complete
            if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                base.SetVisibleCore(_isInitialized && value);
            }
            else
            {
                // For other platforms, if the form is already initialized,
                // we can set its visibility directly to the desired value.
                // If not initialized, we keep it hidden.
                if (_isInitialized)
                {
                    base.SetVisibleCore(value);
                }
                else
                {
                    // Ensure the form remains hidden until initialized
                    base.SetVisibleCore(false);
                }
            }
        }

        private void LoadEmbeddedResources()
        {
            // Note: Settings icon is now loaded by ThemeService.ApplyTheme()
            // which will select the appropriate icon based on the current theme

            // Initialize settings button with fallback in case theme service fails
            try
            {
                btnSettings.Text = "⚙️";
                if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
                {
                    btnSettings.Font = new Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
                }
                btnSettings.ForeColor = System.Drawing.Color.Black; // Will be overridden by theme
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize Settings button: {ex.Message}");
            }
        }

        private void SetupProgressReporting()
        {
            // Store original button state
            _originalButtonText = btnRunTools.Text;
            _originalButtonColor = btnRunTools.BackColor;

            // Create progress reporter that updates UI on main thread
            _progressReporter = new Progress<ProgressReport>(UpdateProgressUI);
        }

        private async void LoadConfigurationAsync()
        {
            _hyperlinkReplacementRules = await HyperlinkReplacementRules.LoadAsync();

            // Initialize services
            InitializeServices();

            // Initialize processor with settings
            _processor = new WordDocumentProcessor(_appSettings.ApiSettings, _appSettings.RetrySettings);

            // Initialize changelog manager
            await _changelogManager.InitializeStorageAsync();

            // Perform cleanup if needed
            await _changelogManager.CleanupOldChangelogsAsync();
        }

        private void InitializeServices()
        {
            _themeService.ApplyTheme(this);
            _windowStateService.RestoreWindowState(this);

            // Setup window state saving events
            SetupWindowStateEvents();

            // Log application startup
            _loggingService.LogUserAction("Application Started", $"Version: {Application.ProductVersion}");
            _loggingService.LogInformation("Services initialized successfully");

            // Mark initialization as complete - form can now be shown with correct theme/icons
            _isInitialized = true;

            // Force the form to become visible now that initialization is complete
            if (!Visible)
            {
                SetVisibleCore(true);
            }
        }

        private void SetupWindowStateEvents()
        {
            // Save window state when form is moved or resized
            this.Move += (s, e) => _windowStateService?.SaveWindowState(this);
            this.Resize += (s, e) => _windowStateService?.SaveWindowState(this);
            this.FormClosing += (s, e) =>
            {
                // Check ConfirmOnExit setting
                if (_appSettings.UI.ConfirmOnExit)
                {
                    var result = MessageBox.Show(
                        "Are you sure you want to exit the application?",
                        "Confirm Exit",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result != DialogResult.Yes)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                // Remove document files from list if setting is enabled
                if (_appSettings.ApplicationSettings.RemoveDocumentFilesOnExit)
                {
                    lstFiles.Items.Clear();
                    lstFiles.Tag = null;
                    txtFolderPath.Text = string.Empty;
                    pnlChangelog.Visible = false;
                }

                _windowStateService?.SaveWindowState(this);
                _loggingService.LogUserAction("Application Closing", "User closed the application");
            };
        }

        private void SetupFileListHandlers()
        {
            lstFiles.SelectedIndexChanged += LstFiles_SelectedIndexChanged!;
            lstFiles.DoubleClick += LstFiles_DoubleClick!;
        }




        private void SetupCheckboxDependencies()
        {
            // Load saved checkbox states
            LoadCheckboxStates();
            UpdateSubCheckboxStates();

            // Setup event handlers for state persistence
            chkFixSourceHyperlinks.CheckedChanged += (s, e) => { UpdateSubCheckboxStates(); SaveCheckboxStates(); };
            chkAppendContentID.CheckedChanged += (s, e) => SaveCheckboxStates();
            chkCheckTitleChanges.CheckedChanged += (s, e) => SaveCheckboxStates();
            chkFixTitles.CheckedChanged += (s, e) => SaveCheckboxStates();
            chkFixInternalHyperlink.CheckedChanged += (s, e) => SaveCheckboxStates();
            chkFixDoubleSpaces.CheckedChanged += (s, e) => SaveCheckboxStates();
            chkReplaceHyperlink.CheckedChanged += (s, e) => SaveCheckboxStates();
            chkOpenChangelogAfterUpdates.CheckedChanged += (s, e) => SaveCheckboxStates();
        }

        private void LoadCheckboxStates()
        {
            try
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
                {
                    chkFixSourceHyperlinks.Checked = _appSettings.UI.FixSourceHyperlinks;
                    chkAppendContentID.Checked = _appSettings.UI.AppendContentID;
                    chkCheckTitleChanges.Checked = _appSettings.UI.CheckTitleChanges;
                    chkFixTitles.Checked = _appSettings.UI.FixTitles;
                    chkFixInternalHyperlink.Checked = _appSettings.UI.FixInternalHyperlink;
                    chkFixDoubleSpaces.Checked = _appSettings.UI.FixDoubleSpaces;
                    chkReplaceHyperlink.Checked = _appSettings.UI.ReplaceHyperlink;
                    chkOpenChangelogAfterUpdates.Checked = _appSettings.UI.OpenChangelogAfterUpdates;
                }

                _loggingService.LogDebug("Checkbox states loaded from settings");
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error loading checkbox states");
            }
        }

        private async void SaveCheckboxStates()
        {
            try
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(6, 1))
                {
                    _appSettings.UI.FixSourceHyperlinks = chkFixSourceHyperlinks.Checked;
                    _appSettings.UI.AppendContentID = chkAppendContentID.Checked;
                    _appSettings.UI.CheckTitleChanges = chkCheckTitleChanges.Checked;
                    _appSettings.UI.FixTitles = chkFixTitles.Checked;
                    _appSettings.UI.FixInternalHyperlink = chkFixInternalHyperlink.Checked;
                    _appSettings.UI.FixDoubleSpaces = chkFixDoubleSpaces.Checked;
                    _appSettings.UI.ReplaceHyperlink = chkReplaceHyperlink.Checked;
                    _appSettings.UI.OpenChangelogAfterUpdates = chkOpenChangelogAfterUpdates.Checked;
                }

                await _settingsService.SaveSettingsAsync();
                _loggingService.LogDebug("Checkbox states saved to settings");
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error saving checkbox states");
            }
        }
    }









}
}

