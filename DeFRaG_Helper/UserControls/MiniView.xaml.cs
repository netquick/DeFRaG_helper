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
    /// Interaction logic for MiniView.xaml
    /// </summary>
    public partial class MiniView : UserControl
    {
        public MiniView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MapProperty = DependencyProperty.Register(
       "Map", typeof(Map), typeof(MiniView), new PropertyMetadata(null, OnMapPropertyChanged));

        public Map Map
        {
            get { return (Map)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }
        private static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as HighLightCard;
            if (control != null)
            {
                control.DataContext = e.NewValue;
            }
        }
        private async void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Cast the DataContext to a Map object
            var map = this.DataContext as Map;
            if (map != null)
            {
                // Assuming you have a way to access the MapViewModel instance
                var viewModel = MapViewModel.GetInstanceAsync().Result; // Note: Using .Result for simplicity; consider using async/await.
                viewModel.SelectedMap = map;
                await viewModel.UpdateConfigurationAsync(map);


            }
        }

    }
}
