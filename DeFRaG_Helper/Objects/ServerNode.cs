using DeFRaG_Helper.Objects;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
                if (map != value)
                {
                    map = value;
                    OnPropertyChanged(nameof(Map));
                    OnPropertyChanged(nameof(ImagePath)); // Notify that ImagePath has changed as well
                }
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
        public string ImagePath
        {
            get
            {
                return GetImagePath(map);
            }
        }
        public string GetImagePath(string mapName)
        {
            if (!string.IsNullOrEmpty(mapName))
            {
                // Replace .bsp extension with .jpg
                string imageName = mapName.EndsWith(".bsp", StringComparison.OrdinalIgnoreCase)
                    ? mapName.Substring(0, mapName.Length - 4) + ".jpg"
                    : mapName + ".jpg";

                // Get the AppData directory and append your application's folder
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string basePath = Path.Combine(appDataPath, "DeFRaG_Helper");

                // List of folders to check for the image
                string[] folders = { "Screenshots", "Levelshots", "Topviews" };

                foreach (var folder in folders)
                {
                    string imagePath = Path.Combine(basePath, $"PreviewImages/{folder}/{imageName}");
                    if (File.Exists(imagePath))
                    {
                        return $"file:///{imagePath}";
                    }
                }

                // If the image is not found in any folder, return the placeholder image path
                string placeholderPath = Path.Combine(basePath, "PreviewImages/placeholder.png");
                return $"file:///{placeholderPath}";
            }
            return null;
        }

        private ObservableCollection<MapIcon> weaponIcons;
        public ObservableCollection<MapIcon> WeaponIcons
        {
            get => weaponIcons;
            set
            {
                weaponIcons = value;
                OnPropertyChanged(nameof(WeaponIcons));
            }
        }

        private ObservableCollection<MapIcon> itemIcons;
        public ObservableCollection<MapIcon> ItemIcons
        {
            get => itemIcons;
            set
            {
                itemIcons = value;
                OnPropertyChanged(nameof(ItemIcons));
            }
        }

        private ObservableCollection<MapIcon> functionIcons;
        public ObservableCollection<MapIcon> FunctionIcons
        {
            get => functionIcons;
            set
            {
                functionIcons = value;
                OnPropertyChanged(nameof(FunctionIcons));
            }
        }

        // Method to load icons from the map
        public void LoadMapIcons(Map map)
        {
            WeaponIcons = map.GenerateWeaponIcons();
            ItemIcons = map.GenerateItemIcons();
            FunctionIcons = map.GenerateFunctionIcons();
        }

    }
}
