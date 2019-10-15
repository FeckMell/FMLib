using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Utils.WPF
{
  /// <summary>
  /// Class to provide extended background and foreground methods
  ///
  /// Usage of Background in XAML: wpfExt:ColorController.Background="LightPink"
  /// Usage of Foreground in XAML: wpfExt:ColorController.Foreground="White"
  /// xmlns:wpfExt="clr-namespace:Utils.WPF;assembly=FMLib"
  public static class ColorController
  {
    /// <summary>
    /// Handler for brightness changes. Changes color for all watched controls
    /// </summary>
    public static void BrightnessChanged_Handler(double newVal)
    {
      foreach (var e in s_backgroundControls)
      {
        var brush = e.Value ?? Brushes.White;
        if (brush is SolidColorBrush solidBrush)
        {
          e.Key.Background = new SolidColorBrush(AdjustBrightness(solidBrush.Color, newVal));
        }
      }

      foreach (var e in s_foregroundControls)
      {
        var brush = e.Value ?? Brushes.Black;
        if (brush is SolidColorBrush solidBrush)
        {
          e.Key.Foreground = new SolidColorBrush(AdjustForegroundBrightness(solidBrush.Color, newVal));
        }
      }
    }

    #region Foreground

    /// <summary>
    /// Store for all dependent elements. TODO: manage overflow (max elements count = int.MaxValue)
    /// </summary>
    private static readonly ConcurrentDictionary<Control, Brush> s_foregroundControls = new ConcurrentDictionary<Control, Brush>();

    /// <summary>
    /// Getter for background
    /// </summary>
    public static string GetForeground(DependencyObject dependencyObject) => (string)dependencyObject.GetValue(Foreground);

    /// <summary>
    /// Setter for background
    /// </summary>
    public static void SetForeground(DependencyObject dependencyObject, string value) => dependencyObject.SetValue(Foreground, value);

    /// <summary>
    /// attached property Background
    /// </summary>
    public static readonly DependencyProperty Foreground = DependencyProperty.RegisterAttached(nameof(Foreground), typeof(string), typeof(ColorController), new UIPropertyMetadata(null, OnForegroundChanged));

    /// <summary>
    /// Main method that is called to add element to watched collection to apply brightness changes.
    /// Also sets Foreground to control
    /// </summary>
    private static void OnForegroundChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (!(dependencyObject is Control control)) return;
      if (!(e.NewValue is string colorText)) return;

      UpdateOrAddElement(control, colorText, s_foregroundControls, (x) => control.Foreground = x);
    }

    /// <summary>
    /// Method to apply brightness change to color
    /// </summary>
    private static Color AdjustForegroundBrightness(Color originalColour, double brightnessFactor)
    {
      if (brightnessFactor < -130)
      {
        double gray = 255 + brightnessFactor + 70;
        Func<double, byte> convert = (x) => (gray + x * 0.5).ToByte();
        return Color.FromArgb(originalColour.A, convert(originalColour.R), convert(originalColour.G), convert(originalColour.B));
      }
      return AdjustBrightness(originalColour, -brightnessFactor);
    }

    #endregion

    #region Background

    /// <summary>
    /// Store for all dependent elements. TODO: manage overflow (max elements count = int.MaxValue)
    /// </summary>
    private static readonly ConcurrentDictionary<Control, Brush> s_backgroundControls = new ConcurrentDictionary<Control, Brush>();

    /// <summary>
    /// Getter for background
    /// </summary>
    public static string GetBackground(DependencyObject dependencyObject) => (string)dependencyObject.GetValue(Background);

    /// <summary>
    /// Setter for background
    /// </summary>
    public static void SetBackground(DependencyObject dependencyObject, string value) => dependencyObject.SetValue(Background, value);

    /// <summary>
    /// attached property Background
    /// </summary>
    public static readonly DependencyProperty Background = DependencyProperty.RegisterAttached(nameof(Background), typeof(string), typeof(ColorController), new UIPropertyMetadata(null, OnBackgroundChanged));

    /// <summary>
    /// Main method that is called to add element to watched collection to apply brightness changes.
    /// Also sets Background to control
    /// </summary>
    private static void OnBackgroundChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (!(dependencyObject is Control control)) return;
      if (!(e.NewValue is string colorText)) return;

      UpdateOrAddElement(control, colorText, s_backgroundControls, (x) => control.Background = x);
    }

    #endregion

    /// <summary>
    /// Common logic used to update or add new element to watched collection
    /// </summary>
    /// <param name="control">Watched control</param>
    /// <param name="colorText">Color represented as text</param>
    /// <param name="dict">Collection to update with provided element</param>
    /// <param name="xgroudSetter">Setter for real property</param>
    private static void UpdateOrAddElement(Control control, string colorText, ConcurrentDictionary<Control, Brush> dict, Action<Brush> xgroudSetter)
    {
      try
      {
        var brush = StringToSolidColorBrush(colorText);
        xgroudSetter?.Invoke(brush);

        if (dict.ContainsKey(control))
        {
          dict[control] = brush;
        }
        else
        {
          dict.TryAdd(control, brush);
          control.Unloaded += Control_Unloaded;
        }
      }
      catch { }
    }

    /// <summary>
    /// Called on control removal from VisualTree to forget about it (remove from watched collection)
    /// </summary>
    private static void Control_Unloaded(object sender, RoutedEventArgs e)
    {
      if (!(sender is Control control)) return;
      control.Unloaded -= Control_Unloaded;
      s_backgroundControls.TryRemove(control, out var dummy1);
      s_foregroundControls.TryRemove(control, out var dummy2);
    }

    /// <summary>
    /// Method to apply brightness change to color
    /// </summary>
    private static Color AdjustBrightness(Color originalColour, double brightnessFactor)
    {
      brightnessFactor = 1 + brightnessFactor / 255;
      (double red, double green, double blue) = (originalColour.R, originalColour.G, originalColour.B);
      Func<double, byte> convert = (color) => (color * brightnessFactor).ToByte();
      return Color.FromArgb(originalColour.A, convert(red), convert(green), convert(blue));
    }

    /// <summary>
    /// Color brightness
    /// </summary>
    private static bool PerceivedBrightness(Color color)
    {
      return Math.Sqrt(color.R * color.R * 0.299 + color.G * color.G * 0.587 + color.B * color.B * 0.114) > 130;
    }

    /// <summary>
    /// Converts text to <see cref="SolidColorBrush"/>. If fails - return null
    /// </summary>
    private static SolidColorBrush StringToSolidColorBrush(string colorText)
    {
      try { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorText)); }
      catch { return null; }
    }

    /// <summary>
    /// Converts double to valid byte
    /// </summary>
    private static byte ToByte(this double value) => (byte)Math.Max(Math.Min(value, 255), 0);
  }
}