using DeFRaG_Helper.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ICollectionView? ActiveServersView { get; private set; }
        private ObservableCollection<Map> localMaps = new ObservableCollection<Map>();

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

            this.Loaded += async (sender, e) =>
            {
                await InitializeAsync();
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.customDropDownButton.MapPlayed += (s, e) => RefreshMapListAsync();
                }
            };
            var mapHistoryManager = MapHistoryManager.GetInstance("DeFRaG_Helper");
            MapHistoryManager.MapHistoryUpdated += async () => await RefreshMapListAsync();
            // Set DataContext to MapViewModel instance
            //this.DataContext = MapViewModel.GetInstanceAsync().Result;
        }

        // Place this method in Start.xaml.cs
        private void MapViewModel_MapsBatchLoaded(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                await SynchronizeLocalMapsAsync();
                RefreshLocalView();
            });
        }


        private async Task SynchronizeLocalMapsAsync()
        {
            var mapViewModel = await MapViewModel.GetInstanceAsync();
            localMaps.Clear();
            foreach (var map in mapViewModel.Maps)
            {
                localMaps.Add(map);
            }
            // Reinitialize lastPlayedMapsView with the updated localMaps
            lastPlayedMapsView = new ListCollectionView(localMaps.ToList());
            ApplyCustomSort(lastPlayedMapsView, lastPlayedMapIds); // Reapply custom sorting
            lastPlayedMapsView.Filter = FilterMaps; // Reapply filtering
            lastPlayedMapsView.Refresh(); // Refresh the view

            // Ensure the UI is bound to the updated lastPlayedMapsView
            ItemsControlMaps.ItemsSource = lastPlayedMapsView;
        }

        // Place this method in Start.xaml.cs
        private void RefreshLocalView()
        {
            var localView = CollectionViewSource.GetDefaultView(localMaps);
            localView.Filter = FilterMaps; // Apply your local filtering logic here
                                           // Apply any sorting or additional transformations here
            localView.Refresh();
        }

        private async Task InitializeAsync()
        {
            try
            {
                var serverViewModel = await ServerViewModel.GetInstanceAsync();
                ActiveServersView = CollectionViewSource.GetDefaultView(serverViewModel.Servers);

                var mapViewModel = await MapViewModel.GetInstanceAsync();
                mapViewModel.MapsBatchLoaded += MapViewModel_MapsBatchLoaded;
                // Create a new CollectionView for this page
                lastPlayedMapsView = new ListCollectionView(mapViewModel.Maps.ToList());
                //lastPlayedMapsView = CollectionViewSource.GetDefaultView(mapViewModel.Maps);
                mapHistoryManager = MapHistoryManager.GetInstance("DeFRaG_Helper");

                // Load last played map IDs and reverse for sorting
                try
                {
                    //lastPlayedMapIds = mapHistoryManagerLazy.Value.GetLastPlayedMaps();
                    lastPlayedMapIds = await mapHistoryManager.GetLastPlayedRandomFromDbAsync();
                    //lastPlayedMapIds.Reverse();
                }
                catch (Exception ex)
                {
                    MessageHelper.Log(ex.Message);
                    throw;
                }
       


                if (lastPlayedMapsView != null) // Check for null
                {
                    lastPlayedMapsView.Filter = FilterMaps; // Ensure this is uncommented
                    ApplyCustomSort(lastPlayedMapsView, lastPlayedMapIds);
                    lastPlayedMapsView.Refresh(); // Refresh the view to apply filter and sort
                }

                this.DataContext = this; // Now 'this' includes both map and server data contexts

                ItemsControlMaps.ItemsSource = lastPlayedMapsView;
                ItemsControlServer.ItemsSource = ActiveServersView;
            }
            catch (Exception ex)
            {
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
        // Filter predicate method
        private bool ServerHasPlayers(object item)
        {
            if (item is ServerNode server) // Change from ServerViewModel to ServerNode
            {
                return server.CurrentPlayers > 0;
            }
            return false;
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
            mapHistoryManager = MapHistoryManager.GetInstance("DeFRaG_Helper");

            lastPlayedMapIds = await mapHistoryManager.GetLastPlayedRandomFromDbAsync();
            //lastPlayedMapIds.Reverse(); // Ensure the list is reversed

            ApplyCustomSort(lastPlayedMapsView, lastPlayedMapIds); // Reapply custom sort
            RefreshFilter(); // Refresh the view
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
