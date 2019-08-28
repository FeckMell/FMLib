using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{

  /// <summary>
  /// Class containing miscellaneous functions
  /// </summary>
  public static class Useful
  {

    #region Enumerable Extensions

    /// <summary>
    /// Makes <see cref="string.Join(string, IEnumerable{string})"/> an extension method
    /// </summary>
    public static string StrJoin<T>(this IEnumerable<T> source, string separator)
    {
      if (source == null) { return string.Empty; }
      return string.Join(separator, source);
    }

    /// <summary>
    /// Adds ForEach extension method. May throw if action parameter throws.
    /// </summary>
    public static IEnumerable ForEach_(this IEnumerable source, Action<object> action)
    {
      return ForEach_A(source, action);
    }

    /// <summary>
    /// Invokes provided function on each element and returns list of results. May throw if action parameter throws.
    /// </summary>
    public static IEnumerable ForEach_<T>(this IEnumerable source, Func<object, T> func)
    {
      return ForEach_F(source, func);
    }

    /// <summary>
    /// Invokes provided function on each element and returns list of results. May throw if action parameter throws.
    /// </summary>
    private static IEnumerable ForEach_F<T>(IEnumerable source, Func<object, T> func)
    {
      List<T> result = new List<T>();
      if (source == null) { return result; }
      if (func == null) { return result; }
      foreach (var e in source) { result.Add(func(e)); }
      return result;
    }

    /// <summary>
    /// Adds ForEach extension method. May throw if action parameter throws.
    /// </summary>
    public static IEnumerable ForEach_A(this IEnumerable source, Action<object> action)
    {
      if (source == null) { return source; }
      if (action == null) { return source; }
      foreach (var e in source) { action(e); }
      return source;
    }

    /// <summary>
    /// Adds ForEach extension method. May throw if action parameter throws.
    /// </summary>
    /// <returns> Returns same collection which is passed to allow method chaining </returns>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
      return ForEachA(source, action);
    }

    /// <summary>
    /// Invokes provided function on each element and returns list of results. May throw if action parameter throws.
    /// </summary>
    public static List<K> ForEach<T, K>(this IEnumerable<T> source, Func<T, K> func)
    {
      return ForEachF(source, func);
    }

    /// <summary>
    /// Adds ForEach extension method. May throw if action parameter throws.
    /// </summary>
    /// <returns> Returns same collection which is passed to allow method chaining </returns>
    public static IEnumerable<T> ForEachA<T>(this IEnumerable<T> source, Action<T> action)
    {
      if (source == null) { return source; }
      if (action == null) { return source; }
      foreach (T e in source) { action(e); }
      return source;
    }

    /// <summary>
    /// Invokes provided function on each element and returns list of results. May throw if action parameter throws.
    /// </summary>
    public static List<K> ForEachF<T, K>(this IEnumerable<T> source, Func<T, K> func)
    {
      List<K> result = new List<K>();
      if (source == null) { return result; }
      if (func == null) { return result; }
      foreach (T e in source) { result.Add(func(e)); }
      return result;
    }

    #endregion

    #region String Extensions

    /// <summary>
    /// Make <see cref="string.IsNullOrWhiteSpace(string)"/> Extension method
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string target)
    {
      return string.IsNullOrWhiteSpace(target);
    }

    /// <summary>
    /// Make <see cref="string.IsNullOrEmpty(string)"/> Extension method
    /// </summary>
    public static bool IsNullOrEmpty(this string target)
    {
      return string.IsNullOrEmpty(target);
    }

    /// <summary>
    /// Make <see cref="string.Equals(string, string, StringComparison)"/> Extension method
    /// </summary>
    public static bool EQUAL(this string first, string second, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
      return string.Equals(first, second, comparison);
    }

    /// <summary>
    /// Replaces illegal chars in file name with '$'
    /// </summary>
    public static string RemoveInvalidFilenameChars(this string target)
    {
      StringBuilder result = new StringBuilder(target);
      Path.GetInvalidFileNameChars().ForEachA(x => result.Replace(x, '$'));
      return result.ToString();
    }

    /// <summary>
    /// Copies string n times
    /// </summary>
    public static string Intend(string intend, int count)
    {
      if (count < 1) { return string.Empty; }
      return string.Concat(Enumerable.Repeat(intend, count));
    }

    /// <summary>
    /// Substring from specified index to specified index. Last element is included
    /// </summary>
    public static string Substring(this string data, int beginIndex, int endIndex, bool dummy)
    {
      try
      {
        if (data == null) { return ""; }
        int dataCount = data.Length;
        if (beginIndex < 0 || endIndex >= data.Length || beginIndex > endIndex) { return ""; }
        return data.Substring(beginIndex, endIndex - beginIndex + 1);
      }
      catch (Exception ex) { Tracer._SystemError(ex.Message); return ""; }
    }

    /// <summary>
    /// Gets strings between two values
    /// </summary>
    public static List<string> StringsBetween(this string data, string start, string end)
    {
      List<string> result = new List<string>();
      try
      {
        if (data.IsNullOrWhiteSpace() || start.IsNullOrWhiteSpace() || end.IsNullOrWhiteSpace()) { return result; }
        int startIndex = 0;
        int endIndex = 0;
        while (!data.IsNullOrWhiteSpace())
        {
          if ((startIndex = data.IndexOf(start, StringComparison.OrdinalIgnoreCase)) == -1) { return result; }
          data = data.Substring(startIndex); // cut beginning
          if ((endIndex = data.IndexOf(end, StringComparison.OrdinalIgnoreCase)) == -1) { return result; }
          result.Add(data.Substring(0, endIndex + end.Length)); // add found part
          data = data.Substring(endIndex + end.Length); // cut found path
        }
        return result;
      }
      catch (Exception ex) { Tracer._SystemError(ex.ToString()); return result; }
    }

    #endregion

    #region Parsing

    /// <summary>
    /// Removes comments from line
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
    /// Parses time string (-2.5ms). May throw.
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
    /// Parses time range in format: "-xx.xxS:yy.yyH". May throw.
    /// </summary>
    public static (Time start, Time duration) ParseTimeRange(string value)
    {
      if (value.IsNullOrWhiteSpace()) { throw new ExpectedException($"string is empty"); }

      List<string> split = value.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
      if (split.Count == 1) { split.Insert(0, "0s"); } // if only duration component is specified
      if (split.Count == 2) { return (new Time(split[0]), new Time(split[1])); }
      throw new ExpectedException($"Duration parsing failed. Wrong amount of arguments({split.Count}) in line({value})");
    }

    /// <summary>
    /// Parses string in format: Event type: RECORD_SET_MATERIAL; Primary token: LOADER_COM; Event time: 10-Apr-19 16:25:56 +03:00; client: 8; Event Id: 808259564590012;
    /// </summary>
    public static List<Tuple<string, string>> SplitTwice(string target, char sep1, char sep2)
    {
      List<Tuple<string, string>> result = new List<Tuple<string, string>>();
      var split1 = target.Split(new[] { sep1 }, StringSplitOptions.RemoveEmptyEntries);
      foreach (var e in split1)
      {
        var split2 = e.Split(new[] { sep2 }, 2);
        if (split2.Length != 2) { continue; }
        result.Add(new Tuple<string, string>(split2[0].Trim(), split2[1].Trim()));
      }
      return result;
    }

    #endregion

    /// <summary>
    /// Converts <see cref="IEnumerable{T}"/> where <see cref="byte"/> (0xff, 0xff, 0xff) to HEX string (FF FF FF)
    /// </summary>
    public static string BytesToHex(IEnumerable<byte> bytes)
    {
      if (bytes == null) { return string.Empty; }
      return BitConverter.ToString(bytes.ToArray()).Replace("-", " ");
    }

    /// <summary>
    /// Converts string to byte array
    /// </summary>
    public static List<byte> HexToBytes(string hex)
    {
      List<byte> result = new List<byte>();
      if (hex.IsNullOrWhiteSpace()) { return result; }
      hex = hex.Replace(" ", "").Replace("-", "");
      for (int i = 0; i + 1 < hex.Length; i = i + 2)
      {
        string s = $"{hex[i]}{hex[i + 1]}";
        byte b = new byte();
        try { b = byte.Parse(s, NumberStyles.HexNumber); }
        catch (Exception ex) { Tracer._SystemError($"Invalid byte value: {s}. Exception:{ex}"); }
        result.Add(b);
      }
      return result;
    }

    /// <summary>
    /// Wraps <see cref="Process.Start()"/> with <see cref="Process.Exited"/> with disposing, running in separate thread and try-catch
    /// </summary>
    public static void StartDisposingProcess(Process process, Action preDisposeAction = null, [CallerFilePath] string file = null, [CallerMemberName] string method = null, [CallerLineNumber] int line = 0)
    {
      if (process == null) { Tracer._SystemError($"Null argument {nameof(process)}"); return; }

      string caller = $"called from {new Caller(file, method, line).ToString()}";
      try
      {
        process.EnableRaisingEvents = true;
        process.Exited += (s, e) => { preDisposeAction?.Invoke(); process?.Dispose(); process = null; };
        process.ErrorDataReceived += (s, e) => { Tracer.Get("External").Error($"External process {process?.StartInfo?.FileName} {caller} sent error: {e?.Data}"); };
        process.OutputDataReceived += (s, e) => { Tracer.Get("External").Info($"External process {process?.StartInfo?.FileName} {caller} sent data: {e?.Data}"); };
        process.Disposed += (s, e) => { Tracer.Get("External").Info($"External process {process?.StartInfo?.FileName} {caller} disposed"); };

        Task task = new Task(() => process.Start());
        StartDisposingTask(task, null, file, method, line);
      }
      catch (Exception ex) { Tracer._SystemError($"Exception starting/disposing process {caller}: {ex.Message}"); }
    }

    /// <summary>
    /// Wraps <see cref="Task.Start()"/> with <see cref="Task.ContinueWith(Action{Task})"/> with disposing and try-catch
    /// </summary>
    public static void StartDisposingTask(Task task, Action preDisposeAction = null, [CallerFilePath] string file = null, [CallerMemberName] string method = null, [CallerLineNumber] int line = 0)
    {
      if (task == null) { Tracer._SystemError($"Null argument {nameof(task)}"); return; }

      string caller = $"called from {new Caller(file, method, line).ToString()}";
      try
      {
        task.ContinueWith((t) =>
        {
          preDisposeAction?.Invoke();
          task?.Dispose();
          task = null;
        });
        task.Start();
      }
      catch (Exception ex) { Tracer._SystemError($"Exception starting/disposing task {caller}: {ex.Message}"); }
    }

    /// <summary>
    /// Gets Enum values as list
    /// </summary>
    public static List<T> GetEnumValues<T>() where T : struct, IComparable, IFormattable, IConvertible // T - enum
    {
      if (!typeof(T).IsEnum) { throw new ExpectedException("T must be an enumerated type"); }
      return ((T[])Enum.GetValues(typeof(T))).ToList();
    }
  }
}