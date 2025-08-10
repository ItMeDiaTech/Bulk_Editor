using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bulk_Editor.Configuration;
using Bulk_Editor.Services;

namespace Bulk_Editor
{
    public partial class MainForm
    {
        private void BtnSelectFolder_Click(object sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Select a folder containing .docx files to process";
            folderDialog.ShowNewFolderButton = false;

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                txtFolderPath.Text = folderDialog.SelectedPath;
                LoadFiles(folderDialog.SelectedPath);
                ShowFileList();
            }
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            using var fileDialog = new OpenFileDialog();
            fileDialog.Title = "Select a .docx file to process";
            fileDialog.Filter = "Word documents (*.docx)|*.docx";
            fileDialog.Multiselect = false;

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                txtFolderPath.Text = fileDialog.FileName;
                LoadFile(fileDialog.FileName);
                ShowFileList();
            }
        }

        private async void LstFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndex >= 0)
            {
                string selectedItem = lstFiles.SelectedItem.ToString();
                string fileName = selectedItem.Split('(')[0].Trim();

                string changelogPath = _changelogManager?.FindLatestChangelog(txtFolderPath.Text) ?? FindLatestChangelog();

                if (File.Exists(changelogPath))
                {
                    await DisplayChangelogForFileAsync(changelogPath, fileName);
                }
                else
                {
                    lblChangelogTitle.Text = $"Changelog - {fileName}";
                    txtChangelog.Text = "No changelog file found. Please run the tools first.";
                    pnlChangelog.Visible = true;
                }
            }
        }

        private void LstFiles_DoubleClick(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndex >= 0 && !string.IsNullOrEmpty(txtFolderPath.Text))
            {
                string selectedItem = lstFiles.SelectedItem.ToString();
                string fileName = selectedItem.Split('(')[0].Trim();

                if (Directory.Exists(txtFolderPath.Text))
                {
                    string filePath = Path.Combine(txtFolderPath.Text, fileName);
                    if (File.Exists(filePath))
                    {
                        OpenFile(filePath);
                    }
                }
                else if (File.Exists(txtFolderPath.Text))
                {
                    OpenFile(txtFolderPath.Text);
                }
            }
        }

        private void ChkFixSourceHyperlinks_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSubCheckboxStates();
        }

        private async void BtnSettings_Click(object sender, EventArgs e)
        {
            try
            {
                using var settingsForm = new SettingsForm(_appSettings);

                // Apply current theme to settings form
                _themeService?.ApplyTheme(settingsForm);

                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    // Settings were saved, reload them asynchronously
                    _appSettings = await AppSettings.LoadAsync();

                    // Reinitialize services with new settings on a separate task to prevent UI blocking
                    await Task.Run(() =>
                    {
                        // Reinitialize changelog manager with new settings
                        _changelogManager = new ChangelogManager(_appSettings.ChangelogSettings);
                    });

                    await _changelogManager.InitializeStorageAsync();

                    // Apply theme changes on UI thread but asynchronously
                    this.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            InitializeServices();
                            lblStatus.Text = "Settings updated successfully";
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error reinitializing services: {ex.Message}");
                            lblStatus.Text = "Settings updated (theme may require restart)";
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddFile_Click(object sender, EventArgs e)
        {
            using var fileDialog = new OpenFileDialog();
            fileDialog.Title = "Select .docx files to add";
            fileDialog.Filter = "Word documents (*.docx)|*.docx";
            fileDialog.Multiselect = true;

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                var filePathMap = lstFiles.Tag as Dictionary<int, string> ?? new Dictionary<int, string>();

                foreach (string fileName in fileDialog.FileNames)
                {
                    FileInfo fileInfo = new(fileName);
                    string displayName = $"{fileInfo.Name} ({FormatFileSize(fileInfo.Length)})";

                    // Check if file is already in the list
                    bool alreadyExists = false;
                    for (int i = 0; i < lstFiles.Items.Count; i++)
                    {
                        if (lstFiles.Items[i].ToString() == displayName)
                        {
                            alreadyExists = true;
                            break;
                        }
                    }

                    if (!alreadyExists)
                    {
                        int index = lstFiles.Items.Add(displayName);
                        filePathMap[index] = fileName;
                    }
                }

                lstFiles.Tag = filePathMap;
                lblStatus.Text = $"Added files. Total: {lstFiles.Items.Count} files";
                ShowFileList();
            }
        }

        private void BtnRemoveFile_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndex >= 0)
            {
                var filePathMap = lstFiles.Tag as Dictionary<int, string> ?? new Dictionary<int, string>();

                // Remove from the file path map
                if (filePathMap.ContainsKey(lstFiles.SelectedIndex))
                {
                    filePathMap.Remove(lstFiles.SelectedIndex);
                }

                // Remove from list
                lstFiles.Items.RemoveAt(lstFiles.SelectedIndex);

                // Update the map indices
                var newFilePathMap = new Dictionary<int, string>();
                foreach (var kvp in filePathMap)
                {
                    if (kvp.Key < lstFiles.SelectedIndex)
                    {
                        newFilePathMap[kvp.Key] = kvp.Value;
                    }
                    else if (kvp.Key > lstFiles.SelectedIndex)
                    {
                        newFilePathMap[kvp.Key - 1] = kvp.Value;
                    }
                }

                lstFiles.Tag = newFilePathMap;
                lblStatus.Text = $"File removed. Total: {lstFiles.Items.Count} files";

                if (lstFiles.Items.Count == 0)
                {
                    pnlChangelog.Visible = false;
                }
            }
            else
            {
                MessageBox.Show("Please select a file to remove.", "No File Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnOpenFileLocation_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndex >= 0)
            {
                try
                {
                    string filePath = GetSelectedFilePath();
                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        string folderPath = Path.GetDirectoryName(filePath);
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = folderPath,
                            UseShellExecute = true
                        });
                    }
                    else if (!string.IsNullOrEmpty(txtFolderPath.Text) && Directory.Exists(txtFolderPath.Text))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = txtFolderPath.Text,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        MessageBox.Show("File location not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening file location: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a file first.", "No File Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnClearFileList_Click(object sender, EventArgs e)
        {
            if (lstFiles.Items.Count > 0)
            {
                var result = MessageBox.Show("Are you sure you want to clear all files from the list?",
                    "Clear File List", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    lstFiles.Items.Clear();
                    lstFiles.Tag = null;
                    txtFolderPath.Text = string.Empty;
                    pnlChangelog.Visible = false;
                    lblStatus.Text = "File list cleared";
                }
            }
        }

        private void BtnFixHighlightedDocument_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndex >= 0)
            {
                string filePath = GetSelectedFilePath();
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    // Temporarily store the current folder path
                    string originalPath = txtFolderPath.Text;

                    // Set the selected file as the target
                    txtFolderPath.Text = filePath;

                    // Process the single file
                    ProcessSingleFile();

                    // Restore original path
                    txtFolderPath.Text = originalPath;
                }
                else
                {
                    MessageBox.Show("Selected file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a file to process.", "No File Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnExportSingleChangelog_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndex >= 0)
            {
                try
                {
                    string selectedItem = lstFiles.SelectedItem.ToString();
                    string fileName = selectedItem.Split('(')[0].Trim();

                    // Determine save location based on settings
                    string exportFolder;
                    if (_appSettings.ChangelogSettings.UseCentralizedStorage)
                    {
                        var basePath = _appSettings.ChangelogSettings.GetIndividualChangelogsPath();
                        exportFolder = _appSettings.ChangelogSettings.GetDateBasedPath(basePath);
                    }
                    else
                    {
                        // Use document folder
                        bool isFolder = Directory.Exists(txtFolderPath.Text);
                        exportFolder = isFolder ? txtFolderPath.Text : Path.GetDirectoryName(txtFolderPath.Text);
                    }

                    // Ensure export folder exists
                    Directory.CreateDirectory(exportFolder);

                    // Generate unique filename with incrementing suffix if needed
                    string baseFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_Changelog_{DateTime.Now:yyyyMMdd}";
                    string exportPath = GetUniqueFilePath(exportFolder, baseFileName, ".txt");

                    // Write the changelog content to the export location
                    File.WriteAllText(exportPath, txtChangelog.Text);

                    // Open in Notepad
                    OpenInNotepad(exportPath);

                    lblStatus.Text = $"Changelog exported to: {Path.GetFileName(exportPath)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting changelog: {ex.Message}", "Export Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a file first.", "No File Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnExportChangelog_Click(object sender, EventArgs e)
        {
            try
            {
                string changelogPath = _changelogManager?.FindLatestChangelog(txtFolderPath.Text) ?? FindLatestChangelog();

                if (!string.IsNullOrEmpty(changelogPath) && File.Exists(changelogPath))
                {
                    // Determine save location based on settings
                    string exportFolder;
                    if (_appSettings.ChangelogSettings.UseCentralizedStorage)
                    {
                        // Use the Combined changelogs folder for batch exports
                        var basePath = _appSettings.ChangelogSettings.GetCombinedChangelogsPath();
                        exportFolder = _appSettings.ChangelogSettings.GetDateBasedPath(basePath);
                    }
                    else
                    {
                        // Use document folder
                        bool isFolder = Directory.Exists(txtFolderPath.Text);
                        exportFolder = isFolder ? txtFolderPath.Text : Path.GetDirectoryName(txtFolderPath.Text);
                    }

                    // Ensure export folder exists
                    Directory.CreateDirectory(exportFolder);

                    // Generate unique filename with incrementing suffix if needed
                    string baseFileName = $"BulkEditor_Changelog_{DateTime.Now:yyyyMMdd_HHmmss}";
                    string exportPath = GetUniqueFilePath(exportFolder, baseFileName, ".txt");

                    // Copy the changelog to the export location
                    File.Copy(changelogPath, exportPath, true);

                    // Open in Notepad
                    OpenInNotepad(exportPath);

                    lblStatus.Text = $"Batch changelog exported to: {Path.GetFileName(exportPath)}";
                }
                else
                {
                    MessageBox.Show("No changelog found to export. Please run the tools first.", "No Changelog",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting changelog: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOpenChangelogFolder_Click(object sender, EventArgs e)
        {
            try
            {
                string folderPath = _changelogManager?.GetMainChangelogsFolder() ??
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening changelog folder: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnConfigureReplaceHyperlink_Click(object sender, EventArgs e)
        {
            try
            {
                using var configForm = new ReplaceHyperlinkConfigForm(_hyperlinkReplacementRules);
                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    // Save the updated rules
                    await _hyperlinkReplacementRules.SaveAsync();
                    lblStatus.Text = "Hyperlink replacement rules updated";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error configuring hyperlink replacements: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Gets a unique file path by incrementing suffix if file exists
        /// </summary>
        private string GetUniqueFilePath(string folderPath, string baseFileName, string extension)
        {
            string filePath = Path.Combine(folderPath, $"{baseFileName}{extension}");
            int counter = 1;

            while (File.Exists(filePath))
            {
                filePath = Path.Combine(folderPath, $"{baseFileName}_{counter}{extension}");
                counter++;
            }

            return filePath;
        }

        /// <summary>
        /// Opens a file in Notepad
        /// </summary>
        private void OpenInNotepad(string filePath)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = false
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening file in Notepad: {ex.Message}");
                // Fallback to default file association
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex2)
                {
                    System.Diagnostics.Debug.WriteLine($"Error opening file with default association: {ex2.Message}");
                }
            }
        }
    }
}