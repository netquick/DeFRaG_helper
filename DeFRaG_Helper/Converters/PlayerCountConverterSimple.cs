﻿using System.Globalization;
using System.Windows.Data;

namespace DeFRaG_Helper.Converters
{

    public class PlayerCountConverterSimple : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is int currentPlayers && values[1] is int maxPlayers)
            {
                return $"{currentPlayers}/{maxPlayers}";
            }
            return "-/-"; // Fallback in case of unexpected input
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
