using DeFRaG_Helper.ViewModels;
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

namespace DeFRaG_Helper.UserControls
{
    /// <summary>
    /// Interaction logic for TagBar.xaml
    /// </summary>
    public partial class TagBar : UserControl
    {
        public TagBar()
        {
            InitializeComponent();
            DataContext = new TagBarViewModel();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var tagItem = checkBox.DataContext as TagItem;
            if (tagItem != null)
            {
                var mapViewModel = MapViewModel.GetInstanceAsync().Result;
                if (mapViewModel != null)
                {
                    mapViewModel.SelectedTags.Add(tagItem.Name);
                    mapViewModel.ApplyFilters();
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var tagItem = checkBox.DataContext as TagItem;
            if (tagItem != null)
            {
                var mapViewModel = MapViewModel.GetInstanceAsync().Result;
                if (mapViewModel != null)
                {
                    mapViewModel.SelectedTags.Remove(tagItem.Name);
                    mapViewModel.ApplyFilters();
                }
            }
        }
    }


}
