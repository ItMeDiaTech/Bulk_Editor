using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bulk_Editor.Models;
using Bulk_Editor.Services;

namespace Bulk_Editor
{
    public partial class MainForm
    {
        private async void BtnRunTools_Click(object sender, EventArgs e)
        {
            // Check if this is a cancellation request
            if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                lblStatus.Text = "Cancelling...";
                return;
            }

            if (string.IsNullOrEmpty(txtFolderPath.Text))
            {
                MessageBox.Show("Please select a file or folder first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Determine files to process first for accurate confirmation count
            bool isFolder = Directory.Exists(txtFolderPath.Text);
            string path = txtFolderPath.Text;

            // Handle both folder-based files and individually added files
            List<string> filesToProcess = new List<string>();

            if (isFolder)
            {
                // Add files matching allowed extensions from the selected folder
                var allowedFiles = new List<string>();
                foreach (string extension in _appSettings.Processing.AllowedExtensions)
                {
                    string pattern = extension.StartsWith("*") ? extension : "*" + extension;
                    string[] matchingFiles = Directory.GetFiles(path, pattern);
                    allowedFiles.AddRange(matchingFiles);
                }
                filesToProcess.AddRange(allowedFiles.Distinct());
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

            // Check ConfirmBeforeProcessing setting with accurate count
            if (_appSettings.ApplicationSettings.ConfirmBeforeProcessing)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to process {filesToProcess.Count} file(s)?",
                    "Confirm Processing",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            string? basePath = isFolder ? path : Path.GetDirectoryName(path);
            // Implement basePath fallback for single files
            basePath ??= Path.GetDirectoryName(Application.ExecutablePath);

            // Store the originally selected index to preserve user's selection
            int originalSelectedIndex = lstFiles.SelectedIndex;

            // Create new cancellation token for this operation with configured timeout
            _cancellationTokenSource = new CancellationTokenSource();
            if (_appSettings.Processing.ProcessingTimeoutMinutes > 0)
            {
                _cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(_appSettings.Processing.ProcessingTimeoutMinutes));
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting processing: {path}");

                // Transform button for cancellation
                TransformButtonForProcessing();

                _hyperlinkReplacementRules?.TrimRules();

                // Create backup directory if backup is enabled
                string? backupPath = GetBackupPath(basePath);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string changelogPath = string.Empty;

                // Only determine changelog path if the checkbox is checked
                if (chkOpenChangelogAfterUpdates.Checked)
                {
                    // Use centralized storage if configured, otherwise use legacy behavior
                    if (_changelogManager != null && _appSettings.ChangelogSettings.UseCentralizedStorage)
                    {
                        if (!isFolder && File.Exists(path))
                        {
                            // Single file processing - use individual changelog path
                            changelogPath = _changelogManager.GetIndividualChangelogPath(Path.GetFileName(path));
                            if (string.IsNullOrEmpty(changelogPath))
                            {
                                // Fallback to legacy behavior
                                string docName = Path.GetFileNameWithoutExtension(path);
                                string dateFormat = DateTime.Now.ToString("MMddyyyy");
                                changelogPath = Path.Combine(basePath ?? string.Empty, $"{docName}_Changelog_{dateFormat}.txt");
                            }
                        }
                        else
                        {
                            // Multiple files processing - use combined changelog path
                            changelogPath = _changelogManager.GetCombinedChangelogPath();
                            if (string.IsNullOrEmpty(changelogPath))
                            {
                                // Fallback to legacy behavior
                                changelogPath = Path.Combine(basePath ?? string.Empty, $"BulkEditor_Changelog_{timestamp}.txt");
                            }
                        }
                    }
                    else
                    {
                        // Legacy behavior for backward compatibility
                        if (!isFolder && File.Exists(path))
                        {
                            string docName = Path.GetFileNameWithoutExtension(path);
                            string dateFormat = DateTime.Now.ToString("MMddyyyy");
                            changelogPath = Path.Combine(basePath ?? string.Empty, $"{docName}_Changelog_{dateFormat}.txt");
                        }
                        else
                        {
                            changelogPath = Path.Combine(basePath ?? string.Empty, $"BulkEditor_Changelog_{timestamp}.txt");
                        }
                    }
                }

                ShowProgress(true);

                // Validate and filter files before processing
                _progressReporter.Report(ProgressReport.CreatePhaseProgress("Validation", "Validating selected files...", 5));
                _loggingService.LogProcessingStep("File Validation", $"Validating {filesToProcess.Count} files");

                var validFiles = await ValidateFilesForProcessing(filesToProcess);

                _loggingService.LogInformation("File validation completed. Valid files: {ValidCount}, Invalid files: {InvalidCount}",
                    validFiles.Count, filesToProcess.Count - validFiles.Count);

                // Respect batch size limits
                if (validFiles.Count > _appSettings.ApplicationSettings.MaxFileBatchSize)
                {
                    int originalCount = validFiles.Count;
                    validFiles = validFiles.Take(_appSettings.ApplicationSettings.MaxFileBatchSize).ToList();
                    _loggingService.LogWarning("Batch size limit applied. Processing {LimitedCount} of {OriginalCount} files",
                        validFiles.Count, originalCount);
                }

                _progressReporter.Report(ProgressReport.CreatePhaseProgress("Initialization", "Setting up processing environment...", 10));

                // Create appropriate writer based on whether changelog generation is enabled
                TextWriter safeWriter;
                StreamWriter? fileWriter = null;
                
                if (chkOpenChangelogAfterUpdates.Checked && !string.IsNullOrEmpty(changelogPath))
                {
                    // Check if changelog file already exists to determine append mode
                    bool appendMode = File.Exists(changelogPath);
                    fileWriter = new StreamWriter(changelogPath, append: appendMode);
                    safeWriter = TextWriter.Synchronized(fileWriter);
                    
                    // Only write header information if this is a new file (not appending)
                    if (!appendMode)
                    {
                        safeWriter.WriteLine($"Bulk Editor: Changelog - {DateTime.Now}");
                        safeWriter.WriteLine($"Version: 2.1");

                        // Check for updates
                        if (_processor?.NeedsUpdate == true)
                        {
                            safeWriter.WriteLine("***New Update Available***");
                            safeWriter.WriteLine("Please download and update as time allows.");
                        }
                        else
                        {
                            safeWriter.WriteLine("Up to Date");
                        }

                        safeWriter.WriteLine();
                        safeWriter.WriteLine($"Path: {path}");

                        // Write file count summary now that we have the correct count
                        if (validFiles.Count > 1)
                        {
                            safeWriter.WriteLine($"Processed {validFiles.Count} files.");
                        }

                        // Write batch size limit note if applicable
                        if (filesToProcess.Count > _appSettings.ApplicationSettings.MaxFileBatchSize)
                        {
                            safeWriter.WriteLine($"Note: Processing limited to {_appSettings.ApplicationSettings.MaxFileBatchSize} files due to batch size limits.");
                        }

                        safeWriter.WriteLine();
                    }
                    else
                    {
                        // Add a separator for appended content
                        safeWriter.WriteLine();
                        safeWriter.WriteLine($"=== Additional Processing Session - {DateTime.Now} ===");
                        safeWriter.WriteLine($"Path: {path}");
                        if (validFiles.Count > 1)
                        {
                            safeWriter.WriteLine($"Processed {validFiles.Count} files.");
                        }
                        safeWriter.WriteLine();
                    }
                }
                else
                {
                    // Use a null writer that discards all output when changelog is disabled
                    safeWriter = TextWriter.Null;
                }

                try
                {

                    // Process files with concurrency control
                    _progressReporter.Report(ProgressReport.CreatePhaseProgress("Processing", "Starting file processing...", 15));
                    _loggingService.LogProcessingStep("File Processing Start",
                        $"Processing {validFiles.Count} files. Concurrent processing: {_appSettings.Processing.MaxConcurrentFiles > 1 && validFiles.Count > 1}");

                    if (_appSettings.Processing.MaxConcurrentFiles > 1 && validFiles.Count > 1)
                    {
                        _loggingService.LogInformation("Using concurrent processing with max {MaxConcurrent} files",
                            _appSettings.Processing.MaxConcurrentFiles);
                        await ProcessFilesConcurrently(validFiles, safeWriter, _cancellationTokenSource.Token);
                    }
                    else
                    {
                        _loggingService.LogInformation("Using sequential processing for {FileCount} files", validFiles.Count);
                        // Sequential processing
                        for (int i = 0; i < validFiles.Count; i++)
                        {
                            _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                            var fileName = Path.GetFileName(validFiles[i]);
                            _progressReporter.Report(ProgressReport.CreateFileProgress(i + 1, validFiles.Count, fileName));
                            _loggingService.LogFileOperation("Processing", validFiles[i], $"File {i + 1} of {validFiles.Count}");

                            await ProcessFileWithProgressAsync(validFiles[i], safeWriter, backupPath, i, validFiles.Count, _cancellationTokenSource.Token);
                        }
                    }

                    _progressReporter.Report(ProgressReport.CreatePhaseProgress("Finalizing", "Completing processing...", 95));
                    _loggingService.LogProcessingStep("File Processing Complete", $"Successfully processed {validFiles.Count} files");
                }
                finally
                {
                    // Dispose of the file writer if we created one
                    fileWriter?.Dispose();
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
                        string fileName = lstFiles.Items[0].ToString()?.Split('(')[0].Trim() ?? string.Empty;
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
                    refreshTimer.Tick += (s, timerArgs) =>
                    {
                        refreshTimer.Stop();
                        refreshTimer.Dispose();
                        LstFiles_SelectedIndexChanged(this, EventArgs.Empty);
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

        private async Task ProcessFileWithProgressAsync(string filePath, TextWriter logWriter, string? backupBasePath, int fileIndex, int totalFiles, CancellationToken cancellationToken)
        {
            var fileName = Path.GetFileName(filePath);
            var startTime = DateTime.UtcNow;

            try
            {
                _loggingService.LogFileOperation("Processing Started", filePath, $"File {fileIndex + 1} of {totalFiles}");
                _progressReporter.Report(ProgressReport.CreateStepProgress(fileIndex + 1, totalFiles, fileName, 1, 7, "Initializing"));

                // Create backup if enabled
                bool backupCreated = false;
                if (_appSettings.Processing.CreateBackups && !string.IsNullOrEmpty(backupBasePath))
                {
                    _progressReporter.Report(ProgressReport.CreateStepProgress(fileIndex + 1, totalFiles, fileName, 2, 7, "Creating backup"));

                    string backupPath = Path.Combine(backupBasePath, Path.GetFileName(filePath));
                    File.Copy(filePath, backupPath, true);
                    backupCreated = true;
                    _loggingService.LogFileOperation("Backup Created", backupPath, "Backup successful");

                    // Preserve file attributes if configured
                    if (_appSettings.Processing.PreserveFileAttributes)
                    {
                        File.SetAttributes(backupPath, File.GetAttributes(filePath));
                        File.SetCreationTime(backupPath, File.GetCreationTime(filePath));
                        File.SetLastWriteTime(backupPath, File.GetLastWriteTime(filePath));
                        _loggingService.LogDebug("File attributes preserved for backup: {BackupPath}", backupPath);
                    }
                }

                _progressReporter.Report(ProgressReport.CreateStepProgress(fileIndex + 1, totalFiles, fileName, 3, 7, "Extracting hyperlinks"));

                List<string> changes = new();

                // Only write to changelog if it's enabled
                if (chkOpenChangelogAfterUpdates.Checked)
                {
                    logWriter.WriteLine();
                    logWriter.WriteLine("__________");
                    logWriter.WriteLine();
                    logWriter.WriteLine($"Title: {Path.GetFileNameWithoutExtension(filePath)}");
                    if (backupCreated)
                    {
                        logWriter.WriteLine("Backup File Created");
                    }
                    logWriter.WriteLine();
                }

                // Extract hyperlinks from the Word document properly
                var hyperlinks = WordDocumentProcessor.ExtractHyperlinks(filePath);
                var originalHyperlinks = hyperlinks.Select(h => h.Clone()).ToList();
                _loggingService.LogProcessingStep("Hyperlink Extraction", $"Extracted {hyperlinks.Count} hyperlinks from {fileName}");

                var updatedLinks = new Collection<string>();
                var notFoundLinks = new Collection<string>();
                var expiredLinks = new Collection<string>();
                var errorLinks = new Collection<string>();
                var updatedUrls = new Collection<string>();
                var replacedHyperlinks = new Collection<string>();

                // First, get API results if needed for various operations
                Dictionary<string, ApiResult>? apiResults = null;

                if (chkFixSourceHyperlinks.Checked || chkFixTitles.Checked)
                {
                    _progressReporter.Report(ProgressReport.CreateStepProgress(fileIndex + 1, totalFiles, fileName, 4, 7, "Calling API for hyperlink data"));

                    // Extract unique lookup IDs for API call
                    var uniqueIds = new HashSet<string>();
                    foreach (var hyperlink in hyperlinks)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        string lookupId = WordDocumentProcessor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
                        if (!string.IsNullOrEmpty(lookupId))
                        {
                            uniqueIds.Add(lookupId);
                        }
                    }

                    if (uniqueIds.Count > 0)
                    {
                        _loggingService.LogProcessingStep("API Call", $"Sending {uniqueIds.Count} lookup IDs to API for {fileName}");
                        var apiStartTime = DateTime.UtcNow;

                        // Call API to get results
                        cancellationToken.ThrowIfCancellationRequested();
                        string apiResponse = await _processor!.SendToPowerAutomateFlow(uniqueIds.ToList());
                        var response = _processor.ParseApiResponse(apiResponse);

                        var apiDuration = DateTime.UtcNow - apiStartTime;
                        _loggingService.LogApiCall("PowerAutomate", "POST", apiDuration, $"Received {response.Results.Count} results");

                        // Build dictionary for lookups
                        apiResults = new Dictionary<string, ApiResult>();
                        foreach (var result in response.Results)
                        {
                            if (!apiResults.ContainsKey(result.Document_ID))
                                apiResults.Add(result.Document_ID, result);
                            if (!apiResults.ContainsKey(result.Content_ID))
                                apiResults.Add(result.Content_ID, result);
                        }
                    }
                    else
                    {
                        _loggingService.LogInformation("No lookup IDs found for API call in {FileName}", fileName);
                    }
                }

                _progressReporter.Report(ProgressReport.CreateStepProgress(fileIndex + 1, totalFiles, fileName, 5, 7, "Processing hyperlinks"));

                if (chkFixSourceHyperlinks.Checked)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _loggingService.LogProcessingStep("Fix Source Hyperlinks", $"Processing {hyperlinks.Count} hyperlinks for {fileName}");
                    var retryService = new RetryPolicyService(_appSettings.RetrySettings, _progressReporter);
                    // Process hyperlinks - this now modifies the hyperlinks list instead of file content
                    await _processingService.FixSourceHyperlinksWithProgress(
                        null, hyperlinks, _processor!, changes, updatedLinks, notFoundLinks,
                        expiredLinks, errorLinks, updatedUrls, retryService, _progressReporter, cancellationToken);
                    _loggingService.LogProcessingStep("Fix Source Hyperlinks Complete",
                        $"Updated: {updatedLinks.Count}, NotFound: {notFoundLinks.Count}, Expired: {expiredLinks.Count}");
                }

                if (chkAppendContentID.Checked)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _loggingService.LogProcessingStep("Append Content ID", $"Appending content IDs to hyperlinks in {fileName}");
                    int modifiedCount = _processingService.AppendContentIDToHyperlinks(hyperlinks, updatedLinks);
                    if (modifiedCount > 0)
                    {
                        changes.Add($"Appended content IDs to {modifiedCount} hyperlinks");
                        _loggingService.LogProcessingStep("Append Content ID Complete", $"Modified {modifiedCount} hyperlinks");
                    }
                }

                if (chkFixInternalHyperlink.Checked)
                {
                    _loggingService.LogProcessingStep("Fix Internal Hyperlinks", $"Processing internal hyperlinks for {fileName}");
                    var internalLinks = new Collection<string>();
                    _processingService.FixInternalHyperlink(null, hyperlinks, changes, internalLinks);

                    // Add internal link changes to the changelog
                    foreach (var link in internalLinks)
                    {
                        updatedUrls.Add(link);
                    }
                    _loggingService.LogProcessingStep("Fix Internal Hyperlinks Complete", $"Fixed {internalLinks.Count} internal links");
                }

                // Check for possible title changes (logs only, no modifications)
                if (chkCheckTitleChanges.Checked)
                {
                    if (apiResults != null)
                    {
                        _loggingService.LogProcessingStep("Check Title Changes", $"Checking for title changes in {fileName}");
                        var titleChanges = new Collection<string>();
                        _processingService.DetectTitleChanges(null, hyperlinks, apiResults, changes, titleChanges);

                        // Add title changes to updatedUrls collection for changelog
                        foreach (var change in titleChanges)
                        {
                            updatedUrls.Add(change);
                        }
                        _loggingService.LogProcessingStep("Check Title Changes Complete", $"Found {titleChanges.Count} potential title changes");
                    }
                }

                // Update incorrect titles if checkbox is enabled
                if (chkFixTitles.Checked)
                {
                    if (apiResults != null)
                    {
                        _loggingService.LogProcessingStep("Fix Titles", $"Updating incorrect titles in {fileName}");
                        var urlUpdatedTracker = new Dictionary<string, bool>();
                        _processingService.UpdateTitles(null, hyperlinks, apiResults, changes, updatedLinks, urlUpdatedTracker);
                        _loggingService.LogProcessingStep("Fix Titles Complete", $"Updated titles based on API data");
                    }
                    else
                    {
                        // If no API results, at least skip already-processed links
                        _processingService.SkipProcessedHyperlinks(null, hyperlinks, changes);
                        _loggingService.LogInformation("No API results available for title fixing in {FileName}", fileName);
                    }
                }

                if (chkReplaceHyperlink.Checked)
                {
                    _loggingService.LogProcessingStep("Replace Hyperlinks", $"Applying replacement rules to {fileName}");
                    _processingService.ReplaceHyperlinks(null, hyperlinks, _hyperlinkReplacementRules!, changes, replacedHyperlinks);
                    _loggingService.LogProcessingStep("Replace Hyperlinks Complete", $"Replaced {replacedHyperlinks.Count} hyperlinks");
                }

                _progressReporter.Report(ProgressReport.CreateStepProgress(fileIndex + 1, totalFiles, fileName, 6, 7, "Saving changes"));

                // Check if any hyperlinks were modified
                bool hyperlinksModified = false;
                for (int i = 0; i < hyperlinks.Count; i++)
                {
                    if (!hyperlinks[i].Equals(originalHyperlinks[i]))
                    {
                        hyperlinksModified = true;
                        break;
                    }
                }

                // Persist hyperlink edits to the file if any were modified
                if (hyperlinksModified)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        WordDocumentProcessor.UpdateHyperlinksInDocument(filePath, hyperlinks);
                        _loggingService.LogProcessingStep("Hyperlink Updates Saved", $"Applied hyperlink changes to {fileName}");
                    }
                    catch (Exception ex)
                    {
                        _loggingService.LogError(ex, "Error saving hyperlink changes to {FileName}", fileName);
                        changes.Add($"Error saving hyperlink changes: {ex.Message}");
                    }
                }

                // Handle double spaces separately as it affects document text content
                int doubleSpaceCount = 0;
                if (chkFixDoubleSpaces.Checked)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _loggingService.LogProcessingStep("Fix Double Spaces", $"Checking for multiple spaces in {fileName}");
                    try
                    {
                        // Use the OpenXML implementation to fix double spaces in Word documents
                        doubleSpaceCount = WordDocumentProcessorExtensions.FixDoubleSpacesInDocument(filePath);

                        if (doubleSpaceCount > 0)
                        {
                            changes.Add($"Fixed {doubleSpaceCount} instances of multiple spaces");
                            // In case I need value in future, going to comment it out. logWriter.WriteLine($"  Fixed {doubleSpaceCount} instances of multiple spaces in document text");
                            _loggingService.LogProcessingStep("Fix Double Spaces Complete", $"Fixed {doubleSpaceCount} instances in {fileName}");
                        }
                        else
                        {
                            if (chkOpenChangelogAfterUpdates.Checked)
                            {
                                logWriter.WriteLine($"  No multiple spaces found in document");
                            }
                            _loggingService.LogDebug("No multiple spaces found in {FileName}", fileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        changes.Add($"Error fixing double spaces: {ex.Message}");
                        if (chkOpenChangelogAfterUpdates.Checked)
                        {
                            logWriter.WriteLine($"  Error fixing double spaces: {ex.Message}");
                        }
                        _loggingService.LogError(ex, "Error fixing double spaces in {FileName}", fileName);
                    }
                }

                _progressReporter.Report(ProgressReport.CreateStepProgress(fileIndex + 1, totalFiles, fileName, 7, 7, "Completing"));

                // Only write changelog details if changelog generation is enabled
                if (chkOpenChangelogAfterUpdates.Checked)
                {
                    WriteDetailedChangelog(logWriter, updatedLinks, notFoundLinks, expiredLinks, errorLinks, updatedUrls, replacedHyperlinks, doubleSpaceCount, _processor!);
                    WriteDetailedChangelogToDownloads(updatedLinks, notFoundLinks, expiredLinks, errorLinks, updatedUrls, replacedHyperlinks, doubleSpaceCount, _processor!);
                }

                var duration = DateTime.UtcNow - startTime;
                _loggingService.LogFileOperation("Processing Complete", filePath, $"Completed in {duration.TotalSeconds:F2} seconds");
                _loggingService.LogPerformanceMetric("FileProcessingTime", duration.TotalMilliseconds, "ms");
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _loggingService.LogError(ex, "Error processing file {FilePath} after {Duration}ms", filePath, duration.TotalMilliseconds);
                if (chkOpenChangelogAfterUpdates.Checked)
                {
                    logWriter.WriteLine($"  Error processing file: {ex.Message}");
                }
                throw; // Re-throw to allow higher-level error handling
            }
        }

        private static System.Text.StringBuilder BuildChangelogContent(
    Collection<string> updatedLinks, Collection<string> notFoundLinks, Collection<string> expiredLinks,
    Collection<string> errorLinks, Collection<string> updatedUrls, Collection<string> replacedHyperlinks, int doubleSpaceCount = 0)
        {
            var content = new System.Text.StringBuilder();

            // Parse updatedUrls for different categories
            var internalHyperlinkIssues = updatedUrls.Where(u =>
                u.Contains("Fixed, No Review Necessary") ||
                u.Contains("Attempted Fix, Please Review") ||
                u.Contains("Broken Internal Hyperlink")).ToList();
            var titleMismatchDetections = updatedUrls.Where(u => u.Contains("Title Mismatch, Please Review")).ToList();
            var fixedMismatchedTitles = updatedLinks.Where(u => u.Contains("Updated URL")).ToList();

            // 1. Updated Links section
            content.AppendLine($"Updated Links ({updatedLinks.Count}):");
            if (updatedLinks.Count > 0)
            {
                foreach (var link in updatedLinks)
                {
                    content.AppendLine($"    {link}");
                }
            }
            content.AppendLine();

            // 2. Found Expired section
            content.AppendLine($"Found Expired ({expiredLinks.Count}):");
            if (expiredLinks.Count > 0)
            {
                foreach (var link in expiredLinks)
                {
                    content.AppendLine($"    {link}");
                }
            }
            content.AppendLine();

            // 3. Not Found section
            content.AppendLine($"Not Found ({notFoundLinks.Count}):");
            if (notFoundLinks.Count > 0)
            {
                foreach (var link in notFoundLinks)
                {
                    content.AppendLine($"    {link}");
                }
            }
            content.AppendLine();

            // 4. Found Error section
            content.AppendLine($"Found Error ({errorLinks.Count}):");
            if (errorLinks.Count > 0)
            {
                foreach (var link in errorLinks)
                {
                    content.AppendLine($"    {link}");
                }
            }
            content.AppendLine();

            // 5. Title Mismatch section (detection only)
            if (titleMismatchDetections.Count > 0)
            {
                content.AppendLine($"Title Mismatch ({titleMismatchDetections.Count}):");
                foreach (var change in titleMismatchDetections)
                {
                    content.AppendLine($"    {change}");
                }
                content.AppendLine();
            }

            // 6. Fixed Mismatched Titles section (actual fixes)
            if (fixedMismatchedTitles.Count > 0)
            {
                content.AppendLine($"Fixed Mismatched Titles ({fixedMismatchedTitles.Count}):");
                foreach (var fix in fixedMismatchedTitles)
                {
                    content.AppendLine($"    {fix}");
                }
                content.AppendLine();
            }

            // 7. Internal Hyperlink Issues section
            if (internalHyperlinkIssues.Count > 0)
            {
                content.AppendLine($"Internal Hyperlink Issues ({internalHyperlinkIssues.Count}):");
                foreach (var issue in internalHyperlinkIssues)
                {
                    content.AppendLine($"    {issue}");
                }
                content.AppendLine();
            }

            // 8. Amount of Double Spaces Removed (at bottom with no indent)
            if (doubleSpaceCount > 0)
            {
                content.AppendLine($"Amount of Double Spaces Removed: {doubleSpaceCount}");
            }

            return content;
        }

        private static void WriteDetailedChangelog(TextWriter writer, Collection<string> updatedLinks, Collection<string> notFoundLinks,
            Collection<string> expiredLinks, Collection<string> errorLinks, Collection<string> updatedUrls,
            Collection<string> replacedHyperlinks, int doubleSpaceCount, WordDocumentProcessor processor)
        {
            var changelogContent = BuildChangelogContent(updatedLinks, notFoundLinks, expiredLinks, errorLinks, updatedUrls, replacedHyperlinks, doubleSpaceCount);
            writer.Write(changelogContent.ToString());
        }

        private static readonly object _downloadsLogLock = new();
        
        private static void WriteDetailedChangelogToDownloads(Collection<string> updatedLinks, Collection<string> notFoundLinks,
            Collection<string> expiredLinks, Collection<string> errorLinks, Collection<string> updatedUrls,
            Collection<string> replacedHyperlinks, int doubleSpaceCount, WordDocumentProcessor processor)
        {
            try
            {
                string changelogPath = GetUniqueChangelogPath();
                var changelogContent = BuildChangelogContent(updatedLinks, notFoundLinks, expiredLinks, errorLinks, updatedUrls, replacedHyperlinks, doubleSpaceCount);

                lock (_downloadsLogLock)
                {
                    using (StreamWriter writer = new StreamWriter(changelogPath, false))
                    {
                        WriteChangelogHeader(writer, processor);
                        writer.Write(changelogContent.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error writing changelog to downloads: {ex.Message}");
            }
        }

        private static string GetUniqueChangelogPath()
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

            return changelogPath;
        }

        private static void WriteChangelogHeader(StreamWriter writer, WordDocumentProcessor processor)
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
        }

        private void TransformButtonForProcessing()
        {
            // Capture original values if not already captured
            if (string.IsNullOrEmpty(_originalButtonText))
            {
                _originalButtonText = btnRunTools.Text;
            }
            if (_originalButtonColor == default)
            {
                _originalButtonColor = btnRunTools.BackColor;
            }
            
            btnRunTools.Text = "❌ Cancel Processing";
            btnRunTools.BackColor = Color.FromArgb(220, 53, 69); // Bootstrap danger red
        }

        private void RestoreButtonFromProcessing()
        {
            btnRunTools.Text = _originalButtonText;
            btnRunTools.BackColor = _originalButtonColor;
        }

        // Field to track the current retry notification timer
        private System.Windows.Forms.Timer? _retryNotificationTimer;

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
                // Cancel any existing retry notification timer
                if (_retryNotificationTimer != null)
                {
                    _retryNotificationTimer.Stop();
                    _retryNotificationTimer.Dispose();
                    _retryNotificationTimer = null;
                }

                lblStatus.ForeColor = Color.FromArgb(255, 193, 7); // Warning yellow

                // Reset color after delay
                _retryNotificationTimer = new System.Windows.Forms.Timer();
                _retryNotificationTimer.Interval = 2000; // 2 seconds
                _retryNotificationTimer.Tick += (s, timerArgs) =>
                {
                    lblStatus.ForeColor = SystemColors.ControlText;
                    if (_retryNotificationTimer != null)
                    {
                        _retryNotificationTimer.Stop();
                        _retryNotificationTimer.Dispose();
                        _retryNotificationTimer = null;
                    }
                };
                _retryNotificationTimer.Start();
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

        /// <summary>
        /// Validates files for processing based on configuration settings
        /// </summary>
        private async Task<List<string>> ValidateFilesForProcessing(List<string> filePaths)
        {
            var validFiles = new List<string>();

            foreach (string filePath in filePaths)
            {
                try
                {
                    // Check if file exists
                    if (!File.Exists(filePath))
                    {
                        _loggingService.LogUserAction("File Validation Failed", $"File not found: {filePath}");
                        continue;
                    }

                    // Check file size
                    var fileInfo = new FileInfo(filePath);
                    if (fileInfo.Length > _appSettings.Processing.MaxFileSizeBytes)
                    {
                        _loggingService.LogUserAction("File Validation Failed", $"File too large: {filePath} ({fileInfo.Length} bytes)");
                        continue;
                    }

                    // Check file extension
                    string extension = Path.GetExtension(filePath).ToLowerInvariant();
                    if (!_appSettings.Processing.AllowedExtensions.Any(ext =>
                        ext.ToLowerInvariant() == extension ||
                        ext.ToLowerInvariant() == "*" + extension))
                    {
                        _loggingService.LogUserAction("File Validation Failed", $"Extension not allowed: {filePath}");
                        continue;
                    }

                    // Validate document if enabled
                    if (_appSettings.Processing.ValidateDocuments)
                    {
                        try
                        {
                            // Basic validation - try to read the file
                            using (var stream = File.OpenRead(filePath))
                            {
                                // Simple validation - ensure we can read the file
                                var buffer = new byte[1024];
                                await stream.ReadAsync(buffer, 0, buffer.Length);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (_appSettings.Processing.SkipCorruptedFiles)
                            {
                                _loggingService.LogUserAction("File Validation Failed", $"Corrupted file skipped: {filePath} - {ex.Message}");
                                continue;
                            }
                            else
                            {
                                throw new InvalidOperationException($"Corrupted file: {filePath} - {ex.Message}");
                            }
                        }
                    }

                    validFiles.Add(filePath);
                }
                catch (Exception ex)
                {
                    _loggingService.LogUserAction("File Validation Error", $"Error validating {filePath}: {ex.Message}");
                    if (!_appSettings.Processing.SkipCorruptedFiles)
                    {
                        throw;
                    }
                }
            }

            return validFiles;
        }

        /// <summary>
        /// Processes files concurrently based on configuration settings
        /// </summary>
        private async Task ProcessFilesConcurrently(List<string> filePaths, TextWriter logWriter, CancellationToken cancellationToken)
        {
            var semaphore = new SemaphoreSlim(_appSettings.Processing.MaxConcurrentFiles, _appSettings.Processing.MaxConcurrentFiles);
            var tasks = new List<Task>();

            for (int i = 0; i < filePaths.Count; i++)
            {
                int fileIndex = i; // Capture loop variable
                string filePath = filePaths[i];

                var task = ProcessFileConcurrently(filePath, logWriter, fileIndex, filePaths.Count, semaphore, cancellationToken);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Processes a single file with concurrency control
        /// </summary>
        private async Task ProcessFileConcurrently(string filePath, TextWriter logWriter, int fileIndex, int totalFiles, SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var fileName = Path.GetFileName(filePath);
                _progressReporter.Report(ProgressReport.CreateFileProgress(fileIndex + 1, totalFiles, fileName));

                // Determine backup path
                string? backupBasePath = GetBackupPath(Path.GetDirectoryName(filePath));

                await ProcessFileWithProgressAsync(filePath, logWriter, backupBasePath, fileIndex, totalFiles, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Gets the backup path based on settings and creates the directory if needed
        /// </summary>
        private string? GetBackupPath(string? basePath)
        {
            if (!_appSettings.Processing.CreateBackups)
                return null;

            string? backupPath;
            if (_appSettings.ChangelogSettings.CentralizeBackups && _appSettings.ChangelogSettings.UseCentralizedStorage)
            {
                // Use centralized backup location
                backupPath = _changelogManager.GetBackupsPath();
            }
            else
            {
                // Use local backup folder
                backupPath = Path.Combine(basePath ?? string.Empty, _appSettings.Processing.BackupFolderName);
            }

            if (!string.IsNullOrEmpty(backupPath) && !Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }

            return backupPath;
        }
    }
}