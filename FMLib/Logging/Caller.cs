using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using FMLib.ExtensionMethods;

namespace Utils.Logging
{
  /// <summary>
  /// Class representing caller of method
  /// </summary>
  internal class Caller
  {

    /// <summary>
    /// File name
    /// </summary>
    public string File { get; private set; }

    /// <summary>
    /// Method name
    /// </summary>
    public string Method { get; private set; }

    /// <summary>
    /// Line number
    /// </summary>
    public int Line { get; private set; }

    /// <summary>
    /// Thread
    /// </summary>
    public int Thread { get; private set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public Caller([CallerFilePath] string file = null, [CallerMemberName] string method = null, [CallerLineNumber] int line = 0, int? thread = 0)
    {
      File = file;
      Method = method;
      Line = line;
      Thread = thread ?? System.Threading.Thread.CurrentThread.ManagedThreadId;
    }

    /// <summary>
    /// ToString
    /// </summary>
    public override string ToString()
    {
      return ToString("THR", "F", "M", "L");
    }

    /// <summary>
    /// ToString. Available formats: TRD - Thread, F - File, M - method, L - Line
    /// </summary>
    public string ToString(params string[] format)
    {
      string shortFile = File;
      try { shortFile = Path.GetFileName(File); }
      catch { }
      if (format == null || format.Length == 0) { return $"[TRD:{Thread}, F:{shortFile}, M:{Method}, L:{Line}]"; }
      else
      {
        List<string> format2 = new List<string>(format);
        List<string> result = new List<string>();
        if (format2.FindIndex(x => x.EQUAL("TRD")) != -1) { result.Add($"TRD:{Thread}"); }
        if (format2.FindIndex(x => x.EQUAL("F")) != -1) { result.Add($"F:{shortFile}"); }
        if (format2.FindIndex(x => x.EQUAL("M")) != -1) { result.Add($"M:{Method}"); }
        if (format2.FindIndex(x => x.EQUAL("L")) != -1) { result.Add($"L:{Line}"); }
        return $"[{result.StrJoin(", ")}]";
      }
    }
  }
}