using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace FMLib.WPF.Converters
{
  /// <summary>
  /// Converter to allow binding parameters with addition to value
  /// usage: Width="{Binding Path=ActualWidth, ElementName=ParentGrid, Converter={converters:XAMLAdditionConverter}, ConverterParameter='-0.5'}"
  /// </summary>
  public class XAMLAdditionConverter : MarkupExtension, IValueConverter
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
      try { return System.Convert.ToDouble(value) + System.Convert.ToDouble(parameter); }
      catch (Exception ex) { Utils.Logging.Tracer.Get(Utils.Logging.Tracer.UI).Error(ex.FullInfo()); return value; }
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