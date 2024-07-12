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
            // Initialize logging
            SimpleLogger.Log("Application starting");
            LoadConfigurationAndStartAsync();
        }

        private async void LoadConfigurationAndStartAsync()
        {
            // Ensure the configuration is loaded before proceeding
            await SimpleLogger.LogAsync("Loading configuration");

            await AppConfig.LoadConfigurationAsync();
            await SimpleLogger.LogAsync($"Configuration loaded: {AppConfig.GameDirectoryPath}");

            ApplyThemeColor();
            // Create an instance of MapHistoryManager
            SimpleLogger.Log("Creating MapHistoryManager");
            var mapHistoryManager = MapHistoryManager.GetInstance("DeFRaG_Helper");
            SimpleLogger.Log("MapHistoryManager created");

            await AppConfig.EnsureDatabaseExistsAsync();
            SimpleLogger.Log("Database exists");
            MainWindow mainWindow = new MainWindow();
            //maybe this would be good?
            //mainWindow.DataContext = mainWindow;
            SimpleLogger.Log("Main window created");
            mainWindow.Show();

            // Assuming StartChecks is static and can be called like this
            //CheckGameInstall.StartChecks();
        }
        private void ApplyThemeColor()
        {
            if (!string.IsNullOrEmpty(AppConfig.SelectedColor))
            {
                SimpleLogger.Log($"Applying theme color: {AppConfig.SelectedColor}");
                // Assuming AppConfig.SelectedColor is a string like "Red", "Yellow", etc.
                var color = (Color)ColorConverter.ConvertFromString(AppConfig.SelectedColor);
                var brush = new SolidColorBrush(color);
                Current.Resources["ThemeColor"] = brush;
            }
            else {                 SimpleLogger.Log("No theme color selected");
            }
        }
    }

}
