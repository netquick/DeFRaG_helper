using System.Collections.ObjectModel;
using DeFRaG_Helper;
using Microsoft.Data.Sqlite;
using System.Windows;
namespace DeFRaG_Helper.ViewModels
{
    public class MapViewModel
    {
        public bool IsDataLoaded { get; private set; }


        public ObservableCollection<Map> Maps { get; set; }
        private DbQueue dbQueue;
        private bool dataLoaded = false; // Flag to indicate if data has been loaded

        private static MapViewModel instance;
        private static bool isInitialized = false;


        private MapViewModel()
        {
            Maps = new ObservableCollection<Map>();
            dbQueue = new DbQueue("Data Source=MapData.db;");
        }
        public static async Task<MapViewModel> GetInstanceAsync()
        {
            if (instance == null)
            {
                instance = new MapViewModel();
                await instance.InitializeAsync();
            }
            return instance;
        }

        private async Task InitializeAsync()
        {
            if (!isInitialized)
            {
                await LoadMapsFromDatabase();
                isInitialized = true;
            }
        }
        public event EventHandler DataLoaded;

        // Public method to allow external subscription to the DataLoaded event
        public void SubscribeToDataLoaded(EventHandler handler)
        {
            DataLoaded += handler;
        }

        // Public method to allow external unsubscription from the DataLoaded event
        public void UnsubscribeFromDataLoaded(EventHandler handler)
        {
            DataLoaded -= handler;
        }

        private async Task LoadMapsFromDatabase()
        {
            if (dataLoaded) return;

            dbQueue.Enqueue(async connection =>
            {
                // First, determine the total number of maps
                int totalMaps = 0;
                using (var countCommand = new SqliteCommand("SELECT COUNT(*) FROM Maps", connection))
                {
                    totalMaps = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
                }

                int loadedMaps = 0;

                // Existing database loading logic...
                using (var command = new SqliteCommand("SELECT * FROM Maps", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            App.Current.Dispatcher.Invoke(() => MainWindow.Instance.ShowProgressBar());
                            var map = new Map
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ID")),
                                Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetString(reader.GetOrdinal("Name")),
                                MapName = reader.IsDBNull(reader.GetOrdinal("Mapname")) ? null : reader.GetString(reader.GetOrdinal("Mapname")),
                                FileName = reader.IsDBNull(reader.GetOrdinal("Filename")) ? null : reader.GetString(reader.GetOrdinal("Filename")),
                                ReleaseDate = reader.IsDBNull(reader.GetOrdinal("Releasedate")) ? null : reader.GetString(reader.GetOrdinal("Releasedate")),
                                Author = reader.IsDBNull(reader.GetOrdinal("Author")) ? null : reader.GetString(reader.GetOrdinal("Author")),
                                GameType = reader.IsDBNull(reader.GetOrdinal("Mod")) ? null : reader.GetString(reader.GetOrdinal("Mod")),
                                Size = reader.IsDBNull(reader.GetOrdinal("Size")) ? 0 : reader.GetInt64(reader.GetOrdinal("Size")),
                                Physics = reader.IsDBNull(reader.GetOrdinal("Physics")) ? 0 : reader.GetInt32(reader.GetOrdinal("Physics")),
                                Hits = reader.IsDBNull(reader.GetOrdinal("Hits")) ? 0 : reader.GetInt32(reader.GetOrdinal("Hits")),
                                Downloadlink = reader.IsDBNull(reader.GetOrdinal("LinkDetailpage")) ? null : reader.GetString(reader.GetOrdinal("LinkDetailpage")),
                                Style = reader.IsDBNull(reader.GetOrdinal("Style")) ? null : reader.GetString(reader.GetOrdinal("Style")),
                                // Add other properties as needed
                            };
                            // Since we're on a background thread, ensure UI updates are dispatched on the UI thread
                            App.Current.Dispatcher.Invoke(() => Maps.Add(map));
                            loadedMaps++;
                            double progress = (double)loadedMaps / totalMaps * 100;
                            App.Current.Dispatcher.Invoke(() => MainWindow.Instance.UpdateProgressBar(progress));
                        }
                    }
                }
                // After loading data, raise the DataLoaded event on the UI thread
                App.Current.Dispatcher.Invoke(() =>
                {
                    dataLoaded = true; // Set the flag to true after loading data
                    DataLoaded?.Invoke(this, EventArgs.Empty); // Raise the event
                    App.Current.Dispatcher.Invoke(() => MainWindow.Instance.HideProgressBar());
                    MainWindow.Instance.ShowMessage(totalMaps + " Maps loaded"); // Display the completion message


                });
            });
        }


        
    }
}
