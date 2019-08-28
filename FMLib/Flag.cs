namespace Utils
{
  /// <summary>
  /// Represents flag
  /// </summary>
  public struct Flag
  {

    #region Fields

    /// <summary>
    /// Flag value
    /// </summary>
    private bool m_flag;

    #endregion

    #region Constructors and Destructors

    /// <summary>
    /// Bool implicit operator. To use that class instance as bool
    /// </summary>
    public static implicit operator bool(Flag flag) => flag.m_flag;

    #endregion

    #region Public methods

    /// <summary>
    /// Returns value of flag and then sets it to true
    /// </summary>
    public bool CheckThenSet()
    {
      bool result = m_flag;
      m_flag = true;
      return result;
    }

    /// <summary>
    /// Checks for current flag value
    /// </summary>
    public bool Check() => m_flag;

    /// <summary>
    /// Sets flag to true
    /// </summary>
    public void Set() => m_flag = true;

    /// <summary>
    /// Resets flag to false
    /// </summary>
    public void Reset() => m_flag = false;

    /// <summary>
    /// To string
    /// </summary>
    public override string ToString() => m_flag.ToString();

    #endregion

  }
}