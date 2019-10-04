using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Utils.WPF.Converters
{
  /// <summary>
  /// Converter to allow binding parameters with multiplier on value
  /// usage: Width="{Binding Path=ActualWidth, ElementName=ParentGrid, Converter={converters:XAMLMultiplyConverter}, ConverterParameter='0.5'}"
  /// Width="{Binding Path=ActualWidth, RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type WrapPanel}}, Converter={converters:XAMLMultiplyConverter}, ConverterParameter='0.48'}"
  /// </summary>
  public class XAMLVisibilityConverter : MarkupExtension, IValueConverter
  {
    /// <summary>
    /// <see cref="MarkupExtension.ProvideValue(IServiceProvider)"/> implementation
    /// </summary>
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    /// <summary>
    /// <see cref="IValueConverter.Convert(object, Type, object, CultureInfo)"/> implementation
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is bool)) { value = value != null; }
      bool val = value == null ? false : (bool)value;
      val = "!".Equals(parameter) ? !val : val;
      return val ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// <see cref="IValueConverter.ConvertBack(object, Type, object, CultureInfo)"/> implementation
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      Utils.Logging.Tracer.Get(Utils.Logging.Tracer.UI).Error($"Called Convert back");
      return value;
    }
  }
}