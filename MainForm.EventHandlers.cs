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
            _loggingService.LogUserAction("Select Folder", "User opened folder selection dialog");
            
            using var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Select a folder containing .docx files to process";
            folderDialog.ShowNewFolderButton = false;

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                _loggingService.LogUserAction("Folder Selected", $"User selected folder: {folderDialog.SelectedPath}");
                txtFolderPath.Text = folderDialog.SelectedPath;
                LoadFiles(folderDialog.SelectedPath);
                UpdatePanelVisibility();
            }
            else
            {
                _loggingService.LogDebug("Folder selection cancelled by user");
            }
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            _loggingService.LogUserAction("Select File", "User opened file selection dialog");
            
            using var fileDialog = new OpenFileDialog();
            fileDialog.Title = "Select a .docx file to process";
            fileDialog.Filter = "Word documents (*.docx)|*.docx";
            fileDialog.Multiselect = false;

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                _loggingService.LogUserAction("File Selected", $"User selected file: {Path.GetFileName(fileDialog.FileName)}");
                txtFolderPath.Text = fileDialog.FileName;
                LoadFile(fileDialog.FileName);
                UpdatePanelVisibility();
            }
            else
            {
                _loggingService.LogDebug("File selection cancelled by user");
            }
        }

        private async void LstFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndex >= 0)
            {
                string selectedItem = lstFiles.SelectedItem?.ToString() ?? string.Empty;
                string fileName = selectedItem.Split('(')[0].Trim();

                string changelogPath = _changelogManager?.FindLatestChangelog(txtFolderPath.Text) ?? string.Empty;

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
                string selectedItem = lstFiles.SelectedItem?.ToString() ?? string.Empty;
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
            _loggingService.LogUserAction("Open Settings", "User opened settings dialog");
            
            try
            {
                using var settingsForm = new SettingsForm(_settingsService, _loggingService);
                
                // Apply theme to settings form safely
                try
                {
                    _themeService?.ApplyTheme(settingsForm);
                }
                catch (Exception themeEx)
                {
                    _loggingService.LogWarning(themeEx, "Error applying theme to settings form, continuing without theme");
                }

                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        await _settingsService.LoadSettingsAsync();
                        _appSettings = _settingsService.Settings;
                        
                        // Safely reinitialize services and reapply theme
                        try
                        {
                            // Reapply theme with updated settings
                            _themeService?.ApplyTheme(this);
                            
                            // Update any other settings-dependent components
                            _windowStateService?.RestoreWindowState(this);
                            
                            // Force a complete repaint to ensure theme changes are visible
                            this.Invalidate(true);
                            this.Update();
                            
                            lblStatus.Text = "Settings updated successfully";
                            _loggingService.LogInformation("Settings reloaded and theme reapplied successfully");
                        }
                        catch (Exception serviceEx)
                        {
                            _loggingService.LogError(serviceEx, "Error reinitializing services after settings update");
                            lblStatus.Text = "Settings updated (with minor display issues)";
                            
                            // Try a simple refresh as fallback
                            try
                            {
                                this.Refresh();
                            }
                            catch (Exception refreshEx)
                            {
                                _loggingService.LogError(refreshEx, "Error refreshing form after settings update");
                            }
                        }
                    }
                    catch (Exception settingsEx)
                    {
                        _loggingService.LogError(settingsEx, "Error reloading settings after save");
                        MessageBox.Show($"Settings may not have been fully applied: {settingsEx.Message}",
                            "Settings Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    _loggingService.LogDebug("Settings dialog cancelled by user");
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error opening settings dialog");
                MessageBox.Show($"Error opening settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddFile_Click(object sender, EventArgs e)
        {
            _loggingService.LogUserAction("Add Files", "User opened add files dialog");
            
            using var fileDialog = new OpenFileDialog();
            fileDialog.Title = "Select .docx files to add";
            fileDialog.Filter = "Word documents (*.docx)|*.docx";
            fileDialog.Multiselect = true;

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                _loggingService.LogUserAction("Files Added", $"User selected {fileDialog.FileNames.Length} files to add");
                
                var filePathMap = lstFiles.Tag as Dictionary<int, string> ?? new Dictionary<int, string>();
                int addedCount = 0;
                int duplicateCount = 0;

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
                            duplicateCount++;
                            break;
                        }
                    }

                    if (!alreadyExists)
                    {
                        int index = lstFiles.Items.Add(displayName);
                        filePathMap[index] = fileName;
                        addedCount++;
                    }
                }

                lstFiles.Tag = filePathMap;
                lblStatus.Text = $"Added files. Total: {lstFiles.Items.Count} files";
                
                _loggingService.LogFileOperation("Add Files", "",
                    $"Added {addedCount} new files, {duplicateCount} duplicates skipped. Total files: {lstFiles.Items.Count}");
                
                UpdatePanelVisibility();
            }
            else
            {
                _loggingService.LogDebug("Add files dialog cancelled by user");
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
            if (lstFiles.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a file first.", "No File Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                _loggingService.LogUserAction("Open File Location", "User requested to open file location");
                
                string filePath = GetSelectedFilePath();
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    _loggingService.LogWarning("File location not found for selected item. Path was '{FilePath}'", filePath);
                    MessageBox.Show("Could not determine the file's location. The file may have been moved or deleted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string? folderToOpen = Path.GetDirectoryName(filePath);
                if (string.IsNullOrEmpty(folderToOpen) || !Directory.Exists(folderToOpen))
                {
                    _loggingService.LogError(new DirectoryNotFoundException($"Directory for file '{filePath}' not found."), "Error opening file location");
                    MessageBox.Show($"The directory for the selected file could not be found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = folderToOpen,
                    UseShellExecute = true
                });
                
                _loggingService.LogUserAction("File Location Opened", $"Opened folder: {folderToOpen}");
                lblStatus.Text = $"Opened folder: {Path.GetFileName(folderToOpen)}";
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error opening file location");
                MessageBox.Show($"Error opening file location: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    _loggingService.LogUserAction("Process Single File", $"User initiated processing of: {Path.GetFileName(filePath)}");
                    
                    // Process the single file directly without changing UI state
                    ProcessSingleFile();
                }
                else
                {
                    _loggingService.LogWarning("Single file processing failed - file not found: {FilePath}", filePath);
                    MessageBox.Show("Selected file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                _loggingService.LogWarning("Single file processing attempted with no file selected");
                MessageBox.Show("Please select a file to process.", "No File Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnExportSingleChangelog_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndex >= 0)
            {
                try
                {
                    string selectedItem = lstFiles.SelectedItem?.ToString() ?? string.Empty;
                    string fileName = selectedItem.Split('(')[0].Trim();

                    // Determine save location based on settings
                    string exportFolder;
                    if (_appSettings.ChangelogSettings.UseCentralizedStorage)
                    {
                        var basePath = _changelogManager?.GetIndividualChangelogPath(fileName);
                        exportFolder = Path.GetDirectoryName(basePath ?? string.Empty) ?? string.Empty;
                    }
                    else
                    {
                        // Use document folder
                        bool isFolder = Directory.Exists(txtFolderPath.Text);
                        exportFolder = isFolder ? txtFolderPath.Text : (Path.GetDirectoryName(txtFolderPath.Text) ?? string.Empty);
                    }

                    // Ensure export folder exists
                    if (!string.IsNullOrEmpty(exportFolder))
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
                _loggingService.LogUserAction("Export Batch Changelog", "User requested to export batch changelog");

                string changelogPath = _changelogManager.FindLatestChangelog(txtFolderPath.Text);

                if (string.IsNullOrEmpty(changelogPath) || !File.Exists(changelogPath))
                {
                    _loggingService.LogWarning("No changelog found to export for path: {Path}", txtFolderPath.Text);
                    MessageBox.Show("No changelog found to export. Please run the tools first to generate a changelog.", "No Changelog Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string exportFolder;
                if (_appSettings.ChangelogSettings.UseCentralizedStorage)
                {
                    var basePath = _changelogManager.GetCombinedChangelogPath();
                    exportFolder = Path.GetDirectoryName(basePath ?? string.Empty) ?? string.Empty;
                }
                else
                {
                    exportFolder = Directory.Exists(txtFolderPath.Text)
                        ? txtFolderPath.Text
                        : Path.GetDirectoryName(txtFolderPath.Text) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }

                if (!string.IsNullOrEmpty(exportFolder))
                    Directory.CreateDirectory(exportFolder);

                string baseFileName = $"BulkEditor_Changelog_{DateTime.Now:yyyyMMdd_HHmmss}";
                string exportPath = GetUniqueFilePath(exportFolder, baseFileName, ".txt");

                File.Copy(changelogPath, exportPath, true);
                OpenInNotepad(exportPath);

                _loggingService.LogUserAction("Batch Changelog Exported", $"Exported '{changelogPath}' to '{exportPath}'");
                lblStatus.Text = $"Batch changelog exported to: {Path.GetFileName(exportPath)}";
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error exporting batch changelog");
                MessageBox.Show($"Error exporting changelog: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOpenChangelogFolder_Click(object sender, EventArgs e)
        {
            try
            {
                _loggingService.LogUserAction("Open Changelog Folder", "User requested to open the main changelogs folder");
                
                string folderPath = _changelogManager.GetMainChangelogsFolder();
                Directory.CreateDirectory(folderPath);

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true
                });
                
                _loggingService.LogDebug("Opened changelog folder at {Path}", folderPath);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error opening changelog folder");
                MessageBox.Show($"Error opening changelog folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnConfigureReplaceHyperlink_Click(object sender, EventArgs e)
        {
            _loggingService.LogUserAction("Configure Hyperlink Replacements", "User opened hyperlink replacement configuration");
            try
            {
                using var configForm = new ReplaceHyperlinkConfigForm(_hyperlinkReplacementRules!);
                _themeService.ApplyTheme(configForm);
                
                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    await _hyperlinkReplacementRules!.SaveAsync();
                    lblStatus.Text = "Hyperlink replacement rules updated";
                    _loggingService.LogInformation("Hyperlink replacement rules saved successfully.");
                }
                else
                {
                    _loggingService.LogDebug("Hyperlink replacement configuration cancelled by user.");
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error saving hyperlink replacement rules");
                MessageBox.Show($"Error configuring hyperlink replacements: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Gets a unique file path by incrementing suffix if file exists
        /// </summary>
        private static string GetUniqueFilePath(string folderPath, string baseFileName, string extension)
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
        private static void OpenInNotepad(string filePath)
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