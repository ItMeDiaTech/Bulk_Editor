using System;

namespace Bulk_Editor.Utils
{
    /// <summary>
    /// Provides static utility methods for formatting data.
    /// </summary>
    public static class FormatUtils
    {
        /// <summary>
        /// Formats a file size in bytes into a human-readable string (e.g., KB, MB).
        /// </summary>
        /// <param name="bytes">The file size in bytes.</param>
        /// <returns>A formatted string representing the file size.</returns>
        public static string FormatFileSize(long bytes)
        {
            if (bytes < 0)
            {
                return "0 B";
            }

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
    }
}