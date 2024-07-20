using DeFRaG_Helper.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Threading;

namespace DeFRaG_Helper
{
    /// <summary>
    /// Interaction logic for Demos.xaml
    /// </summary>
    public partial class Demos : Page
    {
        private DispatcherTimer debounceTimer;
        private List<int> lastPlayedMapIds = new List<int>();
        private ICollectionView? lastPlayedMapsView;
        public ICollectionView? ActiveServersView { get; private set; }
        private ObservableCollection<Map> localMaps = new ObservableCollection<Map>();

        private MapHistoryManager mapHistoryManager;
        private MapViewModel mapViewModel = MapViewModel.GetInstanceAsync().Result;

        private static Demos? instance;

        public static Demos Instance
        {
            get
            {
                if (instance == null)
                    instance = new Demos();
                return instance;
            }
        }
        public Demos()
        {
            InitializeComponent();
            LoadViewModelAsync();

            this.Loaded += async (sender, e) =>
            {
                await InitializeAsync();
                LoadDataAsync();

            };
            var mapHistoryManager = MapHistoryManager.GetInstance("DeFRaG_Helper");
            MapHistoryManager.MapHistoryUpdated += async () => await RefreshMapListAsync();
            mapViewModel.PropertyChanged += MapViewModel_PropertyChanged;

        }
        private void MapViewModel_MapsBatchLoaded(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                await SynchronizeLocalMapsAsync();
                RefreshLocalView();
            });
        }
        private void MapViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MapViewModel.SelectedMap))
            {
                LoadDataAsync(); // Call your method to load the demos based on the new selected map.
            }
        }
        private async Task SynchronizeLocalMapsAsync()
        {
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
        private void RefreshLocalView()
        {
            var localView = CollectionViewSource.GetDefaultView(localMaps);
            localView.Filter = FilterMaps; // Apply your local filtering logic here
                                           // Apply any sorting or additional transformations here
            localView.Refresh();
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

            lastPlayedMapIds = await mapHistoryManager.GetLastPlayedMapsFromDbAsync();
            lastPlayedMapIds.Reverse(); // Ensure the list is reversed

            ApplyCustomSort(lastPlayedMapsView, lastPlayedMapIds); // Reapply custom sort
            RefreshFilter(); // Refresh the view
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
                    lastPlayedMapIds = await mapHistoryManager.GetLastPlayedMapsFromDbAsync();
                    lastPlayedMapIds.Reverse();
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        //Method to load the data for the selected map
        private async void LoadDataAsync()
        {
            var viewModel = await MapViewModel.GetInstanceAsync();
            var selectedMap = viewModel.SelectedMap;
            if (selectedMap != null)
            {
                //Mapname without extension

                var demoLink = DemoParser.GetDemoLink(System.IO.Path.GetFileNameWithoutExtension(selectedMap.Mapname));
                var demoItems = await DemoParser.GetDemoLinksAsync(demoLink);
                foreach (var item in demoItems)
                {
                    item.DecodeName();
                }
                icDemos.ItemsSource = demoItems;
                txtMapSearch.Text = System.IO.Path.GetFileNameWithoutExtension(selectedMap.Mapname);

            }
        }

        //method to load the demo data
        private async void LoadDemoDataAsync()
        {
            // Use the text from txtMapSearch as the search query
            string searchQuery = txtMapSearch.Text.Trim();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                // Use the search query to get the demo link
                var demoLink = DemoParser.GetDemoLink(searchQuery);
                var demoItems = await DemoParser.GetDemoLinksAsync(demoLink);
                foreach (var item in demoItems)
                {
                    item.DecodeName();
                }
                icDemos.ItemsSource = demoItems;
            }
            else
            {
                // Optionally clear the list if the search query is empty
                icDemos.ItemsSource = null;
            }
        }


        private async void TxtMapSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(txtMapSearch.Text))
                {
                    // Clear the ListView's ItemsSource
                    icDemos.ItemsSource = null;
                    MessageHelper.Log("Clear list");
                }
                else
                {
                    MessageHelper.Log($"Searching for demos with map name: {txtMapSearch.Text}");
                    LoadDemoDataAsync();
                }
            }
        }



        private async void LoadViewModelAsync()
        {
            var viewModel = await MapViewModel.GetInstanceAsync();
            this.DataContext = viewModel; // This sets the DataContext for the entire page
                                          // Alternatively, set the DataContext for just the ComboBox if needed:
                                          // cmbMap.DataContext = viewModel;
            var selectedMap = viewModel.SelectedMap; // Access the selected map

        }




        private async void LvDemos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Check if the actual item (not empty space) in the ListView was double-clicked
            var item = ((FrameworkElement)e.OriginalSource).DataContext as DemoItem;
            if (item != null)
            {
               //we need the mapname of the actual demo
               var mapName = item.Mapname;
                //now we have to check if the map is installed. We check "IsInstalled" property of the map
                var viewModel = MapViewModel.GetInstanceAsync().Result; 
                MessageHelper.Log($"Checking for Mapname: {mapName}");
                var map = viewModel.Maps.FirstOrDefault(m => m.Mapname == (mapName + ".bsp")); 
                if (map != null)
                {
                    if (map.IsInstalled == 0)
                    {
                        await MapInstaller.InstallMap(map);
                        MessageHelper.Log($"Map {map.Mapname} installed.");
                    }
                    //Prepare the progress handler to update the UI
                    var progressHandler = new Progress<double>(value =>
                    {
                        App.Current.Dispatcher.Invoke(() => MainWindow.Instance.UpdateProgressBar(value));
                    });
                    //Prepare the link for the demo. It's contructed from http://95.31.6.66/~/api/get_file_list?uri=/demos/{mapName[0]}/{mapName}/" and the name of the demo
                    var demoLink = $"http://95.31.6.66/demos/{mapName[0]}/{mapName}/{item.Name}";

                    //check if there is a demo folder. If not, create it
                    if (!System.IO.Directory.Exists(AppConfig.GameDirectoryPath + "\\defrag\\demos"))
                    {
                        System.IO.Directory.CreateDirectory(AppConfig.GameDirectoryPath + "\\defrag\\demos");
                    }
                    //check if there is a demo folder for the map. If not, create it
                    if (!System.IO.Directory.Exists(AppConfig.GameDirectoryPath + $"\\defrag\\demos\\{mapName}"))
                    {
                        System.IO.Directory.CreateDirectory(AppConfig.GameDirectoryPath + $"\\defrag\\demos\\{mapName}");
                    }

                    //Download the demo to the demo folder
                    await Downloader.DownloadFileAsync(demoLink, AppConfig.GameDirectoryPath + $"\\defrag\\demos\\{mapName}\\{item.Name}", progressHandler);
                    



                    //System.Diagnostics.Process.Start(AppConfig.GameDirectoryPath + "\\oDFe.x64.exe", $"+demo {mapName}//{System.IO.Path.GetFilenameWithoutExtension(item.Name)}") ;
                    System.Diagnostics.Process.Start(AppConfig.GameDirectoryPath + "\\oDFe.x64.exe", $"+demo {mapName}/{System.IO.Path.GetFileNameWithoutExtension(item.Name)}");


                }




                // Implement your double-click logic here
                // For example, navigate to a detail page or display a dialog
                ///MessageBox.Show($"Double-clicked on item: {item.Name}");


            }
        }

    }
}
