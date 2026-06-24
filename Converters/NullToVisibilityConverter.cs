using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Babel.Converters;

[ValueConversion(typeof(object), typeof(Visibility))]
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var invert = parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase);
        var isNull = value is null;
        var visible = invert ? !isNull : isNull;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
