using DeFRaG_Helper.ViewModels;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
