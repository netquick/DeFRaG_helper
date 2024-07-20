using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Resources;
using System.Xml.Linq;

namespace DeFRaG_Helper.Converters
{
    public class SvgPathAndColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || !(values[0] is string path) || !(values[1] is string colorParam))
                return DependencyProperty.UnsetValue;

            Brush colorBrush = Brushes.White; // Default color
            if (!string.IsNullOrEmpty(colorParam))
            {
                var tempBrush = new BrushConverter().ConvertFromString(colorParam);
                if (tempBrush != null)
                {
                    colorBrush = (Brush)tempBrush;
                }
            }

            // The rest of the method follows the same logic as in DynamicSvgConverter's Convert method,
            // but uses the path and colorBrush determined from the values array.

            StreamResourceInfo streamResourceInfo = null;
            var resourcePath = $"pack://application:,,,/DeFRaG_Helper;component/{path}";
            try
            {
                var uri = new Uri(resourcePath, UriKind.RelativeOrAbsolute);
                streamResourceInfo = Application.GetResourceStream(uri);
                if (streamResourceInfo != null)
                {
                    using (var stream = streamResourceInfo.Stream)
                    {
                        return LoadSvgFromStream(stream, colorBrush);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading SVG as resource: {ex.Message}");
            }

            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = System.IO.Path.Combine(basePath, path);
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    using (var stream = System.IO.File.OpenRead(filePath))
                    {
                        return LoadSvgFromStream(stream, colorBrush);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading SVG from file: {ex.Message}");
                }
            }

            return DependencyProperty.UnsetValue;
        }

        private object LoadSvgFromStream(System.IO.Stream stream, Brush colorBrush)
        {
            var svgDocument = XDocument.Load(stream);
            if (svgDocument.Root == null) return DependencyProperty.UnsetValue; // Fix for Problem 6

            var ns = svgDocument.Root.GetDefaultNamespace();
            var paths = svgDocument.Descendants(ns + "path");

            var geometryGroup = new GeometryGroup();
            foreach (var pathElement in paths)
            {
                var dataAttribute = pathElement.Attribute("d");
                if (dataAttribute != null)
                {
                    var geometry = Geometry.Parse(dataAttribute.Value);
                    geometryGroup.Children.Add(geometry);
                }
            }

            var drawing = new GeometryDrawing
            {
                Geometry = geometryGroup,
                Brush = colorBrush,
                Pen = new Pen(colorBrush, 1)
            };

            return new DrawingImage(drawing);
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
