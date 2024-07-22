using DeFRaG_Helper.Windows;
using static DeFRaG_Helper.Windows.CustomMessageBox;
using System.IO;
using System.Xml.Linq;
using System.Text;
using System.Xml;
namespace DeFRaG_Helper.Helpers
{
    internal class CheckEditorInstallation
    {
        private static CheckEditorInstallation instance;
        public static CheckEditorInstallation Instance
        {
            get
            {
                if (instance == null)
                    instance = new CheckEditorInstallation();
                return instance;
            }
        }

        private string _editorPath;

        public async Task CheckEditor()
        {
            //check if radiant.exe exists in the path EditorPath from AppConfig. If the path is empty, set the path to the default radiant path
            if (string.IsNullOrEmpty(_editorPath))
            {
                _editorPath = AppConfig.GameDirectoryPath + "\\Netradiant_Custom";
            }




            if (!System.IO.File.Exists(_editorPath + "\\radiant.exe"))
            {
                //Install Radiant Editor
                InstallEditor();

            } else
            {
                return;
            }
        }

        private async Task InstallEditor()
        {
           
            // Show customMessageBox to ask user if they want to install radiant editor
            var result = await CustomMessageBox.Show("Select existing Radiant.exe or chose install to download.", "Radiant Editor not found", "Install", "Select");
            if (result)
            {
                // Open a Custom Folder Dialog to select the radiant.exe
                // Assuming this code is inside a method in CheckEditorInstallation.cs
                var folderBrowserDialog = new CustomFolderBrowser();
                var dialogResult = folderBrowserDialog.ShowDialog();

                if (dialogResult == true)
                {
                    // User made a selection and clicked "Select"
                    string selectedFolderPath = folderBrowserDialog.SelectedFolderPath;
                    // Now you can use selectedFolderPath as the installation directory
                    // For example, setting it as the editor path
                    SetEditorPath(selectedFolderPath + "\\radiant.exe");
                    //save config
                    await AppConfig.SaveConfigurationAsync();
                }
                else
                {
                    // The user canceled the folder selection
                }
            }

            else
            {
                // User chose to install the editor
                //download the latest release from https://github.com/Garux/netradiant-custom/releases/tag/latest using DownloadLatestReleaseAssetAsync from GitHubReleaseChecker
                var releaseChecker = new GitHubReleaseChecker();


                string destinationPath = _editorPath; // Ensure this is a directory path
                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                }
                string downloadedFile = await releaseChecker.DownloadLatestReleaseAssetAsync("Garux", "netradiant-custom", destinationPath);
                if (downloadedFile != null)
                {
                    await Downloader.UnpackFile(downloadedFile, destinationPath, null);
                    MessageHelper.ShowMessage("Radiant Editor installed successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to download the latest release asset.");
                }
                //when installed trough app, we will set the config files of Radiant to point to defrag/Quake3. This will be done in the next release
                //for this we need to write the file \Netradiant_Custom\settings\1.6.0\global.pref
                WriteGlobalPrefFile();

                //and we need write the q3 specific file to \Netradiant_Custom\settings\1.6.0\Q3.game\local.pref
                WriteLocalPrefFile();
            }
        }
        private void WriteGlobalPrefFile()
        {
            // Define the path to the global.pref file
            string globalPrefPath = Path.Combine(_editorPath, "settings", "1.6.0", "global.pref");

            // Create the directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(globalPrefPath));

            // Define the XML content as a string
            string xmlContent = @"<?xml version=""1.0""?>
                <qpref version=""1.0"">
                <epair name=""gamePrompt"">false</epair>
                <epair name=""gamefile"">Q3.game</epair>
                <epair name=""log console"">true</epair>
                </qpref>";

            // Write the XML string to the global.pref file
            File.WriteAllText(globalPrefPath, xmlContent);

            // Optionally, log or notify that the file has been written
            Console.WriteLine($"global.pref file has been written to {globalPrefPath}");
        }


        private void WriteLocalPrefFile()
        {
            // Define the path to the local.pref file
            string localPrefPath = Path.Combine(_editorPath, "settings", "1.6.0", "Q3.game", "local.pref");

            // Create the directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(localPrefPath));

            // Create the XML content with utf-16 encoding specified
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-16", null),
                new XElement("qpref", new XAttribute("version", "1.0"),
                    new XElement("epair", new XAttribute("name", "EnginePath"), "E:/Q3DeFRaG/")
                )
            );

            // Prepare XmlWriterSettings to enable indentation
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: true) // UTF-16 Little Endian with BOM
            };

            // Use XmlWriter to write the XDocument to the file with specified settings
            using (XmlWriter writer = XmlWriter.Create(localPrefPath, settings))
            {
                doc.Save(writer);
            }

            // Optionally, log or notify that the file has been written
            Console.WriteLine($"local.pref file has been written to {localPrefPath}");
        }



        private CheckEditorInstallation()
        {
            _editorPath = AppConfig.GameDirectoryPath + "\\Netradiant_Custom";


        
        }

        //method to manually point to the radiant editor
        public void SetEditorPath(string path)
        {
            _editorPath = path;
        }

       

    }
}
