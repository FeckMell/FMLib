using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Utils.Collections;
using Utils.ExtensionMethods;

namespace Utils.Logging
{

  /// <summary>
  /// Level of messages
  /// </summary>
  public enum LVL
  {
    /// <summary> Most low-level messages. For Ports messages or very often messages. </summary>
    Trace = 0,
    /// <summary> Valuable information about actions performed or initialisation of ports and hardwares success </summary>
    Info = 1,
    /// <summary> Some expected problems that can be ignored </summary>
    Warning = 2,
    /// <summary> Some expected problems that can't be ignored due to incorrect data (port didn't initialise) </summary>
    Error = 3,
    /// <summary> Any problems connected with logic or behavior, but not incorrect data </summary>
    SystemError = 4,
  }

  /// <summary>
  /// Tracer class for simulator logging
  /// </summary>
  public class Tracer
  {

    #region Constants

    // Names of constant tracers
    public const string COMMON = "Common";
    public const string PARSING = "Parsing";
    public const string ERROR = "Error";
    public const string SYSTEM_ERROR = "SystemError";
    public const string UI = "UI";

    // Map of LVL names to display names
    private static readonly Dictionary<LVL, string> s_lvlNames = new Dictionary<LVL, string> { { LVL.Trace, "TRC" }, { LVL.Info, "INF" }, { LVL.Warning, "WRN" }, { LVL.Error, "ERR" }, { LVL.SystemError, "SER" } };

    // Constants of formats
    private const string s_dateTimeFormat = s_dateFormat + " " + s_timeFormat;
    private const string s_dateFormat = "yyyy.MM.dd";
    private const string s_timeFormat = "HH:mm:ss.fff";
    private const int PAD = 100;
    #endregion

    #region Fields

    #region Configuration

    /// <summary>
    /// Is caller is optimized traced
    /// </summary>
    public static bool IsOptimizedCaller { get; set; } = false;

    /// <summary>
    /// Minimum level of tracing messages that can go to files
    /// </summary>
    public static LVL TraceLevel { get; set; } = LVL.Trace;

    /// <summary>
    /// Path of log files
    /// </summary>
    public static string LogPath { get; set; }

    #endregion

    #region Static members

    /// <summary>
    /// Map of existing tracers
    /// </summary>
    private static readonly Dictionary<string, Tracer> s_tracers = new Dictionary<string, Tracer>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Time of app start
    /// </summary>
    private static readonly DateTimeOffset s_appStart = Process.GetCurrentProcess().StartTime;

    #endregion

    #region Instance members

    /// <summary>
    /// Locker for thread-sensitive data
    /// </summary>
    private readonly object m_lock = new object();

    /// <summary>
    /// Map of optimized callers
    /// </summary>
    private readonly Dictionary<int, int> m_callerOptimized = new Dictionary<int, int>();

    /// <summary>
    /// Data pending to be written
    /// </summary>
    private readonly Circular<string> m_pendingData = new Circular<string>(100);

    /// <summary>
    /// Tag of current tracer
    /// </summary>
    private readonly string m_tracerTag;

    /// <summary>
    /// Name of log file of that tracer
    /// </summary>
    private string m_logFileName;

    /// <summary>
    /// Value to understand if needs to switch to new file
    /// </summary>
    private int m_logDay;

    /// <summary>
    /// Was header written
    /// </summary>
    private Flag m_isHeaderWritten;

    #endregion

    #endregion

    #region Static

    /// <summary>
    /// Gets tracer with passed name
    /// </summary>
    public static Tracer Get(string name = COMMON)
    {
      name = name.IsNullOrWhiteSpace() ? COMMON : name;
      name = name.RemoveInvalidFilenameChars();
      return GetTracer(name);
    }

    /// <summary>
    /// Writes message to <see cref="ERROR"/> <see cref="Tracer"/> log
    /// </summary>
    public static void _Error(string message, [CallerFilePath] string file = null, [CallerMemberName] string method = null, [CallerLineNumber] int line = 0)
    {
      DateTimeOffset logTime = DateTimeOffset.Now;
      Caller caller = new Caller(file, method, line);
      Get(ERROR).Log(message, logTime, caller, LVL.Error);
    }

    /// <summary>
    /// Writes message to <see cref="ERROR"/> <see cref="Tracer"/> log
    /// </summary>
    public static void _SystemError(string message, [CallerFilePath] string file = null, [CallerMemberName] string method = null, [CallerLineNumber] int line = 0)
    {
      DateTimeOffset logTime = DateTimeOffset.Now;
      Caller caller = new Caller(file, method, line);
      Get(SYSTEM_ERROR).Log(message, logTime, caller, LVL.SystemError);
    }

    #endregion

    #region Constructors and Destructors

    /// <summary>
    /// Constructor
    /// </summary>
    private Tracer(string name)
    {
      m_tracerTag = name;
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Writes message to log with Trace level
    /// </summary>
    public void Trace(string message, [CallerFilePath] string file = null, [CallerMemberName] string method = null, [CallerLineNumber] int line = 0)
    {
      DateTimeOffset logTime = DateTimeOffset.Now;
      Caller caller = new Caller(file, method, line);
      Log(message, logTime, caller, LVL.Trace);
    }
    /// <summary>
    /// Writes message to log with Information level
    /// </summary>
    public void Info(string message, [CallerFilePath] string file = null, [CallerMemberName] string method = null, [CallerLineNumber] int line = 0)
    {
      DateTimeOffset logTime = DateTimeOffset.Now;
      Caller caller = new Caller(file, method, line);
      Log(message, logTime, caller, LVL.Info);
    }

    /// <summary>
    /// Writes message to log with Warning level
    /// </summary>
    public void Warn(string message, [CallerFilePath] string file = null, [CallerMemberName] string method = null, [CallerLineNumber] int line = 0)
    {
      DateTimeOffset logTime = DateTimeOffset.Now;
      Caller caller = new Caller(file, method, line);
      Log(message, logTime, caller, LVL.Warning);
    }

    /// <summary>
    /// Writes message to log with Error level. Also copies message to separate log with errors
    /// </summary>
    public void Error(string message, [CallerFilePath] string file = null, [CallerMemberName] string method = null, [CallerLineNumber] int line = 0)
    {
      DateTimeOffset logTime = DateTimeOffset.Now;
      Caller caller = new Caller(file, method, line);
      Log(message, logTime, caller, LVL.Error);
    }

    /// <summary>
    /// Start copying all messages from other tracers from current thread
    /// </summary>
    public void StartTreadTracing()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Stop copying all messages from other tracers from current thread
    /// </summary>
    public void StopThreadTracing()
    {
      throw new NotImplementedException();
    }

    #endregion

    #region Private static

    /// <summary>
    /// Gets tracer by name
    /// </summary>
    private static Tracer GetTracer(string name)
    {
      lock (s_tracers)
      {
        if (!s_tracers.TryGetValue(name, out var tracer))
        {
          tracer = new Tracer(name);
          s_tracers[name] = tracer;
        }
        return tracer;
      }
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Applies logic of logging: filters, addition tracing functions, limitations
    /// </summary>
    private void Log(string message, DateTimeOffset logTime, Caller caller, LVL lvl)
    {
      if (TraceLevel > lvl) { return; }
      // TODO: what if tracing is disabled
      // TODO: what if logs are oversizes

      List<Tracer> tracers = new List<Tracer> { this };
      if (lvl >= LVL.Error) { tracers.Add(GetTracer(ERROR)); }
      // TODO: add thread tracers

      tracers.ForEach(x => x.FormMessage(message, logTime, caller, lvl));
    }

    /// <summary>
    /// Forms message to log and writes it
    /// </summary>
    private void FormMessage(string message, DateTimeOffset logTime, Caller caller, LVL lvl)
    {
      lock (m_lock)
      {
        // TODO: if trace optimized
        // TODO: if Append

        string result;
        result = string.Format(Formatting_LVL(lvl), Environment.NewLine);
        result = string.Format(Formatting_DateTime(logTime), result);
        result = string.Format(Formatting_Caller(caller), result);
        result = string.Format(Formatting_Stack(), result);
        result = string.Format(Formatting_Indent(result), result);
        Formatting_Message(ref message);
        result += $"{caller.Method}: {message}";

        Write(result);
      }
    }

    /// <summary>
    /// Writes data to destination
    /// </summary>
    private void Write(string data)
    {
      try
      {
        // Check if LogPath is set.
        if (LogPath.IsNullOrEmpty())
        {
          m_pendingData.Push(data);
          return;
        }

        // Check if we switched to next day
        DateTimeOffset now = DateTimeOffset.Now;
        if (m_logDay != now.Day)
        {
          m_logDay = now.Day;
          m_logFileName = Formatting_LogFileName(now);
        }

        // Check if new file or directory needs to be created
        bool isNewFile = false;
        DataTree<string> error = new DataTree<string>();
        if (!FileOperations.Exists(m_logFileName, error))
        {
          if (!FileOperations.CreateDirectory(LogPath, error)) { return; }
          isNewFile = true;
          m_isHeaderWritten.Reset();
        }

        // Check if data is pending
        if (m_pendingData.Count > 0)
        {
          data = m_pendingData.StrJoin("") + data;
          m_pendingData.Clear();
        }

        // Check if need to write header
        if (!m_isHeaderWritten.CheckThenSet())
        {
          data = string.Format(isNewFile ? Formatting_NewFileStart() : Formatting_LogHeader(), data);
        }

        File.AppendAllText(m_logFileName, data);
      }
      catch (Exception ex) { var t = ex.Message; }
    }

    #endregion

    #region Formatting

    /// <summary>
    /// Format message
    /// </summary>
    private void Formatting_Message(ref string message)
    {
      message = message.Replace("\n", "\n".PadRight(PAD));
    }

    /// <summary>
    /// Returns formated DateTime info based on configuration
    /// </summary>
    private string Formatting_DateTime(DateTimeOffset time)
    {
      return "{0} " + time.ToString(s_dateTimeFormat);
    }

    /// <summary>
    /// Return formated Caller info based on configuration
    /// </summary>
    private string Formatting_Caller(Caller caller)
    {
      if (IsOptimizedCaller)
      {
        string shortcut = "";
        var hash = $"{caller.File}{caller.Line}".GetHashCode();
        if (!m_callerOptimized.TryGetValue(hash, out var value))
        {
          value = m_callerOptimized.Count;
          m_callerOptimized[hash] = value;
          shortcut = $"{Environment.NewLine}SHORTCUT: [C:{string.Format("{0:0000}", value)}] {caller.ToString("F", "M", "L")}";
        }
        return shortcut + "{0} " + $"[TRD={caller.Thread}, C:{string.Format("{0:0000}", value)}]";
      }
      return "{0} " + caller.ToString();
    }

    /// <summary>
    /// Return formated LVL based on configuration
    /// </summary>
    private string Formatting_LVL(LVL lvl)
    {
      return "{0} " + s_lvlNames[lvl];
    }

    /// <summary>
    /// Returns formated stack info based on configuration
    /// </summary>
    private string Formatting_Stack()
    {
      // TODO: real StackTrace
      return "{0}";
    }

    /// <summary>
    /// Makes message body start at least on specified index place
    /// </summary>
    private string Formatting_Indent(string data)
    {
      int index = data.LastIndexOf(Environment.NewLine);
      if (index == -1) { index = 0; }
      int spacesCount = data.Length - index;
      if (spacesCount > PAD) { return "{0}"; }
      else { return "{0}" + " ".PadLeft(PAD - spacesCount + 1); }
    }

    /// <summary>
    /// Returns file name formatted
    /// </summary>
    private string Formatting_LogFileName(DateTimeOffset dateTime)
    {
      return Path.Combine(LogPath, $"{dateTime.ToString(s_dateFormat)}_SIM_{m_tracerTag}.log");
    }

    /// <summary>
    /// Returns log start header
    /// </summary>
    private string Formatting_LogHeader()
    {
      return Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine +
      $"\r\n\r\n\r\n\r\n!!!!!!!!!!!!!! -----------= [LOG STARTS: {s_appStart.ToString(s_dateTimeFormat)}] =----------- !!!!!!!!!!!!!!" + Environment.NewLine +
      "LOG_NAME=" + m_tracerTag + Environment.NewLine +
      "PATH_NAME=" + m_logFileName + Environment.NewLine +
      "DIR_NAME=" + AppDomain.CurrentDomain?.BaseDirectory + Environment.NewLine +
      "APP_NAME=" + AppDomain.CurrentDomain?.FriendlyName + Environment.NewLine +
      "APP_VERSION=" + Assembly.GetEntryAssembly()?.GetName()?.Version + Environment.NewLine +
       Environment.NewLine + Environment.NewLine + "{0}";
    }

    /// <summary>
    /// Returns all saved optimized data
    /// </summary>
    private string Formatting_NewFileStart()
    {
      // TODO: add caller informations
      m_callerOptimized.Clear();
      // TODO: add StackTrace informations
      return Formatting_LogHeader();
    }

    #endregion

  }
}