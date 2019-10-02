using System;
using FMLib.ExtensionMethods;
using Utils.Logging;

namespace Utils
{
  /// <summary>
  /// Class representing time
  /// </summary>
  public class Time
  {

    #region Fields

    /// <summary>
    /// String representation of time in format: -5s
    /// </summary>
    private string m_toString = string.Empty;

    /// <summary>
    /// TimeSpan from <see cref="m_toString"/>
    /// </summary>
    public TimeSpan TimeSpan { get; private set; }

    /// <summary>
    /// Total milliseconds in <see cref="Time.TimeSpan"/>
    /// </summary>
    public int Milliseconds
    {
      get
      {
        if (TimeSpan > TimeSpan.FromMilliseconds(int.MaxValue)) { return int.MaxValue; }
        if (TimeSpan < TimeSpan.FromMilliseconds(int.MinValue)) { return int.MinValue; }
        try { return (int)TimeSpan.TotalMilliseconds; }
        catch (Exception ex) { Tracer._SystemError($"Time: exception converting TimeSpan to milliseconds: {ex.Message}"); return 0; }
      }
    }

    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    public Time(TimeSpan timeSpan, string timeString = null)
    {
      m_toString = timeString ?? string.Empty;
      TimeSpan = timeSpan;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public Time(string timeString) : this(Useful.ParseTimeSpan(timeString), timeString)
    {
      // nothing to do here
    }

    /// <summary>
    /// Implicit operator to <see cref="System.TimeSpan"/>
    /// </summary>
    public static implicit operator TimeSpan(Time time) => time.TimeSpan;

    /// <summary>
    /// ToString
    /// </summary>
    public override string ToString()
    {
      return m_toString.IsNullOrWhiteSpace() ? $"{TimeSpan.TotalSeconds}s" : m_toString;
    }
  }
}