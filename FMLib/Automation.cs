using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Utils
{
  /// <summary>
  /// Provides various methods for automated work with objects
  /// </summary>
  public static class Automation
  {

    #region Fields

    private static readonly JsonSerializerSettings WithoutTypeSettings = new JsonSerializerSettings
    {
      NullValueHandling = NullValueHandling.Ignore,
      DateFormatHandling = DateFormatHandling.IsoDateFormat,
      DateFormatString = Constants.DATE_TIME_FORMAT,
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
    };

    #endregion

    #region Public methods

    /// <summary>
    /// Constructs string representation of object
    /// </summary>
    public static string ToString(object target)
    {
      if (target == null) { return "{NULL}"; }

      try { return JsonConvert.SerializeObject(target, WithoutTypeSettings); }
      catch (Exception ex) { Tracer._SystemError(ex.ToString()); return ""; }
    }

    /// <summary>
    /// Checks that two objects are equal by checking their properties on default equality
    /// </summary>
    public static bool AreEqual(object one, object two)
    {
      if (one == null && two == null) { return true; }
      if (one == null) { return false; }
      if (two == null) { return false; }
      if (one.GetType() != two.GetType()) { return false; }

      try
      {
        string oneStr = JsonConvert.SerializeObject(one, WithTypeSettings);
        string twoStr = JsonConvert.SerializeObject(two, WithTypeSettings);
        return string.Equals(oneStr, twoStr, StringComparison.OrdinalIgnoreCase);
      }
      catch (Exception ex) { Tracer._SystemError(ex.ToString()); return false; }
    }

    /// <summary>
    /// Clones object. No guarantee that it will be OK.
    /// </summary>
    public static T Clone<T>(T target)
    {
      if (EqualityComparer<T>.Default.Equals(target, default(T))) { return default(T); }

      try
      {
        string temp = JsonConvert.SerializeObject(target, WithTypeSettings);
        return (T)JsonConvert.DeserializeObject(temp, WithTypeSettings);
      }
      catch (Exception ex) { Tracer._SystemError(ex.ToString()); return default(T); }
    }

    /// <summary>
    /// Fills properties of target from dictionary of property names and values
    /// </summary>
    public static void DictToProps(object target, Dictionary<string, object> dict)
    {
      if (target == null) { return; }
      if (dict == null) { return; }
      try
      {
        var classProps = target.GetType().GetProperties();
        foreach (var e in dict)
        {
          var property = classProps.FirstOrDefault(x => x.Name.EQUAL(e.Key));
          try { property?.SetValue(target, e.Value); }
          catch (Exception ex) { Tracer._SystemError(ex.ToString()); }
        }
      }
      catch (Exception ex) { Tracer._SystemError(ex.ToString()); }
    }

    #endregion

  }
}