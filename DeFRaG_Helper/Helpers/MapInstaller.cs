using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeFRaG_Helper.ViewModels;

namespace DeFRaG_Helper
{
    class MapInstaller
    {
        public static async Task InstallMap(Map map)
        {

            // Prepare the progress handler to update the UI
            var progressHandler = new Progress<double>(value =>
            {
                App.Current.Dispatcher.Invoke(() => MainWindow.Instance.UpdateProgressBar(value));
            });
            IProgress<double> progress = progressHandler;
            //if the map is downloaded and installed, we will skip the download and install steps.
            if (map.IsDownloaded == 1 && map.IsInstalled == 1)
            {
                return;
            }           

            //check isDownloaded and isInstalled for the given map.in Maps Viewmodel. If none of them, we will download the map and install it.
                if (map.IsDownloaded == 0 && map.IsInstalled == 0)
            {
                //download the map


                await Downloader.DownloadFileAsync($"https://ws.q3df.org/maps/downloads/{map.Filename}", AppConfig.GameDirectoryPath + $"\\defrag\\{map.Filename}", progress);

            } else if (map.IsDownloaded == 1 && map.IsInstalled == 0)
            {
                //install the map from archive by it's filename

                System.IO.File.Move(AppConfig.GameDirectoryPath + $"\\archive\\{map.Filename}", AppConfig.GameDirectoryPath + $"\\defrag\\{map.Filename}"); 
            }
            map.IsDownloaded = 1;
            map.IsInstalled = 1;
            //update the map in Maps Viewmodel
            // Assuming GetInstanceAsync is an async method returning the singleton instance of MapViewModel
            var mapViewModel = await MapViewModel.GetInstanceAsync();
            await mapViewModel.UpdateMapFlagsAsync(map);


        }

        public static async void UninstallMap(Map map)
        {

           //check isInstalled for the given map in Maps Viewmodel. If it is installed, we will uninstall it.
            if (map.IsInstalled == 1)
            {
                //uninstall the map
                System.IO.File.Move($"defrag/{map.Filename}", $"archive/{map.Filename}");
            }
            map.IsInstalled = 0;
            //update the map in Maps Viewmodel
            // Assuming GetInstanceAsync is an async method returning the singleton instance of MapViewModel
            var mapViewModel = await MapViewModel.GetInstanceAsync();
            await mapViewModel.UpdateMapFlagsAsync(map);
        }
    
    }
}
