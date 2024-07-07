using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeFRaG_Helper
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {

        private static Settings instance;

        public static Settings Instance
        {
            get
            {
                if (instance == null)
                    instance = new Settings();
                return instance;
            }
        }
        public Settings()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                Border b1 = TreeHelper.FindChild<Border>(CustomCalendar);
                Border b2 = TreeHelper.FindChild<Border>(b1);
                b2.BorderBrush = Brushes.Transparent;
            };
        }

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            var selectedItem = comboBox.SelectedItem as ComboBoxItem;
            if (selectedItem != null && selectedItem.Background is SolidColorBrush)
            {
                // Retrieve the SolidColorBrush from the selected ComboBoxItem
                var selectedBrush = selectedItem.Background as SolidColorBrush;

                // Create a new SolidColorBrush instance with the selected color
                var newBrush = new SolidColorBrush(selectedBrush.Color);

                // Replace the ThemeColor resource with the new SolidColorBrush instance
                Application.Current.Resources["ThemeColor"] = newBrush;
            }
        }

    }
    public static class TreeHelper
    {
        public static T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    foundChild = (T)child;
                    break;
                }
                else
                {
                    foundChild = FindChild<T>(child);
                    if (foundChild != null)
                        break;
                }
            }
            return foundChild;
        }
    }
}
