using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DeFRaG_Helper.Converters
{
    public class BoolToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Assuming true moves the toggle to the right (HorizontalAlignment.Right)
            // and false moves it to the left (HorizontalAlignment.Left).
            if (value is bool isChecked)
            {
                return isChecked ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            }
            return HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Converting from HorizontalAlignment to bool is not supported.");
        }
    }
}
