using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeFRaG_Helper
{
    public static class AppConfig
    {
        private static readonly string configFilePath;
        public static string GameDirectoryPath { get; set; }
        public static string SelectedColor { get; set; }
        public static string ButtonState { get; set; } 

        public static string PhysicsSetting { get; set; }
        public static string DatabasePath { get; set; } // Added for database path
        public static string DatabaseUrl { get; set; } // Added for database URL
        static AppConfig()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "DeFRaG_Helper");
            configFilePath = Path.Combine(appFolder, "config.json");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            // Default values for new properties
            DatabasePath = Path.Combine(appFolder, "MapData.db");
            DatabaseUrl = "https://dl.netquick.ch/MapData.db"; // Placeholder URL
        }

        public static async Task LoadConfigurationAsync()
        {
            if (File.Exists(configFilePath))
            {
                string json = await File.ReadAllTextAsync(configFilePath);
                var config = JsonSerializer.Deserialize<Configuration>(json);
                GameDirectoryPath = config?.GameDirectoryPath ?? string.Empty;
                SelectedColor = config?.SelectedColor ?? "Yellow";
                ButtonState = config?.ButtonState ?? "Play Game"; // Load button state
                PhysicsSetting = config?.PhysicsSetting ?? "CPM"; // Default to "CPM" if not set
                DatabasePath = config?.DatabasePath ?? DatabasePath; // Use default if not set
                DatabaseUrl = config?.DatabaseUrl ?? DatabaseUrl; // Use default if not set
            }
        }

        public static async Task SaveConfigurationAsync()
        {
            var config = new Configuration
            {
                GameDirectoryPath = GameDirectoryPath,
                SelectedColor = SelectedColor,
                ButtonState = ButtonState,
                PhysicsSetting = PhysicsSetting,
                DatabasePath = DatabasePath, // Save database path
                DatabaseUrl = DatabaseUrl // Save database URL
            };
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(config, options);
            await File.WriteAllTextAsync(configFilePath, json);
        }
        public static async Task EnsureDatabaseExistsAsync()
        {
            if (!File.Exists(DatabasePath))
            {
                // Use Downloader to download the database
                // Assuming Downloader has a static method DownloadFileAsync for this purpose
                await Downloader.DownloadFileAsync(DatabaseUrl, DatabasePath, null);
            }
        }
        private class Configuration
        {
            public string GameDirectoryPath { get; set; }
            public string SelectedColor { get; set; } 
            public string ButtonState { get; set; } 
            public string PhysicsSetting { get; set; }
            public string DatabasePath { get; set; } // Added for database path
            public string DatabaseUrl { get; set; } // Added for database URL
        }
    }
}
