using DeFRaG_Helper.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for Maps.xaml
    /// </summary>
    public partial class Maps : Page
    {
        private static Maps instance;
        private ICollectionView mapsView;
        public ICollectionView MapsView
        {
            get { return mapsView; }
        }

        public static Maps Instance
        {
            get
            {
                if (instance == null)
                    instance = new Maps();
                return instance;
            }
        }
        public Maps()
        {
            InitializeComponent();
            var _ = InitializeDataContextAsync(); // Discard the task since we can't await in the constructor
            RefreshView();
            this.Loaded += Maps_Loaded;

        }
        private async Task InitializeDataContextAsync()
        {
            this.DataContext = await MapViewModel.GetInstanceAsync();
            SetupFiltering();
            
        }
        private void SetupFiltering()
        {
            mapsView = CollectionViewSource.GetDefaultView(((MapViewModel)this.DataContext).Maps);
            mapsView.Filter = FilterMaps;

            // Change sorting to descending to have the newest maps at the top
            mapsView.SortDescriptions.Clear(); // It's good practice to clear existing sort descriptions first
            mapsView.SortDescriptions.Add(new SortDescription("Releasedate", ListSortDirection.Descending));

            // Subscribe to filter changes
            chkFavorite.Checked += (s, e) => RefreshView();
            chkFavorite.Unchecked += (s, e) => RefreshView();
            chkInstalled.Checked += (s, e) => RefreshView();
            chkInstalled.Unchecked += (s, e) => RefreshView();
            chkDownloaded.Checked += (s, e) => RefreshView();
            chkDownloaded.Unchecked += (s, e) => RefreshView();
            searchBar.TextChanged += (s, e) => RefreshView();
        }


        private void RefreshView()
        {
            mapsView.Refresh();
            UpdateLblCount();
        }
        private void UpdateLblCount()
        {
            // Assuming lblCount is the Label in your XAML that shows the count
            lblCount.Content = mapsView.Cast<object>().Count().ToString();
        }
        //when maps is fully loaded
        private void Maps_Loaded(object sender, RoutedEventArgs e)
        {
            lblCount.Content = mapsView.Cast<object>().Count().ToString();

        }

        private bool FilterMaps(object item)
        {
            if (item is not Map map) return false;

            bool isFavoriteChecked = chkFavorite.IsChecked ?? false;
            bool isInstalledChecked = chkInstalled.IsChecked ?? false;
            bool isDownloadedChecked = chkDownloaded.IsChecked ?? false;
            // Ensure searchBar is not null and handle case where searchBar.Text is null
            string searchText = searchBar?.Text?.ToLower() ?? string.Empty;

            bool matchesFavorite = !isFavoriteChecked || (map.IsFavorite == 1);
            bool matchesInstalled = !isInstalledChecked || (map.IsInstalled == 1);
            bool matchesDownloaded = !isDownloadedChecked || (map.IsDownloaded == 1);

            // Safely check if the map's properties contain the search text, accounting for potential null values
            bool matchesSearchText = string.IsNullOrEmpty(searchText) || searchText == "filter..." || // Ignore if searchText is default or empty
                                     (map.Name?.ToLower().Contains(searchText) ?? false) ||
                                     (map.Mapname?.ToLower().Contains(searchText) ?? false) ||
                                     (map.Filename?.ToLower().Contains(searchText) ?? false) ||
                                     (map.Author?.ToLower().Contains(searchText) ?? false);

            return matchesFavorite && matchesInstalled && matchesDownloaded && matchesSearchText;
        }


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

        private void SearchBox_GotFocus (object sender, RoutedEventArgs e)
        {
            if (searchBar.Text == "Filter...")
            {
                searchBar.Text = "";
            }


        }
    }
}
