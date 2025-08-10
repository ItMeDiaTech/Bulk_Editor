using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bulk_Editor
{
    public partial class MainForm
    {
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

        private void UpdateSubCheckboxStates()
        {
            // Enable/disable sub-checkboxes based on parent checkbox
            chkAppendContentID.Enabled = chkFixSourceHyperlinks.Checked;
            chkCheckTitleChanges.Enabled = chkFixSourceHyperlinks.Checked;
            chkFixTitles.Enabled = chkFixSourceHyperlinks.Checked;

            // Uncheck sub-checkboxes when parent is unchecked
            if (!chkFixSourceHyperlinks.Checked)
            {
                chkAppendContentID.Checked = false;
                chkCheckTitleChanges.Checked = false;
                chkFixTitles.Checked = false;
            }

            // Update checkbox colors through theme service if available
            if (_themeService != null)
            {
                UpdateCheckboxColors();
            }
            else
            {
                // Fallback to system colors if theme service not available
                Color enabledColor = SystemColors.ControlText;
                Color disabledColor = SystemColors.ControlDark;

                // Fix Source Hyperlinks should always use enabled color
                chkFixSourceHyperlinks.ForeColor = enabledColor;

                chkAppendContentID.ForeColor = chkFixSourceHyperlinks.Checked ? enabledColor : disabledColor;
                chkCheckTitleChanges.ForeColor = chkFixSourceHyperlinks.Checked ? enabledColor : disabledColor;
                chkFixTitles.ForeColor = chkFixSourceHyperlinks.Checked ? enabledColor : disabledColor;
            }
        }

        private void UpdateCheckboxColors()
        {
            // Manually apply theme colors to just the checkboxes to avoid recursive theme application
            var theme = _themeService?.GetCurrentTheme();
            if (theme != null)
            {
                // Fix Source Hyperlinks should always remain visible with normal color
                chkFixSourceHyperlinks.ForeColor = theme.PrimaryCheckBoxForeground;

                if (chkFixSourceHyperlinks.Checked)
                {
                    chkAppendContentID.ForeColor = theme.SubCheckBoxForeground;
                    chkCheckTitleChanges.ForeColor = theme.SubCheckBoxForeground;
                    chkFixTitles.ForeColor = theme.SubCheckBoxForeground;
                }
                else
                {
                    chkAppendContentID.ForeColor = theme.DisabledCheckBoxForeground;
                    chkCheckTitleChanges.ForeColor = theme.DisabledCheckBoxForeground;
                    chkFixTitles.ForeColor = theme.DisabledCheckBoxForeground;
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
    }
}