using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bulk_Editor.Services;

namespace Bulk_Editor
{
    public partial class MainForm
    {
        private void LoadFile(string filePath)
        {
            try
            {
                _loggingService?.LogUserAction("Load File", $"Loading single file: {Path.GetFileName(filePath)}");
                
                lstFiles.Items.Clear();
                FileInfo fileInfo = new(filePath);
                lstFiles.Items.Add($"{fileInfo.Name} ({FormatFileSize(fileInfo.Length)})");

                // Auto-select first file if setting is enabled
                if (_appSettings.UI.AutoSelectFirstFile && lstFiles.Items.Count > 0)
                {
                    lstFiles.SelectedIndex = 0;
                    _loggingService?.LogDebug("Auto-selected first file due to setting");
                }

                lblStatus.Text = $"Loaded file: {filePath}";
                _loggingService?.LogFileOperation("Load", filePath, $"Successfully loaded file ({FormatFileSize(fileInfo.Length)})");
            }
            catch (Exception ex)
            {
                _loggingService?.LogError(ex, "Failed to load file: {FilePath}", filePath);
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFiles(string folderPath)
        {
            try
            {
                _loggingService?.LogUserAction("Load Files", $"Loading files from folder: {folderPath}");
                
                lstFiles.Items.Clear();
                string[] files = Directory.GetFiles(folderPath, "*.docx");

                _loggingService?.LogDebug("Found {FileCount} .docx files in folder: {FolderPath}", files.Length, folderPath);

                foreach (string file in files)
                {
                    FileInfo fileInfo = new(file);
                    lstFiles.Items.Add($"{fileInfo.Name} ({FormatFileSize(fileInfo.Length)})");
                }

                // Auto-select first file if setting is enabled
                if (_appSettings.UI.AutoSelectFirstFile && lstFiles.Items.Count > 0)
                {
                    lstFiles.SelectedIndex = 0;
                    _loggingService?.LogDebug("Auto-selected first file due to setting");
                }

                lblStatus.Text = $"Loaded {files.Length} files from {folderPath}";
                _loggingService?.LogFileOperation("Load Folder", folderPath, $"Successfully loaded {files.Length} files");
            }
            catch (Exception ex)
            {
                _loggingService?.LogError(ex, "Error loading files from folder: {FolderPath}", folderPath);
                MessageBox.Show($"Error loading files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetSelectedFilePath()
        {
            try
            {
                if (lstFiles.SelectedIndex >= 0)
                {
                    var filePathMap = lstFiles.Tag as Dictionary<int, string>;
                    if (filePathMap != null && filePathMap.ContainsKey(lstFiles.SelectedIndex))
                    {
                        var path = filePathMap[lstFiles.SelectedIndex];
                        _loggingService?.LogDebug("Retrieved file path from map: {FilePath}", path);
                        return path;
                    }

                    // Fallback for folder-based selection
                    if (Directory.Exists(txtFolderPath.Text))
                    {
                        string selectedItem = lstFiles.SelectedItem.ToString();
                        string fileName = selectedItem.Split('(')[0].Trim();
                        var path = Path.Combine(txtFolderPath.Text, fileName);
                        _loggingService?.LogDebug("Built file path from folder selection: {FilePath}", path);
                        return path;
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                _loggingService?.LogError(ex, "Error getting selected file path");
                return string.Empty;
            }
        }

        private void OpenFile(string filePath)
        {
            try
            {
                _loggingService?.LogUserAction("Open File", $"Opening file: {Path.GetFileName(filePath)}");
                
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
                
                _loggingService?.LogFileOperation("Open", filePath, "File opened successfully");
            }
            catch (Exception ex)
            {
                _loggingService?.LogError(ex, "Error opening file: {FilePath}", filePath);
                MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ProcessSingleFile()
        {
            _loggingService?.LogUserAction("Process Single File", "Processing single file from current selection");
            
            // Store the original selection and file list state
            int originalSelectedIndex = lstFiles.SelectedIndex;
            var originalItems = new object[lstFiles.Items.Count];
            lstFiles.Items.CopyTo(originalItems, 0);
            var originalTag = lstFiles.Tag;
            var originalFolderPath = txtFolderPath.Text;
            

            try
            {
                // Set up for single file processing
                string filePath = txtFolderPath.Text;
                if (File.Exists(filePath))
                {
                    _loggingService?.LogDebug("Setting up single file processing for: {FilePath}", filePath);
                    
                    // Temporarily clear list and load single file
                    lstFiles.Items.Clear();
                    LoadFile(filePath);

                    // Process the file - this will trigger the automatic refresh timer
                    BtnRunTools_Click(null, null);
                    
                    // Wait a bit for processing to complete
                    await Task.Delay(200);
                }
                else
                {
                    _loggingService?.LogWarning("Single file processing requested but file does not exist: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _loggingService?.LogError(ex, "Error in single file processing");
            }
            finally
            {
                // Always restore the original state
                // Restore folder path first
                txtFolderPath.Text = originalFolderPath;
                
                // Restore original list and selection
                lstFiles.Items.Clear();
                lstFiles.Items.AddRange(originalItems);
                lstFiles.Tag = originalTag;
                
                // Restore the original selection immediately
                if (originalSelectedIndex >= 0 && originalSelectedIndex < lstFiles.Items.Count)
                {
                    lstFiles.SelectedIndex = originalSelectedIndex;
                    
                    // Trigger changelog refresh manually after a delay to avoid conflicts
                    await Task.Delay(100);
                    
                    // Force a manual changelog refresh for the restored selection
                    LstFiles_SelectedIndexChanged(null, null);
                }
                
                _loggingService?.LogDebug("Restored original file list and selection after single file processing");
            }
        }

        private static string FindLatestChangelog()
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string changelogPath = Path.Combine(desktopPath, "BulkEditor_Changelog.txt");
                bool exists = File.Exists(changelogPath);
                
                Services.LoggingService.Instance.LogDebug("Searching for latest changelog. Path: {ChangelogPath}, Exists: {Exists}",
                    changelogPath, exists);
                
                return exists ? changelogPath : string.Empty;
            }
            catch (Exception ex)
            {
                Services.LoggingService.Instance.LogError(ex, "Error searching for latest changelog");
                return string.Empty;
            }
        }
    }
}