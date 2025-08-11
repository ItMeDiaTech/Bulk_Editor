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
            Collection<string> updatedUrls, Configuration.ApiSettings apiSettings = null)
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

                // Send to API using configured endpoint
                string apiResponse = await processor.SendToPowerAutomateFlow(uniqueIds.ToList());
                var response = processor.ParseApiResponse(apiResponse);

                // Use the new centralized method to update hyperlinks
                var updatedHyperlinks = WordDocumentProcessor.UpdateHyperlinksFromApiResponse(hyperlinks, response, changes, apiSettings);

                // Copy the updated hyperlinks back to the original list
                for (int i = 0; i < hyperlinks.Count && i < updatedHyperlinks.Count; i++)
                {
                    hyperlinks[i] = updatedHyperlinks[i];
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
            IProgress<ProgressReport> progress, CancellationToken cancellationToken, Configuration.ApiSettings apiSettings = null)
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

                // Send to API with retry policy using configured endpoint
                string apiResponse = await retryService.ExecuteWithRetryAsync(async (ct) =>
                {
                    return await processor.SendToPowerAutomateFlow(uniqueIds.ToList());
                }, cancellationToken);

                var response = processor.ParseApiResponse(apiResponse);

                // Use the new centralized method to update hyperlinks
                progress?.Report(ProgressReport.CreateStatus($"Processing {hyperlinks.Count} hyperlinks..."));
                var updatedHyperlinks = WordDocumentProcessor.UpdateHyperlinksFromApiResponse(hyperlinks, response, changes, apiSettings);

                // Copy the updated hyperlinks back to the original list with progress reporting
                for (int i = 0; i < hyperlinks.Count && i < updatedHyperlinks.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    hyperlinks[i] = updatedHyperlinks[i];

                    // Update progress every 10 hyperlinks processed
                    if ((i + 1) % 10 == 0)
                    {
                        progress?.Report(ProgressReport.CreateStatus($"Processed {i + 1} of {hyperlinks.Count} hyperlinks"));
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
        public static string AppendContentIDToHyperlinks(List<HyperlinkData> hyperlinks)
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

            return $"Appended Content ID to {modifiedCount} links";
        }

        /// <summary>
        /// Fixes internal hyperlinks by ensuring they point to valid anchors
        /// </summary>
        public static string FixInternalHyperlink(string content, List<HyperlinkData> hyperlinks, List<string> changes)
        {
            int fixedCount = 0;
            int brokenCount = 0;

            // Collect all valid anchors/bookmarks from the hyperlinks
            var validAnchors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // First pass: collect all valid anchor destinations
            foreach (var hyperlink in hyperlinks)
            {
                // Check for bookmarked locations (hyperlinks that can be targets)
                if (!string.IsNullOrEmpty(hyperlink.SubAddress))
                {
                    // Remove common prefixes to get the actual bookmark name
                    string anchor = hyperlink.SubAddress;
                    if (anchor.StartsWith('_'))
                        anchor = anchor.Substring(1);
                    if (anchor.StartsWith('!'))
                        anchor = anchor.Substring(1);

                    validAnchors.Add(anchor);
                }
            }

            // Second pass: fix broken internal hyperlinks
            foreach (var hyperlink in hyperlinks)
            {
                // Check if it's an internal hyperlink (empty address but has sub-address)
                if (string.IsNullOrEmpty(hyperlink.Address) && !string.IsNullOrEmpty(hyperlink.SubAddress))
                {
                    string originalSubAddress = hyperlink.SubAddress;
                    string cleanedAnchor = originalSubAddress;

                    // Remove prefixes to get the actual bookmark name
                    if (cleanedAnchor.StartsWith('_'))
                        cleanedAnchor = cleanedAnchor.Substring(1);
                    if (cleanedAnchor.StartsWith('!'))
                        cleanedAnchor = cleanedAnchor.Substring(1);

                    // Check if the anchor exists
                    if (!validAnchors.Contains(cleanedAnchor))
                    {
                        // Try to find a similar anchor (case-insensitive match)
                        var matchingAnchor = validAnchors.FirstOrDefault(a =>
                            string.Equals(a, cleanedAnchor, StringComparison.OrdinalIgnoreCase));

                        if (matchingAnchor != null)
                        {
                            // Fix the case mismatch
                            hyperlink.SubAddress = "_" + matchingAnchor;
                            fixedCount++;
                            changes.Add($"Fixed internal hyperlink case: {originalSubAddress} -> {hyperlink.SubAddress}");
                        }
                        else
                        {
                            // Try to find a partial match
                            var partialMatch = validAnchors.FirstOrDefault(a =>
                                a.IndexOf(cleanedAnchor, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                cleanedAnchor.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0);

                            if (partialMatch != null)
                            {
                                hyperlink.SubAddress = "_" + partialMatch;
                                fixedCount++;
                                changes.Add($"Fixed internal hyperlink (partial match): {originalSubAddress} -> {hyperlink.SubAddress}");
                            }
                            else
                            {
                                // Mark as broken if no match found
                                brokenCount++;
                                changes.Add($"Broken internal hyperlink detected: {originalSubAddress}");

                                // Optionally remove the broken link or mark it
                                if (!hyperlink.TextToDisplay.Contains(" - Broken"))
                                {
                                    hyperlink.TextToDisplay += " - Broken ";
                                }
                            }
                        }
                    }
                    else
                    {
                        // Ensure proper formatting with underscore prefix
                        if (!originalSubAddress.StartsWith('_') && !originalSubAddress.StartsWith('!'))
                        {
                            hyperlink.SubAddress = "_" + cleanedAnchor;
                            fixedCount++;
                            changes.Add($"Standardized internal hyperlink format: {originalSubAddress} -> {hyperlink.SubAddress}");
                        }
                    }
                }
            }

            if (fixedCount > 0)
            {
                changes.Add($"Fixed {fixedCount} internal hyperlinks");
            }
            if (brokenCount > 0)
            {
                changes.Add($"Found {brokenCount} broken internal hyperlinks");
            }

            return content;
        }

        /// <summary>
        /// Skips processing of already-processed hyperlinks (those with status markers)
        /// </summary>
        public static string SkipProcessedHyperlinks(string content, List<HyperlinkData> hyperlinks, List<string> changes)
        {
            int skippedExpiredCount = 0;
            int skippedNotFoundCount = 0;

            foreach (var hyperlink in hyperlinks)
            {
                string lookupId = WordDocumentProcessor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
                if (!string.IsNullOrEmpty(lookupId))
                {
                    // Check if already processed (has status markers)
                    if (!string.IsNullOrEmpty(hyperlink.TextToDisplay))
                    {
                        if (hyperlink.TextToDisplay.Contains(" - Expired"))
                        {
                            skippedExpiredCount++;
                            changes.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | Already marked as Expired: {hyperlink.TextToDisplay}");
                        }
                        else if (hyperlink.TextToDisplay.Contains(" - Not Found"))
                        {
                            skippedNotFoundCount++;
                            changes.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | Already marked as Not Found: {hyperlink.TextToDisplay}");
                        }
                    }
                }
            }

            if (skippedExpiredCount > 0)
            {
                changes.Add($"Skipped {skippedExpiredCount} already-expired hyperlinks");
            }
            if (skippedNotFoundCount > 0)
            {
                changes.Add($"Skipped {skippedNotFoundCount} already-not-found hyperlinks");
            }

            return content;
        }

        /// <summary>
        /// Detects possible title changes by comparing current titles with API results
        /// </summary>
        public static string DetectTitleChanges(string content, List<HyperlinkData> hyperlinks,
            Dictionary<string, ApiResult> apiResults, List<string> changes, Collection<string> titleChangesList)
        {
            int possibleChangesCount = 0;

            foreach (var hyperlink in hyperlinks)
            {
                string lookupId = WordDocumentProcessor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
                if (!string.IsNullOrEmpty(lookupId) && apiResults != null && apiResults.ContainsKey(lookupId))
                {
                    var result = apiResults[lookupId];

                    // Skip if already has status markers
                    if (hyperlink.TextToDisplay.Contains(" - Expired") ||
                        hyperlink.TextToDisplay.Contains(" - Not Found"))
                    {
                        continue;
                    }

                    string currentTitle = hyperlink.TextToDisplay?.Trim() ?? "";
                    string apiTitle = result.Title?.Trim() ?? "";

                    // Remove Content ID pattern from both titles for comparison
                    string currentTitleForComparison = RemoveContentIdFromTitle(currentTitle).Trim();
                    string apiTitleForComparison = RemoveContentIdFromTitle(apiTitle).Trim();

                    // Only detect changes if titles are actually different (ignoring Content IDs and whitespace)
                    if (!string.IsNullOrEmpty(apiTitleForComparison) &&
                        !currentTitleForComparison.Equals(apiTitleForComparison, StringComparison.OrdinalIgnoreCase))
                    {
                        possibleChangesCount++;
                        string changeEntry = $"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | Possible Title Change\n" +
                            $"        Current Title: {currentTitle}\n" +
                            $"        New Title:     {apiTitle}\n" +
                            $"        Content ID:    {result.Content_ID}";

                        changes.Add(changeEntry);
                        titleChangesList?.Add(changeEntry);
                    }
                }
            }

            if (possibleChangesCount > 0)
            {
                changes.Add($"Detected {possibleChangesCount} possible title changes");
            }

            return content;
        }

        /// <summary>
        /// Updates titles based on API results (actually changes them)
        /// </summary>
        public static string UpdateTitles(string content, List<HyperlinkData> hyperlinks,
            Dictionary<string, ApiResult> apiResults, List<string> changes)
        {
            int updatedCount = 0;

            foreach (var hyperlink in hyperlinks)
            {
                string lookupId = WordDocumentProcessor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
                if (!string.IsNullOrEmpty(lookupId) && apiResults != null && apiResults.ContainsKey(lookupId))
                {
                    var result = apiResults[lookupId];

                    // Skip if already has status markers
                    if (hyperlink.TextToDisplay.Contains(" - Expired") ||
                        hyperlink.TextToDisplay.Contains(" - Not Found"))
                    {
                        continue;
                    }

                    // Extract content ID if present
                    string contentIdSuffix = "";
                    var contentIdMatch = ContentIdPatternRegex().Match(hyperlink.TextToDisplay);
                    if (contentIdMatch.Success)
                    {
                        contentIdSuffix = hyperlink.TextToDisplay.Substring(contentIdMatch.Index);
                    }

                    string apiTitle = result.Title?.Trim() ?? "";
                    string currentTitle = hyperlink.TextToDisplay?.Trim() ?? "";

                    // Remove Content ID pattern from both titles for comparison
                    string currentTitleForComparison = RemoveContentIdFromTitle(currentTitle).Trim();
                    string apiTitleForComparison = RemoveContentIdFromTitle(apiTitle).Trim();

                    // Only update if titles are actually different (ignoring Content IDs and whitespace)
                    if (!string.IsNullOrEmpty(apiTitleForComparison) && currentTitleForComparison != apiTitleForComparison)
                    {
                        string newDisplayText = apiTitle + contentIdSuffix;
                        string oldTitle = hyperlink.TextToDisplay;
                        hyperlink.TextToDisplay = newDisplayText;
                        updatedCount++;
                        changes.Add($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | Title Updated: {oldTitle} → {newDisplayText}");
                    }
                }
            }

            if (updatedCount > 0)
            {
                changes.Add($"Updated {updatedCount} titles");
            }

            return content;
        }

        /// <summary>
        /// Helper method to remove Content ID pattern from title for comparison
        /// </summary>
        private static string RemoveContentIdFromTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                return title;

            // Remove Content ID pattern (5 or 6 digits in parentheses at the end)
            return Regex.Replace(title, @"\s*\(\d{5,6}\)\s*$", "");
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
                        sanitizedText = sanitizedText[..match.Index].Trim();
                    }

                    // Check if the sanitized text matches the old title
                    if (sanitizedText.Equals(oldTitle, StringComparison.OrdinalIgnoreCase))
                    {
                        // Update the hyperlink text to the new title with new content ID
                        hyperlink.TextToDisplay = $"{newTitle} ({newContentIdLast6})";

                        // Update the hyperlink address based on the new content ID
                        // Use configured URLs from ApiSettings instead of hard-coded values
                        var settings = new Configuration.ApiSettings(); // Use default if none provided
                        hyperlink.Address = settings.HyperlinkBaseUrl;
                        hyperlink.SubAddress = settings.HyperlinkViewPath + rule.NewFullContentID;

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