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

        static AppConfig()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "DeFRaG_Helper");
            configFilePath = Path.Combine(appFolder, "config.json");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
        }

        public static async Task LoadConfigurationAsync()
        {
            if (File.Exists(configFilePath))
            {
                string json = await File.ReadAllTextAsync(configFilePath);
                var config = JsonSerializer.Deserialize<Configuration>(json);
                GameDirectoryPath = config?.GameDirectoryPath ?? string.Empty;
                SelectedColor = config?.SelectedColor ?? "Yellow";
            }
        }

        public static async Task SaveConfigurationAsync()
        {
            var config = new Configuration { GameDirectoryPath = GameDirectoryPath, SelectedColor = SelectedColor }; // Include SelectedColor
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(config, options);
            await File.WriteAllTextAsync(configFilePath, json);
        }

        private class Configuration
        {
            public string GameDirectoryPath { get; set; }
            public string SelectedColor { get; set; } // Correctly added
        }
    }
}
