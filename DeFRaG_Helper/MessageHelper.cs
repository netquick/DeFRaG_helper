using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeFRaG_Helper
{
    internal class MessageHelper
    {
        //method to show message in MainWindow showmessage method
        public static void ShowMessage(string message)
        {
            App.Current.Dispatcher.Invoke(() => { MainWindow.Instance.ShowMessage(message); });
            SimpleLogger.Log(message);
        }
        public static void ShowMessageAsync(string message)
        {
            App.Current.Dispatcher.Invoke(() => { MainWindow.Instance.ShowMessage(message); });
            SimpleLogger.Log(message);
        }

        public static void LogMessage(string message)
        {
            SimpleLogger.Log(message);
        }

        public static async Task LogMessageAsync(string message)
        {
            await SimpleLogger.LogAsync(message);
        }

        public static void LogException(Exception ex)
        {
            SimpleLogger.Log($"Exception: {ex.Message}, {ex.StackTrace}");
        }

        public static async Task LogExceptionAsync(Exception ex)
        {
            await SimpleLogger.LogAsync($"Exception: {ex.Message}, {ex.StackTrace}");
        }

        public static void VerboseLog(string message)
        {
            SimpleLogger.Log($"Verbose: {message}");
        }
        public static async Task VerboseLogAsync(string message)
        {
            await SimpleLogger.LogAsync($"Verbose: {message}");
        }
    }
}
