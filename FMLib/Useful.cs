using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FMLib.Collections;
using FMLib.ExtensionMethods;
using IWshRuntimeLibrary;

namespace Utils
{

  /// <summary>
  /// Class containing miscellaneous functions
  /// </summary>
  public static class Useful
  {

    #region Parsing

    /// <summary>
    /// <para/>Removes comments from line
    /// <para/>Do not throw.
    /// </summary>
    public static string DeleteComments(string line)
    {
      const string lineCommentSign = "//";

      // Check if line is empty
      if (line == null) { return null; }

      // Trim line
      line = line.Trim();

      // Check if line is a comment
      if (line.StartsWith(lineCommentSign)) { return string.Empty; }

      // Check for comments in rest of the line
      line = line.Contains(lineCommentSign) ? line.Substring(0, line.IndexOf(lineCommentSign)).Trim() : line;

      // Final check just to make sure that it is not empty
      if (line.IsNullOrWhiteSpace()) { return string.Empty; }

      return line;
    }

    /// <summary>
    /// <para/>Parses time string (-2.5ms).
    /// <para/>May throw.
    /// </summary>
    public static TimeSpan ParseTimeSpan(string timeRaw)
    {
      try
      {
        if (timeRaw.IsNullOrWhiteSpace()) { throw new ExpectedException($"string is empty"); }

        const string START = "Start";
        const string END = "End";
        timeRaw = timeRaw.Trim();
        if (timeRaw.EQUAL(START)) { return TimeSpan.MinValue; }
        if (timeRaw.EQUAL(END)) { return TimeSpan.MaxValue; }
        var formats = new List<(string postfix, Func<double, TimeSpan> translator)?>
        {
          ("ms", TimeSpan.FromMilliseconds),
          ("h", TimeSpan.FromHours),
          ("m", TimeSpan.FromMinutes),
          ("s", TimeSpan.FromSeconds),
        };

        // Handle negative numbers
        bool isNegative = false;
        if (timeRaw.StartsWith("-"))
        {
          isNegative = true;
          timeRaw = timeRaw.Remove(0, 1).Trim();
        }

        var format = formats.FirstOrDefault(x => timeRaw.EndsWith(x?.postfix, StringComparison.OrdinalIgnoreCase));
        TimeSpan? span = format?.translator(double.Parse(timeRaw.Substring(0, timeRaw.Length - format?.postfix.Length ?? 0).Replace(',', '.')));
        if (span == null) { throw new ExpectedException($"invalid timeout provided, known: h, m, s, ms"); }
        TimeSpan result = span.Value;

        return isNegative ? -result : result;
      }
      catch (Exception e) { throw new ExpectedException($"TIME parsing: {timeRaw}: {e.Message}"); }
    }

    /// <summary>
    /// <para/>Parses string in format: Event type: RECORD_SET_MATERIAL; Primary token: LOADER_COM; Event time: 10-Apr-19 16:25:56 +03:00; client: 8; Event Id: 808259564590012;
    /// <para/>Do not throw.
    /// </summary>
    public static List<Tuple<string, string>> SplitTwice(string target, char sep1, char sep2)
    {
      List<Tuple<string, string>> result = new List<Tuple<string, string>>();
      if (target == null) { return result; }
      var split1 = target.Split(new[] { sep1 }, StringSplitOptions.RemoveEmptyEntries);
      foreach (var e in split1)
      {
        var split2 = e.Split(new[] { sep2 }, 2);
        if (split2.Length != 2) { continue; }
        result.Add(new Tuple<string, string>(split2[0].Trim(), split2[1].Trim()));
      }
      return result;
    }

    /// <summary>
    /// <para/>Splits parameters from line format: "parameter1, parameter2, parameter3" -> new List/<string/> { "parameter1", "parameter2", "parameter3" }
    /// <para/>Ignore commas if in braces ('{', '}', '[', ']', '(', ')')
    /// <para/>Ignore commas and braces if in quotes (', ")
    /// <para/>Do not throw
    /// </summary>
    public static List<string> SplitParameters(string line, DataTree<string> error)
    {
      line = line.Trim();
      var result = new List<string>();
      int curly = 0;
      int square = 0;
      int round = 0;
      bool double_quotes = false;
      bool single_quotes = false;
      var mapBraces = new Dictionary<char, Action>
      {
        {'{', () => curly++ },
        {'}', () => curly-- },
        {'[', () => square++ },
        {']', () => square-- },
        {'(', () => round++ },
        {')', () => round-- },
      };
      var mapQuotes = new Dictionary<char, Action>
      {
        {'\'', () => { if (!double_quotes) { single_quotes = !single_quotes; } } },
        {'\"', () => { if (!single_quotes) { double_quotes = !double_quotes; } } },
      };

      if (line.IsNullOrWhiteSpace()) { return result; }
      for (int i = 0; i < line.Length; i++)
      {
        char sign = line[i];
        if (mapQuotes.TryGetValue(sign, out Action action)) { action.Invoke(); continue; } // if found quote - move their counter and go to next char
        if (double_quotes || single_quotes) { continue; } // if we are in quotes - ignore everything except braces
        if (mapBraces.TryGetValue(sign, out action)) { action.Invoke(); continue; } // if found any of braces - move their counter and go to next char
        if (curly > 0 || square > 0 || round > 0) { continue; } // if any counter is above zero - then do not want to split on comma
        if (curly < 0 || square < 0 || round < 0) { error.Add($"Error in parameters: missing opening brace."); return new List<string>(); }

        if (sign.Equals(','))
        {
          result.Add(line.Substring(0, i).Trim());
          line = line.Substring(i + 1).Trim();
          i = -1; // make for-cycle go from zero
        }
      }

      if (double_quotes || single_quotes) { error.Add("Error in parameters: missing quote"); return new List<string>(); }
      if (line.Length > 0) { result.Add(line.Trim()); }
      if (result.Count > 0 && line.Length == 0) { result.Add(""); } // make "300," parse into ["300", ""]
      return result;
    }

    #endregion

    /// <summary>
    /// <para/>Converts string to byte array.
    /// <para/>Throws if can't parse any part of <paramref name="hex"/>
    /// </summary>
    /// <param name="hex">If null - returns empty <see cref="List{byte}"/></param>
    public static List<byte> HexToBytes(string hex)
    {
      List<byte> result = new List<byte>();
      if (hex.IsNullOrWhiteSpace()) { return result; }
      hex = hex.Replace(" ", "").Replace("-", "");
      for (int i = 0; i + 1 < hex.Length; i = i + 2)
      {
        string s = $"{hex[i]}{hex[i + 1]}";
        result.Add(byte.Parse(s, NumberStyles.HexNumber));
      }
      return result;
    }

    /// <summary>
    /// <para/>Gets Enum values as list
    /// <para/>Throws if not enum type
    /// </summary>
    public static List<T> GetEnumValues<T>() where T : struct, IComparable, IFormattable, IConvertible // T - enum
    {
      if (!typeof(T).IsEnum) { throw new ExpectedException("T must be an enumerated type"); }
      return ((T[])Enum.GetValues(typeof(T))).ToList();
    }

    /// <summary>
    /// <para/>Creates shortcut in Windows.
    /// <para/>May throw.
    /// </summary>
    public static void CreateShortcut(string fullShortcutName, string fullTargetName)
    {
      WshShell shell = new WshShell();
      IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(fullShortcutName);
      shortcut.TargetPath = fullTargetName;
      shortcut.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
      shortcut.Save();
    }
  }
}