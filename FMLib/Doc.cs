using System.Collections.Generic;
using Utils.Collections;
using Utils.ExtensionMethods;

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
    public DataTree<string> Get()
    {
      DataTree<string> result = new DataTree<string>($"Documentation for {Name}: {Description}");
      if (Examples.Count == 1) { result.Add($"Example: {Examples[0]}"); }
      if (Examples.Count > 1)
      {
        DataTree<string> example = new DataTree<string>("Examples:");
        Examples.ForEach(x => example.Add(x));
        result.Add(example);
      }
      if (!Default.IsNullOrWhiteSpace()) { result.Add($"Default: {Default}"); }
      if (!PossibleValues.IsNullOrWhiteSpace()) { result.Add($"PossibleValues: {PossibleValues}"); }
      if (!Remark.IsNullOrWhiteSpace()) { result.Add($"Remarks: {Remark}"); }

      return result;
    }

    /// <summary>
    /// ToString
    /// </summary>
    public override string ToString()
    {
      return ToString(DataTree<string>.DELIMETER, DataTree<string>.INTEND);
    }

    /// <summary>
    /// ToString
    /// </summary>
    public string ToString(string delimeter = DataTree<string>.DELIMETER, string intend = DataTree<string>.INTEND)
    {
      return Get().ToString(delimeter, intend);
    }
  }
}