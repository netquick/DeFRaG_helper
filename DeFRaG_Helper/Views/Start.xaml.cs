using DeFRaG_Helper.ViewModels;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Threading.Tasks;

namespace DeFRaG_Helper
{
    public partial class Start : Page
    {
        private ICollectionView? lastPlayedMapsView;
        public ICollectionView? ActiveServersView { get; private set; }
        private ObservableCollection<Map> localMaps = new ObservableCollection<Map>();

        private MapHistoryManager mapHistoryManager;

        private static Start? instance;
        private List<int> lastPlayedMapIds = new List<int>();

        private bool isBatchLoadingComplete = false;


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
            mapHistoryManager = MapHistoryManager.Instance;
            MapHistoryManager.MapHistoryUpdated += async () => await RefreshMapListAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                var serverViewModel = await ServerViewModel.GetInstanceAsync();
                serverViewModel.ServerListUpdated += OnServerListUpdated;
                ActiveServersView = CollectionViewSource.GetDefaultView(serverViewModel.Servers);

                var mapViewModel = await MapViewModel.GetInstanceAsync();
                mapViewModel.MapsBatchLoaded += MapViewModel_MapsBatchLoaded;
                mapViewModel.DataLoaded += MapViewModel_DataLoaded; // Subscribe to the DataLoaded event
                lastPlayedMapsView = new ListCollectionView(mapViewModel.Maps.ToList());
                mapHistoryManager = MapHistoryManager.Instance;

                try
                {
                    lastPlayedMapIds = await mapHistoryManager.GetLastPlayedRandomFromDbAsync();
                }
                catch (Exception ex)
                {
                    MessageHelper.Log(ex.Message);
                    throw;
                }

                if (lastPlayedMapsView != null)
                {
                    lastPlayedMapsView.Filter = FilterMaps;
                    ApplyCustomSort(lastPlayedMapsView, lastPlayedMapIds);
                    lastPlayedMapsView.Refresh();
                }

                this.DataContext = this;

                ItemsControlMaps.ItemsSource = lastPlayedMapsView;
                ItemsControlServer.ItemsSource = ActiveServersView;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void MapViewModel_DataLoaded(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                await SynchronizeLocalMapsAsync();
                RefreshLocalView();
            });
        }

        private void MapViewModel_MapsBatchLoaded(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                //Freezes UI when SynchronizeLocalMapsAsync is called

                //await SynchronizeLocalMapsAsync();
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
            lastPlayedMapsView = new ListCollectionView(localMaps.ToList());
            ApplyCustomSort(lastPlayedMapsView, lastPlayedMapIds);
            lastPlayedMapsView.Filter = FilterMaps;
            lastPlayedMapsView.Refresh();

            ItemsControlMaps.ItemsSource = lastPlayedMapsView;
        }

        private void RefreshLocalView()
        {
            var localView = CollectionViewSource.GetDefaultView(localMaps);
            localView.Filter = FilterMaps;
            localView.Refresh();
        }

        private void OnServerListUpdated(object sender, EventArgs e)
        {
            RefreshServerView();
        }

        private void RefreshServerView()
        {
            ActiveServersView?.Refresh();
        }

        private void ApplyCustomSort(ICollectionView collectionView, List<int> sortOrder)
        {
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
                return lastPlayedMapIds.Contains(mapItem.Id);
            }
            return false;
        }

        private void RefreshFilter()
        {
            lastPlayedMapsView?.Refresh();
        }

        public async Task RefreshMapListAsync()
        {
            mapHistoryManager = MapHistoryManager.Instance;

            lastPlayedMapIds = await mapHistoryManager.GetLastPlayedRandomFromDbAsync();
            ApplyCustomSort(lastPlayedMapsView, lastPlayedMapIds);
            RefreshFilter();
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

            if (mapX == null || mapY == null) return 0;

            int indexX = sortOrder.IndexOf(mapX.Id);
            int indexY = sortOrder.IndexOf(mapY.Id);

            if (indexX == -1) indexX = int.MaxValue;
            if (indexY == -1) indexY = int.MaxValue;

            return indexX.CompareTo(indexY);
        }
    }
}
