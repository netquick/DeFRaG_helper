using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DeFRaG_Helper
{
    public class ProgressBarWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 3 && values[0] is double value && values[1] is double maximum && values[2] is double actualWidth)
            {
                if (maximum > 0) // Avoid division by zero
                {
                    double proportion = value / maximum;
                    return actualWidth * proportion; // Calculate the proportional width
                }
            }
            return 0; // Default width if inputs are not valid
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // Not needed for this use case
        }
    }
}
