using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace DeFRaG_Helper.Converters
{
    public class LevelToIndentConverter : IValueConverter
    {
        public double IndentSize { get; set; } = 10.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TreeViewItem item)
            {
                return new Thickness(GetItemLevel(item) * IndentSize, 0, 0, 0);
            }
            return new Thickness(0);
        }

        private static int GetItemLevel(TreeViewItem item)
        {
            int level = 0; // Start from 0 for the top-level items
            DependencyObject parent = VisualTreeHelper.GetParent(item);
            // Traverse up the visual tree to count the levels.
            while (parent != null)
            {
                if (parent is TreeView)
                {
                    // Direct child of TreeView, break the loop.
                    break;
                }
                if (parent is TreeViewItem)
                {
                    // Increment level for TreeViewItem parents.
                    level++;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return level;
        }




        private static TreeViewItem GetParentTreeViewItem(DependencyObject item)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(item);
            while (parent != null && !(parent is TreeView))
            {
                if (parent is TreeViewItem)
                    return parent as TreeViewItem;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
