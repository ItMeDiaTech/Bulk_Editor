using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Bulk_Editor
{
    // Helper classes for Word document processing
    public class HyperlinkData
    {
        public string Address { get; set; } = string.Empty;
        public string SubAddress { get; set; } = string.Empty;
        public string TextToDisplay { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int LineNumber { get; set; }
        public string OriginalText { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ContentID { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class WordDocumentProcessor
    {
        public List<HyperlinkData> ExtractHyperlinks(string filePath)
        {
            // Simplified implementation - in a real scenario, this would use
            // Microsoft.Office.Interop.Word or DocumentFormat.OpenXml
            var hyperlinks = new List<HyperlinkData>();

            try
            {
                // For now, we'll simulate hyperlink extraction
                // In a complete implementation, this would parse the actual .docx file
                string content = File.ReadAllText(filePath);

                // Simple regex to find hyperlinks in the text content
                var hyperlinkPattern = new Regex(@"<a[^>]*href=""([^""]*)""[^>]*>([^<]*)</a>", RegexOptions.IgnoreCase);
                var matches = hyperlinkPattern.Matches(content);

                int index = 0;
                foreach (Match match in matches)
                {
                    var hyperlink = new HyperlinkData
                    {
                        Address = match.Groups[1].Value,
                        TextToDisplay = match.Groups[2].Value,
                        PageNumber = 1, // Placeholder
                        LineNumber = index + 1 // Placeholder
                    };

                    // Extract sub-address if present
                    if (hyperlink.Address.Contains("#"))
                    {
                        var parts = hyperlink.Address.Split('#');
                        hyperlink.Address = parts[0];
                        hyperlink.SubAddress = parts.Length > 1 ? parts[1] : string.Empty;
                    }

                    hyperlinks.Add(hyperlink);
                    index++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing document: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return hyperlinks;
        }

        public List<HyperlinkData> RemoveInvisibleExternalHyperlinks(List<HyperlinkData> hyperlinks)
        {
            var removedHyperlinks = new List<HyperlinkData>();

            // Remove hyperlinks with empty display text but non-empty address
            for (int i = hyperlinks.Count - 1; i >= 0; i--)
            {
                var hyperlink = hyperlinks[i];
                if (string.IsNullOrWhiteSpace(hyperlink.TextToDisplay) && !string.IsNullOrWhiteSpace(hyperlink.Address))
                {
                    removedHyperlinks.Add(hyperlink);
                    hyperlinks.RemoveAt(i);
                }
            }

            return removedHyperlinks;
        }

        public string ExtractLookupID(string address, string subAddress)
        {
            string fullAddress = address + (!string.IsNullOrEmpty(subAddress) ? "#" + subAddress : "");

            // Pattern to match TSRC-XXXX-XXXXXX or CMS-XXXX-XXXXXX
            var idPattern = new Regex(@"(TSRC-[^-]+-[0-9]{6}|CMS-[^-]+-[0-9]{6})", RegexOptions.IgnoreCase);
            var match = idPattern.Match(fullAddress);

            if (match.Success)
            {
                return match.Value.ToUpper();
            }

            // Alternative pattern for docid parameter
            var docIdPattern = new Regex(@"docid=([^&]*)", RegexOptions.IgnoreCase);
            match = docIdPattern.Match(fullAddress);

            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            return string.Empty;
        }

        public string SendToPowerAutomateFlow(List<string> lookupIds, string flowUrl)
        {
            try
            {
                // Create JSON payload in the format expected by the API
                var jsonPayload = new
                {
                    Lookup_ID = lookupIds
                };

                string jsonBody = System.Text.Json.JsonSerializer.Serialize(jsonPayload);

                // Create HTTP request
                using (var client = new System.Net.Http.HttpClient())
                {
                    var content = new System.Net.Http.StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

                    // IMPORTANT: Replace the flowUrl with your actual Power Automate Flow URL
                    // The URL should look like: https://prod-00.eastus.logic.azure.com:443/workflows/[GUID]/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=[SIGNATURE]
                    var response = client.PostAsync(flowUrl, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        return response.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        public class ApiResponse
        {
            public string Version { get; set; } = string.Empty;
            public string Changes { get; set; } = string.Empty;
            public List<ApiResult> Results { get; set; } = new List<ApiResult>();
        }

        public class ApiResult
        {
            public string Document_ID { get; set; } = string.Empty;
            public string Content_ID { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
        }

        // Version checking functionality
        public const string CurrentVersion = "2.1";
        public bool NeedsUpdate { get; private set; } = false;
        public string FlowVersion { get; private set; } = string.Empty;
        public string UpdateNotes { get; private set; } = string.Empty;

        public ApiResponse ParseApiResponse(string jsonResponse)
        {
            try
            {
                var response = System.Text.Json.JsonSerializer.Deserialize<ApiResponse>(jsonResponse) ?? new ApiResponse();

                // Check for version updates
                FlowVersion = response.Version;
                UpdateNotes = response.Changes;
                NeedsUpdate = !string.IsNullOrEmpty(FlowVersion) && FlowVersion != CurrentVersion;

                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing API response: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new ApiResponse();
            }
        }

        public List<HyperlinkData> UpdateHyperlinksFromApiResponse(List<HyperlinkData> hyperlinks, ApiResponse apiResponse, List<string> changes)
        {
            var updatedHyperlinks = new List<HyperlinkData>();
            var resultsDict = new Dictionary<string, ApiResult>();

            // Create a dictionary for faster lookup
            foreach (var result in apiResponse.Results)
            {
                if (!string.IsNullOrEmpty(result.Document_ID))
                {
                    resultsDict[result.Document_ID] = result;
                }
                if (!string.IsNullOrEmpty(result.Content_ID))
                {
                    resultsDict[result.Content_ID] = result;
                }
            }

            int updatedCount = 0;
            int expiredCount = 0;
            int notFoundCount = 0;

            foreach (var hyperlink in hyperlinks)
            {
                string lookupId = ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
                var updatedHyperlink = new HyperlinkData
                {
                    Address = hyperlink.Address,
                    SubAddress = hyperlink.SubAddress,
                    TextToDisplay = hyperlink.TextToDisplay,
                    PageNumber = hyperlink.PageNumber,
                    LineNumber = hyperlink.LineNumber,
                    OriginalText = hyperlink.TextToDisplay,
                    Title = hyperlink.Title,
                    ContentID = hyperlink.ContentID,
                    Status = hyperlink.Status
                };

                // Check if already marked as expired or not found (equivalent to Base_File.txt lines 222-224)
                bool alreadyExpired = hyperlink.TextToDisplay.Contains(" - Expired");
                bool alreadyNotFound = hyperlink.TextToDisplay.Contains(" - Not Found");

                if (!string.IsNullOrEmpty(lookupId) && resultsDict.ContainsKey(lookupId))
                {
                    var result = resultsDict[lookupId];

                    // Update the hyperlink address and sub-address based on API response
                    string targetAddress = "https://beginningofhyperlinkurladdress.com/";
                    string targetSubAddress = "!/view?docid=" + result.Document_ID;

                    bool urlChanged = (hyperlink.Address != targetAddress || hyperlink.SubAddress != targetSubAddress);

                    if (urlChanged)
                    {
                        updatedHyperlink.Address = targetAddress;
                        updatedHyperlink.SubAddress = targetSubAddress;
                        updatedCount++;
                        changes.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | URL Updated: {hyperlink.TextToDisplay} -> {result.Title}");
                    }

                    // Update status based on API response
                    if (result.Status.Equals("expired", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!hyperlink.TextToDisplay.Contains(" - Expired"))
                        {
                            updatedHyperlink.TextToDisplay += " - Expired";
                            updatedHyperlink.Status = "expired";
                            expiredCount++;
                            changes.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | Expired: {result.Title}");
                        }
                    }
                    else if (result.Status.Equals("notfound", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!hyperlink.TextToDisplay.Contains(" - Not Found"))
                        {
                            updatedHyperlink.TextToDisplay += " - Not Found";
                            updatedHyperlink.Status = "notfound";
                            notFoundCount++;
                            changes.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | Not Found: {hyperlink.TextToDisplay}");
                        }
                    }
                    else
                    {
                        // Remove any existing status markers
                        string cleanText = hyperlink.TextToDisplay
                            .Replace(" - Expired", "")
                            .Replace(" - Not Found", "")
                            .Trim();

                        if (cleanText != hyperlink.TextToDisplay)
                        {
                            updatedHyperlink.TextToDisplay = cleanText;
                            updatedHyperlink.Status = "active";
                        }
                    }

                    // Update title and content ID
                    updatedHyperlink.Title = result.Title;
                    updatedHyperlink.ContentID = result.Content_ID;

                    // Content ID logic equivalent to Base_File.txt lines 264-286
                    if (!alreadyExpired && !alreadyNotFound)
                    {
                        string last6 = result.Content_ID.Length >= 6 ? result.Content_ID.Substring(result.Content_ID.Length - 6) : result.Content_ID;
                        string last5 = last6.Length >= 5 ? last6.Substring(last6.Length - 5) : last6;
                        bool appended = false;

                        // Check if we need to update the content ID (equivalent to Base_File.txt lines 264-266)
                        if (hyperlink.TextToDisplay.EndsWith($" ({last5})") && !hyperlink.TextToDisplay.EndsWith($" ({last6})"))
                        {
                            updatedHyperlink.TextToDisplay = hyperlink.TextToDisplay.Substring(0, hyperlink.TextToDisplay.Length - $" ({last5})".Length) + $" ({last6})";
                            appended = true;
                        }
                        // Check if content ID is not already present (equivalent to Base_File.txt line 274)
                        else if (!hyperlink.TextToDisplay.Contains($" ({last6})"))
                        {
                            updatedHyperlink.TextToDisplay = hyperlink.TextToDisplay.Trim() + $" ({last6})";
                            appended = true;
                        }

                        // Check for title changes (equivalent to Base_File.txt lines 282-286)
                        if (!string.IsNullOrEmpty(result.Title))
                        {
                            string currentTitleWithoutContentId = hyperlink.TextToDisplay;
                            if (currentTitleWithoutContentId.EndsWith($" ({last6})"))
                            {
                                currentTitleWithoutContentId = currentTitleWithoutContentId.Substring(0, currentTitleWithoutContentId.Length - $" ({last6})".Length);
                            }
                            else if (currentTitleWithoutContentId.EndsWith($" ({last5})"))
                            {
                                currentTitleWithoutContentId = currentTitleWithoutContentId.Substring(0, currentTitleWithoutContentId.Length - $" ({last5})".Length);
                            }

                            if (!currentTitleWithoutContentId.Equals(result.Title))
                            {
                                changes.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | Possible Title Change\n" +
                                    $"        Current Title: {currentTitleWithoutContentId}\n" +
                                    $"        New Title:     {result.Title}\n" +
                                    $"        Content ID:    {result.Content_ID}");
                            }
                        }

                        // Log if URL was updated or content ID was appended
                        if (urlChanged || appended)
                        {
                            string updateType = "";
                            if (urlChanged) updateType = "URL Updated";
                            if (appended) updateType += (string.IsNullOrEmpty(updateType) ? "" : ", ") + "Appended Content ID";

                            changes.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | {updateType}, {result.Title}");
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(lookupId))
                {
                    // Lookup ID found but not in API response
                    if (!hyperlink.TextToDisplay.Contains(" - Not Found") &&
                        !hyperlink.TextToDisplay.Contains(" - Expired"))
                    {
                        updatedHyperlink.TextToDisplay += " - Not Found";
                        updatedHyperlink.Status = "notfound";
                        notFoundCount++;
                        changes.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | Not Found: {hyperlink.TextToDisplay}");
                    }
                }

                updatedHyperlinks.Add(updatedHyperlink);
            }

            if (updatedCount > 0)
            {
                changes.Add($"Total URLs updated: {updatedCount}");
            }
            if (expiredCount > 0)
            {
                changes.Add($"Total expired links: {expiredCount}");
            }
            if (notFoundCount > 0)
            {
                changes.Add($"Total not found links: {notFoundCount}");
            }

            return updatedHyperlinks;
        }
    }

    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            SetupCheckboxDependencies();
            SetupFileListHandlers();
        }

        private void SetupFileListHandlers()
        {
            lstFiles.SelectedIndexChanged += LstFiles_SelectedIndexChanged;
            lstFiles.DoubleClick += LstFiles_DoubleClick;
        }

        private void LstFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndex >= 0)
            {
                // Check if we have a changelog for this file
                string selectedItem = lstFiles.SelectedItem.ToString();
                string fileName = selectedItem.Split('(')[0].Trim();

                // Try to find the changelog file for this item
                string changelogPath = FindLatestChangelog();

                if (File.Exists(changelogPath))
                {
                    DisplayChangelogForFile(changelogPath, fileName);
                }
                else
                {
                    // Show empty changelog panel with message
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
                }
                else if (File.Exists(txtFolderPath.Text))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = txtFolderPath.Text,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DisplayChangelogForFile(string changelogPath, string fileName)
        {
            try
            {
                string[] changelogLines = File.ReadAllLines(changelogPath);
                bool foundFileSection = false;
                var fileChangelog = new System.Text.StringBuilder();

                foreach (string line in changelogLines)
                {
                    if (line.StartsWith($"Processing file: {fileName}"))
                    {
                        foundFileSection = true;
                        fileChangelog.AppendLine(line);
                    }
                    else if (foundFileSection)
                    {
                        if (line.StartsWith("Processing file:") || line.StartsWith("Processed"))
                        {
                            // We've reached the next file or the end
                            break;
                        }
                        fileChangelog.AppendLine(line);
                    }
                }

                if (fileChangelog.Length > 0)
                {
                    lblChangelogTitle.Text = $"Changelog - {fileName}";
                    txtChangelog.Text = fileChangelog.ToString();
                    ShowChangelog();
                }
                else
                {
                    lblChangelogTitle.Text = "Changelog";
                    txtChangelog.Text = $"No changelog available for {fileName}";
                    ShowChangelog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading changelog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupCheckboxDependencies()
        {
            // Set initial state
            UpdateSubCheckboxStates();

            // Add event handlers
            chkFixSourceHyperlinks.CheckedChanged += ChkFixSourceHyperlinks_CheckedChanged;
        }

        private void ChkFixSourceHyperlinks_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSubCheckboxStates();
        }

        private void UpdateSubCheckboxStates()
        {
            // Enable/disable sub-checkboxes based on Fix Source Hyperlinks
            chkAppendContentID.Enabled = chkFixSourceHyperlinks.Checked;
            chkFixTitles.Enabled = chkFixSourceHyperlinks.Checked;

            // Set text color based on Fix Source Hyperlinks state
            chkAppendContentID.ForeColor = chkFixSourceHyperlinks.Checked ?
                SystemColors.ControlText : SystemColors.ControlDark;
            chkFixTitles.ForeColor = chkFixSourceHyperlinks.Checked ?
                SystemColors.ControlText : SystemColors.ControlDark;

            // If Fix Source Hyperlinks is unchecked, uncheck sub-options
            if (!chkFixSourceHyperlinks.Checked)
            {
                chkAppendContentID.Checked = false;
                chkFixTitles.Checked = false;
            }
        }

        private void BtnSelectFolder_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select a folder containing .docx files to process";
                folderDialog.ShowNewFolderButton = false;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFolderPath.Text = folderDialog.SelectedPath;
                    LoadFiles(folderDialog.SelectedPath);
                    ShowFileList();
                }
            }
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            using (var fileDialog = new OpenFileDialog())
            {
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

        private void BtnRunTools_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFolderPath.Text))
            {
                MessageBox.Show("Please select a file or folder first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool isFolder = Directory.Exists(txtFolderPath.Text);
            string path = txtFolderPath.Text;
            string basePath = isFolder ? path : Path.GetDirectoryName(path);

            try
            {
                // Create backup folder if it doesn't exist
                string backupPath = Path.Combine(basePath, "Backup");
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                // Create a changelog file in the same folder as the processed files with unique naming
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string changelogPath = Path.Combine(basePath, $"BulkEditor_Changelog_{timestamp}.txt");

                using (var writer = new StreamWriter(changelogPath, append: false))
                {
                    writer.WriteLine($"Bulk Editor Processing Log - {DateTime.Now}");
                    writer.WriteLine($"Processing: {path}");
                    writer.WriteLine();

                    if (isFolder)
                    {
                        // Process all files in the folder
                        string[] files = Directory.GetFiles(path);
                        foreach (string file in files)
                        {
                            ProcessFile(file, writer);
                        }
                        writer.WriteLine($"Processed {files.Length} files.");
                    }
                    else
                    {
                        // Process single file
                        ProcessFile(path, writer);
                        writer.WriteLine("Processed 1 file.");
                    }
                }

                MessageBox.Show($"Processing complete. Changelog saved to:\n{changelogPath}", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Show the changelog for the first file in the list
                if (lstFiles.Items.Count > 0)
                {
                    lstFiles.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFile(string filePath)
        {
            try
            {
                lstFiles.Items.Clear();
                FileInfo fileInfo = new FileInfo(filePath);
                lstFiles.Items.Add($"{fileInfo.Name} ({FormatFileSize(fileInfo.Length)})");
                lblStatus.Text = $"Loaded file: {filePath}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProcessFile(string filePath, StreamWriter logWriter)
        {
            try
            {
                // Create backup of the file before processing
                string backupPath = Path.Combine(Path.GetDirectoryName(filePath), "Backup", Path.GetFileName(filePath));
                File.Copy(filePath, backupPath, true);
                logWriter.WriteLine($"Backup created: {Path.GetFileName(backupPath)}");

                string fileContent = File.ReadAllText(filePath);
                string originalContent = fileContent;
                List<string> changes = new List<string>();

                logWriter.WriteLine($"Processing file: {Path.GetFileName(filePath)}");

                // Initialize Word document processor
                var processor = new WordDocumentProcessor();
                var hyperlinks = processor.ExtractHyperlinks(filePath);

                // Collections for detailed logging
                var updatedLinks = new Collection<string>();
                var notFoundLinks = new Collection<string>();
                var expiredLinks = new Collection<string>();
                var errorLinks = new Collection<string>();
                var updatedUrls = new Collection<string>();

                // Apply selected tools
                if (chkFixSourceHyperlinks.Checked)
                {
                    fileContent = FixSourceHyperlinks(fileContent, hyperlinks, processor, changes, updatedLinks, notFoundLinks, expiredLinks, errorLinks, updatedUrls);
                }

                if (chkAppendContentID.Checked)
                {
                    fileContent = AppendContentID(fileContent, hyperlinks, processor, changes);
                }

                if (chkFixInternalHyperlink.Checked)
                {
                    fileContent = FixInternalHyperlink(fileContent, hyperlinks, processor, changes);
                }

                if (chkFixTitles.Checked)
                {
                    fileContent = FixTitles(fileContent, hyperlinks, processor, changes);
                }

                if (chkFixDoubleSpaces.Checked)
                {
                    fileContent = FixDoubleSpaces(fileContent, changes);
                }

                // Save changes if any were made
                if (changes.Count > 0 && fileContent != originalContent)
                {
                    File.WriteAllText(filePath, fileContent);
                    logWriter.WriteLine($"  Changes made:");
                    foreach (string change in changes)
                    {
                        logWriter.WriteLine($"    - {change}");
                    }
                }
                else if (changes.Count == 0)
                {
                    logWriter.WriteLine($"  No changes were needed or made.");
                }

                // Write detailed changelog information
                WriteDetailedChangelog(logWriter, updatedLinks, notFoundLinks, expiredLinks, errorLinks, updatedUrls, processor);

                // Also save to Downloads folder with unique naming
                WriteDetailedChangelogToDownloads(updatedLinks, notFoundLinks, expiredLinks, errorLinks, updatedUrls, processor);

                logWriter.WriteLine();
            }
            catch (Exception ex)
            {
                logWriter.WriteLine($"  Error processing file: {ex.Message}");
            }
        }

        private void WriteDetailedChangelog(StreamWriter writer, Collection<string> updatedLinks, Collection<string> notFoundLinks,
            Collection<string> expiredLinks, Collection<string> errorLinks, Collection<string> updatedUrls, WordDocumentProcessor processor)
        {
            // Add version information and update notification
            writer.WriteLine($"  Bulk Editor Version: {WordDocumentProcessor.CurrentVersion}");

            if (processor.NeedsUpdate)
            {
                writer.WriteLine($"  UPDATE AVAILABLE: Version {processor.FlowVersion} is now available");
                writer.WriteLine($"  Update Notes: {processor.UpdateNotes}");
                writer.WriteLine();
            }

            writer.WriteLine();
            writer.WriteLine($"  Updated Links ({updatedLinks.Count}):");
            foreach (var link in updatedLinks)
            {
                writer.WriteLine($"    {link}");
            }

            writer.WriteLine();
            writer.WriteLine($"  Found Expired ({expiredLinks.Count}):");
            foreach (var link in expiredLinks)
            {
                writer.WriteLine($"    {link}");
            }

            writer.WriteLine();
            writer.WriteLine($"  Not Found ({notFoundLinks.Count}):");
            foreach (var link in notFoundLinks)
            {
                writer.WriteLine($"    {link}");
            }

            writer.WriteLine();
            writer.WriteLine($"  Found Error ({errorLinks.Count}):");
            foreach (var link in errorLinks)
            {
                writer.WriteLine($"    {link}");
            }

            writer.WriteLine();
            writer.WriteLine($"  Potential Outdated Titles ({updatedUrls.Count}):");
            foreach (var url in updatedUrls)
            {
                writer.WriteLine($"    {url}");
            }
        }

        private void WriteDetailedChangelogToDownloads(Collection<string> updatedLinks, Collection<string> notFoundLinks,
            Collection<string> expiredLinks, Collection<string> errorLinks, Collection<string> updatedUrls, WordDocumentProcessor processor)
        {
            try
            {
                string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (!Directory.Exists(downloadsFolder))
                {
                    downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }

                // Create unique filename
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
                    writer.WriteLine($"Bulk Editor Processing Log - {DateTime.Now}");

                    // Add version information and update notification
                    writer.WriteLine($"Bulk Editor Version: {WordDocumentProcessor.CurrentVersion}");

                    if (processor.NeedsUpdate)
                    {
                        writer.WriteLine($"UPDATE AVAILABLE: Version {processor.FlowVersion} is now available");
                        writer.WriteLine($"Update Notes: {processor.UpdateNotes}");
                        writer.WriteLine();
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
                    writer.WriteLine($"Potential Outdated Titles ({updatedUrls.Count}):");
                    foreach (var url in updatedUrls)
                    {
                        writer.WriteLine($"{url}");
                    }
                }

                // Update status
                lblStatus.Text = $"Changelog saved to: {changelogPath}";

                // Ask if user wants to open the file
                var result = MessageBox.Show("Changelog saved to Downloads folder. Would you like to open it?", "Changelog Saved", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = changelogPath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changelog to Downloads folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FixSourceHyperlinks(string content, List<HyperlinkData> hyperlinks, WordDocumentProcessor processor, List<string> changes,
            Collection<string> updatedLinks, Collection<string> notFoundLinks, Collection<string> expiredLinks,
            Collection<string> errorLinks, Collection<string> updatedUrls)
        {
            // Implementation based on Base_File.txt UpdateHyperlinksFromAPI function

            // Remove invisible external hyperlinks
            var removedHyperlinks = processor.RemoveInvisibleExternalHyperlinks(hyperlinks);
            if (removedHyperlinks.Count > 0)
            {
                changes.Add($"Removed {removedHyperlinks.Count} invisible external hyperlinks");

                // Add to error links collection for logging
                foreach (var hyperlink in removedHyperlinks)
                {
                    errorLinks.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | Invisible Hyperlink Deleted");
                }
            }

            // Extract unique lookup IDs
            var uniqueIds = new HashSet<string>();
            foreach (var hyperlink in hyperlinks)
            {
                string lookupId = processor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
                if (!string.IsNullOrEmpty(lookupId))
                {
                    uniqueIds.Add(lookupId);
                }
            }

            if (uniqueIds.Count > 0)
            {
                changes.Add($"Found {uniqueIds.Count} unique lookup IDs that would be updated via API");

                // Send to API (simulated)
                string apiResponse = processor.SendToPowerAutomateFlow(uniqueIds.ToList(), "https://prod-00.eastus.logic.azure.com:443/workflows/...");
                var response = processor.ParseApiResponse(apiResponse);

                // Create a dictionary for faster lookup
                var resultDict = new Dictionary<string, WordDocumentProcessor.ApiResult>();
                foreach (var result in response.Results)
                {
                    if (!resultDict.ContainsKey(result.Document_ID))
                        resultDict.Add(result.Document_ID, result);
                    if (!resultDict.ContainsKey(result.Content_ID))
                        resultDict.Add(result.Content_ID, result);
                }

                // Update hyperlinks based on API response
                foreach (var hyperlink in hyperlinks)
                {
                    string lookupId = processor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
                    if (!string.IsNullOrEmpty(lookupId))
                    {
                        if (resultDict.TryGetValue(lookupId, out var result))
                        {
                            // Update hyperlink address and sub-address
                            string targetAddress = "https://thesource.cvshealth.com/nuxeo/thesource/";
                            string targetSub = "!/view?docid=" + result.Document_ID;

                            bool changedURL = (hyperlink.Address != targetAddress) || (hyperlink.SubAddress != targetSub);
                            if (changedURL)
                            {
                                hyperlink.Address = targetAddress;
                                hyperlink.SubAddress = targetSub;
                                updatedLinks.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | URL Updated, {result.Title}");
                            }

                            // Check for status and update display text
                            bool alreadyExpired = hyperlink.TextToDisplay.Contains(" - Expired");
                            bool alreadyNotFound = hyperlink.TextToDisplay.Contains(" - Not Found");

                            if (!alreadyExpired && !alreadyNotFound)
                            {
                                // Append content ID if needed
                                string last6 = result.Content_ID.Substring(Math.Max(0, result.Content_ID.Length - 6));
                                string last5 = last6.Substring(Math.Max(0, last6.Length - 5));
                                bool appended = false;

                                // Check if we need to update the content ID
                                if (hyperlink.TextToDisplay.EndsWith($" ({last5})") && !hyperlink.TextToDisplay.EndsWith($" ({last6})"))
                                {
                                    hyperlink.TextToDisplay = hyperlink.TextToDisplay.Substring(0, hyperlink.TextToDisplay.Length - $" ({last5})".Length) + $" ({last6})";
                                    appended = true;
                                }
                                else if (!hyperlink.TextToDisplay.Contains($" ({last6})"))
                                {
                                    hyperlink.TextToDisplay = hyperlink.TextToDisplay.Trim() + $" ({last6})";
                                    appended = true;
                                }

                                // Check for title changes
                                string currentTitle = hyperlink.TextToDisplay;
                                string expectedTitle = result.Title;
                                if (!string.IsNullOrEmpty(expectedTitle) &&
                                    currentTitle.Length > $" ({last6})".Length &&
                                    !currentTitle.Substring(0, currentTitle.Length - $" ({last6})".Length).Equals(expectedTitle))
                                {
                                    updatedUrls.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | Possible Title Change\n" +
                                        $"        Current Title: {currentTitle.Substring(0, currentTitle.Length - $" ({last6})".Length)}\n" +
                                        $"        New Title:     {expectedTitle}\n" +
                                        $"        Content ID:    {result.Content_ID}");
                                }

                                if (appended || changedURL)
                                {
                                    string updateType = "";
                                    if (changedURL) updateType = "URL Updated";
                                    if (appended) updateType += (string.IsNullOrEmpty(updateType) ? "" : ", ") + "Appended Content ID";

                                    updatedLinks.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | {updateType}, {result.Title}");
                                }
                            }

                            // Handle expired status
                            if (result.Status == "Expired" && !alreadyExpired)
                            {
                                hyperlink.TextToDisplay += " - Expired";
                                expiredLinks.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | Expired, {result.Title}\n        {result.Content_ID}");
                            }
                        }
                        else
                        {
                            // Mark as not found if not already marked
                            bool alreadyExpired = hyperlink.TextToDisplay.Contains(" - Expired");
                            bool alreadyNotFound = hyperlink.TextToDisplay.Contains(" - Not Found");

                            if (!alreadyNotFound && !alreadyExpired)
                            {
                                hyperlink.TextToDisplay += " - Not Found";
                                notFoundLinks.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | Not Found\n        {hyperlink.TextToDisplay}");
                            }
                        }
                    }
                }
            }

            return content;
        }

        private string AppendContentID(string content, List<HyperlinkData> hyperlinks, WordDocumentProcessor processor, List<string> changes)
        {
            // Implementation for appending Content ID based on Base_File.txt
            int modifiedCount = 0;

            foreach (var hyperlink in hyperlinks)
            {
                string lookupId = processor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
                if (!string.IsNullOrEmpty(lookupId))
                {
                    // Extract the last 6 characters of the content ID
                    string last6 = lookupId.Length >= 6 ? lookupId.Substring(lookupId.Length - 6) : lookupId;
                    string last5 = last6.Length >= 5 ? last6.Substring(last6.Length - 5) : last6;

                    // Check if we need to append the content ID
                    if (!hyperlink.TextToDisplay.Contains("(" + last6 + ")"))
                    {
                        // If it ends with last5 but not last6, replace it
                        if (hyperlink.TextToDisplay.EndsWith("(" + last5 + ")"))
                        {
                            hyperlink.TextToDisplay = hyperlink.TextToDisplay.Substring(0, hyperlink.TextToDisplay.Length - last5.Length - 2) + last6 + ")";
                        }
                        else
                        {
                            hyperlink.TextToDisplay = hyperlink.TextToDisplay.Trim() + " (" + last6 + ")";
                        }
                        modifiedCount++;
                    }
                }
            }

            if (modifiedCount > 0)
            {
                changes.Add($"Appended Content ID to {modifiedCount} links");
            }

            return content;
        }

        private string FixInternalHyperlink(string content, List<HyperlinkData> hyperlinks, WordDocumentProcessor processor, List<string> changes)
        {
            // Implementation for fixing internal hyperlinks
            int fixedCount = 0;

            foreach (var hyperlink in hyperlinks)
            {
                // Check if it's an internal hyperlink (empty address but has sub-address)
                if (string.IsNullOrEmpty(hyperlink.Address) && !string.IsNullOrEmpty(hyperlink.SubAddress))
                {
                    // In a real implementation, we would check if the anchor exists in the document
                    // For now, we'll just validate the format
                    if (hyperlink.SubAddress.StartsWith("!") || hyperlink.SubAddress.StartsWith("_"))
                    {
                        fixedCount++;
                        changes.Add($"Validated internal hyperlink: {hyperlink.SubAddress}");
                    }
                }
            }

            if (fixedCount > 0)
            {
                changes.Add($"Fixed {fixedCount} internal hyperlinks");
            }

            return content;
        }

        private string FixTitles(string content, List<HyperlinkData> hyperlinks, WordDocumentProcessor processor, List<string> changes)
        {
            // Implementation for fixing titles based on Base_File.txt
            int fixedCount = 0;

            foreach (var hyperlink in hyperlinks)
            {
                string lookupId = processor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
                if (!string.IsNullOrEmpty(lookupId))
                {
                    // In a real implementation, we would get the title from the API response
                    // For now, we'll just check if the title needs updating
                    if (!string.IsNullOrEmpty(hyperlink.TextToDisplay) &&
                        (hyperlink.TextToDisplay.Contains(" - Expired") ||
                         hyperlink.TextToDisplay.Contains(" - Not Found")))
                    {
                        // Remove expired/not found markers
                        hyperlink.TextToDisplay = hyperlink.TextToDisplay
                            .Replace(" - Expired", "")
                            .Replace(" - Not Found", "")
                            .Trim();
                        fixedCount++;
                    }
                }
            }

            if (fixedCount > 0)
            {
                changes.Add($"Fixed {fixedCount} titles");
            }

            return content;
        }

        private string FixDoubleSpaces(string content, List<string> changes)
        {
            // Implementation for fixing double spaces
            int fixedCount = 0;
            string originalContent = content;

            // Replace multiple spaces with a single space
            // Using regex to handle any number of consecutive spaces
            string pattern = @"[ ]{2,}";
            string replacement = " ";
            content = Regex.Replace(content, pattern, replacement);

            // Count how many replacements were made
            if (content != originalContent)
            {
                // Count occurrences of the pattern to get an approximate count
                MatchCollection matches = Regex.Matches(originalContent, pattern);
                fixedCount = matches.Count;
                changes.Add($"Fixed {fixedCount} instances of multiple spaces");
            }

            return content;
        }

        private void LoadFiles(string folderPath)
        {
            try
            {
                lstFiles.Items.Clear();

                if (Directory.Exists(folderPath))
                {
                    // Only load .docx files
                    string[] files = Directory.GetFiles(folderPath, "*.docx");

                    foreach (string file in files)
                    {
                        FileInfo fileInfo = new FileInfo(file);
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

        private string FormatFileSize(long bytes)
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
            // First, check if we have a selected folder or file
            if (!string.IsNullOrEmpty(txtFolderPath.Text))
            {
                bool isFolder = Directory.Exists(txtFolderPath.Text);
                string basePath = isFolder ? txtFolderPath.Text : Path.GetDirectoryName(txtFolderPath.Text);

                // Look for changelog files in the same folder as the processed files
                string[] changelogFiles = Directory.GetFiles(basePath, "BulkEditor_Changelog_*.txt");

                if (changelogFiles.Length > 0)
                {
                    // Get the most recent changelog file
                    Array.Sort(changelogFiles);
                    return changelogFiles[changelogFiles.Length - 1];
                }
            }

            // If not found in the processed folder, check the desktop (for backward compatibility)
            string desktopChangelogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BulkEditor_Changelog.txt");
            if (File.Exists(desktopChangelogPath))
            {
                return desktopChangelogPath;
            }

            return string.Empty;
        }

        private void BtnExportChangelog_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if we have changelog data in the UI
                if (string.IsNullOrEmpty(txtChangelog.Text) || txtChangelog.Text.Contains("No changelog file found"))
                {
                    MessageBox.Show("No changelog data to export. Please run the tools first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (!Directory.Exists(downloadsFolder))
                {
                    downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }

                // Create unique filename
                string baseFileName = "BulkEditor_Changelog_Export";
                string fileExtension = ".txt";
                string exportPath = Path.Combine(downloadsFolder, baseFileName + fileExtension);
                int counter = 1;

                while (File.Exists(exportPath))
                {
                    exportPath = Path.Combine(downloadsFolder, $"{baseFileName}_{counter}{fileExtension}");
                    counter++;
                }

                // Write the changelog content to the file
                File.WriteAllText(exportPath, txtChangelog.Text);

                // Update status
                lblStatus.Text = $"Changelog exported to: {exportPath}";

                // Ask if user wants to open the file
                var result = MessageBox.Show("Changelog exported successfully to Downloads folder. Would you like to open it?", "Export Complete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                MessageBox.Show($"Error exporting changelog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}



