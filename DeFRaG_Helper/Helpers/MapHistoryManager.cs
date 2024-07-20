using Microsoft.Data.Sqlite;
using System.IO;

namespace DeFRaG_Helper
{
    public class MapHistoryManager
    {
        private static MapHistoryManager instance;
        private readonly string filePath;
        private List<int> lastPlayedMapsInMemory = new List<int>(); // In-memory list
        public static event Action MapHistoryUpdated;
        private Lazy<MapHistoryManager> mapHistoryManagerLazy = new Lazy<MapHistoryManager>(() => new MapHistoryManager("DeFRaG_Helper"));

        private MapHistoryManager(string appName)
        {
            //check if the db file exists


            // Load the last played maps into memory at initialization
            MessageHelper.Log($"Loading last played maps from dB");
            LoadLastPlayedMapsFromFile();

        }
        // Provide a static method to get the instance
        public static MapHistoryManager GetInstance(string appName)
        {
            if (instance == null)
            {
                instance = new MapHistoryManager(appName);
            }
            return instance;
        }
        private async void LoadLastPlayedMapsFromFile()
        {
            string dbFilePath = Path.Combine(AppConfig.DatabasePath, $"MapData.db");

            if (!File.Exists(dbFilePath))
            {
                // Create the database file if it doesn't exist.
                await CreateLastPlayedMapsTableAsync();

            }
            await GetLastPlayedMapsFromDbAsync();


        }
        //method to get the last played maps from the database
        public async Task<List<int>> GetLastPlayedMapsFromDbAsync()
        {
            List<int> lastPlayedMaps = new List<int>();

            // SQL command to select the MapId column from the LastPlayedMaps table.
            string selectCommandText = @"
                SELECT MapId
                FROM LastPlayedMaps
                ORDER BY PlayedDateTime DESC
                LIMIT 10;
            ";

            // Enqueue the command to be executed on the database connection.
            DbQueue.Instance.Enqueue(async connection =>
            {
                using (var command = new SqliteCommand(selectCommandText, connection))
                {
                    // Execute the command and read the results.
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lastPlayedMaps.Add(reader.GetInt32(0));
                        }
                    }
                }
            });

            // Wait for all enqueued database operations to complete.
            await DbQueue.Instance.WhenAllCompleted();

            return lastPlayedMaps;
        }
        public async Task<List<int>> GetLastPlayedRandomFromDbAsync()
        {
            List<int> lastPlayedMaps = new List<int>();

            // SQL command to select the MapId column from the LastPlayedMaps table.
            string selectCommandText = @"
                SELECT MapId
                FROM LastPlayedMaps
                WHERE Mode = 'Random'
                ORDER BY PlayedDateTime DESC
                LIMIT 10;
            ";

            // Enqueue the command to be executed on the database connection.
            DbQueue.Instance.Enqueue(async connection =>
            {
                using (var command = new SqliteCommand(selectCommandText, connection))
                {
                    // Execute the command and read the results.
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lastPlayedMaps.Add(reader.GetInt32(0));
                        }
                    }
                }
            });

            // Wait for all enqueued database operations to complete.
            await DbQueue.Instance.WhenAllCompleted();

            return lastPlayedMaps;
        }
        public async Task UpdateLastPlayedMapsAsync(int mapId)
        {
            lastPlayedMapsInMemory.Add(mapId);
            if (lastPlayedMapsInMemory.Count > 10)
            {
                lastPlayedMapsInMemory.RemoveAt(0); 
                await MessageHelper.LogAsync("Removed oldest map from last played list");
            }
            MapHistoryUpdated?.Invoke();

            // Optionally, save to file immediately or wait to save at session end
            await MessageHelper.LogAsync($"Added map {mapId} to last played list");
            //await SaveLastPlayedMapsAsync();
            await AddLastPlayedMapAsync(mapId, "LastPlayed");


        }


        //Change to store in DB
        //create a table in database to store the last played maps
        public async Task CreateLastPlayedMapsTableAsync()
        {
            string createTableCommand = @"
        CREATE TABLE IF NOT EXISTS LastPlayedMaps (
            MapId INTEGER NOT NULL,
            PlayedDateTime TEXT NOT NULL,
            Mode STRING NOT NULL
        );";

            DbQueue.Instance.Enqueue(async connection =>
            {
                using (var command = new SqliteCommand(createTableCommand, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            });

            await DbQueue.Instance.WhenAllCompleted(); // Wait for the operation to complete
        }


        //method to add the last played map to the database and check if there are more maps than the count in appConfig
        public async Task AddLastPlayedMapAsync(int mapId, string mode)
        {
            // SQL command to insert a new row into the LastPlayedMaps table.
            string insertCommandText = @"
        INSERT INTO LastPlayedMaps (MapId, PlayedDateTime, Mode)
        VALUES (@MapId, @PlayedDateTime, @Mode);
    ";

            // Enqueue the insert command to be executed on the database connection.
            DbQueue.Instance.Enqueue(async connection =>
            {
                using (var command = new SqliteCommand(insertCommandText, connection))
                {
                    // Add parameters to the command to replace the placeholders in the SQL command text.
                    command.Parameters.AddWithValue("@MapId", mapId);
                    command.Parameters.AddWithValue("@PlayedDateTime", DateTime.UtcNow.ToString("s")); // Using a sortable date/time pattern.
                    command.Parameters.AddWithValue("@Mode", mode);

                    // Execute the command.
                    await command.ExecuteNonQueryAsync();
                }
            });

            // Wait for all enqueued database operations to complete.
            await DbQueue.Instance.WhenAllCompleted();

            // Check if the number of entries exceeds CountHistory and remove the oldest if necessary.
            await CheckAndRemoveOldestIfNecessary();
        }

        private async Task CheckAndRemoveOldestIfNecessary()
        {
            // SQL command to count the number of rows in the LastPlayedMaps table.
            string countCommandText = "SELECT COUNT(*) FROM LastPlayedMaps;";

            // SQL command to delete the oldest row based on PlayedDateTime.
            string deleteOldestCommandText = @"
        DELETE FROM LastPlayedMaps
        WHERE ROWID IN (
            SELECT ROWID FROM LastPlayedMaps
            ORDER BY PlayedDateTime ASC
            LIMIT 1
        );
    ";

            int currentCount = 0;
            int? countHistoryLimit = AppConfig.CountHistory;

            // Enqueue the count command to be executed on the database connection.
            DbQueue.Instance.Enqueue(async connection =>
            {
                using (var command = new SqliteCommand(countCommandText, connection))
                {
                    currentCount = Convert.ToInt32(await command.ExecuteScalarAsync());
                }

                // If the current count exceeds CountHistory, delete the oldest entry.
                if (countHistoryLimit.HasValue && currentCount > countHistoryLimit.Value)
                {
                    using (var deleteCommand = new SqliteCommand(deleteOldestCommandText, connection))
                    {
                        await deleteCommand.ExecuteNonQueryAsync();
                    }
                }
            });

            // Wait for all enqueued database operations to complete.
            await DbQueue.Instance.WhenAllCompleted();
        }


   

    }
}
