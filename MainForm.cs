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

namespace Bulk_Editor
{
    public partial class MainForm : Form
    {
        [GeneratedRegex(@"\s*\((\d{5,6})\)\s*$")]
        private static partial Regex ContentIdPatternRegex();

        [GeneratedRegex(@"[ ]{2,}")]
        private static partial Regex MultipleSpacesPatternRegex();

        private HyperlinkReplacementRules _hyperlinkReplacementRules;
        private WordDocumentProcessor _processor;
        private AppSettings _appSettings;
        private ChangelogManager _changelogManager;

        // Services
        private LoggingService _loggingService;
        private ThemeService _themeService;
        private WindowStateService _windowStateService;

        // Progress reporting and cancellation
        private CancellationTokenSource _cancellationTokenSource;
        private IProgress<ProgressReport> _progressReporter;

        // Button state management
        private string _originalButtonText;
        private Color _originalButtonColor;

        public MainForm()
        {
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
            base.SetVisibleCore(_isInitialized && value);
        }

        private void LoadEmbeddedResources()
        {
            // Note: Settings icon is now loaded by ThemeService.ApplyTheme()
            // which will select the appropriate icon based on the current theme

            // Initialize settings button with fallback in case theme service fails
            try
            {
                btnSettings.Text = "⚙️";
                btnSettings.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
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
            _appSettings = await AppSettings.LoadAsync();
            _hyperlinkReplacementRules = await HyperlinkReplacementRules.LoadAsync();

            // Initialize processor with settings
            _processor = new WordDocumentProcessor(_appSettings.ApiSettings, _appSettings.RetrySettings);

            // Initialize changelog manager
            _changelogManager = new ChangelogManager(_appSettings.ChangelogSettings);
            await _changelogManager.InitializeStorageAsync();

            // Perform cleanup if needed
            await _changelogManager.CleanupOldChangelogsAsync();

            // Initialize services
            InitializeServices();
        }

        private void InitializeServices()
        {
            // Initialize logging service using singleton pattern
            LoggingService.Initialize(_appSettings.Logging);
            _loggingService = LoggingService.Instance;

            // Initialize theme service and apply current theme
            _themeService = new ThemeService(_appSettings.UI);
            _themeService.ApplyTheme(this);

            // Initialize window state service and restore window state
            _windowStateService = new WindowStateService(_appSettings.ApplicationSettings, _appSettings.UI);
            _windowStateService.RestoreWindowState(this);

            // Setup window state saving events
            SetupWindowStateEvents();

            // Log application startup
            _loggingService?.LogUserAction("Application Started", $"Version: {Application.ProductVersion}");
            _loggingService?.LogInformation("Services initialized successfully");

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
                _loggingService?.LogUserAction("Application Closing");
            };
        }

        private void SetupFileListHandlers()
        {
            lstFiles.SelectedIndexChanged += LstFiles_SelectedIndexChanged;
            lstFiles.DoubleClick += LstFiles_DoubleClick;
        }




        private void SetupCheckboxDependencies()
        {
            UpdateSubCheckboxStates();
            chkFixSourceHyperlinks.CheckedChanged += ChkFixSourceHyperlinks_CheckedChanged;
        }









    }
}

