using System.Text;
using System.Text.RegularExpressions;

/// Minimal unified-diff applier. Assumes files are UTF-8 text.
/// Keeps you in 'patch-only' editing.
static class UnifiedDiff
{
  public static bool Apply(string diff, string repoRoot)
  {
    try
    {
      var chunks = SplitFiles(diff);
      foreach (var f in chunks)
      {
        var target = Path.GetFullPath(Path.Combine(repoRoot, f.Path));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        var original = File.Exists(target) ? File.ReadAllText(target, Encoding.UTF8).Split('\n').ToList() : new List<string>();
        var patched  = ApplyFilePatch(original, f.Hunks);
        File.WriteAllText(target, string.Join("\n", patched), new UTF8Encoding(false));
      }
      return true;
    }
    catch (Exception ex)
    {
      Console.Error.WriteLine($"Patch error: {ex.Message}");
      return false;
    }
  }

  static List<string> ApplyFilePatch(List<string> lines, List<Hunk> hunks)
  {
    int offset = 0;
    foreach (var h in hunks)
    {
      var start = h.OldStart - 1 + offset;
      var end   = start + h.OldCount;
      // Verify context
      int i = start;
      foreach (var l in h.Lines)
      {
        if (l.Kind == LineKind.Context)
        {
          if (i >= lines.Count || lines[i] != l.Text) throw new Exception("Context mismatch");
          i++;
        }
        else if (l.Kind == LineKind.Remove)
        {
          if (i >= lines.Count || lines[i] != l.Text) throw new Exception("Delete mismatch");
          i++;
        }
      }
      // Apply: rebuild segment
      var rebuild = new List<string>();
      foreach (var l in h.Lines)
        if (l.Kind != LineKind.Remove) rebuild.Add(l.Text);
      lines.RemoveRange(start, h.OldCount);
      lines.InsertRange(start, rebuild);
      offset += rebuild.Count - h.OldCount;
    }
    return lines;
  }

  static List<FilePatch> SplitFiles(string diff)
  {
    var files = new List<FilePatch>();
    var lines = diff.Replace("\r\n","\n").Split('\n');
    int idx = 0;
    while (idx < lines.Length)
    {
      while (idx < lines.Length && !lines[idx].StartsWith("--- ")) idx++;
      if (idx >= lines.Length) break;
      // --- a/path
      // +++ b/path
      var oldLine = lines[idx++]; var newLine = lines[idx++];
      var path = newLine.StartsWith("+++ b/") ? newLine[6..] : newLine.Replace("+++ ","");
      var fp = new FilePatch { Path = path };
      while (idx < lines.Length && lines[idx].StartsWith("@@ "))
      {
        var m = Regex.Match(lines[idx], @"@@ -(?<oStart>\d+),(?<oCount>\d+) \+(?<nStart>\d+),(?<nCount>\d+) @@");
        if (!m.Success) break;
        var h = new Hunk
        {
          OldStart = int.Parse(m.Groups["oStart"].Value),
          OldCount = int.Parse(m.Groups["oCount"].Value),
          NewStart = int.Parse(m.Groups["nStart"].Value),
          NewCount = int.Parse(m.Groups["nCount"].Value)
        };
        idx++;
        var collected = new List<HunkLine>();
        while (idx < lines.Length && !lines[idx].StartsWith("@@ ") && !lines[idx].StartsWith("--- "))
        {
          if (lines[idx].Length == 0) { collected.Add(new HunkLine(LineKind.Context, "")); idx++; continue; }
          var c = lines[idx][0];
          var text = c == '+' || c == '-' || c == ' ' ? lines[idx][1..] : lines[idx];
          if (c == '+') collected.Add(new HunkLine(LineKind.Add, text));
          else if (c == '-') collected.Add(new HunkLine(LineKind.Remove, text));
          else collected.Add(new HunkLine(LineKind.Context, text));
          idx++;
        }
        h.Lines = collected;
        fp.Hunks.Add(h);
      }
      files.Add(fp);
    }
    return files;
  }

  sealed class FilePatch { public string Path = ""; public List<Hunk> Hunks = new(); }
  sealed class Hunk
  {
    public int OldStart, OldCount, NewStart, NewCount;
    public List<HunkLine> Lines = new();
  }
  sealed class HunkLine(LineKind kind, string text) { public LineKind Kind = kind; public string Text = text; }
  enum LineKind { Context, Add, Remove }
}