using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DeFRaG_Helper
{
    public class ServerNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        private string ip;
        public string IP
        {
            get => ip;
            set
            {
                ip = value;
                OnPropertyChanged(nameof(IP));
            }
        }

        private int port;
        public int Port
        {
            get => port;
            set
            {
                port = value;
                OnPropertyChanged(nameof(Port));
            }
        }

        private int currentPlayers;
        public int CurrentPlayers
        {
            get => currentPlayers;
            set
            {
                currentPlayers = value;
                OnPropertyChanged(nameof(CurrentPlayers));
            }
        }

        private int maxPlayers;
        public int MaxPlayers
        {
            get => maxPlayers;
            set
            {
                maxPlayers = value;
                OnPropertyChanged(nameof(MaxPlayers));
            }
        }

        private string map;
        public string Map
        {
            get => map;
            set
            {
                map = value;
                OnPropertyChanged(nameof(Map));
            }
        }

        private string style;
        public string Style
        {
            get => style;
            set
            {
                style = value;
                OnPropertyChanged(nameof(Style));
            }
        }

        private string physics;
        public string Physics
        {
            get => physics;
            set
            {
                physics = value;
                OnPropertyChanged(nameof(Physics));
            }
        }

        private string record;
        public string Record
        {
            get => record;
            set
            {
                record = value;
                OnPropertyChanged(nameof(Record));
            }
        }

        private DateTime lastUpdate;
        public DateTime LastUpdate
        {
            get => lastUpdate;
            set
            {
                lastUpdate = value;
                OnPropertyChanged(nameof(LastUpdate));
            }
        }

        private List<string> players = new List<string>();
        public List<string> Players
        {
            get => players;
            set
            {
                players = value;
                OnPropertyChanged(nameof(Players));
            }
        }

        // Add other server properties as needed
    }
}
