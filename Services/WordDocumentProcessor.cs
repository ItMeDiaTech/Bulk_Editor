using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bulk_Editor.Configuration;
using Bulk_Editor.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Bulk_Editor.Services
{
    /// <summary>
    /// Handles Word document processing operations
    /// </summary>
    public partial class WordDocumentProcessor
    {
        [GeneratedRegex(@"(TSRC-[^-]+-[0-9]{6}|CMS-[^-]+-[0-9]{6})", RegexOptions.IgnoreCase)]
        private static partial Regex IdPatternRegex();

        [GeneratedRegex(@"docid=([^&]*)", RegexOptions.IgnoreCase)]
        private static partial Regex DocIdPatternRegex();

        // Configuration settings
        private readonly ApiSettings _apiSettings;
        private readonly RetrySettings _retrySettings;

        // Version checking functionality
        public const string CurrentVersion = "2.1";
        public bool NeedsUpdate { get; private set; } = false;
        public string FlowVersion { get; private set; } = string.Empty;
        public string UpdateNotes { get; private set; } = string.Empty;

        public WordDocumentProcessor(ApiSettings apiSettings = null, RetrySettings retrySettings = null)
        {
            _apiSettings = apiSettings ?? new ApiSettings();
            _retrySettings = retrySettings ?? new RetrySettings();
        }

        /// <summary>
        /// Extracts hyperlinks from a Word document file
        /// </summary>
        public static List<HyperlinkData> ExtractHyperlinks(string filePath)
        {
            var hyperlinks = new List<HyperlinkData>();

            try
            {
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false))
                {
                    var body = wordDoc.MainDocumentPart.Document.Body;
                    int pageNumber = 1;
                    int lineNumber = 1;

                    // Find all hyperlinks in the document
                    var hyperlinkElements = body.Descendants<Hyperlink>().ToList();

                    foreach (var hyperlinkElement in hyperlinkElements)
                    {
                        var hyperlink = new HyperlinkData();

                        // Get the hyperlink relationship
                        if (!string.IsNullOrEmpty(hyperlinkElement.Id))
                        {
                            var hyperlinkRelationship = wordDoc.MainDocumentPart
                                .HyperlinkRelationships
                                .FirstOrDefault(r => r.Id == hyperlinkElement.Id);

                            if (hyperlinkRelationship != null)
                            {
                                hyperlink.Address = hyperlinkRelationship.Uri.ToString();
                            }
                        }

                        // Get the anchor (internal link)
                        if (!string.IsNullOrEmpty(hyperlinkElement.Anchor))
                        {
                            hyperlink.SubAddress = hyperlinkElement.Anchor;
                        }

                        // Get the display text
                        var textElements = hyperlinkElement.Descendants<Text>();
                        hyperlink.TextToDisplay = string.Join("", textElements.Select(t => t.Text));

                        // Extract sub-address if present in the address
                        if (!string.IsNullOrEmpty(hyperlink.Address) && hyperlink.Address.Contains('#'))
                        {
                            var parts = hyperlink.Address.Split('#');
                            hyperlink.Address = parts[0];
                            if (string.IsNullOrEmpty(hyperlink.SubAddress))
                            {
                                hyperlink.SubAddress = parts.Length > 1 ? parts[1] : string.Empty;
                            }
                        }

                        // Set position information (approximation)
                        hyperlink.PageNumber = pageNumber;
                        hyperlink.LineNumber = lineNumber++;

                        // Store the hyperlink element reference for later updates
                        hyperlink.ElementId = hyperlinkElement.Id;

                        hyperlinks.Add(hyperlink);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing document: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return hyperlinks;
        }

        /// <summary>
        /// Updates hyperlinks in a Word document
        /// </summary>
        public static void UpdateHyperlinksInDocument(string filePath, List<HyperlinkData> updatedHyperlinks)
        {
            try
            {
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, true))
                {
                    var body = wordDoc.MainDocumentPart.Document.Body;
                    var hyperlinkElements = body.Descendants<Hyperlink>().ToList();

                    foreach (var updatedHyperlink in updatedHyperlinks)
                    {
                        // Find the corresponding hyperlink element
                        var hyperlinkElement = hyperlinkElements.FirstOrDefault(h => h.Id == updatedHyperlink.ElementId);

                        if (hyperlinkElement != null)
                        {
                            // Update the display text
                            var textElements = hyperlinkElement.Descendants<Text>().ToList();
                            if (textElements.Any())
                            {
                                // Clear existing text
                                foreach (var text in textElements.Skip(1))
                                {
                                    text.Remove();
                                }
                                // Update first text element
                                textElements.First().Text = updatedHyperlink.TextToDisplay;
                            }

                            // Update the hyperlink URL if needed
                            if (!string.IsNullOrEmpty(hyperlinkElement.Id))
                            {
                                var hyperlinkRelationship = wordDoc.MainDocumentPart
                                    .HyperlinkRelationships
                                    .FirstOrDefault(r => r.Id == hyperlinkElement.Id);

                                if (hyperlinkRelationship != null)
                                {
                                    // Build the new URI
                                    string newUri = updatedHyperlink.Address;
                                    if (!string.IsNullOrEmpty(updatedHyperlink.SubAddress))
                                    {
                                        newUri += "#" + updatedHyperlink.SubAddress;
                                    }

                                    // Remove old relationship and add new one
                                    var relationshipId = hyperlinkRelationship.Id;
                                    wordDoc.MainDocumentPart.DeleteReferenceRelationship(hyperlinkRelationship);
                                    wordDoc.MainDocumentPart.AddHyperlinkRelationship(new Uri(newUri), true, relationshipId);
                                }
                            }

                            // Update anchor if it's an internal link
                            if (!string.IsNullOrEmpty(updatedHyperlink.SubAddress) && string.IsNullOrEmpty(updatedHyperlink.Address))
                            {
                                hyperlinkElement.Anchor = updatedHyperlink.SubAddress;
                            }
                        }
                    }

                    // Save the document
                    wordDoc.MainDocumentPart.Document.Save();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating hyperlinks in document: {ex.Message}", ex);
            }
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
        public async Task<string> SendToPowerAutomateFlow(List<string> lookupIds, string flowUrl = null)
        {
            // Use configured flow URL if not provided
            string targetUrl = flowUrl ?? _apiSettings.PowerAutomateFlowUrl;

            for (int attempt = 0; attempt <= _retrySettings.MaxRetryAttempts; attempt++)
            {
                try
                {
                    // Create JSON payload in the format expected by the API
                    var jsonPayload = new
                    {
                        Lookup_ID = lookupIds
                    };

                    string jsonBody = JsonSerializer.Serialize(jsonPayload);

                    // Create HTTP request with configured settings
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(_apiSettings.TimeoutSeconds);
                    client.DefaultRequestHeaders.Add("User-Agent", _apiSettings.UserAgent);

                    var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

                    // Send request to Power Automate Flow
                    var response = await client.PostAsync(targetUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        string errorMessage = $"Error: {response.StatusCode} - {response.ReasonPhrase}";

                        // Only retry on server errors (5xx) and some client errors
                        if (attempt < _retrySettings.MaxRetryAttempts && ShouldRetry(response.StatusCode))
                        {
                            await DelayBeforeRetry(attempt);
                            continue;
                        }

                        return errorMessage;
                    }
                }
                catch (Exception ex)
                {
                    // Retry on network exceptions
                    if (attempt < _retrySettings.MaxRetryAttempts)
                    {
                        await DelayBeforeRetry(attempt);
                        continue;
                    }

                    return $"Exception: {ex.Message}";
                }
            }

            return "Failed after maximum retry attempts";
        }

        /// <summary>
        /// Determines if an HTTP status code should trigger a retry
        /// </summary>
        private static bool ShouldRetry(System.Net.HttpStatusCode statusCode)
        {
            return statusCode == System.Net.HttpStatusCode.InternalServerError ||
                   statusCode == System.Net.HttpStatusCode.BadGateway ||
                   statusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                   statusCode == System.Net.HttpStatusCode.GatewayTimeout ||
                   statusCode == System.Net.HttpStatusCode.TooManyRequests;
        }

        /// <summary>
        /// Calculates delay before retry using exponential backoff
        /// </summary>
        private async Task DelayBeforeRetry(int attemptNumber)
        {
            if (_retrySettings.UseExponentialBackoff)
            {
                // Calculate exponential backoff delay
                int delay = Math.Min(
                    _retrySettings.BaseDelayMs * (int)Math.Pow(2, attemptNumber),
                    _retrySettings.MaxDelayMs
                );
                await Task.Delay(delay);
            }
            else
            {
                // Use fixed base delay
                await Task.Delay(_retrySettings.BaseDelayMs);
            }
        }

        /// <summary>
        /// Parses API response and checks for version updates
        /// </summary>
        public ApiResponse ParseApiResponse(string jsonResponse)
        {
            try
            {
                var response = JsonSerializer.Deserialize<ApiResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new();

                // Validate response structure
                if (string.IsNullOrEmpty(response.StatusCode))
                {
                    throw new InvalidOperationException("Invalid API response: missing statusCode");
                }

                // Check HTTP status code
                if (!response.StatusCode.StartsWith("2")) // Not a 2xx success code
                {
                    throw new InvalidOperationException($"API returned error status: {response.StatusCode}");
                }

                // Validate headers
                if (response.Headers?.ContentType != "application/json")
                {
                    throw new InvalidOperationException("Invalid API response: expected JSON content type");
                }

                // Validate body structure
                if (response.Body == null)
                {
                    throw new InvalidOperationException("Invalid API response: missing body");
                }

                // Check for version updates using the new structure
                FlowVersion = response.Body.Version;
                UpdateNotes = response.Body.Changes;
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
        public static List<HyperlinkData> UpdateHyperlinksFromApiResponse(List<HyperlinkData> hyperlinks, ApiResponse apiResponse, List<string> changes, ApiSettings apiSettings = null)
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
                    // Use configured URLs from ApiSettings instead of hard-coded values
                    var settings = apiSettings ?? new ApiSettings();
                    string targetAddress = settings.HyperlinkBaseUrl;
                    string targetSubAddress = settings.HyperlinkViewPath + result.Document_ID;

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
    /// Complete API response structure matching the new schema
    /// </summary>
    public class ApiResponse
    {
        public string StatusCode { get; set; } = string.Empty;
        public ApiHeaders Headers { get; set; } = new();
        public ApiBody Body { get; set; } = new();

        // Legacy properties for backward compatibility
        public string Version => Body?.Version ?? string.Empty;
        public string Changes => Body?.Changes ?? string.Empty;
        public List<ApiResult> Results => Body?.Results ?? new();
    }

    /// <summary>
    /// API response headers
    /// </summary>
    public class ApiHeaders
    {
        public string ContentType { get; set; } = "application/json";

        // Map property name for JSON deserialization
        [System.Text.Json.Serialization.JsonPropertyName("Content-Type")]
        public string ContentTypeJson
        {
            get => ContentType;
            set => ContentType = value;
        }
    }

    /// <summary>
    /// API response body containing the actual data
    /// </summary>
    public class ApiBody
    {
        public List<ApiResult> Results { get; set; } = new();
        public string Version { get; set; } = string.Empty;
        public string Changes { get; set; } = string.Empty;
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