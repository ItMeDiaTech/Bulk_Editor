using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bulk_Editor.Models;

namespace Bulk_Editor.Services
{
    /// <summary>
    /// Handles Word document processing operations
    /// </summary>
    public partial class WordDocumentProcessor
    {
        [GeneratedRegex(@"<a[^>]*href=""([^""]*)""[^>]*>([^<]*)</a>", RegexOptions.IgnoreCase)]
        private static partial Regex HyperlinkPatternRegex();

        [GeneratedRegex(@"(TSRC-[^-]+-[0-9]{6}|CMS-[^-]+-[0-9]{6})", RegexOptions.IgnoreCase)]
        private static partial Regex IdPatternRegex();

        [GeneratedRegex(@"docid=([^&]*)", RegexOptions.IgnoreCase)]
        private static partial Regex DocIdPatternRegex();

        // Version checking functionality
        public const string CurrentVersion = "2.1";
        public bool NeedsUpdate { get; private set; } = false;
        public string FlowVersion { get; private set; } = string.Empty;
        public string UpdateNotes { get; private set; } = string.Empty;

        /// <summary>
        /// Extracts hyperlinks from a document file (placeholder implementation)
        /// </summary>
        public static List<HyperlinkData> ExtractHyperlinks(string filePath)
        {
            var hyperlinks = new List<HyperlinkData>();

            try
            {
                // TODO: Replace with actual .docx processing using DocumentFormat.OpenXml
                // For now, we'll simulate hyperlink extraction
                string content = File.ReadAllText(filePath);

                // Simple regex to find hyperlinks in the text content
                var matches = HyperlinkPatternRegex().Matches(content);

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
                    if (hyperlink.Address.Contains('#'))
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

        /// <summary>
        /// Removes invisible external hyperlinks from the collection
        /// </summary>
        public static List<HyperlinkData> RemoveInvisibleExternalHyperlinks(List<HyperlinkData> hyperlinks)
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

        /// <summary>
        /// Extracts lookup ID from hyperlink address and sub-address
        /// </summary>
        public static string ExtractLookupID(string address, string subAddress)
        {
            string fullAddress = address + (!string.IsNullOrEmpty(subAddress) ? "#" + subAddress : "");

            // Pattern to match TSRC-XXXX-XXXXXX or CMS-XXXX-XXXXXX
            var match = IdPatternRegex().Match(fullAddress);

            if (match.Success)
            {
                return match.Value.ToUpper();
            }

            // Alternative pattern for docid parameter
            match = DocIdPatternRegex().Match(fullAddress);

            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            return string.Empty;
        }

        /// <summary>
        /// Sends lookup IDs to Power Automate Flow for processing
        /// </summary>
        public async Task<string> SendToPowerAutomateFlow(List<string> lookupIds, string flowUrl)
        {
            try
            {
                // Create JSON payload in the format expected by the API
                var jsonPayload = new
                {
                    Lookup_ID = lookupIds
                };

                string jsonBody = JsonSerializer.Serialize(jsonPayload);

                // Create HTTP request with timeout
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

                // Send request to Power Automate Flow
                var response = await client.PostAsync(flowUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        /// <summary>
        /// Parses API response and checks for version updates
        /// </summary>
        public ApiResponse ParseApiResponse(string jsonResponse)
        {
            try
            {
                var response = JsonSerializer.Deserialize<ApiResponse>(jsonResponse) ?? new();

                // Check for version updates
                FlowVersion = response.Version;
                UpdateNotes = response.Changes;
                NeedsUpdate = !string.IsNullOrEmpty(FlowVersion) && FlowVersion != CurrentVersion;

                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing API response: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new();
            }
        }

        /// <summary>
        /// Updates hyperlinks based on API response
        /// </summary>
        public static List<HyperlinkData> UpdateHyperlinksFromApiResponse(List<HyperlinkData> hyperlinks, ApiResponse apiResponse, List<string> changes)
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
                var updatedHyperlink = hyperlink.Clone();

                // Check if already marked as expired or not found
                bool alreadyExpired = hyperlink.TextToDisplay.Contains(" - Expired");
                bool alreadyNotFound = hyperlink.TextToDisplay.Contains(" - Not Found");

                if (!string.IsNullOrEmpty(lookupId) && resultsDict.TryGetValue(lookupId, out var result))
                {
                    // Update the hyperlink address and sub-address based on API response
                    string targetAddress = "https://beginningofhyperlinkurladdress.com/";
                    string targetSubAddress = "!/view?docid=" + result.Document_ID;

                    bool urlChanged = hyperlink.Address != targetAddress || hyperlink.SubAddress != targetSubAddress;

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
                        }
                        expiredCount++;
                        changes.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | Expired: {result.Title}");
                    }
                    else if (result.Status.Equals("notfound", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!hyperlink.TextToDisplay.Contains(" - Not Found"))
                        {
                            updatedHyperlink.TextToDisplay += " - Not Found";
                            updatedHyperlink.Status = "notfound";
                        }
                        notFoundCount++;
                        changes.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | Not Found: {hyperlink.TextToDisplay}");
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

                    // Content ID logic
                    if (!alreadyExpired && !alreadyNotFound)
                    {
                        string last6 = result.Content_ID.Length >= 6 ? result.Content_ID.Substring(result.Content_ID.Length - 6) : result.Content_ID;
                        string last5 = last6.Length >= 5 ? last6.Substring(last6.Length - 5) : last6;
                        bool appended = false;

                        // Check if we need to update the content ID
                        if (hyperlink.TextToDisplay.EndsWith($" ({last5})") && !hyperlink.TextToDisplay.EndsWith($" ({last6})"))
                        {
                            updatedHyperlink.TextToDisplay = hyperlink.TextToDisplay.Substring(0, hyperlink.TextToDisplay.Length - $" ({last5})".Length) + $" ({last6})";
                            appended = true;
                        }
                        // Check if content ID is not already present
                        else if (!hyperlink.TextToDisplay.Contains($" ({last6})"))
                        {
                            updatedHyperlink.TextToDisplay = hyperlink.TextToDisplay.Trim() + $" ({last6})";
                            appended = true;
                        }

                        // Check for title changes
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

                            if (!currentTitleWithoutContentId.Equals(result.Title.Trim()))
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

    /// <summary>
    /// API response structure
    /// </summary>
    public class ApiResponse
    {
        public string Version { get; set; } = string.Empty;
        public string Changes { get; set; } = string.Empty;
        public List<ApiResult> Results { get; set; } = new();
    }

    /// <summary>
    /// Individual API result item
    /// </summary>
    public class ApiResult
    {
        public string Document_ID { get; set; } = string.Empty;
        public string Content_ID { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}