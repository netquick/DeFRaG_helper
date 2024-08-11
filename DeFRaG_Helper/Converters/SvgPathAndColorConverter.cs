using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
            var filePath = Path.Combine(basePath, path);
            if (File.Exists(filePath))
            {
                try
                {
                    using (var stream = File.OpenRead(filePath))
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

        private object LoadSvgFromStream(Stream stream, Brush colorBrush)
        {
            var svgDocument = XDocument.Load(stream);
            if (svgDocument.Root == null) return DependencyProperty.UnsetValue;

            var ns = svgDocument.Root.GetDefaultNamespace();
            var elements = svgDocument.Descendants(ns + "path")
                                      .Concat(svgDocument.Descendants(ns + "circle"))
                                      .Concat(svgDocument.Descendants(ns + "rect"))
                                      .Concat(svgDocument.Descendants(ns + "ellipse"))
                                      .Concat(svgDocument.Descendants(ns + "polygon"))
                                      .Concat(svgDocument.Descendants(ns + "polyline"))
                                      .Concat(svgDocument.Descendants(ns + "use"));

            var geometryGroup = new GeometryGroup();
            foreach (var element in elements)
            {
                Geometry geometry = null;
                switch (element.Name.LocalName)
                {
                    case "path":
                        var dataAttribute = element.Attribute("d");
                        if (dataAttribute != null)
                        {
                            geometry = Geometry.Parse(dataAttribute.Value);
                        }
                        break;
                    case "circle":
                        var cx = double.Parse(element.Attribute("cx")?.Value ?? "0");
                        var cy = double.Parse(element.Attribute("cy")?.Value ?? "0");
                        var r = double.Parse(element.Attribute("r")?.Value ?? "0");
                        geometry = new EllipseGeometry(new Point(cx, cy), r, r);
                        break;
                    case "rect":
                        var x = double.Parse(element.Attribute("x")?.Value ?? "0");
                        var y = double.Parse(element.Attribute("y")?.Value ?? "0");
                        var width = double.Parse(element.Attribute("width")?.Value ?? "0");
                        var height = double.Parse(element.Attribute("height")?.Value ?? "0");
                        geometry = new RectangleGeometry(new Rect(x, y, width, height));
                        break;
                    case "ellipse":
                        var ecx = double.Parse(element.Attribute("cx")?.Value ?? "0");
                        var ecy = double.Parse(element.Attribute("cy")?.Value ?? "0");
                        var rx = double.Parse(element.Attribute("rx")?.Value ?? "0");
                        var ry = double.Parse(element.Attribute("ry")?.Value ?? "0");
                        geometry = new EllipseGeometry(new Point(ecx, ecy), rx, ry);
                        break;
                    case "polygon":
                    case "polyline":
                        var points = element.Attribute("points")?.Value;
                        if (points != null)
                        {
                            geometry = CreateStreamGeometryFromPoints(points, element.Name.LocalName == "polygon");
                        }
                        break;
                    case "use":
                        var href = element.Attribute(XName.Get("href", "http://www.w3.org/1999/xlink"))?.Value;
                        if (href != null && href.StartsWith("#"))
                        {
                            var referencedElement = svgDocument.Descendants().FirstOrDefault(e => e.Attribute("id")?.Value == href.Substring(1));
                            if (referencedElement != null)
                            {
                                var referencedGeometry = LoadSvgElement(referencedElement);
                                if (referencedGeometry != null)
                                {
                                    geometryGroup.Children.Add(referencedGeometry);
                                }
                            }
                        }
                        break;
                }

                if (geometry != null)
                {
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

        private Geometry LoadSvgElement(XElement element)
        {
            Geometry geometry = null;
            switch (element.Name.LocalName)
            {
                case "path":
                    var dataAttribute = element.Attribute("d");
                    if (dataAttribute != null)
                    {
                        geometry = Geometry.Parse(dataAttribute.Value);
                    }
                    break;
                case "circle":
                    var cx = double.Parse(element.Attribute("cx")?.Value ?? "0");
                    var cy = double.Parse(element.Attribute("cy")?.Value ?? "0");
                    var r = double.Parse(element.Attribute("r")?.Value ?? "0");
                    geometry = new EllipseGeometry(new Point(cx, cy), r, r);
                    break;
                case "rect":
                    var x = double.Parse(element.Attribute("x")?.Value ?? "0");
                    var y = double.Parse(element.Attribute("y")?.Value ?? "0");
                    var width = double.Parse(element.Attribute("width")?.Value ?? "0");
                    var height = double.Parse(element.Attribute("height")?.Value ?? "0");
                    geometry = new RectangleGeometry(new Rect(x, y, width, height));
                    break;
                case "ellipse":
                    var ecx = double.Parse(element.Attribute("cx")?.Value ?? "0");
                    var ecy = double.Parse(element.Attribute("cy")?.Value ?? "0");
                    var rx = double.Parse(element.Attribute("rx")?.Value ?? "0");
                    var ry = double.Parse(element.Attribute("ry")?.Value ?? "0");
                    geometry = new EllipseGeometry(new Point(ecx, ecy), rx, ry);
                    break;
                case "polygon":
                case "polyline":
                    var points = element.Attribute("points")?.Value;
                    if (points != null)
                    {
                        geometry = CreateStreamGeometryFromPoints(points, element.Name.LocalName == "polygon");
                    }
                    break;
            }
            return geometry;
        }

        private StreamGeometry CreateStreamGeometryFromPoints(string points, bool isClosed)
        {
            var pointCollection = PointCollection.Parse(points);
            var streamGeometry = new StreamGeometry();

            using (var context = streamGeometry.Open())
            {
                context.BeginFigure(pointCollection[0], true, isClosed);
                context.PolyLineTo(pointCollection.Skip(1).ToArray(), true, true);
            }

            return streamGeometry;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
