using DeFRaG_Helper.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace DeFRaG_Helper.Views
{
    /// <summary>
    /// Interaction logic for Maps2.xaml
    /// </summary>
    
    public partial class Maps2 : Page
    {

        private static Maps2 instance;
        public static Maps2 Instance
        {
            get
            {
                if (instance == null)
                    instance = new Maps2();
                return instance;
            }
        }
        public Maps2()
        {
            InitializeComponent();
            //MapsView.DataContext = this;
            // this.DataContext = this; // If DisplayedMaps is a property of Maps2
            //this.DataContext = MapViewModel.GetInstanceAsync().Result;

            var _ = InitializeDataContextAsync();
            this.Loaded += async (sender, e) =>
            {
                await InitializeDataContextAsync();
                AttachScrollViewerScrollChanged();
            };
        }

        private void AttachScrollViewerScrollChanged()
        {
            var scrollViewer = FindChildOfType<ScrollViewer>(MapsView); // Corrected to use the instance name
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            var viewModel = this.DataContext as MapViewModel;
            if (viewModel == null) return;

            if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 50) // Near the bottom
            {
                // Load more maps
                LoadMoreMaps(viewModel);
            }
        }

        private void LoadMoreMaps(MapViewModel viewModel)
        {
            var filteredMaps = viewModel.Maps.Where(map =>
                (string.IsNullOrEmpty(viewModel.SearchText) || map.Name.Contains(viewModel.SearchText, StringComparison.OrdinalIgnoreCase)) &&
                (!viewModel.ShowFavorites || map.IsFavorite == 1) &&
                (!viewModel.ShowInstalled || map.IsInstalled == 1) &&
                (!viewModel.ShowDownloaded || map.IsDownloaded == 1))
                .OrderByDescending(map => map.Releasedate)
                .ToList();

            viewModel.LoadDisplayedMapsSubset(filteredMaps, viewModel.DisplayedMaps.Count, 100);
        }
        private async Task InitializeDataContextAsync()
        {
            var viewModel = await MapViewModel.GetInstanceAsync();
            this.DataContext = viewModel;

            // Load initial subset of maps
            var filteredMaps = viewModel.Maps.Where(map =>
                (string.IsNullOrEmpty(viewModel.SearchText) || map.Name.Contains(viewModel.SearchText, StringComparison.OrdinalIgnoreCase)) &&
                (!viewModel.ShowFavorites || map.IsFavorite == 1) &&
                (!viewModel.ShowInstalled || map.IsInstalled == 1) &&
                (!viewModel.ShowDownloaded || map.IsDownloaded == 1))
                .OrderByDescending(map => map.Releasedate)
                .ToList();

            viewModel.LoadDisplayedMapsSubset(filteredMaps, 0, 100); // Load the first 100 maps
            MapsView.ItemsSource = viewModel.DisplayedMaps;
        }

        private void LoadDisplayedMapsSubset(int startIndex, int count)
        {
            var viewModel = this.DataContext as MapViewModel;
            if (viewModel == null) return;

            for (int i = startIndex; i < Math.Min(startIndex + count, viewModel.Maps.Count); i++)
            {
                viewModel.DisplayedMaps.Add(viewModel.Maps[i]);
            }

        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView != null)
            {
                var wrapPanel = FindChildOfType<WrapPanel>(listView);
                if (wrapPanel != null)
                {
                    // Assuming a margin of 5 units on the right as specified in the XAML for the ListView
                    var newMaxWidth = listView.ActualWidth - 5; // Adjust this value based on actual margins/padding
                    wrapPanel.MaxWidth = newMaxWidth;
                }
            }
        }

        private T? FindChildOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T)
                {
                    return (T)child;
                }

                var childOfChild = FindChildOfType<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }



    }
}
