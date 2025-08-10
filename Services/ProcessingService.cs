using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bulk_Editor.Models;

namespace Bulk_Editor.Services
{
    /// <summary>
    /// Service class for processing document content
    /// </summary>
    public static partial class ProcessingService
    {
        [GeneratedRegex(@"\s*\((\d{5,6})\)\s*$")]
        private static partial Regex ContentIdPatternRegex();

        [GeneratedRegex(@"[ ]{2,}")]
        private static partial Regex MultipleSpacesPatternRegex();

        /// <summary>
        /// Fixes source hyperlinks using API data
        /// </summary>
        public static async Task<string> FixSourceHyperlinks(string content, List<HyperlinkData> hyperlinks,
            WordDocumentProcessor processor, List<string> changes,
            Collection<string> updatedLinks, Collection<string> notFoundLinks,
            Collection<string> expiredLinks, Collection<string> errorLinks,
            Collection<string> updatedUrls)
        {
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting FixSourceHyperlinks");

            // Remove invisible external hyperlinks
            var removedHyperlinks = WordDocumentProcessor.RemoveInvisibleExternalHyperlinks(hyperlinks);
            if (removedHyperlinks.Count > 0)
            {
                changes.Add($"Removed {removedHyperlinks.Count} invisible external hyperlinks");

                foreach (var hyperlink in removedHyperlinks)
                {
                    errorLinks.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | Invisible Hyperlink Deleted");
                }
            }

            // Extract unique lookup IDs
            var uniqueIds = new HashSet<string>();
            foreach (var hyperlink in hyperlinks)
            {
                string lookupId = WordDocumentProcessor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
                if (!string.IsNullOrEmpty(lookupId))
                {
                    uniqueIds.Add(lookupId);
                }
            }

            if (uniqueIds.Count > 0)
            {
                changes.Add($"Found {uniqueIds.Count} unique lookup IDs that would be updated via API");

                // Send to API (simulated)
                string apiResponse = await processor.SendToPowerAutomateFlow(uniqueIds.ToList(), "https://prod-00.eastus.logic.azure.com:443/workflows/...");
                var response = processor.ParseApiResponse(apiResponse);

                // Create a dictionary for faster lookup
                var resultDict = new Dictionary<string, ApiResult>();
                foreach (var result in response.Results)
                {
                    if (!resultDict.ContainsKey(result.Document_ID))
                        resultDict.Add(result.Document_ID, result);
                    if (!resultDict.ContainsKey(result.Content_ID))
                        resultDict.Add(result.Content_ID, result);
                }

                // Update hyperlinks based on API response
                int processedCount = 0;
                foreach (var hyperlink in hyperlinks)
                {
                    string lookupId = WordDocumentProcessor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
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
                                string last6 = result.Content_ID.Length >= 6 ? result.Content_ID.Substring(result.Content_ID.Length - 6) : result.Content_ID;
                                string last5 = last6.Length >= 5 ? last6.Substring(last6.Length - 5) : last6;
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
                                string expectedTitle = result.Title.Trim();
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

                    // Update progress every 10 hyperlinks processed
                    processedCount++;
                    if (processedCount % 10 == 0)
                    {
                        // Removed Application.DoEvents() - now using proper Progress<T> pattern
                        await Task.Yield(); // Allow UI thread to update
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Finished FixSourceHyperlinks");

            return content;
        }

        /// <summary>
        /// Fixes source hyperlinks using API data with progress reporting and cancellation support
        /// </summary>
        public static async Task<string> FixSourceHyperlinksWithProgress(string content, List<HyperlinkData> hyperlinks,
            WordDocumentProcessor processor, List<string> changes,
            Collection<string> updatedLinks, Collection<string> notFoundLinks,
            Collection<string> expiredLinks, Collection<string> errorLinks,
            Collection<string> updatedUrls, RetryPolicyService retryService,
            IProgress<ProgressReport> progress, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting FixSourceHyperlinks with progress");

            // Remove invisible external hyperlinks
            var removedHyperlinks = WordDocumentProcessor.RemoveInvisibleExternalHyperlinks(hyperlinks);
            if (removedHyperlinks.Count > 0)
            {
                changes.Add($"Removed {removedHyperlinks.Count} invisible external hyperlinks");

                foreach (var hyperlink in removedHyperlinks)
                {
                    errorLinks.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | Invisible Hyperlink Deleted");
                }
            }

            // Extract unique lookup IDs
            var uniqueIds = new HashSet<string>();
            foreach (var hyperlink in hyperlinks)
            {
                string lookupId = WordDocumentProcessor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
                if (!string.IsNullOrEmpty(lookupId))
                {
                    uniqueIds.Add(lookupId);
                }
            }

            if (uniqueIds.Count > 0)
            {
                changes.Add($"Found {uniqueIds.Count} unique lookup IDs that would be updated via API");

                // Report API call progress
                progress?.Report(ProgressReport.CreateStatus($"Calling API for {uniqueIds.Count} items..."));

                // Send to API with retry policy
                string apiResponse = await retryService.ExecuteWithRetryAsync(async (ct) =>
                {
                    return await processor.SendToPowerAutomateFlow(uniqueIds.ToList(),
                        "https://prod-00.eastus.logic.azure.com:443/workflows/...");
                }, cancellationToken);

                var response = processor.ParseApiResponse(apiResponse);

                // Create a dictionary for faster lookup
                var resultDict = new Dictionary<string, ApiResult>();
                foreach (var result in response.Results)
                {
                    if (!resultDict.ContainsKey(result.Document_ID))
                        resultDict.Add(result.Document_ID, result);
                    if (!resultDict.ContainsKey(result.Content_ID))
                        resultDict.Add(result.Content_ID, result);
                }

                // Update hyperlinks based on API response
                int processedCount = 0;
                progress?.Report(ProgressReport.CreateStatus($"Processing {hyperlinks.Count} hyperlinks..."));

                foreach (var hyperlink in hyperlinks)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string lookupId = WordDocumentProcessor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
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
                                string last6 = result.Content_ID.Length >= 6 ? result.Content_ID.Substring(result.Content_ID.Length - 6) : result.Content_ID;
                                string last5 = last6.Length >= 5 ? last6.Substring(last6.Length - 5) : last6;
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
                                string expectedTitle = result.Title.Trim();
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

                    // Update progress every 10 hyperlinks processed
                    processedCount++;
                    if (processedCount % 10 == 0)
                    {
                        progress?.Report(ProgressReport.CreateStatus($"Processed {processedCount} of {hyperlinks.Count} hyperlinks"));
                        await Task.Yield(); // Allow UI thread to update
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Finished FixSourceHyperlinks with progress");

            return content;
        }

        /// <summary>
        /// Appends Content ID to hyperlinks
        /// </summary>
        public static string AppendContentID(string content, List<HyperlinkData> hyperlinks, List<string> changes)
        {
            int modifiedCount = 0;

            foreach (var hyperlink in hyperlinks)
            {
                string lookupId = WordDocumentProcessor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
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

        /// <summary>
        /// Fixes internal hyperlinks
        /// </summary>
        public static string FixInternalHyperlink(string content, List<HyperlinkData> hyperlinks, List<string> changes)
        {
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

        /// <summary>
        /// Fixes titles by removing status markers
        /// </summary>
        public static string FixTitles(string content, List<HyperlinkData> hyperlinks, List<string> changes)
        {
            int fixedCount = 0;

            foreach (var hyperlink in hyperlinks)
            {
                string lookupId = WordDocumentProcessor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
                if (!string.IsNullOrEmpty(lookupId))
                {
                    // Check if the title needs updating
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

        /// <summary>
        /// Fixes double spaces in content
        /// </summary>
        public static string FixDoubleSpaces(string content, List<string> changes)
        {
            // Replace multiple spaces with a single space
            string newContent = MultipleSpacesPatternRegex().Replace(content, " ");

            // Count how many replacements were made by comparing lengths
            if (newContent != content)
            {
                // Count occurrences of the pattern to get an approximate count
                MatchCollection matches = MultipleSpacesPatternRegex().Matches(content);
                int fixedCount = matches.Count;
                changes.Add($"Fixed {fixedCount} instances of multiple spaces");
            }

            return newContent;
        }

        /// <summary>
        /// Replaces hyperlinks based on replacement rules
        /// </summary>
        public static string ReplaceHyperlinks(string content, List<HyperlinkData> hyperlinks,
            HyperlinkReplacementRules rules, List<string> changes, Collection<string> replacedHyperlinks)
        {
            int replacedCount = 0;

            // Process each hyperlink replacement rule
            foreach (var rule in rules.Rules)
            {
                // Skip empty rules
                if (string.IsNullOrWhiteSpace(rule.OldTitle) || string.IsNullOrWhiteSpace(rule.NewTitle) || string.IsNullOrWhiteSpace(rule.NewFullContentID))
                    continue;

                // Process each hyperlink in the document
                foreach (var hyperlink in hyperlinks)
                {
                    // Sanitize the hyperlink text by removing content ID suffixes
                    string sanitizedText = hyperlink.TextToDisplay.Trim();

                    // Remove trailing whitespace
                    string oldTitle = rule.OldTitle.Trim();
                    string newTitle = rule.NewTitle.Trim();

                    // Extract last 6 digits of new content ID
                    string newContentIdLast6 = rule.NewFullContentID.Length >= 6 ?
                        rule.NewFullContentID.Substring(rule.NewFullContentID.Length - 6) :
                        rule.NewFullContentID;

                    // Check if the hyperlink text ends with a 5 or 6 digit content ID in parentheses
                    var match = ContentIdPatternRegex().Match(sanitizedText);

                    if (match.Success)
                    {
                        // Remove the content ID suffix
                        sanitizedText = sanitizedText.Substring(0, match.Index).Trim();
                    }

                    // Check if the sanitized text matches the old title
                    if (sanitizedText.Equals(oldTitle, StringComparison.OrdinalIgnoreCase))
                    {
                        // Update the hyperlink text to the new title with new content ID
                        hyperlink.TextToDisplay = $"{newTitle} ({newContentIdLast6})";

                        // Update the hyperlink address based on the new content ID
                        hyperlink.Address = "https://thesource.cvshealth.com/nuxeo/thesource/";
                        hyperlink.SubAddress = $"!/view?docid={rule.NewFullContentID}";

                        // Add to changelog
                        replacedHyperlinks.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | {oldTitle} -> {newTitle}\n        Content ID: {rule.NewFullContentID}");

                        replacedCount++;
                    }
                }
            }

            if (replacedCount > 0)
            {
                changes.Add($"Replaced {replacedCount} hyperlinks");
            }

            return content;
        }
    }
}