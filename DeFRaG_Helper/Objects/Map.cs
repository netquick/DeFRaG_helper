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
                if (!string.IsNullOrEmpty(Mapname))
                {
                    // Replace .bsp extension with .jpg
                    string imageName = Mapname.EndsWith(".bsp", StringComparison.OrdinalIgnoreCase)
                        ? Mapname.Substring(0, Mapname.Length - 4) + ".jpg"
                        : Mapname + ".jpg";

                    // Get the AppData directory and append your application's folder
                    string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string basePath = System.IO.Path.Combine(appDataPath, "DeFRaG_Helper"); // Adjusted to use the AppData directory

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
        public string? Mapname
        {
            get => mapName;
            set
            {
                if (mapName != value)
                {
                    mapName = value;
                    OnPropertyChanged(nameof(Mapname));
                }
            }
        }

        private string filename;
        public string? Filename
        {
            get => filename;
            set
            {
                if (filename != value)
                {
                    filename = value;
                    OnPropertyChanged(nameof(Filename));
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

        private string releasedate;
        public string? Releasedate
        {
            get => releasedate;
            set
            {
                if (releasedate != value)
                {
                    releasedate = value;
                    OnPropertyChanged(nameof(Releasedate));
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

        private string linkDetailpage;
        public string? LinkDetailpage
        {
            get => linkDetailpage;
            set
            {
                if (linkDetailpage != value)
                {
                    linkDetailpage = value;
                    OnPropertyChanged(nameof(LinkDetailpage));
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

        private string? linksOnlineRecordsQ3DFVQ3;
        public string? LinksOnlineRecordsQ3DFVQ3
        {
            get => linksOnlineRecordsQ3DFVQ3;
            set
            {
                if (linksOnlineRecordsQ3DFVQ3 != value)
                {
                    linksOnlineRecordsQ3DFVQ3 = value;
                    OnPropertyChanged(nameof(LinksOnlineRecordsQ3DFVQ3));
                }
            }
        }
        private string? linksOnlineRecordsQ3DFCPM;
        public string? LinksOnlineRecordsQ3DFCPM
        {
            get => linksOnlineRecordsQ3DFCPM;
            set
            {
                if (linksOnlineRecordsQ3DFCPM != value)
                {
                    linksOnlineRecordsQ3DFCPM = value;
                    OnPropertyChanged(nameof(LinksOnlineRecordsQ3DFCPM));
                }
            }
        }
        private string? linksOnlineRecordsRacingVQ3;
        public string? LinksOnlineRecordsRacingVQ3
        {
            get => linksOnlineRecordsRacingVQ3;
            set
            {
                if (linksOnlineRecordsRacingVQ3 != value)
                {
                    linksOnlineRecordsRacingVQ3 = value;
                    OnPropertyChanged(nameof(LinksOnlineRecordsRacingVQ3));
                }
            }
        }
        private string? linksOnlineRecordsRacingCPM;
        public string? LinksOnlineRecordsRacingCPM
        {
            get => linksOnlineRecordsRacingCPM;
            set
            {
                if (linksOnlineRecordsRacingCPM != value)
                {
                    linksOnlineRecordsRacingCPM = value;
                    OnPropertyChanged(nameof(LinksOnlineRecordsRacingCPM));
                }
            }
        }
        private string? linkDemosVQ3;
        public string? LinkDemosVQ3
        {
            get => linkDemosVQ3;
            set
            {
                if (linkDemosVQ3 != value)
                {
                    linkDemosVQ3 = value;
                    OnPropertyChanged(nameof(LinkDemosVQ3));
                }
            }
        }
        private string? linkDemosCPM;
        public string? LinkDemosCPM
        {
            get => linkDemosCPM;
            set
            {
                if (linkDemosCPM != value)
                {
                    linkDemosCPM = value;
                    OnPropertyChanged(nameof(LinkDemosCPM));
                }
            }
        }

        private string? dependencies;
        public string? Dependencies
        {
            get => dependencies;
            set
            {
                if (dependencies != value)
                {
                    dependencies = value;
                    OnPropertyChanged(nameof(Dependencies));
                }
            }
        }

        private List<string> weapons = new List<string>();
        public List<string> Weapons
        {
            get => weapons;
            set
            {
                if (weapons != value)
                {
                    weapons = value;
                    OnPropertyChanged(nameof(Weapons));
                }
            }
        }

        private List<string> items = new List<string>();
        public List<string> Items
        {
            get => items;
            set
            {
                if (items != value)
                {
                    items = value;
                    OnPropertyChanged(nameof(Items));
                }
            }
        }

        private List<string> functions = new List<string>();
        public List<string> Functions
        {
            get => functions;
            set
            {
                if (functions != value)
                {
                    functions = value;
                    OnPropertyChanged(nameof(Functions));
                }
            }
        }   
    }
}
