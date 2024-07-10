using DeFRaG_Helper.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DeFRaG_Helper
{
    class CheckGameInstall
    {
        //start method to do all the checks
        public async static void StartChecks()
        {
            // Check if the game directory path is set in the AppConfig class
            SetGameDirectoryPath();

            // Await the fully initialized instance of MapViewModel
            var mapViewModelInstance = await MapViewModel.GetInstanceAsync();

            App.Current.Dispatcher.Invoke(() => MainWindow.Instance.LoadNavigationBar());
        }




        //first, we check if there is a folder called "defrag" in the directory where the application was launched
        public static bool CheckInstall(string path)
        {
            string[] dirs = System.IO.Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                if (dir.Contains("defrag"))
                {
                    return true;
                }
            }
            return false;
        }
        //if the folder is found, we check if there is a file called "oDFe.x64.exe" or "oDFe.exe" in the "defrag" folder
        public static bool CheckExe(string path)
        {
            //string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string[] files = System.IO.Directory.GetFiles(path);
            foreach (string file in files)
            {
                if (file.Contains("oDFe.x64.exe") || file.Contains("oDFe.exe"))
                {
                    return true;
                }
            }
            return false;
        }

        //first we check if the game directory is set in AppConfig. Else, if the folder and the file are found, we set the game directory path in the AppConfig class to the actual application path. If not, we ask the user to set the game directory path manually
        public async static void SetGameDirectoryPath()
        {

         
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (AppConfig.GameDirectoryPath == null)
            {
                if (CheckInstall(path) && CheckExe(path))
                {
                    AppConfig.GameDirectoryPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    //save the game directory path in the app.config file
                    await AppConfig.SaveConfigurationAsync();
                    App.Current.Dispatcher.Invoke(() => MainWindow.Instance.ShowMessage("Game found"));


                }
                else
                {
                    //prompt user to set game directory path in a folder browser dialog
                    //App.Current.Dispatcher.Invoke(() => MainWindow.Instance.ShowMessage("Game not found")); 
                    //open file browser dialog from wpf (as forms not working) to set game directory path
                    var tempDir = BrowseFolder();
                    if (tempDir != null)
                    {
                        AppConfig.GameDirectoryPath = tempDir;

                        //save the game directory path in the app.config file
                        await AppConfig.SaveConfigurationAsync();
                        //we need check again, if the folder and the file are found now, if yes we set the game directory path in the AppConfig class to the actual application path, if no, we install the game in method InstallGame()
                        path = tempDir;
                        
                        if (CheckInstall(path) && CheckExe(path))
                        {
                            AppConfig.GameDirectoryPath = path;
                            App.Current.Dispatcher.Invoke(() => MainWindow.Instance.ShowMessage("Game found"));
                            await AppConfig.SaveConfigurationAsync();
                        }
                        else
                        {
                            InstallGame();
                        }


                        //App.Current.Dispatcher.Invoke(() => MainWindow.Instance.ShowMessage("Game found"));
                        await AppConfig.SaveConfigurationAsync();
                    }
                    else
                    {
                        App.Current.Dispatcher.Invoke(() => MainWindow.Instance.ShowMessage("Game not found"));
                    }


                }
            }
        }

        //method to install the game
        private async static Task InstallGame()
        {
            App.Current.Dispatcher.Invoke(() => MainWindow.Instance.ShowMessage("Game data will be downloaded"));
            // Create a folder called "defrag" in the GameDirectoryPath
            string path = AppConfig.GameDirectoryPath;
            System.IO.Directory.CreateDirectory(path + "\\defrag");

            // Prepare the progress handler to update the UI
            var progressHandler = new Progress<double>(value =>
            {
                App.Current.Dispatcher.Invoke(() => MainWindow.Instance.UpdateProgressBar(value));
            });
            IProgress<double> progress = progressHandler;

            // Download the game from the server
            await Downloader.DownloadFileAsync("https://github.com/JBustos22/oDFe/releases/download/latest/oDFe-windows-x86_64.zip", path + "\\oDFe-windows-x86_64.zip", progress);
            // Extract the engine from the zip file
            await Downloader.UnpackFile(path + "\\oDFe-windows-x86_64.zip", path, progress);

            //check if the defrag folder already contains autoexec.cfg. if not we need install the gamedata
            if (!System.IO.File.Exists(path + "\\defrag\\autoexec.cfg"))
            {
                //Download the game data from
                await Downloader.DownloadFileAsync("https://dl.defrag.racing/downloads/game-bundles/DeFRaG%20Bundle%20all-in-one%20Windows%2064bit.7z", path + "\\DeFRaG Bundle all-in-one Windows 64bit.7z", progress);
                //Extract the game data from the 7z file
                await Downloader.UnpackFile(path + "\\DeFRaG Bundle all-in-one Windows 64bit.7z", path, progress);
                //move the contents of the extracted folder (DeFRaG Bundle all-in-one Windows 64bit) to the root folder
                await Downloader.MoveFolderContents(path + "\\DeFRaG Bundle all-in-one Windows 64bit", path, progress);
                //create "archive" folder in the root folder
                System.IO.Directory.CreateDirectory(path + "\\archive");

            }

            App.Current.Dispatcher.Invoke(() => MainWindow.Instance.ShowMessage("Game download completed"));
            App.Current.Dispatcher.Invoke(() => MainWindow.Instance.HideProgressBar());

        }



        public static string BrowseFolder()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ValidateNames = false;
            openFileDialog.CheckFileExists = false;
            openFileDialog.CheckPathExists = true;
            // Set the title of the dialog window
            openFileDialog.Title = "Select existing oDFe(.x64).exe location or chose an empty folder";
            // Set the filter to only show directories
            openFileDialog.Filter = "Folder|*.none";
            openFileDialog.FileName = "Select Folder";

            if (openFileDialog.ShowDialog() == true)
            {
                // Return the directory path
                return System.IO.Path.GetDirectoryName(openFileDialog.FileName);
            }
            else
            {
                return null;
            }
        }


    }
}
