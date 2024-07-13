using DeFRaG_Helper.ViewModels;
using System;
using System.Collections.Generic;
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


        private static Demos instance;

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
            this.Loaded += DemoLoaded;
        }
        //method when demo loaded
        private void DemoLoaded(object sender, RoutedEventArgs e)
        {
            LoadDataAsync();

        }

        //Method to load the data for the selected map
        private async void LoadDataAsync()
        {
            var viewModel = await MapViewModel.GetInstanceAsync();
            var selectedMap = viewModel.SelectedMap;
            if (selectedMap != null)
            {
                //MapName without extension

                var demoLink = DemoParser.GetDemoLink(System.IO.Path.GetFileNameWithoutExtension(selectedMap.MapName));
                var demoItems = await DemoParser.GetDemoLinksAsync(demoLink);
                foreach (var item in demoItems)
                {
                    item.DecodeName();
                }
                lvDemos.ItemsSource = demoItems;
                txtMapSearch.Text = System.IO.Path.GetFileNameWithoutExtension(selectedMap.MapName);

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
                lvDemos.ItemsSource = demoItems;
            }
            else
            {
                // Optionally clear the list if the search query is empty
                lvDemos.ItemsSource = null;
            }
        }


        private async void TxtMapSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(txtMapSearch.Text))
                {
                    // Clear the ListView's ItemsSource
                    lvDemos.ItemsSource = null;
                    SimpleLogger.Log("Clear list");
                }
                else
                {
                    SimpleLogger.Log($"Searching for demos with map name: {txtMapSearch.Text}");
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
               var mapName = item.MapName;
                //now we have to check if the map is installed. We check "IsInstalled" property of the map
                var viewModel = MapViewModel.GetInstanceAsync().Result; 
                SimpleLogger.Log($"Checking for MapName: {mapName}");
                var map = viewModel.Maps.FirstOrDefault(m => m.MapName == (mapName + ".bsp")); 
                if (map != null)
                {
                    if (map.IsInstalled == 0)
                    {
                        await MapInstaller.InstallMap(map);
                        SimpleLogger.Log($"Map {map.MapName} installed.");
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
                    



                    //System.Diagnostics.Process.Start(AppConfig.GameDirectoryPath + "\\oDFe.x64.exe", $"+demo {mapName}//{System.IO.Path.GetFileNameWithoutExtension(item.Name)}") ;
                    System.Diagnostics.Process.Start(AppConfig.GameDirectoryPath + "\\oDFe.x64.exe", $"+demo {mapName}/{System.IO.Path.GetFileNameWithoutExtension(item.Name)}");


                }




                // Implement your double-click logic here
                // For example, navigate to a detail page or display a dialog
                ///MessageBox.Show($"Double-clicked on item: {item.Name}");


            }
        }

    }
}
