using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DeFRaG_Helper.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible;

            if (value is int intValue)
            {
                isVisible = intValue != 0; // Treat non-zero integers as true
            }
            else if (value is bool boolValue)
            {
                isVisible = boolValue;
            }
            else
            {
                throw new InvalidOperationException("Unsupported type");
            }

            bool invert = parameter != null && bool.Parse((string)parameter);

            if (invert) isVisible = !isVisible;

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // Convert boolean back to integer (0 or 1) for SQLite storage.
                return boolValue ? 1 : 0;
            }
            else
            {
                throw new ArgumentException("Expected value to be of type Boolean", nameof(value));
            }
        }




    }

}
