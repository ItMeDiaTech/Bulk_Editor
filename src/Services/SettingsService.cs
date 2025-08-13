#nullable enable
using Bulk_Editor.Configuration;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Bulk_Editor.Services.Abstractions;

namespace Bulk_Editor.Services
{
    public class SettingsService : ISettingsService
    {
        private const string ConfigFileName = "appsettings.json";
        private readonly string _configPath;

        public AppSettings Settings { get; private set; } = default!;

        // Event to notify when settings are reloaded
        public event EventHandler SettingsReloaded;

        public SettingsService()
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
        }

        public async Task LoadSettingsAsync()
        {
            if (File.Exists(_configPath))
            {
                var json = await File.ReadAllTextAsync(_configPath);
                Settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new AppSettings();
            }
            else
            {
                Settings = new AppSettings();
            }
            
            // Notify that settings have been reloaded
            SettingsReloaded?.Invoke(this, EventArgs.Empty);
        }

        public async Task SaveSettingsAsync()
        {
            var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_configPath, json);
        }
    }
}