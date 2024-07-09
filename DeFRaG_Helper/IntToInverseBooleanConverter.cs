using System;
using System.Globalization;
using System.Windows.Data;

namespace DeFRaG_Helper
{
    public class IntToInverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                // Assuming 0 means not downloaded and should return true to enable the button
                return intValue == 0;
            }
            return false; // Default to false if the value is not an int
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This converter only works for one-way binding.");
        }
    }
}
