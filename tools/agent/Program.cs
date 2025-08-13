using System.Diagnostics;
using System.Text;
using System.Text.Json;

#nullable enable

sealed record AgentConfig(Dictionary<string, string> Commands, string[] AllowWriteGlobs, string[] DenyWriteGlobs, int MaxPatchLines)
{
  public static AgentConfig Load(string path)
  {
    var json = File.ReadAllText(path);
    using var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;
    var commands = root.GetProperty("commands").EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetString()!);
    var allow = root.GetProperty("allowWriteGlobs").EnumerateArray().Select(e => e.GetString()!).ToArray();
    var deny = root.GetProperty("denyWriteGlobs").EnumerateArray().Select(e => e.GetString()!).ToArray();
    var max = root.GetProperty("maxPatchLines").GetInt32();
    return new AgentConfig(commands, allow, deny, max);
  }
}

internal partial class Program
{
  private static async Task Main(string[] args)
  {
    var cfg = AgentConfig.Load(".agent/config.json");
    var cmd = args.FirstOrDefault() ?? "help";

    switch (cmd)
    {
      case "run":
        var what = args.Skip(1).FirstOrDefault() ?? "build";
        Console.WriteLine($"> agent run {what}");
        await RunPipelineAsync(what, cfg);
        break;

      case "apply":
        var path = args.Skip(1).FirstOrDefault() ?? "-";
        var patchText = path == "-" ? await ReadStdinAsync() : await File.ReadAllTextAsync(path);
        var result = UnifiedDiff.Apply(patchText, Directory.GetCurrentDirectory());
        Console.WriteLine(result ? "Patch applied." : "Patch failed.");
        Environment.ExitCode = result ? 0 : 1;
        break;

      default:
        Console.WriteLine("Usage:");
        Console.WriteLine("  agent run build|test|lint|format");
        Console.WriteLine("  agent apply <path|->   # unified diff from file or STDIN");
        break;
    }
  }

  private static async Task RunPipelineAsync(string stage, AgentConfig cfg)
  {
    var allowed = new[] { "build", "test", "lint", "format" };
    if (!allowed.Contains(stage)) { Console.WriteLine($"Unknown stage '{stage}'"); return; }

    var cmd = cfg.Commands[stage];
    var exit = await Exec(cmd);
    if (exit != 0) Environment.Exit(exit);
  }

  private static async Task<int> Exec(string cmd, string? cwd = null)
  {
    var psi = new ProcessStartInfo()
    {
      FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/bash",
      Arguments = OperatingSystem.IsWindows() ? $"/c {cmd}" : $"-lc \"{cmd}\"",
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false,
      WorkingDirectory = cwd ?? Directory.GetCurrentDirectory()
    };
    var p = Process.Start(psi)!;
    p.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
    p.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };
    p.BeginOutputReadLine(); p.BeginErrorReadLine();
    await p.WaitForExitAsync();
    return p.ExitCode;
  }

  private static async Task<string> ReadStdinAsync()
  {
    using var ms = new MemoryStream();
    await Console.OpenStandardInput().CopyToAsync(ms);
    return Encoding.UTF8.GetString(ms.ToArray());
  }

  public static class UnifiedDiff
  {
    public static bool Apply(string patchText, string basePath)
    {
      // Simple placeholder implementation
      // In a real implementation, this would parse and apply unified diff format
      Console.WriteLine($"Applying patch to {basePath}");
      Console.WriteLine(patchText);
      return true;
    }
  }