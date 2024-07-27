using DeFRaG_Helper.Objects;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
                // Check the UseHighQualityImages setting from AppConfig
                if (AppConfig.UseHighQualityImages.HasValue && AppConfig.UseHighQualityImages.Value)
                {
                    // If UseHighQualityImages is true, use the high-quality image path
                    return GetImagePath(Mapname);
                }
                else
                {
                    // Otherwise, use the small image path
                    // Assuming SmallImagePath is already implemented and returns a path
                    return SmallImagePath;
                }
            }
        }
        public string SmallImagePath
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
        public string ScreenshotPath
        {
            get
            {
                return GetImagePath(Mapname);
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
        private ObservableCollection<MapIcon>? weaponIcons;
        public ObservableCollection<MapIcon> WeaponIcons
        {
            get
            {
                if (weaponIcons == null || !weaponIcons.Any())
                {
                    weaponIcons = GenerateWeaponIcons();
                }
                return weaponIcons;
            }
            set
            {
                if (weaponIcons != value)
                {
                    weaponIcons = value;
                    OnPropertyChanged(nameof(weaponIcons));
                }
            }
        }
        private ObservableCollection<MapIcon>? itemIcons;
        public ObservableCollection<MapIcon> ItemIcons
        {
            get
            {
                if (itemIcons == null || !itemIcons.Any())
                {
                    itemIcons = GenerateItemIcons();
                }
                return itemIcons;
            }
            set
            {
                if (itemIcons != value)
                {
                    itemIcons = value;
                    OnPropertyChanged(nameof(itemIcons));
                }
            }
        }
        private ObservableCollection<MapIcon>? functionIcons;
        public ObservableCollection<MapIcon> FunctionIcons
        {
            get
            {
                if (functionIcons == null || !functionIcons.Any())
                {
                    functionIcons = GenerateFunctionIcons();
                }
                return functionIcons;
            }
            set
            {
                if (functionIcons != value)
                {
                    functionIcons = value;
                    OnPropertyChanged(nameof(functionIcons));
                }
            }
        }
        public ObservableCollection<MapIcon> GenerateWeaponIcons()
        {
            ObservableCollection<MapIcon> newWeaponIcons = new ObservableCollection<MapIcon>();

            // Example mapping for demonstration purposes
            Dictionary<string, (string path, string color)> iconMapping = new Dictionary<string, (string, string)>
            {
                {"Rocket Launcher", ("Icons/Weapons/iconw_rocket.svg", "Red")},
                {"Plasmagun", ("Icons/Weapons/iconw_plasma.svg", "Blue")},
                {"Railgun", ("Icons/Weapons/iconw_railgun.svg", "Green") },
                {"Shotgun", ("Icons/Weapons/iconw_shotgun.svg", "Yellow") },
                {"Lightning Gun", ("Icons/Weapons/iconw_lightning.svg", "Orange") },
                {"Big fucking gun", ("Icons/Weapons/iconw_bfg.svg", "Black") },
                {"Gauntlet", ("Icons/Weapons/iconw_gauntlet.svg", "Cyan") },
                {"Grappling hook", ("Icons/Weapons/iconw_grapple.svg", "Green") },
                {"Grenade Launcher", ("Icons/Weapons/iconw_grenade.svg", "Green") },
                {"Machinegun", ("Icons/Weapons/iconw_machinegun", "yellow") },
                {"Proximity Mine Launcher (Team Arena)", ("Icons/Weapons/proxmine.svg", "Red") },
                {"Chaingun (Team Arena)", ("Icons/Weapons/chaingun.svg", "Black") },
                {"Nailgun (Team Arena)", ("Icons/Weapons/nailgun.svg", "Green") }

        };

            foreach (var weapon in Weapons)
            {
                if (iconMapping.TryGetValue(weapon, out var iconInfo))
                {
                    newWeaponIcons.Add(new MapIcon { SvgPath = iconInfo.path, Color = iconInfo.color });
                }
            }
            return newWeaponIcons;
        }
        public ObservableCollection<MapIcon> GenerateItemIcons()
        {
            ObservableCollection<MapIcon> newItemIcons = new ObservableCollection<MapIcon>();

            // Example mapping for demonstration purposes
            Dictionary<string, (string path, string color)> iconMapping = new Dictionary<string, (string, string)>
            {
                {"Body Armor (Red Armor)", ("Icons/Items/iconr_red.svg", "Red") },
                {"Combat Armor (Yellow Armor)", ("Icons/Items/iconr_yellow.svg", "Yellow") },
                {"Battle Suit", ("Icons/Items/envirosuit.svg", "Orange") },
                {"Shard Armor", ("Icons/Items/iconr_shard.svg", "Green") },
                {"Flight", ("Icons/Items/flight.svg", "Purple") },
                {"Haste", ("Icons/Items/haste.svg", "Yellow") },
                {"Health", ("Icons/Items/iconh_red.svg", "Red") },
                {"Large health", ("Icons/Items/iconh_yellow.svg", "Yellow") },
                {"Mega health", ("Icons/Items/iconh_mega.svg", "Blue") },
                {"Small health", ("Icons/Items/iconh_green.svg", "Green") },
                {"Invisibility", ("Icons/Items/invis.svg", "Purple") },
                {"Quad-damage", ("Icons/Items/quad.svg", "Cyan") },
                {"Regeneration", ("Icons/Items/regen.svg", "Green") },
                {"Personal Teleporter", ("Icons/Items/teleporter.svg", "Blue") },
                {"Medikit", ("Icons/Items/medkit.svg", "Red") },
                {"Ammo Regen (Team Arena)", ("Icons/Items/ammo_regen.svg", "Yellow") },
                {"Scout (Team Arena)", ("Icons/Items/scout.svg", "Green") },
                {"Doubler (Team Arena)", ("Icons/Items/doubler.svg", "Red") },
                {"Guard (Team Arena)", ("Icons/Items/guard.svg", "Blue") },
                {"Kamikaze (Team Arena)", ("Icons/Items/kamikaze.svg", "Red") },
                {"Invulnerability (Team Arena)", ("Icons/Items/invulnerability.svg", "Blue") },
                {"Green Armor (CPMA)", ("Icons/Items/iconr_green.svg", "Green") }
               
               
                

        };

            foreach (var item in Items)
            {
                if (iconMapping.TryGetValue(item, out var iconInfo))
                {
                    newItemIcons.Add(new MapIcon { SvgPath = iconInfo.path, Color = iconInfo.color });
                }
            }
            return newItemIcons;
        }
        public ObservableCollection<MapIcon> GenerateFunctionIcons()
        {
            ObservableCollection<MapIcon> newFunctionIcons = new ObservableCollection<MapIcon>();

            // Example mapping for demonstration purposes
            Dictionary<string, (string path, string color)> iconMapping = new Dictionary<string, (string, string)>
            {
                {"Door/Gate", ("Icons/Functions/door.svg", "Red") },
                {"Button", ("Icons/Functions/button.svg", "Red") },
                {"Teleporter/Portal", ("Icons/Functions/tele.svg", "Blue") },
                {"Jumppad/Launchramp", ("Icons/Functions/push.svg", "Yellow") },
                {"Moving object/platform", ("Icons/Functions/moving2.svg", "Gray") },
                {"Shooter Grenade", ("Icons/Functions/shootergl.svg", "Green") },
                {"Shooter Plasma", ("Icons/Functions/shooterpg.svg", "Purple") },
                {"Shooter Rocket", ("Icons/Functions/shooterrl.svg", "Red") },
                {"Slick", ("Icons/Functions/slick.svg", "Yellow") },
                {"Water", ("Icons/Functions/water.svg", "Blue") },
                {"Fog", ("Icons/Functions/fog.svg", "Gray") },
                {"Slime", ("Icons/Functions/slime.svg", "Green") },
                {"Lava", ("Icons/Functions/lava.svg", "Red") },
                {"breakable", ("Icons/Functions/break.svg", "Gray") },
                {"Ambient sounds", ("Icons/Functions/Speaker_Icon.svg", "Gray") },
                {"Timer", ("Icons/Functions/timer2.svg", "Gray") }


        };

            foreach (var function in Functions)
            {
                if (iconMapping.TryGetValue(function, out var iconInfo))
                {
                    newFunctionIcons.Add(new MapIcon { SvgPath = iconInfo.path, Color = iconInfo.color });
                }
            }
            return newFunctionIcons;
        }

    }


}

