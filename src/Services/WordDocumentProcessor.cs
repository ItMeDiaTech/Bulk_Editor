using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        // Regex pattern to match internal document IDs (e.g., TSRC-XXXX-XXXXXX or CMS-XXXX-XXXXXX)
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
                using WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false);
                {
                    var body = wordDoc.MainDocumentPart?.Document?.Body;
                    if (body == null) return hyperlinks;
                    int pageNumber = 1;
                    int lineNumber = 1;

                    // Find all hyperlinks in the document
                    var hyperlinkElements = body?.Descendants<Hyperlink>().ToList() ?? new List<Hyperlink>();

                    foreach (var hyperlinkElement in hyperlinkElements)
                    {
                        var hyperlink = new HyperlinkData();

                        // Get the hyperlink relationship
                        if (!string.IsNullOrEmpty(hyperlinkElement.Id))
                        {
                            var hyperlinkRelationship = wordDoc.MainDocumentPart?
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
                            hyperlink.SubAddress = hyperlinkElement.Anchor.Value ?? string.Empty;
                        }

                        // Get the display text
                        var textElements = hyperlinkElement.Descendants<Text>();
                        hyperlink.TextToDisplay = string.Join("", textElements.Select(t => t.Text ?? string.Empty));

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
                        hyperlink.ElementId = hyperlinkElement.Id?.Value ?? string.Empty;

                        hyperlinks.Add(hyperlink);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing document: {ex.Message}");
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
                    var body = wordDoc.MainDocumentPart?.Document?.Body;
                    if (body == null) return;
                    var hyperlinkElements = body?.Descendants<Hyperlink>().ToList() ?? new List<Hyperlink>();

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
                                var hyperlinkRelationship = wordDoc.MainDocumentPart?
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
                                    wordDoc.MainDocumentPart?.DeleteReferenceRelationship(hyperlinkRelationship);
                                    wordDoc.MainDocumentPart?.AddHyperlinkRelationship(new Uri(newUri), true, relationshipId);
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
                    wordDoc.MainDocumentPart?.Document.Save();
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
                // First try to deserialize as the new schema
                var response = JsonSerializer.Deserialize<ApiResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new();

                // If the new schema doesn't work, try legacy format
                if (string.IsNullOrEmpty(response.StatusCode) && response.Body?.Results?.Count == 0)
                {
                    // Try to parse as legacy direct format
                    var legacyResponse = JsonSerializer.Deserialize<ApiBody>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (legacyResponse != null && legacyResponse.Results?.Count > 0)
                    {
                        // Create a properly structured response
                        response = new ApiResponse
                        {
                            StatusCode = "200",
                            Headers = new ApiHeaders { ContentType = "application/json" },
                            Body = legacyResponse
                        };
                    }
                }

                // Set default values if still missing
                if (string.IsNullOrEmpty(response.StatusCode))
                {
                    response.StatusCode = "200"; // Assume success if we got data
                }

                if (response.Headers == null)
                {
                    response.Headers = new ApiHeaders { ContentType = "application/json" };
                }

                if (response.Body == null)
                {
                    response.Body = new ApiBody();
                }

                // Validate that we have some data to work with
                if (response.Body.Results == null || response.Body.Results.Count == 0)
                {
                    // Try to extract results from root level (another legacy format)
                    try
                    {
                        using var jsonDoc = JsonDocument.Parse(jsonResponse);
                        var root = jsonDoc.RootElement;

                        if (root.ValueKind == JsonValueKind.Array)
                        {
                            var results = JsonSerializer.Deserialize<List<ApiResult>>(jsonResponse, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                            if (results != null && results.Count > 0)
                            {
                                response.Body.Results = results;
                            }
                        }
                    }
                    catch
                    {
                        // If all parsing attempts fail, return empty response
                        System.Diagnostics.Debug.WriteLine("Unable to parse API response in any known format");
                    }
                }

                // Check for version updates using the new structure
                FlowVersion = response.Body.Version;
                UpdateNotes = response.Body.Changes;
                NeedsUpdate = !string.IsNullOrEmpty(FlowVersion) && FlowVersion != CurrentVersion;

                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing API response: {ex.Message}");
                // Return a minimal valid response instead of showing error to user
                return new ApiResponse
                {
                    StatusCode = "200",
                    Headers = new ApiHeaders { ContentType = "application/json" },
                    Body = new ApiBody()
                };
            }
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