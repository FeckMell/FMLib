using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Utils.WPF.Internal
{
  /// <summary>
  /// Class that held information about dependency properties for specified type
  /// </summary>
  internal class KnowDependencyProperties
  {
    /// <summary>
    /// Type of instance which has got that properties
    /// </summary>
    public Type Type { get; private set; }

    /// <summary>
    /// Background dependency property
    /// </summary>
    public DependencyProperty Background { get; set; }

    /// <summary>
    /// Foreground dependency property
    /// </summary>
    public DependencyProperty Foreground { get; set; }

    /// <summary>
    /// Text(Content) dependency property
    /// </summary>
    public DependencyProperty Text { get; set; }

    /// <summary>
    /// FontSize dependency property
    /// </summary>
    public DependencyProperty FontSize { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public KnowDependencyProperties(Type type)
    {
      Type = type;

      Background = GetBaseProperty(nameof(Background));
      Foreground = GetBaseProperty(nameof(Foreground));
      FontSize = GetBaseProperty(nameof(FontSize));

      if (type == typeof(TextBox) || type == typeof(TextBlock))
        Text = GetBaseProperty(nameof(Text));
      else if (type == typeof(Label))
        Text = GetBaseProperty("Content");
    }

    /// <summary>
    /// Gets property by name
    /// </summary>
    /// <param name="propertyName">Name of property without "Property" postfix</param>
    public DependencyProperty GetBaseProperty(string propertyName)
    {
      return GetBaseProperty(Type, propertyName);
    }

    /// <summary>
    /// Gets property by name for given type
    /// </summary>
    /// <param name="propertyName">Name of property without "Property" postfix</param>
    public static DependencyProperty GetBaseProperty(Type type, string propertyName)
    {
      return (DependencyProperty)type.GetField($"{propertyName}Property", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public)?.GetValue(null);
    }
  }
}