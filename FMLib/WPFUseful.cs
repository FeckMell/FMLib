using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FMLib
{
  public static class WPFUseful
  {

    /// <summary>
    /// Checks if child element is fully displayed in parent
    /// </summary>
    public static bool IsUserVisible(FrameworkElement element, FrameworkElement container)
    {
      Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
      Rect rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
      return rect.IntersectsWith(bounds); //rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight);
    }
  }
}