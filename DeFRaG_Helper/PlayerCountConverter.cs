﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DeFRaG_Helper
{

    public class PlayerCountConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is int currentPlayers && values[1] is int maxPlayers)
            {
                return $"Players: {currentPlayers}/{maxPlayers}";
            }
            return "Players: -/-"; // Fallback in case of unexpected input
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
