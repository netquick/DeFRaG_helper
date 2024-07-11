using DeFRaG_Helper.ViewModels;
using System;
using System.Collections;
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
    /// Interaction logic for Start.xaml
    /// </summary>
    public partial class Start : Page
    {
        private ICollectionView? lastPlayedMapsView;

        private MapHistoryManager mapHistoryManager;

        private static Start? instance;
        private List<int> lastPlayedMapIds = new List<int>();
        public static Start Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Start();
                    }
                }
                return instance;
            }
        }
        private static readonly object syncRoot = new Object();
        public Start()
        {
            InitializeComponent();
            this.Loaded += async (sender, e) => await InitializeAsync();

            mapHistoryManager = new MapHistoryManager("DeFRaG_Helper");
            MapHistoryManager.MapHistoryUpdated += async () => await RefreshMapListAsync();
            // Set DataContext to MapViewModel instance
            //this.DataContext = MapViewModel.GetInstanceAsync().Result;
        }


        private async Task InitializeAsync()
        {
            try
            {
                var mapViewModel = await MapViewModel.GetInstanceAsync();
                this.DataContext = mapViewModel;

                // Load last played map IDs and reverse for sorting
                lastPlayedMapIds = (await mapHistoryManager.LoadLastPlayedMapsAsync());
                lastPlayedMapIds.Reverse();

                lastPlayedMapsView = CollectionViewSource.GetDefaultView(((MapViewModel)this.DataContext).Maps);

                if (lastPlayedMapsView != null) // Check for null
                {
                    lastPlayedMapsView.Filter = FilterMaps;

                    // Apply custom sort logic
                    ApplyCustomSort(lastPlayedMapsView, lastPlayedMapIds);

                    ItemsControlMaps.ItemsSource = lastPlayedMapsView;
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                Console.WriteLine(ex.Message);
            }
        }

        private void ApplyCustomSort(ICollectionView collectionView, List<int> sortOrder)
        {
            // Assuming your Map model has an Id property
            var customSort = new CustomSorter(sortOrder);
            if (collectionView is ListCollectionView listCollectionView)
            {
                listCollectionView.CustomSort = customSort;
            }
        }

        private bool FilterMaps(object item)
        {
            if (item is Map mapItem)
            {
                // Return true if the map's ID is in the list of last played map IDs
                return lastPlayedMapIds.Contains(mapItem.Id);
            }
            return false;
        }




        // Call this method to refresh the view when your filter criteria change
        private void RefreshFilter()
        {
            lastPlayedMapsView?.Refresh();
        }
        public async Task RefreshMapListAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                lastPlayedMapIds = await mapHistoryManager.LoadLastPlayedMapsAsync();

                //var mapViewModel = await MapViewModel.GetInstanceAsync();
                RefreshFilter(); // Instead of re-binding, refresh the existing view
            });
        }


    }
    class CustomSorter : IComparer
    {
        private readonly List<int> sortOrder;

        public CustomSorter(List<int> sortOrder)
        {
            this.sortOrder = sortOrder;
        }

        public int Compare(object? x, object? y)
        {
            var mapX = x as Map;
            var mapY = y as Map;

            if (mapX == null || mapY == null) return 0; // Or handle null comparison logic as needed

            int indexX = sortOrder.IndexOf(mapX.Id);
            int indexY = sortOrder.IndexOf(mapY.Id);

            if (indexX == -1) indexX = int.MaxValue; // Not found items go to the end
            if (indexY == -1) indexY = int.MaxValue; // Not found items go to the end

            return indexX.CompareTo(indexY);
        }
    }

}
