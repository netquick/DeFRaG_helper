using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DeFRaG_Helper.Converters
{
    public class CombinedDataContextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return new CombinedDataContext
            {
                DisplayMaps = values[0],
                MapViewModel = values[1]
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CombinedDataContext
    {
        public object DisplayMaps { get; set; }
        public object MapViewModel { get; set; }
    }
}
