#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Bulk_Editor.Configuration;
using Bulk_Editor.Models;

using Bulk_Editor.Services.Abstractions;

namespace Bulk_Editor.Services
{
    /// <summary>
    /// Provides validation services for input data and files
    /// </summary>
    public class ValidationService : IValidationService
    {
        private static readonly Regex UrlRegex = new(@"^https?:\/\/[^\s/$.?#].[^\s]*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ContentIdRegex = new(@"^[A-Z]{2,4}-[A-Z0-9]+-\d{6}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Validates a file path for processing
        /// </summary>
        public ProcessingResult ValidateFilePath(string filePath, ProcessingSettings settings)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return ProcessingResult.CreateFailure("File path cannot be empty");
            }

            // Normalize path
            try
            {
                filePath = Path.GetFullPath(filePath);
            }
            catch (Exception ex)
            {
                return ProcessingResult.CreateFailure($"Invalid file path: {ex.Message}", ex);
            }

            // Check if file exists
            if (!File.Exists(filePath))
            {
                return ProcessingResult.CreateFailure($"File does not exist: {filePath}");
            }

            // Check file extension
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (!settings.AllowedExtensions.Contains(extension))
            {
                return ProcessingResult.CreateFailure(
                    $"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", settings.AllowedExtensions)}");
            }

            // Check file size
            try
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > settings.MaxFileSizeBytes)
                {
                    return ProcessingResult.CreateFailure(
                        $"File size ({FormatFileSize(fileInfo.Length)}) exceeds maximum allowed size ({FormatFileSize(settings.MaxFileSizeBytes)})");
                }

                // Check if file is readable
                using var stream = File.OpenRead(filePath);
            }
            catch (UnauthorizedAccessException)
            {
                return ProcessingResult.CreateFailure($"Access denied to file: {filePath}");
            }
            catch (Exception ex)
            {
                return ProcessingResult.CreateFailure($"Cannot access file: {ex.Message}", ex);
            }

            return ProcessingResult.CreateSuccess();
        }

        /// <summary>
        /// Validates a directory path for processing
        /// </summary>
        public ProcessingResult ValidateDirectoryPath(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                return ProcessingResult.CreateFailure("Directory path cannot be empty");
            }

            // Normalize path
            try
            {
                directoryPath = Path.GetFullPath(directoryPath);
            }
            catch (Exception ex)
            {
                return ProcessingResult.CreateFailure($"Invalid directory path: {ex.Message}", ex);
            }

            // Check if directory exists
            if (!Directory.Exists(directoryPath))
            {
                return ProcessingResult.CreateFailure($"Directory does not exist: {directoryPath}");
            }

            // Check directory access
            try
            {
                Directory.GetFiles(directoryPath);
            }
            catch (UnauthorizedAccessException)
            {
                return ProcessingResult.CreateFailure($"Access denied to directory: {directoryPath}");
            }
            catch (Exception ex)
            {
                return ProcessingResult.CreateFailure($"Cannot access directory: {ex.Message}", ex);
            }

            return ProcessingResult.CreateSuccess();
        }

        /// <summary>
        /// Validates URL format
        /// </summary>
        public ProcessingResult ValidateUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return ProcessingResult.CreateFailure("URL cannot be empty");
            }

            if (!UrlRegex.IsMatch(url))
            {
                return ProcessingResult.CreateFailure($"Invalid URL format: {url}");
            }

            // Additional security checks
            var uri = new Uri(url);
            if (uri.Scheme != "https")
            {
                return ProcessingResult.CreateFailure("Only HTTPS URLs are allowed for security reasons");
            }

            return ProcessingResult.CreateSuccess();
        }

        /// <summary>
        /// Validates content ID format
        /// </summary>
        public ProcessingResult ValidateContentId(string contentId)
        {
            if (string.IsNullOrWhiteSpace(contentId))
            {
                return ProcessingResult.CreateFailure("Content ID cannot be empty");
            }

            if (!ContentIdRegex.IsMatch(contentId))
            {
                return ProcessingResult.CreateFailure($"Invalid Content ID format: {contentId}. Expected format: XXX-XXXX-XXXXXX");
            }

            return ProcessingResult.CreateSuccess();
        }

        /// <summary>
        /// Validates hyperlink replacement rule
        /// </summary>
        public ProcessingResult ValidateHyperlinkReplacementRule(HyperlinkReplacementRule rule)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(rule.OldTitle))
            {
                errors.Add("Old title cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(rule.NewTitle))
            {
                errors.Add("New title cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(rule.NewFullContentID))
            {
                errors.Add("New content ID cannot be empty");
            }
            else
            {
                var contentIdValidation = ValidateContentId(rule.NewFullContentID);
                if (!contentIdValidation.Success)
                {
                    errors.Add($"Invalid new content ID: {contentIdValidation.ErrorMessage}");
                }
            }

            // Check for dangerous characters that could cause issues
            var dangerousChars = new[] { '<', '>', '"', '&', '\0', '\r', '\n' };
            if (rule.OldTitle?.IndexOfAny(dangerousChars) >= 0)
            {
                errors.Add("Old title contains invalid characters");
            }

            if (rule.NewTitle?.IndexOfAny(dangerousChars) >= 0)
            {
                errors.Add("New title contains invalid characters");
            }

            if (errors.Any())
            {
                return ProcessingResult.CreateFailure($"Validation failed: {string.Join(", ", errors)}");
            }

            return ProcessingResult.CreateSuccess();
        }

        /// <summary>
        /// Validates hyperlink data
        /// </summary>
        public ProcessingResult ValidateHyperlinkData(HyperlinkData hyperlink)
        {
            var warnings = new List<string>();

            // Check for potential issues
            if (string.IsNullOrWhiteSpace(hyperlink.TextToDisplay))
            {
                warnings.Add("Hyperlink has empty display text");
            }

            if (!string.IsNullOrWhiteSpace(hyperlink.Address))
            {
                var urlValidation = ValidateUrl(hyperlink.Address);
                if (!urlValidation.Success)
                {
                    warnings.Add($"Invalid hyperlink address: {urlValidation.ErrorMessage}");
                }
            }

            if (hyperlink.PageNumber <= 0)
            {
                warnings.Add("Invalid page number");
            }

            if (hyperlink.LineNumber <= 0)
            {
                warnings.Add("Invalid line number");
            }

            return ProcessingResult.CreateSuccess(warnings);
        }

        /// <summary>
        /// Validates backup directory creation
        /// </summary>
        public ProcessingResult ValidateBackupDirectory(string basePath, string backupFolderName)
        {
            try
            {
                var backupPath = Path.Combine(basePath, backupFolderName);

                // Ensure backup directory exists
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                // Test write access
                var testFile = Path.Combine(backupPath, $"test_{Guid.NewGuid()}.tmp");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);

                return ProcessingResult.CreateSuccess();
            }
            catch (Exception ex)
            {
                return ProcessingResult.CreateFailure($"Cannot create or access backup directory: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Formats file size for display
        /// </summary>
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

        /// <summary>
        /// Sanitizes file path for safe operations
        /// </summary>
        public string SanitizeFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return string.Empty;

            // Remove invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", filePath.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            // Limit length
            if (sanitized.Length > 255)
            {
                sanitized = sanitized.Substring(0, 255);
            }

            return sanitized;
        }

        /// <summary>
        /// Checks if a path is safe (prevents path traversal attacks)
        /// </summary>
        public bool IsSafePath(string basePath, string targetPath)
        {
            try
            {
                var fullBasePath = Path.GetFullPath(basePath);
                var fullTargetPath = Path.GetFullPath(targetPath);

                return fullTargetPath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}