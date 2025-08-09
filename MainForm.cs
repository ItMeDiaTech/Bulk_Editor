using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        public MainForm()
        {
            InitializeComponent();
            _processor = new WordDocumentProcessor();
            SetupCheckboxDependencies();
            SetupFileListHandlers();
            LoadHyperlinkReplacementRules();
        }

        private async void LoadHyperlinkReplacementRules()
        {
            _hyperlinkReplacementRules = await HyperlinkReplacementRules.LoadAsync();
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
                string selectedItem = lstFiles.SelectedItem.ToString();
                string fileName = selectedItem.Split('(')[0].Trim();

                string changelogPath = FindLatestChangelog();

                if (File.Exists(changelogPath))
                {
                    DisplayChangelogForFile(changelogPath, fileName);
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

        private void DisplayChangelogForFile(string changelogPath, string fileName)
        {
            try
            {
                string[] changelogLines = File.ReadAllLines(changelogPath);
                bool foundFileSection = false;
                var fileChangelog = new System.Text.StringBuilder();

                // Add document title
                fileChangelog.AppendLine(Path.GetFileNameWithoutExtension(fileName));
                fileChangelog.AppendLine();

                foreach (string line in changelogLines)
                {
                    if (line.StartsWith($"Processing file: {fileName}"))
                    {
                        foundFileSection = true;
                        continue; // Skip the "Processing file:" line as we're showing the document title above
                    }
                    else if (foundFileSection)
                    {
                        if (line.StartsWith("Processing file:") || line.StartsWith("Processed"))
                        {
                            break;
                        }
                        fileChangelog.AppendLine(line);
                    }
                }

                if (fileChangelog.Length > 0)
                {
                    lblChangelogTitle.Text = $"Changelog - {fileName}";
                    txtChangelog.Text = fileChangelog.ToString().Trim();
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
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting processing: {path}");

                _hyperlinkReplacementRules.TrimRules();

                string backupPath = Path.Combine(basePath, "Backup");
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string changelogPath = Path.Combine(basePath, $"BulkEditor_Changelog_{timestamp}.txt");

                ShowProgress(true);
                btnRunTools.Enabled = false;

                using (var writer = new StreamWriter(changelogPath, append: false))
                {
                    writer.WriteLine($"Bulk Editor Processing Log - {DateTime.Now}");
                    writer.WriteLine($"Processing: {path}");
                    writer.WriteLine();

                    if (isFolder)
                    {
                        string[] files = Directory.GetFiles(path, "*.docx");
                        for (int i = 0; i < files.Length; i++)
                        {
                            await ProcessFile(files[i], writer, i, files.Length);
                            UpdateProgress((i + 1) * 100 / files.Length);
                        }
                        writer.WriteLine($"Processed {files.Length} files.");
                    }
                    else
                    {
                        await ProcessFile(path, writer, 0, 1);
                        writer.WriteLine("Processed 1 file.");
                        UpdateProgress(100);
                    }
                }

                ShowProgress(false);
                btnRunTools.Enabled = true;

                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Finished processing: {path}");

                MessageBox.Show($"Processing complete. Changelog saved to:\n{changelogPath}", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (lstFiles.Items.Count > 0)
                {
                    lstFiles.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Error processing: {path} - {ex.Message}");
                ShowProgress(false);
                btnRunTools.Enabled = true;
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            Application.DoEvents();
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

        private async Task ProcessFile(string filePath, StreamWriter logWriter, int fileIndex, int totalFiles)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting processing file: {filePath}");

                string backupPath = Path.Combine(Path.GetDirectoryName(filePath), "Backup", Path.GetFileName(filePath));
                File.Copy(filePath, backupPath, true);
                logWriter.WriteLine($"Backup created: {Path.GetFileName(backupPath)}");

                string fileContent = File.ReadAllText(filePath);
                string originalContent = fileContent;
                List<string> changes = new();

                logWriter.WriteLine($"Processing file: {Path.GetFileName(filePath)}");

                var hyperlinks = WordDocumentProcessor.ExtractHyperlinks(filePath);

                var updatedLinks = new Collection<string>();
                var notFoundLinks = new Collection<string>();
                var expiredLinks = new Collection<string>();
                var errorLinks = new Collection<string>();
                var updatedUrls = new Collection<string>();
                var replacedHyperlinks = new Collection<string>();

                if (chkFixSourceHyperlinks.Checked)
                {
                    fileContent = await ProcessingService.FixSourceHyperlinks(fileContent, hyperlinks, _processor, changes, updatedLinks, notFoundLinks, expiredLinks, errorLinks, updatedUrls);
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
                else if (changes.Count == 0)
                {
                    logWriter.WriteLine($"  No changes were needed or made.");
                }

                WriteDetailedChangelog(logWriter, updatedLinks, notFoundLinks, expiredLinks, errorLinks, updatedUrls, replacedHyperlinks, _processor);
                WriteDetailedChangelogToDownloads(updatedLinks, notFoundLinks, expiredLinks, errorLinks, updatedUrls, replacedHyperlinks, _processor);

                logWriter.WriteLine();

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
            writer.WriteLine($"  Not Found ({notFoundLinks.Count}):");
            foreach (var link in notFoundLinks)
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

            writer.WriteLine();
            writer.WriteLine($"  Replaced Hyperlinks ({replacedHyperlinks.Count}):");
            foreach (var hyperlink in replacedHyperlinks)
            {
                writer.WriteLine($"    {hyperlink}");
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
                    writer.WriteLine($"Bulk Editor Processing Log - {DateTime.Now}");
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

                    writer.WriteLine();
                    writer.WriteLine($"Replaced Hyperlinks ({replacedHyperlinks.Count}):");
                    foreach (var hyperlink in replacedHyperlinks)
                    {
                        writer.WriteLine($"{hyperlink}");
                    }
                }

                lblStatus.Text = $"Changelog saved to: {changelogPath}";

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

                string[] changelogFiles = Directory.GetFiles(basePath, "BulkEditor_Changelog_*.txt");

                if (changelogFiles.Length > 0)
                {
                    Array.Sort(changelogFiles);
                    return changelogFiles[changelogFiles.Length - 1];
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
                string singleChangelog = $"Bulk Editor Processing Log - {WordDocumentProcessor.CurrentVersion}\n";
                singleChangelog += $"Processing: {txtFolderPath.Text}\n\n";
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
    }
}
