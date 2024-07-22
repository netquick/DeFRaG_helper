using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
namespace DeFRaG_Helper
{
    public static class AppConfig
    {
        private static readonly string configFilePath;
        public static string? GameDirectoryPath { get; set; }
        public static string? SelectedColor { get; set; }
        public static string? ButtonState { get; set; }
        public static string? MenuState { get; set; }
        public static string? PhysicsSetting { get; set; }
        public static string? DatabasePath { get; set; } 
        public static string? DatabaseUrl { get; set; } 
        public static string? ConnectionString { get; set; }
        public static bool ? UseUnsecureConnection { get; set; }
        public static bool ? DownloadImagesOnUpdate { get; set; }
        public static int? CountHistory { get; set; }
        public static bool ? UseHighQualityImages { get; set; }
        public static int? SelectedMap { get; set; }
        public static string? EditorPath { get; set; }


        public delegate Task<string> RequestGameDirectoryDelegate();
        public static event RequestGameDirectoryDelegate? OnRequestGameDirectory;
        static AppConfig()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "DeFRaG_Helper");
            configFilePath = Path.Combine(appFolder, "config.json");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
                MessageHelper.Log($"Created directory: {appFolder}");
            }
            // Default values for new properties
            DatabasePath = Path.Combine(appFolder, "MapData.db");
            DatabaseUrl = "https://dl.netquick.ch/MapData.db"; // Placeholder URL
            ConnectionString = $"Data Source={DatabasePath}";
        }

        public static async Task LoadConfigurationAsync()
        {
            try
            {
                if (File.Exists(configFilePath))
                {
                    await MessageHelper.LogAsync($"Found config {configFilePath}");

                    string json = await File.ReadAllTextAsync(configFilePath);
                    var config = JsonSerializer.Deserialize<Configuration>(json);
                    GameDirectoryPath = config?.GameDirectoryPath ?? string.Empty;
                    SelectedColor = config?.SelectedColor ?? "Yellow";
                    ButtonState = config?.ButtonState ?? "Play Game"; // Load button state
                    PhysicsSetting = config?.PhysicsSetting ?? "CPM"; // Default to "CPM" if not set
                    DatabasePath = config?.DatabasePath ?? DatabasePath; // Use default if not set
                    DatabaseUrl = config?.DatabaseUrl ?? DatabaseUrl;
                    MenuState = config?.MenuState ?? MenuState; // Use default if not set
                    ConnectionString = config?.ConnectionString ?? ConnectionString; // Use default if not set
                    UseUnsecureConnection = config?.UseUnsecureConnection ?? false;
                    DownloadImagesOnUpdate = config?.DownloadImagesOnUpdate ?? false;
                    CountHistory = config?.CountHistory ?? 100;
                    UseHighQualityImages = config?.UseHighQualityImages ?? false;
                    SelectedMap = config?.SelectedMap ?? 0;
                    EditorPath = config?.EditorPath ?? string.Empty;
                    await MessageHelper.LogAsync($"GameDirectoryPath: {GameDirectoryPath}");
                    await MessageHelper.LogAsync($"SelectedColor: {SelectedColor}");
                    await MessageHelper.LogAsync($"ButtonState: {ButtonState}");
                    await MessageHelper.LogAsync($"PhysicsSetting: {PhysicsSetting}");
                    await MessageHelper.LogAsync($"DatabasePath: {DatabasePath}");
                    await MessageHelper.LogAsync($"DatabaseUrl: {DatabaseUrl}");


                }
                else
                {
                    //create the file if it doesn't exist

                    await MessageHelper.LogAsync($"{configFilePath} not found");
                    //await SaveConfigurationAsync();

                }
            }
            catch (Exception ex)
            {
                MessageHelper.Log(ex.Message);
                throw;
            }

        }
        public static void UpdateThemeColor()
        {
            try
            {
                // Attempt to convert SelectedColor to a SolidColorBrush
                if (!string.IsNullOrEmpty(AppConfig.SelectedColor))
                {
                    var color = (Color)ColorConverter.ConvertFromString(AppConfig.SelectedColor);
                    var brush = new SolidColorBrush(color);
                    Application.Current.Resources["ThemeColor"] = brush;
                }
                else
                {
                    throw new FormatException("SelectedColor is null or empty.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Debug.WriteLine($"Failed to update theme color, applying default color. Error: {ex.Message}");

                // Apply a default color
                var defaultColor = Colors.Yellow; // Example default color
                Application.Current.Resources["ThemeColor"] = new SolidColorBrush(defaultColor);
            }
        }




        public static async Task SaveConfigurationAsync()
        {

            if (string.IsNullOrEmpty(GameDirectoryPath))
            {
                if (OnRequestGameDirectory != null)
                {
                    GameDirectoryPath = await OnRequestGameDirectory.Invoke();
                }
            }



            var config = new Configuration
            {
                GameDirectoryPath = GameDirectoryPath,
                SelectedColor = SelectedColor,
                ButtonState = ButtonState,
                PhysicsSetting = PhysicsSetting,
                DatabasePath = DatabasePath,
                DatabaseUrl = DatabaseUrl,
                MenuState = MenuState,
                ConnectionString = ConnectionString,
                UseUnsecureConnection = UseUnsecureConnection,
                DownloadImagesOnUpdate = DownloadImagesOnUpdate,
                CountHistory = CountHistory,
                UseHighQualityImages = UseHighQualityImages,
                SelectedMap = SelectedMap,
                EditorPath = EditorPath
                
            };
            MessageHelper.Log($"GameDirectoryPath: {GameDirectoryPath}");
            MessageHelper.Log($"SelectedColor: {SelectedColor}");
            MessageHelper.Log($"ButtonState: {ButtonState}");
            MessageHelper.Log($"PhysicsSetting: {PhysicsSetting}");
            MessageHelper.Log($"DatabasePath: {DatabasePath}");
            MessageHelper.Log($"DatabaseUrl: {DatabaseUrl}");

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(config, options);
            try
            {
                await File.WriteAllTextAsync(configFilePath, json);
                MessageHelper.Log("Configuration saved");
            }
            catch (Exception ex)
            {
                MessageHelper.Log(ex.Message);
                throw;
            }
        }
        public static async Task EnsureDatabaseExistsAsync()
        {
            if (!File.Exists(DatabasePath))
            {
                await MessageHelper.LogAsync("Database not found, downloading...");
                // Use Downloader to download the database
                // Assuming Downloader has a static method DownloadFileAsync for this purpose
                await Downloader.DownloadFileAsync(DatabaseUrl, DatabasePath, null);
                await MessageHelper.LogAsync("Database downloaded");
            }
            else
            {
                await MessageHelper.LogAsync($"Database found at {DatabasePath}");
            }
        }
        
    }
}
