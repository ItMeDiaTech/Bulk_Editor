using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Bulk_Editor.Configuration;
using Bulk_Editor.Services;
using Bulk_Editor.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Bulk_Editor
{
    internal static class Program
    {
        // Optional: access configuration anywhere via Program.Configuration
        public static IConfiguration Configuration { get; private set; } = default!;

        [STAThread]
        static void Main()
        {
            // 1) Build configuration with embedded fallback support
            var cfgBuilder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory);

            // Add embedded default configuration as fallback
            AddEmbeddedConfiguration(cfgBuilder);

            // Add external configuration files (higher priority)
            cfgBuilder
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = cfgBuilder.Build();

            // 2) Configure Serilog from config (works even if the "Serilog" section is absent)
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            // 3) WinForms bootstrap
#if NET6_0_OR_GREATER
            ApplicationConfiguration.Initialize();
#else
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#endif

            // 4) Global exception logging
            Application.ThreadException += (s, e) =>
                Log.Error(e.Exception, "Unhandled UI thread exception");
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                Log.Fatal(e.ExceptionObject as Exception, "Unhandled non-UI exception");

            try
            {
                Log.Information("Bulk Editor starting up...");

                // 5) Configure services for dependency injection
                var appSettings = Configuration.Get<AppSettings>() ?? new AppSettings();

                // Instantiate services
                var loggingService = new LoggingService(appSettings.Logging);
                var settingsService = new SettingsService();
                
                // Load initial settings first
                settingsService.LoadSettingsAsync().Wait();
                
                var themeService = new ThemeService(settingsService);
                var processingService = new ProcessingService(loggingService);
                var logViewerService = new LogViewerService(loggingService, appSettings.Logging);
                var changelogManager = new ChangelogManager(appSettings.ChangelogSettings);
                var validationService = new ValidationService();
                var windowStateService = new WindowStateService(appSettings.ApplicationSettings, appSettings.UI);

                // 6) Run the application, injecting services into the main form
                Application.Run(new MainForm(
                    loggingService,
                    settingsService,
                    themeService,
                    processingService,
                    logViewerService,
                    changelogManager,
                    validationService,
                    windowStateService
                ));
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.Information("Bulk Editor shutting down.");
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Add embedded configuration as fallback when external files don't exist
        /// </summary>
        private static void AddEmbeddedConfiguration(IConfigurationBuilder builder)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "Bulk_Editor.appsettings.default.json";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    builder.AddJsonStream(stream);
                    Log.Information("Loaded embedded default configuration");
                }
                else
                {
                    Log.Warning("Embedded default configuration not found: {ResourceName}", resourceName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to load embedded configuration");
            }
        }
    }
}
