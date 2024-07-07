using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace DeFRaG_Helper
{
    public class SidebarWidthConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double sidebarWidth = (double)value;
            Debug.WriteLine($"Converting sidebar width: {sidebarWidth}"); // Use Debug.WriteLine
            double margin = 20; // Adjust for 10px margin on each side
            Debug.WriteLine($"Final sidebar width: {Math.Max(0, sidebarWidth - margin)}");
            return Math.Max(0, sidebarWidth - margin); // Ensure width doesn't go negative
                                                       //show the final output in debug console

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
