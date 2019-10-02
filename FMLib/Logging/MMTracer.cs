using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Utils.Logging
{
  /// <summary>
  /// The class to trace and manage tracings
  /// </summary>
  /// <remarks>
  /// Initializing of whole tracing framework requires:
  ///
  /// Hooking unhandled exception to trace
  ///   Logging.Tracer.HookUnhandledException();
  ///
  /// Setting parameters:
  ///   Logging.Tracer.ApplicationAbbreviation = LocalMachineInformation.ApplicationAbbrev.ToLower();
  ///   Logging.Tracer.DayLimitLogsSizeInMB = tc.DayLimitLogsSizeInMB;
  ///   Logging.Tracer.LogPath = Path.Combine(LocalMachineInformation.ApplicationDataFolder, "Logs");
  ///   Logging.Tracer.FillLogHeader = logName => $@"
  ///         ADDITIONAL_DATA_TO_SHOW_IN_EVERY_TRACING_HEADER: {DATA_VALUE}
  ///         ANOTHER_DATA_TO_SHOW_IN_EVERY_TRACING_HEADER: {ANOTHER_DATA_VALUE}";
  ///   Logging.Tracer.EnableLogging = !tc.DisableFileLogging;
  ///
  /// Periodically cleaning logs:
  ///   Logging.Tracer.Clean(TimeSpan.FromDays(m_maxDaysToKeepTracerLogs), ErrorTracer);
  ///
  /// All tracer classes should be created by the Tracer.Create method
  ///   private Tracer m_tracer = Tracer.Create("main_tracing");
  ///
  /// To write optimized logs use the TraceOptimized method
  ///   m_tracer.TraceOptimized(Tracer.ErrorType, $"Caught the exception: {ex}");
  /// </remarks>
  public class MMTracer
  {
    /// <summary>
    /// Enables or disables all the tracing
    /// </summary>
    public static volatile bool EnableLogging = false;

    /// <summary>
    /// Path to write log files
    /// </summary>
    public static string LogPath;

    /// <summary>
    /// Callback to obtain header when tracing starts
    /// Parameter is log name
    /// </summary>
    public static Func<string, string> FillLogHeader;

    /// <summary>
    /// Application abbreviation, used as the name part of tracing files
    /// </summary>
    public static string ApplicationAbbreviation;

    /// <summary>
    /// size limit of all files per day PR-26437
    /// </summary>
    public static int DayLimitLogsSizeInMB { get; set; }

    /// <summary>
    /// We can stop optimizing logs by setting the flag to true
    /// </summary>
    public static bool DisableOptimizations = false;


    /// <summary>
    /// Trace level enum
    /// </summary>
    public enum Level
    {
      /// <summary>
      /// Info tracing type
      /// </summary>
      Info = 0,
      /// <summary>
      /// Warning tracing type
      /// </summary>
      Warning = 1,
      /// <summary>
      /// Error tracing type
      /// </summary>
      Error = 2,

      /// <summary>
      /// Constant to turn stack trace OFF
      /// </summary>
      StackTraceOff = 100,
    }

    /// <summary>
    /// Info tracing type
    /// </summary>
    public const Level InfoType = Level.Info;
    /// <summary>
    /// Warning tracing type
    /// </summary>
    public const Level WarningType = Level.Warning;
    /// <summary>
    /// Error tracing type
    /// </summary>
    public const Level ErrorType = Level.Error;

    /// <summary>
    /// Constant to turn stack trace OFF
    /// </summary>
    public const Level StackTraceOff = Level.StackTraceOff;


    /// <summary>
    /// Constructs a tracer object
    /// </summary>
    /// <param name="logFileName">Main Part of creating log files name</param>
    /// <param name="threadRelated"></param>
    /// <param name="traceStackLevel"></param>
    public static MMTracer Create(string logFileName = "common", bool threadRelated = true, Level traceStackLevel = ErrorType)
    {
      string name = logFileName.ToLower();
      lock (s_ExistingTracers)
      {
        if (!s_ExistingTracers.TryGetValue(name, out var tracer))
        {
          tracer = new MMTracer(logFileName)
          {
            ThreadRelatedTracing = threadRelated,
            TraceStackLevel = traceStackLevel,
          };
          s_ExistingTracers[name] = tracer;
        }

        return tracer;
      }
    }

    /// <summary>
    /// Delete old files
    /// </summary>
    /// <param name="logStoringPeriod"></param>
    /// <param name="tracer"></param>
    /// <param name="cleanAllFiles"></param>
    public static void Clean(TimeSpan logStoringPeriod, MMTracer tracer, bool cleanAllFiles = false)
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
        string abbrev = string.IsNullOrEmpty(ApplicationAbbreviation) ? "" : (ApplicationAbbreviation + "_");
        string[] files = cleanAllFiles
          ? Directory.GetFiles(MMTracer.LogPath, "*", SearchOption.AllDirectories)
          : Directory.GetFiles(MMTracer.LogPath, $"{s_FileDeleteMask}{abbrev}*.log", SearchOption.TopDirectoryOnly);
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
        tracer?.Error("Error deleting old log files.", ex);
      }
    }

    /// <summary>
    /// Add default log handler to log Unhandled Exceptions
    /// </summary>
    public static void HookUnhandledException()
    {
      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    /// <summary>
    /// Get existing file pathnames for a date
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static string[] GetLogFiles(DateTime date)
    {
      if (!Directory.Exists(MMTracer.LogPath))
      {
        // GetFiles on a path that does not exist throws an exception, interrupting startup procedure.
        return new string[0];
      }
      return Directory.GetFiles(MMTracer.LogPath, $"{date.ToString(s_FileDateMask)}*.log", SearchOption.AllDirectories);
    }

    /// <summary>
    /// Gets or Sets the default logger for current thread
    /// </summary>
    public static MMTracer CurrentThreadTracer
    {
      get
      {
        try { return Thread.GetData(Thread.GetNamedDataSlot(s_ThreadSlotName)) as MMTracer; }
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
          value?.Error("Can't set log file for this thread.", ex);
        }
      }
    }



    /// <summary>
    /// Is particular tracer enabled or not
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or Sets tread relative behavior
    /// Which allows to search thread Tracer and use it instead of called one.
    /// </summary>
    public bool ThreadRelatedTracing { get; set; }

    /// <summary>
    /// Sets starting level to add stack trace information
    /// </summary>
    public Level TraceStackLevel { get; set; }

    /// <summary>
    /// Add types of classes to exclude them from stack trace string
    /// </summary>
    public HashSet<Type> TracingTypesToIgnore { get; }




    /// <summary>
    /// Info tracing
    /// </summary>
    /// <param name="message">Message to be logged</param>
    /// <param name="exceptionMessage">Exception message to log separately</param>
    public void Info(string message, string exceptionMessage = null)
    {
      TraceInternal(InfoType, null, message, exceptionMessage, null, false);
    }
    /// <summary>
    /// Info tracing
    /// </summary>
    /// <param name="message">Message to be logged</param>
    /// <param name="exception">Exception to be logged</param>
    public void Info(string message, Exception exception)
    {
      TraceInternal(InfoType, null, message, null, exception, false);
    }
    /// <summary>
    /// Info tracing
    /// </summary>
    /// <param name="startTime">Start Time which is used to calculate duration when logging</param>
    /// <param name="message">Message to be logged</param>
    /// <param name="exceptionMessage">Exception message to log separately</param>
    public void Info(DateTime? startTime, string message, string exceptionMessage = null)
    {
      TraceInternal(InfoType, startTime, message, exceptionMessage, null, false);
    }
    /// <summary>
    /// Info tracing
    /// </summary>
    /// <param name="startTime">Start Time which is used to calculate duration when logging</param>
    /// <param name="message">Message to be logged</param>
    /// <param name="exception">Exception to be logged</param>
    public void Info(DateTime? startTime, string message, Exception exception)
    {
      TraceInternal(InfoType, startTime, message, null, exception, false);
    }

    /// <summary>
    /// Warning tracing
    /// </summary>
    /// <param name="message">Message to be logged</param>
    /// <param name="exceptionMessage">Exception message to log separately</param>
    public void Warning(string message, string exceptionMessage = null)
    {
      TraceInternal(WarningType, null, message, exceptionMessage, null, false);
    }
    /// <summary>
    /// Warning tracing
    /// </summary>
    /// <param name="message">Message to be logged</param>
    /// <param name="exception">Exception to be logged</param>
    public void Warning(string message, Exception exception)
    {
      TraceInternal(WarningType, null, message, null, exception, false);
    }
    /// <summary>
    /// Warning tracing
    /// </summary>
    /// <param name="startTime">Start Time which is used to calculate duration when logging</param>
    /// <param name="message">Message to be logged</param>
    /// <param name="exceptionMessage">Exception message to log separately</param>
    public void Warning(DateTime? startTime, string message, string exceptionMessage = null)
    {
      TraceInternal(WarningType, startTime, message, exceptionMessage, null, false);
    }
    /// <summary>
    /// Warning tracing
    /// </summary>
    /// <param name="startTime">Start Time which is used to calculate duration when logging</param>
    /// <param name="message">Message to be logged</param>
    /// <param name="exception">Exception to be logged</param>
    public void Warning(DateTime? startTime, string message, Exception exception)
    {
      TraceInternal(WarningType, startTime, message, null, exception, false);
    }

    /// <summary>
    /// Error tracing
    /// </summary>
    /// <param name="message">Message to be logged</param>
    /// <param name="exceptionMessage">Exception message to log separately</param>
    public void Error(string message, string exceptionMessage = null)
    {
      TraceInternal(ErrorType, null, message, exceptionMessage, null, false);
    }
    /// <summary>
    /// Error tracing
    /// </summary>
    /// <param name="exception">Exception to be logged</param>
    public void Error(Exception exception)
    {
      TraceInternal(ErrorType, null, "GENERAL EXCEPTION", null, exception, false);
    }
    /// <summary>
    /// Error tracing
    /// </summary>
    /// <param name="message">Message to be logged</param>
    /// <param name="exception">Exception to be logged</param>
    public void Error(string message, Exception exception)
    {
      TraceInternal(ErrorType, null, message, null, exception, false);
    }
    /// <summary>
    /// Error tracing
    /// </summary>
    /// <param name="startTime">Start Time which is used to calculate duration when logging</param>
    /// <param name="message">Message to be logged</param>
    /// <param name="exceptionMessage">Exception message to log separately</param>
    public void Error(DateTime? startTime, string message, string exceptionMessage = null)
    {
      TraceInternal(ErrorType, startTime, message, exceptionMessage, null, false);
    }
    /// <summary>
    /// Error tracing
    /// </summary>
    /// <param name="startTime">Start Time which is used to calculate duration when logging</param>
    /// <param name="message">Message to be logged</param>
    /// <param name="exception">Exception to be logged</param>
    public void Error(DateTime? startTime, string message, Exception exception)
    {
      TraceInternal(ErrorType, startTime, message, null, exception, false);
    }

    /// <summary>
    /// Common tracing method
    /// </summary>
    /// <param name="tracingType">Tracing Level Type</param>
    /// <param name="message">Message to be logged</param>
    /// <param name="exceptionMessage">Exception message to log separately</param>
    public void Trace(Level tracingType, string message, string exceptionMessage = null)
    {
      TraceInternal(tracingType, null, message, exceptionMessage, null, false);
    }
    /// <summary>
    /// Common tracing method
    /// </summary>
    /// <param name="tracingType">Tracing Level Type</param>
    /// <param name="message">Message to be logged</param>
    /// <param name="exception">Exception to be logged</param>
    public void Trace(Level tracingType, string message, Exception exception)
    {
      TraceInternal(tracingType, null, message, null, exception, false);
    }
    /// <summary>
    /// Common tracing method
    /// </summary>
    /// <param name="tracingType">Tracing Level Type</param>
    /// <param name="startTime">Start Time which is used to calculate duration when logging</param>
    /// <param name="message">Message to be logged</param>
    /// <param name="exceptionMessage">Exception message to log separately</param>
    public void Trace(Level tracingType, DateTime? startTime, string message, string exceptionMessage = null)
    {
      TraceInternal(tracingType, startTime, message, exceptionMessage, null, false);
    }
    /// <summary>
    /// Common tracing method
    /// </summary>
    /// <param name="tracingType">Tracing Level Type</param>
    /// <param name="startTime">Start Time which is used to calculate duration when logging</param>
    /// <param name="message">Message to be logged</param>
    /// <param name="exception">Exception to be logged</param>
    public void Trace(Level tracingType, DateTime? startTime, string message, Exception exception)
    {
      TraceInternal(tracingType, startTime, message, null, exception, false);
    }

    /// <summary>
    /// Append text to existing log without caret returning and additional formatting
    /// </summary>
    /// <param name="startTime">Start Time which is used to calculate duration when logging</param>
    /// <param name="message">message</param>
    /// <param name="prefix">new line prefix</param>
    /// <param name="maxLineLength">line length</param>
    public void Append(DateTime? startTime, string message, string prefix = null, int maxLineLength = 100)
    {
      DateTime now = DateTime.Now;

      { // try to trace with thread tracer
        MMTracer tracer = ThreadRelatedTracing ? CurrentThreadTracer : null;
        if (tracer != this)
        {
          tracer?.Append(startTime, message, prefix, maxLineLength);
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
            TraceInternal("\r\n", now, true);
          }
          TraceInternal(InfoType, startTime, prefix + message, null, null, true);
        }
        else
        {
          TraceInternal(message, now, true);
        }
      }
    }

    /// <summary>
    /// Append text to existing log without caret returning and additional formatting
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="prefix">new line prefix</param>
    /// <param name="maxLineLength">line length</param>
    public void Append(string message, string prefix = null, int maxLineLength = 100) => Append(null, message, prefix, maxLineLength);

    /// <summary>
    /// trace with replacing common messages by placeholders
    /// </summary>
    /// <param name="tracingType">Tracing Level Type</param>
    /// <param name="message">Message to be logged</param>
    /// <param name="exception">Exception to log</param>
    public void TraceOptimized(Level tracingType, string message, Exception exception = null) { GetOptimizedTrace(tracingType, null, message, message, null, exception, false); }
    /// <summary>
    /// trace with replacing common messages by placeholders, using hash as placeholder key
    /// </summary>
    /// <param name="tracingType">Tracing Level Type</param>
    /// <param name="hash"></param>
    /// <param name="message">Message to be logged</param>
    /// <param name="exception">Exception to log</param>
    public void TraceOptimized(Level tracingType, string hash, string message, Exception exception = null) { GetOptimizedTrace(tracingType, null, hash, message, null, exception, false); }

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
          bool ignore = TracingTypesToIgnore.Contains(method.DeclaringType);

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

    #region static implementation

    private static readonly string s_FileDateMask = "yyyy-MM-dd-";
    private static readonly string s_FileDeleteMask = "????-??-??-";
    private static readonly string s_DateFormat = "dd.MM.yyyy HH:mm:ss.fff";
    private static readonly string s_ThreadSlotName = "MMTracer";
    private static readonly Dictionary<string, MMTracer> s_ExistingTracers = new Dictionary<string, MMTracer>();
    private static readonly string[] s_TracingTypeStrings = { "INF", "WRN", "ERR" };
    private static DateTime s_nextCheckLogsSizeTime;
    private static volatile bool s_EnableLoggingLimitation = false;

    private static bool? AreLogsOverflowed(DateTime now)
    {
      if (s_nextCheckLogsSizeTime > now)
      {
        return null;
      }

      // How often check for logs size
      TimeSpan checkLogsSizePeriod = TimeSpan.FromMinutes(5);

      s_nextCheckLogsSizeTime = now.Add(checkLogsSizePeriod);

      long totalSizeInBytes = 0;

      if (DayLimitLogsSizeInMB > 0) // if DayLimitLogsSizeInMB set to zero or lesser - there is no limitation
      {
        //calculates total size of logs for today
        string[] files = GetLogFiles(now);
        foreach (string fn in files)
        {
          try { totalSizeInBytes += new FileInfo(fn).Length; }
          catch { }
        }
      }

      return (totalSizeInBytes / 1024 / 1024) > DayLimitLogsSizeInMB;
    }
    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      MMTracer tracer = MMTracer.Create("unhandled");
      tracer.Error("Unhandled exception occurred!", e.ExceptionObject?.ToString());
    }

    #endregion static implementation

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

    private MMTracer(string logFileName)
    {
      if (ApplicationAbbreviation == null)
      {
        ApplicationAbbreviation = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
      }
      if (LogPath == null)
      {
        LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
      }

      m_logFileName = logFileName;
      TracingTypesToIgnore = new HashSet<Type> { typeof(MMTracer) };
      IsEnabled = true;
    }

    private string GetOptimizedStackTrace(out string stackTraceDefinition)
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

    private void TraceInternal(Level tracingType, DateTime? startTime, string message, string exceptionMessage, Exception exception, bool appendLine)
    {
      DateTime now = DateTime.Now;

      { // try to trace with thread tracer
        MMTracer tracer = ThreadRelatedTracing ? CurrentThreadTracer : null;
        if (tracer != this)
        {
          tracer?.TraceInternal(tracingType, startTime, message, exceptionMessage, exception, appendLine);
        }
      }

      string stackTrace = "";
      if (!appendLine && (TraceStackLevel <= tracingType))
      {
        stackTrace = "[" + GetOptimizedStackTrace(out string stackTraceDefinition) + "]";
        if (!string.IsNullOrEmpty(stackTraceDefinition))
        {
          TraceInternal($"STACK: {stackTrace}={stackTraceDefinition}", now, false);
        }
        stackTrace += "  ";
      }

      TraceInternal((((tracingType >= 0) && ((int)tracingType < s_TracingTypeStrings.Length)) ? s_TracingTypeStrings[(int)tracingType] : "UNK") + " "
                                         + now.ToString(s_DateFormat)
                                         + (" (THRD=" + Thread.CurrentThread.ManagedThreadId + ")" + (startTime == null ? ":" : ",")).PadRight(14)
                                         + (startTime == null ? "" : ("(" + (int)(now - startTime.Value).TotalMilliseconds + "ms):")).PadRight(10)
                                         + stackTrace
                                         + message
                                         + (exceptionMessage == null ? "" : (" EX: " + exceptionMessage))
                                         + (exception == null ? "" : (" EX: \r\n" + exception))
        , now, appendLine);
    }

    private void TraceInternal(string message, DateTime now, bool appendLine)
    {
      if (!EnableLogging || !IsEnabled)
      {
        return;
      }

      bool? areLogsOverflowed;
      lock (m_locker)
      {
        areLogsOverflowed = AreLogsOverflowed(now);

        if (s_EnableLoggingLimitation && (areLogsOverflowed ?? true))
        {
          return;
        }

        s_EnableLoggingLimitation = false;

        try
        {
          if (m_logDate != now.Date)
          {
            m_logDate = now.Date;
            string abbrev = string.IsNullOrEmpty(ApplicationAbbreviation) ? "" : (ApplicationAbbreviation + "_");
            m_logPathName = Path.Combine(LogPath, $"{m_logDate.ToString(s_FileDateMask)}{abbrev}{m_logFileName}.log");
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

            TraceInternal("\r\n\r\n\r\n\r\n!!!!!!!!!!!!!! -----------= LOG STARTS AT: " + now + " =----------- !!!!!!!!!!!!!!", now, false);
            TraceInternal("LOG_NAME=" + m_logFileName, now, false);
            TraceInternal("PATH_NAME=" + m_logPathName, now, false);
            TraceInternal("DIR_NAME=" + AppDomain.CurrentDomain.BaseDirectory, now, false);
            TraceInternal("APP_NAME=" + AppDomain.CurrentDomain.FriendlyName, now, false);
            try
            {
              TraceInternal("APP_BUILT=" + new FileInfo(AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName).LastWriteTimeUtc + " UTC", now, false);
            }
            catch
            {
              TraceInternal("APP_BUILT=Could not get build time", now, false);
            }
            if (FillLogHeader != null)
            {
              try { TraceInternal(FillLogHeader(m_logFileName), now, false); }
              catch (Exception ex) { TraceInternal("HEADER EXCEPTION: " + ex.Message, now, false); }
            }

            TraceInternal("", now, false);

            m_stackInfo.Clear();

            lock (m_optimizedLock)
            {
              m_logCacheList.Clear(); // clears optimized placeholders to define them in the new log.
            }
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
              File.AppendAllText(m_logPathName, $@"
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
WRITING LOGS FAILED {m_errorAttempts} TIMES
LAST LOG MESSAGE: {m_lastLogWriteFailMessage}
", m_logEncoding);
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

      if (areLogsOverflowed ?? false)
      {
        lock (s_ExistingTracers)
        {
          foreach (MMTracer tracer in s_ExistingTracers.Values)
          {
            tracer.TraceInternal("Logs are stopped due to day size limit.", now, false);
          }
        }
        s_EnableLoggingLimitation = true;
      }
    }

    private void GetOptimizedTrace(Level tracingType, DateTime? startTime, string hash, string message, string exceptionMessage, Exception exception, bool appendLine)
    {
      if (DisableOptimizations)
      {
        TraceInternal(tracingType, startTime, message, exceptionMessage, exception, appendLine);
      }
      else
      {
        string s = GetOptimizedLog(hash, message);
        if (s.Length == 1) // placeholder
        {
          Append(s);
        }
        else // placeholder definition
        {
          TraceInternal(tracingType, startTime, s, exceptionMessage, exception, appendLine);
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

    private string GetOptimizedLog(string hash, string message)
    {
      lock (m_optimizedLock)
      {
        DateTime now = DateTime.Now;
        List<LogCacheInfo> list2Remove = new List<LogCacheInfo>();
        LogCacheInfo lci = null;
        LogCacheInfo lciOldest = null;
        string symbols = "*#xo+@$vABCDEF";
        foreach (LogCacheInfo item in m_logCacheList)
        {
          if (now - item.Time > m_removeSpan)
            list2Remove.Add(item);
          else
          {
            symbols = symbols.Replace(item.Symbol, ' ');
            if (item.Descr == hash)
            {
              lci = item;
              break;
            }
            if ((lciOldest == null) || (item.Time < lciOldest.Time))
              lciOldest = item;
          }
        }
        foreach (LogCacheInfo item in list2Remove)
          m_logCacheList.Remove(item);

        if (lci == null)
        {
          symbols = symbols.Replace(" ", "").Trim();
          if (symbols.Length == 0)
            lci = lciOldest;
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
        if (now - lci.LogTime > m_repeatSpan) //  || (lci.LogStr != log_str) - commented condition reduces readability, what is why it is commented :)
        {
          lci.LogStr = message;
          lci.LogTime = now;
          return "[" + lci.Symbol + "] " + message + "\r\n";
        }
        return "" + lci.Symbol;
      }
    }
    #endregion implementation
  }
}
