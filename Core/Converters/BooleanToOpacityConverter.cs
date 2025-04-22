using System.Globalization;
using System.Windows.Data;

namespace RegridMapper.Core.Converters
{
    public class BooleanToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible)
            {
                return isVisible ? 1.0 : 0.0; // Fully visible if true, hidden if false
            }
            return 0.0; // Default to hidden
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double opacity)
            {
                return opacity > 0.5; // Consider anything above 0.5 as visible
            }
            return false;
        }
    }
}