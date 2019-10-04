using System.IO;
using Newtonsoft.Json;
using Utils.ExtensionMethods;

namespace Utils.Configuration
{
  /// <summary>
  /// Base class for configurations.
  /// </summary>
  public abstract class ConfigurationBase
  {
    /// <summary>
    /// <para/>Loads configuration from file
    /// <para/>Throws if filepath is inaccessible or data in file can't be deserialised.
    /// </summary>
    protected void LoadFromFile(string filepath)
    {
      JsonConvert.PopulateObject(File.ReadAllText(filepath), this);
    }

    /// <summary>
    /// <para/>Saves configuration to file
    /// <para/>Throws if file can't be written
    /// </summary>
    protected void SaveToFile(string filepath)
    {
      File.WriteAllText(filepath, this.AsString());
    }

    /// <summary>
    /// <para/>Populates instance from provided string
    /// <para/>Throws if provided string can't be deserialised
    /// </summary>
    protected void LoadFromString(string raw)
    {
      JsonConvert.PopulateObject(raw, this);
    }
  }
}