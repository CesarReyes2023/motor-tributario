using System;
using System.Globalization;
using System.Windows.Data;

namespace LibroFiscal.Desktop.Converters;

public class PercentageToWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double percentage)
        {
            double maxWidth = 300; // Default max width if not provided
            if (parameter != null && double.TryParse(parameter.ToString(), out double paramWidth))
            {
                maxWidth = paramWidth;
            }

            // Validar que el porcentaje esté entre 0 y 1
            var safePercentage = Math.Max(0.0, Math.Min(1.0, percentage));
            return safePercentage * maxWidth;
        }

        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
