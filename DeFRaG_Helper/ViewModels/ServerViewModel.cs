using DeFRaG_Helper.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace DeFRaG_Helper
{
    public class ServerViewModel : INotifyPropertyChanged
    {


        public bool IsDataLoaded { get; private set; }
        private static bool isInitialized = false;

        public ObservableCollection<ServerNode> Servers { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private DispatcherTimer updateTimer;
        private static ServerViewModel instance;
        // Helper method to raise the PropertyChanged event
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ServerViewModel()
        {
            Servers = new ObservableCollection<ServerNode>();

            // Initialize the timer
            updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromSeconds(30);
            updateTimer.Tick += UpdateServerData;
            updateTimer.Start();

            // Initialize with some data (for testing)
            //var _ = InitializeServers();

        }
        public static async Task<ServerViewModel> GetInstanceAsync()
        {
            if (instance == null)
            {
                instance = new ServerViewModel();
                await instance.InitializeAsync();

            }
            return instance;
        }

        private async Task InitializeAsync()
        {
            if (!isInitialized)
            {
                await InitializeServers();



                isInitialized = true;
            }
        }
        private async Task InitializeServers()
        {
            Servers.Add(new ServerNode { IP = "83.243.73.220", Port = 27965 });
            Servers.Add(new ServerNode { IP = "83.243.73.220", Port = 27961 });
            Servers.Add(new ServerNode { IP = "81.25.18.133", Port = 55575 });
            Servers.Add(new ServerNode { IP = "81.25.18.133", Port = 55561 });
            Servers.Add(new ServerNode { IP = "83.243.73.220", Port = 27960 });
            Servers.Add(new ServerNode { IP = "81.25.18.133", Port = 55576 });
            Servers.Add(new ServerNode { IP = "140.82.4.154", Port = 27961 });
            Servers.Add(new ServerNode { IP = "81.25.18.133", Port = 55571 });
            Servers.Add(new ServerNode { IP = "81.25.18.133", Port = 55572 });
            Servers.Add(new ServerNode { IP = "81.25.18.133", Port = 55580 });
            Servers.Add(new ServerNode { IP = "81.25.18.133", Port = 55590 });
            Servers.Add(new ServerNode { IP = "155.138.136.62", Port = 27960 });
            Servers.Add(new ServerNode { IP = "155.138.136.62", Port = 27963 });
            Servers.Add(new ServerNode { IP = "140.82.4.154", Port = 27962 });
            Servers.Add(new ServerNode { IP = "140.82.4.154", Port = 27960 });
            Servers.Add(new ServerNode { IP = "83.243.73.220", Port = 27968 });
            Servers.Add(new ServerNode { IP = "83.243.73.220", Port = 27962 });
            Servers.Add(new ServerNode { IP = "83.243.73.220", Port = 27966 });
            Servers.Add(new ServerNode { IP = "212.64.17.145", Port = 27961 });
            Servers.Add(new ServerNode { IP = "212.64.17.145", Port = 27960 });
            Servers.Add(new ServerNode { IP = "155.138.136.62", Port = 27962 });
            Servers.Add(new ServerNode { IP = "155.138.136.62", Port = 27961 });

            UpdateServerData(this, EventArgs.Empty);
        }

        // In ServerViewModel.cs
        private async void UpdateServerData(object sender, EventArgs e)
        {
            var tasks = Servers.Select(async serverNode =>
            {
                var serverQuery = new Quake3ServerQuery(serverNode.IP, serverNode.Port);
                try
                {
                    var (Success, Response) = await serverQuery.QueryServerAsync();
                    if (Success)
                    {
                        // Ensure UI thread is used for property updates if needed
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            ParseServerResponseAndUpdate(serverNode, Response);
                        });
                    }
                    else
                    {
                        // Log or display the error message contained in Response
                        Debug.WriteLine(Response); // Replace with your preferred logging or error display method
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }).ToList();

            await Task.WhenAll(tasks);

            // Refresh the SortedServersView on the UI thread after all updates
            App.Current.Dispatcher.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Servers)));
            });
        }




        public static void ParseServerResponseAndUpdate(ServerNode serverNode, string response)
        {
            // Clear the Players list at the beginning of the update
            serverNode.Players.Clear();

            string startMarker = "????statusResponse\n";
            int startIndex = response.IndexOf(startMarker);
            if (startIndex == -1)
            {
                return; // Marker not found, cannot parse
            }

            startIndex += startMarker.Length;
            string serverData = response.Substring(startIndex);
            string[] lines = serverData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 1)
            {
                return; // Not enough data to parse
            }

            string[] serverInfo = lines[0].Split('\\');
            for (int i = 1; i < serverInfo.Length; i += 2)
            {
                string key = serverInfo[i];
                if (i + 1 < serverInfo.Length)
                {
                    string value = serverInfo[i + 1];
                    switch (key)
                    {
                        case "mapname":
                            serverNode.Map = value;
                            break;
                        case "sv_hostname":
                            serverNode.Name = value;
                            break;
                        case "sv_maxclients":
                            serverNode.MaxPlayers = int.TryParse(value, out int maxPlayers) ? maxPlayers : 0;
                            break;
                        case "df_promode":
                            serverNode.Physics = value == "0" ? "VQ3" : value == "1" ? "CPM" : "Unknown";
                            break;
                            // Add more cases as needed for other properties
                    }
                }
            }
            // Process player lines
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                    continue; // Skip empty lines

                // Extract and clean player name, then add to Players list
                int firstQuoteIndex = line.IndexOf('\"');
                int lastQuoteIndex = line.LastIndexOf('\"');
                if (firstQuoteIndex >= 0 && lastQuoteIndex > firstQuoteIndex)
                {
                    string playerName = line.Substring(firstQuoteIndex + 1, lastQuoteIndex - firstQuoteIndex - 1);
                    //string cleanPlayerName = Regex.Replace(playerName, @"\^\d", "");
                    serverNode.Players.Add(playerName);
                }
            }
            serverNode.CurrentPlayers = serverNode.Players.Count;

            // Subscribe to PropertyChanged event if needed
            serverNode.PropertyChanged -= ServerNode_PropertyChanged;
            serverNode.PropertyChanged += ServerNode_PropertyChanged;
        }

        private static void ServerNode_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ServerNode.Map))
            {
                var serverNode = sender as ServerNode;
                if (serverNode != null)
                {
                    // Here you can log or perform actions knowing the Map (and thus potentially the ImagePath) has changed
                    // For example, you could explicitly call a method to handle the image update
                    UpdateServerNodeImage(serverNode);
                }
            }
        }

        private static void UpdateServerNodeImage(ServerNode serverNode)
        {
            // This method can be used to perform actions based on the updated Map property
            // For instance, you could log the new ImagePath or trigger UI updates
            Debug.WriteLine($"New Image Path for server {serverNode.Name}: {serverNode.ImagePath}");
        }
    }
}
