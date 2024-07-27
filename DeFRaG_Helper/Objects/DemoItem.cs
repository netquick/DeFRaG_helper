using DeFRaG_Helper.Views;
using System.Text.RegularExpressions;
using System.IO;
namespace DeFRaG_Helper
{
    public class DemoItem
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public long Size { get; set; }
        public string Mapname { get; private set; }
        public string Type { get; private set; }
        public string Physics { get; private set; }
        public string Time { get; private set; }
        public long TotalMilliseconds { get; private set; }
        public string PlayerName { get; private set; }
        public string PlayerCountry { get; private set; }

        public string FormattedSize
        {
            get
            {
                if (Size >= 1_000_000) // More than or equal to 1 MB
                    return $"{Size / 1_000_000.0:#.##} MB";
                else if (Size >= 1_000) // More than or equal to 1 KB
                    return $"{Size / 1_000.0:#.##} KB";
                else
                    return $"{Size} Bytes";
            }
        }

        public void DecodeName()
        {
            // Modified pattern with an optional country group
            var pattern = @"^(?<Mapname>[^\[]+)\[(?<Type>[^\.]+)\.(?<Physics>[^\]]+)\](?<Time>\d{2}\.\d{2}\.\d{3})\((?<PlayerName>[^\.]+)(\.(?<PlayerCountry>[^\)]+))?\)";
            var match = Regex.Match(Name, pattern);
            if (match.Success)
            {
                Mapname = match.Groups["Mapname"].Value;
                Type = match.Groups["Type"].Value;
                Physics = match.Groups["Physics"].Value;
                Time = match.Groups["Time"].Value;
                TotalMilliseconds = ConvertTimeToMilliseconds(Time);
                PlayerName = match.Groups["PlayerName"].Value;
                // Use a conditional to check if the PlayerCountry group was matched
                PlayerCountry = match.Groups["PlayerCountry"].Success ? match.Groups["PlayerCountry"].Value : string.Empty;
            }
            else
            {
                Console.WriteLine("No match found.");
            }
        }



        private long ConvertTimeToMilliseconds(string timeString)
        {
            // Split the time string into minutes, seconds, and milliseconds.
            var parts = timeString.Split(new char[] { '.', '.' });
            if (parts.Length == 3)
            {
                // Parse each part of the time string.
                int minutes = int.Parse(parts[0]);
                int seconds = int.Parse(parts[1]);
                int milliseconds = int.Parse(parts[2]);

                // Calculate total milliseconds.
                long totalMilliseconds = (minutes * 60 * 1000) + (seconds * 1000) + milliseconds;
                return totalMilliseconds;
            }
            else
            {
                // Return 0 or throw an exception if the format is incorrect.
                // Depending on your application's needs, you might choose to handle this differently.
                return 0;
            }
        }
        private string ConvertMillisecondsToTime(long totalMilliseconds)
        {
            // Calculate minutes, seconds, and milliseconds from the total milliseconds
            int minutes = (int)(totalMilliseconds / (60 * 1000));
            int seconds = (int)((totalMilliseconds / 1000) % 60);
            int milliseconds = (int)(totalMilliseconds % 1000);

            // Format and return the time string
            return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
        }

        public string ImagePath
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








    }
}

