using System;
using System.Windows;
using System.Windows.Media;

namespace Utils.WPF.Internal
{
  /// <summary>
  /// Class to store values for framework elements
  /// </summary>
  internal class WpfExtensionValues
  {
    /// <summary>
    /// FrameworkElement values are binded
    /// </summary>
    public FrameworkElement FrameworkElement { get; private set; } = null;

    /// <summary>
    /// Were values loaded by <see cref="WpfExtention.GetDefaultValues(FrameworkElement, KnowDependencyProperties, WpfExtensionValues)"/>
    /// </summary>
    public bool IsLoaded { get; set; } = false;

    /// <summary>
    /// Max font size. Got from FontSize property
    /// </summary>
    public double MaxFontSize { get; set; } = 12;

    /// <summary>
    /// Max font size. Got from WpfExtension.MinFontSize property
    /// </summary>
    public double MinFontSize { get; set; } = 12;

    /// <summary>
    /// Original background brush
    /// </summary>
    public Brush BackgroundBrush { get; set; } = null;

    /// <summary>
    /// Original background brush
    /// </summary>
    public Brush ForegroundBrush { get; set; } = null;

    /// <summary>
    /// Is background managed
    /// </summary>
    public bool ManagedBackground { get; set; } = false;

    /// <summary>
    /// Is foreground managed
    /// </summary>
    public bool ManagedForeground { get; set; } = false;

    /// <summary>
    /// Is TextWrapping dependency property is set to wrapping
    /// </summary>
    public bool IsTextWrapping { get; set; } = false;

    /// <summary>
    /// Is running line enabled
    /// </summary>
    public bool RunningLine { get; set; } = false;

    /// <summary>
    /// Constructor
    /// </summary>
    public WpfExtensionValues(FrameworkElement frameworkElement)
    {
      FrameworkElement = frameworkElement ?? throw new ArgumentNullException(nameof(frameworkElement));
    }
  }
}