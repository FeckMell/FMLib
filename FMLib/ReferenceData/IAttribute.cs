namespace FMLib.ReferenceData
{
  /// <summary>
  /// Attribute class
  /// </summary>
  public interface IAttribute : IToken
  {
    /// <summary>
    /// Type of data in that attribute
    /// </summary>
    DataType DataType { get; set; }

    /// <summary>
    /// Value of attribute
    /// </summary>
    string Value { get; set; }
  }
}