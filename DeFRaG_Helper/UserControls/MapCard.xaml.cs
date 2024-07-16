using System;
using System.Collections;
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
    /// Interaction logic for MapCard.xaml
    /// </summary>
    public partial class MapCard : UserControl
    {
        public Map CurrentMap { get; set; }
        public static readonly DependencyProperty IconsProperty = DependencyProperty.Register(
            "Icons", typeof(IEnumerable), typeof(MapCard), new PropertyMetadata(null));

        public IEnumerable Icons
        {
            get { return (IEnumerable)GetValue(IconsProperty); }
            set { SetValue(IconsProperty, value); }
        }

        public MapCard()
        {
            InitializeComponent();
        }
        private void FavoriteCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Add map to favorites
        }

        private void FavoriteCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Remove map from favorites
        }
    }

 
}
