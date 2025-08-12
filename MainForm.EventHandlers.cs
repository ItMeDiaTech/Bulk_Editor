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
            _loggingService?.LogUserAction("Select Folder", "User opened folder selection dialog");
            
            using var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Select a folder containing .docx files to process";
            folderDialog.ShowNewFolderButton = false;

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                _loggingService?.LogUserAction("Folder Selected", $"User selected folder: {folderDialog.SelectedPath}");
                txtFolderPath.Text = folderDialog.SelectedPath;
                LoadFiles(folderDialog.SelectedPath);
                ShowFileList();
            }
            else
            {
                _loggingService?.LogDebug("Folder selection cancelled by user");
            }
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            _loggingService?.LogUserAction("Select File", "User opened file selection dialog");
            
            using var fileDialog = new OpenFileDialog();
            fileDialog.Title = "Select a .docx file to process";
            fileDialog.Filter = "Word documents (*.docx)|*.docx";
            fileDialog.Multiselect = false;

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                _loggingService?.LogUserAction("File Selected", $"User selected file: {Path.GetFileName(fileDialog.FileName)}");
                txtFolderPath.Text = fileDialog.FileName;
                LoadFile(fileDialog.FileName);
                ShowFileList();
            }
            else
            {
                _loggingService?.LogDebug("File selection cancelled by user");
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
            _loggingService?.LogUserAction("Open Settings", "User opened settings dialog");
            
            try
            {
                using var settingsForm = new SettingsForm(_appSettings);

                // Apply current theme to settings form
                _themeService?.ApplyTheme(settingsForm);

                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    _loggingService?.LogUserAction("Settings Saved", "User saved settings changes");
                    
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
                            _loggingService?.LogInformation("Settings reloaded and services reinitialized successfully");
                        }
                        catch (Exception ex)
                        {
                            _loggingService?.LogError(ex, "Error reinitializing services after settings change");
                            lblStatus.Text = "Settings updated (theme may require restart)";
                        }
                    }));
                }
                else
                {
                    _loggingService?.LogDebug("Settings dialog cancelled by user");
                }
            }
            catch (Exception ex)
            {
                _loggingService?.LogError(ex, "Error opening settings dialog");
                MessageBox.Show($"Error opening settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddFile_Click(object sender, EventArgs e)
        {
            _loggingService?.LogUserAction("Add Files", "User opened add files dialog");
            
            using var fileDialog = new OpenFileDialog();
            fileDialog.Title = "Select .docx files to add";
            fileDialog.Filter = "Word documents (*.docx)|*.docx";
            fileDialog.Multiselect = true;

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                _loggingService?.LogUserAction("Files Added", $"User selected {fileDialog.FileNames.Length} files to add");
                
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
                
                _loggingService?.LogFileOperation("Add Files", "",
                    $"Added {addedCount} new files, {duplicateCount} duplicates skipped. Total files: {lstFiles.Items.Count}");
                
                ShowFileList();
            }
            else
            {
                _loggingService?.LogDebug("Add files dialog cancelled by user");
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
                    _loggingService?.LogUserAction("Open File Location", "User requested to open file location");
                    
                    string filePath = GetSelectedFilePath();
                    string folderToOpen = null;
                    
                    // Try to get file path from the selected item
                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        folderToOpen = Path.GetDirectoryName(filePath);
                        _loggingService?.LogDebug("Opening file location from selected file path: {FilePath}", filePath);
                    }
                    else
                    {
                        // Fallback: try to determine path from current context
                        if (Directory.Exists(txtFolderPath.Text))
                        {
                            // txtFolderPath contains a folder path
                            folderToOpen = txtFolderPath.Text;
                            _loggingService?.LogDebug("Opening folder from txtFolderPath (directory): {FolderPath}", txtFolderPath.Text);
                        }
                        else if (File.Exists(txtFolderPath.Text))
                        {
                            // txtFolderPath contains a file path
                            folderToOpen = Path.GetDirectoryName(txtFolderPath.Text);
                            _loggingService?.LogDebug("Opening folder from txtFolderPath (file): {FilePath}", txtFolderPath.Text);
                        }
                        else
                        {
                            // Last resort: try to build path from selected item
                            string selectedItem = lstFiles.SelectedItem.ToString();
                            string fileName = selectedItem.Split('(')[0].Trim();
                            
                            if (Directory.Exists(txtFolderPath.Text))
                            {
                                string potentialFilePath = Path.Combine(txtFolderPath.Text, fileName);
                                if (File.Exists(potentialFilePath))
                                {
                                    folderToOpen = txtFolderPath.Text;
                                    _loggingService?.LogDebug("Built path from selected item: {FileName} in {FolderPath}", fileName, txtFolderPath.Text);
                                }
                            }
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(folderToOpen) && Directory.Exists(folderToOpen))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = folderToOpen,
                            UseShellExecute = true
                        });
                        
                        _loggingService?.LogUserAction("File Location Opened", $"Opened folder: {folderToOpen}");
                        lblStatus.Text = $"Opened folder: {Path.GetFileName(folderToOpen)}";
                    }
                    else
                    {
                        _loggingService?.LogWarning("File location not found. FilePath: {FilePath}, TxtFolderPath: {TxtFolderPath}", filePath, txtFolderPath.Text);
                        MessageBox.Show("File location not found. Please ensure a file is selected and the file exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    _loggingService?.LogError(ex, "Error opening file location");
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
                    _loggingService?.LogUserAction("Process Single File", $"User initiated processing of: {Path.GetFileName(filePath)}");
                    
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
                    _loggingService?.LogWarning("Single file processing failed - file not found: {FilePath}", filePath);
                    MessageBox.Show("Selected file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                _loggingService?.LogWarning("Single file processing attempted with no file selected");
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
                _loggingService?.LogUserAction("Export Changelog", "User requested to export batch changelog");
                
                // Try multiple methods to find a changelog
                string changelogPath = null;
                
                // Method 1: Use changelog manager
                if (_changelogManager != null)
                {
                    changelogPath = _changelogManager.FindLatestChangelog(txtFolderPath.Text);
                    if (!string.IsNullOrEmpty(changelogPath) && File.Exists(changelogPath))
                    {
                        _loggingService?.LogDebug("Found changelog via ChangelogManager: {ChangelogPath}", changelogPath);
                    }
                    else
                    {
                        changelogPath = null;
                    }
                }
                
                // Method 2: Fallback to legacy method
                if (string.IsNullOrEmpty(changelogPath))
                {
                    changelogPath = FindLatestChangelog();
                    if (!string.IsNullOrEmpty(changelogPath) && File.Exists(changelogPath))
                    {
                        _loggingService?.LogDebug("Found changelog via legacy method: {ChangelogPath}", changelogPath);
                    }
                    else
                    {
                        changelogPath = null;
                    }
                }
                
                // Method 3: Search in common locations
                if (string.IsNullOrEmpty(changelogPath))
                {
                    var searchPaths = new List<string>();
                    
                    // Add current folder paths
                    if (Directory.Exists(txtFolderPath.Text))
                    {
                        searchPaths.Add(txtFolderPath.Text);
                    }
                    else if (!string.IsNullOrEmpty(txtFolderPath.Text))
                    {
                        var dir = Path.GetDirectoryName(txtFolderPath.Text);
                        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                        {
                            searchPaths.Add(dir);
                        }
                    }
                    
                    // Add centralized storage paths if configured
                    if (_appSettings.ChangelogSettings.UseCentralizedStorage)
                    {
                        try
                        {
                            var combinedPath = _appSettings.ChangelogSettings.GetCombinedChangelogsPath();
                            var datePath = _appSettings.ChangelogSettings.GetDateBasedPath(combinedPath);
                            searchPaths.Add(datePath);
                            searchPaths.Add(combinedPath);
                        }
                        catch { }
                    }
                    
                    // Search for changelog files
                    foreach (var searchPath in searchPaths.Distinct())
                    {
                        if (Directory.Exists(searchPath))
                        {
                            var files = Directory.GetFiles(searchPath, "*Changelog*.txt")
                                .Concat(Directory.GetFiles(searchPath, "BulkEditor_*.txt"))
                                .OrderByDescending(File.GetLastWriteTime)
                                .ToArray();
                            
                            if (files.Length > 0)
                            {
                                changelogPath = files[0];
                                _loggingService?.LogDebug("Found changelog via search: {ChangelogPath}", changelogPath);
                                break;
                            }
                        }
                    }
                }

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
                        // Use document folder or Desktop as fallback
                        if (Directory.Exists(txtFolderPath.Text))
                        {
                            exportFolder = txtFolderPath.Text;
                        }
                        else if (!string.IsNullOrEmpty(txtFolderPath.Text))
                        {
                            exportFolder = Path.GetDirectoryName(txtFolderPath.Text) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        }
                        else
                        {
                            exportFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        }
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

                    _loggingService?.LogUserAction("Changelog Exported", $"Exported changelog to: {exportPath}");
                    lblStatus.Text = $"Batch changelog exported to: {Path.GetFileName(exportPath)}";
                }
                else
                {
                    _loggingService?.LogWarning("No changelog found to export. Searched paths and methods exhausted.");
                    MessageBox.Show("No changelog found to export. Please run the tools first to generate a changelog.", "No Changelog",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _loggingService?.LogError(ex, "Error exporting changelog");
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