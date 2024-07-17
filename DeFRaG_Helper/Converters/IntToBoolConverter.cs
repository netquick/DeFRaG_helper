using System;
using System.Globalization;
using System.Windows.Data;

namespace DeFRaG_Helper.Converters
{
    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Assuming 1 represents true/favorite, and 0 (or null) represents false/not favorite
            return value is int intValue && intValue == 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Convert back to integer representation for the database
            return value is bool boolValue && boolValue ? 1 : 0;
        }
    }
}
