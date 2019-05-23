using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FMLib
{
  /// <summary>
  /// The class to trace and manage tracings
  /// </summary>
  public class Tracer
  {
    /// <summary>
    /// Path to write log files
    /// </summary>
    public static volatile string LogPath = "C:\\ProgramData\\Micromine\\PITRAM\\Logs\\Simulator";

    /// <summary>
    /// Callback to obtain header when tracing starts
    /// Parameter is log name
    /// </summary>
    public static Func<string, string> FillLogHeader;

    /// <summary>
    /// We can stop optimizing logs by setting the flag to true
    /// </summary>
    public static bool DisallowPlaceholders = false;

    private static readonly string s_FileDateMask = "yyyy-MM-dd-";
    private static readonly string s_FileDeleteMask = "????-??-??-";
    private static readonly string s_DateFormat = "dd.MM.yyyy HH:mm:ss.fff";
    private static readonly string s_ThreadSlotName = "MMTracer";
    private static readonly Dictionary<string, Tracer> s_ExistingLoggers = new Dictionary<string, Tracer>();
    private static readonly string[] s_TracingTypeStrings = { "INF", "WRN", "ERR" };

    /// <summary>
    /// Info tracing type
    /// </summary>
    public const int InfoType = 0;
    /// <summary>
    /// Warning tracing type
    /// </summary>
    public const int WarningType = 1;
    /// <summary>
    /// Error tracing type
    /// </summary>
    public const int ErrorType = 2;

    /// <summary>
    /// Constant to turn stack trace OFF
    /// </summary>
    public const int StackTraceOff = 100;

    /// <summary>
    /// Enables or disables all the tracing
    /// </summary>
    public static volatile bool EnableLogging = true;

    /// <summary>
    /// Enables or disables all the tracing based on day limit size of logs
    /// </summary>
    public static volatile bool EnableLoggingLimitation = false;

    /// <summary>
    /// Gets or Sets tread relative behavior
    /// Which allows to search thread Tracer and use it instead of called one.
    /// </summary>
    public bool ThreadRelatedTracing { get; set; } = false;

    /// <summary>
    /// Sets starting level to add stack trace information
    /// </summary>
    public int TraceStackLevel { get; set; }

    /// <summary>
    /// Add types of classes to exclude them from stack trace string
    /// </summary>
    public HashSet<Type> TracingTypesToIgnore { get; }

    /// <summary>
    /// Constructs a tracer object
    /// </summary>
    /// <param name="logFileName">Main Part of creating log files name</param>
    /// <param name="threadRelated"></param>
    /// <param name="traceStackLevel"></param>
    public static Tracer Create(string logFileName = "common", bool threadRelated = true, int traceStackLevel = ErrorType)
    {
      logFileName = RemoveInvalidFilenameChars(logFileName);
      string name = logFileName.ToLower();
      lock (s_ExistingLoggers)
      {
        if (!s_ExistingLoggers.TryGetValue(name, out var tracer))
        {
          tracer = new Tracer(logFileName)
          {
            ThreadRelatedTracing = threadRelated,
            TraceStackLevel = traceStackLevel,
          };
          s_ExistingLoggers[name] = tracer;
        }

        return tracer;
      }
    }

    /// <summary>
    /// Removes invalid chars from filename
    /// </summary>
    /// <param name="logFileName"></param>
    /// <returns></returns>
    private static string RemoveInvalidFilenameChars(string target)
    {
      StringBuilder result = new StringBuilder(target);
      char[] invalidChars = Path.GetInvalidFileNameChars();
      foreach(var e in invalidChars) { result.Replace(e, '$'); }
      return result.ToString();
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logFileName"></param>
    private Tracer(string logFileName)
    {
      m_logFileName = logFileName;
      TracingTypesToIgnore = new HashSet<Type> { typeof(Tracer) };
    }

    /// <summary>
    /// Delete old files
    /// </summary>
    /// <param name="logStoringPeriod"></param>
    /// <param name="tracer"></param>
    /// <param name="cleanAllFiles"></param>
    public static void Clean(TimeSpan logStoringPeriod, Tracer tracer, bool cleanAllFiles = false)
    {
      try
      {
        if (logStoringPeriod < TimeSpan.FromDays(1))
        {
          tracer?.Info("Cleaning of old log files is switched off due to zero log storing period.");
          return;
        }

        int deleted = 0;
        DateTime cutoffTime = DateTime.Now - logStoringPeriod;
        string[] files = cleanAllFiles
          ? Directory.GetFiles(Tracer.LogPath, "*", SearchOption.AllDirectories)
          : Directory.GetFiles(Tracer.LogPath, $"{s_FileDeleteMask}SIM_*.log", SearchOption.TopDirectoryOnly);
        foreach (string fn in files)
        {
          if ((new FileInfo(fn)).LastWriteTime < cutoffTime)
          {
            try
            {
              File.Delete(fn);
              deleted++;
              tracer?.Info($"file '{fn}' is deleted.");
            }
            catch (Exception ex)
            {
              tracer?.Error($"Error deleting the old log file: {fn}", ex.Message);
            }
          }
        }
        if (deleted > 0)
        {
          tracer?.Info($"{deleted} old log files was deleted.");
        }
      }
      catch (Exception ex)
      {
        tracer?.Error($"Error deleting old log files. {ex}");
      }
    }

    /// <summary>
    /// Add default log handler to log Unhandled Exceptions
    /// </summary>
    public static void HookUnhandledException()
    {
      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }
    static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      Tracer tracer = Tracer.Create("unhandled");
      tracer.Error("Unhandled exception occurred!", e.ExceptionObject?.ToString());
    }

    /// <summary>
    /// Get existing file pathnames for a date
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static string[] GetLogFiles(DateTime date)
    {
      if (!Directory.Exists(Tracer.LogPath))
      {
        // GetFiles on a path that does not exist throws an exception, interrupting startup procedure.
        return new string[0];
      }
      return Directory.GetFiles(Tracer.LogPath, $"{date.ToString(s_FileDateMask)}*.log", SearchOption.AllDirectories);
    }

    /// <summary>
    /// Gets or Sets the default logger for current thread
    /// </summary>
    public static Tracer CurrentThreadTracer
    {
      get
      {
        try { return Thread.GetData(Thread.GetNamedDataSlot(s_ThreadSlotName)) as Tracer; }
        catch { return null; }
      }
      set
      {
        try
        {
          Thread.SetData(Thread.GetNamedDataSlot(s_ThreadSlotName), value);
        }
        catch (Exception ex)
        {
          value?.Error($"Can't set log file for this thread. {ex}");
        }
      }
    }

    /// <summary>
    /// Info tracing
    /// </summary>
    /// <param name="message">Message to be logged</param>
    /// <param name="exceptionMessage">Exception message to log separately</param>
    public void Info(string message, [CallerMemberName] string method = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
    {
      DateTime now = DateTime.Now;
      message = $"{method}: {message}\t[file:{Path.GetFileName(file)}, line:{line}]";
      //Task.Factory.StartNew(() => trace(InfoType, null, message, null, null, false, now));
      trace(InfoType, null, message, null, null, false, now);
    }

    /// <summary>
    /// Info tracing
    /// </summary>
    /// <param name="startTime">Start Time which is used to calculate duration when logging</param>
    /// <param name="message">Message to be logged</param>
    /// <param name="exceptionMessage">Exception message to log separately</param>
    public void Info(DateTime startTime, string message, [CallerMemberName] string method = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
    {
      DateTime now = DateTime.Now;
      message = $"{message}\t[file:{Path.GetFileName(file)}, line:{line}, method:{method}]";
      //Task.Factory.StartNew(() => trace(InfoType, startTime, message, null, null, false, now));
      trace(InfoType, startTime, message, null, null, false, now);
    }

    /// <summary>
    /// Warning tracing
    /// </summary>
    /// <param name="message">Message to be logged</param>
    /// <param name="exceptionMessage">Exception message to log separately</param>
    public void Warning(string message, [CallerMemberName] string method = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
    {
      DateTime now = DateTime.Now;
      message = $"{message}\t[file:{Path.GetFileName(file)}, line:{line}, method:{method}]";
      //Task.Factory.StartNew(() => trace(WarningType, null, message, null, null, false, now));
      trace(WarningType, null, message, null, null, false, now);
    }

    /// <summary>
    /// Warning tracing
    /// </summary>
    /// <param name="startTime">Start Time which is used to calculate duration when logging</param>
    /// <param name="message">Message to be logged</param>
    /// <param name="exceptionMessage">Exception message to log separately</param>
    public void Warning(DateTime startTime, string message, [CallerMemberName] string method = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
    {
      DateTime now = DateTime.Now;
      message = $"{message}\t[file:{Path.GetFileName(file)}, line:{line}, method:{method}]";
      //Task.Factory.StartNew(() => trace(WarningType, startTime, message, null, null, false, now));
      trace(WarningType, startTime, message, null, null, false, now);
    }

    /// <summary>
    /// Error tracing
    /// </summary>
    /// <param name="message">Message to be logged</param>
    /// <param name="exceptionMessage">Exception message to log separately</param>
    public void Error(string message, [CallerMemberName] string method = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
    {
      DateTime now = DateTime.Now;
      message = $"{message}\t[file:{Path.GetFileName(file)}, line:{line}, method:{method}]";
      //Task.Factory.StartNew(() => trace(ErrorType, null, message, null, null, false, now));
      trace(ErrorType, null, message, null, null, false, now);
    }

    /// <summary>
    /// Error tracing
    /// </summary>
    /// <param name="startTime">Start Time which is used to calculate duration when logging</param>
    /// <param name="message">Message to be logged</param>
    /// <param name="exceptionMessage">Exception message to log separately</param>
    public void Error(DateTime startTime, string message, [CallerMemberName] string method = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
    {
      DateTime now = DateTime.Now;
      message = $"{message}\t[file:{Path.GetFileName(file)}, line:{line}, method:{method}]";
      //Task.Factory.StartNew(() => trace(ErrorType, startTime, message, null, null, false, now));
      trace(ErrorType, startTime, message, null, null, false, now);
    }

    /// <summary>
    /// Append text to existing log without caret returning and additional formatting
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="prefix">new line prefix</param>
    /// <param name="maxLineLength">line length</param>
    public void Append(string message, string prefix = null, int maxLineLength = 100)
    {
      DateTime now = DateTime.Now;
      //Task.Factory.StartNew(() =>
      //{
      { // try to trace with thread tracer
        Tracer tracer = ThreadRelatedTracing ? CurrentThreadTracer : null;
        if (tracer != this)
        {
          tracer?.Append(message, prefix, maxLineLength);
        }
      }

      lock (m_locker)
      {
        bool newLine = (m_pureCounter == 0);
        m_pureCounter += message?.Length ?? 0;
        if (m_pureCounter >= maxLineLength) // new string after 80 characters
        {
          m_pureCounter = 0;
        }
        if (newLine)
        {
          if (m_lineWasAppended)
          {
            trace("\r\n", now, true);
          }
          trace(InfoType, null, prefix + message, null, null, true, now);
        }
        else
        {
          trace(message, now, true);
        }
      }
      // });
    }

    /// <summary>
    /// trace with replacing common messages by placeholders
    /// </summary>
    /// <param name="tracingType">Tracing Level Type</param>
    /// <param name="message">Message to be logged</param>
    public void TraceOptimized(int tracingType, string message)
    {
      DateTime now = DateTime.Now;
      Task.Factory.StartNew(() => optimizedTrace(tracingType, null, message, message, null, null, false, now));
    }
    /// <summary>
    /// trace with replacing common messages by placeholders, using hash as placeholder key
    /// </summary>
    /// <param name="tracingType">Tracing Level Type</param>
    /// <param name="hash"></param>
    /// <param name="message">Message to be logged</param>
    public void TraceOptimized(int tracingType, string hash, string message)
    {
      DateTime now = DateTime.Now;
      Task.Factory.StartNew(() => optimizedTrace(tracingType, null, hash, message, null, null, false, now));
    }

    #region formatters

    /// <summary>
    /// format dataRow to log string
    /// </summary>
    /// <param name="dataRow"></param>
    public static string Format(DataRow dataRow)
    {
      try
      {
        return dataRow == null ? "NULL" :
          string.Join(",", dataRow.Table.Columns.Cast<DataColumn>().Select(c => $"{c.ColumnName ?? c.Ordinal.ToString()}='{dataRow[c]}'"));
      }
      catch (Exception ex) { return "EX:" + ex.Message; }
    }

    /// <summary>
    /// Returns short stack trace string
    /// </summary>
    /// <returns></returns>
    public string StackTrace()
    {
      string stackIndent = "";
      try
      {
        string lastFileName = "";
        StackTrace st = new StackTrace(true);
        for (int i = 3; i < st.FrameCount; i++)
        {
          // Note that at this level, there are four
          // stack frames, one for each method invocation.
          StackFrame sf = st.GetFrame(i);

          var method = sf.GetMethod();
          bool ignore = false;
          foreach (var typeToIgnore in TracingTypesToIgnore)
          {
            if (typeToIgnore == method.DeclaringType)
            {
              ignore = true;
              break;
            }
          }

          if (!ignore)
          {
            string fileName = sf.GetFileName();
            if (string.IsNullOrEmpty(fileName)) // release without .pdb debug info
            {
              stackIndent = $"/{method.DeclaringType}:{method}" + stackIndent;
            }
            else
            {
              fileName = Path.GetFileName(fileName);
              string info = $"{method?.Name}:{sf.GetFileLineNumber()}";
              if (lastFileName == fileName)
              {
                stackIndent = stackIndent.Substring(0, fileName.Length + 2) + info + "," + stackIndent.Substring(fileName.Length + 2);
              }
              else
              {
                stackIndent = $"/{fileName}:" + info + stackIndent;
              }
              lastFileName = fileName;
            }
          }
        }
      }
      catch
      {
        stackIndent = "/EXCEPTION" + stackIndent;
      }

      return stackIndent;
    }

    #endregion formatters

    #region implementation

    private readonly object m_locker = new object();
    private readonly Encoding m_logEncoding = Encoding.UTF8;
    private readonly string m_logFileName;
    private readonly Dictionary<string, uint> m_stackInfo = new Dictionary<string, uint>();

    private DateTime m_logDate = DateTime.MinValue;
    private bool m_started = false;
    private bool m_lineWasAppended = false;
    private volatile int m_pureCounter = 0;
    private string m_logPathName = "";
    private int m_errorAttempts = 0;
    private string m_lastLogWriteFailMessage = "";

    private string optimizedStackTrace(out string stackTraceDefinition)
    {
      string stackTrace = StackTrace();

      lock (m_stackInfo)
      {
        if (!m_stackInfo.TryGetValue(stackTrace, out uint number))
        {
          m_stackInfo[stackTrace] = number = (uint)m_stackInfo.Count;
          stackTraceDefinition = stackTrace;
        }
        else
        {
          stackTraceDefinition = null;
        }
        return number.ToString("X4");
      }
    }

    private void trace(int tracingType, DateTime? startTime, string message, string exceptionMessage, Exception exception, bool appendLine, DateTime now)
    {
      { // try to trace with thread tracer
        Tracer tracer = ThreadRelatedTracing ? CurrentThreadTracer : null;
        if (tracer != this)
        {
          tracer?.trace(tracingType, startTime, message, exceptionMessage, exception, appendLine, now);
        }
      }

      string stackTrace = "";
      if (!appendLine && (TraceStackLevel <= tracingType))
      {
        stackTrace = "[" + optimizedStackTrace(out string stackTraceDefinition) + "]";
        if (!string.IsNullOrEmpty(stackTraceDefinition))
        {
          trace($"STACK: {stackTrace}={stackTraceDefinition}", now, false);
        }
        stackTrace += "  ";
      }

      trace((((tracingType >= 0) && (tracingType < s_TracingTypeStrings.Length)) ? s_TracingTypeStrings[tracingType] : "UNK") + " "
                                         + now.ToString(s_DateFormat)
                                         + (" (THRD=" + Thread.CurrentThread.ManagedThreadId + ")" + (startTime == null ? ":" : ",")).PadRight(14)
                                         + (startTime == null ? "" : ("(" + (int)(now - startTime.Value).TotalMilliseconds + "ms):")).PadRight(10)
                                         + stackTrace
                                         + message
                                         + (exceptionMessage == null ? "" : (" EX: " + exceptionMessage))
                                         + (exception == null ? "" : (" EX: \r\n" + exception))
        , now, appendLine);
    }


    private void trace(string message, DateTime now, bool appendLine)
    {
      if (!EnableLogging)
      {
        return;
      }

      lock (m_locker)
      {
        try
        {
          if (m_logDate != now.Date)
          {
            m_logDate = now.Date;
            m_logPathName = Path.Combine(LogPath, $"{m_logDate.ToString(s_FileDateMask)}SIM_{m_logFileName}.log");
          }

          if (!File.Exists(m_logPathName))
          {
            try { Directory.CreateDirectory(LogPath); }
            catch { }
            File.AppendAllText(m_logPathName, "", m_logEncoding); // creating new file, to avoid of stack overflow
            m_started = false;
          }

          if (!m_started)
          {
            m_started = true;

            trace("\r\n\r\n\r\n\r\n!!!!!!!!!!!!!! -----------= LOG STARTS AT: " + now + " =----------- !!!!!!!!!!!!!!", now, false);
            trace("LOG_NAME=" + m_logFileName, now, false);
            trace("PATH_NAME=" + m_logPathName, now, false);
            trace("DIR_NAME=" + AppDomain.CurrentDomain.BaseDirectory, now, false);
            trace("APP_NAME=" + AppDomain.CurrentDomain.FriendlyName, now, false);
            try
            {
              trace("APP_BUILT=" + new FileInfo(AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName).LastWriteTimeUtc + " UTC", now, false);
            }
            catch
            {
              trace("APP_BUILT=Could not get build time", now, false);
            }
            if (FillLogHeader != null)
            {
              try { trace(FillLogHeader(m_logFileName), now, false); }
              catch (Exception ex) { trace("HEADER EXCEPTION: " + ex.Message, now, false); }
            }

            trace("", now, false);

            m_stackInfo.Clear();
          }

          if (!appendLine)
          {
            m_pureCounter = 0;
          }

          File.AppendAllText(m_logPathName, ((m_lineWasAppended && !appendLine) ? "\r\n" : "") + message + (appendLine ? "" : "\r\n"), m_logEncoding);
          if (m_errorAttempts != 0)
          {
            try
            {
              File.AppendAllText(m_logPathName,
                "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" +
                "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" +
                $"WRITING LOGS FAILED {m_errorAttempts} TIMES" +
                $"LAST LOG MESSAGE: {m_lastLogWriteFailMessage}",
                m_logEncoding);
              m_errorAttempts = 0;
            }
            catch { } // still have a problem
          }
        }
        catch (Exception ex)
        {
          try { m_lastLogWriteFailMessage = message + " EX: " + ex; }
          catch { m_lastLogWriteFailMessage = "UNKNOWN EX: " + ex; }
          m_errorAttempts++;
        }
        m_lineWasAppended = appendLine;
      }
    }

    private void optimizedTrace(int tracingType, DateTime? startTime, string hash, string message, string exceptionMessage, Exception exception, bool appendLine, DateTime now)
    {
      if (DisallowPlaceholders)
      {
        trace(tracingType, startTime, message, exceptionMessage, exception, appendLine, now);
      }
      else
      {
        string s = getOptimizedLog(hash, message);
        if (s.Length == 1) // placeholder
        {
          Append(s);
        }
        else // placeholder definition
        {
          trace(tracingType, startTime, s, exceptionMessage, exception, appendLine, now);
        }
      }
    }

    private class LogCacheInfo
    {
      public string Descr;
      public string LogStr;
      public DateTime Time;
      public DateTime LogTime;
      public char Symbol;
    }
    private readonly List<LogCacheInfo> m_logCacheList = new List<LogCacheInfo>();
    private readonly TimeSpan m_repeatSpan = TimeSpan.FromMinutes(10);
    private readonly TimeSpan m_removeSpan = TimeSpan.FromMinutes(1);
    private readonly object m_optimizedLock = new object();

    private string getOptimizedLog(string hash, string log_str)
    {
      lock (m_optimizedLock)
      {
        DateTime now = DateTime.Now;
        List<LogCacheInfo> list2remove = new List<LogCacheInfo>();
        LogCacheInfo lci = null;
        LogCacheInfo lci_oldest = null;
        string symbols = "QWERTYUIOPASDFGHJKLZXCVBNM";
        foreach (LogCacheInfo item in m_logCacheList)
        {
          if (now - item.Time > m_removeSpan)
            list2remove.Add(item);
          else
          {
            symbols = symbols.Replace(item.Symbol, ' ');
            if (item.Descr == hash)
            {
              lci = item;
              break;
            }
            if ((lci_oldest == null) || (item.Time < lci_oldest.Time))
              lci_oldest = item;
          }
        }
        foreach (LogCacheInfo item in list2remove)
          m_logCacheList.Remove(item);

        if (lci == null)
        {
          symbols = symbols.Replace(" ", "").Trim();
          if (symbols.Length == 0)
            lci = lci_oldest;
          else
          {
            lci = new LogCacheInfo();
            lci.Symbol = symbols[0];
            m_logCacheList.Add(lci);
          }
          lci.Descr = hash;
          lci.LogStr = "";
          lci.LogTime = DateTime.MinValue;
        }

        lci.Time = now;
        if (now - lci.LogTime > m_repeatSpan) //  || (lci.LogStr != log_str) - commented condition deduces readability, what is why it is commented :)
        {
          lci.LogStr = log_str;
          lci.LogTime = now;
          return "[" + lci.Symbol + "] " + log_str + "\r\n";
        }
        return "" + lci.Symbol;
      }
    }
    #endregion implementation
  }
}