using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeFRaG_Helper
{
    public class MapHistoryManager
    {
        private readonly string filePath;
        public static event Action MapHistoryUpdated;


        public MapHistoryManager(string appName)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, appName);
            filePath = Path.Combine(appFolder, "lastPlayedMaps.json");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
        }

        public async Task SaveLastPlayedMapsAsync(List<int> mapIds)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(mapIds, options);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<List<int>> LoadLastPlayedMapsAsync()
        {
            if (File.Exists(filePath))
            {
                string json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
            }
            MapHistoryUpdated?.Invoke();

            return new List<int>();
        }

        public async Task UpdateLastPlayedMapsAsync(int mapId)
        {
            List<int> lastPlayedMaps = await LoadLastPlayedMapsAsync();
            lastPlayedMaps.Add(mapId);
            if (lastPlayedMaps.Count > 10)
            {
                lastPlayedMaps.RemoveAt(0); // Remove the oldest
            }
            await SaveLastPlayedMapsAsync(lastPlayedMaps);
            MapHistoryUpdated?.Invoke();
        }

    }
}
