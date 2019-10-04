using System.Windows;

namespace Utils
{
  /// <summary>
  /// Some methods that can be useful for WPF
  /// </summary>
  public static class WPFUseful
  {
    /// <summary>
    /// Checks if child element is fully displayed in parent
    /// </summary>
    public static bool IsUserVisible(this FrameworkElement element, FrameworkElement container)
    {
      Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
      Rect rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
      return rect.IntersectsWith(bounds); //rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight);
    }
  }
}