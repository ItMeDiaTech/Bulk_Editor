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
        private readonly WordDocumentProcessor _processor;
        private AppSettings _appSettings;

        // Progress reporting and cancellation
        private CancellationTokenSource _cancellationTokenSource;
        private IProgress<ProgressReport> _progressReporter;

        // Button state management
        private string _originalButtonText;
        private Color _originalButtonColor;

        public MainForm()
        {
            InitializeComponent();
            _processor = new WordDocumentProcessor();
            SetupProgressReporting();
            SetupCheckboxDependencies();
            SetupFileListHandlers();
            LoadConfigurationAsync();
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
        }

        private void SetupFileListHandlers()
        {
            lstFiles.SelectedIndexChanged += LstFiles_SelectedIndexChanged;
            lstFiles.DoubleClick += LstFiles_DoubleClick;
        }

        private async void LstFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndex >= 0)
            {
                string selectedItem = lstFiles.SelectedItem.ToString();
                string fileName = selectedItem.Split('(')[0].Trim();

                string changelogPath = FindLatestChangelog();

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

        private void OpenFile(string filePath)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DisplayChangelogForFileAsync(string changelogPath, string fileName)
        {
            // Retry logic with progressive delays to handle file locking
            int maxRetries = 3;
            int retryDelay = 250; // Start with 250ms delay

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    string[] changelogLines = await File.ReadAllLinesAsync(changelogPath);
                    bool foundFileSection = false;
                    var fileChangelog = new System.Text.StringBuilder();

                    foreach (string line in changelogLines)
                    {
                        // Look for the document processing section using the actual pattern written
                        if (line.Equals($"Title: {Path.GetFileNameWithoutExtension(fileName)}", StringComparison.OrdinalIgnoreCase))
                        {
                            foundFileSection = true;
                            fileChangelog.AppendLine(line); // Include the title line
                            continue;
                        }
                        else if (foundFileSection)
                        {
                            // Stop when we hit the next file section
                            if (line.StartsWith("Title:") && !line.Contains(Path.GetFileNameWithoutExtension(fileName)))
                            {
                                break;
                            }

                            // Filter out summary line that should only appear in exported files
                            if (line.StartsWith("Processed ") && line.EndsWith(" files."))
                            {
                                continue; // Skip this line in UI display
                            }

                            // Filter out document separator lines from UI display
                            if (line.Trim() == "__________")
                            {
                                continue; // Skip separator lines in UI display
                            }

                            // Include all other content for this file, including empty lines to preserve formatting
                            fileChangelog.AppendLine(line);
                        }
                    }

                    if (fileChangelog.Length > 0)
                    {
                        lblChangelogTitle.Text = $"Changelog - {fileName}";
                        txtChangelog.Text = fileChangelog.ToString().TrimEnd();
                        ShowChangelog();
                    }
                    else
                    {
                        lblChangelogTitle.Text = "Changelog";
                        txtChangelog.Text = $"No changelog available for {fileName}";
                        ShowChangelog();
                    }

                    // Success - exit retry loop
                    return;
                }
                catch (IOException ex) when (ex.Message.Contains("being used by another process"))
                {
                    // File is locked, wait and retry
                    if (attempt < maxRetries - 1)
                    {
                        await Task.Delay(retryDelay); // Non-blocking delay
                        retryDelay *= 2; // Double the delay for next retry
                        continue;
                    }

                    // Final attempt failed - show temporary message without error dialog
                    lblChangelogTitle.Text = $"Changelog - {fileName}";
                    txtChangelog.Text = "Changelog is being updated... Please wait a moment and try selecting the file again.";
                    ShowChangelog();
                    return;
                }
                catch (Exception ex)
                {
                    // Other errors - show error message only on final attempt
                    if (attempt == maxRetries - 1)
                    {
                        MessageBox.Show($"Error reading changelog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return;
                }
            }
        }

        private void SetupCheckboxDependencies()
        {
            UpdateSubCheckboxStates();
            chkFixSourceHyperlinks.CheckedChanged += ChkFixSourceHyperlinks_CheckedChanged;
        }

        private void ChkFixSourceHyperlinks_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSubCheckboxStates();
        }

        private void UpdateSubCheckboxStates()
        {
            chkAppendContentID.Enabled = chkFixSourceHyperlinks.Checked;
            chkFixTitles.Enabled = chkFixSourceHyperlinks.Checked;

            chkAppendContentID.ForeColor = chkFixSourceHyperlinks.Checked ?
                SystemColors.ControlText : SystemColors.ControlDark;
            chkFixTitles.ForeColor = chkFixSourceHyperlinks.Checked ?
                SystemColors.ControlText : SystemColors.ControlDark;

            if (!chkFixSourceHyperlinks.Checked)
            {
                chkAppendContentID.Checked = false;
                chkFixTitles.Checked = false;
            }
        }

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

        private void ShowFileList()
        {
            lstFiles.Visible = true;
            pnlChangelog.Visible = true;
        }

        private void ShowChangelog()
        {
            lstFiles.Visible = true;
            pnlChangelog.Visible = true;
        }

        private async void BtnRunTools_Click(object sender, EventArgs e)
        {
            // Check if this is a cancellation request
            if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                lblStatus.Text = "Cancelling processing...";
                return;
            }

            if (string.IsNullOrEmpty(txtFolderPath.Text))
            {
                MessageBox.Show("Please select a file or folder first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool isFolder = Directory.Exists(txtFolderPath.Text);
            string path = txtFolderPath.Text;
            string basePath = isFolder ? path : Path.GetDirectoryName(path);

            // Store the originally selected index to preserve user's selection
            int originalSelectedIndex = lstFiles.SelectedIndex;

            // Create new cancellation token for this operation
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting processing: {path}");

                // Transform button for cancellation
                TransformButtonForProcessing();

                _hyperlinkReplacementRules.TrimRules();

                string backupPath = Path.Combine(basePath, "Backup");
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string changelogPath;

                // Use document filename for single file processing
                if (!isFolder && File.Exists(path))
                {
                    string docName = Path.GetFileNameWithoutExtension(path);
                    string dateFormat = DateTime.Now.ToString("MMddyyyy");
                    changelogPath = Path.Combine(basePath, $"{docName}_Changelog_{dateFormat}.txt");
                }
                else
                {
                    changelogPath = Path.Combine(basePath, $"BulkEditor_Changelog_{timestamp}.txt");
                }

                ShowProgress(true);

                using (var writer = new StreamWriter(changelogPath, append: false))
                {
                    writer.WriteLine($"Bulk Editor: Changelog - {DateTime.Now}");
                    writer.WriteLine($"Version: 2.1");

                    // Check for updates
                    if (_processor.NeedsUpdate)
                    {
                        writer.WriteLine("***New Update Available***");
                        writer.WriteLine("Please download and update as time allows.");
                    }
                    else
                    {
                        writer.WriteLine("Up to Date");
                    }

                    writer.WriteLine();
                    writer.WriteLine($"Path: {path}");

                    // Handle both folder-based files and individually added files
                    List<string> filesToProcess = new List<string>();

                    // Only write summary for multiple files
                    if (filesToProcess.Count > 1)
                    {
                        writer.WriteLine($"Processed {filesToProcess.Count} files.");
                    }

                    if (isFolder)
                    {
                        // Add all .docx files from the selected folder
                        string[] folderFiles = Directory.GetFiles(path, "*.docx");
                        filesToProcess.AddRange(folderFiles);
                    }
                    else if (File.Exists(path))
                    {
                        // Single file selected via file dialog
                        filesToProcess.Add(path);
                    }

                    // Add any individually added files from the file list
                    if (lstFiles.Tag is Dictionary<int, string> filePathMap)
                    {
                        foreach (var kvp in filePathMap)
                        {
                            string individualFile = kvp.Value;
                            if (File.Exists(individualFile) && !filesToProcess.Contains(individualFile))
                            {
                                filesToProcess.Add(individualFile);
                            }
                        }
                    }

                    // Process all files
                    for (int i = 0; i < filesToProcess.Count; i++)
                    {
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                        var fileName = Path.GetFileName(filesToProcess[i]);
                        _progressReporter.Report(ProgressReport.CreateFileProgress(i + 1, filesToProcess.Count, fileName));

                        await ProcessFileWithProgress(filesToProcess[i], writer, i, filesToProcess.Count, _cancellationTokenSource.Token);
                    }
                }

                _progressReporter.Report(ProgressReport.CreateStatus("Processing complete!", 100));

                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Finished processing: {path}");

                // Restore original selection or set to first item if nothing was selected
                if (lstFiles.Items.Count > 0)
                {
                    if (originalSelectedIndex >= 0 && originalSelectedIndex < lstFiles.Items.Count)
                    {
                        // Restore the originally selected item
                        lstFiles.SelectedIndex = originalSelectedIndex;
                    }
                    else
                    {
                        // No item was originally selected, default to first item
                        lstFiles.SelectedIndex = 0;
                    }
                }

                // Handle changelog display based on checkbox
                if (chkOpenChangelogAfterUpdates.Checked)
                {
                    if (lstFiles.Items.Count == 1)
                    {
                        // Single document: show individual changelog in the UI panel
                        string fileName = lstFiles.Items[0].ToString().Split('(')[0].Trim();
                        await DisplayChangelogForFileAsync(changelogPath, fileName);
                    }
                    else if (lstFiles.Items.Count > 1)
                    {
                        // Multiple documents: auto-open the batch changelog file
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = changelogPath,
                            UseShellExecute = true
                        });
                    }
                }

                // Force refresh the changelog display for the currently selected item with a delay
                // This ensures the changelog updates even if the same item was already selected
                // The delay allows the file system to fully release the changelog file lock
                if (lstFiles.SelectedIndex >= 0)
                {
                    var refreshTimer = new System.Windows.Forms.Timer();
                    refreshTimer.Interval = 1000; // 1 second delay to ensure file is fully released
                    refreshTimer.Tick += (s, e) =>
                    {
                        refreshTimer.Stop();
                        refreshTimer.Dispose();
                        LstFiles_SelectedIndexChanged(null, null);
                    };
                    refreshTimer.Start();
                }
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Processing cancelled by user: {path}");
                _progressReporter.Report(ProgressReport.CreateStatus("Processing cancelled by user."));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Error processing: {path} - {ex.Message}");
                _progressReporter.Report(ProgressReport.CreateStatus($"Error: {ex.Message}"));
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Always restore button state
                RestoreButtonFromProcessing();
                ShowProgress(false);

                // Clean up cancellation token
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void TransformButtonForProcessing()
        {
            btnRunTools.Text = "❌ Cancel Processing";
            btnRunTools.BackColor = Color.FromArgb(220, 53, 69); // Bootstrap danger red
        }

        private void RestoreButtonFromProcessing()
        {
            btnRunTools.Text = _originalButtonText;
            btnRunTools.BackColor = _originalButtonColor;
        }

        private void UpdateProgressUI(ProgressReport report)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<ProgressReport>(UpdateProgressUI), report);
                return;
            }

            // Update progress bar
            if (report.PercentComplete >= 0)
            {
                progressBar.Value = Math.Min(report.PercentComplete, progressBar.Maximum);
            }

            // Update status label
            if (!string.IsNullOrEmpty(report.StatusMessage))
            {
                lblStatus.Text = report.StatusMessage;
            }

            // Handle retry notifications with visual feedback
            if (report.IsRetryNotification)
            {
                lblStatus.ForeColor = Color.FromArgb(255, 193, 7); // Warning yellow

                // Reset color after delay
                var timer = new System.Windows.Forms.Timer();
                timer.Interval = 2000; // 2 seconds
                timer.Tick += (s, e) =>
                {
                    lblStatus.ForeColor = SystemColors.ControlText;
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
        }

        private void ShowProgress(bool show)
        {
            progressBar.Visible = show;
            if (show)
            {
                progressBar.Value = 0;
                progressBar.Maximum = 100;
            }
        }

        private void UpdateProgress(int value)
        {
            progressBar.Value = Math.Min(value, progressBar.Maximum);
            // Removed Application.DoEvents() - now using proper Progress<T> pattern
        }

        private void LoadFile(string filePath)
        {
            try
            {
                lstFiles.Items.Clear();
                FileInfo fileInfo = new(filePath);
                lstFiles.Items.Add($"{fileInfo.Name} ({FormatFileSize(fileInfo.Length)})");
                lblStatus.Text = $"Loaded file: {filePath}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ProcessFileWithProgress(string filePath, StreamWriter logWriter, int fileIndex, int totalFiles, CancellationToken cancellationToken)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting processing file: {filePath}");

                string backupPath = Path.Combine(Path.GetDirectoryName(filePath), "Backup", Path.GetFileName(filePath));
                File.Copy(filePath, backupPath, true);

                string fileContent = File.ReadAllText(filePath);
                string originalContent = fileContent;
                List<string> changes = new();

                logWriter.WriteLine();
                logWriter.WriteLine("__________");
                logWriter.WriteLine();
                logWriter.WriteLine($"Title: {Path.GetFileNameWithoutExtension(filePath)}");
                logWriter.WriteLine("Backup File Created");
                logWriter.WriteLine();

                var hyperlinks = WordDocumentProcessor.ExtractHyperlinks(filePath);

                var updatedLinks = new Collection<string>();
                var notFoundLinks = new Collection<string>();
                var expiredLinks = new Collection<string>();
                var errorLinks = new Collection<string>();
                var updatedUrls = new Collection<string>();
                var replacedHyperlinks = new Collection<string>();

                if (chkFixSourceHyperlinks.Checked)
                {
                    var retryService = new RetryPolicyService(_appSettings.RetrySettings, _progressReporter);
                    fileContent = await ProcessingService.FixSourceHyperlinksWithProgress(
                        fileContent, hyperlinks, _processor, changes, updatedLinks, notFoundLinks,
                        expiredLinks, errorLinks, updatedUrls, retryService, _progressReporter, cancellationToken);
                }

                if (chkAppendContentID.Checked)
                {
                    fileContent = ProcessingService.AppendContentID(fileContent, hyperlinks, changes);
                }

                if (chkFixInternalHyperlink.Checked)
                {
                    fileContent = ProcessingService.FixInternalHyperlink(fileContent, hyperlinks, changes);
                }

                if (chkFixTitles.Checked)
                {
                    fileContent = ProcessingService.FixTitles(fileContent, hyperlinks, changes);
                }

                if (chkFixDoubleSpaces.Checked)
                {
                    fileContent = ProcessingService.FixDoubleSpaces(fileContent, changes);
                }

                if (chkReplaceHyperlink.Checked)
                {
                    fileContent = ProcessingService.ReplaceHyperlinks(fileContent, hyperlinks, _hyperlinkReplacementRules, changes, replacedHyperlinks);
                }

                if (changes.Count > 0 && fileContent != originalContent)
                {
                    File.WriteAllText(filePath, fileContent);
                    logWriter.WriteLine($"  Changes made:");
                    foreach (string change in changes)
                    {
                        logWriter.WriteLine($"    - {change}");
                    }
                }

                WriteDetailedChangelog(logWriter, updatedLinks, notFoundLinks, expiredLinks, errorLinks, updatedUrls, replacedHyperlinks, _processor);
                WriteDetailedChangelogToDownloads(updatedLinks, notFoundLinks, expiredLinks, errorLinks, updatedUrls, replacedHyperlinks, _processor);

                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Finished processing file: {filePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Error processing file: {filePath} - {ex.Message}");
                logWriter.WriteLine($"  Error processing file: {ex.Message}");
            }
        }

        private void WriteDetailedChangelog(StreamWriter writer, Collection<string> updatedLinks, Collection<string> notFoundLinks,
            Collection<string> expiredLinks, Collection<string> errorLinks, Collection<string> updatedUrls,
            Collection<string> replacedHyperlinks, WordDocumentProcessor processor)
        {
            writer.WriteLine($"Changes:");
            writer.WriteLine($"  Updated Links ({updatedLinks.Count}):");
            if (updatedLinks.Count > 0)
            {
                foreach (var link in updatedLinks)
                {
                    writer.WriteLine($"    {link}");
                }
            }

            writer.WriteLine();
            writer.WriteLine($"  Not Found ({notFoundLinks.Count}):");
            if (notFoundLinks.Count > 0)
            {
                foreach (var link in notFoundLinks)
                {
                    writer.WriteLine($"    {link}");
                }
            }

            writer.WriteLine();
            writer.WriteLine($"  Found Expired ({expiredLinks.Count}):");
            if (expiredLinks.Count > 0)
            {
                foreach (var link in expiredLinks)
                {
                    writer.WriteLine($"    {link}");
                }
            }

            writer.WriteLine();
            writer.WriteLine($"  Found Error ({errorLinks.Count}):");
            if (errorLinks.Count > 0)
            {
                foreach (var link in errorLinks)
                {
                    writer.WriteLine($"    {link}");
                }
            }

            writer.WriteLine();
            writer.WriteLine($"  Potential Title Change ({updatedUrls.Count}):");
            if (updatedUrls.Count > 0)
            {
                foreach (var url in updatedUrls)
                {
                    writer.WriteLine($"    {url}");
                }
            }

            writer.WriteLine();
            writer.WriteLine($"  Replaced Hyperlinks ({replacedHyperlinks.Count}):");
            if (replacedHyperlinks.Count > 0)
            {
                foreach (var hyperlink in replacedHyperlinks)
                {
                    writer.WriteLine($"    {hyperlink}");
                }
            }
        }

        private void WriteDetailedChangelogToDownloads(Collection<string> updatedLinks, Collection<string> notFoundLinks,
            Collection<string> expiredLinks, Collection<string> errorLinks, Collection<string> updatedUrls,
            Collection<string> replacedHyperlinks, WordDocumentProcessor processor)
        {
            try
            {
                string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (!Directory.Exists(downloadsFolder))
                {
                    downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }

                string baseFileName = "BulkEditor_Changelog";
                string fileExtension = ".txt";
                string changelogPath = Path.Combine(downloadsFolder, baseFileName + fileExtension);
                int counter = 1;

                while (File.Exists(changelogPath))
                {
                    changelogPath = Path.Combine(downloadsFolder, $"{baseFileName}_{counter}{fileExtension}");
                    counter++;
                }

                using (StreamWriter writer = new StreamWriter(changelogPath, false))
                {
                    writer.WriteLine($"Bulk Editor: Changelog - {DateTime.Now}");
                    writer.WriteLine($"Version: 2.1");

                    if (processor.NeedsUpdate)
                    {
                        writer.WriteLine("***New Update Available***");
                        writer.WriteLine("Please download and update as time allows.");
                    }
                    else
                    {
                        writer.WriteLine("Up to Date");
                    }

                    writer.WriteLine();
                    writer.WriteLine($"Updated Links ({updatedLinks.Count}):");
                    foreach (var link in updatedLinks)
                    {
                        writer.WriteLine($"{link}");
                    }

                    writer.WriteLine();
                    writer.WriteLine($"Found Expired ({expiredLinks.Count}):");
                    foreach (var link in expiredLinks)
                    {
                        writer.WriteLine($"{link}");
                    }

                    writer.WriteLine();
                    writer.WriteLine($"Not Found ({notFoundLinks.Count}):");
                    foreach (var link in notFoundLinks)
                    {
                        writer.WriteLine($"{link}");
                    }

                    writer.WriteLine();
                    writer.WriteLine($"Found Error ({errorLinks.Count}):");
                    foreach (var link in errorLinks)
                    {
                        writer.WriteLine($"{link}");
                    }

                    writer.WriteLine();
                    writer.WriteLine($"Potential Title Change ({updatedUrls.Count}):");
                    foreach (var url in updatedUrls)
                    {
                        writer.WriteLine($"{url}");
                    }

                    writer.WriteLine();
                    writer.WriteLine($"Replaced Hyperlinks ({replacedHyperlinks.Count}):");
                    foreach (var hyperlink in replacedHyperlinks)
                    {
                        writer.WriteLine($"{hyperlink}");
                    }
                }

                lblStatus.Text = $"Changelog saved to: {changelogPath}";

                // No automatic prompts or opening - this is handled by the main processing logic
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changelog to Downloads folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            LstFiles_SelectedIndexChanged(null, null); // Refresh changelog display in UI
        }

        private void LoadFiles(string folderPath)
        {
            try
            {
                lstFiles.Items.Clear();

                if (Directory.Exists(folderPath))
                {
                    string[] files = Directory.GetFiles(folderPath, "*.docx");

                    foreach (string file in files)
                    {
                        FileInfo fileInfo = new(file);
                        lstFiles.Items.Add($"{fileInfo.Name} ({FormatFileSize(fileInfo.Length)})");
                    }

                    lblStatus.Text = $"Loaded {files.Length} .docx files from {folderPath}";
                }
                else
                {
                    MessageBox.Show("The selected folder does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }

        private string FindLatestChangelog()
        {
            if (!string.IsNullOrEmpty(txtFolderPath.Text))
            {
                bool isFolder = Directory.Exists(txtFolderPath.Text);
                string basePath = isFolder ? txtFolderPath.Text : Path.GetDirectoryName(txtFolderPath.Text);

                var allChangelogFiles = new List<string>();

                // Look for both old format and new format changelog files
                var bulkEditorFiles = Directory.GetFiles(basePath, "BulkEditor_Changelog_*.txt");
                allChangelogFiles.AddRange(bulkEditorFiles);

                // Look for single document changelog files (new format)
                var singleDocFiles = Directory.GetFiles(basePath, "*_Changelog_*.txt")
                    .Where(f => !Path.GetFileName(f).StartsWith("BulkEditor_"));
                allChangelogFiles.AddRange(singleDocFiles);

                if (allChangelogFiles.Count > 0)
                {
                    // Sort by file creation time to get the most recent
                    allChangelogFiles.Sort((x, y) => File.GetCreationTime(y).CompareTo(File.GetCreationTime(x)));
                    return allChangelogFiles[0];
                }
            }

            string desktopChangelogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BulkEditor_Changelog.txt");
            if (File.Exists(desktopChangelogPath))
            {
                return desktopChangelogPath;
            }

            return string.Empty;
        }

        private async void BtnConfigureReplaceHyperlink_Click(object sender, EventArgs e)
        {
            using var configForm = new ReplaceHyperlinkConfigForm(_hyperlinkReplacementRules);
            if (configForm.ShowDialog() == DialogResult.OK)
            {
                await _hyperlinkReplacementRules.SaveAsync();
            }
        }

        private void BtnExportChangelog_Click(object sender, EventArgs e)
        {
            try
            {
                string changelogPath = FindLatestChangelog();
                if (string.IsNullOrEmpty(changelogPath) || !File.Exists(changelogPath))
                {
                    MessageBox.Show("No changelog file found. Please run the tools first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (!Directory.Exists(downloadsFolder))
                {
                    downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }

                string baseFileName = "BulkEditor_Combined_Changelog";
                string fileExtension = ".txt";
                string exportPath = Path.Combine(downloadsFolder, baseFileName + fileExtension);
                int counter = 1;

                while (File.Exists(exportPath))
                {
                    exportPath = Path.Combine(downloadsFolder, $"{baseFileName}_{counter}{fileExtension}");
                    counter++;
                }

                // Read the complete changelog and export it
                string completeChangelog = File.ReadAllText(changelogPath);
                File.WriteAllText(exportPath, completeChangelog);

                lblStatus.Text = $"Combined changelog exported to: {exportPath}";

                var result = MessageBox.Show("Combined changelog exported successfully to Downloads folder. Would you like to open it?", "Export Complete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = exportPath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting combined changelog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportSingleChangelog_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtChangelog.Text) || txtChangelog.Text.Contains("No changelog file found"))
                {
                    MessageBox.Show("No changelog data to export. Please select a file first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (!Directory.Exists(downloadsFolder))
                {
                    downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }

                string selectedFileName = "";
                if (lstFiles.SelectedIndex >= 0)
                {
                    selectedFileName = lstFiles.SelectedItem.ToString().Split('(')[0].Trim();
                    selectedFileName = Path.GetFileNameWithoutExtension(selectedFileName);
                }

                string baseFileName = string.IsNullOrEmpty(selectedFileName) ? "BulkEditor_Single_Changelog" : $"BulkEditor_{selectedFileName}_Changelog";
                string fileExtension = ".txt";
                string exportPath = Path.Combine(downloadsFolder, baseFileName + fileExtension);
                int counter = 1;

                while (File.Exists(exportPath))
                {
                    exportPath = Path.Combine(downloadsFolder, $"{baseFileName}_{counter}{fileExtension}");
                    counter++;
                }

                // Create single file changelog with header
                string singleChangelog = $"Bulk Editor: Changelog - {DateTime.Now}\n";
                singleChangelog += $"Version: 2.1\n";
                singleChangelog += $"Up to Date\n\n";
                singleChangelog += $"Path: {txtFolderPath.Text}\n\n__________\n\n";
                singleChangelog += txtChangelog.Text;

                File.WriteAllText(exportPath, singleChangelog);

                lblStatus.Text = $"Single changelog exported to: {exportPath}";

                var result = MessageBox.Show("Single file changelog exported successfully to Downloads folder. Would you like to open it?", "Export Complete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = exportPath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting single changelog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddFile_Click(object sender, EventArgs e)
        {
            using var fileDialog = new OpenFileDialog();
            fileDialog.Title = "Select a .docx file to add";
            fileDialog.Filter = "Word documents (*.docx)|*.docx";
            fileDialog.Multiselect = true;

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                // Initialize file path mapping if not exists
                if (lstFiles.Tag == null)
                {
                    lstFiles.Tag = new Dictionary<int, string>();
                }
                var filePathMap = (Dictionary<int, string>)lstFiles.Tag;

                foreach (string fileName in fileDialog.FileNames)
                {
                    FileInfo fileInfo = new(fileName);
                    string displayText = $"{fileInfo.Name} ({FormatFileSize(fileInfo.Length)})";

                    // Check if file is already in the list
                    bool alreadyExists = false;
                    for (int i = 0; i < lstFiles.Items.Count; i++)
                    {
                        if (lstFiles.Items[i].ToString().StartsWith(fileInfo.Name))
                        {
                            alreadyExists = true;
                            break;
                        }
                    }

                    if (!alreadyExists)
                    {
                        int newIndex = lstFiles.Items.Add(displayText);
                        // Map the list index to the full file path
                        filePathMap[newIndex] = fileName;
                    }
                }

                lblStatus.Text = $"Added {fileDialog.FileNames.Length} file(s) to selection";
                ShowFileList();
            }
        }

        private void BtnRemoveFile_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndex >= 0)
            {
                int selectedIndex = lstFiles.SelectedIndex;

                // Remove from the file path mapping if it exists
                if (lstFiles.Tag is Dictionary<int, string> filePathMap)
                {
                    filePathMap.Remove(selectedIndex);

                    // Rebuild the mapping with updated indices
                    var updatedMap = new Dictionary<int, string>();
                    foreach (var kvp in filePathMap)
                    {
                        if (kvp.Key > selectedIndex)
                        {
                            // Shift indices down after the removed item
                            updatedMap[kvp.Key - 1] = kvp.Value;
                        }
                        else if (kvp.Key < selectedIndex)
                        {
                            // Keep existing indices before the removed item
                            updatedMap[kvp.Key] = kvp.Value;
                        }
                    }
                    lstFiles.Tag = updatedMap;
                }

                string fileName = lstFiles.SelectedItem.ToString().Split('(')[0].Trim();
                lstFiles.Items.RemoveAt(selectedIndex);

                lblStatus.Text = $"Removed {fileName} from file list";

                // Select next item if available
                if (lstFiles.Items.Count > 0)
                {
                    if (selectedIndex >= lstFiles.Items.Count)
                        selectedIndex = lstFiles.Items.Count - 1;
                    lstFiles.SelectedIndex = selectedIndex;
                }
                else
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
                string fileName = lstFiles.SelectedItem.ToString().Split('(')[0].Trim();
                string folderPath = null;

                // First priority: Try to get the full path from stored paths (individually added files)
                if (lstFiles.Tag is Dictionary<int, string> filePathMap &&
                    filePathMap.TryGetValue(lstFiles.SelectedIndex, out string fullFilePath))
                {
                    if (File.Exists(fullFilePath))
                    {
                        folderPath = Path.GetDirectoryName(fullFilePath);
                    }
                }

                // Second priority: If no stored path or file doesn't exist, check the main folder/file path
                if (string.IsNullOrEmpty(folderPath))
                {
                    if (Directory.Exists(txtFolderPath.Text))
                    {
                        // Main path is a directory, check if file exists there
                        string possibleFilePath = Path.Combine(txtFolderPath.Text, fileName);
                        if (File.Exists(possibleFilePath))
                        {
                            folderPath = txtFolderPath.Text;
                        }
                    }
                    else if (File.Exists(txtFolderPath.Text))
                    {
                        // Main path is a file, use its directory
                        folderPath = Path.GetDirectoryName(txtFolderPath.Text);
                    }
                }

                // If still no valid folder found, show error
                if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                {
                    MessageBox.Show($"Unable to determine file location for '{fileName}'. The file may have been moved or the path is no longer valid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = folderPath,
                        UseShellExecute = true
                    });
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
                var result = MessageBox.Show("Are you sure you want to remove all documents from the File List?",
                                            "Clear Document List",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    lstFiles.Items.Clear();
                    lstFiles.Tag = null;
                    txtFolderPath.Text = string.Empty;
                    pnlChangelog.Visible = false;
                    lblStatus.Text = "Document list cleared";
                }
            }
            else
            {
                MessageBox.Show("File list is already empty.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
