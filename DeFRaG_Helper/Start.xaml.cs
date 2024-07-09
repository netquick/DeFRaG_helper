using DeFRaG_Helper.ViewModels;
using System;
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

namespace DeFRaG_Helper
{
    /// <summary>
    /// Interaction logic for Start.xaml
    /// </summary>
    public partial class Start : Page
    {

        private MapHistoryManager mapHistoryManager;

        private static Start? instance;

        public static Start Instance
        {
            get
            {
                if (instance == null)
                    instance = new Start();
                return instance;
            }
        }
        public Start()
        {
            InitializeComponent();
            mapHistoryManager = new MapHistoryManager("DeFRaG_Helper"); // Initialize mapHistoryManager here
            MapHistoryManager.MapHistoryUpdated += () => Task.Run(() => RefreshMapListAsync());

            LoadLastPlayedMaps();

        }

        public static async Task RefreshMapListAsync() // Change method signature to be async Task
        {
            await Application.Current.Dispatcher.InvokeAsync(async () => // Use InvokeAsync with async lambda
            {
                var mapViewModel = await MapViewModel.GetInstanceAsync();
                // Use the instance to access ItemsControlMaps
                Instance.ItemsControlMaps.ItemsSource = null;
                Instance.ItemsControlMaps.ItemsSource = mapViewModel.Maps;
            });
        }


        private async void LoadLastPlayedMaps()
        {
            var lastPlayedMapIds = await mapHistoryManager.LoadLastPlayedMapsAsync();
            // Correctly await the asynchronous call to GetMapsFromIds
            var lastPlayedMaps = await GetMapsFromIds(lastPlayedMapIds);
            ItemsControlMaps.ItemsSource = lastPlayedMaps;
        }


        // Dummy method to represent fetching map objects from IDs
        private async Task<List<Map>> GetMapsFromIds(List<int> mapIds)
        {
            var mapViewModel = await MapViewModel.GetInstanceAsync(); // Ensure you have access to the MapViewModel instance
            var maps = new List<Map>();

            foreach (var id in mapIds)
            {
                var map = mapViewModel.Maps.FirstOrDefault(m => m.Id == id);
                if (map != null)
                {
                    maps.Add(map);
                }
            }

            return maps;
        }

    }

    //Treehelper class f

}
