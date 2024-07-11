using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace DeFRaG_Helper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LoadConfigurationAndStartAsync();
        }

        private async void LoadConfigurationAndStartAsync()
        {
            // Ensure the configuration is loaded before proceeding
            await AppConfig.LoadConfigurationAsync();
            ApplyThemeColor();
            // Create an instance of MapHistoryManager
            MapHistoryManager mapHistoryManager = new MapHistoryManager("DeFRaG_Helper");

            // Load the last played maps
            await mapHistoryManager.LoadLastPlayedMapsAsync();
            // Now that the configuration is loaded, proceed with the rest of the startup sequence
            await AppConfig.EnsureDatabaseExistsAsync();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            // Assuming StartChecks is static and can be called like this
            //CheckGameInstall.StartChecks();
        }
        private void ApplyThemeColor()
        {
            if (!string.IsNullOrEmpty(AppConfig.SelectedColor))
            {
                // Assuming AppConfig.SelectedColor is a string like "Red", "Yellow", etc.
                var color = (Color)ColorConverter.ConvertFromString(AppConfig.SelectedColor);
                var brush = new SolidColorBrush(color);
                Current.Resources["ThemeColor"] = brush;
            }
        }
    }

}
