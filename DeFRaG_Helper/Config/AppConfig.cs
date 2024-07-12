using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeFRaG_Helper
{
    public static class AppConfig
    {
        private static readonly string configFilePath;
        public static string? GameDirectoryPath { get; set; }
        public static string? SelectedColor { get; set; }
        public static string? ButtonState { get; set; }
        public static string? PhysicsSetting { get; set; }
        public static string? DatabasePath { get; set; } 
        public static string? DatabaseUrl { get; set; } 

        public delegate Task<string> RequestGameDirectoryDelegate();
        public static event RequestGameDirectoryDelegate OnRequestGameDirectory;
        static AppConfig()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "DeFRaG_Helper");
            configFilePath = Path.Combine(appFolder, "config.json");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
                SimpleLogger.Log($"Created directory: {appFolder}");
            }
            // Default values for new properties
            DatabasePath = Path.Combine(appFolder, "MapData.db");
            DatabaseUrl = "https://dl.netquick.ch/MapData.db"; // Placeholder URL
        }

        public static async Task LoadConfigurationAsync()
        {
            try
            {
                if (File.Exists(configFilePath))
                {
                    await SimpleLogger.LogAsync($"Found config {configFilePath}");

                    string json = await File.ReadAllTextAsync(configFilePath);
                    var config = JsonSerializer.Deserialize<Configuration>(json);
                    GameDirectoryPath = config?.GameDirectoryPath ?? string.Empty;
                    SelectedColor = config?.SelectedColor ?? "Yellow";
                    ButtonState = config?.ButtonState ?? "Play Game"; // Load button state
                    PhysicsSetting = config?.PhysicsSetting ?? "CPM"; // Default to "CPM" if not set
                    DatabasePath = config?.DatabasePath ?? DatabasePath; // Use default if not set
                    DatabaseUrl = config?.DatabaseUrl ?? DatabaseUrl; // Use default if not set
                    await SimpleLogger.LogAsync($"GameDirectoryPath: {GameDirectoryPath}");
                    await SimpleLogger.LogAsync($"SelectedColor: {SelectedColor}");
                    await SimpleLogger.LogAsync($"ButtonState: {ButtonState}");
                    await SimpleLogger.LogAsync($"PhysicsSetting: {PhysicsSetting}");
                    await SimpleLogger.LogAsync($"DatabasePath: {DatabasePath}");
                    await SimpleLogger.LogAsync($"DatabaseUrl: {DatabaseUrl}");
                }
                else
                {
                    //create the file if it doesn't exist

                    await SimpleLogger.LogAsync($"{configFilePath} not found");
                    //await SaveConfigurationAsync();

                }
            }
            catch (Exception ex)
            {
                SimpleLogger.Log(ex.Message);
                throw;
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
                DatabasePath = DatabasePath, // Save database path
                DatabaseUrl = DatabaseUrl // Save database URL

            };
            SimpleLogger.Log($"GameDirectoryPath: {GameDirectoryPath}");
            SimpleLogger.Log($"SelectedColor: {SelectedColor}");
            SimpleLogger.Log($"ButtonState: {ButtonState}");
            SimpleLogger.Log($"PhysicsSetting: {PhysicsSetting}");
            SimpleLogger.Log($"DatabasePath: {DatabasePath}");
            SimpleLogger.Log($"DatabaseUrl: {DatabaseUrl}");

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(config, options);
            try
            {
                await File.WriteAllTextAsync(configFilePath, json);
                SimpleLogger.Log("Configuration saved");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log(ex.Message);
                throw;
            }
        }
        public static async Task EnsureDatabaseExistsAsync()
        {
            if (!File.Exists(DatabasePath))
            {
                await SimpleLogger.LogAsync("Database not found, downloading...");
                // Use Downloader to download the database
                // Assuming Downloader has a static method DownloadFileAsync for this purpose
                await Downloader.DownloadFileAsync(DatabaseUrl, DatabasePath, null);
                await SimpleLogger.LogAsync("Database downloaded");
            }
            else
            {
                await SimpleLogger.LogAsync($"Database found at {DatabasePath}");
            }
        }
        
    }
}
