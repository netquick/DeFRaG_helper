﻿using System;
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
                await SimpleLogger.LogAsync($"{configFilePath} not found");

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
            SimpleLogger.Log($"GameDirectoryPath: {GameDirectoryPath}");
            SimpleLogger.Log($"SelectedColor: {SelectedColor}");
            SimpleLogger.Log($"ButtonState: {ButtonState}");
            SimpleLogger.Log($"PhysicsSetting: {PhysicsSetting}");
            SimpleLogger.Log($"DatabasePath: {DatabasePath}");
            SimpleLogger.Log($"DatabaseUrl: {DatabaseUrl}");

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(config, options);
            await File.WriteAllTextAsync(configFilePath, json);
            SimpleLogger.Log("Configuration saved");
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
