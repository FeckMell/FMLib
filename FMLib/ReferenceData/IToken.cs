namespace FMLib.ReferenceData
{
  /// <summary>
  /// Most low-level parent for all ReferenceData structure
  /// </summary>
  public interface IToken
  {
    /// <summary>
    /// Name of Token
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets name to be displayed to user depending on culture
    /// </summary>
    string GetDisplayName();
  }
}