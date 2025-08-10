using System;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Bulk_Editor.Services
{
    /// <summary>
    /// Extension methods for WordDocumentProcessor to handle additional text processing
    /// </summary>
    public static class WordDocumentProcessorExtensions
    {
        private static readonly Regex MultipleSpacesRegex = new Regex(@"[ ]{2,}", RegexOptions.Compiled);

        /// <summary>
        /// Fixes double (or multiple) spaces in a Word document
        /// </summary>
        /// <param name="filePath">Path to the Word document</param>
        /// <returns>Number of fixes made</returns>
        public static int FixDoubleSpacesInDocument(string filePath)
        {
            int fixedCount = 0;

            try
            {
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, true))
                {
                    var body = wordDoc.MainDocumentPart.Document.Body;

                    // Process all text elements in the document
                    var textElements = body.Descendants<Text>().ToList();

                    foreach (var textElement in textElements)
                    {
                        if (textElement.Text != null && MultipleSpacesRegex.IsMatch(textElement.Text))
                        {
                            string originalText = textElement.Text;
                            string fixedText = MultipleSpacesRegex.Replace(originalText, " ");

                            if (originalText != fixedText)
                            {
                                textElement.Text = fixedText;

                                // Count the number of replacements
                                var matches = MultipleSpacesRegex.Matches(originalText);
                                fixedCount += matches.Count;
                            }
                        }
                    }

                    // Also check and fix spaces in paragraphs with multiple runs
                    var paragraphs = body.Descendants<Paragraph>().ToList();

                    foreach (var paragraph in paragraphs)
                    {
                        var runs = paragraph.Elements<Run>().ToList();

                        // Check for spaces between runs
                        for (int i = 0; i < runs.Count - 1; i++)
                        {
                            var currentRun = runs[i];
                            var nextRun = runs[i + 1];

                            var currentText = currentRun.GetFirstChild<Text>();
                            var nextText = nextRun.GetFirstChild<Text>();

                            if (currentText != null && nextText != null)
                            {
                                // Check if current run ends with space and next run starts with space
                                if (currentText.Text.EndsWith(" ") && nextText.Text.StartsWith(" "))
                                {
                                    // Remove leading space from next run
                                    nextText.Text = nextText.Text.TrimStart();
                                    fixedCount++;

                                    // If next text becomes empty, preserve at least one space
                                    if (string.IsNullOrEmpty(nextText.Text) && i + 2 < runs.Count)
                                    {
                                        // Remove the empty run
                                        nextRun.Remove();
                                        runs.RemoveAt(i + 1);
                                        i--; // Adjust index since we removed an element
                                    }
                                }
                            }
                        }
                    }

                    // Save the document if any changes were made
                    if (fixedCount > 0)
                    {
                        wordDoc.MainDocumentPart.Document.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error fixing double spaces in document: {ex.Message}", ex);
            }

            return fixedCount;
        }

        /// <summary>
        /// Counts double (or multiple) spaces in a Word document without fixing them
        /// </summary>
        /// <param name="filePath">Path to the Word document</param>
        /// <returns>Number of double spaces found</returns>
        public static int CountDoubleSpacesInDocument(string filePath)
        {
            int count = 0;

            try
            {
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false))
                {
                    var body = wordDoc.MainDocumentPart.Document.Body;
                    var textElements = body.Descendants<Text>();

                    foreach (var textElement in textElements)
                    {
                        if (textElement.Text != null)
                        {
                            var matches = MultipleSpacesRegex.Matches(textElement.Text);
                            count += matches.Count;
                        }
                    }

                    // Also check for spaces between runs
                    var paragraphs = body.Descendants<Paragraph>();

                    foreach (var paragraph in paragraphs)
                    {
                        var runs = paragraph.Elements<Run>().ToList();

                        for (int i = 0; i < runs.Count - 1; i++)
                        {
                            var currentText = runs[i].GetFirstChild<Text>();
                            var nextText = runs[i + 1].GetFirstChild<Text>();

                            if (currentText?.Text != null && nextText?.Text != null)
                            {
                                if (currentText.Text.EndsWith(" ") && nextText.Text.StartsWith(" "))
                                {
                                    count++;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error counting double spaces: {ex.Message}");
            }

            return count;
        }
    }
}