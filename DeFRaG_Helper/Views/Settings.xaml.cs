using DeFRaG_Helper.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {

        private static Settings instance;

        public static Settings Instance
        {
            get
            {
                if (instance == null)
                    instance = new Settings();
                return instance;
            }
        }
        public Settings()
        {
            InitializeComponent();
            
        }

        private async void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            var selectedItem = comboBox.SelectedItem as ComboBoxItem;
            if (selectedItem != null && selectedItem.Background is SolidColorBrush)
            {
                // Retrieve the SolidColorBrush from the selected ComboBoxItem
                var selectedBrush = selectedItem.Background as SolidColorBrush;

                // Create a new SolidColorBrush instance with the selected color
                var newBrush = new SolidColorBrush(selectedBrush.Color);

                // Replace the ThemeColor resource with the new SolidColorBrush instance
                Application.Current.Resources["ThemeColor"] = newBrush;
                AppConfig.SelectedColor = selectedItem.Content.ToString();
                await AppConfig.SaveConfigurationAsync();

            }
        }

        private async void btnSync_Click(object sender, RoutedEventArgs e)
        {
            // Get the singleton instance of MapViewModel
            var mapViewModel = await MapViewModel.GetInstanceAsync();
            App.Current.Dispatcher.Invoke(() => MainWindow.Instance.ShowMessage("Syncing maps..."));

            // Access the Maps collection
            var maps = mapViewModel.Maps;

            // Create a progress reporter
            var progressReporter = new Progress<double>(progress =>
            {
                App.Current.Dispatcher.Invoke(() => MainWindow.Instance.UpdateProgressBar(progress));
                Console.WriteLine($"Progress: {progress}%");
            });

            // Use the singleton instance of MapFileSyncService instead of creating a new one
            var syncService = MapFileSyncService.Instance;

            // Start the synchronization process
            await syncService.SyncMapFilesWithFileSystem(maps, progressReporter);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Load the selected color from AppConfig and update the ComboBox selection
            var selectedItem = ColorComboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == AppConfig.SelectedColor);
            if (selectedItem != null)
            {
                ColorComboBox.SelectedItem = selectedItem;
                txtGamePath.Text = AppConfig.GameDirectoryPath;
            }
        }
    }
    
}
