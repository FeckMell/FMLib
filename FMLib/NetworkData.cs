using System.Collections.Generic;

namespace Utils
{
  /// <summary>
  /// Represents structure of data to be sent
  /// </summary>
  public class NetworkData
  {
    /// <summary>
    /// Name of method to call
    /// </summary>
    public string MethodName { get; set; } = string.Empty;

    /// <summary>
    /// Data to be sent
    /// </summary>
    public List<object> Data { get; set; } = new List<object>();

    /// <summary>
    /// Constructor
    /// </summary>
    public NetworkData(string name, List<object> data)
    {
      MethodName = name;
      Data = data;
    }

    /// <summary>
    /// To string
    /// </summary>
    public override string ToString()
    {
      return Automation.ToString(this);
    }
  }
}