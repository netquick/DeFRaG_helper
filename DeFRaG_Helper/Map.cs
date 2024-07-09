using System.ComponentModel;

namespace DeFRaG_Helper
{
    public class Map : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Helper method to raise the PropertyChanged event
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        public string ImagePath
        {
            get
            {
                if (!string.IsNullOrEmpty(MapName))
                {
                    // Replace .bsp extension with .jpg
                    string imageName = MapName.EndsWith(".bsp", StringComparison.OrdinalIgnoreCase)
                        ? MapName.Substring(0, MapName.Length - 4) + ".jpg"
                        : MapName + ".jpg";

                    // Adjusted to the correct folder name "PreviewImages"
                    string basePath = AppDomain.CurrentDomain.BaseDirectory;
                    string imagePath = System.IO.Path.Combine(basePath, $"PreviewImages/{imageName}");

                    // Check if the image file exists
                    if (System.IO.File.Exists(imagePath))
                    {
                        return $"file:///{imagePath}";
                    }
                    else
                    {
                        // Return the placeholder image path if the specific map image does not exist
                        string placeholderPath = System.IO.Path.Combine(basePath, "PreviewImages/placeholder.png");
                        return $"file:///{placeholderPath}";
                    }
                }
                return null;
            }
        }



        private int id;
        public int Id
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        private string mapName;
        public string? MapName
        {
            get => mapName;
            set
            {
                if (mapName != value)
                {
                    mapName = value;
                    OnPropertyChanged(nameof(MapName));
                }
            }
        }

        private string fileName;
        public string? FileName
        {
            get => fileName;
            set
            {
                if (fileName != value)
                {
                    fileName = value;
                    OnPropertyChanged(nameof(FileName));
                }
            }
        }

        private string author;
        public string? Author
        {
            get => author;
            set
            {
                if (author != value)
                {
                    author = value;
                    OnPropertyChanged(nameof(Author));
                }
            }
        }

        private string gameType;
        public string? GameType
        {
            get => gameType;
            set
            {
                if (gameType != value)
                {
                    gameType = value;
                    OnPropertyChanged(nameof(GameType));
                }
            }
        }

        private string releaseDate;
        public string? ReleaseDate
        {
            get => releaseDate;
            set
            {
                if (releaseDate != value)
                {
                    releaseDate = value;
                    OnPropertyChanged(nameof(ReleaseDate));
                }
            }
        }

        private string style;
        public string? Style
        {
            get => style;
            set
            {
                if (style != value)
                {
                    style = value;
                    OnPropertyChanged(nameof(Style));
                }
            }
        }

        private long size;
        public long Size
        {
            get => size;
            set
            {
                if (size != value)
                {
                    size = value;
                    OnPropertyChanged(nameof(Size));
                }
            }
        }

        private int physics;
        public int Physics
        {
            get => physics;
            set
            {
                if (physics != value)
                {
                    physics = value;
                    OnPropertyChanged(nameof(Physics));
                }
            }
        }

        private int hits;
        public int Hits
        {
            get => hits;
            set
            {
                if (hits != value)
                {
                    hits = value;
                    OnPropertyChanged(nameof(Hits));
                }
            }
        }

        private string downloadlink;
        public string? Downloadlink
        {
            get => downloadlink;
            set
            {
                if (downloadlink != value)
                {
                    downloadlink = value;
                    OnPropertyChanged(nameof(Downloadlink));
                }
            }
        }

        private int? isDownloaded;
        public int? IsDownloaded
        {
            get => isDownloaded;
            set
            {
                if (isDownloaded != value)
                {
                    isDownloaded = value;
                    OnPropertyChanged(nameof(IsDownloaded));
                }
            }
        }

        private int? isInstalled;
        public int? IsInstalled
        {
            get => isInstalled;
            set
            {
                if (isInstalled != value)
                {
                    isInstalled = value;
                    OnPropertyChanged(nameof(IsInstalled));
                }
            }
        }

        private int? isFavorite;
        public int? IsFavorite
        {
            get => isFavorite;
            set
            {
                if (isFavorite != value)
                {
                    isFavorite = value;
                    OnPropertyChanged(nameof(IsFavorite));
                }
            }
        }
    }
}
