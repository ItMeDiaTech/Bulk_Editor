using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bulk_Editor.Configuration;
using Bulk_Editor.Models;
using Bulk_Editor.Services.Abstractions;

namespace Bulk_Editor.Services
{
    public class LogViewerService : ILogViewerService
    {
        private readonly ILoggingService _loggingService;
        private readonly LoggingSettings _logSettings;
        
        private static readonly Regex LogLineRx = new(
            @"^\[(?<ts>[^\]]+)\]\s*\[(?<lvl>[^\]]+)\]\s*(?<msg>.*)$",
            RegexOptions.Compiled);

        public LogViewerService(ILoggingService loggingService, LoggingSettings logSettings)
        {
            _loggingService = loggingService;
            _logSettings = logSettings;
        }

        public async Task<List<LogEntry>> GetRecentLogsAsync(int lineCount)
        {
            var result = new List<LogEntry>();
            var logPath = GetLogFilePath();
            
            if (string.IsNullOrWhiteSpace(logPath) || !File.Exists(logPath))
                return result;

            const int maxRetries = 3;
            int retryDelay = 100;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

                    // Read all lines
                    var raw = new List<string>();
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                        raw.Add(line);

                    // Collapse multi-line entries: a new entry starts when a line matches the regex
                    var entries = new List<(string ts, string lvl, StringBuilder msg)>();
                    foreach (var l in raw)
                    {
                        var m = LogLineRx.Match(l);
                        if (m.Success)
                        {
                            entries.Add((m.Groups["ts"].Value, m.Groups["lvl"].Value, new StringBuilder(m.Groups["msg"].Value)));
                        }
                        else if (entries.Count > 0)
                        {
                            entries[^1].msg.AppendLine().Append(l); // attach stack trace / continuation
                        }
                    }

                    // Take the most recent N (from end), then return chronological order
                    foreach (var e in entries.AsEnumerable().Reverse().Take(lineCount).Reverse())
                    {
                        // Be tolerant of formats: don't drop on parse; store current time if parse fails
                        DateTime ts;
                        if (!DateTime.TryParse(e.ts, System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.AssumeLocal, out ts))
                        {
                            // fallback: try local culture or just set to now
                            if (!DateTime.TryParse(e.ts, out ts))
                                ts = DateTime.Now;
                        }

                        result.Add(new LogEntry
                        {
                            Timestamp = ts,
                            Level = e.lvl,
                            Message = e.msg.ToString()
                        });
                    }

                    return result;
                }
                catch (IOException ex) when (attempt < maxRetries)
                {
                    _loggingService.LogWarning(ex, "Attempt {Attempt}: Log file locked, retrying in {Delay}ms. Path: {Path}", attempt, retryDelay, logPath);
                    await Task.Delay(retryDelay);
                    retryDelay *= 2;
                }
                catch (Exception ex)
                {
                    _loggingService.LogError(ex, "Unexpected error reading log file: {Path}", logPath);
                    break;
                }
            }

            return result;
        }

        public async Task<bool> ExportLogsAsync(string exportPath)
        {
            try
            {
                var logPath = GetLogFilePath();
                if (string.IsNullOrWhiteSpace(logPath) || !File.Exists(logPath))
                    return false;

                // Expand env vars and normalize
                exportPath = Environment.ExpandEnvironmentVariables(exportPath);
                if (!Path.IsPathRooted(exportPath))
                    exportPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, exportPath));

                // If a directory was provided, append filename
                if (Directory.Exists(exportPath) || exportPath.EndsWith(Path.DirectorySeparatorChar.ToString()) || exportPath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                {
                    exportPath = Path.Combine(exportPath, Path.GetFileName(logPath));
                }

                // Ensure destination directory exists
                var destDir = Path.GetDirectoryName(exportPath);
                if (!string.IsNullOrEmpty(destDir))
                    Directory.CreateDirectory(destDir);

                await Task.Run(() => File.Copy(logPath, exportPath, overwrite: true));
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Failed to export log file to {ExportPath}", exportPath);
                return false;
            }
        }

        public async Task<int> ClearOldLogsAsync(int daysToKeep)
        {
            int deletedCount = 0;
            string? logDirectory = Path.GetDirectoryName(GetLogFilePath());

            if (!Directory.Exists(logDirectory))
            {
                return 0;
            }

            // Handle multiple log file patterns (*.log, *.txt, *.log.*)
            var patterns = new[] { "*.log", "*.txt", "*.log.*" };
            var filesToDelete = new List<string>();

            foreach (var pattern in patterns)
            {
                try
                {
                    filesToDelete.AddRange(Directory.GetFiles(logDirectory, pattern)
                        .Where(file => File.GetLastWriteTime(file) < DateTime.Now.AddDays(-daysToKeep)));
                }
                catch (Exception ex)
                {
                    _loggingService.LogWarning(ex, "Failed to search for log files with pattern {Pattern}", pattern);
                }
            }

            foreach (var file in filesToDelete.Distinct())
            {
                try
                {
                    await Task.Run(() => File.Delete(file));
                    deletedCount++;
                    _loggingService.LogInformation("Deleted old log file: {FileName}", Path.GetFileName(file));
                }
                catch (Exception ex)
                {
                    _loggingService.LogError(ex, "Failed to delete old log file: {FilePath}", file);
                }
            }
            
            return deletedCount;
        }

        private string GetLogFilePath()
        {
            if (_logSettings?.LogFilePath == null)
                return string.Empty;

            var p = Environment.ExpandEnvironmentVariables(_logSettings.LogFilePath);

            // If it's a directory, choose the newest *.log inside
            if (Directory.Exists(p))
            {
                var latest = new DirectoryInfo(p).GetFiles("*.log")
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .FirstOrDefault();
                return latest?.FullName ?? string.Empty;
            }

            if (!Path.IsPathRooted(p))
                p = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, p);

            return p;
        }

        public async Task<string> GetChangelogForFileAsync(string changelogPath, string fileName)
        {
            const int maxRetries = 3;
            int retryDelay = 250;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    string[] changelogLines = await File.ReadAllLinesAsync(changelogPath);
                    return ParseChangelogContent(changelogLines, fileName);
                }
                catch (IOException ex) when (attempt < maxRetries)
                {
                    _loggingService.LogWarning(ex, "Attempt {Attempt}: File was locked, retrying in {RetryDelay}ms. Path: {ChangelogPath}", attempt, retryDelay, changelogPath);
                    await Task.Delay(retryDelay);
                    retryDelay *= 2;
                }
                catch (Exception ex)
                {
                    _loggingService.LogError(ex, "An unexpected error occurred while reading the changelog for {FileName} from {Path}", fileName, changelogPath);
                    // Re-throw to be caught by the UI layer, which will show a message box
                    throw;
                }
            }

            // This part is reached only if all retries fail with an IOException
            var finalException = new IOException($"Could not read the changelog file '{changelogPath}' after {maxRetries} attempts as it remained locked.");
            _loggingService.LogError(finalException, "All retries failed to read changelog file.");
            throw finalException;
        }

        private string ParseChangelogContent(string[] lines, string fileName)
        {
            var fileChangelog = new StringBuilder();
            bool foundFileSection = false;
            var targetTitle = $"Title: {Path.GetFileNameWithoutExtension(fileName)}";

            foreach (string line in lines)
            {
                if (!foundFileSection && line.Equals(targetTitle, StringComparison.OrdinalIgnoreCase))
                {
                    foundFileSection = true;
                    fileChangelog.AppendLine(line);
                }
                else if (foundFileSection)
                {
                    // Stop at the start of the next file's log
                    if (line.StartsWith("Title:", StringComparison.OrdinalIgnoreCase) && !line.Equals(targetTitle, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    // Filter out unwanted lines from the UI display
                    if (IsSkippableLine(line))
                    {
                        continue;
                    }

                    fileChangelog.AppendLine(line);
                }
            }
            
            return fileChangelog.Length > 0
                ? fileChangelog.ToString().TrimEnd()
                : $"No changelog entries found for {fileName}.";
        }

        private bool IsSkippableLine(string line)
        {
            var trimmedLine = line.Trim();
            // Filter out summary lines and document separators
            return (trimmedLine.StartsWith("Processed ") && trimmedLine.EndsWith(" files.")) ||
                   trimmedLine == "__________";
        }
        
        // Removed ParseLogEntry - logic moved to GetRecentLogsAsync for better performance
    }
}