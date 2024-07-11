using System.Collections.ObjectModel;
using DeFRaG_Helper;
using Microsoft.Data.Sqlite;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Data;

namespace DeFRaG_Helper.ViewModels
{
    public class MapViewModel : INotifyPropertyChanged
    {
        public bool IsDataLoaded { get; private set; }
        public ICommand PlayMapCommand { get; private set; }
        public ICommand DownloadMapCommand { get; private set; }

        public ObservableCollection<Map> Maps { get; set; }
        private DbQueue dbQueue;
        private bool dataLoaded = false; // Flag to indicate if data has been loaded

        private static MapViewModel instance;
        private static bool isInitialized = false;
        // Step 2: Declare the PropertyChanged event
        public event PropertyChangedEventHandler PropertyChanged;

        // Step 3: Create the OnPropertyChanged method
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }









        private MapViewModel()
        {
            Maps = new ObservableCollection<Map>();


            dbQueue = new DbQueue($"Data Source={AppConfig.DatabasePath};");

            PlayMapCommand = new RelayCommand(PlayMap);
            DownloadMapCommand = new RelayCommand(DownloadMap);
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

        private async void PlayMap(object mapParameter)
        {
           //MessageBox.Show("Play Map");
            var mainWindow = Application.Current.MainWindow as MainWindow;

            var map = mapParameter as Map;
            if (map != null)
            {
                await MapInstaller.InstallMap(map);



                int physicsSetting = mainWindow.GetPhysicsSetting();
                System.Diagnostics.Process.Start(AppConfig.GameDirectoryPath + "\\oDFe.x64.exe", $"+set fs_game defrag +df_promode {physicsSetting} +map {System.IO.Path.GetFileNameWithoutExtension(map.MapName)}");


            }
        }

        private async void DownloadMap(object mapParameter)
        {
            var map = mapParameter as Map;
            if (map != null)
            {
                await MapInstaller.InstallMap(map);

            }
        }




        private async Task LoadMapsFromDatabase()
        {
            var totalMaps = 0;
            if (dataLoaded) return;

            dbQueue.Enqueue(async connection =>
            {
                // First, determine the total number of maps
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
                                IsDownloaded = reader.IsDBNull(reader.GetOrdinal("isDownloaded")) ? 0 : reader.GetInt32(reader.GetOrdinal("isDownloaded")),
                                IsInstalled = reader.IsDBNull(reader.GetOrdinal("isInstalled")) ? 0 : reader.GetInt32(reader.GetOrdinal("isInstalled")),
                                IsFavorite = reader.IsDBNull(reader.GetOrdinal("isFavorite")) ? 0 : reader.GetInt32(reader.GetOrdinal("isFavorite"))
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

                App.Current.Dispatcher.Invoke(() =>
                {
                    dataLoaded = true; // Set the flag to true after loading data
                    DataLoaded?.Invoke(this, EventArgs.Empty); // Raise the event
                    App.Current.Dispatcher.Invoke(() => MainWindow.Instance.HideProgressBar());
                    // Display the completion message
                    //Test refresh
                    
                    
                });

            });

            //we will call it somewhere else
            //await StartSync(totalMaps);



        }

        private async Task StartSync(int totalMaps) 
        {
            await dbQueue.WhenAllCompleted(); // Wait for all operations to complete

            // After loading data, raise the DataLoaded event on the UI thread
            IProgress<double> syncProgressReporter = new Progress<double>(progress =>
            {
                App.Current.Dispatcher.Invoke(() => MainWindow.Instance.UpdateProgressBar(progress));
            });
            App.Current.Dispatcher.Invoke(() => MainWindow.Instance.ShowMessage(totalMaps + " Maps loaded"));


            // 2. Create an instance of MapFileSyncService
            MapFileSyncService syncService = new MapFileSyncService();
            syncService.OnMapFlagsChanged += async map => await UpdateMapFlagsAsync(map);
            syncService.SyncMapFilesWithFileSystem(Maps, syncProgressReporter);

        }
        public async Task UpdateMapFlagsAsync(Map map)
        {
            // Find the map in the collection
            var existingMap = Maps.FirstOrDefault(m => m.Id == map.Id);
            if (existingMap != null)
            {
                // Update properties in memory
                existingMap.IsDownloaded = map.IsDownloaded;
                existingMap.IsInstalled = map.IsInstalled;
                // Other properties as needed

                // Enqueue database update operation
                dbQueue.Enqueue(async connection =>
                {
                    using (var command = new SqliteCommand("UPDATE Maps SET isDownloaded = @isDownloaded, isInstalled = @isInstalled WHERE ID = @ID", connection))
                    {
                        command.Parameters.AddWithValue("@isDownloaded", map.IsDownloaded ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@isInstalled", map.IsInstalled ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ID", map.Id);
                        Debug.WriteLine(command);
                        await command.ExecuteNonQueryAsync();
                    }
                });
            }
        }

        //method to update favorite state in map
        public async Task UpdateFavoriteStateAsync(Map map)
        {
            // Find the map in the collection
            var existingMap = Maps.FirstOrDefault(m => m.Id == map.Id);
            if (existingMap != null)
            {
                // Update properties in memory
                existingMap.IsFavorite = map.IsFavorite;
                // Other properties as needed

                // Enqueue database update operation
                dbQueue.Enqueue(async connection =>
                {
                    using (var command = new SqliteCommand("UPDATE Maps SET isFavorite = @isFavorite WHERE ID = @ID", connection))
                    {
                        command.Parameters.AddWithValue("@isFavorite", map.IsFavorite ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ID", map.Id);

                        await command.ExecuteNonQueryAsync();
                    }
                });
            }
        }
  






    }
}
