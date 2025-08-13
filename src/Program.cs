#nullable enable
using System.Diagnostics;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        // Load config
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), ".agent", "config.json");
        var cfg = AgentConfig.Load(configPath);

        // Args: run <stage> | apply <path|->
        if (args.Length == 0) { PrintUsage(); return; }

        switch (args[0].ToLowerInvariant())
        {
            case "run":
                var stage = args.Length > 1 ? args[1] : "build";
                await RunPipelineAsync(stage, cfg);
                break;

            case "apply":
                var path = args.Length > 1 ? args[1] : "-";
                var patchText = path == "-" ? await ReadStdinAsync()
                                            : await File.ReadAllTextAsync(path);
                var ok = UnifiedDiff.Apply(patchText, Directory.GetCurrentDirectory());
                Console.WriteLine(ok ? "Patch applied." : "Patch failed.");
                Environment.ExitCode = ok ? 0 : 1;
                break;

            default:
                PrintUsage();
                break;
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  agent run <build|test|lint|format>");
        Console.WriteLine("  agent apply <path|->");
    }

    static async Task RunPipelineAsync(string stage, AgentConfig cfg)
    {
        if (!cfg.Commands.TryGetValue(stage, out var cmd))
        {
            Console.Error.WriteLine($"Unknown stage '{stage}'.");
            Environment.ExitCode = 2;
            return;
        }
        var code = await Exec(cmd);
        Environment.ExitCode = code;
    }

    static async Task<int> Exec(string cmd, string? cwd = null)
    {
        var isWin = OperatingSystem.IsWindows();
        var file = isWin ? "cmd.exe" : "/bin/bash";
        var args = isWin ? $"/c {cmd}" : $"-lc \"{cmd}\"";

        var psi = new ProcessStartInfo(file, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = cwd ?? Directory.GetCurrentDirectory()
        };

        using var p = Process.Start(psi)!;
        p.OutputDataReceived += (_, e) => { if (e.Data is not null) Console.WriteLine(e.Data); };
        p.ErrorDataReceived += (_, e) => { if (e.Data is not null) Console.Error.WriteLine(e.Data); };
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        await p.WaitForExitAsync();
        return p.ExitCode;
    }

    static async Task<string> ReadStdinAsync()
    {
        using var ms = new MemoryStream();
        await Console.OpenStandardInput().CopyToAsync(ms);
        return Encoding.UTF8.GetString(ms.ToArray());
    }
}

public class AgentConfig
{
    public Dictionary<string, string> Commands { get; set; } = new();

    public static AgentConfig Load(string path)
    {
        // Placeholder implementation
        return new AgentConfig
        {
            Commands = new Dictionary<string, string>
            {
                ["build"] = "dotnet build",
                ["test"] = "dotnet test",
                ["lint"] = "dotnet format --verify-no-changes",
                ["format"] = "dotnet format"
            }
        };
    }
}

public static class UnifiedDiff
{
    public static bool Apply(string patchText, string basePath)
    {
        // Placeholder implementation
        Console.WriteLine($"Applying patch to {basePath}");
        return true;
    }
}

