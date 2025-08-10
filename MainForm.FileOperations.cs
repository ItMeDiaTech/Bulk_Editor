using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bulk_Editor
{
    public partial class MainForm
    {
        private void LoadFile(string filePath)
        {
            try
            {
                lstFiles.Items.Clear();
                FileInfo fileInfo = new(filePath);
                lstFiles.Items.Add($"{fileInfo.Name} ({FormatFileSize(fileInfo.Length)})");
                
                // Auto-select first file if setting is enabled
                if (_appSettings.UI.AutoSelectFirstFile && lstFiles.Items.Count > 0)
                {
                    lstFiles.SelectedIndex = 0;
                }
                
                lblStatus.Text = $"Loaded file: {filePath}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFiles(string folderPath)
        {
            try
            {
                lstFiles.Items.Clear();
                string[] files = Directory.GetFiles(folderPath, "*.docx");

                foreach (string file in files)
                {
                    FileInfo fileInfo = new(file);
                    lstFiles.Items.Add($"{fileInfo.Name} ({FormatFileSize(fileInfo.Length)})");
                }

                // Auto-select first file if setting is enabled
                if (_appSettings.UI.AutoSelectFirstFile && lstFiles.Items.Count > 0)
                {
                    lstFiles.SelectedIndex = 0;
                }

                lblStatus.Text = $"Loaded {files.Length} files from {folderPath}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetSelectedFilePath()
        {
            if (lstFiles.SelectedIndex >= 0)
            {
                var filePathMap = lstFiles.Tag as Dictionary<int, string>;
                if (filePathMap != null && filePathMap.ContainsKey(lstFiles.SelectedIndex))
                {
                    return filePathMap[lstFiles.SelectedIndex];
                }

                // Fallback for folder-based selection
                if (Directory.Exists(txtFolderPath.Text))
                {
                    string selectedItem = lstFiles.SelectedItem.ToString();
                    string fileName = selectedItem.Split('(')[0].Trim();
                    return Path.Combine(txtFolderPath.Text, fileName);
                }
            }
            return string.Empty;
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

        private void ProcessSingleFile()
        {
            // Create a temporary single-item list for processing
            var originalItems = new object[lstFiles.Items.Count];
            lstFiles.Items.CopyTo(originalItems, 0);
            var originalTag = lstFiles.Tag;

            try
            {
                // Set up for single file processing
                string filePath = txtFolderPath.Text;
                if (File.Exists(filePath))
                {
                    lstFiles.Items.Clear();
                    LoadFile(filePath);

                    // Process the file using the existing method
                    BtnRunTools_Click(null, null);
                }
            }
            finally
            {
                // Restore original list
                lstFiles.Items.Clear();
                lstFiles.Items.AddRange(originalItems);
                lstFiles.Tag = originalTag;
            }
        }

        private string FindLatestChangelog()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string changelogPath = Path.Combine(desktopPath, "BulkEditor_Changelog.txt");
            return File.Exists(changelogPath) ? changelogPath : string.Empty;
        }
    }
}