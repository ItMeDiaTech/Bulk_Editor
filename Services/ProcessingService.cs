#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bulk_Editor.Models;

namespace Bulk_Editor.Services
{
    /// <summary>
    /// Service class for processing document content with unified changelog formatting
    /// </summary>
    public static partial class ProcessingService
    {
        #region Constants & Regex Patterns
        [GeneratedRegex(@"\s*\((\d{5,6})\)\s*$")]
        private static partial Regex ContentIdPatternRegex();

        [GeneratedRegex(@"[ ]{2,}")]
        private static partial Regex MultipleSpacesPatternRegex();

        private const string ExpiredMarker = " - Expired";
        private const string NotFoundMarker = " - Not Found";
        private const string BrokenMarker = " - Broken";
        #endregion

        #region Changelog Helper Methods
        /// <summary>
        /// Creates a standardized changelog entry with proper formatting
        /// </summary>
        private static string CreateChangelogEntry(HyperlinkData hyperlink, string note, string? title = null, string? contentId = null)
        {
            var sb = new StringBuilder();
            sb.Append("    Page:").Append(hyperlink.PageNumber)
              .Append(" | Line:").Append(hyperlink.LineNumber)
              .Append(" | ").Append(note);

            if (!string.IsNullOrEmpty(title))
            {
                sb.Append("\n        Title: ").Append(title);
            }

            if (!string.IsNullOrEmpty(contentId))
            {
                sb.Append("\n        Content ID: ").Append(contentId);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates a changelog entry for current title display
        /// </summary>
        private static string CreateCurrentTitleEntry(HyperlinkData hyperlink, string note)
        {
            return $"    Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | {note}\n" +
                   $"        Current Title:    {hyperlink.TextToDisplay}";
        }

        /// <summary>
        /// Creates a changelog entry for title mismatches
        /// </summary>
        private static string CreateTitleMismatchEntry(HyperlinkData hyperlink, string currentTitle, string correctTitle, string contentId)
        {
            return $"    Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | Title Mismatch - Please Review\n" +
                   $"        Current Title:    {currentTitle}\n" +
                   $"        Correct Title:    {correctTitle}\n" +
                   $"        Content ID:    {contentId}";
        }
        #endregion

        #region Common Helper Methods
        /// <summary>
        /// Generates a unique key for tracking hyperlinks
        /// </summary>
        private static string GetHyperlinkKey(HyperlinkData hyperlink) => $"{hyperlink.PageNumber}:{hyperlink.LineNumber}";

        /// <summary>
        /// Checks if hyperlink has status markers
        /// </summary>
        private static bool HasStatusMarkers(string? textToDisplay) =>
            !string.IsNullOrEmpty(textToDisplay) &&
            (textToDisplay.Contains(ExpiredMarker) || textToDisplay.Contains(NotFoundMarker));

        /// <summary>
        /// Extracts the last 6 and 5 characters from content ID
        /// </summary>
        private static (string last6, string last5) ExtractContentIdSuffixes(string lookupId)
        {
            string last6 = lookupId.Length >= 6 ? lookupId[^6..] : lookupId;
            string last5 = last6.Length >= 5 ? last6[^5..] : last6;
            return (last6, last5);
        }

        /// <summary>
        /// Removes Content ID pattern from title for comparison
        /// </summary>
        private static string RemoveContentIdFromTitle(string title)
        {
            if (string.IsNullOrEmpty(title)) return title;
            return ContentIdPatternRegex().Replace(title, "");
        }

        /// <summary>
        /// Processes invisible hyperlinks removal with changelog tracking
        /// </summary>
        private static void ProcessInvisibleHyperlinks(List<HyperlinkData> hyperlinks, List<string> changes, Collection<string> errorLinks)
        {
            var removedHyperlinks = WordDocumentProcessor.RemoveInvisibleExternalHyperlinks(hyperlinks);
            if (removedHyperlinks.Count <= 0) return;

            changes.Add($"Removed {removedHyperlinks.Count} invisible external hyperlinks");
            foreach (var hyperlink in removedHyperlinks)
            {
                errorLinks.Add(CreateChangelogEntry(hyperlink, "Invisible Hyperlink Deleted"));
            }
        }

        /// <summary>
        /// Extracts unique lookup IDs from hyperlinks
        /// </summary>
        private static HashSet<string> ExtractUniqueLookupIds(List<HyperlinkData> hyperlinks)
        {
            return hyperlinks
                .Select(h => WordDocumentProcessor.ExtractLookupID(h.Address, h.SubAddress))
                .Where(id => !string.IsNullOrEmpty(id))
                .ToHashSet();
        }
        #endregion

        #region Legacy BuildLogLine Method (for backward compatibility)
        /// <summary>
        /// Creates a properly formatted log line for changelog entries (legacy method)
        /// </summary>
        public static string BuildLogLine(int pageNumber, int lineNumber, string note, string? title = null, string? contentId = null)
        {
            // Use existing CreateChangelogEntry for consistency
            var mockHyperlink = new HyperlinkData { PageNumber = pageNumber, LineNumber = lineNumber };
            return CreateChangelogEntry(mockHyperlink, note, title, contentId);
        }
        #endregion

        #region Main Processing Methods
        /// <summary>
        /// Fixes source hyperlinks using API data
        /// </summary>
        public static async Task<string> FixSourceHyperlinks(string content, List<HyperlinkData> hyperlinks,
            WordDocumentProcessor processor, List<string> changes,
            Collection<string> updatedLinks, Collection<string> notFoundLinks,
            Collection<string> expiredLinks, Collection<string> errorLinks,
            Collection<string> updatedUrls, Configuration.ApiSettings? apiSettings = null)
        {
            return await ProcessHyperlinks(content, hyperlinks, processor, changes, updatedLinks, notFoundLinks,
                expiredLinks, errorLinks, updatedUrls, apiSettings);
        }

        /// <summary>
        /// Fixes source hyperlinks using API data with progress reporting and cancellation support
        /// </summary>
        public static async Task<string> FixSourceHyperlinksWithProgress(string content, List<HyperlinkData> hyperlinks,
            WordDocumentProcessor processor, List<string> changes,
            Collection<string> updatedLinks, Collection<string> notFoundLinks,
            Collection<string> expiredLinks, Collection<string> errorLinks,
            Collection<string> updatedUrls, RetryPolicyService retryService,
            IProgress<ProgressReport> progress, CancellationToken cancellationToken, Configuration.ApiSettings? apiSettings = null)
        {
            return await ProcessHyperlinks(content, hyperlinks, processor, changes, updatedLinks, notFoundLinks,
                expiredLinks, errorLinks, updatedUrls, apiSettings, retryService, progress, cancellationToken);
        }

        /// <summary>
        /// Core hyperlink processing logic shared between sync and async methods
        /// </summary>
        private static async Task<string> ProcessHyperlinks(string content, List<HyperlinkData> hyperlinks,
            WordDocumentProcessor processor, List<string> changes,
            Collection<string> updatedLinks, Collection<string> notFoundLinks,
            Collection<string> expiredLinks, Collection<string> errorLinks,
            Collection<string> updatedUrls, Configuration.ApiSettings? apiSettings = null,
            RetryPolicyService? retryService = null, IProgress<ProgressReport>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var methodName = retryService != null ? "FixSourceHyperlinks with progress" : "FixSourceHyperlinks";
            var startTime = DateTime.UtcNow;
            var logger = LoggingService.Instance;
            
            logger.LogProcessingStep("Hyperlink Processing Start", $"{methodName} - Processing {hyperlinks.Count} hyperlinks");

            // Remove invisible external hyperlinks
            progress?.Report(ProgressReport.CreateItemProgress(1, 1, "", "Removing invisible hyperlinks", 0, hyperlinks.Count));
            ProcessInvisibleHyperlinks(hyperlinks, changes, errorLinks);
            logger.LogProcessingStep("Invisible Hyperlinks", $"Processed invisible hyperlinks removal");

            // Extract unique lookup IDs
            progress?.Report(ProgressReport.CreateItemProgress(1, 1, "", "Extracting lookup IDs", 0, hyperlinks.Count));
            var uniqueIds = ExtractUniqueLookupIds(hyperlinks);
            logger.LogProcessingStep("Lookup ID Extraction", $"Extracted {uniqueIds.Count} unique lookup IDs from {hyperlinks.Count} hyperlinks");

            if (uniqueIds.Count > 0)
            {
                changes.Add($"Found {uniqueIds.Count} unique lookup IDs that would be updated via API");
                progress?.Report(ProgressReport.CreateApiProgress("Calling PowerAutomate API", uniqueIds.Count));

                var apiStartTime = DateTime.UtcNow;
                logger.LogProcessingStep("API Call Start", $"Sending {uniqueIds.Count} lookup IDs to PowerAutomate API");

                // Send to API using configured endpoint
                string apiResponse = retryService != null
                    ? await retryService.ExecuteWithRetryAsync(async (ct) =>
                        await processor.SendToPowerAutomateFlow(uniqueIds.ToList()), cancellationToken)
                    : await processor.SendToPowerAutomateFlow(uniqueIds.ToList());

                var apiDuration = DateTime.UtcNow - apiStartTime;
                logger.LogApiCall("PowerAutomate", "POST", apiDuration, $"Response received for {uniqueIds.Count} items");

                var response = processor.ParseApiResponse(apiResponse);
                logger.LogProcessingStep("API Response Processing", $"Parsed API response with {response.Results.Count} results");

                // Use the new centralized method to update hyperlinks
                progress?.Report(ProgressReport.CreateItemProgress(1, 1, "", "Updating hyperlinks from API", 0, hyperlinks.Count));
                var updatedHyperlinks = WordDocumentProcessor.UpdateHyperlinksFromApiResponse(hyperlinks, response, changes, apiSettings);

                logger.LogProcessingStep("Hyperlink Updates", $"Updated hyperlinks based on API response");

                // Copy the updated hyperlinks back to the original list
                await CopyUpdatedHyperlinks(hyperlinks, updatedHyperlinks, progress, cancellationToken);
            }
            else
            {
                logger.LogInformation("No lookup IDs found for API processing");
                progress?.Report(ProgressReport.CreateStatus("No API calls needed - no valid lookup IDs found"));
            }

            var totalDuration = DateTime.UtcNow - startTime;
            logger.LogProcessingStep("Hyperlink Processing Complete",
                $"{methodName} completed in {totalDuration.TotalSeconds:F2} seconds");
            logger.LogPerformanceMetric("HyperlinkProcessingTime", totalDuration.TotalMilliseconds, "ms");
            
            return content;
        }

        /// <summary>
        /// Copies updated hyperlinks back with optional progress reporting
        /// </summary>
        private static async Task CopyUpdatedHyperlinks(List<HyperlinkData> originalList, List<HyperlinkData> updatedList,
            IProgress<ProgressReport>? progress = null, CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < originalList.Count && i < updatedList.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                originalList[i] = updatedList[i];

                // Update progress every 10 hyperlinks processed
                if (progress != null && (i + 1) % 10 == 0)
                {
                    progress.Report(ProgressReport.CreateStatus($"Processed {i + 1} of {originalList.Count} hyperlinks"));
                    await Task.Yield(); // Allow UI thread to update
                }
            }
        }
        #endregion

        /// <summary>
        /// Appends Content ID to hyperlinks and tracks changes for changelog
        /// </summary>
        public static string AppendContentIDToHyperlinks(List<HyperlinkData> hyperlinks, Collection<string> updatedLinks, Dictionary<string, bool>? urlUpdatedTracker = null)
        {
            int modifiedCount = 0;

            foreach (var hyperlink in hyperlinks)
            {
                string lookupId = WordDocumentProcessor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
                if (string.IsNullOrEmpty(lookupId)) continue;

                var (last6, last5) = ExtractContentIdSuffixes(lookupId);

                // Check if we need to append the content ID
                if (!string.IsNullOrEmpty(hyperlink.TextToDisplay) && !hyperlink.TextToDisplay.Contains($"({last6})"))
                {
                    // Update the display text
                    UpdateHyperlinkDisplayText(hyperlink, last6, last5);

                    // Handle changelog tracking
                    HandleContentIdChangelogEntry(hyperlink, lookupId, updatedLinks, urlUpdatedTracker);

                    modifiedCount++;
                }
            }

            return modifiedCount.ToString();
        }

        /// <summary>
        /// Updates hyperlink display text with content ID
        /// </summary>
        private static void UpdateHyperlinkDisplayText(HyperlinkData hyperlink, string last6, string last5)
        {
            if (string.IsNullOrEmpty(hyperlink.TextToDisplay))
            {
                hyperlink.TextToDisplay = $"({last6})";
                return;
            }

            if (hyperlink.TextToDisplay.EndsWith($"({last5})"))
            {
                // Replace last5 with last6
                hyperlink.TextToDisplay = hyperlink.TextToDisplay[..^(last5.Length + 2)] + $"{last6})";
            }
            else
            {
                // Append new content ID
                hyperlink.TextToDisplay = $"{hyperlink.TextToDisplay.Trim()} ({last6})";
            }
        }

        /// <summary>
        /// Handles changelog entry for content ID changes
        /// </summary>
        private static void HandleContentIdChangelogEntry(HyperlinkData hyperlink, string lookupId,
            Collection<string> updatedLinks, Dictionary<string, bool>? urlUpdatedTracker)
        {
            string hyperlinkKey = GetHyperlinkKey(hyperlink);
            bool urlWasUpdated = urlUpdatedTracker?.ContainsKey(hyperlinkKey) == true;
            string note = urlWasUpdated ? "Updated URL and Appended Content ID" : "Appended Content ID";

            if (!urlWasUpdated)
            {
                updatedLinks.Add(CreateChangelogEntry(hyperlink, note, hyperlink.TextToDisplay, lookupId));
            }
            else
            {
                // Update existing entry to reflect both actions
                UpdateExistingChangelogEntry(updatedLinks, hyperlink, "Updated URL", "Updated URL and Appended Content ID");
            }
        }

        /// <summary>
        /// Updates an existing changelog entry
        /// </summary>
        private static void UpdateExistingChangelogEntry(Collection<string> entries, HyperlinkData hyperlink, string oldText, string newText)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Contains($"Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber}"))
                {
                    entries[i] = entries[i].Replace(oldText, newText);
                    break;
                }
            }
        }

        /// <summary>
        /// Fixes internal hyperlinks by ensuring they point to valid anchors
        /// </summary>
        public static string FixInternalHyperlink(string content, List<HyperlinkData> hyperlinks, List<string> changes, Collection<string> internalLinks)
        {
            var validAnchors = CollectValidAnchors(hyperlinks);
            ProcessInternalHyperlinks(hyperlinks, validAnchors, internalLinks);
            return content;
        }

        /// <summary>
        /// Collects all valid anchors/bookmarks from hyperlinks
        /// </summary>
        private static HashSet<string> CollectValidAnchors(List<HyperlinkData> hyperlinks)
        {
            var validAnchors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var hyperlink in hyperlinks.Where(h => !string.IsNullOrEmpty(h.SubAddress)))
            {
                string anchor = CleanAnchorName(hyperlink.SubAddress);
                validAnchors.Add(anchor);
            }

            return validAnchors;
        }

        /// <summary>
        /// Processes internal hyperlinks and attempts to fix broken ones
        /// </summary>
        private static void ProcessInternalHyperlinks(List<HyperlinkData> hyperlinks, HashSet<string> validAnchors, Collection<string> internalLinks)
        {
            foreach (var hyperlink in hyperlinks.Where(h => string.IsNullOrEmpty(h.Address) && !string.IsNullOrEmpty(h.SubAddress)))
            {
                ProcessSingleInternalHyperlink(hyperlink, validAnchors, internalLinks);
            }
        }

        /// <summary>
        /// Processes a single internal hyperlink
        /// </summary>
        private static void ProcessSingleInternalHyperlink(HyperlinkData hyperlink, HashSet<string> validAnchors, Collection<string> internalLinks)
        {
            string originalSubAddress = hyperlink.SubAddress;
            string cleanedAnchor = CleanAnchorName(originalSubAddress);

            if (!validAnchors.Contains(cleanedAnchor))
            {
                AttemptInternalHyperlinkFix(hyperlink, validAnchors, internalLinks, cleanedAnchor);
            }
            else if (!originalSubAddress.StartsWith('_') && !originalSubAddress.StartsWith('!'))
            {
                // Standardize format with underscore prefix
                hyperlink.SubAddress = "_" + cleanedAnchor;
                internalLinks.Add(CreateCurrentTitleEntry(hyperlink, "Fixed Internal Hyperlink"));
            }
        }

        /// <summary>
        /// Attempts to fix a broken internal hyperlink
        /// </summary>
        private static void AttemptInternalHyperlinkFix(HyperlinkData hyperlink, HashSet<string> validAnchors,
            Collection<string> internalLinks, string cleanedAnchor)
        {
            // Try exact case-insensitive match
            var exactMatch = validAnchors.FirstOrDefault(a => string.Equals(a, cleanedAnchor, StringComparison.OrdinalIgnoreCase));
            if (exactMatch != null)
            {
                hyperlink.SubAddress = "_" + exactMatch;
                internalLinks.Add(CreateCurrentTitleEntry(hyperlink, "Fixed Internal Hyperlink"));
                return;
            }

            // Try partial match
            var partialMatch = validAnchors.FirstOrDefault(a =>
                a.IndexOf(cleanedAnchor, StringComparison.OrdinalIgnoreCase) >= 0 ||
                cleanedAnchor.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0);

            if (partialMatch != null)
            {
                hyperlink.SubAddress = "_" + partialMatch;
                internalLinks.Add(CreateCurrentTitleEntry(hyperlink, "Attempted Fix, Please Review"));
            }
            else
            {
                // Mark as broken
                internalLinks.Add(CreateCurrentTitleEntry(hyperlink, "Broken Internal Hyperlink, Please Review"));
                if (!string.IsNullOrEmpty(hyperlink.TextToDisplay) && !hyperlink.TextToDisplay.Contains(BrokenMarker))
                {
                    hyperlink.TextToDisplay += BrokenMarker;
                }
                else if (string.IsNullOrEmpty(hyperlink.TextToDisplay))
                {
                    hyperlink.TextToDisplay = BrokenMarker;
                }
            }
        }

        /// <summary>
        /// Cleans anchor name by removing prefixes
        /// </summary>
        private static string CleanAnchorName(string anchor)
        {
            if (anchor.StartsWith('_')) anchor = anchor[1..];
            if (anchor.StartsWith('!')) anchor = anchor[1..];
            return anchor;
        }

        /// <summary>
        /// Skips processing of already-processed hyperlinks (those with status markers)
        /// </summary>
        public static string SkipProcessedHyperlinks(string content, List<HyperlinkData> hyperlinks, List<string> changes)
        {
            foreach (var hyperlink in hyperlinks)
            {
                string lookupId = WordDocumentProcessor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
                if (string.IsNullOrEmpty(lookupId) || string.IsNullOrEmpty(hyperlink.TextToDisplay)) continue;

                if (hyperlink.TextToDisplay.Contains(ExpiredMarker))
                {
                    changes.Add(CreateCurrentTitleEntry(hyperlink, $"Expired Hyperlink, added \"{ExpiredMarker}\" to Title"));
                }
                else if (hyperlink.TextToDisplay.Contains(NotFoundMarker))
                {
                    changes.Add(CreateCurrentTitleEntry(hyperlink, $"Unrecognized Link, added \"{NotFoundMarker}\" to Title"));
                }
            }

            return content;
        }

        /// <summary>
        /// Detects possible title changes by comparing current titles with API results
        /// </summary>
        public static string DetectTitleChanges(string content, List<HyperlinkData> hyperlinks,
            Dictionary<string, ApiResult> apiResults, List<string> changes, Collection<string> titleChangesList)
        {
            if (apiResults == null) return content;

            int possibleChangesCount = 0;

            foreach (var hyperlink in hyperlinks)
            {
                string lookupId = WordDocumentProcessor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
                if (string.IsNullOrEmpty(lookupId) || !apiResults.ContainsKey(lookupId)) continue;

                var result = apiResults[lookupId];
                if (HasStatusMarkers(hyperlink.TextToDisplay)) continue;

                string currentTitle = hyperlink.TextToDisplay?.Trim() ?? "";
                string apiTitle = result.Title?.Trim() ?? "";

                if (AreTitlesDifferent(currentTitle, apiTitle))
                {
                    possibleChangesCount++;
                    string changeEntry = CreateTitleMismatchEntry(hyperlink, currentTitle, apiTitle, result.Content_ID);
                    changes.Add(changeEntry);
                    titleChangesList?.Add(changeEntry);
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
            Dictionary<string, ApiResult> apiResults, List<string> changes, Collection<string> updatedLinks, Dictionary<string, bool> urlUpdatedTracker)
        {
            if (apiResults == null) return content;

            foreach (var hyperlink in hyperlinks)
            {
                string lookupId = WordDocumentProcessor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
                if (string.IsNullOrEmpty(lookupId) || !apiResults.ContainsKey(lookupId)) continue;

                var result = apiResults[lookupId];
                if (HasStatusMarkers(hyperlink.TextToDisplay)) continue;

                string currentTitle = hyperlink.TextToDisplay?.Trim() ?? "";
                string apiTitle = result.Title?.Trim() ?? "";

                if (AreTitlesDifferent(currentTitle, apiTitle))
                {
                    UpdateHyperlinkTitle(hyperlink, apiTitle, updatedLinks, urlUpdatedTracker);
                }
            }

            return content;
        }

        /// <summary>
        /// Checks if two titles are different (ignoring Content IDs and whitespace)
        /// </summary>
        private static bool AreTitlesDifferent(string currentTitle, string apiTitle)
        {
            string currentTitleForComparison = RemoveContentIdFromTitle(currentTitle).Trim();
            string apiTitleForComparison = RemoveContentIdFromTitle(apiTitle).Trim();

            return !string.IsNullOrEmpty(apiTitleForComparison) &&
                   !currentTitleForComparison.Equals(apiTitleForComparison, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Updates a hyperlink's title and tracks the change
        /// </summary>
        private static void UpdateHyperlinkTitle(HyperlinkData hyperlink, string apiTitle,
            Collection<string> updatedLinks, Dictionary<string, bool> urlUpdatedTracker)
        {
            // Extract existing content ID suffix if present
            string contentIdSuffix = "";
            if (!string.IsNullOrEmpty(hyperlink.TextToDisplay))
            {
                var contentIdMatch = ContentIdPatternRegex().Match(hyperlink.TextToDisplay);
                if (contentIdMatch.Success)
                {
                    contentIdSuffix = hyperlink.TextToDisplay[contentIdMatch.Index..];
                }
            }

            string newDisplayText = apiTitle + contentIdSuffix;
            hyperlink.TextToDisplay = newDisplayText;

            // Track this hyperlink as having URL updated
            string hyperlinkKey = GetHyperlinkKey(hyperlink);
            urlUpdatedTracker[hyperlinkKey] = true;

            string lookupIdExtracted = WordDocumentProcessor.ExtractLookupID(hyperlink.Address, hyperlink.SubAddress);
            updatedLinks.Add(CreateChangelogEntry(hyperlink, "Updated URL", newDisplayText, lookupIdExtracted));
        }



        #region Content Processing Methods
        /// <summary>
        /// Fixes double spaces in content
        /// </summary>
        public static string FixDoubleSpaces(string content, List<string> changes)
        {
            var regex = MultipleSpacesPatternRegex();
            var matches = regex.Matches(content);
            
            if (matches.Count > 0)
            {
                changes.Add($"Removed {matches.Count} instances of Extra Spaces");
                return regex.Replace(content, " ");
            }

            return content;
        }

        /// <summary>
        /// Replaces hyperlinks based on replacement rules
        /// </summary>
        public static string ReplaceHyperlinks(string content, List<HyperlinkData> hyperlinks,
            HyperlinkReplacementRules rules, List<string> changes, Collection<string> replacedHyperlinks)
        {
            int replacedCount = 0;

            foreach (var rule in rules.Rules.Where(r => IsValidRule(r)))
            {
                replacedCount += ProcessReplacementRule(hyperlinks, rule, replacedHyperlinks);
            }

            if (replacedCount > 0)
            {
                changes.Add($"Replaced {replacedCount} hyperlinks");
            }

            return content;
        }

        /// <summary>
        /// Validates a replacement rule
        /// </summary>
        private static bool IsValidRule(HyperlinkReplacementRule rule) =>
            !string.IsNullOrWhiteSpace(rule.OldTitle) &&
            !string.IsNullOrWhiteSpace(rule.NewTitle) &&
            !string.IsNullOrWhiteSpace(rule.NewFullContentID);

        /// <summary>
        /// Processes a single replacement rule against all hyperlinks
        /// </summary>
        private static int ProcessReplacementRule(List<HyperlinkData> hyperlinks, HyperlinkReplacementRule rule, Collection<string> replacedHyperlinks)
        {
            int count = 0;
            string oldTitle = rule.OldTitle.Trim();
            string newTitle = rule.NewTitle.Trim();
            var (newContentIdLast6, _) = ExtractContentIdSuffixes(rule.NewFullContentID);

            foreach (var hyperlink in hyperlinks)
            {
                string sanitizedText = SanitizeHyperlinkText(hyperlink.TextToDisplay);

                if (sanitizedText.Equals(oldTitle, StringComparison.OrdinalIgnoreCase))
                {
                    ApplyHyperlinkReplacement(hyperlink, newTitle, newContentIdLast6, rule.NewFullContentID, replacedHyperlinks);
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Sanitizes hyperlink text by removing content ID suffixes
        /// </summary>
        private static string SanitizeHyperlinkText(string? text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            
            string sanitized = text.Trim();
            var match = ContentIdPatternRegex().Match(sanitized);

            return match.Success ? sanitized[..match.Index].Trim() : sanitized;
        }

        /// <summary>
        /// Applies a hyperlink replacement
        /// </summary>
        private static void ApplyHyperlinkReplacement(HyperlinkData hyperlink, string newTitle, string newContentIdLast6,
            string fullContentId, Collection<string> replacedHyperlinks)
        {
            string oldHyperlinkText = hyperlink.TextToDisplay ?? string.Empty;
            string newHyperlinkText = $"{newTitle} ({newContentIdLast6})";

            // Update hyperlink properties
            hyperlink.TextToDisplay = newHyperlinkText;

            var settings = new Configuration.ApiSettings();
            hyperlink.Address = settings.HyperlinkBaseUrl;
            hyperlink.SubAddress = $"{settings.HyperlinkViewPath}{fullContentId}";

            // Add to changelog
            replacedHyperlinks.Add($"    Page:{hyperlink.PageNumber} | Line:{hyperlink.LineNumber} | Replaced Hyperlink based on User Replacement\n" +
                $"        Old Hyperlink: {oldHyperlinkText}\n" +
                $"        New Hyperlink: {newHyperlinkText}");
        }
        #endregion
    }
}