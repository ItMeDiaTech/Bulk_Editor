#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Bulk_Editor.Configuration;

namespace Bulk_Editor.Services
{
    /// <summary>
    /// Service for reading and displaying application logs
    /// </summary>
    public class LogViewerService
    {
        private readonly LoggingSettings _settings;

        public LogViewerService(LoggingSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Get recent log entries from the current log file
        /// </summary>
        public async Task<List<LogEntry>> GetRecentLogsAsync(int maxEntries = 100)
        {
            var logEntries = new List<LogEntry>();

            try
            {
                string logPath = GetLogFilePath();
                if (!File.Exists(logPath))
                {
                    return logEntries;
                }

                var lines = await File.ReadAllLinesAsync(logPath);
                
                // Take the last N lines and parse them
                var recentLines = lines.TakeLast(maxEntries).ToList();
                
                foreach (var line in recentLines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    try
                    {
                        var logEntry = ParseLogEntry(line);
                        if (logEntry != null)
                        {
                            logEntries.Add(logEntry);
                        }
                    }
                    catch
                    {
                        // If JSON parsing fails, treat as plain text
                        logEntries.Add(new LogEntry
                        {
                            Timestamp = DateTime.Now,
                            Level = "Information",
                            Message = line
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logEntries.Add(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = "Error",
                    Message = $"Error reading logs: {ex.Message}"
                });
            }

            return logEntries.OrderByDescending(x => x.Timestamp).ToList();
        }

        /// <summary>
        /// Get all available log files
        /// </summary>
        public List<string> GetAvailableLogFiles()
        {
            var logFiles = new List<string>();

            try
            {
                string logPath = GetLogFilePath();
                string? logDirectory = Path.GetDirectoryName(logPath);
                string logFilePattern = Path.GetFileNameWithoutExtension(logPath) + "*" + Path.GetExtension(logPath);

                if (!string.IsNullOrEmpty(logDirectory) && Directory.Exists(logDirectory))
                {
                    logFiles.AddRange(Directory.GetFiles(logDirectory, logFilePattern)
                        .OrderByDescending(File.GetCreationTime));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting log files: {ex.Message}");
            }

            return logFiles;
        }

        /// <summary>
        /// Export logs to a file
        /// </summary>
        public async Task<bool> ExportLogsAsync(string exportPath, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                // Try to get more logs and also get raw log content if parsing fails
                var allLogs = await GetRecentLogsAsync(5000);
                
                // Filter by date if specified
                if (fromDate.HasValue)
                {
                    allLogs = allLogs.Where(x => x.Timestamp >= fromDate.Value).ToList();
                }
                if (toDate.HasValue)
                {
                    allLogs = allLogs.Where(x => x.Timestamp <= toDate.Value).ToList();
                }

                using var writer = new StreamWriter(exportPath);
                await writer.WriteLineAsync($"Bulk Editor Log Export - {DateTime.Now}");
                await writer.WriteLineAsync("=" + new string('=', 50));
                await writer.WriteLineAsync($"Total log entries found: {allLogs.Count}");
                await writer.WriteLineAsync();

                if (allLogs.Count == 0)
                {
                    // If no parsed logs found, try to get raw log file content
                    await writer.WriteLineAsync("No parsed log entries found. Attempting to export raw log file content:");
                    await writer.WriteLineAsync();
                    
                    string logPath = GetLogFilePath();
                    if (File.Exists(logPath))
                    {
                        var rawContent = await File.ReadAllTextAsync(logPath);
                        await writer.WriteLineAsync("=== Raw Log File Content ===");
                        await writer.WriteLineAsync(rawContent);
                    }
                    else
                    {
                        await writer.WriteLineAsync($"Log file not found at: {logPath}");
                    }
                }
                else
                {
                    foreach (var log in allLogs.OrderBy(x => x.Timestamp))
                    {
                        await writer.WriteLineAsync($"[{log.Timestamp:yyyy-MM-dd HH:mm:ss}] [{log.Level}] {log.Message}");
                        if (!string.IsNullOrEmpty(log.Exception))
                        {
                            await writer.WriteLineAsync($"    Exception: {log.Exception}");
                        }
                        await writer.WriteLineAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting logs: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clear old log files
        /// </summary>
        public Task<int> ClearOldLogsAsync(int daysToKeep = 7)
        {
            int deletedCount = 0;

            try
            {
                var logFiles = GetAvailableLogFiles();
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);

                foreach (var logFile in logFiles)
                {
                    if (File.GetCreationTime(logFile) < cutoffDate)
                    {
                        try
                        {
                            File.Delete(logFile);
                            deletedCount++;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error deleting log file {logFile}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing old logs: {ex.Message}");
            }

            return Task.FromResult(deletedCount);
        }

        private string GetLogFilePath()
        {
            if (Path.IsPathRooted(_settings.LogFilePath))
            {
                return _settings.LogFilePath;
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings.LogFilePath);
        }

        private LogEntry? ParseLogEntry(string logLine)
        {
            if (string.IsNullOrWhiteSpace(logLine)) return null;

            try
            {
                // Try JSON format first
                if (logLine.TrimStart().StartsWith("{"))
                {
                    return ParseJsonLogEntry(logLine);
                }
                
                // Try standard Serilog format: 2024-01-01 12:00:00.000 +00:00 [INF] Message
                return ParseStandardLogEntry(logLine);
            }
            catch (Exception ex)
            {
                // If all parsing fails, create a basic entry
                System.Diagnostics.Debug.WriteLine($"Failed to parse log line: {ex.Message}");
                return new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = "Information",
                    Message = logLine
                };
            }
        }

        private LogEntry? ParseJsonLogEntry(string logLine)
        {
            try
            {
                using var jsonDoc = JsonDocument.Parse(logLine);
                var root = jsonDoc.RootElement;

                return new LogEntry
                {
                    Timestamp = root.TryGetProperty("@t", out var timestamp) ?
                        (DateTime.TryParse(timestamp.GetString(), out var dt) ? dt : DateTime.Now) : DateTime.Now,
                    Level = root.TryGetProperty("@l", out var level) ?
                        (level.GetString() ?? "Information") : "Information",
                    Message = root.TryGetProperty("@m", out var message) ?
                        (message.GetString() ?? logLine) : logLine,
                    Exception = root.TryGetProperty("@x", out var exception) ? exception.GetString() : null
                };
            }
            catch
            {
                return null;
            }
        }

        private LogEntry? ParseStandardLogEntry(string logLine)
        {
            try
            {
                // Parse standard Serilog format: 2024-01-01 12:00:00.000 +00:00 [INF] Message
                // Also handle format: [2024-01-01 12:00:00.000 +00:00] [INF] Message
                
                var line = logLine.Trim();
                
                // Handle bracketed timestamp format
                if (line.StartsWith('['))
                {
                    return ParseBracketedLogEntry(line);
                }
                
                // Handle standard timestamp format (most common)
                return ParseTimestampLogEntry(line);
            }
            catch
            {
                return null;
            }
        }

        private LogEntry? ParseBracketedLogEntry(string line)
        {
            var timestampEnd = line.IndexOf(']');
            if (timestampEnd <= 0) return null;

            var levelStart = line.IndexOf('[', timestampEnd);
            var levelEnd = line.IndexOf(']', levelStart);

            if (levelStart <= 0 || levelEnd <= 0) return null;

            var timestampStr = line.Substring(1, timestampEnd - 1);
            var levelStr = line.Substring(levelStart + 1, levelEnd - levelStart - 1);
            var messageStr = line.Length > levelEnd + 2 ? line.Substring(levelEnd + 2) : "";

            return new LogEntry
            {
                Timestamp = DateTime.TryParse(timestampStr, out var ts) ? ts : DateTime.Now,
                Level = levelStr.Trim(),
                Message = messageStr.Trim()
            };
        }

        private LogEntry? ParseTimestampLogEntry(string line)
        {
            // Look for pattern: timestamp [LEVEL] message
            var levelStart = line.IndexOf('[');
            var levelEnd = line.IndexOf(']', levelStart);

            if (levelStart <= 0 || levelEnd <= 0) return null;

            var timestampStr = line.Substring(0, levelStart).Trim();
            var levelStr = line.Substring(levelStart + 1, levelEnd - levelStart - 1);
            var messageStr = line.Length > levelEnd + 2 ? line.Substring(levelEnd + 2) : "";

            return new LogEntry
            {
                Timestamp = DateTime.TryParse(timestampStr, out var ts) ? ts : DateTime.Now,
                Level = levelStr.Trim(),
                Message = messageStr.Trim()
            };
        }
    }

    /// <summary>
    /// Represents a single log entry
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}] {Message}";
        }
    }
}