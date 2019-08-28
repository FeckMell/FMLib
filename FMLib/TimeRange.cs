namespace Utils
{
  /// <summary>
  /// Class representing time range
  /// </summary>
  public class TimeRange
  {
    /// <summary>
    /// to string
    /// </summary>
    private string m_toString = string.Empty;

    /// <summary>
    /// Start time
    /// </summary>
    public Time Start { get; private set; }

    /// <summary>
    /// Duration time
    /// </summary>
    public Time Duration { get; private set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public TimeRange(string timeRangeString)
    {
      (Start, Duration) = Useful.ParseTimeRange(timeRangeString);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public TimeRange(Time start, Time duration, string timeRangeString = null)
    {
      m_toString = timeRangeString ?? string.Empty;
      Start = start ?? throw new ExpectedException($"Null argument {nameof(start)}");
      Duration = duration ?? throw new ExpectedException($"Null argument {nameof(duration)}");
    }

    /// <summary>
    /// ToString
    /// </summary>
    public override string ToString()
    {
      return m_toString.IsNullOrWhiteSpace() ? $"{Start.ToString()}:{Duration.ToString()}" : m_toString;
    }
  }
}