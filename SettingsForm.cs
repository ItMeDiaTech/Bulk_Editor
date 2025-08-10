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

            // Update dependent controls
            UpdateDependentControls();
        }

        private void SetupEventHandlers()
        {
            // Track changes
            txtBaseStoragePath.TextChanged += (s, e) => _hasChanges = true;
            chkUseCentralizedStorage.CheckedChanged += (s, e) => { _hasChanges = true; UpdateDependentControls(); };
            chkOrganizeByDate.CheckedChanged += (s, e) => _hasChanges = true;
            numAutoCleanupDays.ValueChanged += (s, e) => _hasChanges = true;
            chkSeparateIndividualAndCombined.CheckedChanged += (s, e) => _hasChanges = true;
            chkCentralizeBackups.CheckedChanged += (s, e) => _hasChanges = true;
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
                "This will reset all changelog settings to their default values. Are you sure?",
                "Reset to Defaults",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                var defaultSettings = new ChangelogSettings();

                txtBaseStoragePath.Text = defaultSettings.BaseStoragePath;
                chkUseCentralizedStorage.Checked = defaultSettings.UseCentralizedStorage;
                chkOrganizeByDate.Checked = defaultSettings.OrganizeByDate;
                numAutoCleanupDays.Value = defaultSettings.AutoCleanupDays;
                chkSeparateIndividualAndCombined.Checked = defaultSettings.SeparateIndividualAndCombined;
                chkCentralizeBackups.Checked = defaultSettings.CentralizeBackups;

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

                // Save settings
                _settings.ChangelogSettings.BaseStoragePath = txtBaseStoragePath.Text.Trim();
                _settings.ChangelogSettings.UseCentralizedStorage = chkUseCentralizedStorage.Checked;
                _settings.ChangelogSettings.OrganizeByDate = chkOrganizeByDate.Checked;
                _settings.ChangelogSettings.AutoCleanupDays = (int)numAutoCleanupDays.Value;
                _settings.ChangelogSettings.SeparateIndividualAndCombined = chkSeparateIndividualAndCombined.Checked;
                _settings.ChangelogSettings.CentralizeBackups = chkCentralizeBackups.Checked;

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
    }
}