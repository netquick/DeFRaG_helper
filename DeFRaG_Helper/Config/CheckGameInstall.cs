using DeFRaG_Helper.ViewModels;

namespace DeFRaG_Helper
{
    class CheckGameInstall
    {
        //start method to do all the checks
        public async static void StartChecks()
        {
            // Check if the game directory path is set in the AppConfig class
            MessageHelper.Log("Checking game install");
            SetGameDirectoryPath();

            // Await the fully initialized instance of MapViewModel
            var mapViewModelInstance = await MapViewModel.GetInstanceAsync();

            App.Current.Dispatcher.Invoke(() => MainWindow.Instance.LoadNavigationBar());
        }




        //first, we check if there is a folder called "defrag" in the directory where the application was launched
        public static bool CheckInstall(string path)
        {
            string[] dirs = System.IO.Directory.GetDirectories(path);
            MessageHelper.Log($"Checking for defrag folder in {path}");
            foreach (string dir in dirs)
            {
                if (dir.Contains("defrag"))
                {
                    MessageHelper.Log("Defrag folder found");
                    return true;
                }
            }
            MessageHelper.Log("Defrag folder not found");
            return false;
        }
        //if the folder is found, we check if there is a file called "oDFe.x64.exe" or "oDFe.exe" in the "defrag" folder
        public static bool CheckExe(string path)
        {
            //string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string[] files = System.IO.Directory.GetFiles(path);
            MessageHelper.Log($"Checking for oDFe.x64.exe or oDFe.exe in {path}");
            foreach (string file in files)
            {
                if (file.Contains("oDFe.x64.exe") || file.Contains("oDFe.exe"))
                {
                    MessageHelper.Log("oDFe.x64.exe or oDFe.exe found");
                    return true;
                }
            }
            MessageHelper.Log("oDFe.x64.exe or oDFe.exe not found");
            return false;
        }

        //first we check if the game directory is set in AppConfig. Else, if the folder and the file are found, we set the game directory path in the AppConfig class to the actual application path. If not, we ask the user to set the game directory path manually
        public async static void SetGameDirectoryPath()
        {

         
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(AppConfig.GameDirectoryPath))
            {
                MessageHelper.Log("Game directory path not set");
                if (CheckInstall(path) && CheckExe(path))
                {
                    MessageHelper.Log($"Game found in {path}");
                    AppConfig.GameDirectoryPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    //save the game directory path in the app.config file
                    await AppConfig.SaveConfigurationAsync();
                    MessageHelper.ShowMessage("Game found");


                }
                else
                {
                    MessageHelper.Log($"Game not found in {path}");
                    //prompt user to set game directory path in a folder browser dialog
                    //App.Current.Dispatcher.Invoke(() => MainWindow.Instance.ShowMessage("Game not found")); 
                    //open file browser dialog from wpf (as forms not working) to set game directory path
                    var tempDir = BrowseFolder();
                    if (tempDir != null)
                    {
                        AppConfig.GameDirectoryPath = tempDir;
                        MessageHelper.Log($"Game directory path set to {tempDir}");
                        //save the game directory path in the app.config file
                        await AppConfig.SaveConfigurationAsync();
                        MessageHelper.Log("Game directory path saved");
                        //we need check again, if the folder and the file are found now, if yes we set the game directory path in the AppConfig class to the actual application path, if no, we install the game in method InstallGame()
                        path = tempDir;
                        MessageHelper.Log($"Checking game install in {path}");
                        
                        if (CheckInstall(path) && CheckExe(path))
                        {
                            MessageHelper.Log($"Game found in {path}");
                            AppConfig.GameDirectoryPath = path;
                            MessageHelper.ShowMessage("Game found");
                            await AppConfig.SaveConfigurationAsync();
                        }
                        else
                        {
                            MessageHelper.Log($"Game not found in {path}");
                            InstallGame();
                        }


                        //App.Current.Dispatcher.Invoke(() => MainWindow.Instance.ShowMessage("Game found"));
                        await AppConfig.SaveConfigurationAsync();
                    }
                    else
                    {
                        MessageHelper.ShowMessage("Game not found");
                    }


                }
            }
            else
            {
                MessageHelper.Log($"Game directory path set to {AppConfig.GameDirectoryPath}");
            }
        }

        //method to install the game
        private async static Task InstallGame()
        {
            MessageHelper.ShowMessage("Game data will be downloaded");
            MessageHelper.Log("Game data will be downloaded");
            // Create a folder called "defrag" in the GameDirectoryPath
            string path = AppConfig.GameDirectoryPath;
            MessageHelper.Log($"Game directory path set to {path}");
            System.IO.Directory.CreateDirectory(path + "\\defrag");

            // Prepare the progress handler to update the UI
            var progressHandler = new Progress<double>(value =>
            {
                App.Current.Dispatcher.Invoke(() => MainWindow.Instance.UpdateProgressBar(value));
            });
            IProgress<double> progress = progressHandler;

            // Download the game from the server
            MessageHelper.Log("Downloading game data");
            await Downloader.DownloadFileAsync("https://github.com/JBustos22/oDFe/releases/download/latest/oDFe-windows-x86_64.zip", path + "\\oDFe-windows-x86_64.zip", progress);
            // Extract the engine from the zip file
            MessageHelper.Log($"Extracting game data in {path}");
            await Downloader.UnpackFile(path + "\\oDFe-windows-x86_64.zip", path, progress);

            //check if the defrag folder already contains autoexec.cfg. if not we need install the gamedata
            if (!System.IO.File.Exists(path + "\\defrag\\autoexec.cfg"))
            {
                MessageHelper.Log("Game data will be installed");
                //Download the game data from
                await Downloader.DownloadFileAsync("https://dl.defrag.racing/downloads/game-bundles/DeFRaG%20Bundle%20all-in-one%20Windows%2064bit.7z", path + "\\DeFRaG Bundle all-in-one Windows 64bit.7z", progress);
                //Extract the game data from the 7z file
                MessageHelper.Log("Extracting game data");
                await Downloader.UnpackFile(path + "\\DeFRaG Bundle all-in-one Windows 64bit.7z", path, progress);
                //move the contents of the extracted folder (DeFRaG Bundle all-in-one Windows 64bit) to the root folder
                MessageHelper.Log($"Moving game data in {path}");
                await Downloader.MoveFolderContents(path + "\\DeFRaG Bundle all-in-one Windows 64bit", path, progress);
                //create "archive" folder in the root folder
                MessageHelper.Log("Creating archive folder");
                System.IO.Directory.CreateDirectory(path + "\\archive");

            }

            MessageHelper.ShowMessage("Game download completed");
            //App.Current.Dispatcher.Invoke(() => MainWindow.Instance.HideProgressBar());

        }



        public static string BrowseFolder()
        {
            // Create an instance of the CustomBrowser window
            var folderBrowser = new DeFRaG_Helper.Windows.CustomBrowser();
            // Show the CustomBrowser dialog
            var result = folderBrowser.ShowDialog();
            if (result == true)
            {
                // If the user selected a folder, return the selected folder path
                return folderBrowser.SelectedFolderPath;
            }
            else
            {
                // If the user canceled the dialog, return null
                return null;
            }
        }




    }
}
