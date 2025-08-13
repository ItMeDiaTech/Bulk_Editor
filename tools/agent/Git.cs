using System.Diagnostics;

static class Git
{
  public static async Task<int> CommitAsync(string message)
    => await ExecGit($"commit -m \"{message.Replace("\"","\\\"")}\"");

  public static async Task<int> BranchAsync(string name)
    => await ExecGit($"checkout -B {name}");

  public static async Task<int> PushAsync(string remote="origin")
    => await ExecGit($"push -u {remote} HEAD");

  static async Task<int> ExecGit(string args)
  {
    var psi = new ProcessStartInfo
    {
      FileName = "git",
      Arguments = args,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false
    };
    var p = Process.Start(psi)!;
    p.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
    p.ErrorDataReceived  += (_, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };
    p.BeginOutputReadLine(); p.BeginErrorReadLine();
    await p.WaitForExitAsync();
    return p.ExitCode;
  }
}