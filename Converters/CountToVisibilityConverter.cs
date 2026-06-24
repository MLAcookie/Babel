using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Babel.Converters;

[ValueConversion(typeof(int), typeof(Visibility))]
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var count = value is int i ? i : 0;
        var invert = parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase);
        var visible = invert ? count == 0 : count > 0;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
