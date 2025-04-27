using RegridMapper.Core.Configuration;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RegridMapper.Core.Converters
{
    public class StateIDToStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StateCode stateCode)
            {
                string styleKey = $"RadioButtons.{stateCode}"; // Convert enum to key
                return Application.Current.Resources[styleKey] as Style;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
            => throw new NotImplementedException(); // No need for reverse conversion
    }
}