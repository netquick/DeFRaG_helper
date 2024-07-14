using DeFRaG_Helper.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeFRaG_Helper
{
    public class BackgroundTaskRunner
    {


        public BackgroundTaskRunner()
        {
            Initialize();
        }

        private async void Initialize()
        {
            var viewModel = await MapViewModel.GetInstanceAsync();
            // Subscribe to the DataLoaded event
            viewModel.DataLoaded += ViewModel_DataLoaded;
        }

        private async void ViewModel_DataLoaded(object sender, EventArgs e)
        {
            // Optionally, unsubscribe if you only need to run this once
            // ((MapViewModel)sender).DataLoaded -= ViewModel_DataLoaded;

            // Maps are loaded, now start the background task
            await RunTaskAsync();
        }

        public async Task RunTaskAsync()
        {
            ShowMessage("Starting background task");
            await CheckInstallState();       
            //implement sync with files and update db tasks here

        }

        private void ShowMessage(string message)
        {
            App.Current.Dispatcher.Invoke(() => { MainWindow.Instance.ShowMessage(message); });

        }

        private async Task CheckInstallState()
        {
            ShowMessage($"Checking install state for maps in Game directory");
            var viewModel = await MapViewModel.GetInstanceAsync();
            int totalMaps = viewModel.Maps.Count;
            int processedMaps = 0;

            foreach (var map in viewModel.Maps)
            {
                var mapPath = AppConfig.GameDirectoryPath + "\\defrag\\" + map.Filename;
                var archivePath = AppConfig.GameDirectoryPath + "\\archive\\" + map.Filename;
                var mapChanged = false;

                bool existsInDefrag = System.IO.File.Exists(mapPath);
                bool existsInArchive = System.IO.File.Exists(archivePath);

                // Check if the map exists in the defrag directory
                if (existsInDefrag)
                {
                    if (map.IsInstalled == 0) { map.IsInstalled = 1; mapChanged = true; }
                    if (map.IsDownloaded == 0) { map.IsDownloaded = 1; mapChanged = true; }
                }

                // Check if the map exists in the archive directory
                if (existsInArchive && !existsInDefrag)
                {
                    if (map.IsDownloaded == 0) { map.IsDownloaded = 1; mapChanged = true; }
                    if (map.IsInstalled != 0) { map.IsInstalled = 0; mapChanged = true; }
                }

                // If the map doesn't exist in either directory, set both to 0
                if (!existsInDefrag && !existsInArchive)
                {
                    if (map.IsInstalled != 0) { map.IsInstalled = 0; mapChanged = true; }
                    if (map.IsDownloaded != 0) { map.IsDownloaded = 0; mapChanged = true; }
                }

                // Optionally, update the map in the ViewModel if it changed
                if (mapChanged)
                {
                    ShowMessage($"Updating map flags for {map.Mapname}");
                    await viewModel.UpdateMapFlagsAsync(map);
                }

                processedMaps++;

                // Update UI only after every 10 maps processed or if it's the last map
                if (processedMaps % 10 == 0 || processedMaps == totalMaps)
                {
                    double progress = (double)processedMaps / totalMaps * 100;
                    App.Current.Dispatcher.Invoke(() => { MainWindow.Instance.UpdateProgressBar(progress); });
                }
            }
        }




    }


}
