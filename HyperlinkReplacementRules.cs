using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bulk_Editor
{
    public class HyperlinkReplacementRules
    {
        public List<HyperlinkReplacementRule> Rules { get; set; } = new List<HyperlinkReplacementRule>();

        // File path for saving/loading rules
        private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hyperlink_replacement_rules.json");

        // Serialize rules to JSON and save to file
        public async Task SaveAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                // Log error or handle as appropriate for the application
                System.Diagnostics.Debug.WriteLine($"Error saving hyperlink replacement rules: {ex.Message}");
            }
        }

        // Load rules from JSON file
        public static async Task<HyperlinkReplacementRules> LoadAsync()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var json = await File.ReadAllTextAsync(SettingsFilePath);
                    var rules = JsonSerializer.Deserialize<HyperlinkReplacementRules>(json);
                    return rules ?? new HyperlinkReplacementRules();
                }
            }
            catch (Exception ex)
            {
                // Log error or handle as appropriate for the application
                System.Diagnostics.Debug.WriteLine($"Error loading hyperlink replacement rules: {ex.Message}");
            }

            // Return empty rules if file doesn't exist or there was an error
            return new HyperlinkReplacementRules();
        }
    }
}