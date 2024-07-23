using System.Diagnostics;
using System.IO;

namespace DeFRaG_Helper.Helpers
{
    internal class EditMap
    {
        private static EditMap _instance;
        public static EditMap Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EditMap();
                }
                return _instance;
            }
        }

        // Make the constructor private so it can't be instantiated outside of this class
        private EditMap() { }
        //first make sure the actual map is installed
        public async Task ConvertMap(Map map)
        {
            //check if the converted map already exists 
         
            if (System.IO.File.Exists(AppConfig.GameDirectoryPath + "\\defrag\\maps\\" + System.IO.Path.GetFileNameWithoutExtension(map.Mapname) + ".map"))
            {
                //open the map in the editor
                System.Diagnostics.Process.Start(AppConfig.GameDirectoryPath + "\\Netradiant_Custom\\radiant.exe", $"-map {AppConfig.GameDirectoryPath + "\\defrag\\maps\\" + System.IO.Path.GetFileNameWithoutExtension(map.Mapname) + ".map"}");
                return;
            }


            //check if the map is installed
            if (!System.IO.File.Exists(AppConfig.GameDirectoryPath + "\\defrag\\" + map.Filename))
            {
                //Install the map
                await MapInstaller.InstallMap(map);
            }
            else
            {
                
            }
            MessageHelper.ShowMessage($"Map {map.Mapname} is installed and will be opened in the editor.");

            //create a temporary folder to extract the map to
            string tempFolder = System.IO.Path.GetTempPath() + "DeFRaG_Helper";
            if (!System.IO.Directory.Exists(tempFolder))
            {
                System.IO.Directory.CreateDirectory(tempFolder);
            }
            //get a copy of the map file
            System.IO.File.Copy(AppConfig.GameDirectoryPath + "\\defrag\\" + map.Filename, tempFolder + "\\" + map.Filename, true);
            //rename the .pk3 file to .zip
            var pureFileName = System.IO.Path.GetFileNameWithoutExtension(map.Filename);
            System.IO.File.Move(tempFolder + "\\" + map.Filename, tempFolder + "\\" + pureFileName + ".zip");
            //extract the map file
            await Downloader.UnpackFile(tempFolder + "\\" + pureFileName + ".zip", tempFolder, null);




            var outFile = AppConfig.GameDirectoryPath + "\\defrag\\maps\\" + map.Mapname;
            //if directory does not exist, create it
            if (!System.IO.Directory.Exists(AppConfig.GameDirectoryPath + "\\defrag\\maps"))
            {
                System.IO.Directory.CreateDirectory(AppConfig.GameDirectoryPath + "\\defrag\\maps");
            }
            MessageHelper.Log(AppConfig.GameDirectoryPath + "\\Netradiant_Custom\\q3map2.exe");
            MessageHelper.Log($"-convert -format map {tempFolder + "\\maps\\" + map.Mapname} {outFile}");
            var cmdOptions = $"-convert -format map {tempFolder + "\\maps\\" + map.Mapname}";
            // Start the external process
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = AppConfig.GameDirectoryPath + "\\Netradiant_Custom\\q3map2.exe",
                    Arguments = cmdOptions,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            MessageHelper.ShowMessage("Converting map, please wait...");
            process.Start();

            // Await the process to exit
            await Task.Run(() => process.WaitForExit());


            // After the map conversion process
            string tempFolderBase = tempFolder; // Assuming tempFolder is the root of your temporary directory structure
            string destinationBase = Path.Combine(AppConfig.GameDirectoryPath, "baseq3");

            await CopyDirectoryAsync(tempFolderBase, destinationBase);







            //delete the temporary folder
            System.IO.Directory.Delete(tempFolder, true);


            var mapFile = AppConfig.GameDirectoryPath + "\\baseq3\\maps\\" + System.IO.Path.GetFileNameWithoutExtension(map.Mapname) + ".map";
            var convertedMapFile = AppConfig.GameDirectoryPath + "\\baseq3\\maps\\" + System.IO.Path.GetFileNameWithoutExtension(map.Mapname) + "_converted.map";

            //rename the converted file from convertedMapFile to mapFile
            System.IO.File.Move(convertedMapFile, mapFile);
            MessageHelper.ShowMessage($"Map {map.Mapname} is converted and will be opened in the editor.");
            //open the map in the editor
            System.Diagnostics.Process.Start(AppConfig.GameDirectoryPath + "\\Netradiant_Custom\\radiant.exe", $"-map {mapFile}");


        }

        private async Task CopyDirectoryAsync(string sourceDir, string destinationDir)
        {
            // Ensure the destination directory exists
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy them to the destination directory
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destFile, true); // true allows overwriting of existing files
            }

            // Recursively copy subdirectories
            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                string destDirectory = Path.Combine(destinationDir, Path.GetFileName(directory));
                await CopyDirectoryAsync(directory, destDirectory);
            }
        }


    }
}
