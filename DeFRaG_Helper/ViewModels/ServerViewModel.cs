using DeFRaG_Helper.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
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

        public ServerViewModel()
        {
            Servers = new ObservableCollection<ServerNode>();

            // Initialize the timer
            updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromSeconds(30);
            updateTimer.Tick += UpdateServerData;
            updateTimer.Start();

            // Initialize with some data (for testing)
            var _ = InitializeServers();

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

            // Notify the UI that the server properties have been updated
            App.Current.Dispatcher.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Servers)));
            });
        }



        public static void ParseServerResponseAndUpdate(ServerNode serverNode, string response)
        {
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
                            serverNode.Name = value.Replace("^7", "").Replace("^4", "").Replace("^1", "");
                            break;
                        case "sv_maxclients":
                            serverNode.MaxPlayers = int.TryParse(value, out int maxPlayers) ? maxPlayers : 0;
                            break;
                        //case "df_promode":
                        //    serverNode.Physics = int.TryParse(value, out int mode) ? mode : 0; // Assuming Mode is an integer
                        //    break;
                            // Add more cases as needed for other properties
                    }
                }
            }

            // Assuming player info starts after the server settings
            //serverNode.Players = lines.Length - 1; // Subtract 1 for the server settings line
        }

    }
}
