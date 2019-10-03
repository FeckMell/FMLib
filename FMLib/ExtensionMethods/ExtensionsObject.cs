using System;
using System.Collections.Generic;
using FMLib.Configuration;
using Newtonsoft.Json;

namespace FMLib.ExtensionMethods
{
  /// <summary>
  /// Extension methods for any type in project
  /// </summary>
  public static class ExtensionsObject
  {

    #region Fields

    private static readonly JsonSerializerSettings WithoutTypeSettings = new JsonSerializerSettings
    {
      NullValueHandling = NullValueHandling.Ignore,
      DateFormatHandling = DateFormatHandling.IsoDateFormat,
      DateFormatString = GlobalInformation.DateTimeFormat,
      DateParseHandling = DateParseHandling.DateTimeOffset,
      FloatFormatHandling = FloatFormatHandling.String,
      FloatParseHandling = FloatParseHandling.Double,
      Formatting = Formatting.None,
      MissingMemberHandling = MissingMemberHandling.Ignore,
      ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
      TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
    };

    private static readonly JsonSerializerSettings WithTypeSettings = new JsonSerializerSettings
    {
      Formatting = Formatting.None,
      TypeNameHandling = TypeNameHandling.Objects,
      NullValueHandling = NullValueHandling.Ignore,
      TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
      ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    };

    #endregion

    /// <summary>
    /// Checks if <paramref name="target"/> is null and if so - throws exception with <paramref name="errorMessage"/>
    /// </summary>
    public static void ThrowIfNull(this object target, string errorMessage = null)
    {
      if (target == null)
      {
        if (errorMessage == null)
          throw new Exception();
        else
          throw new Exception(errorMessage);
      }
    }

    /// <summary>
    /// Constructs string representation of object
    /// </summary>
    public static string AsString(this object target)
    {
      if (target == null) { return "{NULL}"; }
      return JsonConvert.SerializeObject(target, WithoutTypeSettings);
    }

    /// <summary>
    /// Checks that two objects are equal by checking their properties on default equality
    /// </summary>
    public static bool IsEqualTo(this object one, object two)
    {
      if (one == null && two == null) { return true; }
      if (one == null) { return false; }
      if (two == null) { return false; }
      if (one.GetType() != two.GetType()) { return false; }

      string oneStr = JsonConvert.SerializeObject(one, WithTypeSettings);
      string twoStr = JsonConvert.SerializeObject(two, WithTypeSettings);
      return string.Equals(oneStr, twoStr, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Clones object. No guarantee that it will be OK.
    /// </summary>
    public static T Clone<T>(this T target)
    {
      if (EqualityComparer<T>.Default.Equals(target, default(T))) { return default(T); }

      string temp = JsonConvert.SerializeObject(target, WithTypeSettings);
      return (T)JsonConvert.DeserializeObject(temp, WithTypeSettings);
    }

    /// <summary>
    /// Checks if <paramref name="target"/> is of <typeparamref name="T"/> type and casts <paramref name="target"/> to <paramref name="result"/> if true. 
    /// </summary>
    /// <typeparam name="T">Type to check</typeparam>
    /// <param name="target">Object to check</param>
    /// <param name="result">Casted value. If check is failed - returns default(T)</param>
    /// <returns>True if <paramref name="target"/> is of <typeparamref name="T"/> type</returns>
    public static bool IsOfType<T, R>(this R target, out T result) where T : class where R: class
    {
      if (target is T) { result = target as T; return true; }
      else { result = null; return false; }
    }
  }
}