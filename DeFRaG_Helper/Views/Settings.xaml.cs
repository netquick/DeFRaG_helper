using DeFRaG_Helper.Helpers;
using DeFRaG_Helper.ViewModels;
using DeFRaG_Helper.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

                // Convert the Color to a hexadecimal string
                var color = selectedBrush.Color;
                string hexColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";

                // Replace the ThemeColor resource with the new SolidColorBrush instance
                Application.Current.Resources["ThemeColor"] = new SolidColorBrush(color);
                AppConfig.SelectedColor = hexColor; // Save the hexadecimal color string
                await AppConfig.SaveConfigurationAsync();
            }
        }

        private async void txtGamePath_LostFocus(object sender, RoutedEventArgs e)
        {
            // Update the GameDirectoryPath in AppConfig
            AppConfig.GameDirectoryPath = txtGamePath.Text;

            // Optionally, save the configuration to persist the change
            await AppConfig.SaveConfigurationAsync();
        }

        private async void txtCountHistory_LostFocus(object sender, RoutedEventArgs e)
        {
            // Update the CountHistory in AppConfig
            if (int.TryParse(txtCountHistory.Text, out int count))
            {
                AppConfig.CountHistory = count;
                await AppConfig.SaveConfigurationAsync();
            }
        }

        private async void btnSync_Click(object sender, RoutedEventArgs e)
        {
            // Get the singleton instance of MapViewModel
            var mapViewModel = await MapViewModel.GetInstanceAsync();
            MessageHelper.ShowMessage("Syncing maps...");

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
            // Assuming AppConfig.SelectedColor is in the format "#RRGGBB"
            var hexColor = AppConfig.SelectedColor;

            // Find the ComboBoxItem whose background color matches the hexColor
            ComboBoxItem selectedItem = null;
            foreach (var item in ColorComboBox.Items.OfType<ComboBoxItem>())
            {
                if (item.Background is SolidColorBrush brush)
                {
                    var color = brush.Color;
                    string itemHexColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                    if (itemHexColor.Equals(hexColor, StringComparison.OrdinalIgnoreCase))
                    {
                        selectedItem = item;
                        break;
                    }
                }
            }

            // Update the ComboBox selection and other settings
            if (selectedItem != null)
            {
                ColorComboBox.SelectedItem = selectedItem;
            }
            txtGamePath.Text = AppConfig.GameDirectoryPath;

            chkDlImages.IsChecked = AppConfig.DownloadImagesOnUpdate;
            chkUnsecure.IsChecked = AppConfig.UseUnsecureConnection;

            txtCountHistory.Text = AppConfig.CountHistory.ToString();
            chkImgQuali.IsChecked = AppConfig.UseHighQualityImages;
        }

        private async void DownloadImg_Changed(object sender, RoutedEventArgs e)
        {
            // Update the DownloadImagesOnUpdate setting in AppConfig
            AppConfig.DownloadImagesOnUpdate = chkDlImages.IsChecked;

            // Optionally, save the configuration to persist the change
            await AppConfig.SaveConfigurationAsync();
        }

        private async void DlUnsecure_Changed(object sender, RoutedEventArgs e)
        {
            // Update the UseUnsecureConnection setting in AppConfig
            AppConfig.UseUnsecureConnection = chkUnsecure.IsChecked;

            // Optionally, save the configuration to persist the change
            await AppConfig.SaveConfigurationAsync();
        }

        private async void chkImgQuali_Changed(object sender, RoutedEventArgs e)
        {
            // Update the UseHighQualityImages setting in AppConfig
            AppConfig.UseHighQualityImages = chkImgQuali.IsChecked;

            // Optionally, save the configuration to persist the change
            await AppConfig.SaveConfigurationAsync();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of CustomBrowser
            var customBrowser = new CustomBrowser(AppConfig.GameDirectoryPath); // Pass the current game directory path as the initial path

            // Access the main window
            var mainWindow = Application.Current.MainWindow;

            // Center the CustomBrowser window relative to the main window
            if (mainWindow != null)
            {
                customBrowser.WindowStartupLocation = WindowStartupLocation.Manual;
                customBrowser.Left = mainWindow.Left + (mainWindow.Width - customBrowser.Width) / 2;
                customBrowser.Top = mainWindow.Top + (mainWindow.Height - customBrowser.Height) / 2;
            }

            var result = customBrowser.ShowDialog();
            if (result == true)
            {
                string selectedPath = customBrowser.SelectedFolderPath;
                // Use the selectedPath as needed, for example, setting it to a TextBox
                txtGamePath.Text = selectedPath;
                // Update the GameDirectoryPath in AppConfig
                AppConfig.GameDirectoryPath = selectedPath;

                // Save the configuration to persist the change
                Task.Run(async () => await AppConfig.SaveConfigurationAsync());
            }
        }

        private async void btnRadiant_Click(object sender, RoutedEventArgs e)
        {
            await CheckEditorInstallation.Instance.CheckEditor();
        }


    }
}
