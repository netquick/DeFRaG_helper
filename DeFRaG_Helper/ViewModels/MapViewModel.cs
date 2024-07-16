using System.Collections.ObjectModel;
using DeFRaG_Helper;
using Microsoft.Data.Sqlite;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Data;
using System.IO;

namespace DeFRaG_Helper.ViewModels
{
    public class MapViewModel : INotifyPropertyChanged
    {
        public bool IsDataLoaded { get; private set; }
        public ICommand PlayMapCommand { get; private set; }
        public ICommand DownloadMapCommand { get; private set; }
        public event EventHandler MapsBatchLoaded;
        public ObservableCollection<Map> Maps { get; set; }
        // Centralized DbQueue instance
        private static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string AppDataFolder = Path.Combine(AppDataPath, "DeFRaG_Helper");
        private static readonly string DbPath = Path.Combine(AppDataFolder, "MapData.db");

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


        public IEnumerable<Map> FilterMaps(string filterText)
        {
            if (string.IsNullOrWhiteSpace(filterText))
            {
                return Maps;
            }

            return Maps.Where(m => m.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase));
        }

        private int _filteredMapsCount;
        public int FilteredMapsCount
        {
            get => _filteredMapsCount;
            set
            {
                if (_filteredMapsCount != value)
                {
                    _filteredMapsCount = value;
                    OnPropertyChanged(nameof(FilteredMapsCount)); // Make sure your ViewModel implements INotifyPropertyChanged
                }
            }
        }

        // In MapViewModel or a shared ViewModel
        private Map _selectedMap;
        public Map SelectedMap
        {
            get => _selectedMap;
            set
            {
                _selectedMap = value;
                OnPropertyChanged(nameof(SelectedMap)); // Notify if you're implementing INotifyPropertyChanged
            }
        }




        private MapViewModel()
        {
            Maps = new ObservableCollection<Map>();


            //dbQueue = new DbQueue($"Data Source={AppConfig.DatabasePath};");

            PlayMapCommand = new RelayCommand(PlayMap);
            DownloadMapCommand = new RelayCommand(DownloadMap);
        }
        public static async Task<MapViewModel> GetInstanceAsync()
        {
            if (instance == null)
            {
                if (!isInitialized)
                {
                    instance = new MapViewModel();
                    await instance.InitializeAsync();
                    isInitialized = true;
                }
            }
            return instance;
        }


        private async Task InitializeAsync()
        {
            if (!isInitialized)
            {

                await LoadMapsFromDatabase();

                // Assuming MapFileSyncService is a singleton or accessible instance
                MapFileSyncService syncService = MapFileSyncService.Instance;
                syncService.MapFlagsChanged += SyncService_MapFlagsChanged;



                isInitialized = true;
            }
        }
        private async void SyncService_MapFlagsChanged(object sender, MapFlagChangedEventArgs e)
        {
            await UpdateMapFlagsAsync(e.UpdatedMap);
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
                System.Diagnostics.Process.Start(AppConfig.GameDirectoryPath + "\\oDFe.x64.exe", $"+set fs_game defrag +df_promode {physicsSetting} +map {System.IO.Path.GetFileNameWithoutExtension(map.Mapname)}");


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

            DbQueue.Instance.Enqueue(async connection =>
            {
                // First, determine the total number of maps
                using (var countCommand = new SqliteCommand("SELECT COUNT(*) FROM Maps", connection))
                {
                    totalMaps = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
                }

                int loadedMaps = 0;
                int refreshThreshold = 10; // Number of maps after which the UI is refreshed

                // Existing database loading logic...
                using (var command = new SqliteCommand(@"SELECT 
                    Maps.ID, 
                    Maps.Name, 
                    Maps.Mapname, 
                    Maps.Filename, 
                    Maps.Releasedate, 
                    Maps.Author, 
                    Maps.Mod, 
                    Maps.Size, 
                    Maps.Physics, 
                    Maps.Hits, 
                    Maps.LinkDetailpage, 
                    Maps.Style, 
                    Maps.isDownloaded, 
                    Maps.isInstalled, 
                    Maps.isFavorite, 
                    GROUP_CONCAT(DISTINCT Weapon.Weapon) AS Weapons, 
                    GROUP_CONCAT(DISTINCT Item.Item) AS Items, 
                    GROUP_CONCAT(DISTINCT Function.Function) AS Functions
                FROM 
                    Maps 
                LEFT JOIN 
                    MapWeapon ON Maps.ID = MapWeapon.MapID 
                LEFT JOIN 
                    Weapon ON MapWeapon.WeaponID = Weapon.WeaponID
                LEFT JOIN 
                    MapItem ON Maps.ID = MapItem.MapID 
                LEFT JOIN 
                    Item ON MapItem.ItemID = Item.ItemID
                LEFT JOIN 
                    MapFunction ON Maps.ID = MapFunction.MapID 
                LEFT JOIN 
                    Function ON MapFunction.FunctionID = Function.FunctionID
                GROUP BY 
                    Maps.ID, 
                    Maps.Name, 
                    Maps.Mapname, 
                    Maps.Filename, 
                    Maps.Releasedate, 
                    Maps.Author, 
                    Maps.Mod, 
                    Maps.Size, 
                    Maps.Physics, 
                    Maps.Hits, 
                    Maps.LinkDetailpage, 
                    Maps.Style, 
                    Maps.isDownloaded, 
                    Maps.isInstalled, 
                    Maps.isFavorite
                ", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var map = new Map
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ID")),
                                Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetString(reader.GetOrdinal("Name")),
                                Mapname = reader.IsDBNull(reader.GetOrdinal("Mapname")) ? null : reader.GetString(reader.GetOrdinal("Mapname")),
                                Filename = reader.IsDBNull(reader.GetOrdinal("Filename")) ? null : reader.GetString(reader.GetOrdinal("Filename")),
                                Releasedate = reader.IsDBNull(reader.GetOrdinal("Releasedate")) ? null : reader.GetString(reader.GetOrdinal("Releasedate")),
                                Author = reader.IsDBNull(reader.GetOrdinal("Author")) ? null : reader.GetString(reader.GetOrdinal("Author")),
                                GameType = reader.IsDBNull(reader.GetOrdinal("Mod")) ? null : reader.GetString(reader.GetOrdinal("Mod")),
                                Size = reader.IsDBNull(reader.GetOrdinal("Size")) ? 0 : reader.GetInt64(reader.GetOrdinal("Size")),
                                Physics = reader.IsDBNull(reader.GetOrdinal("Physics")) ? 0 : reader.GetInt32(reader.GetOrdinal("Physics")),
                                Hits = reader.IsDBNull(reader.GetOrdinal("Hits")) ? 0 : reader.GetInt32(reader.GetOrdinal("Hits")),
                                LinkDetailpage = reader.IsDBNull(reader.GetOrdinal("LinkDetailpage")) ? null : reader.GetString(reader.GetOrdinal("LinkDetailpage")),
                                Style = reader.IsDBNull(reader.GetOrdinal("Style")) ? null : reader.GetString(reader.GetOrdinal("Style")),
                                IsDownloaded = reader.IsDBNull(reader.GetOrdinal("isDownloaded")) ? 0 : reader.GetInt32(reader.GetOrdinal("isDownloaded")),
                                IsInstalled = reader.IsDBNull(reader.GetOrdinal("isInstalled")) ? 0 : reader.GetInt32(reader.GetOrdinal("isInstalled")),
                                IsFavorite = reader.IsDBNull(reader.GetOrdinal("isFavorite")) ? 0 : reader.GetInt32(reader.GetOrdinal("isFavorite")),
                                Weapons = reader.IsDBNull(reader.GetOrdinal("Weapons")) ? new List<string>() : reader.GetString(reader.GetOrdinal("Weapons")).Split(',').ToList(),
                                Items = reader.IsDBNull(reader.GetOrdinal("Items")) ? new List<string>() : reader.GetString(reader.GetOrdinal("Items")).Split(',').ToList(),
                                Functions = reader.IsDBNull(reader.GetOrdinal("Functions")) ? new List<string>() : reader.GetString(reader.GetOrdinal("Functions")).Split(',').ToList(),


                            };



                            // Since we're on a background thread, ensure UI updates are dispatched on the UI thread
                            App.Current.Dispatcher.Invoke(() => Maps.Add(map));

                            loadedMaps++;
                            if (loadedMaps % refreshThreshold == 0 || loadedMaps == totalMaps)
                            {
                                double progress = (double)loadedMaps / totalMaps * 100;
                                // Update progress bar and potentially refresh the UI here
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    map.GenerateWeaponIcons();
                                    map.GenerateItemIcons();
                                    map.GenerateFunctionIcons();

                                    MainWindow.Instance.UpdateProgressBar(progress);
                                    MapsBatchLoaded?.Invoke(this, EventArgs.Empty);

                                    // Optionally, refresh the UI to show the newly loaded maps
                                });
                                OnPropertyChanged(nameof(Maps));

                            }
                        }
                    }
                }

                await App.Current.Dispatcher.InvokeAsync(async () =>
                {
                    dataLoaded = true; // Set the flag to true after loading data
                    DataLoaded?.Invoke(this, EventArgs.Empty); // Raise the event
                                                               //MainWindow.Instance.HideProgressBar();
                    await PerformPostDataLoadActions();
                    FilteredMapsCount = Maps.Count;

                    // Optionally, do a final UI refresh here if needed
                });


            });
            await Task.CompletedTask;

        }









        private async Task PerformPostDataLoadActions()
        {
            // Ensure that BackgroundTaskRunner and CreateAndUpdateDB methods are awaited
            BackgroundTaskRunner backgroundTaskRunner = new BackgroundTaskRunner();
            await backgroundTaskRunner.RunTaskAsync();
            await CreateAndUpdateDB.UpdateDB();

            // Any other actions that need to be performed after data is loaded
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
                DbQueue.Instance.Enqueue(async connection =>
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
                await Task.CompletedTask;

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
                DbQueue.Instance.Enqueue(async connection =>
                {
                    using (var command = new SqliteCommand("UPDATE Maps SET isFavorite = @isFavorite WHERE ID = @ID", connection))
                    {
                        command.Parameters.AddWithValue("@isFavorite", map.IsFavorite ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ID", map.Id);

                        await command.ExecuteNonQueryAsync();
                    }
                });
                await Task.CompletedTask;

            }
        }

        // Assuming map is an instance of your Map object filled with the necessary data
        // Change the method to be an instance method instead of a static method
        public async Task UpdateOrAddMap(Map map)
        {
            try
            {
                // Adjust the existence check to consider both Mapname and Filename
                var existingMap = Maps.FirstOrDefault(m => m.Mapname == map.Mapname && m.Filename == map.Filename);

                if (existingMap != null)
                {
                    // Map exists, update its properties
                    UpdateMapProperties(existingMap, map);

                    // Enqueue database update operation
                    DbQueue.Instance.Enqueue(async connection =>
                    {
                        // Assuming you have a method to create and execute the SQL command for updating
                        await DbActions.Instance.UpdateMap(existingMap);
                    });
                    await Task.CompletedTask;

                }
                else
                {
                    // Map does not exist, add it to the collection
                    App.Current.Dispatcher.Invoke(() => Maps.Add(map));

                    // Enqueue database insert operation
                    DbQueue.Instance.Enqueue(async connection =>
                    {
                        // Assuming you have a method to create and execute the SQL command for inserting
                        await DbActions.Instance.AddMap(map);
                    });
                    await Task.CompletedTask;

                }
            }
            catch (Exception ex)
            {
                MessageHelper.Log($"Error updating or adding map: {ex.Message}");
            }
        }

        private void UpdateMapProperties(Map existingMap, Map newMap)
        {
            // Update the properties of existingMap with those from newMap
            existingMap.Name = newMap.Name;
            existingMap.Mapname = newMap.Mapname;
            existingMap.Filename = newMap.Filename;
            existingMap.Releasedate = newMap.Releasedate;
            existingMap.Author = newMap.Author;
            existingMap.GameType = newMap.GameType;
            existingMap.Size = newMap.Size;
            existingMap.Physics = newMap.Physics;
            existingMap.Hits = newMap.Hits;
            existingMap.LinkDetailpage = newMap.LinkDetailpage;
            existingMap.Style = newMap.Style;
            existingMap.LinksOnlineRecordsQ3DFVQ3 = newMap.LinksOnlineRecordsQ3DFVQ3;
            existingMap.LinksOnlineRecordsQ3DFCPM = newMap.LinksOnlineRecordsQ3DFCPM;
            existingMap.LinksOnlineRecordsRacingVQ3 = newMap.LinksOnlineRecordsRacingVQ3;
            existingMap.LinksOnlineRecordsRacingCPM = newMap.LinksOnlineRecordsRacingCPM;
            existingMap.LinkDemosVQ3 = newMap.LinkDemosVQ3;
            existingMap.LinkDemosCPM = newMap.LinkDemosCPM;
            existingMap.Dependencies = newMap.Dependencies;
            existingMap.Weapons = newMap.Weapons;
            existingMap.Items = newMap.Items;
            existingMap.Functions = newMap.Functions;



            // Notify UI of property changes
            OnPropertyChanged(nameof(Maps));
        }

        // Example placeholder methods for database operations
   








    }
}
