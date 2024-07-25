using DeFRaG_Helper.Helpers;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace DeFRaG_Helper.ViewModels
{
    /// <summary>
    /// MapViewModel class to handle map data and operations for all the maps used in the application.
    /// </summary>
    public class MapViewModel : INotifyPropertyChanged
    {
        public ICommand PlayMapCommand { get; private set; }
        public ICommand DownloadMapCommand { get; private set; }
        public ICommand EditMapCommand { get; private set; }
        public event EventHandler MapsBatchLoaded;
        public ObservableCollection<Map> Maps { get; set; }
        private static readonly object _lock = new object();

        private bool dataLoaded = false;
        private MapHistoryManager mapHistoryManager;
        private static MapViewModel instance;
        private static bool isInitialized = false;
        public event PropertyChangedEventHandler PropertyChanged;

     

        private MapViewModel()
        {
            Maps = new ObservableCollection<Map>();
            FilteredMapsCount = Maps.Count; // Initialize with the total count


            //dbQueue = new DbQueue($"Data Source={AppConfig.DatabasePath};");

            PlayMapCommand = new RelayCommand(PlayMap);
            DownloadMapCommand = new RelayCommand(DownloadMap);
            EditMapCommand = new RelayCommand(EditMapCommandAction);

            var mapHistoryManager = MapHistoryManager.Instance;;

        }
        public static async Task<MapViewModel> GetInstanceAsync()
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    if (instance == null)
                    {
                        instance = new MapViewModel();
                    }
                }
                await instance.InitializeAsync().ConfigureAwait(false);
                isInitialized = true;
            }
            return instance;
        }

        private async Task InitializeAsync()
        {
            if (!isInitialized)
            {
                MapFileSyncService syncService = MapFileSyncService.Instance;
                syncService.MapFlagsChanged += SyncService_MapFlagsChanged;
                mapHistoryManager = MapHistoryManager.Instance;

                await LoadMapsIntelligent();

                isInitialized = true;
            }
        }



        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                ApplyFilters();
            }
        }

        private bool _showFavorites;
        public bool ShowFavorites
        {
            get => _showFavorites;
            set
            {
                _showFavorites = value;
                OnPropertyChanged(nameof(ShowFavorites));
                ApplyFilters();
            }
        }

        private bool _showInstalled;
        public bool ShowInstalled
        {
            get => _showInstalled;
            set
            {
                _showInstalled = value;
                OnPropertyChanged(nameof(ShowInstalled));
                ApplyFilters();
            }
        }

        private bool _showDownloaded;
        public bool ShowDownloaded
        {
            get => _showDownloaded;
            set
            {
                _showDownloaded = value;
                OnPropertyChanged(nameof(ShowDownloaded));
                ApplyFilters();
            }
        }

        public ObservableCollection<Map> DisplayedMaps { get; set; } = new ObservableCollection<Map>();

        private async void ApplyFilters()
        {
            await Task.Run(() =>
            {
                var filteredMaps = Maps.Where(map =>
                    (string.IsNullOrEmpty(SearchText) || map.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) &&
                    (!ShowFavorites || map.IsFavorite == 1) &&
                    (!ShowInstalled || map.IsInstalled == 1) &&
                    (!ShowDownloaded || map.IsDownloaded == 1))
                    .OrderByDescending(map => map.Releasedate)
                    .ToList();

                App.Current.Dispatcher.Invoke(() =>
                {
                    DisplayedMaps.Clear();
                    LoadDisplayedMapsSubset(filteredMaps, 0, 100);
                    FilteredMapsCount = filteredMaps.Count; // Update the count with the total filtered maps count
                });
            });
        }


        public void LoadDisplayedMapsSubset(List<Map> sourceMaps, int startIndex, int count)
        {
            for (int i = startIndex; i < Math.Min(startIndex + count, sourceMaps.Count); i++)
            {
                DisplayedMaps.Add(sourceMaps[i]);
            }
        }



        private Map _selectedMap;
        public Map SelectedMap
        {
            get => _selectedMap;
            set
            {
                _selectedMap = value;
                OnPropertyChanged(nameof(SelectedMap)); // Notify if you're implementing INotifyPropertyChanged

                if (value != null)
                {
                    SimpleLogger.Log($"Selected map: {value.Name}");
                }
                else
                {
                    SimpleLogger.Log("Selected map is null.");
                }
            }
        }
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

        private void UpdateFilteredMapsCount()
        {
            if (string.IsNullOrEmpty(SearchText) && !ShowFavorites && !ShowInstalled && !ShowDownloaded)
            {
                FilteredMapsCount = Maps.Count; // Show total count when no filter is applied
            }
            else
            {
                FilteredMapsCount = DisplayedMaps.Count; // Show filtered count
            }
        }


        private async void SaveAppConfigChangesAsync()
        {
            try
            {
                await AppConfig.SaveConfigurationAsync();
            }
            catch (Exception ex)
            {
                // Handle exceptions, possibly logging them or notifying the user
                Debug.WriteLine($"Failed to save AppConfig changes: {ex.Message}");
            }
        }
        public async Task UpdateConfigurationAsync(Map selectedMap)
        {
            // Assuming AppConfig has a method to update the selected map and save the configuration
            AppConfig.SelectedMap = selectedMap.Id; // Or any other relevant property of Map
            await AppConfig.SaveConfigurationAsync(); // Save the updated configuration
        }


        private async void SyncService_MapFlagsChanged(object sender, MapFlagChangedEventArgs e)
        {
            await UpdateMapFlagsAsync(e.UpdatedMap);
            ApplyFilters(); // Call ApplyFilters to update the displayed maps
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
            var mainWindow = Application.Current.MainWindow as MainWindow;

            var map = mapParameter as Map;
            if (map != null)
            {
                await MapInstaller.InstallMap(map);

                mapHistoryManager = MapHistoryManager.Instance;;

                await mapHistoryManager.AddLastPlayedMapAsync(map.Id, "Normal");

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
        private async void EditMapCommandAction(object mapParameter)
        {
            var map = mapParameter as Map;
            if (map != null)
            {
                await EditMap.Instance.ConvertMap(map);
            }
        }


        private async Task LoadMapsIntelligent()
        {
            if (dataLoaded) return;

            var totalMaps = await GetTotalMapCountAsync();
            var loadedMaps = 0;
            var refreshThreshold = 100; // Adjust this value based on performance testing

            // Load last played maps first
            var lastPlayedMapIds = await mapHistoryManager.GetLastPlayedMapsFromDbAsync();
            foreach (var mapId in lastPlayedMapIds)
            {
                var map = await GetMapByIdAsync(mapId);
                if (map != null)
                {
                    App.Current.Dispatcher.Invoke(() => Maps.Add(map));
                    loadedMaps++;
                }
            }

            // Refresh UI immediately after loading history maps
            CheckAndRefreshUI(loadedMaps, totalMaps, refreshThreshold, true); // Passing 0 to force refresh

            // Fetch all map IDs and exclude the IDs of the last played maps
            var allMapIds = await GetAllMapIdsAsync();
            var remainingMapIds = allMapIds.Except(lastPlayedMapIds);

            // Load the rest of the maps with threshold-based refresh
            foreach (var mapId in remainingMapIds)
            {
                var map = await GetMapByIdAsync(mapId);
                if (map != null)
                {
                    App.Current.Dispatcher.Invoke(() => Maps.Add(map));
                    loadedMaps++;
                    // Check if the loadedMaps count has reached the refreshThreshold or if it's the last map
                    if (loadedMaps % refreshThreshold == 0 || loadedMaps == totalMaps)
                    {
                        CheckAndRefreshUI(loadedMaps, totalMaps, refreshThreshold);
                        // Calculate progress
                        int progress = (int)((double)loadedMaps / totalMaps * 100);

                        // Ensure the call is made on the UI thread
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MainWindow.Instance.UpdateProgressBar(progress);
                        });
                    }
                }
            }

            // Ensure UI is refreshed one last time in case the last batch didn't reach the threshold
            if (loadedMaps % refreshThreshold != 0)
            {
                CheckAndRefreshUI(loadedMaps, totalMaps, refreshThreshold);
            }
            await App.Current.Dispatcher.InvokeAsync(async () =>
            {
                await PerformPostDataLoadActions();
                FilteredMapsCount = Maps.Count;
            });
            dataLoaded = true;
            DataLoaded?.Invoke(this, EventArgs.Empty);
            ApplyFilters(); // Call ApplyFilters to update the displayed maps

        }


        private async Task<List<int>> GetAllMapIdsAsync()
        {
            var mapIds = new List<int>();
            DbQueue.Instance.Enqueue(async connection =>
            {
                using (var command = new SqliteCommand("SELECT ID FROM Maps", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            mapIds.Add(reader.GetInt32(0));
                        }
                    }
                }
            });

            await DbQueue.Instance.WhenAllCompleted();
            return mapIds;
        }

        private void CheckAndRefreshUI(int loadedMaps, int totalMaps, int refreshThreshold, bool forceRefresh = false)
        {
            if (forceRefresh || loadedMaps % refreshThreshold == 0 || loadedMaps == totalMaps)
            {
                double progress = (double)loadedMaps / totalMaps * 100;
                App.Current.Dispatcher.Invoke(() =>
                {
                    // Example: MainWindow.Instance.UpdateProgressBar(progress);
                    MapsBatchLoaded?.Invoke(this, EventArgs.Empty);
                });
                OnPropertyChanged(nameof(Maps));
            }
        }

        private async Task<int> GetTotalMapCountAsync()
        {
            int count = 0;
            // Enqueue the operation without awaiting here
            DbQueue.Instance.Enqueue(async connection =>
            {
                var command = new SqliteCommand("SELECT COUNT(*) FROM Maplist", connection);
                count = Convert.ToInt32(await command.ExecuteScalarAsync());
            });

            // Now, await the completion of all enqueued operations, including the one just added
            await DbQueue.Instance.WhenAllCompleted();

            return count;
        }

        private async Task<List<Map>> GetLastPlayedMaps()
        {
            var lastPlayedMapIds = await mapHistoryManager.GetLastPlayedMapsFromDbAsync(); // This returns List<int>
            var lastPlayedMaps = new List<Map>();

            foreach (var mapId in lastPlayedMapIds)
            {
                // Assuming you have a method to get a Map object by its ID
                var map = await GetMapByIdAsync(mapId);
                if (map != null)
                {
                    lastPlayedMaps.Add(map);
                }
            }

            return lastPlayedMaps;
        }

        private async Task<Map> GetMapByIdAsync(int mapId)
        {
            Map map = null;
            DbQueue.Instance.Enqueue(async connection =>
            {
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
            WHERE 
                Maps.ID = @mapId
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
                Maps.isFavorite", connection))
                {
                    command.Parameters.AddWithValue("@mapId", mapId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            map = new Map
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
                        }
                    }
                }
            });

            // Await the completion of all enqueued operations, including the one just added
            await DbQueue.Instance.WhenAllCompleted();

            return map;
        }

        private async Task PerformPostDataLoadActions()
        {
            BackgroundTaskRunner backgroundTaskRunner = new BackgroundTaskRunner();
            await backgroundTaskRunner.RunTaskAsync();
            await CreateAndUpdateDB.UpdateDB();

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

        public async Task UpdateOrAddMap(Map map)
        {
            try
            {
                var existingMap = Maps.FirstOrDefault(m => m.Mapname == map.Mapname && m.Filename == map.Filename);

                if (existingMap != null)
                {
                    // Map exists, update its properties
                    UpdateMapProperties(existingMap, map);

                    // Enqueue database update operation
                    DbQueue.Instance.Enqueue(async connection =>
                    {
                        await DbActions.Instance.UpdateMap(existingMap);
                    });
                }
                else
                {
                    // Map does not exist, add it to the collection
                    App.Current.Dispatcher.Invoke(() => Maps.Add(map));

                    // Enqueue database insert operation
                    DbQueue.Instance.Enqueue(async connection =>
                    {
                        await DbActions.Instance.AddMap(map);
                    });
                }

                // Call ApplyFilters to update the displayed maps
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageHelper.Log($"Error updating or adding map: {ex.Message}");
            }
        }


        private void UpdateMapProperties(Map existingMap, Map newMap)
        {
            // Update the properties of existingMap with those from newMap
            if (newMap.Name != null && newMap.Name != "")
            {
                existingMap.Name = newMap.Name;
            }
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
    }
}
