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
            mapHistoryManager = new MapHistoryManager("DeFRaG_Helper");
            MapHistoryManager.MapHistoryUpdated += async () => await RefreshMapListAsync();
            // Set DataContext to MapViewModel instance
            Task.Run(InitializeAsync);
        }


        private async Task InitializeAsync()
        {
            var mapViewModel = await MapViewModel.GetInstanceAsync(); // Get the instance once
            this.DataContext = mapViewModel; // Use the same instance for DataContext
            await mapViewModel.UpdateLastPlayedMapsViewAsync(); // Use the same instance to refresh
            ItemsControlMaps.ItemsSource = mapViewModel.LastPlayedMapsView; // Use the same instance for ItemsSource
        }


        public async Task RefreshMapListAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                var mapViewModel = await MapViewModel.GetInstanceAsync();
                await mapViewModel.UpdateLastPlayedMapsViewAsync(); // Ensure this method updates the filtered view
                ItemsControlMaps.ItemsSource = mapViewModel.LastPlayedMapsView; // Bind to the filtered view
            });
        }

    }

}
