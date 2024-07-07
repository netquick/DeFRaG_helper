﻿using DeFRaG_Helper.ViewModels;
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

            // Assuming chkFavorite, chkInstalled, and chkDownloaded are CheckBoxes in your XAML
            chkFavorite.Checked += (s, e) => mapsView.Refresh();
            chkFavorite.Unchecked += (s, e) => mapsView.Refresh();
            chkInstalled.Checked += (s, e) => mapsView.Refresh();
            chkInstalled.Unchecked += (s, e) => mapsView.Refresh();
            chkDownloaded.Checked += (s, e) => mapsView.Refresh();
            chkDownloaded.Unchecked += (s, e) => mapsView.Refresh();
        }

        private bool FilterMaps(object item)
        {
            if (item is not Map map) return false;

            bool isFavoriteChecked = chkFavorite.IsChecked ?? false;
            bool isInstalledChecked = chkInstalled.IsChecked ?? false;
            bool isDownloadedChecked = chkDownloaded.IsChecked ?? false;

            bool matchesFavorite = !isFavoriteChecked || (map.IsFavorite == 1);
            bool matchesInstalled = !isInstalledChecked || (map.IsInstalled == 1);
            bool matchesDownloaded = !isDownloadedChecked || (map.IsDownloaded == 1);

            return matchesFavorite && matchesInstalled && matchesDownloaded;
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

    }
}
