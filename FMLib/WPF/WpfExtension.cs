using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Utils.WPF.Internal;

namespace Urils.WPF
{
  /// <summary>
  /// Class providing additional properties and behaviors to WPF controls
  /// </summary>
  public static class WpfExtention
  {
    /// <summary>
    /// Attached property to set managed Text with FontSize depending on control width/height and text length
    /// </summary>
    public static readonly DependencyProperty ManagedText = DependencyProperty.RegisterAttached(nameof(ManagedText), typeof(string), typeof(WpfExtention), new UIPropertyMetadata(string.Empty, (x, y) => InvokeOnUI(x, y, OnManagedTextChanged)));

    /// <summary>
    /// Attached property to mark control that it has managed Text FontSize
    /// </summary>
    public static readonly DependencyProperty MinFontSize = DependencyProperty.RegisterAttached(nameof(MinFontSize), typeof(double), typeof(WpfExtention), new UIPropertyMetadata(12D, (x, y) => InvokeOnUI(x, y, OnMinFontSizeChanged)));

    /// <summary>
    /// Attached property to mark control that it can use running line
    /// </summary>
    public static readonly DependencyProperty RunningLine = DependencyProperty.RegisterAttached(nameof(RunningLine), typeof(bool), typeof(WpfExtention), new UIPropertyMetadata(false, (x, y) => InvokeOnUI(x, y, OnRunningLineChanged)));

    /// <summary>
    /// Attached property to mark control that it has managed brightness of Foreground
    /// </summary>
    public static readonly DependencyProperty ManagedForeground = DependencyProperty.RegisterAttached(nameof(ManagedForeground), typeof(bool), typeof(WpfExtention), new UIPropertyMetadata(false, (x, y) => InvokeOnUI(x, y, OnManagedForegroundChanged)));

    /// <summary>
    /// Attached property to mark control that it has managed brightness of Background
    /// </summary>
    public static readonly DependencyProperty ManagedBackground = DependencyProperty.RegisterAttached(nameof(ManagedBackground), typeof(bool), typeof(WpfExtention), new UIPropertyMetadata(false, (x, y) => InvokeOnUI(x, y, OnManagedBackgroundChanged)));

    #region Text controller

    #region RunningLine

    /// <summary>
    /// Store for all dependent elements for running line. It is selection from <see cref="s_managedElements"/>.
    /// </summary>
    private static readonly ConcurrentDictionary<FrameworkElement, (KnowDependencyProperties props, WpfExtensionValues value)> s_runningLineElements = new ConcurrentDictionary<FrameworkElement, (KnowDependencyProperties props, WpfExtensionValues value)>();

    /// <summary>
    /// Getter for RunningLine
    /// </summary>
    public static bool GetRunningLine(this DependencyObject dependencyObject) => (bool)dependencyObject.GetValue(RunningLine);

    /// <summary>
    /// Setter for RunningLine
    /// </summary>
    public static void SetRunningLine(this DependencyObject dependencyObject, bool value) => dependencyObject.SetValue(RunningLine, value);

    /// <summary>
    /// Main method that is called to add element to watched collection.
    /// </summary>
    private static void OnRunningLineChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      try
      {
        if (!(e.NewValue is bool)) // if not bool - can't proceed
          return;

        if (!UpdateOrAddFrameworkElement(dependencyObject, out var properties, out var values, out var frameworkElement)) // if failed - can't proceed
          return;

        if (properties.Text == null) // if text property is null - no where to apply running line
          return;

        values.RunningLine = (bool)(e.NewValue ?? false); // update value
        if (!values.RunningLine) // if false - remove from watched collection of RunningLine
        {
          FrameworkElement_Unloaded_RunningLine(frameworkElement, null);
          return;
        }

        if (s_runningLineElements.ContainsKey(frameworkElement)) // if this element is already in managed collection - do nothing
          return;

        frameworkElement.Unloaded += FrameworkElement_Unloaded_RunningLine; // bind to event of clearing
        s_runningLineElements.AddOrUpdate(frameworkElement, (properties, values), (x, y) => (properties, values)); // add element to managed collection of RunningLine
      }
      catch { }
    }

    /// <summary>
    /// Handler for clearing data from <see cref="s_runningLineElements"/> on unload
    /// </summary>
    private static void FrameworkElement_Unloaded_RunningLine(object sender, RoutedEventArgs e)
    {
      try
      {
        if (!(sender is FrameworkElement frameworkElement)) // check that sender object is of correct type
          return;

        frameworkElement.Unloaded -= FrameworkElement_Unloaded_RunningLine; // unbind from that event
        s_runningLineElements.TryRemove(frameworkElement, out var dummy); // Remove instance from mapping
      }
      catch { }
    }

    /// <summary>
    /// Method that is invoked on Timer ticks to update information of RunningLine
    /// </summary>
    private static void RunningLineTick()
    {
      foreach (var frameworkElement in s_runningLineElements.Keys)
      {
        var (props, value) = s_runningLineElements[frameworkElement];
        if (!value.IsLoaded) // if element not loaded - nothing to update
          continue;
        if (!value.RunningLine)// if not running line - nothing to update
          continue;

        (double visualWidth, double visualHeight) = (frameworkElement.ActualWidth, frameworkElement.ActualHeight); // get initial width and height of control
        if (frameworkElement.IsTextFitting(value.IsTextWrapping, visualWidth, visualHeight)) // if it is fitting - nothing to update
        {
          frameworkElement.Arrange(new Rect(frameworkElement.RelativePosition(), new Size(visualWidth, visualHeight))); // second step of update size of a layout update.
          continue;
        }

        var text = RunningLineComputeString(frameworkElement.GetValue(props.Text) as string); // form new string
        try { frameworkElement.SetValue(props.Text, text); }
        catch { }
      }
    }

    /// <summary>
    /// Modifies string so first character goes to last position
    /// </summary>
    private static string RunningLineComputeString(string str)
    {
      if (string.IsNullOrWhiteSpace(str))
        return str;
      return str.Substring(1, str.Length - 1) + str.Substring(0, 1);
    }

    #endregion

    #region MinFontSize

    /// <summary>
    /// Getter for MinFontSize
    /// </summary>
    public static double GetMinFontSize(this DependencyObject dependencyObject) => (double)dependencyObject.GetValue(MinFontSize);

    /// <summary>
    /// Setter for MinFontSize
    /// </summary>
    public static void SetMinFontSize(this DependencyObject dependencyObject, double value) => dependencyObject.SetValue(MinFontSize, value);

    /// <summary>
    /// Main method that is called to add element to watched collection.
    /// Also sets FontSize to control
    /// </summary>
    private static void OnMinFontSizeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      try
      {
        if (!(e.NewValue is double)) // if not double - can't proceed
          return;

        if (!UpdateOrAddFrameworkElement(dependencyObject, out var properties, out var values, out var frameworkElement)) // if failed - can't proceed
          return;

        values.MinFontSize = (double)e.NewValue;

        if (!values.IsLoaded) // if values were not loaded - no text to resize
          return;

        ResizeText(frameworkElement, properties, values); // call main method to resize text
      }
      catch { }
    }

    #endregion

    #region Managed text

    /// <summary>
    /// Getter for ManagedText
    /// </summary>
    public static string GetManagedText(this DependencyObject dependencyObject) => (string)dependencyObject.GetValue(ManagedText);

    /// <summary>
    /// Setter for ManagedText
    /// </summary>
    public static void SetManagedText(this DependencyObject dependencyObject, string value) => dependencyObject.SetValue(ManagedText, value);

    /// <summary>
    /// Main method that is called to add element to watched collection.
    /// Also sets Text or Content to control
    /// </summary>
    private static void OnManagedTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      try
      {
        if (!(e.NewValue is string textValue)) // if not string - can't proceed
          return;

        if (!UpdateOrAddFrameworkElement(dependencyObject, out var properties, out var values, out var frameworkElement)) // if failed - can't proceed
          return;

        if (properties.FontSize == null || properties.Text == null) // if any of dependency properties is null - can't proceed
          return;

        frameworkElement.SetValue(properties.Text, textValue); // sets value for underlying property

        if (!frameworkElement.IsLoaded) // if control is not loaded all it's sizes (width, height fontSize) have initial values.
        {
          frameworkElement.Loaded += FrameworkElement_Loaded_ManagedText; // wait until loaded. When loaded - will finalize initialisation
          return;
        }

        if (!values.IsLoaded) // if values were not loaded - can't proceed
          return;

        ResizeText(frameworkElement, properties, values); // call main method to resize text
      }
      catch { }
    }

    /// <summary>
    /// Handler to obtain control properties when loaded
    /// </summary>
    private static void FrameworkElement_Loaded_ManagedText(object sender, RoutedEventArgs e)
    {
      try
      {
        if (!(sender is FrameworkElement frameworkElement)) // check that sender object is of correct type
          return;

        frameworkElement.Loaded -= FrameworkElement_Loaded_ManagedText; // unbind from that event
        if (!s_managedElements.TryGetValue(frameworkElement, out var values)) // if can't get element - it is not managed or unsupported type
          return;

        if (!values.value.IsLoaded) // if values not loaded - call default loaded handler to fill instance
          FrameworkElement_Loaded_Default(sender, e);

        if (!values.value.IsLoaded) // if still are not loaded - can't do anything
          return;

        ResizeText(frameworkElement, values.props, values.value);
      }
      catch { }
    }

    /// <summary>
    /// Resizes text
    /// </summary>
    private static void ResizeText(FrameworkElement frameworkElement, KnowDependencyProperties properties, WpfExtensionValues values)
    {
      double minFontSize = values.MinFontSize;
      double maxFontSize = values.MaxFontSize;

      (double visualWidth, double visualHeight) = (frameworkElement.ActualWidth, frameworkElement.ActualHeight); // get initial width and height of control
      for (double i = maxFontSize; i >= minFontSize; i--) // loop through font sizes from max to min to find best
      {
        frameworkElement.SetValue(properties.FontSize, i); // set new font size
        if (frameworkElement.IsTextFitting(values.IsTextWrapping, visualWidth, visualHeight))
          break;
      }
    }

    #endregion

    /// <summary>
    /// Check if text is fitting in control
    /// </summary>
    private static bool IsTextFitting(this FrameworkElement frameworkElement, bool isTextWrapping, double visualWidth, double visualHeight)
    {
      double width = isTextWrapping ? visualWidth : double.PositiveInfinity; // fix for wrap for TextBlock
      var point = frameworkElement.RelativePosition(); // get position in parent container (fix for Label)
      frameworkElement.Measure(new Size(width, double.PositiveInfinity)); // first step of update size
      frameworkElement.Arrange(new Rect(point, frameworkElement.DesiredSize)); // second step of update size of a layout update.

      var (actualWidth, actualHeight) = (frameworkElement.ActualWidth, frameworkElement.ActualHeight); // height and width will tell as if control is bigger or smaller than initial
      return actualWidth <= visualWidth && actualHeight <= visualHeight;
    }

    /// <summary>
    /// Gets position of element in parent
    /// </summary>
    private static Point RelativePosition(this FrameworkElement frameworkElement)
    {
      var positionTransform = frameworkElement.TransformToAncestor(frameworkElement.Parent as UIElement);
      return positionTransform.Transform(new Point(0, 0));
    }

    #endregion

    #region Color controller

    #region Foreground

    /// <summary>
    /// Getter for foreground
    /// </summary>
    public static bool GetManagedForeground(this DependencyObject dependencyObject) => (bool)(dependencyObject.GetValue(ManagedForeground) ?? false);

    /// <summary>
    /// Setter for foreground
    /// </summary>
    public static void SetManagedForeground(this DependencyObject dependencyObject, bool value) => dependencyObject.SetValue(ManagedForeground, value);

    /// <summary>
    /// Main method that is called to add element to watched collection to apply brightness changes.
    /// </summary>
    private static void OnManagedForegroundChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      try
      {
        if (!(e.NewValue is bool)) // if not bool - can't proceed
          return;

        if (!UpdateOrAddFrameworkElement(dependencyObject, out var properties, out var values, out var frameworkElement)) // if failed - can't proceed
          return;

        if (properties.Foreground == null) // if dependency property is null - can't proceed
          return;

        values.ManagedForeground = (bool)(e.NewValue ?? false);
        // values.Foreground should be set OnLoaded
      }
      catch { }
    }

    #endregion

    #region Background

    /// <summary>
    /// Getter for background
    /// </summary>
    public static bool GetManagedBackground(this DependencyObject dependencyObject) => (bool)(dependencyObject.GetValue(ManagedBackground) ?? false);

    /// <summary>
    /// Setter for background
    /// </summary>
    public static void SetManagedBackground(this DependencyObject dependencyObject, bool value) => dependencyObject.SetValue(ManagedBackground, value);

    /// <summary>
    /// Main method that is called to add element to watched collection to apply brightness changes.
    /// Also sets Background to control
    /// </summary>
    private static void OnManagedBackgroundChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      try
      {
        if (!(e.NewValue is bool)) // if not bool - can't proceed
          return;

        if (!UpdateOrAddFrameworkElement(dependencyObject, out var properties, out var values, out var frameworkElement)) // if failed - can't proceed
          return;

        if (properties.Background == null) // if Dependency property is null - can't proceed
          return;

        values.ManagedBackground = (bool)(e.NewValue ?? false);
        // values.Background should be set OnLoaded
      }
      catch { }
    }

    #endregion

    /// <summary>
    /// Handler for brightness changes. Changes color for all watched controls
    /// </summary>
    public static void BrightnessChanged_Handler(double newVal)
    {
      var managedBackgrounds = s_managedElements.Where(x => x.Value.value.ManagedBackground).Select(x => x.Value).ToList();
      var managedForegrounds = s_managedElements.Where(x => x.Value.value.ManagedForeground).Select(x => x.Value).ToList();

      foreach (var (props, value) in managedBackgrounds)
      {
        try
        {
          var brush = value.BackgroundBrush ?? Brushes.White;
          if (brush is SolidColorBrush solidBrush)
            value.FrameworkElement.SetValue(props.Background, new SolidColorBrush(AdjustBrightness(solidBrush.Color, newVal)));
        }
        catch { }
      }

      foreach (var (props, value) in managedForegrounds)
      {
        try
        {
          var brush = value.ForegroundBrush ?? Brushes.Black;
          if (brush is SolidColorBrush solidBrush)
            value.FrameworkElement.SetValue(props.Foreground, new SolidColorBrush(AdjustForegroundBrightness(solidBrush.Color, newVal)));
        }
        catch { }
      }
    }

    /// <summary>
    /// Method to apply brightness change to color
    /// </summary>
    private static Color AdjustForegroundBrightness(Color originalColour, double brightnessFactor)
    {
      if (brightnessFactor < -130)
      {
        double gray = 255 + brightnessFactor + 70;
        byte convert(double x) => (gray + x * 0.5).ToByte();
        return Color.FromArgb(originalColour.A, convert(originalColour.R), convert(originalColour.G), convert(originalColour.B));
      }
      return AdjustBrightness(originalColour, -brightnessFactor);
    }

    /// <summary>
    /// Method to apply brightness change to color
    /// </summary>
    private static Color AdjustBrightness(Color originalColour, double brightnessFactor)
    {
      brightnessFactor = 1 + brightnessFactor / 255;
      (double red, double green, double blue) = (originalColour.R, originalColour.G, originalColour.B);
      byte convert(double color) => (color * brightnessFactor).ToByte();
      return Color.FromArgb(originalColour.A, convert(red), convert(green), convert(blue));
    }

    /// <summary>
    /// Converts double to valid byte
    /// </summary>
    private static byte ToByte(this double value) => (byte)Math.Max(Math.Min(value, 255), 0);

    #endregion

    /// <summary>
    /// Static constructor
    /// </summary>
    static WpfExtention()
    {
      s_timer = new DispatcherTimer(TimeSpan.FromMilliseconds(200), DispatcherPriority.Normal, new EventHandler((s, e) => RunningLineTick()), Application.Current.Dispatcher);
      s_timer.Start();
    }

    /// <summary>
    /// Try to update or add provided element. Gets required properties and values
    /// </summary>
    private static bool UpdateOrAddFrameworkElement(DependencyObject dependencyObject, out KnowDependencyProperties properties, out WpfExtensionValues values, out FrameworkElement frameworkElement)
    {
      properties = null;
      values = null;
      frameworkElement = null;

      try
      {
        if (!(dependencyObject is FrameworkElement)) // check that object is valid for our logic
          return false;

        frameworkElement = dependencyObject as FrameworkElement; // cast

        var props = GetProperties(frameworkElement);
        if (!GetProperties(frameworkElement, out properties)) // get dependency properties from dictionary by final type of frameworkElement or obtain it by reflection
          return false;

        if (!GetValues(frameworkElement, out values)) // if couldn't get value - can't proceed
          return false;

        return true;
      }
      catch { return false; }
    }

    #region Helpers

    /// <summary>
    /// Timer for updating UI
    /// </summary>
    private static readonly DispatcherTimer s_timer;

    /// <summary>
    /// Store for all dependent elements. TODO: manage overflow (max elements count = int.MaxValue)
    /// </summary>
    private static readonly ConcurrentDictionary<FrameworkElement, (KnowDependencyProperties props, WpfExtensionValues value)> s_managedElements = new ConcurrentDictionary<FrameworkElement, (KnowDependencyProperties props, WpfExtensionValues value)>();

    /// <summary>
    /// Mapping of dependence properties
    /// </summary>
    private static readonly ConcurrentDictionary<Type, KnowDependencyProperties> s_mappingTypes = new ConcurrentDictionary<Type, KnowDependencyProperties>();

    /// <summary>
    /// Invokes on UI thread
    /// </summary>
    private static void InvokeOnUI(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e, Action<DependencyObject, DependencyPropertyChangedEventArgs> action)
    {
      if (dependencyObject == null) // not to cause any problems - just return if null
        return;

      if (!dependencyObject.Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        dependencyObject.Dispatcher.Invoke(() => InvokeOnUI(dependencyObject, e, action));
        return;
      }
      action?.Invoke(dependencyObject, e);
    }

    /// <summary>
    /// When control is loaded we can obtain size, remember it and call initial <see cref="ResizeText(FrameworkElement, DependencyProperty, double)"/> method.
    /// </summary>
    private static void FrameworkElement_Loaded_Default(object sender, RoutedEventArgs e)
    {
      try
      {
        if (!(sender is FrameworkElement frameworkElement)) // check that sender object is of correct type
          return;

        frameworkElement.Loaded -= FrameworkElement_Loaded_Default; // unbind from that event
        if (!s_managedElements.TryGetValue(frameworkElement, out var value)) // if can't get element - it is not managed or unsupported type
          return;

        GetDefaultValues(frameworkElement, value.props, value.value); // get values from control
      }
      catch { }
    }

    /// <summary>
    /// Map cleaner
    /// </summary>
    private static void FrameworkElement_Unloaded(object sender, RoutedEventArgs e)
    {
      try
      {
        if (!(sender is FrameworkElement frameworkElement)) // check that sender object is of correct type
          return;

        s_managedElements.TryRemove(frameworkElement, out var dummy); // Remove instance from mapping
        frameworkElement.Unloaded -= FrameworkElement_Unloaded; // unbind from that event
      }
      catch { }
    }

    /// <summary>
    /// Get values for given instance
    /// </summary>
    private static WpfExtensionValues GetValues(FrameworkElement frameworkElement)
    {
      if (s_managedElements.TryGetValue(frameworkElement, out var values))
        return values.value;

      if (!GetProperties(frameworkElement, out var properties))
        return null;

      var value = new WpfExtensionValues(frameworkElement);
      if (!frameworkElement.IsLoaded)
        frameworkElement.Loaded += FrameworkElement_Loaded_Default;
      else
        GetDefaultValues(frameworkElement, properties, value);
      frameworkElement.Unloaded += FrameworkElement_Unloaded;
      return s_managedElements.AddOrUpdate(frameworkElement, (properties, value), (x, y) => (properties, value)).value;
    }

    /// <summary>
    /// Gets values set for instance
    /// </summary>
    private static void GetDefaultValues(FrameworkElement frameworkElement, KnowDependencyProperties properties, WpfExtensionValues value)
    {
      if (value.IsLoaded) // if was loaded - doesn't want to reload
        return;

      if (properties.Background != null)
        value.BackgroundBrush = frameworkElement.GetValue(properties.Background) as Brush;
      if (properties.Foreground != null)
        value.ForegroundBrush = frameworkElement.GetValue(properties.Foreground) as Brush;
      if (properties.FontSize != null)
        value.MaxFontSize = (double)frameworkElement.GetValue(properties.FontSize);
      if (frameworkElement.FindDependencyProperty("TextWrapping", out var wrapping))
        value.IsTextWrapping = (TextWrapping)frameworkElement.GetValue(wrapping) != TextWrapping.NoWrap;

      value.IsLoaded = true;
    }

    /// <summary>
    /// Get know dependency properties for given instance
    /// </summary>
    private static KnowDependencyProperties GetProperties(FrameworkElement frameworkElement)
    {
      Type type = frameworkElement.GetType();
      if (s_mappingTypes.TryGetValue(type, out var props))
        return props;

      props = new KnowDependencyProperties(type);
      return s_mappingTypes.AddOrUpdate(type, props, (x, y) => props);
    }

    /// <summary>
    /// Checks for dependency property
    /// </summary>
    public static DependencyProperty FindDependencyProperty(this DependencyObject target, string propertyName) => KnowDependencyProperties.GetBaseProperty(target.GetType(), propertyName);

    /// <summary>
    /// Checks for dependency property
    /// </summary>
    public static bool FindDependencyProperty(this DependencyObject target, string propertyName, out DependencyProperty property) => (property = FindDependencyProperty(target, propertyName)) != null;

    /// <summary>
    /// Get know dependency properties for given instance
    /// </summary>
    private static bool GetProperties(FrameworkElement frameworkElement, out KnowDependencyProperties properties) => (properties = GetProperties(frameworkElement)) != null;

    /// <summary>
    /// Get values for given instance
    /// </summary>
    private static bool GetValues(FrameworkElement frameworkElement, out WpfExtensionValues value) => (value = GetValues(frameworkElement)) != null;

    #endregion

  }
}