﻿using DeFRaG_Helper.Helpers;
using System.Windows;
using System.Windows.Media;
namespace DeFRaG_Helper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MessageHelper.Log("Application starting");

            try
            {
                await LoadConfigurationAndStartAsync();
                MessageHelper.Log("Main window created");
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageHelper.Log($"Error during startup: {ex}");
                Current.Shutdown();
            }

            StartDelayedTasks();
        }


        private async void StartDelayedTasks()
        {
            // Wait for 1 minute after the application starts
            //await Task.Delay(TimeSpan.FromMinutes(1));

 

            // Execute your tasks here
            //BackgroundTaskRunner backgroundTaskRunner = new BackgroundTaskRunner();
            //await backgroundTaskRunner.RunTaskAsync();
        }

    


        private async Task LoadConfigurationAndStartAsync()
        {
            // Ensure the configuration is loaded before proceeding
            await MessageHelper.LogAsync("Loading configuration");

            await AppConfig.LoadConfigurationAsync();
            await MessageHelper.LogAsync($"Configuration loaded: {AppConfig.GameDirectoryPath}");
            AppConfig.UpdateThemeColor();

            ApplyThemeColor();
            // Create an instance of MapHistoryManager
   

            await AppConfig.EnsureDatabaseExistsAsync();
            MessageHelper.Log("Database exists");

            MessageHelper.Log("Creating MapHistoryManager");
            var mapHistoryManager = MapHistoryManager.Instance;;
            MessageHelper.Log("MapHistoryManager created");


            // The main window creation and showing is moved to the continuation of this method in OnStartup
        }
        private void ApplyThemeColor()
        {
            if (!string.IsNullOrEmpty(AppConfig.SelectedColor))
            {
                MessageHelper.Log($"Applying theme color: {AppConfig.SelectedColor}");
                // Assuming AppConfig.SelectedColor is a string like "Red", "Yellow", etc.
                var color = (Color)ColorConverter.ConvertFromString(AppConfig.SelectedColor);
                var brush = new SolidColorBrush(color);
                Current.Resources["ThemeColor"] = brush;
            }
            else {                 MessageHelper.Log("No theme color selected");
            }
        }
    }

}
