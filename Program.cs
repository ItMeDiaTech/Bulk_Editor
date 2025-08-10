using System;
using System.Windows.Forms;
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
            // 1) Build configuration: base + (Release-only) Publish + environment variables
            var cfgBuilder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
#if !DEBUG
                .AddJsonFile("Publish/appsettings.json", optional: true, reloadOnChange: true)
#endif
                .AddEnvironmentVariables();

            Configuration = cfgBuilder.Build();

            // 2) Configure Serilog from config (works even if the "Serilog" section is absent)
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
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
                Log.Error(e.Exception, "UI thread exception");
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                Log.Fatal(e.ExceptionObject as Exception, "Unhandled exception");

            try
            {
                Log.Information("Bulk Editor starting");
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
