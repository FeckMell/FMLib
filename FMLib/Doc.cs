using System.Collections.Generic;

namespace Utils
{
  /// <summary>
  /// Class for providing RunTime documentation
  /// </summary>
  public class Doc
  {
    /// <summary>
    /// Name of documenting class/field/method
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// General description
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Default value
    /// </summary>
    public string Default { get; set; } = string.Empty;

    /// <summary>
    /// Description of entity of possible values
    /// </summary>
    public string PossibleValues { get; set; } = string.Empty;

    /// <summary>
    /// General examples of usage
    /// </summary>
    public List<string> Examples { get; set; } = new List<string>();

    /// <summary>
    /// Remarks for instance
    /// </summary>
    public string Remark { get; set; } = string.Empty;

    /// <summary>
    /// Is visible for editing
    /// </summary>
    public bool IsEditVisible { get; set; } = true;

    /// <summary>
    /// Is visible for displaying
    /// </summary>
    public bool IsViewVisible { get; set; } = true;

    /// <summary>
    /// Constructor
    /// </summary>
    public Doc(string name, string description)
    {
      Name = name ?? throw new ExpectedException($"Null argument {nameof(name)}");
      Description = description ?? throw new ExpectedException($"Null argument {nameof(description)}");
    }

    /// <summary>
    /// Gets structured content
    /// </summary>
    public Error Get()
    {
      Error result = new Error();
      if (Examples.Count == 1) { result.Add($"Example: {Examples[0]}"); }
      if (Examples.Count > 1)
      {
        Error example = new Error("Examples:");
        Examples.ForEachA(x => example.SubErrors.Add(new Error(x)));
        result.Add(example);
      }
      if (!Default.IsNullOrWhiteSpace()) { result.Add($"Default: {Default}"); }
      if (!PossibleValues.IsNullOrWhiteSpace()) { result.Add($"PossibleValues: {PossibleValues}"); }
      if (!Remark.IsNullOrWhiteSpace()) { result.Add($"Remarks: {Remark}"); }

      return new Error($"Documentation for {Name}: {Description}", result);
    }

    /// <summary>
    /// ToString
    /// </summary>
    public override string ToString()
    {
      return ToString(Error.DELIMETER, Error.INTEND);
    }

    /// <summary>
    /// ToString
    /// </summary>
    public string ToString(string delimeter = Error.DELIMETER, string intend = Error.INTEND)
    {
      return Get().ToString(delimeter, intend);
    }
  }
}