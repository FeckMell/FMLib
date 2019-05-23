using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FMLib
{
  /// <summary>
  /// Class that provides methods to perform automated actions. // TODO
  /// </summary>
  public static class PropertyAutomation
  {

    #region Fields

    /// <summary>
    /// List of assembly's names to call basic ToString method 
    /// </summary>
    public static List<string> NonToStringAssemblies { get; } = new List<string>() { "mscorlib" };

    /// <summary>
    /// List of namespaces to call basic ToString method 
    /// </summary>
    public static List<string> NonToStringNamespaces { get; } = new List<string>() { "System." };

    #endregion

    #region Public methods

    /// <summary>
    /// Constructs string representation of object
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static string ToString(object target)
    {
      if (target == null) { return "{NULL}"; }
      string result = "";
      try
      {
        if (!ToStringObject(target, ref result)) { ToStringProperties(target, ref result); }
      }
      catch { }
      return result;
    }

    /// <summary>
    /// Puts all property values to dictionary string:name - object:value
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static Dictionary<string, object> PropsToDict(object target)
    {
      var result = new Dictionary<string, object>();
      if (target == null) { return result; }
      try
      {
        var classProps = target.GetType().GetProperties();
        foreach (var e in classProps)
        {
          try { result[e.Name] = e.GetValue(target); }
          catch { }
        }
      }
      catch { }
      return result;
    }

    /// <summary>
    /// Fills properties of target from dictionary of property names and values
    /// </summary>
    /// <param name="target"></param>
    /// <param name="dict"></param>
    public static void DictToProps(object target, Dictionary<string, object> dict)
    {
      if (target == null) { return; }
      if (dict == null) { return; }
      try
      {
        var classProps = target.GetType().GetProperties();
        foreach (var e in dict)
        {
          var property = classProps.FirstOrDefault(x => x.Name.Equals(e.Key, StringComparison.OrdinalIgnoreCase));
          try { property?.SetValue(target, e.Value); }
          catch { }
        }
      }
      catch { }
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Calls ToString method of object
    /// </summary>
    /// <param name="target"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private static bool ToStringObject(object target, ref string result)
    {
      if (CustomToString(target, ref result)) { return true; }
      if (SystemToString(target, ref result)) { return true; }
      return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private static bool CustomToString(object target, ref string result)
    {
      if (target is string)
      {
        result += target as string;
        return true;
      }
      if (target is System.Collections.IDictionary)
      {
        var converted = target as System.Collections.IDictionary;
        var keys = converted.Keys;
        var strings = new List<string>();
        foreach (var e in keys) { strings.Add($"\"{ToString(e)}\":\"{ToString(converted[e])}\""); }
        result += "{" + string.Join(", ", strings) + "}";
        return true;
      }
      if (target is System.Collections.IEnumerable)
      {
        var converted = target as System.Collections.IEnumerable;
        var strings = new List<string>();
        foreach (var e in converted) { strings.Add(ToString(e)); }
        result += "{" + string.Join(", ", strings) + "}";
        return true;
      }
      return false;
    }

    /// <summary>
    /// Calls <see cref="object.ToString"/> on classes from "System" namespace
    /// </summary>
    /// <param name="target"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private static bool SystemToString(object target, ref string result)
    {
      // Check if file is in 
      string assemblyName = target.GetType().Assembly.GetName().Name;
      for (int i = 0; i < NonToStringAssemblies.Count; i++)
      {
        if (assemblyName.Equals(NonToStringAssemblies[i], StringComparison.OrdinalIgnoreCase))
        {
          result += target.ToString();
          return true;
        }
      }

      string namespaceName = target.GetType().Namespace;
      for (int i = 0; i < NonToStringAssemblies.Count; i++)
      {
        if (assemblyName.Equals(NonToStringAssemblies[i], StringComparison.OrdinalIgnoreCase))
        {
          result += target.ToString();
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Process property list of target and format them into string
    /// </summary>
    /// <param name="target"></param>
    /// <param name="result"></param>
    private static void ToStringProperties(object target, ref string result)
    {
      result += "{";
      try
      {
        var props = target.GetType().GetProperties();
        var strings = new List<string>();
        foreach (var e in props)
        {
          try { strings.Add($"\"{e.Name}\":\"{ToString(e.GetValue(target))}\""); }
          catch { }
        }
        result += string.Join(", ", strings);
      }
      catch { }
      result += "}";
    }

    #endregion
  }
}