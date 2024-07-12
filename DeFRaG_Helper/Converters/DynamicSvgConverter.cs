using System;
//using SharpVectors.Renderers.Wpf;
//using SharpVectors.Converters;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Linq;

namespace DeFRaG_Helper.Converters
{
    public class DynamicSvgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (string.IsNullOrEmpty(path))
                return null;

            // First, try to construct the pack URI for the embedded resource.
            var resourcePath = $"pack://application:,,,/DeFRaG_Helper;component/{path}";
            try
            {
                var uri = new Uri(resourcePath, UriKind.RelativeOrAbsolute);
                var streamResourceInfo = Application.GetResourceStream(uri);
                if (streamResourceInfo != null)
                {
                    using (var stream = streamResourceInfo.Stream)
                    {
                        return LoadSvgFromStream(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading SVG as resource: {ex.Message}");
            }

            // If loading as a resource failed, try loading from the file system.
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = System.IO.Path.Combine(basePath, path);
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    using (var stream = System.IO.File.OpenRead(filePath))
                    {
                        return LoadSvgFromStream(stream);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading SVG from file: {ex.Message}");
                }
            }

            return null;
        }

        private object LoadSvgFromStream(System.IO.Stream stream)
        {
            var svgDocument = XDocument.Load(stream);
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
                Brush = Brushes.White, // Set to your desired color
                Pen = new Pen(Brushes.White, 1) // Set to your desired pen
            };

            return new DrawingImage(drawing);
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

