using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeFRaG_Helper
{
    public static class SimpleLogger
    {
        private static readonly object lockObj = new object();
        private static readonly string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string logDirectory = Path.Combine(appDataFolder, "DeFRaG_Helper");
        private static readonly string logFilePath = Path.Combine(logDirectory, "application.log");



        public static void Log(string message)
        {
            try
            {
                // Ensure the log directory exists
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                lock (lockObj)
                {
                    using (StreamWriter sw = new StreamWriter(logFilePath, true))
                    {
                        sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any logging exceptions or ignore them
                Console.WriteLine($"Logging failed: {ex.Message}");
            }
        }



        // Async version of Log method
        public static async Task LogAsync(string message)
        {
            try
            {
                lock (lockObj)
                {
                    using (StreamWriter sw = new StreamWriter(logFilePath, true))
                    {
                        Task writeTask = sw.WriteLineAsync($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                        writeTask.Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle or ignore exceptions
                Console.WriteLine($"Logging failed: {ex.Message}");
            }
        }
    }
}
