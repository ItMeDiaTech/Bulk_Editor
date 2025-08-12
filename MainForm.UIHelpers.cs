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
             try
             {
                 var changelogContent = await _logViewerService.GetChangelogForFileAsync(changelogPath, fileName);
 
                 lblChangelogTitle.Text = $"Changelog - {fileName}";
                 txtChangelog.Text = changelogContent;
                 UpdatePanelVisibility();
             }
             catch (IOException ex)
             {
                 _loggingService.LogWarning(ex, "IOException while displaying changelog for {FileName}", fileName);
                 txtChangelog.Text = "Changelog is currently being updated. Please wait a moment and select the file again.";
             }
             catch (Exception ex)
             {
                 _loggingService.LogError(ex, "Failed to display changelog for {FileName}", fileName);
                 MessageBox.Show($"Error reading changelog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 txtChangelog.Text = $"An error occurred while trying to read the changelog for {fileName}.";
             }
             finally
             {
                 lblChangelogTitle.Text = $"Changelog - {fileName}";
                 UpdatePanelVisibility();
             }
         }
 
         private void UpdateSubCheckboxStates()
         {
             bool isEnabled = chkFixSourceHyperlinks.Checked;
 
             // Enable/disable sub-checkboxes based on parent
             chkAppendContentID.Enabled = isEnabled;
             chkCheckTitleChanges.Enabled = isEnabled;
             chkFixTitles.Enabled = isEnabled;
 
             // Uncheck sub-checkboxes when parent is unchecked
             if (!isEnabled)
             {
                 chkAppendContentID.Checked = false;
                 chkCheckTitleChanges.Checked = false;
                 chkFixTitles.Checked = false;
             }
 
             // Delegate color updates to the theme service to avoid full-form refreshes
             _themeService.UpdateCheckboxColors(this);
         }
 
         /// <summary>
         /// Sets the visibility of the main content panels.
         /// </summary>
         private void UpdatePanelVisibility()
         {
             bool hasFiles = lstFiles.Items.Count > 0;
             lstFiles.Visible = hasFiles;
             pnlChangelog.Visible = hasFiles;
         }
 
         private static string FormatFileSize(long bytes)
         {
            return Bulk_Editor.Utils.FormatUtils.FormatFileSize(bytes);
         }
     }
 }