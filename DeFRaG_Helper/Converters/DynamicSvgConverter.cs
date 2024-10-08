﻿using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Resources;
using System.Xml.Linq;

namespace DeFRaG_Helper.Converters
{
    public class DynamicSvgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (string.IsNullOrEmpty(path))
                return DependencyProperty.UnsetValue; // Fix for Problem 4

            Brush colorBrush = Brushes.White; // Default color
            if (parameter is string colorParam)
            {
                var tempBrush = new BrushConverter().ConvertFromString(colorParam);
                if (tempBrush != null) // Fix for Problem 5
                {
                    colorBrush = (Brush)tempBrush;
                }
            }

            StreamResourceInfo streamResourceInfo = null; // Declare outside try block for wider scope

            var resourcePath = $"pack://application:,,,/DeFRaG_Helper;component/{path}";
            try
            {
                var uri = new Uri(resourcePath, UriKind.RelativeOrAbsolute);
                streamResourceInfo = Application.GetResourceStream(uri);
                if (streamResourceInfo != null)
                {
                    using (var stream = streamResourceInfo.Stream)
                    {
                        return LoadSvgFromStream(stream, colorBrush); // Fix for Problem 1
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
                        return LoadSvgFromStream(stream, colorBrush); // Fix for Problem 2
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading SVG from file: {ex.Message}");
                }
            }

            if (streamResourceInfo != null) // Check for null to fix Problem 3
            {
                return LoadSvgFromStream(streamResourceInfo.Stream, colorBrush);
            }

            return DependencyProperty.UnsetValue; // Return a non-null default value if all else fails
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
