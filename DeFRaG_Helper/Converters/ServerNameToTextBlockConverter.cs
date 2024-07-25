using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace DeFRaG_Helper.Converters
{
    public class ServerNameToTextBlockConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string serverName)
            {
                var segments = ParseQuakeColorCodes(serverName); // Call the static method
                var textBlock = new TextBlock
                {
                    TextAlignment = TextAlignment.Left, // Center the text
                    TextWrapping = TextWrapping.Wrap // Ensure text wrapping is enabled
                };
                foreach (var segment in segments)
                {
                    textBlock.Inlines.Add(new Run(segment.Text) { Foreground = segment.Color });
                }
                return textBlock;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static List<(string Text, SolidColorBrush Color)> ParseQuakeColorCodes(string serverName)
        {
            var segments = new List<(string Text, SolidColorBrush Color)>();
            var colors = new Dictionary<char, SolidColorBrush>
    {
        { '0', Brushes.Gray },
        { '1', Brushes.Red },
        { '2', Brushes.Green },
        { '3', Brushes.Yellow },
        { '4', Brushes.Blue },
        { '5', Brushes.Cyan },
        { '6', Brushes.Magenta },
        { '7', Brushes.White },
        { '8', Brushes.Orange },
        { '9', Brushes.Gray },
        // Add more colors if needed
    };

            int lastIndex = 0;
            for (int i = 0; i < serverName.Length; i++)
            {
                if (serverName[i] == '^' && i + 1 < serverName.Length && colors.ContainsKey(serverName[i + 1]))
                {
                    if (i > lastIndex)
                    {
                        segments.Add((serverName.Substring(lastIndex, i - lastIndex), Brushes.White)); // Default color
                    }
                    lastIndex = i + 2; // Skip color code
                    i++; // Move past the color digit

                    if (i + 1 < serverName.Length)
                    {
                        int nextColorIndex = serverName.IndexOf('^', i + 1);
                        if (nextColorIndex == -1) nextColorIndex = serverName.Length;
                        segments.Add((serverName.Substring(i + 1, nextColorIndex - i - 1), colors[serverName[i]]));
                        i = nextColorIndex - 1;
                        lastIndex = nextColorIndex;
                    }
                }
            }

            // Add the last segment if there's any
            if (lastIndex < serverName.Length)
            {
                segments.Add((serverName.Substring(lastIndex), Brushes.White)); // Default color
            }

            return segments;
        }

    }
}
