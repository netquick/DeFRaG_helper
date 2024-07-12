using DeFRaG_Helper.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DeFRaG_Helper
{
    public class MapFlagChangedEventArgs : EventArgs
    {
        public Map UpdatedMap { get; }

        public MapFlagChangedEventArgs(Map updatedMap)
        {
            UpdatedMap = updatedMap;
        }
    }
    public class MapFileSyncService
    {
        private static MapFileSyncService instance;
        public static MapFileSyncService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MapFileSyncService();
                }
                return instance;
            }
        }

        public event EventHandler<MapFlagChangedEventArgs> MapFlagsChanged;

        protected virtual void OnMapFlagsChanged(Map updatedMap)
        {
            MapFlagsChanged?.Invoke(this, new MapFlagChangedEventArgs(updatedMap));
        }


        public async Task SyncMapFilesWithFileSystem(IEnumerable<Map> maps, IProgress<double> progress)
        {
            int totalMaps = maps.Count();
            int processedMaps = 0;
            await App.Current.Dispatcher.InvokeAsync(() => MainWindow.Instance.UpdateProgressBar(0));

            foreach (var map in maps)
            {
                // Track initial states
                int initialIsDownloaded = map.IsDownloaded ?? 0;
                int initialIsInstalled = map.IsInstalled ?? 0;
                // Check in the "defrag" folder
                if (map.FileName != null)
                {
                    await Task.Run(() =>
                    {
                        if (AppConfig.GameDirectoryPath == null || (map.FileName == null))
                        {
                            MessageBox.Show($"Game directory path {AppConfig.GameDirectoryPath} or map filename {map.FileName} is not set in the configuration file.");
                        }

                        string defragFilePath = System.IO.Path.Combine(AppConfig.GameDirectoryPath, "defrag", map.FileName);
                        if (System.IO.File.Exists(defragFilePath))
                        {
                            map.IsDownloaded = 1;
                            map.IsInstalled = 1; // Assuming you want to set IsInstalled when found in "defrag"
                        }
                        else
                        {
                            // If not found in "defrag", check in the "archive" folder
                            string archiveFilePath = System.IO.Path.Combine(AppConfig.GameDirectoryPath, "archive", map.FileName);
                            if (System.IO.File.Exists(archiveFilePath))
                            {
                                map.IsDownloaded = 1;
                                map.IsInstalled = 0;
                                // Do not modify IsInstalled here, as it's only checked in the "defrag" folder
                            }
                            else
                            {
                                // If not found in either, set IsDownloaded to 0
                                map.IsDownloaded = 0;
                                // Optionally, reset IsInstalled if you want to ensure it reflects current state
                                map.IsInstalled = 0;
                            }
                        }
                        // After modification, check if there's a change
                        if (map.IsDownloaded != initialIsDownloaded || map.IsInstalled != initialIsInstalled)
                        {
                            // If there's a change, invoke the delegate to update the database. Call UpdateMapFlagsAsync method in MapViewModel

                            OnMapFlagsChanged(map);

                        }

                    });
                    processedMaps++;
                    
                }

                // Update progress every N maps to reduce UI thread marshalling
                int updateFrequency = 100; // Adjust based on performance
                if (processedMaps % updateFrequency == 0 || processedMaps == totalMaps)
                {
                    double progressPercentage = (double)processedMaps / totalMaps * 100;
                    App.Current.Dispatcher.BeginInvoke(new Action(() => MainWindow.Instance.UpdateProgressBar(progressPercentage)));
                    progress?.Report(progressPercentage);

                }
                Debug.WriteLine($"Map: {map.MapName} Filename: {map.FileName}");
            }
        }



    }
}
