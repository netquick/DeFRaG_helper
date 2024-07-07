using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeFRaG_Helper
{
    public static class AppConfig
    {
        public static string GameDirectoryPath { get; set; }

        public static void LoadConfiguration()
        {
            // Load the game directory path from the app.config file if it exists
            GameDirectoryPath = ConfigurationManager.AppSettings["GameDirectoryPath"];
        }

        public static void SaveConfiguration()
        {
            // Get the current configuration file
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Remove the existing setting, if any
            config.AppSettings.Settings.Remove("GameDirectoryPath");

            // Add the new setting
            config.AppSettings.Settings.Add("GameDirectoryPath", GameDirectoryPath);

            // Save the configuration file
            config.Save(ConfigurationSaveMode.Modified);

            // Refresh the appSettings section to reflect the update
            ConfigurationManager.RefreshSection("appSettings");
        }
    }


}
