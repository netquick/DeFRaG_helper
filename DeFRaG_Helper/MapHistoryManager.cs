using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeFRaG_Helper
{
    public class MapHistoryManager
    {
        private readonly string filePath;
        private List<int> lastPlayedMapsInMemory = new List<int>(); // In-memory list
        public static event Action MapHistoryUpdated;

        public MapHistoryManager(string appName)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            SimpleLogger.Log($"AppDataPath from MapHistoryManager: {appDataPath}");
            string appFolder = Path.Combine(appDataPath, appName);
            SimpleLogger.Log($"AppFolder from MapHistoryManager: {appFolder}");
            filePath = Path.Combine(appFolder, "lastPlayedMaps.json");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
                SimpleLogger.Log($"Created directory: {appFolder}");
            }

            // Load the last played maps into memory at initialization
            SimpleLogger.Log($"Loading last played maps from file: {filePath}");
            LoadLastPlayedMapsFromFile();

        }

        private async void LoadLastPlayedMapsFromFile()
        {
            if (File.Exists(filePath))
            {
                SimpleLogger.Log($"Found file: {filePath}");
                string json = await File.ReadAllTextAsync(filePath);
                lastPlayedMapsInMemory = JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
                SimpleLogger.Log($"Loaded {lastPlayedMapsInMemory.Count} last played maps");
            }
        }

        public async Task SaveLastPlayedMapsAsync()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(lastPlayedMapsInMemory, options);
            await File.WriteAllTextAsync(filePath, json);
            SimpleLogger.Log($"Saved {lastPlayedMapsInMemory.Count} last played maps to file: {filePath}");
        }

        public List<int> LoadLastPlayedMapsAsync()
        {
            // Return a copy of the in-memory list to prevent external modifications
            return new List<int>(lastPlayedMapsInMemory);
            SimpleLogger.Log($"Loaded async {lastPlayedMapsInMemory.Count} last played maps");
        }
        public List<int> GetLastPlayedMaps()
        {
            // Return a copy of the in-memory list to prevent external modifications
            return new List<int>(lastPlayedMapsInMemory);
            SimpleLogger.Log($"Loaded {lastPlayedMapsInMemory.Count} last played maps");
        }
        public async Task UpdateLastPlayedMapsAsync(int mapId)
        {
            lastPlayedMapsInMemory.Add(mapId);
            if (lastPlayedMapsInMemory.Count > 10)
            {
                lastPlayedMapsInMemory.RemoveAt(0); 
                await SimpleLogger.LogAsync("Removed oldest map from last played list");
            }
            MapHistoryUpdated?.Invoke();

            // Optionally, save to file immediately or wait to save at session end
            await SimpleLogger.LogAsync($"Added map {mapId} to last played list");
            await SaveLastPlayedMapsAsync();
        }
    }
}
