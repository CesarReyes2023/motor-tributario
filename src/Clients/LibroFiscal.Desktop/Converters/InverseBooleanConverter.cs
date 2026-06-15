using System;
using System.Globalization;
using System.Windows.Data;

namespace LibroFiscal.Desktop.Converters;

public class InverseBooleanConverter : IValueConverter
{
    private static object Translate(object value) => TranslateValue(value);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return TranslateValue(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return TranslateValue(value);
    }

    private static object TranslateValue(object value)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return value;
    }
}
