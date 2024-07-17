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
using System.ComponentModel;
using DeFRaG_Helper.ViewModels;

namespace DeFRaG_Helper.UserControls
{
    /// <summary>
    /// Interaction logic for MapCard.xaml
    /// </summary>
    public partial class MapCard : UserControl, INotifyPropertyChanged
    {

        private bool _isFavoriteChecked = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsFavoriteChecked
        {
            get => _isFavoriteChecked;
            set
            {
                if (_isFavoriteChecked != value)
                {
                    _isFavoriteChecked = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFavoriteChecked)));
                    // Update visual state based on the new value
                    VisualStateManager.GoToState(this, _isFavoriteChecked ? "MouseOver" : "Normal", true);
                }
            }
        }

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

        private bool isFavoriteChecked = false;
        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            IsFavoriteChecked = !IsFavoriteChecked; // Use the property to trigger UI update
        }
        //private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        //{
        //    isFavoriteChecked = !isFavoriteChecked;
        //    VisualStateManager.GoToState((Button)sender, isFavoriteChecked ? "Checked" : "Unchecked", true);
        //}

        private async void FavoriteCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var map = checkBox.DataContext as Map;
            if (map != null)
            {
                map.IsFavorite = 1;
                var mapViewModel = await MapViewModel.GetInstanceAsync();

                await mapViewModel.UpdateFavoriteStateAsync(map);

            }
        }

        private async void FavoriteCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var map = checkBox.DataContext as Map;
            if (map != null)
            {
                map.IsFavorite = 0;
                var mapViewModel = await MapViewModel.GetInstanceAsync();

                await mapViewModel.UpdateFavoriteStateAsync(map);
            }
        }
    }

 
}
