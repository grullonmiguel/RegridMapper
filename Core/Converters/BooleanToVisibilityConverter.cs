using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RegridMapper.Converters
{
    /// <summary>
    /// Converts a boolean value to Visibility.
    /// If the converter parameter is "Inverse", it reverses the logic.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                bool isInverse = parameter?.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) == true;
                return (boolValue ^ isInverse) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool isInverse = parameter?.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) == true;
                return visibility == Visibility.Visible ^ isInverse;
            }
            return false;
        }
    }
}