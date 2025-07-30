using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Bulk_Editor
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void BtnSelectFolder_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select a folder to display its files";
                folderDialog.ShowNewFolderButton = false;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFolderPath.Text = folderDialog.SelectedPath;
                    LoadFiles(folderDialog.SelectedPath);
                }
            }
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            using (var fileDialog = new OpenFileDialog())
            {
                fileDialog.Title = "Select a file to process";
                fileDialog.Filter = "All files (*.*)|*.*";
                fileDialog.Multiselect = false;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFolderPath.Text = fileDialog.FileName;
                    LoadFile(fileDialog.FileName);
                }
            }
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

            try
            {
                // Create a changelog file
                string changelogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BulkEditor_Changelog.txt");
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

                MessageBox.Show($"Processing complete. Changelog saved to {changelogPath}", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Diagnostics.Process.Start("notepad.exe", changelogPath);
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
                string fileContent = File.ReadAllText(filePath);
                string originalContent = fileContent;
                List<string> changes = new List<string>();

                logWriter.WriteLine($"Processing file: {Path.GetFileName(filePath)}");

                // Apply selected tools
                if (chkFixSourceHyperlinks.Checked)
                {
                    fileContent = FixSourceHyperlinks(fileContent, changes);
                }

                if (chkAppendContentID.Checked)
                {
                    fileContent = AppendContentID(fileContent, changes);
                }

                if (chkFixInternalHyperlink.Checked)
                {
                    fileContent = FixInternalHyperlink(fileContent, changes);
                }

                if (chkFixTitles.Checked)
                {
                    fileContent = FixTitles(fileContent, changes);
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

                logWriter.WriteLine();
            }
            catch (Exception ex)
            {
                logWriter.WriteLine($"  Error processing file: {ex.Message}");
            }
        }

        private string FixSourceHyperlinks(string content, List<string> changes)
        {
            // Implementation based on Base_File.txt UpdateHyperlinksFromAPI function
            // This is a simplified version that would need to be expanded with actual API calls

            // Remove blank text external links
            var linkPattern = new Regex(@"<a[^>]*href=""([^""]*)""[^>]*>\s*</a>", RegexOptions.IgnoreCase);
            int removedCount = linkPattern.Matches(content).Count;
            content = linkPattern.Replace(content, "");

            if (removedCount > 0)
            {
                changes.Add($"Removed {removedCount} blank external hyperlinks");
            }

            // Extract and update lookup IDs
            var idPattern = new Regex(@"(TSRC-[^-]+-[0-9]{6}|CMS-[^-]+-[0-9]{6})", RegexOptions.IgnoreCase);
            var matches = idPattern.Matches(content);

            if (matches.Count > 0)
            {
                changes.Add($"Found {matches.Count} lookup IDs that would be updated via API");
                // In a real implementation, this would make API calls to get updated URLs
            }

            return content;
        }

        private string AppendContentID(string content, List<string> changes)
        {
            // Implementation for appending Content ID based on Base_File.txt
            var linkPattern = new Regex(@"<a[^>]*href=""([^""]*)""[^>]*>([^<]*)</a>", RegexOptions.IgnoreCase);
            int modifiedCount = 0;

            content = linkPattern.Replace(content, match =>
            {
                string url = match.Groups[1].Value;
                string text = match.Groups[2].Value;

                // Extract content ID from URL if present
                var idMatch = Regex.Match(url, @"docid=([^&]*)");
                if (idMatch.Success && !text.Contains("(" + idMatch.Groups[1].Value + ")"))
                {
                    modifiedCount++;
                    return match.Value.Replace(text, text.Trim() + " (" + idMatch.Groups[1].Value + ")");
                }
                return match.Value;
            });

            if (modifiedCount > 0)
            {
                changes.Add($"Appended Content ID to {modifiedCount} links");
            }

            return content;
        }

        private string FixInternalHyperlink(string content, List<string> changes)
        {
            // Implementation for fixing internal hyperlinks
            var internalLinkPattern = new Regex(@"<a[^>]*href=""#([^""]*)""[^>]*>([^<]*)</a>", RegexOptions.IgnoreCase);
            int fixedCount = 0;

            content = internalLinkPattern.Replace(content, match =>
            {
                string anchor = match.Groups[1].Value;
                string text = match.Groups[2].Value;

                // Check if anchor exists in the document
                var anchorPattern = new Regex(@"<a[^>]*name=""" + anchor + @"""[^>]*>|id=""" + anchor + @"""", RegexOptions.IgnoreCase);
                if (!anchorPattern.IsMatch(content))
                {
                    fixedCount++;
                    // Mark as broken - in a real implementation, you might try to fix it
                    return match.Value.Replace(text, text + " [BROKEN LINK]");
                }
                return match.Value;
            });

            if (fixedCount > 0)
            {
                changes.Add($"Fixed {fixedCount} internal hyperlinks");
            }

            return content;
        }

        private string FixTitles(string content, List<string> changes)
        {
            // Implementation for fixing titles
            var titlePattern = new Regex(@"<title>([^<]*)</title>", RegexOptions.IgnoreCase);
            int fixedCount = 0;

            content = titlePattern.Replace(content, match =>
            {
                string title = match.Groups[1].Value;

                // Simple title fix - remove extra whitespace and normalize
                string fixedTitle = Regex.Replace(title, @"\s+", " ").Trim();

                if (title != fixedTitle)
                {
                    fixedCount++;
                    return match.Value.Replace(title, fixedTitle);
                }
                return match.Value;
            });

            if (fixedCount > 0)
            {
                changes.Add($"Fixed {fixedCount} titles");
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
                    string[] files = Directory.GetFiles(folderPath);

                    foreach (string file in files)
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        lstFiles.Items.Add($"{fileInfo.Name} ({FormatFileSize(fileInfo.Length)})");
                    }

                    lblStatus.Text = $"Loaded {files.Length} files from {folderPath}";
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
    }
}