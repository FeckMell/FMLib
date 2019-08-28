using System.Collections.Generic;
using System.Linq;

namespace Utils
{
  /// <summary>
  /// Class stores information about errors
  /// </summary>
  public class Error
  {

    #region Fields

    /// <summary> Used in <see cref="ToString()"/> to separate errors. </summary>
    public const string DELIMETER = "\n";
    /// <summary> Used in <see cref="ToString()"/> to indent levels. </summary>
    public const string INTEND = "\t";

    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Sub errors
    /// </summary>
    public List<Error> SubErrors { get; private set; } = new List<Error>();

    /// <summary>
    /// Is any data in error. If empty - there was no errors
    /// </summary>
    public bool IsEmpty { get; private set; } = true;

    #endregion

    #region Constructors and Destructors

    /// <summary>
    /// Constructor
    /// </summary>
    public Error(string data = null, Error error = null)
    {
      if (data.IsNullOrWhiteSpace()) { IsEmpty = true; return; }
      IsEmpty = false;
      Message = data;
      if (error == null) { return; }
      if (error.Message.IsNullOrWhiteSpace()) { SubErrors.AddRange(error.SubErrors); }
      else { SubErrors.Add(error); }
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Adds sub error to that instance
    /// </summary>
    public void Add(string data)
    {
      if (data.IsNullOrWhiteSpace()) { return; }
      SubErrors.Add(new Error(data));
      IsEmpty = false;
    }

    /// <summary>
    /// Adds sub error to that instance
    /// </summary>
    public void Add(string data, Error error)
    {
      if (data.IsNullOrWhiteSpace()) { data = "NoName"; }
      if (error == null) { error = new Error(); }
      if (error.Message.IsNullOrWhiteSpace())
      {
        error.Message = data;
        SubErrors.Add(error);
      }
      else
      {
        SubErrors.Add(new Error(data, error));
      }
      IsEmpty = false;
    }

    /// <summary>
    /// Adds sub error
    /// </summary>
    public void Add(Error error)
    {
      if (error == null) { return; }
      if (error.Message.IsNullOrWhiteSpace()) { error.SubErrors.ForEachA(x => Add(x)); }
      else { SubErrors.Add(error); }
    }

    /// <summary>
    /// ToString
    /// </summary>
    public override string ToString()
    {
      return ToString(DELIMETER, INTEND);
    }

    /// <summary>
    /// ToString
    /// </summary>
    public string ToString(string delimeter = DELIMETER, string intend = INTEND)
    {
      if (IsEmpty) { return string.Empty; }
      if (Message.IsNullOrWhiteSpace())
      {
        var t = SubErrors.Select(x => x.ToString(delimeter, intend, 0)).ToList();
        return SubErrors.Select(x => x.ToString(delimeter, intend, 0)).StrJoin(delimeter);
      }
      else
      {
        return Message + (SubErrors.Count == 0 ? string.Empty : $"{delimeter}{intend}" + SubErrors.Select(x => x.ToString(delimeter, intend, 2)).StrJoin($"{delimeter}{intend}"));
      }
    }

    /// <summary>
    /// ToString
    /// </summary>
    private string ToString(string delimeter, string intend, int depth)
    {
      if (IsEmpty) { return string.Empty; }
      string levelIntend = Useful.Intend(intend, depth);
      string result = Message;
      foreach (var error in SubErrors)
      {
        if (error.IsEmpty) { continue; }
        result += delimeter + levelIntend + error.ToString(delimeter, intend, depth + 1);
      }
      return result;
    }

    #endregion

  }
}