using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;
using DeFRaG_Helper.Converters;
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
            LoadConfigurationAndStartAsync().ContinueWith(_ =>
            {
                // This ensures the continuation runs on the UI thread
                Dispatcher.Invoke(() =>
                {
                    // Now that configuration and resources are loaded, show the main window
                    SimpleLogger.Log("Main window created");
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                });
            }); 
            StartDelayedTasks();

        }
        private async void StartDelayedTasks()
        {
            // Wait for 1 minute after the application starts
            await Task.Delay(TimeSpan.FromMinutes(1));

            // Execute your tasks here
            BackgroundTaskRunner backgroundTaskRunner = new BackgroundTaskRunner();
            await backgroundTaskRunner.RunTaskAsync();
        }

    


        private async Task LoadConfigurationAndStartAsync()
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
            // The main window creation and showing is moved to the continuation of this method in OnStartup
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
