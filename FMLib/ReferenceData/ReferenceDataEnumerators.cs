namespace Utils.ReferenceData
{
  /// <summary>
  /// Activity states of <see cref="IAttribute"/>
  /// </summary>
  public enum ActivityState
  {
    /// <summary> Active and visible </summary>
    Active = 0,
    /// <summary> Disabled and invisible </summary>
    Disabled = 1,
    /// <summary> Active and visible but marked </summary>
    Obsolete = 2,
  }

  /// <summary>
  /// Data type in attribute
  /// </summary>
  public enum DataType
  {
    /// <summary> Numeric data type </summary>
    Numeric = 0,
    /// <summary> Text data type </summary>
    Text = 1,
  }
}