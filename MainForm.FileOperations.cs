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
        private void UpdateFileList(IEnumerable<string> filePaths)
        {
            lstFiles.Items.Clear();
            var fileItems = new List<string>();

            foreach (var filePath in filePaths)
            {
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    fileItems.Add($"{fileInfo.Name} ({FormatFileSize(fileInfo.Length)})");
                }
                catch (Exception ex)
                {
                    _loggingService.LogError(ex, "Failed to get file info for: {FilePath}", filePath);
                }
            }

            lstFiles.Items.AddRange(fileItems.ToArray());

            if (_appSettings.UI.AutoSelectFirstFile && lstFiles.Items.Count > 0)
            {
                lstFiles.SelectedIndex = 0;
            }

            lblStatus.Text = $"Loaded {lstFiles.Items.Count} file(s).";
            _loggingService.LogUserAction("Update File List", $"Loaded {lstFiles.Items.Count} file(s).");
        }

        private void LoadFile(string filePath)
        {
            UpdateFileList(new[] { filePath });
            
            // Create file path mapping for single file
            var filePathMap = new Dictionary<int, string>();
            if (lstFiles.Items.Count > 0)
            {
                filePathMap[0] = filePath;
                lstFiles.Tag = filePathMap;
            }
        }

        private void LoadFiles(string folderPath)
        {
            try
            {
                var files = Directory.GetFiles(folderPath, "*.docx");
                UpdateFileList(files);
                
                // Create file path mapping for folder-based files
                var filePathMap = new Dictionary<int, string>();
                for (int i = 0; i < files.Length && i < lstFiles.Items.Count; i++)
                {
                    filePathMap[i] = files[i];
                }
                lstFiles.Tag = filePathMap;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error loading files from folder: {FolderPath}", folderPath);
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
                        _loggingService.LogDebug("Retrieved file path from map: {FilePath}", path);
                        return path;
                    }

                    // Fallback for folder-based selection
                    if (Directory.Exists(txtFolderPath.Text))
                    {
                        string selectedItem = lstFiles.SelectedItem?.ToString() ?? string.Empty;
                        string fileName = selectedItem.Split('(')[0].Trim();
                        var path = Path.Combine(txtFolderPath.Text, fileName);
                        _loggingService.LogDebug("Built file path from folder selection: {FilePath}", path);
                        return path;
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error getting selected file path");
                return string.Empty;
            }
        }

        private void OpenFile(string filePath)
        {
            try
            {
                _loggingService.LogUserAction("Open File", $"Opening file: {Path.GetFileName(filePath)}");
                
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
                
                _loggingService.LogFileOperation("Open", filePath, "File opened successfully");
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error opening file: {FilePath}", filePath);
                MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ProcessSingleFile()
        {
            _loggingService.LogUserAction("Process Single File", "Processing single file from current selection");
            
            try
            {
                // Get the selected file path directly
                string filePath = GetSelectedFilePath();
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    _loggingService.LogWarning("Single file processing requested but file not found: {FilePath}", filePath);
                    MessageBox.Show("Selected file not found or could not be determined.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _loggingService.LogDebug("Processing single file directly: {FilePath}", filePath);
                
                // Store original selection - DON'T change txtFolderPath.Text to avoid affecting other files' changelog detection
                int originalSelectedIndex = lstFiles.SelectedIndex;
                
                // Process the file directly without changing UI state
                await ProcessSingleFileDirectly(filePath);
                
                // Ensure selection is maintained
                if (originalSelectedIndex >= 0 && originalSelectedIndex < lstFiles.Items.Count)
                {
                    lstFiles.SelectedIndex = originalSelectedIndex;
                }
                
                // Force refresh changelog for the current selection after a delay to ensure file is released
                var refreshTimer = new System.Windows.Forms.Timer();
                refreshTimer.Interval = 1500; // Increased delay to ensure file release
                refreshTimer.Tick += (s, timerArgs) =>
                {
                    refreshTimer.Stop();
                    refreshTimer.Dispose();
                    // Trigger changelog refresh for currently selected item
                    LstFiles_SelectedIndexChanged(this, EventArgs.Empty);
                };
                refreshTimer.Start();
                
                _loggingService.LogDebug("Single file processing completed successfully");
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error in single file processing");
                MessageBox.Show($"Error processing file: {ex.Message}", "Processing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private async Task ProcessSingleFileDirectly(string filePath)
        {
            // Create a temporary file list with just this file for processing
            var tempList = new List<string> { filePath };
            
            // Use the existing processing logic but bypass the UI file list manipulation
            var validFiles = await ValidateFilesForProcessing(tempList);
            if (validFiles.Count == 0)
            {
                MessageBox.Show("The selected file failed validation and cannot be processed.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Set up changelog path - use the same logic as the main processing to ensure consistency
            string fileName = Path.GetFileName(filePath);
            string? baseDir = Path.GetDirectoryName(filePath);
            string changelogPath;
            
            // Ensure we use the same changelog detection logic as the main process
            // This prevents conflicts with other files' changelog detection
            if (_changelogManager != null && _appSettings.ChangelogSettings.UseCentralizedStorage)
            {
                // For single file processing, always use individual changelog path to avoid conflicts
                changelogPath = _changelogManager.GetIndividualChangelogPath(fileName);
                if (string.IsNullOrEmpty(changelogPath))
                {
                    string docName = Path.GetFileNameWithoutExtension(filePath);
                    string dateFormat = DateTime.Now.ToString("MMddyyyy");
                    changelogPath = Path.Combine(baseDir ?? string.Empty, $"{docName}_Changelog_{dateFormat}.txt");
                }
            }
            else
            {
                // Use legacy behavior with proper file naming to avoid conflicts
                string docName = Path.GetFileNameWithoutExtension(filePath);
                string dateFormat = DateTime.Now.ToString("MMddyyyy");
                changelogPath = Path.Combine(baseDir ?? string.Empty, $"{docName}_Changelog_{dateFormat}.txt");
            }
            
            // Store the changelog path for consistent detection later
            _loggingService.LogDebug("Single file processing changelog path: {ChangelogPath}", changelogPath);
            
            // Process the single file
            bool appendMode = File.Exists(changelogPath);
            using (var writer = new StreamWriter(changelogPath, append: appendMode))
            {
                if (!appendMode)
                {
                    writer.WriteLine($"Bulk Editor: Single File Processing - {DateTime.Now}");
                    writer.WriteLine($"Version: 2.1");
                    writer.WriteLine($"File: {fileName}");
                    writer.WriteLine();
                }
                else
                {
                    writer.WriteLine();
                    writer.WriteLine($"=== Single File Processing Session - {DateTime.Now} ===");
                    writer.WriteLine($"File: {fileName}");
                    writer.WriteLine();
                }
                
                await ProcessFileWithProgressAsync(filePath, writer, null, 0, 1, CancellationToken.None);
            }
            
            _loggingService.LogDebug("Single file processing completed for: {FileName}", fileName);
        }
    }
}