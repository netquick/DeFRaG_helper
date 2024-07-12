using System;
using System.Globalization;
using System.Windows.Data;

namespace DeFRaG_Helper.Converters
{
    public class SizeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long size)
            {
                // Convert size to KB, MB, etc., as appropriate
                const int scale = 1024;
                double kbSize = size / scale;
                if (kbSize < scale)
                {
                    return $"{kbSize:0.##} KB";
                }
                double mbSize = kbSize / scale;
                if (mbSize < scale)
                {
                    return $"{mbSize:0.##} MB";
                }
                double gbSize = mbSize / scale;
                return $"{gbSize:0.##} GB";
            }
            return "Unknown size";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Converting from string to size is not supported.");
        }
    }
}
