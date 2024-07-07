using System;
using System.Globalization;
using System.Windows.Data;

namespace DeFRaG_Helper
{
    public class PhysicsModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Assuming the Physics property is an integer or similar
            var physicsValue = (int)value;
            switch (physicsValue)
            {
                case 1:
                    return "VQ3";
                case 2:
                    return "CPM";
                case 3:
                    return "VQ3 / CPM";
                default:
                    return "Unknown";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Implement conversion back if necessary, or throw NotSupportedException
            throw new NotSupportedException();
        }
    }
}
